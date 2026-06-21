# Implementation Packet

## Task

- Story ID: `PILOT-SCAN-03`
- Approved story: Nieuw scanstartscherm met routering
- Story source: `.docs/releases/holiday-pilot-2026.md`
- Goal: vervang de tijdelijke `/scan`-redirect door een echt nieuw scanstartscherm met camera, handmatige invoer, sessie-recents en eerste code-routering, terwijl product- en onbekende-code-paden tijdelijk gecontroleerd doorlopen naar `/scan/old`
- Required branch: `codex/pilot-scan-03-scanstart-routering`

The story is already approved. Do not restate it or ask for approval. Give a
short plan, implement directly, run the checks, and provide completion notes.

Codex must create and verify the required feature branch before giving this packet to
Claude. Claude must stop and report `not ready` when the active branch is `master` or
does not match the required branch.

## Scope

- Bouw een nieuw zelfstandig scanstartscherm op `/scan`.
- Toon daarop:
  - camera-startactie;
  - handmatige code-invoer;
  - recente scans van de huidige sessie.
- Hergebruik de bestaande scannertechniek als technische basis; introduceer geen tweede
  scannerstack.
- Routeer bekende locatiecodes direct naar de bestaande locatiepagina.
- Routeer bekende productcodes tijdelijk gecontroleerd door naar `/scan/old` met
  overdracht van de al gescande code.
- Routeer onbekende codes tijdelijk gecontroleerd door naar `/scan/old` met overdracht
  van de al gescande code.
- Zorg dat `/scan/old` de ontvangen handoff-code zonder herinvoer kan verwerken.
- Lever niet alleen correcte routering, maar ook een herkenbare UI-vertaling van het
  scanstartdesign zodat de gebruiker de draad niet kwijtraakt.

## Outside Scope

- Nieuwe locatiecontextschermen uit `PILOT-SCAN-04`.
- Nieuwe productcontext of expliciete onbekende-code-flow uit `PILOT-SCAN-05`.
- Persistente of cross-device scanhistorie.
- Verwijderen of visueel redesignen van `/scan/old`.
- Wijzigingen aan release-, TODO-, README-, handoff- of legacy-documentatie.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

- `BootManager.Web\Components\Pages\Scan.razor`
- `BootManager.Web\Components\Pages\Scan.razor.css`
- `BootManager.Web\Components\Pages\ScanOld.razor`
- `BootManager.Web\Components\Pages\ScanOld.razor.css` only if styling is strictly needed
- `BootManager.UnitTests\Storage\ScanComponentTests.cs`
- a new targeted test file under `BootManager.UnitTests\Storage\` for the new `/scan`
  startscreen behavior, for example `ScanStartComponentTests.cs`

Do not change by default:

- `BootManager.Web\Components\Layout\NavMenu.razor`
- `BootManager.Web\wwwroot\js\barcodeScanner.js`
- inventory, storage or product pages outside what is strictly needed for the handoff
  contract

Before changing an additional area, explain why it is required.

## Execution Boundaries

- Implement only application code, configuration and tests explicitly required by this
  packet.
- Before editing, verify that the active branch matches `Required branch` and is not
  `master`.
- Do not change story, release, TODO, legacy, README, handoff or other project
  documentation.
- Do not create commits, pushes, branches, PRs, merges, releases or deployments.
- Do not change scope, acceptance criteria or architectural direction. Stop and report
  the smallest missing decision when an approved direction cannot be followed.
- Never declare the story `Done`, accepted or production-ready. Only report
  `ready for Codex review` after satisfying the technical completion definition.

## Minimal Context

Read:

- `.codex\PILOT-SCAN-03-implementation-packet.md`
- the `PILOT-SCAN-03` section in `.docs/releases/holiday-pilot-2026.md`
- `.docs/analysis/ScannenFlow/scanflow-herdefinitie.md`
- `.docs/analysis/ScannenFlow/scanflow-ui-richtlijnen.md`
- `BootManager.Web\Components\Pages\Scan.razor`
- `BootManager.Web\Components\Pages\Scan.razor.css`
- `BootManager.Web\Components\Pages\ScanOld.razor`
- `BootManager.UnitTests\Storage\ScanComponentTests.cs`

Do not load by default:

- full `.docs/TODO.md`;
- unrelated epic documents;
- `.docs/legacy-analysis/`;
- `.docs/legacy-input/`;
- `.codex/current-session-handoff.md`;
- repository-wide source trees.

## Existing Constraints

- `/scan` is currently only a temporary redirect shell and must become the new canonical
  scanstartscherm in this story.
- `/scan/old` remains the temporary functional fallback and must stay usable.
- De scanstories worden vanaf nu beoordeeld op flow en UI samen. Een werkende route die
  eindigt in een oud CRUD-achtig scherm is niet voldoende als beoogde eindervaring.
- The old flow already contains real processing for:
  - known location QR to existing location page;
  - known product handling;
  - unknown-code handling;
  - mutation/inventory follow-up.
- There is currently no persistent “recent scans” backend. For this slice, a
  component-local session list is acceptable and preferred.
- The old scanner tests currently target `ScanOld`; preserve those success/error paths.

## Acceptance Focus

- `/scan` feels like a real new start page rather than a redirect notice.
- The new page makes scan, manual input and recent activity immediately visible.
- The page hierarchy matches the intended design translation: one dominant scan action,
  clear supporting blocks, little noise, and no “where am I?” confusion.
- Known location routing already lands on the correct existing page.
- Product and unknown routes do not force the user to re-enter the code when they
  temporarily hand off to `/scan/old`.
- The temporary bridge keeps `PILOT-SCAN-04` and `PILOT-SCAN-05` small instead of
  partially implementing them now.
- If a temporary bridge still lands in a visibly old page, call that out explicitly as
  temporary technical debt instead of presenting it as final UX.

## Test Evidence Requirements

- Name the production behavior each new test executes.
- Require real component rendering and user interaction through bUnit.
- For the new `/scan` screen, include tests that prove:
  - the new scanstart UI renders;
  - a known location code navigates directly to the existing location page;
  - a known product code hands off to `/scan/old` with the code preserved;
  - an unknown code hands off to `/scan/old` with the code preserved;
  - session-recents update newest first after scan or manual entry.
- For `/scan/old`, add or update regression checks that prove a handoff value can be
  processed without the user re-entering the code, while preserving the existing old
  success path.
- Forbid placeholder or documentary tests, including `Assert.True(true)`, empty tests,
  source-shape assertions instead of behavior, and `async` tests without meaningful
  awaited behavior.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ScanStartComponentTests|FullyQualifiedName~ScanComponentTests"
```

Then:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
```

Before accepting the command results, inspect every new or changed test and confirm
that it can fail for the behavior it claims to cover. A green test suite is not
evidence when its tests only document intended behavior.

## Definition of Technical Completion

Report `ready for Codex review` only when:

- every scope item and acceptance criterion is technically implemented;
- all targeted tests pass;
- every new or changed test executes real product behavior and contains meaningful
  assertions;
- the full required test run contains no new failure;
- build and `git diff --check` pass;
- no unexplained change exists outside the expected write-set;
- remaining manual acceptance steps are listed explicitly.

Report `not ready` when any scope item is incomplete, a handoff path still requires
manual re-entry, a new or changed test fails, build/diffcheck fails, a required
decision is missing, or an additional write area cannot be justified.

## Completion Notes

Return only:

1. changed files and implemented behavior;
2. tests/checks and results;
3. exact new/changed test names and the production behavior they execute;
4. migration/configuration impact;
5. remaining risks and manual test requirements;
6. final status: `ready for Codex review` or `not ready`, with the concrete reason.
