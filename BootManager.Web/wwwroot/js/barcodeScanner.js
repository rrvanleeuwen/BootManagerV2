'use strict';

// Lokale ZXing UMD-bundle; mag niet via CDN worden geladen tijdens gebruik.
const ZXING_PATH = '/lib/zxing/zxing.min.js';

// Ondersteunde barcode-/QR-formaten (pilot-scope).
const SUPPORTED_FORMAT_NAMES = ['QR_CODE', 'EAN_13', 'EAN_8', 'UPC_A', 'CODE_128'];

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
    // Verbetert herkenning van lineaire barcodes (EAN-13/EAN-8/UPC-A/Code 128)
    // op mobiele camera's met lagere beeldkwaliteit of scherpte-variatie.
    hints.set(ZXing.DecodeHintType.TRY_HARDER, true);
    return hints;
}

function _getFormatName(ZXing, formatValue) {
    try {
        return ZXing.BarcodeFormat[formatValue] ?? String(formatValue);
    } catch {
        return String(formatValue);
    }
}

// Defensief: stopt tracks op het video-element als reader.reset() ze niet heeft vrijgegeven.
function _stopVideoTracks() {
    const el = _videoElementId ? document.getElementById(_videoElementId) : null;
    if (el && el.srcObject) {
        el.srcObject.getTracks?.().forEach(t => { try { t.stop(); } catch { } });
        el.srcObject = null;
    }
}

/**
 * Inventariseert beschikbare video-inputs en leest diagnostische gegevens van de actieve
 * cameratrack. Wordt aangeroepen nadat decodeFromConstraints is teruggekeerd (permission
 * verleend, stream actief). Resultaten worden via callbacks doorgegeven aan Blazor.
 * Alle async-stappen worden gecontroleerd op sessie-annulering en requestId-validiteit.
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

    const mySession = ++_sessionId;
    let resultReceived = false;

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

    const videoEl = document.getElementById(videoElementId);
    if (!videoEl) {
        await _dotnetRef.invokeMethodAsync('OnScanError', requestId, 'CAMERA_ERROR');
        return;
    }

    // Eerdere reader opruimen.
    if (_reader) {
        try { _reader.reset(); } catch { }
        _reader = null;
    }

    // Expliciete camerakeuze via deviceId of achtercameravoorkeur via facingMode.
    const videoConstraints = selectedDeviceId
        ? { deviceId: { exact: selectedDeviceId }, width: { ideal: 1920 }, height: { ideal: 1080 } }
        : { facingMode: { ideal: 'environment' },  width: { ideal: 1920 }, height: { ideal: 1080 } };

    // Maak een nieuwe reader voor deze sessie en sla hem op zodat stopScan
    // hem via _reader.reset() kan bereiken, ook tijdens de getUserMedia-await.
    const localReader = new ZXing.BrowserMultiFormatReader(_buildHints(ZXing));
    _reader = localReader;

    try {
        // decodeFromConstraints: awaits getUserMedia, start daarna de continue decode-loop
        // via setTimeout en retourneert Promise<void> zodra de loop actief is.
        await localReader.decodeFromConstraints(
            { video: videoConstraints },
            videoEl,
            (result, _err) => {
                // NotFoundException per frame zonder code is normaal; negeren.
                // Verwerp callbacks van een inmiddels geannuleerde sessie.
                if (_sessionId !== mySession || resultReceived) return;
                if (!result) return;

                resultReceived = true;
                const value = result.getText();
                const format = _getFormatName(ZXing, result.getBarcodeFormat());

                // Stopt de decode-loop en de mediastream.
                try { localReader.reset(); } catch { }
                if (_reader === localReader) _reader = null;
                _stopVideoTracks();

                _dotnetRef?.invokeMethodAsync('OnScanResult', requestId, value, format).catch(() => { });
            }
        );

        // decodeFromConstraints is teruggekeerd: camera actief, decode-loop loopt via setTimeout.
        // Als stopScan tussentijds werd aangeroepen, is de reader al gereset maar kan de loop
        // nog kort actief zijn geweest; stop defensief opnieuw.
        if (_sessionId !== mySession) {
            try { localReader.reset(); } catch { }
            _stopVideoTracks();
            return;
        }

        // Camera actief: inventariseer beschikbare camera's en lees diagnostics.
        await _reportDiagnosticsAndCameras(mySession, requestId, videoEl);

    } catch (e) {
        // Zorg dat de reader niet blijft hangen ongeacht de oorzaak.
        if (_reader === localReader) _reader = null;

        if (_sessionId !== mySession) {
            try { localReader.reset(); } catch { }
            _stopVideoTracks();
            return;
        }

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

    const reader = _reader;
    _reader = null;

    if (reader) {
        try { reader.reset(); } catch { }
    }
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
