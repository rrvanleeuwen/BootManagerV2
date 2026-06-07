# Implementation Packet

## Task

- Story ID: `PILOT-SCAN-01`
- Approved story: Camera-, QR- en barcode-proof-of-concept
- Story source: `.docs/releases/holiday-pilot-2026.md`, section `PILOT-SCAN-01`
- Goal: add an isolated mobile scan proof-of-concept that proves camera access, QR/barcode decoding, lifecycle control and manual fallback without introducing inventory domain behavior.

The story is already approved. Do not restate it or ask for approval. Give a short plan, implement directly, run the checks, and provide completion notes.

## Scope

- Add an authenticated `/scan` page to the existing Interactive Server Blazor app.
- Add a navigation entry for the scan page.
- Prefer the rear camera and support explicit start, stop and restart.
- Decode only QR Code, EAN-13, EAN-8, UPC-A and Code 128.
- Show the raw decoded value and detected format.
- Stop continuous detection after a successful result and suppress duplicate callbacks.
- Show clear Dutch status and error messages, including an explicit insecure-context/HTTPS message.
- Provide manual code entry through the same result display.
- Stop media tracks and dispose JavaScript/Blazor references when scanning stops or the page is disposed.
- Add focused automated tests only where behavior can be tested without a real browser camera.
- Document runtime requirements and the manual Android Edge/Chrome acceptance procedure.

## Outside Scope

- Product, location, inventory or database models and persistence.
- QR generation, external product lookup or interpretation of scanned values.
- Automatic navigation or actions based on a scanned value.
- Final visual design or general UI modernization.
- Replacing the existing `http://bootmanager-pi:5000/` route.
- Selecting or installing a production reverse proxy or local certificate authority unless Codex supplies a separately approved operations packet.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is discovered:

- `BootManager.Web/Components/Pages/Scan.razor`
- `BootManager.Web/Components/Pages/Scan.razor.css`
- `BootManager.Web/Components/Layout/NavMenu.razor`
- `BootManager.Web/wwwroot/js/barcodeScanner.js`
- local static decoder assets under `BootManager.Web/wwwroot/lib/`
- focused test files under `BootManager.UnitTests/` only if testable .NET behavior is extracted
- one focused operational/manual-test document under `.docs/`

Do not modify domain, application, infrastructure, persistence, migrations, authentication, ingest or NMEA modules. Before changing an additional area, explain why it is required.

## Minimal Context

Read:

- `CLAUDE.md`
- this packet
- `.docs/releases/holiday-pilot-2026.md`, section `PILOT-SCAN-01` only
- `BootManager.Web/BootManager.Web.csproj`
- `BootManager.Web/Components/App.razor`
- `BootManager.Web/Components/Layout/NavMenu.razor`
- `BootManager.Web/Components/Pages/Login.razor` for the existing JavaScript module lifecycle pattern
- `BootManager.Web/wwwroot/app.css` only if shared styling is demonstrably needed

Do not load by default:

- full `.docs/TODO.md`;
- unrelated epic documents;
- `.docs/legacy-analysis/`;
- `.docs/legacy-input/`;
- `.codex/current-session-handoff.md`;
- repository-wide source trees.

## Existing Constraints

- Target framework is .NET 8 with Interactive Server components.
- Existing JavaScript modules are imported through `IJSRuntime` during `OnAfterRenderAsync` and disposed with the component.
- The solution must work offline on the boat; decoder code must be committed locally and must not use a CDN at runtime.
- Camera access requires a secure browser context. `http://bootmanager-pi:5000/` is not sufficient for Android camera access and must produce a clear page-level explanation.
- The intended devices are Samsung Android phones, oldest Android 16, using Microsoft Edge and Google Chrome.
- Prefer `facingMode: environment`; do not require a hard-coded device ID.
- Do not rely solely on the native `BarcodeDetector` API.
- Keep internal container/service HTTP communication and port `5000` unchanged.

## Acceptance Focus

- Page renders and manual fallback remains usable when camera APIs are unavailable.
- Start requests permission only from an explicit user action.
- Successful detection returns one result with value and format and stops scanning.
- Stop/restart is repeatable and releases the camera.
- Permission denial, no camera, insecure context and general failures have distinct understandable messages.
- Component disposal releases media tracks without surfacing disconnect/disposal exceptions to the user.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj
```

Then:

```powershell
dotnet build BootManager.sln
git diff --check
```

Do not claim camera or barcode acceptance from automated tests. That requires the documented manual test on both Samsung phones in Edge and Chrome over the approved HTTPS route.

## Completion Notes

Return only:

1. changed files and implemented behavior;
2. tests/checks and results;
3. package, static asset, migration and configuration impact;
4. remaining risks and exact manual phone test requirements.
