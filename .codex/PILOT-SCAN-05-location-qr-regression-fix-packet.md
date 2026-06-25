# Implementation Packet

## Task

- Story ID: `PILOT-SCAN-05`
- Approved story: Onbekende-code-flow binnen nieuwe scanervaring
- Story source: `.docs/releases/holiday-pilot-2026.md`
- Goal: herstel de regressie waarbij de locatie-QR-scan in de nieuwe
  product-inruimflow niet werkt na productaanmaak vanuit een onbekende code, terwijl
  dezelfde QR in het scanstartscherm wel correct resolved wordt
- Required branch: `codex/scan-location-qr-regression-fix`

The story is already approved. Do not restate it or ask for approval. Give a short
plan, implement directly, run the checks, and provide completion notes.

Codex must create and verify the required feature branch before giving this packet to
Claude. Claude must stop and report `not ready` when the active branch is `master` or
does not match the required branch.

## Scope

- Herstel de camera-gebaseerde locatie-scan in de add-stock-flow voor
  `ScanProductAddStock` zodat een gescande BootManager locatie-QR via dezelfde
  resolve-logica wordt verwerkt als in `/scan`.
- Maak het callback-contract tussen `barcodeScanner.js` en
  `ScanProductAddStock.razor` consistent zodat scanresultaten en scanfouten de
  component daadwerkelijk bereiken.
- Behoud bestaand gedrag voor:
  - handmatige locatiecode-invoer in `ScanProductAddStock`;
  - bekende productrouting vanuit `/scan`;
  - bekende locatierouting vanuit `/scan`;
  - productbarcode-scannen in `ScanLocationAddProduct`.
- Voeg gerichte regressietests toe die aantonen dat:
  - de defectroute defectgevoelig was;
  - BootManager locatie-QR's in de add-stock-flow via resolve-logica geselecteerd
    worden;
  - het gedeelde scanner-callbackcontract niet opnieuw ongemerkt kan breken.

## Outside Scope

- Nieuwe storyscope buiten `PILOT-SCAN-05`.
- UI-herontwerp van scanpagina's buiten wat strikt nodig is voor deze bugfix.
- Wijzigingen aan release-, TODO-, README-, handoff- of legacy-documentatie.
- Deployments, Raspberry Pi-runbookwijzigingen of productieconfiguratie.
- Brede herbouw van `barcodeScanner.js` buiten de minimaal noodzakelijke contractfix.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

- `BootManager.Web\Components\Pages\ScanProductAddStock.razor`
- `BootManager.Web\wwwroot\js\barcodeScanner.js`
- targeted tests under `BootManager.UnitTests\Storage\ScanProductAddStockComponentTests.cs`
- only an additional tightly related test file under `BootManager.UnitTests\Storage\`
  if the existing test file cannot express the callback-contract regression cleanly

Before changing an additional area, explain why it is required.

## Execution Boundaries

- Implement only application code and tests explicitly required by this packet.
- Before editing, verify that the active branch matches `Required branch` and is not
  `master`.
- Do not change story, release, TODO, legacy, README, handoff or other project
  documentation.
- Do not create commits, pushes, branches, PRs, merges, releases or deployments.
- Do not broaden this into a generic scanner refactor when a local contract fix is
  sufficient.
- Preserve existing scan behavior in pages that already expect `OnScanResult` /
  `OnScanError`; if you must touch shared scanner callbacks, prove those flows still
  work.
- If the bug cannot be fixed without a larger scanner API redesign, stop and report
  `not ready` with the smallest missing design decision.

## Minimal Context

Read:

- `.codex\PILOT-SCAN-05-location-qr-regression-fix-packet.md`
- the `PILOT-SCAN-05` acceptance section in `.docs/releases/holiday-pilot-2026.md`
- `BootManager.Web\Components\Pages\Scan.razor`
- `BootManager.Web\Components\Pages\ScanProductAddStock.razor`
- `BootManager.Web\Components\Pages\ScanLocationAddProduct.razor`
- `BootManager.Web\Components\Pages\ScanUnknownCodeCreateProduct.razor`
- `BootManager.Web\wwwroot\js\barcodeScanner.js`
- `BootManager.Application\Storage\QrFormat\LocationQrValue.cs`
- `BootManager.Application\Storage\Services\IStorageService.cs`
- `BootManager.UnitTests\Storage\ScanProductAddStockComponentTests.cs`
- only directly relevant existing scan tests if needed for preserving behavior

Do not load by default:

- full `.docs/TODO.md`;
- unrelated epic documents;
- `.docs/legacy-analysis/`;
- `.docs/legacy-input/`;
- `.codex/current-session-handoff.md`;
- repository-wide source trees.

## Existing Constraints

- BootManager locatie-QR's bevatten een tokenwaarde in formaat
  `bootmanager:location:<32-lowercase-hex-token>` en zijn dus niet gelijk aan een
  `LocationId`-GUID.
- `/scan` is de referentie-implementatie voor correcte locatieroutering van een
  gescande QR via `ResolveQrValueAsync`.
- `ScanProductAddStock` bevat zowel handmatige invoer als camera-scan; de handmatige
  route resolved locatie-QR's al correct en mag niet regresseren.
- De gedeelde scanner wordt ook gebruikt door andere scanpagina's; een fix in het
  callbackcontract moet dus expliciet compatibel blijven met bestaande `OnScanResult`
  / `OnScanError` consumers.

## Acceptance Focus

- Een gebruiker die na een onbekende code een nieuw product aanmaakt kan daarna op de
  add-stock-pagina een BootManager locatie-QR scannen en de juiste locatie selecteren.
- Dezelfde locatie-QR wordt in de add-stock-flow inhoudelijk hetzelfde behandeld als in
  `/scan`: eerst resolven, daarna de gelinkte locatie kiezen.
- De add-stock-flow blijft duidelijke fouten tonen voor onbekende of ongeldige
  locatie-QR's.
- Bestaande werkende scanflows blijven ongewijzigd werken.

## Test Evidence Requirements

- Name the production behavior or defect each new test executes.
- For this bugfix, require red-green evidence:
  - add at least one regression test that would fail against the current defect
    because the add-stock camera result is not processed through the correct callback
    and/or QR resolve path;
  - if a true pre-fix red run cannot be executed exactly because the broken callback is
    only observable through JS interop, report that limitation before the fix and add
    equivalent proof with a component-level test that invokes the production callback
    path actually expected from JS after the contract fix.
- Require real component rendering and meaningful assertions on selected location,
  error/success messaging and service calls.
- Preserve success and error paths with regression checks for:
  - pasted BootManager locatie-QR resolves and selects location;
  - unknown BootManager locatie-QR shows the expected error;
  - camera-result handling for a resolved BootManager locatie-QR selects the location;
  - existing `/scan` location routing and `ScanLocationAddProduct` product scan
    behavior remain compatible if touched.
- Forbid placeholder or documentary tests.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ScanProductAddStockComponentTests|FullyQualifiedName~ScanStartComponentTests"
```

Then:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
```

Before accepting the command results, inspect every new or changed test and confirm
that it can fail for the defect it claims to cover. A green test suite is not evidence
when its tests only document intended behavior.

## Definition of Technical Completion

Report `ready for Codex review` only when:

- `ScanProductAddStock` receives scanner success/error callbacks correctly;
- a scanned BootManager locatie-QR in the add-stock flow is resolved to
  `LinkedLocationId` before location selection;
- unknown or invalid location-QR behavior remains explicit and understandable;
- targeted tests pass;
- required red-green or equivalent defect-sensitive evidence is recorded;
- build and `git diff --check` pass;
- no unexplained change exists outside the expected write-set;
- remaining manual acceptance steps are listed explicitly, including mobile/Pi retest of
  the exact unknown-code-to-new-product scenario.

Report `not ready` when:

- callback handling in `ScanProductAddStock` still depends on method names that the
  shared scanner never invokes;
- the add-stock camera path still compares raw QR values directly to `LocationId`
  instead of resolving them first;
- tests/build/diffcheck fail;
- red-green or equivalent defect-sensitive evidence is missing;
- an additional write area cannot be justified;
- a larger scanner API redesign is required but not approved.

## Completion Notes

Return only:

1. changed files and implemented behavior;
2. tests/checks and results;
3. exact new/changed test names, the production behavior they execute and red-green
   evidence for the fix;
4. migration/configuration impact;
5. remaining risks and manual test requirements;
6. final status: `ready for Codex review` or `not ready`, with the concrete reason.
