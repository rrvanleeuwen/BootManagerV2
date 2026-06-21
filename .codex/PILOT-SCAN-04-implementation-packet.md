# Implementation Packet

## Task

- Story ID: `PILOT-SCAN-04`
- Approved story: Locatiegerichte scanmodus
- Story source: `.docs/releases/holiday-pilot-2026.md`
- Goal: bouw een volledig nieuwe locatie-scanwerkcontext voor bekende locatie-QR's,
  zodat de gebruiker na een locatiescan direct in een nieuwe taakgerichte werkcontext
  komt en niet terugvalt op oude scanflow-pagina's of de generieke locatiepagina
- Required branch: `codex/pilot-scan-04-location-scan-context`

The story is already approved. Do not restate it or ask for approval. Give a short
plan, implement directly, run the checks, and provide completion notes.

Claude must stop and report `not ready` when the active branch is `master` or does not
match the required branch.

## Scope

- Routeer een bekende locatie-QR vanuit `/scan` naar een nieuwe locatie-scanwerkcontext.
- Bouw een nieuwe locatiepagina voor de scanflow met minimaal:
  - duidelijke locatiekop;
  - lijst van aanwezige producten op die locatie;
  - directe mutatie-ingang op bestaand product;
  - actie `Ander product toevoegen`.
- Houd de locatiecontext vast binnen dit pad.
- Vertaal de UI expliciet naar de scanflow-richtlijnen:
  - rustige kaartopbouw;
  - minimale informatie;
  - duidelijke hoofdactie;
  - compact op mobiel;
  - overzichtelijk op desktop.
- Implementeer deze route als nieuwe scanflow-implementatie end-to-end voor het
  bekende-locatie-pad.

## Outside Scope

- Bekende productscanroute uit `PILOT-SCAN-03A` anders maken dan strikt nodig voor
  routing naar de nieuwe locatiecontext.
- Definitieve onbekende-code-flow uit `PILOT-SCAN-05`.
- Brede locatiebeheer- of administratiepagina's.
- Definitieve verwijdering van `/scan/old`.
- Wijzigingen aan release-, TODO-, README-, handoff- of legacy-documentatie.

## Hard Boundaries

- Een bekende locatiescan mag NIET eindigen op de bestaande generieke locatiepagina.
- Een bekende locatiescan mag NIET eindigen in `/scan/old`.
- Gebruik geen oude scanflow-pagina's of oude scanflow-componenten voor dit pad.
- Gebruik geen verborgen technische handoff naar legacy die later zichtbaar wordt.
- Vlieg dit pad aan als nieuwe implementatie, overal waar de gebruiker binnen deze
  locatie-scanroute komt.
- Laat geen zichtbare actie half gebouwd achter:
  - geen niet-bestaande route;
  - geen knop zonder echte taakuitvoering;
  - geen formulier zonder echte opslag of vervolgactie.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

- `BootManager.Web\Components\Pages\Scan.razor`
- `BootManager.Web\Components\Pages\Scan.razor.css`
- one or more new scan-specific location pages/components under
  `BootManager.Web\Components\Pages\`
- any new scan-specific supporting components required to keep this path independent
  from the old flow
- targeted tests under `BootManager.UnitTests\Storage\`

Do not change by default:

- `BootManager.Web\Components\Layout\NavMenu.razor`
- `BootManager.Web\wwwroot\js\barcodeScanner.js`
- the existing generic location page as a reusable end screen for this path
- old-flow pages/components
- release/docs/handoff/TODO/README

Before changing an additional area, explain why it is required.

## Execution Boundaries

- Implement only application code, configuration and tests explicitly required by this
  packet.
- Before editing, verify that the active branch matches `Required branch` and is not
  `master`.
- Do not change story, release, TODO, legacy, README, handoff or other project
  documentation.
- Do not create commits, pushes, branches, PRs, merges, releases or deployments.
- Do not silently fall back to legacy pages/components for the known-location success
  path.
- If a complete new known-location path cannot be finished within the write-set, report
  `not ready` instead of mixing old and new.

## Minimal Context

Read:

- `.codex\PILOT-SCAN-04-implementation-packet.md`
- the `PILOT-SCAN-04` section in `.docs/releases/holiday-pilot-2026.md`
- `.docs/analysis/ScannenFlow/scanflow-herdefinitie.md`
- `.docs/analysis/ScannenFlow/scanflow-ui-richtlijnen.md`
- `BootManager.Web\Components\Pages\Scan.razor`
- `BootManager.Web\Components\Pages\Scan.razor.css`
- `BootManager.Web\Components\Pages\ScanProduct.razor`
- relevant targeted tests under `BootManager.UnitTests\Storage\`

Do not load by default:

- full repository trees;
- unrelated epic documents;
- `.docs/legacy-analysis/`;
- `.codex/current-session-handoff.md`.

## Acceptance Focus

- The user immediately understands which location is active.
- The user immediately sees the products on that location.
- The location route feels like a scan workcontext, not a management record page.
- The user can mutate an existing product without leaving the new scanflow.
- The user can add another product while the location context remains fixed.
- Mobile and desktop both preserve clarity and low-noise hierarchy.

## Test Evidence Requirements

- Name the production behavior each new test executes.
- Require real component rendering and user interaction through bUnit.
- Include tests that prove:
  - a known location scan from `/scan` lands in a new location-scan workcontext;
  - the new workcontext renders location identity and stock/product context;
  - the known-location success path no longer ends in the generic location page or
    `/scan/old`;
  - a user can enter the mutate flow for an existing product while staying inside the
    new scan route;
  - a user can enter the add-product/add-stock route while keeping the location context.
- Forbid placeholder or documentary tests.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ScanStartComponentTests|FullyQualifiedName~ScanProductComponentTests|FullyQualifiedName~ScanComponentTests|FullyQualifiedName~ScanLocation"
```

Then:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
```

## Definition of Technical Completion

Report `ready for Codex review` only when:

- the known-location route from `/scan` lands in a new scan-specific location
  workcontext;
- the rendered screen follows the agreed UI direction with minimal information and a
  clear primary action;
- the reachable known-location continuation path remains inside the new implementation
  and does not bounce into legacy pages/components;
- targeted tests pass and execute real behavior;
- build and `git diff --check` pass;
- no unexplained change exists outside the expected write-set;
- remaining manual acceptance steps are listed explicitly.

Report `not ready` when:

- the known-location route still ends in the generic location page or `/scan/old`;
- any reachable action inside the known-location path still routes into legacy pages or
  old-flow components;
- the UI still feels like a legacy management page;
- a required new scan-specific screen cannot be built within the allowed write-set;
- tests/build/diffcheck fail;
- a required design or scope decision is missing.

## Completion Notes

Return only:

1. changed files and implemented behavior;
2. tests/checks and results;
3. exact new/changed test names and the production behavior they execute;
4. migration/configuration impact;
5. remaining risks and manual test requirements;
6. final status: `ready for Codex review` or `not ready`, with the concrete reason.
