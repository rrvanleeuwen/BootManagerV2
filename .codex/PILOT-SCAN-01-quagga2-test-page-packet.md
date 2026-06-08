# Implementation Packet

## Task

- Story ID: `PILOT-SCAN-01` Quagga2 EAN-13 proof-of-concept
- Approved story: Camera-, QR- en barcode-proof-of-concept
- Story source: `.docs/releases/holiday-pilot-2026.md`, section `PILOT-SCAN-01`
- Research source: `.docs/extraInfo/barcode-scanner-findings.md`
- Branch: `feature/pilot-scan-01`
- Goal: add an isolated authenticated Quagga2 test page that proves reliable EAN-13 scanning on the Samsung phone without changing the existing ZXing scan page or designing the final combined scanner.

The story and this small experimental step are approved. Do not restate them or ask for approval. Give a short plan, implement directly, run the checks and return the requested completion notes.

## Observed Behavior

- The existing `/scan` page and ZXing implementation remain useful for QR scanning, camera selection, diagnostics and lifecycle behavior.
- ZXing does not read the tested EAN-13 product barcodes reliably on the Samsung phone.
- The official Quagga2 live demo read both tested EAN-13 values on that same phone:
  - `4007817310809`
  - `3662168005289`
- Working demo settings were EAN reader, 800-pixel width, `patchSize: "large"`, `halfSample: false`, locator enabled, rear camera and torch disabled.

## Scope

- Add a separate authenticated Blazor test page at `/scan-quagga-test`.
- Do not add the experimental page to the main navigation.
- Add a separate JavaScript module for Quagga2; do not add Quagga2 branches to `barcodeScanner.js`.
- Vendor the browser bundle for `@ericblade/quagga2` version `1.12.1` locally under `BootManager.Web/wwwroot/lib/quagga2/`.
- Include the package's MIT license and a short notice containing package name, pinned version and upstream URL.
- Load Quagga2 locally at runtime; do not use a CDN.
- Provide explicit Start, Stop and Restart behavior.
- Use one Quagga-managed live camera stream with:
  - `type: "LiveStream"`;
  - a page-owned target element;
  - `facingMode: "environment"`;
  - width configured according to the proven 800-pixel demo setting;
  - `locator.patchSize: "large"`;
  - `locator.halfSample: false`;
  - `decoder.readers: ["ean_reader"]`;
  - `locate: true`;
  - torch left disabled.
- Verify the exact Quagga2 1.12.1 configuration syntax against its official API while preserving those functional settings.
- Return only the decoded EAN-13 value and uniform format name `EAN_13` through JS interop.
- Validate an EAN-13 result in JavaScript before accepting it:
  - exactly 13 decimal digits;
  - valid EAN-13 check digit.
- Stop Quagga2 immediately after the first valid accepted result to suppress duplicate callbacks.
- Show the accepted value and format on the page.
- Also show a compact test log with:
  - each raw Quagga2 detection;
  - whether it was accepted or rejected;
  - rejection reason for invalid length, non-digits or invalid check digit;
  - cumulative accepted count per value during the component lifetime.
- Keep the two known test values visibly listed with their accepted counts, so manual testing can prove ten correct reads of each.
- Show clear Dutch states and errors for insecure context, permission denial, missing camera/API, decoder load/init failure, active scanning, stopped and recognized.
- Release Quagga handlers, processing, camera tracks and JS/.NET references on Stop, Restart, successful detection, navigation and component disposal.
- Protect callbacks against stale sessions during rapid stop/restart or disposal.
- Extend `.docs/scan-handmatige-test.md` with a clearly marked Quagga2 experiment section and the exact twenty-scan procedure.

## Outside Scope

- Do not modify `BootManager.Web/Components/Pages/Scan.razor`, its CSS or `BootManager.Web/wwwroot/js/barcodeScanner.js`.
- Do not replace ZXing or create a shared/general scanner abstraction yet.
- No QR, EAN-8, UPC-A, Code 128 or additional Quagga2 readers on this page.
- No camera selector, camera diagnostics, autofocus changes, torch control or zoom control.
- No navigation entry for the experimental route.
- No product, location, inventory, persistence, external product lookup or routing behavior.
- No changes to authentication, ingest, NMEA, Docker, HTTPS or certificate configuration.
- No SignalR transport of video frames and no .NET-side image processing.
- Do not update release status, TODO, legacy coverage, handoff, commit, push or create a PR; Codex handles project administration and git flow after review.

## Expected Write-Set

Only change or add:

- `BootManager.Web/Components/Pages/QuaggaScanTest.razor`
- `BootManager.Web/Components/Pages/QuaggaScanTest.razor.css`
- `BootManager.Web/wwwroot/js/quaggaScannerTest.js`
- `BootManager.Web/wwwroot/lib/quagga2/quagga.min.js`
- `BootManager.Web/wwwroot/lib/quagga2/LICENSE.txt`
- `BootManager.Web/wwwroot/lib/quagga2/NOTICE.txt`
- `.docs/scan-handmatige-test.md`

Before changing another file, stop and report why it is required.

## Minimal Context

Read only:

- `CLAUDE.md`
- this packet
- `.docs/releases/holiday-pilot-2026.md`, section `PILOT-SCAN-01`
- `.docs/extraInfo/barcode-scanner-findings.md`
- `BootManager.Web/Components/Pages/Scan.razor` for the established Blazor JS-interop and disposal pattern
- `BootManager.Web/Components/Pages/Scan.razor.css` for local visual conventions
- `BootManager.Web/wwwroot/js/barcodeScanner.js` for the established stale-session and resource-release pattern
- `.docs/scan-handmatige-test.md`

Do not load unrelated source trees, full TODO, legacy analysis, handoff or other epics.

## Implementation Constraints

- Target framework remains .NET 8 with Interactive Server components.
- The experiment must work offline after deployment.
- Pin Quagga2 to version `1.12.1`; do not use an unversioned or latest-at-runtime dependency.
- Obtain the browser bundle and license from the official `@ericblade/quagga2` npm package. Do not minify or reconstruct third-party source manually.
- Quagga2's official lifecycle is `init`, handler registration, `start`, handler removal and `stop`; pair every registration with cleanup.
- Keep one module-level active session only. A new start must fully stop the previous one first.
- A stale or cancelled session must not update Blazor state.
- Feature-detect secure context and required media APIs before initialization.
- Treat frame-level absence of a barcode as normal, not as a user-visible error.
- Do not accept Quagga2 output merely because `onDetected` fired; apply the EAN-13 syntax and check-digit validation first.
- Invalid detections may be logged to the bounded UI test log but must not stop scanning or increment accepted counts.
- Bound the visible/raw detection log to the latest 50 entries to prevent unbounded component state.
- The page may use simple presentation-only DTOs; do not create domain or application services.
- New or changed C# members receive concise Dutch XML documentation where relevant.
- Keep user-facing text in Dutch.

## Acceptance Focus

- `/scan-quagga-test` opens only for an authenticated user.
- The existing `/scan` page and ZXing files are unchanged.
- Start explicitly requests camera permission and opens the rear camera using the proven Quagga2 settings.
- `4007817310809` can be accepted ten times, with a stop/restart between reads.
- `3662168005289` can be accepted ten times, with a stop/restart between reads.
- Accepted counts are exactly ten for both known values after the manual run.
- No different value is accepted during the twenty-scan procedure.
- Non-EAN-13 and invalid-check-digit detections are rejected and visible in the bounded test log.
- A valid result stops camera and decoding before notifying Blazor.
- Stop, Restart, navigation and disposal release the camera and registered Quagga2 handlers.
- HTTP shows the secure-context explanation without attempting camera access.

## Required Checks

Run:

```powershell
node --check BootManager.Web/wwwroot/js/quaggaScannerTest.js
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj
dotnet build BootManager.sln
git diff --check
git status --short
```

Also verify from the diff that:

- no existing `/scan` implementation file changed;
- no CDN URL is used at runtime;
- the vendored Quagga2 version and MIT license are documented.

The known unrelated failure in `OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect` may be reported but must not be changed.

Do not claim EAN-13 acceptance from build or automated tests. It requires the documented manual test on the Samsung phone over HTTPS.

## Completion Notes

Return only:

1. changed files and implemented Quagga2 test behavior;
2. exact Quagga2 version, local asset source and configuration;
3. EAN-13 validation and lifecycle safeguards;
4. checks and results;
5. remaining exact Pi/phone manual test steps.
