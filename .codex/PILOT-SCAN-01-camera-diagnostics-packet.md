# Implementation Packet

## Task

- Story ID: `PILOT-SCAN-01` camera diagnostics correction
- Approved story: Camera-, QR- en barcode-proof-of-concept
- Story source: `.docs/releases/holiday-pilot-2026.md`, section `PILOT-SCAN-01`
- Branch: `feature/pilot-scan-01`
- Goal: diagnose and correct unusable close-focus behavior on Samsung Android by exposing available cameras, allowing explicit camera selection and applying continuous autofocus only when supported.

The story and this corrective direction are approved. Do not restate them or ask for approval. Implement directly, run the required checks and return the requested completion notes.

## Observed Pi Behavior

- HTTPS, camera permission and QR scanning work on Samsung Android in Chrome and Edge.
- Multiple real product barcodes are not recognized.
- The live preview remains visibly out of focus.
- `TRY_HARDER` and ideal 1920x1080 constraints did not resolve the issue.
- The likely problem is camera/lens selection or focus capability, not basic decoder availability.

## Scope

- After camera permission has been granted, enumerate available `videoinput` devices with `navigator.mediaDevices.enumerateDevices()`.
- Return camera ID and browser-provided label to Blazor without inventing Samsung-specific names or ordering assumptions.
- Show a compact camera selector when more than one video input is available.
- Preserve the current selected camera across scan restarts during the component lifetime.
- Initial scan without a selected device continues to prefer `facingMode: environment`.
- A user-selected camera starts with its exact `deviceId` plus the existing ideal 1920x1080 resolution constraints.
- After the stream starts, inspect the active video track through `video.srcObject`.
- Read and return useful diagnostics from `getSettings()` and, when available, `getCapabilities()`:
  - active device ID;
  - active camera label when it can be matched;
  - actual width and height;
  - active facing mode;
  - supported focus modes;
  - whether continuous autofocus was successfully applied.
- If `getCapabilities().focusMode` includes `continuous`, attempt `track.applyConstraints()` with continuous focus.
- Unsupported focus capabilities or rejected focus constraints must not fail the scanner.
- Display the active camera, actual resolution and autofocus result in a compact diagnostics block.
- Changing the selected camera must stop the current reader/stream through the existing race-safe lifecycle and restart scanning with the selected device.
- Preserve supported formats, `TRY_HARDER`, result handling, secure-context handling, manual fallback, session ID protection and `reader.reset()` lifecycle.
- Update the manual test document with exact steps for comparing available cameras and recording which camera focuses/scans correctly on each phone/browser.

## Outside Scope

- No Samsung model detection, user-agent branching or hard-coded camera labels.
- No assumption that the first, last, wide, telephoto or ultrawide camera is correct.
- No native Android application or browser-specific API.
- No torch, zoom slider, tap-to-focus or image upload in this correction.
- No database persistence or long-term camera preference.
- No product, inventory or location behavior.
- No changes to authentication, Docker, Caddy, forwarded headers or other deployment configuration.
- No package or decoder upgrade.

## Expected Write-Set

Only change:

- `BootManager.Web/Components/Pages/Scan.razor`
- `BootManager.Web/Components/Pages/Scan.razor.css`
- `BootManager.Web/wwwroot/js/barcodeScanner.js`
- `.docs/scan-handmatige-test.md`

Before changing any other file, stop and report why it is required.

## Minimal Context

Read only:

- `CLAUDE.md`
- this packet
- `.docs/releases/holiday-pilot-2026.md`, section `PILOT-SCAN-01`
- `BootManager.Web/Components/Pages/Scan.razor`
- `BootManager.Web/Components/Pages/Scan.razor.css`
- `BootManager.Web/wwwroot/js/barcodeScanner.js`
- `.docs/scan-handmatige-test.md`

Do not load unrelated source trees, TODO, legacy analysis, handoff or other epics.

## Implementation Constraints

- Use browser-provided device labels only. Labels may be empty until permission is granted.
- Keep camera diagnostics as presentation-only data; do not create domain/application services.
- Use simple JS-interop DTOs with nullable properties so missing browser capability fields are expected.
- Keep the camera list stable enough for selection by `deviceId`; do not key by label.
- Do not call `getUserMedia()` separately just to enumerate devices. Enumerate after ZXing has started the permitted stream.
- Obtain the active track from the video element's `srcObject` after `decodeFromConstraints()` returns.
- Apply continuous focus defensively:
  - check `track.getCapabilities` and `track.applyConstraints`;
  - verify that `focusMode` is an array containing `continuous`;
  - catch rejection and continue scanning;
  - re-read `getSettings()` after applying constraints when useful.
- A stale or cancelled session must not update camera options, diagnostics or Blazor state.
- Camera-change restart must not allow callbacks from the previous session to overwrite the new session.
- Do not log device IDs or capability dumps to server logs.

## Acceptance Focus

- Initial start still works on devices with one camera or no capability APIs.
- Multiple cameras become selectable after permission is granted.
- Selecting a different camera releases the previous camera and starts the selected camera.
- The displayed active resolution reflects actual track settings, not requested ideals.
- Autofocus reports one of: applied, unsupported or failed without breaking scanning.
- QR scanning remains functional.
- Existing stop, restart, manual input and navigation disposal remain functional.
- The phone test can determine whether one available camera produces a sharp preview and recognizes EAN-13.

## Required Checks

```powershell
node --check BootManager.Web/wwwroot/js/barcodeScanner.js
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj
dotnet build BootManager.sln
git diff --check
```

The known unrelated failure in `OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect` may be reported but must not be changed in this correction.

## Completion Notes

Return only:

1. changed files and camera selection/diagnostics behavior;
2. exact focus capability and constraint behavior implemented;
3. checks and results;
4. remaining manual Pi/phone tests.
