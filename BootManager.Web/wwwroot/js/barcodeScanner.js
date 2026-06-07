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
 * Start de camerastream en barcode-/QR-decoder.
 *
 * Per aanroep wordt een nieuwe BrowserMultiFormatReader aangemaakt en bewaard als
 * de actieve reader (_reader). decodeFromConstraints retourneert Promise<void>;
 * er is geen controlobject. Stoppen gebeurt altijd met reader.reset().
 *
 * Elke aanroep reserveert een uniek sessienummer (mySession). Callbacks en
 * await-terugkeerwaarden van een geannuleerde sessie worden verworpen en de
 * bijbehorende reader en tracks worden direct gestopt.
 */
export async function startScan(dotnetRef, videoElementId) {
    _dotnetRef = dotnetRef;
    _videoElementId = videoElementId;

    const mySession = ++_sessionId;
    let resultReceived = false;

    if (!window.isSecureContext) {
        if (_sessionId === mySession) {
            await _dotnetRef.invokeMethodAsync('OnScanError', 'INSECURE_CONTEXT');
        }
        return;
    }

    if (!navigator.mediaDevices?.getUserMedia) {
        if (_sessionId === mySession) {
            await _dotnetRef.invokeMethodAsync('OnScanError', 'NO_CAMERA_API');
        }
        return;
    }

    let ZXing;
    try {
        ZXing = await _loadZxing();
    } catch {
        if (_sessionId === mySession) {
            await _dotnetRef.invokeMethodAsync('OnScanError', 'DECODER_LOAD_FAILED');
        }
        return;
    }

    // Controleer sessie na elke async stap.
    if (_sessionId !== mySession) return;

    const videoEl = document.getElementById(videoElementId);
    if (!videoEl) {
        await _dotnetRef.invokeMethodAsync('OnScanError', 'CAMERA_ERROR');
        return;
    }

    // Eerdere reader opruimen.
    if (_reader) {
        try { _reader.reset(); } catch { }
        _reader = null;
    }

    // Maak een nieuwe reader voor deze sessie en sla hem op zodat stopScan
    // hem via _reader.reset() kan bereiken, ook tijdens de getUserMedia-await.
    const localReader = new ZXing.BrowserMultiFormatReader(_buildHints(ZXing));
    _reader = localReader;

    try {
        // decodeFromConstraints: awaits getUserMedia, start daarna de continue decode-loop
        // via setTimeout en retourneert Promise<void> zodra de loop actief is.
        await localReader.decodeFromConstraints(
            { video: { facingMode: { ideal: 'environment' } } },
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

                _dotnetRef?.invokeMethodAsync('OnScanResult', value, format).catch(() => { });
            }
        );

        // decodeFromConstraints is teruggekeerd: camera actief, decode-loop loopt via setTimeout.
        // Als stopScan tussentijds werd aangeroepen, is de reader al gereset maar kan de loop
        // nog kort actief zijn geweest; stop defensief opnieuw.
        if (_sessionId !== mySession) {
            try { localReader.reset(); } catch { }
            _stopVideoTracks();
        }

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
        try { await _dotnetRef?.invokeMethodAsync('OnScanError', code); } catch { }
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
