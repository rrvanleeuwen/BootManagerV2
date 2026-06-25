# Implementation Packet

## Task

- Story ID: `PILOT-SCAN-05`
- Approved story: Onbekende-code-flow binnen nieuwe scanervaring
- Story source: `.docs/releases/holiday-pilot-2026.md`
- Goal: vervang de zichtbare fallback van onbekende scans naar `/scan/old` door een
  volledig nieuwe onbekende-code-flow binnen de nieuwe scanroutes, met expliciete
  keuzes, scancontextbehoud en een UI die aantoonbaar de afgesproken scanrichtlijnen
  volgt
- Required branch: `codex/pilot-scan-05-unknown-code-flow`

The story is already approved. Do not restate it or ask for approval. Give a short
plan, implement directly, run the checks, and provide completion notes.

Codex must create and verify the required feature branch before giving this packet to
Claude. Claude must stop and report `not ready` when the active branch is `master` or
does not match the required branch.

## Scope

- Routeer een onbekende scan vanuit `/scan` naar een nieuw scan-specifiek
  onbekende-code-scherm in plaats van naar `/scan/old`.
- Bouw een nieuwe onbekende-code-werkcontext met minimaal:
  - duidelijke melding dat de code onbekend is;
  - zichtbare weergave van de gescande code;
  - expliciete keuze `Nieuw product aanmaken`;
  - expliciete keuze `Aan bestaand product koppelen`;
  - expliciete keuze `Annuleren en terug naar scanstart`.
- Laat `Nieuw product aanmaken` binnen nieuwe scanflow-schermen doorlopen naar de
  strikt noodzakelijke vervolgstappen om de code en eerste productcontext vast te
  leggen.
- Laat `Aan bestaand product koppelen` binnen nieuwe scanflow-schermen doorlopen naar
  de strikt noodzakelijke koppel- en vervolgactie.
- Houd de gescande onbekende code leidend totdat de gebruiker annuleert of opslaat.
- Vertaal de UI expliciet naar de scanflow-richtlijnen:
  - context eerst, formulier daarna;
  - rustige kaartopbouw;
  - maximaal één dominante eerstvolgende actie per scherm;
  - compact en taakgericht op mobiel;
  - rustig en overzichtelijk op desktop.

## Outside Scope

- Definitieve verwijdering van `/scan/old`.
- Brede herbouw van bekende product- of locatieroutes die al via `PILOT-SCAN-03A` en
  `PILOT-SCAN-04` zijn geaccepteerd.
- Nieuwe beheer-, rapportage- of dashboardfunctionaliteit buiten wat strikt nodig is
  voor onbekende-code-afhandeling.
- Wijzigingen aan release-, TODO-, README-, handoff- of legacy-documentatie.

## Hard Boundaries

- Een onbekende scan mag NIET meer zichtbaar eindigen op `/scan/old`.
- Een onbekende scan mag NIET eindigen op een generieke product-, locatie- of
  beheerpagina als eindervaring.
- Gebruik geen oude scanflow-pagina's of oude scanflow-componenten als zichtbare
  eindschermen voor dit pad.
- Hergebruik van onderliggende functionele logica uit de oude flow mag alleen wanneer
  de zichtbare route, schermen en navigatie nieuw scanflow-gedrag blijven tonen.
- Laat geen half afgemaakte keuze achter:
  - geen knop zonder werkende vervolgroute;
  - geen scherm zonder duidelijke hoofdactie;
  - geen tijdelijke zichtbare handoff naar legacy.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

- `BootManager.Web\Components\Pages\Scan.razor`
- `BootManager.Web\Components\Pages\Scan.razor.css`
- one or more new scan-specific unknown-code pages/components under
  `BootManager.Web\Components\Pages\`
- only the new scan-specific product follow-up pages/components that are strictly
  required for `nieuw product` or `koppelen`
- targeted tests under `BootManager.UnitTests\Storage\`

Do not change by default:

- `BootManager.Web\Components\Layout\NavMenu.razor`
- `BootManager.Web\wwwroot\js\barcodeScanner.js`
- old-flow pages/components as the visible solution
- unrelated inventory, logbook or dashboard modules
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
- Do not silently fall back to legacy pages/components for any unknown-code success or
  cancel path.
- If a complete new unknown-code path cannot be finished within the write-set, report
  `not ready` instead of mixing old and new end screens.

## Minimal Context

Read:

- `.codex\PILOT-SCAN-05-implementation-packet.md`
- the `PILOT-SCAN-05` section in `.docs/releases/holiday-pilot-2026.md`
- `.docs/analysis/ScannenFlow/scanflow-herdefinitie.md`
- `.docs/analysis/ScannenFlow/scanflow-ui-richtlijnen.md`
- `BootManager.Web\Components\Pages\Scan.razor`
- `BootManager.Web\Components\Pages\ScanOld.razor`
- `BootManager.Web\Components\Pages\ScanProduct.razor`
- `BootManager.Web\Components\Pages\ScanProductAddStock.razor`
- relevant targeted tests under `BootManager.UnitTests\Storage\`

Do not load by default:

- full `.docs/TODO.md`;
- unrelated epic documents;
- `.docs/legacy-analysis/`;
- `.docs/legacy-input/`;
- `.codex/current-session-handoff.md`;
- repository-wide source trees.

## Existing Constraints

- De nieuwe scanflow gebruikt `/scan` als canonieke ingang.
- `PILOT-SCAN-03A` en `PILOT-SCAN-04` zijn handmatig geaccepteerd; verbreed hun scope
  niet opnieuw zonder noodzaak.
- Flow en UI worden samen beoordeeld; een technisch werkende route zonder duidelijke
  UI-vertaling naar de scanrichtlijnen is niet acceptabel.
- De onbekende-code-flow moet kort, expliciet en veilig blijven: geen impliciete
  productcreatie, geen verborgen koppelingen en geen verlies van de net gescande
  context.

## Acceptance Focus

- De gebruiker begrijpt direct dat de code onbekend is.
- De drie keuzes zijn onmiddellijk zichtbaar en begrijpelijk.
- De gescande code blijft duidelijk leidend in het scherm.
- `Nieuw product` en `Koppelen` voelen als onderdeel van de nieuwe scanervaring, niet
  als een terugval naar beheer of legacy.
- `Annuleren` brengt de gebruiker duidelijk terug naar scanstart.
- Mobile en desktop tonen dezelfde taakhiërarchie met weinig ruis en precies één
  dominante eerstvolgende actie per scherm.

## Test Evidence Requirements

- Name the production behavior each new test executes.
- Require real component rendering and user interaction through bUnit.
- Include tests that prove:
  - an unknown scan from `/scan` no longer routes to `/scan/old`;
  - the new unknown-code screen renders the scanned code and the three explicit
    choices;
  - cancel returns to `/scan`;
  - the `nieuw product` path stays inside the new scan routes;
  - the `koppelen` path stays inside the new scan routes;
  - known location and known product routing from `/scan` still behave as before.
- If a regression test is added for the old unknown-code fallback, require red-green
  evidence or a concrete reason why equivalent proof was used instead.
- Forbid placeholder or documentary tests.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ScanStartComponentTests|FullyQualifiedName~ScanComponentTests|FullyQualifiedName~ScanProduct"
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

- unknown scans from `/scan` land in a new scan-specific unknown-code flow;
- the user can choose `Nieuw product aanmaken`, `Aan bestaand product koppelen` or
  `Annuleren` from that new flow;
- the reachable follow-up paths remain inside the new scan experience and do not bounce
  into visible legacy screens;
- the rendered screens follow the agreed UI direction with clear context and one
  dominant next action;
- targeted tests pass and execute real behavior;
- build and `git diff --check` pass;
- no unexplained change exists outside the expected write-set;
- remaining manual acceptance steps are listed explicitly.

Report `not ready` when:

- unknown scans still end visibly in `/scan/old`;
- any reachable unknown-code continuation path still lands in legacy or generic
  management screens as end experience;
- the UI still feels like an old CRUD-style page or lacks a clear dominant next action;
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
