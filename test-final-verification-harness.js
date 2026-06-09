#!/usr/bin/env node
'use strict';

const vm = require('vm');
const fs = require('fs');
const path = require('path');

console.log('=== Final Verification Harness ===\n');

process.on('unhandledRejection', (reason, promise) => {
    console.error(`FATAL: Unhandled rejection: ${reason}`);
    process.exit(1);
});

const moduleSource = fs.readFileSync(
    path.join(__dirname, 'BootManager.Web/wwwroot/js/barcodeScanner.js'),
    'utf-8'
);

const transformedSource = moduleSource
    .replace(/export\s+async\s+function\s+/g, 'async function ')
    .replace(/export\s+function\s+/g, 'function ')
    .replace(/^'use strict';\n?/m, '');

try {
    new Function(transformedSource);
} catch (e) {
    console.error(`FATAL: Parse error: ${e.message}`);
    process.exit(1);
}

let testsPassed = 0;
let testsFailed = 0;

function assert(condition, message) {
    if (!condition) throw new Error(message);
}

class Deferred {
    constructor() {
        this.promise = new Promise((resolve, reject) => {
            this.resolve = resolve;
            this.reject = reject;
        });
        this.isPending = true;
    }
}

class RAFQueue {
    constructor() {
        this.nextId = 0;
        this.frames = new Map();
        this.toFlush = [];
        this.cancelledIds = new Set();
    }

    request(fn) {
        const id = ++this.nextId;
        this.frames.set(id, { fn, cancelled: false });
        this.toFlush.push(id);
        return id;
    }

    cancel(id) {
        const frame = this.frames.get(id);
        if (frame) {
            frame.cancelled = true;
            this.cancelledIds.add(id);
            this.toFlush = this.toFlush.filter(fid => fid !== id);
        }
    }

    async flushAsync() {
        const toRun = [...this.toFlush];
        this.toFlush = [];

        for (const id of toRun) {
            const frame = this.frames.get(id);
            if (frame && !frame.cancelled) {
                const result = frame.fn();
                if (result instanceof Promise) {
                    await result;
                }
            }
        }
    }

    reset() {
        this.nextId = 0;
        this.frames.clear();
        this.toFlush = [];
        this.cancelledIds.clear();
    }

    countFrames() {
        return this.frames.size;
    }

    isIdCancelled(id) {
        return this.cancelledIds.has(id);
    }
}

const raf = new RAFQueue();

class MockMediaStream {
    constructor() {
        this.tracks = [];
    }

    getTracks() {
        return this.tracks;
    }

    getVideoTracks() {
        return this.tracks.filter(t => t.kind === 'video');
    }
}

async function runScenario(n, title, testFn) {
    try {
        raf.reset();
        await testFn();
        testsPassed++;
        console.log(`✓ Scenario ${n}: ${title}`);
    } catch (e) {
        testsFailed++;
        console.log(`✗ Scenario ${n}: ${e.message}`);
    }
}

function createContext(elementCache) {
    const sandbox = {
        console: console,
        globalThis: null,
        window: {
            isSecureContext: true,
            BarcodeDetector: null,
            ZXing: null
        },
        navigator: {
            mediaDevices: {
                enumerateDevices: async () => [{
                    kind: 'videoinput',
                    deviceId: 'camera-1',
                    label: 'Test Camera'
                }],
                getUserMedia: async (constraints) => {
                    throw new Error('getUserMedia should not be called');
                }
            }
        },
        document: {
            getElementById: (id) => {
                if (!elementCache[id]) {
                    elementCache[id] = { srcObject: null, videoWidth: 1920, videoHeight: 1080 };
                }
                return elementCache[id];
            },
            createElement: (tag) => ({ appendChild: () => { } }),
            head: { appendChild: () => { } }
        },
        requestAnimationFrame: (fn) => raf.request(fn),
        cancelAnimationFrame: (id) => raf.cancel(id),
        clearTimeout: (id) => { },
        setTimeout: (fn, delay) => { fn(); return 1; },
        Promise: Promise,
        Map: Map,
        MediaStream: MockMediaStream,
        Array, Object, Error, String, parseInt,
        Date: { now: () => Date.now() }
    };

    const context = vm.createContext(sandbox);
    context.globalThis = context;

    try {
        vm.runInContext(transformedSource + `
;
globalThis.__scanner = { startScan, stopScan, dispose, checkSecureContext };
`, context, { timeout: 5000 });
    } catch (e) {
        console.error(`FATAL: Execution error: ${e.message}`);
        process.exit(1);
    }

    return context;
}

const elementCache = {};
const testContext = createContext(elementCache);
assert(typeof testContext.__scanner.startScan === 'function', 'startScan');
assert(typeof testContext.__scanner.stopScan === 'function', 'stopScan');
assert(typeof testContext.__scanner.dispose === 'function', 'dispose');
assert(typeof testContext.__scanner.checkSecureContext === 'function', 'checkSecureContext');

console.log(`✓ Module loaded (${moduleSource.length} bytes)\n`);

(async () => {
    // Scenario 1
    await runScenario(1, 'Zonder BarcodeDetector - support/reader/track cleanup', async () => {
        elementCache['video-1'] = { srcObject: null, videoWidth: 1920, videoHeight: 1080 };
        const context = createContext(elementCache);
        const log = [];

        context.window.BarcodeDetector = null;
        context.window.ZXing = {
            BarcodeFormat: { QR_CODE: 1 },
            DecodeHintType: { POSSIBLE_FORMATS: 'f', TRY_HARDER: 't' },
            BrowserMultiFormatReader: class {
                async decodeFromConstraints(constraints, el, callback) {
                    const mockTrack = {
                        stop: () => { log.push('track-stop'); },
                        getSettings: () => ({ deviceId: 'camera-1', width: 1920, height: 1080 }),
                        getCapabilities: () => ({ focusMode: ['continuous'] }),
                        applyConstraints: async () => { },
                        kind: 'video'
                    };

                    const stream = new context.MediaStream();
                    stream.tracks = [mockTrack];
                    el.srcObject = stream;
                }
                reset() { log.push('reader-reset'); }
            }
        };

        const callbacks = {};
        const dotnetRef = {
            invokeMethodAsync: async (method, ...args) => {
                if (method === 'OnDecoderSupport') {
                    callbacks.support = args[args.length - 1];
                }
            }
        };

        const p = context.__scanner.startScan(dotnetRef, 'video-1', null, 1);

        await new Promise(resolve => setImmediate(resolve));
        await new Promise(resolve => setImmediate(resolve));

        assert(callbacks.support, 'support callback');
        assert(callbacks.support.qrAvailable === true, 'QR available');
        assert(callbacks.support.nativeEan13Available === false, 'EAN unsupported');

        const videoEl = elementCache['video-1'];
        assert(videoEl.srcObject, 'stream set');

        context.__scanner.stopScan();

        assert(log.includes('reader-reset'), 'reader reset');
        assert(log.includes('track-stop'), 'track stopped');
        assert(videoEl.srcObject === null, 'srcObject cleared');

        await p;
    });

    // Scenario 2
    await runScenario(2, 'Met native support - detect EAN-13, cleanup', async () => {
        elementCache['video-2'] = { srcObject: null, videoWidth: 1920, videoHeight: 1080 };
        const context = createContext(elementCache);
        const log = [];
        let rafCancelledCount = 0;

        const originalCancel = raf.cancel.bind(raf);
        raf.cancel = function(id) {
            rafCancelledCount++;
            return originalCancel(id);
        };

        context.window.ZXing = {
            BarcodeFormat: { QR_CODE: 1 },
            DecodeHintType: { POSSIBLE_FORMATS: 'f', TRY_HARDER: 't' },
            BrowserMultiFormatReader: class {
                async decodeFromConstraints(constraints, el, callback) {
                    const mockTrack = {
                        stop: () => { log.push('track-stop'); },
                        getSettings: () => ({ deviceId: 'camera-1', width: 1920, height: 1080 }),
                        getCapabilities: () => ({ focusMode: ['continuous'] }),
                        applyConstraints: async () => { },
                        kind: 'video'
                    };

                    const stream = new context.MediaStream();
                    stream.tracks = [mockTrack];
                    el.srcObject = stream;
                }
                reset() { log.push('reader-reset'); }
            }
        };

        context.window.BarcodeDetector = class {
            static async getSupportedFormats() { return ['ean_13']; }
            async detect(el) {
                return [{ rawValue: '9789059965607', format: 'ean_13' }];
            }
        };

        const callbacks = {};
        const dotnetRef = {
            invokeMethodAsync: async (method, ...args) => {
                if (method === 'OnScanResult') {
                    callbacks.result = args[args.length - 2];
                    callbacks.format = args[args.length - 1];
                }
            }
        };

        const p = context.__scanner.startScan(dotnetRef, 'video-2', null, 2);

        await new Promise(resolve => setImmediate(resolve));
        await new Promise(resolve => setImmediate(resolve));

        await raf.flushAsync();
        await new Promise(resolve => setImmediate(resolve));

        assert(callbacks.result === '9789059965607', `result=${callbacks.result}`);
        assert(callbacks.format === 'EAN_13', 'format');
        assert(log.includes('reader-reset'), 'reset');
        assert(log.includes('track-stop'), 'stop');
        assert(rafCancelledCount > 0, 'raf cancelled');

        raf.cancel = originalCancel;
        await p;
    });

    // Scenario 3
    await runScenario(3, 'QR callback voor localStream - defensief', async () => {
        elementCache['video-3'] = { srcObject: null, videoWidth: 1920, videoHeight: 1080 };
        const context = createContext(elementCache);
        const log = [];

        context.window.BarcodeDetector = null;
        context.window.ZXing = {
            BarcodeFormat: { QR_CODE: 1 },
            DecodeHintType: { POSSIBLE_FORMATS: 'f', TRY_HARDER: 't' },
            BrowserMultiFormatReader: class {
                async decodeFromConstraints(constraints, el, callback) {
                    const mockTrack = {
                        stop: () => { log.push('track-stop'); },
                        getSettings: () => ({ deviceId: 'camera-1', width: 1920, height: 1080 }),
                        getCapabilities: () => ({ focusMode: ['continuous'] }),
                        applyConstraints: async () => { },
                        kind: 'video'
                    };

                    const stream = new context.MediaStream();
                    stream.tracks = [mockTrack];
                    el.srcObject = stream;

                    if (callback) {
                        callback({ getText: () => 'QR', getBarcodeFormat: () => 1 }, null);
                    }
                }
                reset() { log.push('reader-reset'); }
            }
        };

        const callbacks = {};
        const dotnetRef = {
            invokeMethodAsync: async (method, ...args) => {
                if (method === 'OnScanResult') {
                    callbacks.result = args[args.length - 2];
                }
            }
        };

        const p = context.__scanner.startScan(dotnetRef, 'video-3', null, 3);

        await new Promise(resolve => setImmediate(resolve));

        assert(callbacks.result === 'QR', 'QR result');
        assert(log.includes('track-stop'), 'track stopped');

        const videoEl = elementCache['video-3'];
        assert(videoEl.srcObject === null, 'srcObject cleared');

        await p;
    });

    // Scenario 4: Session isolatie met native detector - B resources intact
    await runScenario(4, 'Start A deferred, B - B resources intact', async () => {
        elementCache['video-a'] = { srcObject: null, videoWidth: 1920, videoHeight: 1080 };
        elementCache['video-b'] = { srcObject: null, videoWidth: 1920, videoHeight: 1080 };
        const context = createContext(elementCache);
        const logA = [];
        const logB = [];
        const callbacksB = {};
        let decodeDefA = null;
        let detectorBRafId = null;
        let bStreamBefore = null;

        // Counters voor B
        let bReaderResetCountBefore = 0;
        let bTrackStopCountBefore = 0;
        let bSupportCountBefore = 0;
        let bResultCountBefore = 0;

        const createReader = (log, captureDeferred) => class {
            async decodeFromConstraints(constraints, el, callback) {
                const mockTrack = {
                    stop: () => { log.push('track-stop'); },
                    getSettings: () => ({ deviceId: 'camera-1', width: 1920, height: 1080 }),
                    getCapabilities: () => ({ focusMode: ['continuous'] }),
                    applyConstraints: async () => { },
                    kind: 'video'
                };

                const stream = new context.MediaStream();
                stream.tracks = [mockTrack];
                el.srcObject = stream;

                if (captureDeferred) {
                    const def = new Deferred();
                    decodeDefA = def;
                    await def.promise;
                }
            }
            reset() { log.push('reader-reset'); }
        };

        // Session A: deferred decode
        context.window.ZXing = {
            BarcodeFormat: { QR_CODE: 1 },
            DecodeHintType: { POSSIBLE_FORMATS: 'f', TRY_HARDER: 't' },
            BrowserMultiFormatReader: createReader(logA, true)
        };

        context.window.BarcodeDetector = class {
            static async getSupportedFormats() { return ['ean_13']; }
            async detect(el) { return []; }
        };

        const dotnetRefA = { invokeMethodAsync: async () => { } };
        const pA = context.__scanner.startScan(dotnetRefA, 'video-a', null, 10);
        await new Promise(resolve => setImmediate(resolve));

        // Session B: native detector
        context.window.ZXing.BrowserMultiFormatReader = createReader(logB, false);

        const origRequest = raf.request.bind(raf);
        raf.request = function(fn) {
            const id = origRequest(fn);
            detectorBRafId = id;
            return id;
        };

        const dotnetRefB = {
            invokeMethodAsync: async (method, ...args) => {
                if (method === 'OnDecoderSupport') {
                    callbacksB.support = (callbacksB.support || 0) + 1;
                }
                if (method === 'OnScanResult') {
                    callbacksB.result = (callbacksB.result || 0) + 1;
                }
            }
        };

        const pB = context.__scanner.startScan(dotnetRefB, 'video-b', null, 11);
        await new Promise(resolve => setImmediate(resolve));

        raf.request = origRequest;

        // Vóór late reject: snapshot B's state
        assert(detectorBRafId !== null, 'B RAF-ID geregistreerd');
        assert(raf.countFrames() > 0, 'B RAF actief');
        assert(!raf.isIdCancelled(detectorBRafId), 'B RAF nog niet geannuleerd');

        const videoElB = elementCache['video-b'];
        bStreamBefore = videoElB.srcObject;
        bReaderResetCountBefore = logB.filter(l => l === 'reader-reset').length;
        bTrackStopCountBefore = logB.filter(l => l === 'track-stop').length;
        bSupportCountBefore = callbacksB.support || 0;
        bResultCountBefore = callbacksB.result || 0;

        // Reject A's deferred
        if (decodeDefA) {
            decodeDefA.reject(new Error('timeout'));
        }

        await pA;

        // Assert B intact na A's reject
        assert(logB.filter(l => l === 'reader-reset').length === bReaderResetCountBefore, 'B reader-reset unchanged');
        assert(logB.filter(l => l === 'track-stop').length === bTrackStopCountBefore, 'B track-stop unchanged');
        assert(videoElB.srcObject === bStreamBefore, 'B stream exact same');
        assert(!raf.isIdCancelled(detectorBRafId), 'B RAF niet geannuleerd');
        assert((callbacksB.support || 0) === bSupportCountBefore, 'B support count unchanged');
        assert((callbacksB.result || 0) === bResultCountBefore, 'B result count unchanged');

        // Cleanup B
        context.__scanner.stopScan();
        assert(videoElB.srcObject === null, 'B srcObject cleared');

        await pB;
    });

    // Scenario 5: Monotone revisions met final detector_error
    await runScenario(5, 'Support revisions oplopend + detector_error final', async () => {
        elementCache['video-5'] = { srcObject: null, videoWidth: 1920, videoHeight: 1080 };
        const context = createContext(elementCache);
        const revisions = [];
        let detectCount = 0;
        let finalDetectorError = false;

        context.window.ZXing = {
            BarcodeFormat: { QR_CODE: 1 },
            DecodeHintType: { POSSIBLE_FORMATS: 'f', TRY_HARDER: 't' },
            BrowserMultiFormatReader: class {
                async decodeFromConstraints(constraints, el, callback) {
                    const mockTrack = {
                        stop: () => { },
                        getSettings: () => ({ deviceId: 'camera-1', width: 1920, height: 1080 }),
                        getCapabilities: () => ({ focusMode: ['continuous'] }),
                        applyConstraints: async () => { },
                        kind: 'video'
                    };
                    const stream = new context.MediaStream();
                    stream.tracks = [mockTrack];
                    el.srcObject = stream;
                }
                reset() { }
            }
        };

        context.window.BarcodeDetector = class {
            static async getSupportedFormats() { return ['ean_13']; }
            async detect(el) {
                detectCount++;
                if (detectCount <= 2) {
                    return [];
                } else {
                    throw new Error('detector error');
                }
            }
        };

        const dotnetRef = {
            invokeMethodAsync: async (method, ...args) => {
                if (method === 'OnDecoderSupport') {
                    const support = args[args.length - 1];
                    revisions.push(support.supportRevision);
                    if (support.nativeFailureReason === 'detector_error') {
                        finalDetectorError = true;
                    }
                }
            }
        };

        const p = context.__scanner.startScan(dotnetRef, 'video-5', null, 12);

        await new Promise(resolve => setImmediate(resolve));

        // Trigger detector errors
        for (let i = 0; i < 15; i++) {
            await raf.flushAsync();
            await new Promise(resolve => setImmediate(resolve));
        }

        context.__scanner.stopScan();

        // Verify revisions AND final detector_error
        assert(revisions.length >= 3, `revisions: ${revisions.length}`);
        for (let i = 1; i < revisions.length; i++) {
            assert(revisions[i] > revisions[i-1], `${revisions[i]} > ${revisions[i-1]}`);
        }
        assert(finalDetectorError === true, 'detector_error callback received');

        await p;
    });

    // Scenario 6: Native cleanup - stopScan en dispose met pending detect
    await runScenario(6, 'Native cleanup - stopScan en dispose', async () => {
        // Test stopScan with native detector and pending detect
        {
            raf.reset();
            elementCache['video-6a'] = { srcObject: null, videoWidth: 1920, videoHeight: 1080 };
            const context = createContext(elementCache);
            const log = [];
            let rafIdStopScan = null;
            let detectDefStopScan = null;
            let detectWasInvoked = false;
            let supportCallCount = 0;
            let resultCallCount = 0;

            const origRequest = raf.request.bind(raf);
            raf.request = function(fn) {
                const id = origRequest(fn);
                rafIdStopScan = id;
                return id;
            };

            context.window.ZXing = {
                BarcodeFormat: { QR_CODE: 1 },
                DecodeHintType: { POSSIBLE_FORMATS: 'f', TRY_HARDER: 't' },
                BrowserMultiFormatReader: class {
                    async decodeFromConstraints(constraints, el, callback) {
                        const mockTrack = {
                            stop: () => { log.push('track-stop'); },
                            getSettings: () => ({ deviceId: 'camera-1', width: 1920, height: 1080 }),
                            getCapabilities: () => ({ focusMode: ['continuous'] }),
                            applyConstraints: async () => { },
                            kind: 'video'
                        };
                        const stream = new context.MediaStream();
                        stream.tracks = [mockTrack];
                        el.srcObject = stream;
                    }
                    reset() { log.push('reader-reset'); }
                }
            };

            context.window.BarcodeDetector = class {
                static async getSupportedFormats() { return ['ean_13']; }
                async detect(el) {
                    detectWasInvoked = true;
                    const def = new Deferred();
                    detectDefStopScan = def;
                    await def.promise;
                    return [];
                }
            };

            const dotnetRef = {
                invokeMethodAsync: async (method, ...args) => {
                    if (method === 'OnDecoderSupport') supportCallCount++;
                    if (method === 'OnScanResult') resultCallCount++;
                }
            };
            const p = context.__scanner.startScan(dotnetRef, 'video-6a', null, 20);

            await new Promise(resolve => setImmediate(resolve));

            // Flush native RAF to invoke detect
            await raf.flushAsync();
            await new Promise(resolve => setImmediate(resolve));

            // Verify detect was invoked and deferred is pending
            assert(detectWasInvoked, 'stopScan: detect invoked');
            assert(detectDefStopScan !== null, 'stopScan: detect deferred created');
            assert(detectDefStopScan.isPending, 'stopScan: detect deferred pending before cleanup');
            assert(rafIdStopScan !== null, 'stopScan: native RAF registered');

            const videoEl = elementCache['video-6a'];
            assert(videoEl.srcObject !== null, 'stopScan: srcObject active before cleanup');
            assert(!raf.isIdCancelled(rafIdStopScan), 'stopScan: RAF not cancelled before cleanup');

            // === Checkpoint 1: Before cleanup ===
            const c1_readerReset = log.filter(l => l === 'reader-reset').length;
            const c1_trackStop = log.filter(l => l === 'track-stop').length;
            const c1_supportCall = supportCallCount;
            const c1_resultCall = resultCallCount;
            const c1_rafCount = raf.countFrames();

            // === Call stopScan (first cleanup) ===
            context.__scanner.stopScan();

            // === Checkpoint 2: After first cleanup ===
            const c2_readerReset = log.filter(l => l === 'reader-reset').length;
            const c2_trackStop = log.filter(l => l === 'track-stop').length;
            const c2_supportCall = supportCallCount;
            const c2_resultCall = resultCallCount;
            const c2_rafCount = raf.countFrames();

            assert(c2_readerReset === c1_readerReset + 1, 'stopScan: reader-reset called once');
            assert(c2_trackStop === c1_trackStop + 1, 'stopScan: track-stop called once');
            assert(c2_supportCall === c1_supportCall, 'stopScan: no new support callback');
            assert(c2_resultCall === c1_resultCall, 'stopScan: no new result callback');
            assert(raf.isIdCancelled(rafIdStopScan), 'stopScan: RAF cancelled after cleanup');
            assert(videoEl.srcObject === null, 'stopScan: srcObject cleared after cleanup');

            // === Call stopScan again (idempotence) ===
            context.__scanner.stopScan();

            // === Checkpoint 3: After second cleanup ===
            const c3_readerReset = log.filter(l => l === 'reader-reset').length;
            const c3_trackStop = log.filter(l => l === 'track-stop').length;
            const c3_supportCall = supportCallCount;
            const c3_resultCall = resultCallCount;
            const c3_rafCount = raf.countFrames();

            assert(c3_readerReset === c2_readerReset, 'stopScan: idempotent reader-reset');
            assert(c3_trackStop === c2_trackStop, 'stopScan: idempotent track-stop');
            assert(c3_supportCall === c2_supportCall, 'stopScan: idempotent support callback');
            assert(c3_resultCall === c2_resultCall, 'stopScan: idempotent result callback');
            assert(c3_rafCount === c2_rafCount, 'stopScan: idempotent RAF count');
            assert(raf.isIdCancelled(rafIdStopScan), 'stopScan: RAF still cancelled after second cleanup');
            assert(videoEl.srcObject === null, 'stopScan: srcObject still null after second cleanup');

            // === Checkpoint 4: Before late detect resolution ===
            assert(detectDefStopScan.isPending, 'stopScan: detect still pending before resolution');

            // Resolve pending detect with empty array
            detectDefStopScan.resolve([]);
            detectDefStopScan.isPending = false;

            // Wait for detect to complete
            await new Promise(resolve => setImmediate(resolve));

            // === Checkpoint 5: After late detect resolution ===
            const c5_readerReset = log.filter(l => l === 'reader-reset').length;
            const c5_trackStop = log.filter(l => l === 'track-stop').length;
            const c5_supportCall = supportCallCount;
            const c5_resultCall = resultCallCount;
            const c5_rafCount = raf.countFrames();

            assert(c5_readerReset === c2_readerReset, 'stopScan: no new reader-reset after late detect');
            assert(c5_trackStop === c2_trackStop, 'stopScan: no new track-stop after late detect');
            assert(c5_supportCall === c2_supportCall, 'stopScan: no new support callback after late detect');
            assert(c5_resultCall === c2_resultCall, 'stopScan: no new result callback after late detect');
            assert(c5_rafCount === c2_rafCount, 'stopScan: no new RAF after late detect');
            assert(raf.isIdCancelled(rafIdStopScan), 'stopScan: RAF still cancelled after late detect');
            assert(videoEl.srcObject === null, 'stopScan: srcObject still null after late detect');

            raf.request = origRequest;
            await p;
        }

        // Test dispose with native detector and pending detect
        {
            raf.reset();
            elementCache['video-6b'] = { srcObject: null, videoWidth: 1920, videoHeight: 1080 };
            const context = createContext(elementCache);
            const log = [];
            let rafIdDispose = null;
            let detectDefDispose = null;
            let detectWasInvoked = false;
            let supportCallCount = 0;
            let resultCallCount = 0;

            const origRequest = raf.request.bind(raf);
            raf.request = function(fn) {
                const id = origRequest(fn);
                rafIdDispose = id;
                return id;
            };

            context.window.ZXing = {
                BarcodeFormat: { QR_CODE: 1 },
                DecodeHintType: { POSSIBLE_FORMATS: 'f', TRY_HARDER: 't' },
                BrowserMultiFormatReader: class {
                    async decodeFromConstraints(constraints, el, callback) {
                        const mockTrack = {
                            stop: () => { log.push('track-stop'); },
                            getSettings: () => ({ deviceId: 'camera-1', width: 1920, height: 1080 }),
                            getCapabilities: () => ({ focusMode: ['continuous'] }),
                            applyConstraints: async () => { },
                            kind: 'video'
                        };
                        const stream = new context.MediaStream();
                        stream.tracks = [mockTrack];
                        el.srcObject = stream;
                    }
                    reset() { log.push('reader-reset'); }
                }
            };

            context.window.BarcodeDetector = class {
                static async getSupportedFormats() { return ['ean_13']; }
                async detect(el) {
                    detectWasInvoked = true;
                    const def = new Deferred();
                    detectDefDispose = def;
                    await def.promise;
                    return [];
                }
            };

            const dotnetRef = {
                invokeMethodAsync: async (method, ...args) => {
                    if (method === 'OnDecoderSupport') supportCallCount++;
                    if (method === 'OnScanResult') resultCallCount++;
                }
            };
            const p = context.__scanner.startScan(dotnetRef, 'video-6b', null, 21);

            await new Promise(resolve => setImmediate(resolve));

            // Flush native RAF to invoke detect
            await raf.flushAsync();
            await new Promise(resolve => setImmediate(resolve));

            // Verify detect was invoked and pending
            assert(detectWasInvoked, 'dispose: detect invoked');
            assert(detectDefDispose !== null, 'dispose: detect deferred created');
            assert(detectDefDispose.isPending, 'dispose: detect deferred pending before cleanup');
            assert(rafIdDispose !== null, 'dispose: native RAF registered');

            const videoEl = elementCache['video-6b'];
            assert(videoEl.srcObject !== null, 'dispose: srcObject active before cleanup');
            assert(!raf.isIdCancelled(rafIdDispose), 'dispose: RAF not cancelled before cleanup');

            // === Checkpoint 1: Before cleanup ===
            const c1_readerReset = log.filter(l => l === 'reader-reset').length;
            const c1_trackStop = log.filter(l => l === 'track-stop').length;
            const c1_supportCall = supportCallCount;
            const c1_resultCall = resultCallCount;
            const c1_rafCount = raf.countFrames();

            // === Call dispose (first cleanup) ===
            context.__scanner.dispose();

            // === Checkpoint 2: After first cleanup ===
            const c2_readerReset = log.filter(l => l === 'reader-reset').length;
            const c2_trackStop = log.filter(l => l === 'track-stop').length;
            const c2_supportCall = supportCallCount;
            const c2_resultCall = resultCallCount;
            const c2_rafCount = raf.countFrames();

            assert(c2_readerReset === c1_readerReset + 1, 'dispose: reader-reset called once');
            assert(c2_trackStop === c1_trackStop + 1, 'dispose: track-stop called once');
            assert(c2_supportCall === c1_supportCall, 'dispose: no new support callback');
            assert(c2_resultCall === c1_resultCall, 'dispose: no new result callback');
            assert(raf.isIdCancelled(rafIdDispose), 'dispose: RAF cancelled after cleanup');
            assert(videoEl.srcObject === null, 'dispose: srcObject cleared after cleanup');

            // === Call dispose again (idempotence) ===
            context.__scanner.dispose();

            // === Checkpoint 3: After second cleanup ===
            const c3_readerReset = log.filter(l => l === 'reader-reset').length;
            const c3_trackStop = log.filter(l => l === 'track-stop').length;
            const c3_supportCall = supportCallCount;
            const c3_resultCall = resultCallCount;
            const c3_rafCount = raf.countFrames();

            assert(c3_readerReset === c2_readerReset, 'dispose: idempotent reader-reset');
            assert(c3_trackStop === c2_trackStop, 'dispose: idempotent track-stop');
            assert(c3_supportCall === c2_supportCall, 'dispose: idempotent support callback');
            assert(c3_resultCall === c2_resultCall, 'dispose: idempotent result callback');
            assert(c3_rafCount === c2_rafCount, 'dispose: idempotent RAF count');
            assert(raf.isIdCancelled(rafIdDispose), 'dispose: RAF still cancelled after second cleanup');
            assert(videoEl.srcObject === null, 'dispose: srcObject still null after second cleanup');

            // === Checkpoint 4: Before late detect resolution ===
            assert(detectDefDispose.isPending, 'dispose: detect still pending before resolution');

            // Resolve pending detect
            detectDefDispose.resolve([]);
            detectDefDispose.isPending = false;

            // Wait for detect to complete
            await new Promise(resolve => setImmediate(resolve));

            // === Checkpoint 5: After late detect resolution ===
            const c5_readerReset = log.filter(l => l === 'reader-reset').length;
            const c5_trackStop = log.filter(l => l === 'track-stop').length;
            const c5_supportCall = supportCallCount;
            const c5_resultCall = resultCallCount;
            const c5_rafCount = raf.countFrames();

            assert(c5_readerReset === c2_readerReset, 'dispose: no new reader-reset after late detect');
            assert(c5_trackStop === c2_trackStop, 'dispose: no new track-stop after late detect');
            assert(c5_supportCall === c2_supportCall, 'dispose: no new support callback after late detect');
            assert(c5_resultCall === c2_resultCall, 'dispose: no new result callback after late detect');
            assert(c5_rafCount === c2_rafCount, 'dispose: no new RAF after late detect');
            assert(raf.isIdCancelled(rafIdDispose), 'dispose: RAF still cancelled after late detect');
            assert(videoEl.srcObject === null, 'dispose: srcObject still null after late detect');

            raf.request = origRequest;
            await p;
        }
    });

})().then(() => {
    console.log(`\n=== Results ===`);
    console.log(`Passed: ${testsPassed}`);
    console.log(`Failed: ${testsFailed}`);
    console.log(`\nAll 6 scenarios: ${testsFailed === 0 ? '✓ PASS' : '✗ FAIL'}`);

    process.exit(testsFailed > 0 ? 1 : 0);
}).catch(e => {
    console.error(`FATAL: ${e.message}`);
    process.exit(1);
});
