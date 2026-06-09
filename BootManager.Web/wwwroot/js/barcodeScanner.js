'use strict';

// Lokale ZXing UMD-bundle; mag niet via CDN worden geladen tijdens gebruik.
const ZXING_PATH = '/lib/zxing/zxing.min.js';

// Ondersteunde barcode-/QR-formaten voor ZXing (pilot-scope).
const SUPPORTED_FORMAT_NAMES = ['QR_CODE'];

let _zxing = null;

// Actieve BrowserMultiFormatReader van de lopende sessie.
// decodeFromConstraints retourneert Promise<void>, geen controlobject.
// Stoppen gaat altijd via reader.reset(), dat de decode-loop en mediastream beëindigt.
let _reader = null;

let _dotnetRef = null;
let _videoElementId = null;

// Monotoon oplopend sessienummer. Elke startScan-aanroep reserveert een uniek ID.
// stopScan en dispose verhogen dit getal zodat callbacks en await-terugkeerwaarden
// van de vorige sessie worden verworpen en de reader direct wordt gestopt.
let _sessionId = 0;

// Native BarcodeDetector instance (EAN-13).
let _nativeDetector = null;
let _nativeAnimationFrameId = null;

// Shared resultaatguard: zodra één decoder iets vindt, blokkeren beide decoders.
let _resultReceived = false;

const NATIVE_ERROR_THRESHOLD = 3;

async function _loadZxing() {
    if (_zxing) return _zxing;
    if (window.ZXing) {
        _zxing = window.ZXing;
        return _zxing;
    }
    return new Promise((resolve, reject) => {
        const s = document.createElement('script');
        s.src = ZXING_PATH;
        s.onload = () => {
            _zxing = window.ZXing;
            if (_zxing) {
                resolve(_zxing);
            } else {
                reject(new Error('ZXing-global niet gevonden na laden van ' + ZXING_PATH));
            }
        };
        s.onerror = () => reject(new Error('Barcodedecoder kon niet worden geladen van ' + ZXING_PATH));
        document.head.appendChild(s);
    });
}

function _buildHints(ZXing) {
    const formats = SUPPORTED_FORMAT_NAMES
        .map(n => ZXing.BarcodeFormat[n])
        .filter(f => f !== undefined);
    const hints = new Map();
    hints.set(ZXing.DecodeHintType.POSSIBLE_FORMATS, formats);
    hints.set(ZXing.DecodeHintType.TRY_HARDER, true);
    return hints;
}

/**
 * EAN-13 check digit validation (Luhn variant).
 * Verwacht: 13 decimale cijfers.
 * Retourneert: true als geldig, false anders.
 */
function _validateEan13CheckDigit(ean13) {
    if (!ean13 || !/^\d{13}$/.test(ean13)) {
        return false;
    }

    let sum = 0;
    for (let i = 0; i < 12; i++) {
        const digit = parseInt(ean13[i], 10);
        sum += (i % 2 === 0) ? digit : digit * 3;
    }

    const checkDigit = (10 - (sum % 10)) % 10;
    return checkDigit === parseInt(ean13[12], 10);
}

/**
 * Valideert native BarcodeDetector resultaat.
 * Retourneert: { valid: boolean, reason?: string }
 */
function _validateNativeDetection(rawValue) {
    if (!rawValue || typeof rawValue !== 'string') {
        return { valid: false };
    }

    const value = rawValue.trim();

    if (!/^\d{13}$/.test(value)) {
        return { valid: false, reason: `Niet 13 cijfers: "${value}"` };
    }

    if (!_validateEan13CheckDigit(value)) {
        return { valid: false, reason: `Ongeldig EAN-13 checksum: "${value}"` };
    }

    return { valid: true };
}

function _getFormatName(ZXing, formatValue) {
    try {
        return ZXing.BarcodeFormat[formatValue] ?? String(formatValue);
    } catch {
        return String(formatValue);
    }
}

/**
 * Bounded helper: wacht tot video metadata geladen is (videoWidth/videoHeight > 0).
 * Retourneert: 'ready', 'cancelled' (sessionId/result veranderd), 'timeout' (5s limit).
 * mySession, videoEl: beschermde referenties voor sessievalidatie.
 */
async function _waitVideoReady(mySession, videoEl) {
    if (!videoEl) {
        return 'cancelled';
    }

    const startTime = Date.now();
    const timeoutMs = 5000;

    return new Promise((resolve) => {
        const checkReady = () => {
            // Sessievalidatie bij elke poll.
            if (_sessionId !== mySession || _resultReceived) {
                resolve('cancelled');
                return;
            }

            // Video ready?
            if (videoEl.videoWidth > 0 && videoEl.videoHeight > 0) {
                resolve('ready');
                return;
            }

            // Timeout bereikt?
            if (Date.now() - startTime > timeoutMs) {
                resolve('timeout');
                return;
            }

            // Poll weer.
            setTimeout(checkReady, 100);
        };

        checkReady();
    });
}

/**
 * Controleert native BarcodeDetector support en EAN-13 beschikbaarheid.
 * Retourneert uitgebreide DTO met onderscheid tussen missing, check-failed en unsupported.
 * Timeout-beschermde helper; timer wordt opgeruimd in finally.
 */
async function _checkNativeBarcodeDetectorSupport() {
    // BarcodeDetector ontbreekt -> check gelukt, EAN-13 niet beschikbaar.
    if (!window.BarcodeDetector) {
        return {
            qrAvailable: true,
            nativeSupportCheckSucceeded: true,
            nativeEan13Available: false,
            supportedFormats: [],
            nativeFailureReason: 'not_available'
        };
    }

    // Probeer formats op te halen met timeout-bescherming.
    let timeoutId = null;
    try {
        const formatPromise = window.BarcodeDetector.getSupportedFormats();
        const timeoutPromise = new Promise((_, reject) => {
            timeoutId = setTimeout(() => reject(new Error('timeout')), 3000);
        });

        const formats = await Promise.race([formatPromise, timeoutPromise]);

        if (!Array.isArray(formats)) {
            return {
                qrAvailable: true,
                nativeSupportCheckSucceeded: false,
                nativeEan13Available: false,
                supportedFormats: [],
                nativeFailureReason: 'check_failed'
            };
        }

        const hasEan13 = formats.includes('ean_13');
        return {
            qrAvailable: true,
            nativeSupportCheckSucceeded: true,
            nativeEan13Available: false,
            supportedFormats: formats,
            nativeFailureReason: hasEan13 ? 'initializing' : 'format_not_supported'
        };
    } catch {
        // getSupportedFormats() gooit of hangt.
        return {
            qrAvailable: true,
            nativeSupportCheckSucceeded: false,
            nativeEan13Available: false,
            supportedFormats: [],
            nativeFailureReason: 'check_failed'
        };
    } finally {
        // Opruimen timeout.
        if (timeoutId !== null) {
            clearTimeout(timeoutId);
        }
    }
}

// Defensief: stopt tracks op het video-element als reader.reset() ze niet heeft vrijgegeven.
// ALLEEN voor globale cleanup (actuele stop/dispose).
function _stopVideoTracks() {
    const el = _videoElementId ? document.getElementById(_videoElementId) : null;
    if (el && el.srcObject) {
        el.srcObject.getTracks?.().forEach(t => { try { t.stop(); } catch { } });
        el.srcObject = null;
    }
}

// Sessie-owned cleanup: stop ALTIJD de tracks van localStream.
// Wis srcObject UITSLUITEND wanneer het nog exact deze stream is.
// Voorkomt dat stale sessies de stream van een nieuwere sessie stoppen.
function _stopSessionOwnedVideoTracks(localStream) {
    if (!localStream) return;

    // Stop tracks van deze sessie-stream unconditionally.
    localStream.getTracks?.().forEach(t => { try { t.stop(); } catch { } });

    // Wis video.srcObject ALLEEN als het nog onze stream is.
    const el = _videoElementId ? document.getElementById(_videoElementId) : null;
    if (el && el.srcObject === localStream) {
        el.srcObject = null;
    }
}

/**
 * Helper voor monotoon oplopende support-revisions per sessie.
 * Elke supportcallback krijgt een unieke, hogere revision.
 * Stale updates (oudere revision) worden in Blazor genegeerd.
 */
function _sendSupportCallback(mySession, requestId, supportStatus, localRevisionCounter) {
    if (_sessionId !== mySession || !_dotnetRef) {
        return;
    }
    const statusWithRevision = {
        ...supportStatus,
        supportRevision: localRevisionCounter.current
    };
    _dotnetRef.invokeMethodAsync('OnDecoderSupport', requestId, statusWithRevision).catch(() => { });
    localRevisionCounter.current++;
}

/**
 * Stopt de native detection loop en cleanup.
 */
function _stopNativeDetector() {
    if (_nativeAnimationFrameId !== null) {
        cancelAnimationFrame(_nativeAnimationFrameId);
        _nativeAnimationFrameId = null;
    }
    _nativeDetector = null;
}

/**
 * Start de native BarcodeDetector loop; alleen aanroepen nadat ZXing camera open is.
 * mySession: session-ID ter bescherming tegen race-conditions.
 * localReader: ZXing-reader van deze sessie (ter voorkoming van reset-crosstalk).
 * localRevisionCounter: sessielokale revision counter voor monotone support-updates.
 * getSessionStream: getter voor de stream van deze sessie.
 * Retourneert: true als loop succesvol gestart, false als constructor/video-element faalt.
 */
function _startNativeDetectionLoop(mySession, requestId, videoEl, localReader, localRevisionCounter, getSessionStream) {
    if (!window.BarcodeDetector || _resultReceived) {
        return false;
    }

    let localDetector;
    try {
        localDetector = new window.BarcodeDetector({ formats: ['ean_13'] });
    } catch {
        return false;
    }

    if (!videoEl) {
        return false;
    }

    _nativeDetector = localDetector;
    let localNativeConsecutiveErrors = 0;

    const detectionLoop = async () => {
        // Sessie- en ownership-checks.
        if (_sessionId !== mySession || _resultReceived || _nativeDetector !== localDetector || !videoEl) {
            return;
        }

        try {
            const detections = await localDetector.detect(videoEl);

            // Hercontrole na wacht.
            if (_sessionId !== mySession || _resultReceived || _nativeDetector !== localDetector) {
                return;
            }

            // Succesvol: reset lokale fout-teller.
            localNativeConsecutiveErrors = 0;

            if (detections && detections.length > 0) {
                for (const detection of detections) {
                    if (_resultReceived) break;

                    const rawValue = detection.rawValue;
                    const validation = _validateNativeDetection(rawValue);

                    if (validation.valid) {
                        _resultReceived = true;

                        // Stop native loop en ZXing loop beide, stream expliciet.
                        if (_nativeDetector === localDetector) {
                            _stopNativeDetector();
                        }
                        if (_reader === localReader) {
                            try { localReader.reset(); } catch { }
                            _reader = null;
                        }
                        // Stop de sessie-stream expliciet.
                        const sessionStream = getSessionStream();
                        _stopSessionOwnedVideoTracks(sessionStream);

                        // Genormaliseerde (getrimde) waarde doorsturen.
                        const normalizedValue = rawValue.trim();
                        if (_dotnetRef) {
                            _dotnetRef.invokeMethodAsync('OnScanResult', requestId, normalizedValue, 'EAN_13').catch(() => { });
                        }
                        return;
                    }
                }
            }
        } catch {
            // Detector error: increment lokale error counter.
            localNativeConsecutiveErrors++;

            // Hercontrole na fout: eigendom + sessie?
            if (_sessionId !== mySession || _nativeDetector !== localDetector) {
                return;
            }

            // Schakel uit na drempel.
            if (localNativeConsecutiveErrors >= NATIVE_ERROR_THRESHOLD) {
                _stopNativeDetector();
                // Rapporteer dat EAN-13 niet operationeel is met volgende revision.
                const failedSupport = {
                    qrAvailable: true,
                    nativeSupportCheckSucceeded: true,
                    nativeEan13Available: false,
                    nativeFailureReason: 'detector_error',
                    supportedFormats: []
                };
                _sendSupportCallback(mySession, requestId, failedSupport, localRevisionCounter);
                return;
            }
        }

        // Schedule volgende detectie als nog actief en eigendom.
        if (_sessionId === mySession && !_resultReceived && _nativeDetector === localDetector) {
            _nativeAnimationFrameId = requestAnimationFrame(detectionLoop);
        }
    };

    _nativeAnimationFrameId = requestAnimationFrame(detectionLoop);
    return true;
}

/**
 * Inventariseert beschikbare video-inputs en leest diagnostische gegevens van de actieve
 * cameratrack. Wordt aangeroepen nadat decodeFromConstraints is teruggekeerd (permission
 * verleend, stream actief). Resultaten worden via callbacks doorgegeven aan Blazor.
 * Alle async-stappen worden gecontroleerd op sessie-annulering en requestId-validiteit.
 * Onafhankelijk van support-rapportage.
 */
async function _reportDiagnosticsAndCameras(mySession, myRequestId, videoEl) {
    // Enumereer na toestemmingsverlening: labels zijn nu beschikbaar.
    let cameras = [];
    try {
        const devices = await navigator.mediaDevices.enumerateDevices();
        cameras = devices
            .filter(d => d.kind === 'videoinput')
            .map(d => ({ deviceId: d.deviceId, label: d.label || '' }));
    } catch { }

    if (_sessionId !== mySession) return;

    // Lees diagnostics van de actieve videotrack via het video-element.
    let diagnostics = null;
    try {
        const stream = videoEl.srcObject;
        if (stream instanceof MediaStream) {
            const tracks = stream.getVideoTracks();
            if (tracks.length > 0) {
                const track = tracks[0];
                let settings = track.getSettings();
                const activeCamera = cameras.find(c => c.deviceId === settings.deviceId);

                // Ondersteunde focusmodi ophalen.
                let supportedFocusModes = null;
                let autofocus = 'unsupported';
                try {
                    const caps = typeof track.getCapabilities === 'function'
                        ? track.getCapabilities()
                        : null;
                    if (Array.isArray(caps?.focusMode)) {
                        supportedFocusModes = caps.focusMode;
                        // Pas continuous autofocus toe als ondersteund: verplichte constraint.
                        if (caps.focusMode.includes('continuous')) {
                            try {
                                await track.applyConstraints({ focusMode: { exact: 'continuous' } });
                                // Herlezing NA applyConstraints: verificatie dat het is toegepast.
                                settings = track.getSettings();
                                autofocus = (settings.focusMode === 'continuous') ? 'applied' : 'failed';
                            } catch {
                                autofocus = 'failed';
                            }
                        }
                    }
                } catch { }

                if (_sessionId !== mySession) return;

                // Bouw diagnostics UIT de LAATST gelezen settings.
                const activeCameraFinal = cameras.find(c => c.deviceId === settings.deviceId);
                diagnostics = {
                    deviceId:              settings.deviceId           ?? null,
                    label:                 activeCameraFinal?.label     ?? null,
                    width:                 settings.width               ?? null,
                    height:                settings.height              ?? null,
                    facingMode:            settings.facingMode          ?? null,
                    supportedFocusModes:   supportedFocusModes,
                    activeFocusMode:       settings.focusMode           ?? null,
                    autofocus
                };
            }
        }
    } catch { }

    if (_sessionId !== mySession) return;

    if (_dotnetRef) {
        if (cameras.length > 0) {
            _dotnetRef.invokeMethodAsync('OnCamerasAvailable', myRequestId, cameras).catch(() => { });
        }
        if (diagnostics) {
            _dotnetRef.invokeMethodAsync('OnDiagnostics', myRequestId, diagnostics).catch(() => { });
        }
    }
}

/**
 * Start de camerastream en barcode-/QR-decoder.
 *
 * selectedDeviceId (optioneel):
 *   - null/undefined → facingMode ideal environment + ideal 1920×1080
 *   - string         → exact deviceId + ideal 1920×1080
 *
 * requestId: unieke request-ID uit Blazor; wordt doorgegeven in alle callbacks
 * zodat Blazor oude callbacks kan verwerpen.
 *
 * Na toestemmingsverlening worden beschikbare camera's en actieve diagnostics
 * gerapporteerd via OnCamerasAvailable en OnDiagnostics op dotnetRef.
 *
 * Sessie-ID beschermt tegen race-conditions bij snelle stop/herstart of camerawissel.
 */
export async function startScan(dotnetRef, videoElementId, selectedDeviceId, requestId) {
    _dotnetRef = dotnetRef;
    _videoElementId = videoElementId;
    _resultReceived = false;

    const mySession = ++_sessionId;
    const localRevisionCounter = { current: 1 };

    // Opruim vorige resources DIRECT na reservering van nieuwe sessie-id.
    // Voorkom overlap wanneer async operaties traag zijn.
    if (_reader) {
        try { _reader.reset(); } catch { }
        _reader = null;
    }
    _stopNativeDetector();
    _stopVideoTracks();

    if (!window.isSecureContext) {
        if (_sessionId === mySession) {
            await _dotnetRef.invokeMethodAsync('OnScanError', requestId, 'INSECURE_CONTEXT');
        }
        return;
    }

    if (!navigator.mediaDevices?.getUserMedia) {
        if (_sessionId === mySession) {
            await _dotnetRef.invokeMethodAsync('OnScanError', requestId, 'NO_CAMERA_API');
        }
        return;
    }

    let ZXing;
    try {
        ZXing = await _loadZxing();
    } catch {
        if (_sessionId === mySession) {
            await _dotnetRef.invokeMethodAsync('OnScanError', requestId, 'DECODER_LOAD_FAILED');
        }
        return;
    }

    if (_sessionId !== mySession) return;

    // Check native support NADAT ZXing geladen is, VOORDAT camera start.
    const nativeSupport = await _checkNativeBarcodeDetectorSupport();

    if (_sessionId !== mySession) return;

    // Rapporteer initiële supportstatus: EAN-13 nog niet beschikbaar (initializing).
    _sendSupportCallback(mySession, requestId, nativeSupport, localRevisionCounter);

    const videoEl = document.getElementById(videoElementId);
    if (!videoEl) {
        await _dotnetRef.invokeMethodAsync('OnScanError', requestId, 'CAMERA_ERROR');
        return;
    }

    // Expliciete camerakeuze via deviceId of achtercameravoorkeur via facingMode.
    const videoConstraints = selectedDeviceId
        ? { deviceId: { exact: selectedDeviceId }, width: { ideal: 1920 }, height: { ideal: 1080 } }
        : { facingMode: { ideal: 'environment' },  width: { ideal: 1920 }, height: { ideal: 1080 } };

    // Maak een nieuwe reader voor deze sessie en sla hem op zodat stopScan
    // hem via _reader.reset() kan bereiken, ook tijdens de getUserMedia-await.
    const localReader = new ZXing.BrowserMultiFormatReader(_buildHints(ZXing));
    _reader = localReader;

    // Lokale stream-referentie: om te voorkomen dat stale sessies deze stream stoppen.
    let localStream = null;

    try {
        // decodeFromConstraints: awaits getUserMedia, start daarna de continue decode-loop
        // via setTimeout en retourneert Promise<void> zodra de loop actief is.
        await localReader.decodeFromConstraints(
            { video: videoConstraints },
            videoEl,
            (result, _err) => {
                // NotFoundException per frame zonder code is normaal; negeren.
                // Verwerp callbacks van een inmiddels geannuleerde sessie.
                if (_sessionId !== mySession || _resultReceived) return;
                if (!result) return;

                _resultReceived = true;
                const value = result.getText();
                const format = _getFormatName(ZXing, result.getBarcodeFormat());

                // Defensieve bepaling van sessie-stream: localStream mogelijk nog niet vastgelegd.
                let sessionStream = localStream;
                if (!sessionStream && _sessionId === mySession) {
                    sessionStream = videoEl.srcObject instanceof MediaStream ? videoEl.srcObject : null;
                }

                // Stopt de native detection loop ook (alleen als eigendom).
                if (_nativeDetector) {
                    _stopNativeDetector();
                }

                // Stopt de decode-loop en de sessie-mediastream expliciet.
                try { localReader.reset(); } catch { }
                if (_reader === localReader) _reader = null;
                _stopSessionOwnedVideoTracks(sessionStream);

                _dotnetRef?.invokeMethodAsync('OnScanResult', requestId, value, format).catch(() => { });
            }
        );

        // decodeFromConstraints is teruggekeerd: camera actief, decode-loop loopt via setTimeout.
        // Als stopScan tussentijds werd aangeroepen, is de reader al gereset maar kan de loop
        // nog kort actief zijn geweest; stop defensief opnieuw met sessie-ownership.
        if (_sessionId !== mySession) {
            try { localReader.reset(); } catch { }
            _stopSessionOwnedVideoTracks(localStream);
            return;
        }

        // Leg de stream vast zodat stale sessies hem niet kunnen stoppen.
        localStream = videoEl.srcObject instanceof MediaStream ? videoEl.srcObject : null;

        // Getter voor de sessie-stream (voor native loop).
        const getSessionStream = () => localStream || (videoEl?.srcObject instanceof MediaStream ? videoEl.srcObject : null);

        // Wacht tot videoWidth/Height beschikbaar zijn (bounded).
        const readyResult = await _waitVideoReady(mySession, videoEl);

        // Bij cancelled (QR al gevonden): stille cleanup zonder diagnostics/support-update.
        if (readyResult === 'cancelled') {
            return;
        }

        if (_sessionId !== mySession) {
            try { localReader.reset(); } catch { }
            _stopSessionOwnedVideoTracks(localStream);
            return;
        }

        // Verwerk readyResult.
        let videoReady = false;
        if (readyResult === 'ready') {
            videoReady = true;
        } else if (readyResult === 'timeout') {
            // Video timeout maar QR blijft werken; rapporteer EAN-13 als niet operationeel.
            const updatedSupport = {
                ...nativeSupport,
                nativeEan13Available: false,
                nativeFailureReason: 'video_not_ready'
            };
            _sendSupportCallback(mySession, requestId, updatedSupport, localRevisionCounter);
        }

        // Start native detection loop als echt operationeel.
        let nativeStarted = false;
        if (videoReady && nativeSupport.nativeSupportCheckSucceeded &&
            (nativeSupport.nativeFailureReason === 'initializing' || nativeSupport.nativeFailureReason === null)) {
            nativeStarted = _startNativeDetectionLoop(mySession, requestId, videoEl, localReader, localRevisionCounter, getSessionStream);

            // Constructor slaagt: rapporteer operationeel beschikbaar.
            if (nativeStarted) {
                const operationalSupport = {
                    ...nativeSupport,
                    nativeEan13Available: true,
                    nativeFailureReason: null
                };
                _sendSupportCallback(mySession, requestId, operationalSupport, localRevisionCounter);
            } else {
                // Constructor faalt -> rapporteer operationeel falen.
                const failedSupport = {
                    ...nativeSupport,
                    nativeEan13Available: false,
                    nativeFailureReason: 'detector_init_failed'
                };
                _sendSupportCallback(mySession, requestId, failedSupport, localRevisionCounter);
            }
        }

        // Camera actief: inventariseer beschikbare camera's en lees diagnostics (onafhankelijk).
        await _reportDiagnosticsAndCameras(mySession, requestId, videoEl);

    } catch (e) {
        // LOKALE cleanup eerst (altijd, ook voor stale):
        // - reset eigen reader;
        // - stop eigen stream (sessie-owned).
        try { localReader.reset(); } catch { }
        if (_reader === localReader) _reader = null;
        _stopSessionOwnedVideoTracks(localStream);

        // DAARNA: sessiecontrole. Geen globale cleanup voor stale!
        if (_sessionId !== mySession) {
            return;
        }

        // Actuele fout: globale native cleanup en foutrapport.
        _stopNativeDetector();

        const code =
            e?.name === 'NotAllowedError' ? 'PERMISSION_DENIED' :
            e?.name === 'NotFoundError'   ? 'NO_CAMERA' :
                                            'CAMERA_ERROR';
        try { await _dotnetRef?.invokeMethodAsync('OnScanError', requestId, code); } catch { }
    }
}

/**
 * Stopt de actieve scan-sessie en geeft camera en decode-loop vrij.
 * Verhoogt het sessienummer zodat lopende callbacks worden verworpen en een
 * nog wachtende getUserMedia-terugkeer de reader direct reset en stopt.
 */
export function stopScan() {
    _sessionId++;
    _resultReceived = false;

    const reader = _reader;
    _reader = null;

    if (reader) {
        try { reader.reset(); } catch { }
    }

    _stopNativeDetector();
    _stopVideoTracks();
}

/**
 * Ruimt alle resources op; aan te roepen bij component-disposal.
 */
export function dispose() {
    stopScan();
    _dotnetRef = null;
}

/**
 * Geeft terug of de huidige browsercontext beveiligd is (HTTPS of localhost).
 * Aan te roepen bij de eerste render om de HTTP-waarschuwing direct te tonen.
 */
export function checkSecureContext() {
    return Boolean(window.isSecureContext);
}
