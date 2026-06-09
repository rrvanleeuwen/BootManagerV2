'use strict';

// Lokale Quagga2 1.12.1 vendor bundle
const QUAGGA2_PATH = '/lib/quagga2/quagga.min.js';

let _quagga = null;
let _dotnetRef = null;

// Sessie-ID ter bescherming tegen race-conditions en stale callbacks.
let _sessionId = 0;

// Actieve eigendoms-sessie: alleen deze sessie mag globale resources cleanup'en.
let _activeSessionId = null;

// Momenteel actieve Quagga2-sessie (ingesteld in init).
let _currentQuagga = null;

// De geregistreerde detection handler (voor cleanup).
let _activeDetectionHandler = null;

// De geregistreerde processed handler (voor diagnostics).
let _activeProcessedHandler = null;

// Video-element (voor track cleanup op Quagga's eigendom).
let _videoElement = null;

// Container-element ID (target voor Quagga2).
let _containerElementId = null;

// Flag: is een result al ontvangen en verwerkt in deze sessie?
let _resultReceived = false;

// Diagnostics throttling: last report timestamp.
let _lastDiagnosticsReport = 0;
const DIAGNOSTICS_THROTTLE_MS = 1000;

// Verwerkte frames: module-level counter voor cumulative frame count per sessie.
let _processedFrames = 0;

// Serialize Quagga2.init(): promise van huidge init in flight.
let _currentInitPromise = Promise.resolve();

/**
 * Laden van de lokale Quagga2 1.12.1 vendor bundle.
 */
async function _loadQuagga2() {
    if (_quagga) return _quagga;
    if (window.Quagga) {
        _quagga = window.Quagga;
        return _quagga;
    }
    return new Promise((resolve, reject) => {
        const s = document.createElement('script');
        s.src = QUAGGA2_PATH;
        s.onload = () => {
            _quagga = window.Quagga;
            if (_quagga) {
                resolve(_quagga);
            } else {
                reject(new Error('Quagga2-global niet gevonden na laden van ' + QUAGGA2_PATH));
            }
        };
        s.onerror = () => reject(new Error('Quagga2-decoder kon niet worden geladen van ' + QUAGGA2_PATH));
        document.head.appendChild(s);
    });
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
 * Interne cleanup: unregistreert handlers, stopt Quagga2, geeft camera/refs vrij.
 *
 * sessionId (optioneel):
 *   - undefined/null → unconditional cleanup (stop/dispose)
 *   - number       → cleanup only if this session owns the resources (async stale paths)
 *
 * Aangeroepen bij start, stop, error, detection en disposal.
 */
function _cleanup(sessionId) {
    // Owned cleanup: controleer eigenaarschap.
    if (typeof sessionId === 'number' && sessionId !== _activeSessionId) {
        return;
    }

    // Unregister detection handler.
    if (_currentQuagga && _activeDetectionHandler) {
        try {
            _currentQuagga.offDetected(_activeDetectionHandler);
        } catch { }
        _activeDetectionHandler = null;
    }

    // Unregister processed handler.
    if (_currentQuagga && _activeProcessedHandler) {
        try {
            _currentQuagga.offProcessed(_activeProcessedHandler);
        } catch { }
        _activeProcessedHandler = null;
    }

    // Stop Quagga2.
    if (_currentQuagga) {
        try {
            _currentQuagga.stop();
        } catch { }
        _currentQuagga = null;
    }

    // Stop video tracks.
    _stopVideoTracks();
    _videoElement = null;
    _containerElementId = null;

    _resultReceived = false;
    _activeSessionId = null;
    _processedFrames = 0;
    _lastDiagnosticsReport = 0;
}

/**
 * Registreert een detectie via callback naar Blazor.
 * requestId wordt meegezonden voor stale-session bescherming.
 */
function _logDetection(requestId, value, accepted, reason) {
    const entry = {
        value,
        accepted,
        reason: reason || null
    };

    // Rapport naar Blazor voor de UI.
    if (_dotnetRef) {
        _dotnetRef.invokeMethodAsync('OnQuaggaDetection', requestId, entry).catch(() => { });
    }
}

/**
 * Verwerkt een Quagga2-detectie en valideert het resultaat.
 * Retourneert: { valid: boolean, reason?: string }
 */
function _validateDetection(detection) {
    if (!detection || !detection.codeResult || !detection.codeResult.code) {
        return { valid: false };
    }

    const code = detection.codeResult.code;

    // Check: exactelijk 13 decimale cijfers?
    if (!/^\d{13}$/.test(code)) {
        return {
            valid: false,
            reason: `Niet 13 cijfers: "${code}" (${code.length} tekens)`
        };
    }

    // Check: geldig EAN-13 checksum?
    if (!_validateEan13CheckDigit(code)) {
        return {
            valid: false,
            reason: `Ongeldig EAN-13 checksum: "${code}"`
        };
    }

    return { valid: true };
}

/**
 * Defensief: stopt alle mediatracks op het video-element.
 */
function _stopVideoTracks() {
    if (!_videoElement) return;
    if (_videoElement.srcObject) {
        _videoElement.srcObject.getTracks?.().forEach(t => { try { t.stop(); } catch { } });
        _videoElement.srcObject = null;
    }
}

/**
 * Start de Quagga2-scanner met EAN-13 decode configuratie.
 * requestId: unieke request-ID uit Blazor (voor callbacks).
 * processingSize: Quagga2 inputStream.size (800, 1280 of 1600). Default 1280 als niet gegeven/ongeldig.
 */
export async function startScan(dotnetRef, videoElementId, requestId, processingSize) {
    const mySession = ++_sessionId;

    // Cleanup eerdere sessie (voordat nieuwe refs toekennen).
    _cleanup();

    // Valideer en stel processingSize in (default 1280).
    if (!processingSize || ![800, 1280, 1600].includes(processingSize)) {
        processingSize = 1280;
    }

    // Assign eigenaarschap en nieuwe refs na cleanup.
    _activeSessionId = mySession;
    _dotnetRef = dotnetRef;
    _containerElementId = videoElementId;
    _lastDiagnosticsReport = 0;

    // Controleer secure context.
    if (!window.isSecureContext) {
        if (_sessionId === mySession && _dotnetRef) {
            await _dotnetRef.invokeMethodAsync('OnQuaggaError', requestId, 'INSECURE_CONTEXT');
        }
        return;
    }

    // Controleer mediaDevices API.
    if (!navigator.mediaDevices?.getUserMedia) {
        if (_sessionId === mySession && _dotnetRef) {
            await _dotnetRef.invokeMethodAsync('OnQuaggaError', requestId, 'NO_CAMERA_API');
        }
        return;
    }

    // Laad Quagga2.
    let Quagga2;
    try {
        Quagga2 = await _loadQuagga2();
    } catch {
        if (_sessionId === mySession && _dotnetRef) {
            await _dotnetRef.invokeMethodAsync('OnQuaggaError', requestId, 'DECODER_LOAD_FAILED');
        }
        return;
    }

    if (_sessionId !== mySession) return;

    // Resolve container element (Quagga2 target).
    const containerEl = document.getElementById(videoElementId);
    if (!containerEl) {
        if (_sessionId === mySession && _dotnetRef) {
            await _dotnetRef.invokeMethodAsync('OnQuaggaError', requestId, 'CAMERA_ERROR');
        }
        return;
    }

    // Resolve video element inside container (voor cleanup).
    const videoEl = containerEl.querySelector('video');
    if (!videoEl) {
        if (_sessionId === mySession && _dotnetRef) {
            await _dotnetRef.invokeMethodAsync('OnQuaggaError', requestId, 'CAMERA_ERROR');
        }
        return;
    }

    // Sla video element op voor cleanup.
    _videoElement = videoEl;

    // Bewaar de Quagga-instantie voor deze sessie.
    _currentQuagga = Quagga2;

    // Quagga2 init met EAN-13 configuratie (proven instellingen).
    // Pass container element zodat Quagga2 de video element vindt en gebruiken.
    // size: processingSize is de instelbare Quagga2 processing size (NIET hetzelfde als camera constraints width).
    const config = {
        inputStream: {
            type: 'LiveStream',
            size: processingSize,
            constraints: {
                facingMode: 'environment',
                width: { ideal: 1920 },
                height: { ideal: 1080 }
            },
            target: containerEl
        },
        locator: {
            patchSize: 'large',
            halfSample: false
        },
        numOfWorkers: 2,
        decoder: {
            readers: ['ean_reader']
        },
        locate: true
    };

    // Bouw lokale operation promise en wacht erop.
    const localOperation = _currentInitPromise.then(async () => {
        // Controleer: is deze sessie nog steeds current?
        if (_sessionId !== mySession || _activeSessionId !== mySession) {
            return;
        }

        try {
            await Quagga2.init(config);

            // Controleer opnieuw: is deze sessie nog steeds current na init?
            if (_sessionId !== mySession || _activeSessionId !== mySession) {
                // Stale init: zet Quagga2 singleton/stream LOKAAL vrij,
                // maar raak geen newer session metadata aan.
                try {
                    Quagga2.stop();
                } catch { }
                try {
                    // Disconnect streams
                    const inputStream = Quagga2.QuaggaState?.inputStream;
                    if (inputStream) {
                        inputStream.release();
                    }
                } catch { }
                return;
            }

            // Registreer detection handler en sla deze op voor cleanup.
            const detectionHandler = (detection) => {
                if (_sessionId !== mySession || _activeSessionId !== mySession || _resultReceived || !_dotnetRef) return;

                const validation = _validateDetection(detection);

                if (validation.valid) {
                    const code = detection.codeResult.code;
                    _resultReceived = true;

                    // Log acceptatie.
                    _logDetection(requestId, code, true);

                    // Cleanup (owned) en stop scanning voordat .NET aangeroepen wordt.
                    _cleanup(mySession);

                    // Rapport het geldige resultaat naar Blazor.
                    _dotnetRef.invokeMethodAsync('OnQuaggaResult', requestId, code, 'EAN_13').catch(() => { });
                } else {
                    // Geweigerde detectie loggen maar doorscanning toestaan.
                    _logDetection(
                        requestId,
                        detection.codeResult?.code || '?',
                        false,
                        validation.reason
                    );
                }
            };

            _activeDetectionHandler = detectionHandler;
            Quagga2.onDetected(detectionHandler);

            // Registreer processed handler voor minimale diagnostics.
            const processedHandler = (result) => {
                if (_sessionId !== mySession || _activeSessionId !== mySession || !_dotnetRef) return;

                // Incrementeer frame counter op elke verwerking.
                _processedFrames++;

                const now = Date.now();
                if (now - _lastDiagnosticsReport < DIAGNOSTICS_THROTTLE_MS) return;
                _lastDiagnosticsReport = now;

                try {
                    // Verzamel diagnostics.
                    const locatedBoxes = result.boxes ? result.boxes.length : 0;

                    let cameraWidth = null;
                    let cameraHeight = null;
                    let maxCameraWidth = null;
                    let maxCameraHeight = null;
                    let analysisWidth = null;
                    let analysisHeight = null;

                    try {
                        const track = Quagga2.CameraAccess?.getActiveTrack?.();
                        if (track) {
                            const settings = track.getSettings?.();
                            if (settings) {
                                cameraWidth = settings.width;
                                cameraHeight = settings.height;
                            }
                            try {
                                const capabilities = track.getCapabilities?.();
                                if (capabilities && capabilities.width && capabilities.height) {
                                    const widthRange = capabilities.width;
                                    const heightRange = capabilities.height;
                                    if (widthRange && typeof widthRange.max !== 'undefined') {
                                        maxCameraWidth = widthRange.max;
                                    }
                                    if (heightRange && typeof heightRange.max !== 'undefined') {
                                        maxCameraHeight = heightRange.max;
                                    }
                                }
                            } catch { }
                        }
                    } catch { }

                    // Bereken effectieve analyse-resolutie volgens Quagga2-regel.
                    if (cameraWidth && cameraHeight) {
                        if (cameraWidth / cameraHeight > 1) {
                            analysisWidth = processingSize;
                            analysisHeight = Math.floor(cameraHeight / cameraWidth * processingSize);
                        } else {
                            analysisWidth = Math.floor(cameraWidth / cameraHeight * processingSize);
                            analysisHeight = processingSize;
                        }
                    }

                    const diagnostics = {
                        processedFrames: _processedFrames,
                        locatedBoxes: locatedBoxes,
                        cameraWidth: cameraWidth,
                        cameraHeight: cameraHeight,
                        maxCameraWidth: maxCameraWidth,
                        maxCameraHeight: maxCameraHeight,
                        configuredProcessingSize: processingSize,
                        analysisWidth: analysisWidth,
                        analysisHeight: analysisHeight
                    };

                    _dotnetRef.invokeMethodAsync('OnQuaggaDiagnostics', requestId, diagnostics).catch(() => { });
                } catch { }
            };

            _activeProcessedHandler = processedHandler;
            Quagga2.onProcessed(processedHandler);

            // Start de scanner.
            Quagga2.start();

            // Rapport naar Blazor NADAT start daadwerkelijk voltooid.
            if (_sessionId === mySession && _dotnetRef) {
                await _dotnetRef.invokeMethodAsync('OnQuaggaStarted', requestId);
            }

        } catch (e) {
            // Controleer: is deze sessie nog steeds current na error?
            if (_sessionId !== mySession || _activeSessionId !== mySession) {
                // Stale error: zet Quagga2 singleton/stream LOKAAL vrij.
                try {
                    Quagga2.stop();
                } catch { }
                try {
                    const inputStream = Quagga2.QuaggaState?.inputStream;
                    if (inputStream) {
                        inputStream.release();
                    }
                } catch { }
                return;
            }

            // Cleanup (owned) en rapport fout naar Blazor.
            _cleanup(mySession);

            if (_dotnetRef) {
                const code =
                    e?.name === 'NotAllowedError' ? 'PERMISSION_DENIED' :
                    e?.name === 'NotFoundError'   ? 'NO_CAMERA' :
                                                    'CAMERA_ERROR';
                await _dotnetRef.invokeMethodAsync('OnQuaggaError', requestId, code);
            }
        }
    }).catch(() => {
        // Serialisatie error: cleanup en bericht fout.
        if (_sessionId === mySession && _dotnetRef) {
            _cleanup(mySession);
            _dotnetRef.invokeMethodAsync('OnQuaggaError', requestId, 'CAMERA_ERROR').catch(() => { });
        }
    });

    // Update queue: volgende init wacht tot deze operation voltooid (inclusief stale cleanup).
    _currentInitPromise = localOperation;

    // Blazor wacht tot init/start daadwerkelijk voltooid.
    await localOperation;
}

/**
 * Stopt de Quagga2-scanner en geeft resources vrij.
 * Unconditional cleanup: deze actie invalidateert de huidge sessie.
 */
export function stopScan() {
    _sessionId++;
    _cleanup();  // Unconditional: stop invalidates current session
}

/**
 * Ruimt alle resources op (component disposal).
 * Unconditional cleanup: deze actie invalidateert alle sessies.
 */
export function dispose() {
    _sessionId++;
    _cleanup();  // Unconditional: dispose invalidates all sessions
    _dotnetRef = null;
}

/**
 * Geeft terug of de browsercontext beveiligd is (HTTPS of localhost).
 */
export function checkSecureContext() {
    return Boolean(window.isSecureContext);
}
