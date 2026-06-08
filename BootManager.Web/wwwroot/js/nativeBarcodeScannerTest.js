'use strict';

let _dotnetRef = null;

// Sessie-ID ter bescherming tegen race-conditions en stale callbacks.
let _sessionId = 0;

// Actieve eigendoms-sessie: alleen deze sessie mag globale resources cleanup'en.
let _activeSessionId = null;

// Video-element (voor stream cleanup).
let _videoElement = null;

// MediaStream (voor track cleanup).
let _mediaStream = null;

// Container-element ID (target).
let _containerElementId = null;

// Animation frame ID (voor cleanup).
let _animationFrameId = null;

// Flag: is een result al ontvangen in deze sessie?
let _resultReceived = false;

// Diagnostics: aantal detectiepogingen en laatste detecties per poging.
let _detectionAttempts = 0;
let _lastDetectionCount = 0;

// Diagnostics throttling: last report timestamp.
let _lastDiagnosticsReport = 0;
const DIAGNOSTICS_THROTTLE_MS = 500;

// BarcodeDetector instance per sessie.
let _detector = null;

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
 * Interne cleanup: stopt video tracks, animation loop en detector.
 *
 * sessionId (optioneel):
 *   - undefined/null → unconditional cleanup (stop/dispose)
 *   - number       → cleanup only if this session owns the resources
 */
function _cleanup(sessionId) {
    // Owned cleanup: controleer eigenaarschap.
    if (typeof sessionId === 'number' && sessionId !== _activeSessionId) {
        return;
    }

    // Stop animation loop.
    if (_animationFrameId !== null) {
        cancelAnimationFrame(_animationFrameId);
        _animationFrameId = null;
    }

    // Stop video tracks.
    _stopVideoTracks();
    _videoElement = null;
    _mediaStream = null;
    _containerElementId = null;

    // Clear detector instance.
    _detector = null;

    _resultReceived = false;
    _activeSessionId = null;
    _detectionAttempts = 0;
    _lastDetectionCount = 0;
    _lastDiagnosticsReport = 0;
}

/**
 * Defensief: stopt alle mediatracks op de stream.
 */
function _stopVideoTracks() {
    if (!_mediaStream) return;
    try {
        _mediaStream.getTracks?.().forEach(t => {
            try { t.stop(); } catch { }
        });
    } catch { }
    _mediaStream = null;
    if (_videoElement) {
        _videoElement.srcObject = null;
    }
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
        _dotnetRef.invokeMethodAsync('OnNativeDetection', requestId, entry).catch(() => { });
    }
}

/**
 * Verwerkt een BarcodeDetector-detectie en valideert het resultaat.
 * Retourneert: { valid: boolean, reason?: string }
 */
function _validateDetection(rawValue) {
    if (!rawValue || typeof rawValue !== 'string') {
        return { valid: false };
    }

    const value = rawValue.trim();

    // Check: exact 13 decimale cijfers?
    if (!/^\d{13}$/.test(value)) {
        return {
            valid: false,
            reason: `Niet 13 cijfers: "${value}" (${value.length} tekens)`
        };
    }

    // Check: geldig EAN-13 checksum?
    if (!_validateEan13CheckDigit(value)) {
        return {
            valid: false,
            reason: `Ongeldig EAN-13 checksum: "${value}"`
        };
    }

    return { valid: true };
}

/**
 * Retourneert expliciete DTO met support-informatie.
 * supportCheckSucceeded geeft aan of getSupportedFormats() succesvol is uitgevoerd.
 * supportedFormats bevat altijd kommagescheiden volledige lijst wanneer supportCheckSucceeded = true.
 */
export async function checkBarcodeDetectorSupport() {
    const isSecureContext = Boolean(window.isSecureContext);
    const isBarcodeDetectorAvailable = Boolean(window.BarcodeDetector);

    if (!isBarcodeDetectorAvailable) {
        return {
            isSecureContext: isSecureContext,
            isBarcodeDetectorAvailable: false,
            supportCheckSucceeded: false,
            isEan13Supported: false,
            supportedFormats: ''
        };
    }

    try {
        const formats = await window.BarcodeDetector.getSupportedFormats();
        if (!formats || !Array.isArray(formats)) {
            return {
                isSecureContext: isSecureContext,
                isBarcodeDetectorAvailable: true,
                supportCheckSucceeded: false,
                isEan13Supported: false,
                supportedFormats: ''
            };
        }

        const formatsStr = formats.join(', ');
        const hasEan13 = formats.includes('ean_13');

        return {
            isSecureContext: isSecureContext,
            isBarcodeDetectorAvailable: true,
            supportCheckSucceeded: true,
            isEan13Supported: hasEan13,
            supportedFormats: formatsStr
        };
    } catch {
        return {
            isSecureContext: isSecureContext,
            isBarcodeDetectorAvailable: true,
            supportCheckSucceeded: false,
            isEan13Supported: false,
            supportedFormats: ''
        };
    }
}

/**
 * Start de native BarcodeDetector scanner.
 * requestId: unieke request-ID uit Blazor (voor callbacks).
 */
export async function startScan(dotnetRef, videoElementId, requestId) {
    const mySession = ++_sessionId;

    // Cleanup eerdere sessie.
    _cleanup();

    // Assign eigenaarschap en refs na cleanup.
    _activeSessionId = mySession;
    _dotnetRef = dotnetRef;
    _containerElementId = videoElementId;
    _lastDiagnosticsReport = 0;

    // Controleer secure context.
    if (!window.isSecureContext) {
        if (_sessionId === mySession && _dotnetRef) {
            await _dotnetRef.invokeMethodAsync('OnNativeError', requestId, 'INSECURE_CONTEXT');
        }
        return;
    }

    // Controleer mediaDevices API.
    if (!navigator.mediaDevices?.getUserMedia) {
        if (_sessionId === mySession && _dotnetRef) {
            await _dotnetRef.invokeMethodAsync('OnNativeError', requestId, 'NO_CAMERA_API');
        }
        return;
    }

    // Controleer BarcodeDetector.
    if (!window.BarcodeDetector) {
        if (_sessionId === mySession && _dotnetRef) {
            await _dotnetRef.invokeMethodAsync('OnNativeError', requestId, 'DETECTOR_NOT_READY');
        }
        return;
    }

    // Resolve container element.
    const containerEl = document.getElementById(videoElementId);
    if (!containerEl) {
        if (_sessionId === mySession && _dotnetRef) {
            await _dotnetRef.invokeMethodAsync('OnNativeError', requestId, 'CAMERA_ERROR');
        }
        return;
    }

    // Resolve video element inside container.
    const videoEl = containerEl.querySelector('video');
    if (!videoEl) {
        if (_sessionId === mySession && _dotnetRef) {
            await _dotnetRef.invokeMethodAsync('OnNativeError', requestId, 'CAMERA_ERROR');
        }
        return;
    }

    _videoElement = videoEl;

    // Vraag camera-stream aan met voorkeur voor environment-facing.
    let stream;
    try {
        stream = await navigator.mediaDevices.getUserMedia({
            video: {
                facingMode: { ideal: 'environment' },
                width: { ideal: 1920 },
                height: { ideal: 1080 }
            },
            audio: false
        });
    } catch (e) {
        if (_sessionId !== mySession) return;

        const code =
            e?.name === 'NotAllowedError' ? 'PERMISSION_DENIED' :
            e?.name === 'NotFoundError'   ? 'NO_CAMERA' :
                                            'CAMERA_ERROR';
        if (_dotnetRef) {
            await _dotnetRef.invokeMethodAsync('OnNativeError', requestId, code);
        }
        return;
    }

    if (_sessionId !== mySession) {
        stream.getTracks().forEach(t => t.stop());
        return;
    }

    _mediaStream = stream;
    _videoElement.srcObject = stream;

    // Initialiseert detector.
    try {
        _detector = new window.BarcodeDetector({ formats: ['ean_13'] });
    } catch {
        if (_sessionId !== mySession) {
            stream.getTracks().forEach(t => t.stop());
            return;
        }

        // Cleanup: stream stoppen en globale refs wissen.
        _cleanup(mySession);

        if (_dotnetRef) {
            await _dotnetRef.invokeMethodAsync('OnNativeError', requestId, 'DETECTOR_NOT_READY');
        }
        return;
    }

    // Wacht tot video metadata geladen is.
    await new Promise((resolve) => {
        const onLoadedMetadata = () => {
            _videoElement?.removeEventListener('loadedmetadata', onLoadedMetadata);
            resolve();
        };
        _videoElement?.addEventListener('loadedmetadata', onLoadedMetadata, { once: true });

        // Safety timeout: resolve na 5 seconden als event niet komt.
        setTimeout(resolve, 5000);
    });

    if (_sessionId !== mySession) {
        stream.getTracks().forEach(t => t.stop());
        return;
    }

    // Start detection loop.
    _detectionAttempts = 0;
    _lastDetectionCount = 0;

    const detectionLoop = async () => {
        if (_sessionId !== mySession || _activeSessionId !== mySession || _resultReceived || !_detector || !_videoElement) {
            return;
        }

        try {
            const detections = await _detector.detect(_videoElement);

            if (_sessionId !== mySession || _activeSessionId !== mySession || _resultReceived) {
                return;
            }

            _detectionAttempts++;
            _lastDetectionCount = detections ? detections.length : 0;

            // Report diagnostics periodiek.
            const now = Date.now();
            if (now - _lastDiagnosticsReport >= DIAGNOSTICS_THROTTLE_MS) {
                _lastDiagnosticsReport = now;

                if (_dotnetRef) {
                    const diagnostics = {
                        detectionAttempts: _detectionAttempts,
                        lastDetectionCount: _lastDetectionCount,
                        cameraWidth: _videoElement?.videoWidth,
                        cameraHeight: _videoElement?.videoHeight
                    };
                    _dotnetRef.invokeMethodAsync('OnNativeDiagnostics', requestId, diagnostics).catch(() => { });
                }
            }

            // Verwerk detections.
            if (detections && detections.length > 0) {
                for (const detection of detections) {
                    if (_resultReceived) break;

                    const rawValue = detection.rawValue;
                    const validation = _validateDetection(rawValue);

                    if (validation.valid) {
                        _resultReceived = true;

                        // Log acceptatie.
                        _logDetection(requestId, rawValue, true);

                        // Cleanup en stop scanning voordat .NET aangeroepen wordt.
                        _cleanup(mySession);

                        // Rapport het geldige resultaat naar Blazor.
                        if (_dotnetRef) {
                            _dotnetRef.invokeMethodAsync('OnNativeResult', requestId, rawValue, 'EAN_13').catch(() => { });
                        }
                        return;
                    } else {
                        // Geweigerde detectie loggen maar doorscanning toestaan.
                        _logDetection(requestId, rawValue || '?', false, validation.reason);
                    }
                }
            }
        } catch {
            // Detector error: continue with next frame.
        }

        // Schedule volgende detectie.
        if (_sessionId === mySession && _activeSessionId === mySession && !_resultReceived) {
            _animationFrameId = requestAnimationFrame(detectionLoop);
        }
    };

    // Start detection loop.
    _animationFrameId = requestAnimationFrame(detectionLoop);
}

/**
 * Stopt de scanner en geeft resources vrij.
 * Unconditional cleanup: deze actie invalidateert de huidge sessie.
 */
export function stopScan() {
    _sessionId++;
    _cleanup();
}

/**
 * Ruimt alle resources op (component disposal).
 * Unconditional cleanup: deze actie invalidateert alle sessies.
 */
export function dispose() {
    _sessionId++;
    _cleanup();
    _dotnetRef = null;
}
