# Implementation Packet

## Task

- Story ID: `PILOT-INV-06`
- Approved story: Productoverzicht herontwerpen naar dezelfde responsieve zoek- en resultaatstijl
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-INV-06`
- Goal: herontwerp `Voorraadbeheer > Producten` van een generieke beheertabel naar
  een mockup-geleide, responsieve productzoek- en resultaatpagina. De resultaten
  gebruiken dezelfde inhoudelijke opbouw als de home-widget: productnaam, totale
  hoeveelheid, eenheid en locaties.
- Required branch: `codex/pilot-inv-06-products-overview`

The story is already approved. Do not restate it or ask for approval. Give a short
plan, implement directly, run the checks, and provide completion notes.

Codex has already created the required feature branch. Claude must stop and report
`not ready` when the active branch is `master` or does not match the required branch.

## Scope

- Herbouw alleen de niet-bewerkende lijstweergave in
  `Voorraadbeheer > Producten` naar de verplichte mockup-hiërarchie uit
  `.docs/analysis/stitch_responsive_bootstrap_process_design/producten_overzicht/`.
- Plaats de paginatitel, de bestaande actie voor een nieuw product en een direct
  beschikbare zoekinvoer boven de resultaten. De zoekinvoer zoekt op productnaam en
  omschrijving, hoofdletterongevoelig, zoals de bestaande productzoekflow.
- Toon per productresultaat:
  - productnaam;
  - totale actieve voorraad, berekend als de som van de actieve voorraadlocaties;
  - standaardeenheid;
  - alle actieve locaties als herkenbare locatiechips met `gebied - locatie`;
  - bij geen actieve voorraad een duidelijke, rustige no-stockstatus in plaats van
    een ontbrekende hoeveelheid of locatie;
  - gekoppelde productcode als secundaire informatie wanneer beschikbaar, in lijn met
    de mockup, zonder daarvoor nieuwe velden of codeformats toe te voegen.
- Pagineer zowel de initiele productlijst als gefilterde zoekresultaten in groepen van
  exact 10. Een nieuwe zoekopdracht of wijziging van de archiefstand begint op pagina
  1. Toon alleen paginering wanneer meer dan een pagina bestaat.
- Gebruik voor desktop vanaf `768px` een compacte horizontale lijstregel; gebruik
  daaronder een duidelijke cardweergave. Beide weergaven tonen dezelfde inhoud en
  behouden hun bestaande productacties.
- Houd de bestaande primaire klik op een zoekresultaat en de aparte
  `Productdetails`- en `Bewerken/code`-acties functioneel intact. De huidige
  vervolgflow bij een, meerdere of geen actieve locaties mag niet veranderen.
- Houd het bestaande productbeheer intact: nieuw product, bewerken, archiveren,
  reactiveren en het tonen van gearchiveerde producten blijven bereikbaar.
- Verberg uitsluitend op mobiel de knoppen `Gearchiveerd weergeven`/
  `Actieve weergeven` en `Voorraadbijzonderheid`. De functies blijven op desktop
  beschikbaar.

## Outside Scope

- Geen wijziging aan voorraad-, mutatie-, scan- of productbusinesslogica.
- Geen nieuwe productvelden, voorraadstatusdrempels, filters of archiveerregels.
- Geen wijziging aan de functionele home-widget; gebruik diens resultaatsemantiek als
  referentie, maar breid de home-scope niet uit.
- Geen herontwerp van andere voorraad-, opslag- of scanpagina's.
- Geen documentatie-, commit-, push-, branch-, PR-, merge-, release- of
  deploymentacties, behalve de verplichte processtatus hieronder.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- nieuw `BootManager.Web/Components/Pages/Inventory/Products.razor.css` voor de
  responsieve pagina-, lijst- en cardpresentatie;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- `.docs/processtatus/codex-pilot-inv-06-products-overview/ClaudeStatus.md`.

Do not change by default:

- `BootManager.Web/Components/Pages/Home.razor` or `Home.razor.css`;
- `BootManager.Application/**`, `BootManager.Core/**` or
  `BootManager.Infrastructure/**`;
- `BootManager.Web/Components/Inventory/ProductDetailsFromHome.razor`;
- routes, authorization, navigation or global styles;
- story, release, TODO, legacy, README or handoff documentation.

Before changing an additional area, explain why it is required.

## Execution Boundaries

- Implement only application code, page-local styling and tests explicitly required by
  this packet.
- Before editing, verify that the active branch matches
  `codex/pilot-inv-06-products-overview` and is not `master`.
- Do not change story, release, TODO, legacy, README, handoff or other project
  documentation.
- Do not use a generic Bootstrap table or cosmetic table reskin as the resulting
  overview. The card/list hierarchy from the supplied mockup is mandatory.
- Keep existing service contracts as the source of truth. Do not introduce a new
  inventory query, migration or client-side substitute for active stock data.
- Preserve the existing main result-click behavior, detail popup and edit navigation;
  a visual rewrite is not authorization to remove or redirect them.
- Before finishing, create or update
  `.docs/processtatus/codex-pilot-inv-06-products-overview/ClaudeStatus.md`.
- Put the full `Completion Notes` content in that `ClaudeStatus.md` file and end the
  file with a separate line `Done: yyyy-MM-dd HH:mm`.
- Treat that `Done:` line only as a handoff signal for Codex review, never as a claim
  that the story is accepted or production-ready.
- Do not create commits, pushes, branches, PRs, merges, releases or deployments.
- Do not change scope, acceptance criteria or architectural direction. Stop and report
  the smallest missing decision when an approved direction cannot be followed.
- Never declare the story `Done`, accepted or production-ready. Only report
  `ready for Codex review` after satisfying the technical completion definition.

## Minimal Context

Read:

- `CLAUDE.md`;
- `.codex/PILOT-INV-06-implementation-packet.md`;
- the section `PILOT-INV-06` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.Web/Components/Pages/Home.razor` and `Home.razor.css` only as the
  functional and presentation reference for result content and pagination;
- `BootManager.Application/Inventory/DTOs/ProductDto.cs`;
- `BootManager.Application/Inventory/DTOs/StockDto.cs`;
- `BootManager.Application/Inventory/Contracts/IProductService.cs`;
- `BootManager.Application/Inventory/Contracts/IStockService.cs`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- `.docs/analysis/stitch_responsive_bootstrap_process_design/producten_overzicht/code.html`;
- `.docs/analysis/stitch_responsive_bootstrap_process_design/producten_overzicht/screen.png`.

Do not load by default:

- full `.docs/TODO.md`;
- unrelated epic or release documents;
- `.docs/legacy-analysis/` or `.docs/legacy-input/`;
- `.codex/current-session-handoff.md` or `.codex/working-agreement.md`;
- repository-wide source trees.

## Existing Constraints

- Follow .NET 8 and the repository architecture rules in `CLAUDE.md`.
- `Products.razor` already has a functional manual search and product-finding flow.
  Its primary result click can navigate to one location, show several locations, or
  offer stock addition when none exists. Preserve all three paths.
- `Products.razor` already exposes a separate detail popup action and edit route from
  search results. Preserve their visibility and behavior in the redesigned result
  presentation.
- `Home.razor` already establishes the exact inventory result semantics to mirror:
  `IProductService.SearchByNameOrDescriptionAsync`, then
  `IStockService.GetActiveStocksByProductAsync`, total active quantity, standard unit
  and location labels; it uses 10 items per page.
- The page already loads the inventory catalogue with `GetAllAsync`. Its existing
  active/archived toggle must still govern which catalogue items are eligible for the
  initial result set.
- The mockup is leading for hierarchy, spacing and visual priority, not a source for
  its sample SKUs, stock thresholds, floating scanner button, bottom navigation or
  placeholder image. Do not implement those mockup-only artifacts.
- Prefer page-local CSS isolation over global CSS changes. Use semantic classes and
  accessible buttons; do not add JavaScript just to determine the viewport.

## Acceptance Focus

- `Voorraadbeheer > Producten` is recognizably related to the home search widget and
  visibly follows the supplied product-overview mockup rather than a generic table.
- Every visible product result shows name, total quantity, unit and active locations.
- Results are paginated at 10 on both the initial overview and after searching.
- Desktop uses list rows and mobile uses cards without losing result information or
  existing product actions.
- `Gearchiveerd weergeven`/`Actieve weergeven` and `Voorraadbijzonderheid` are absent
  on mobile but usable on desktop.
- Existing search-result interaction, product details, edit navigation, archive state
  and the no-active-stock fallback are not regressed.

## Test Evidence Requirements

- Name the production behavior or defect each new test executes.
- This is a new UX slice, so formal red-green evidence is not required unless you also
  fix a discovered pre-existing defect outside the planned feature work.
- Require real bUnit rendering and user interaction. Do not test private helpers by
  reflection when the behavior is observable through the rendered component.
- Add or update tests that prove at least:
  - an initial catalogue with 11 active products renders only 10 results, shows the
    correct product/total/unit/location content and moves to the eleventh result with
    the pagination control;
  - a search starts on page 1, returns the same result information and has its active
    stock totals/locations loaded through `IStockService`;
  - a no-active-stock product renders the deliberate no-stock state;
  - the separate product-detail and edit actions remain distinct from the primary
    product-result click;
  - the current one-location, multi-location and no-stock primary-click behaviors
    remain covered by the existing tests after the markup changes;
  - desktop-only controls have the responsive visibility contract in the rendered
    component, with manual viewport verification required below.
- Inspect every new or changed test for defect sensitivity. Forbid `Assert.True(true)`,
  empty tests, source-shape assertions used instead of behavior, and `async` tests
  without relevant awaited behavior.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductsComponentTests"
```

Then:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
```

Before accepting test results, inspect every new or changed test and confirm it can
fail for the behavior it claims to cover.

## Definition of Technical Completion

Report `ready for Codex review` only when:

- every scope and acceptance-focus item is technically implemented;
- the generic product table is no longer the main product-overview UI;
- 10-item pagination works for initial and searched result sets;
- all result cards/list rows expose name, total, unit and locations or the deliberate
  no-stock state;
- existing primary result-click, detail, edit and no-stock flows are preserved;
- all targeted tests pass and every new/changed test executes real product behavior
  with meaningful assertions;
- build and `git diff --check` pass;
- no unexplained change exists outside the expected write-set;
- remaining manual acceptance steps are listed explicitly, including desktop and
  mobile viewport checks against the supplied mockup.

Report `not ready` when any scope item is incomplete, the page remains a generic table,
pagination or result content is missing, an existing interaction regresses, a test is
documentary or cannot detect the claimed behavior, build/diffcheck fails, a required
decision is missing, or an additional write area cannot be justified.

## Completion Notes

Return only:

1. changed files and implemented behavior;
2. tests/checks and results;
3. exact new/changed test names and the production behavior they execute;
4. migration/configuration impact;
5. remaining risks and manual test requirements;
6. final status: `ready for Codex review` or `not ready`, with the concrete reason.
