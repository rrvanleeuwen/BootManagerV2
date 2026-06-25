# Implementation Packet

## Task

- Story ID: `PILOT-UX-01`
- Approved story: Home optimaliseren als snelle pilot-hub
- Story source: `.codex/claude-sources/ux/PILOT-UX-01.md`
- Goal: vervang de huidige placeholder-home op `/` door een mockup-geleide pilot-home
  met directe tegels naar `Logboek`, `Dashboard` en `Scannen`, plus een directe
  productzoekwidget met resultaatgedrag dat functioneel aansluit op de bestaande
  productzoekflow
- Required branch: `codex/pilot-ux-01-home-hub`

The story is already approved. Do not restate it or ask for approval. Give a short
plan, implement directly, run the checks, and provide completion notes.

Codex has already created the required feature branch. Claude must stop and report
`not ready` when the active branch is `master` or does not match the required branch.

## Scope

- Vervang `BootManager.Web/Components/Pages/Home.razor` als placeholder door een echte
  homepagina op `/`.
- Toon direct zichtbare primaire tegels naar:
  - `Logboek`
  - `Dashboard`
  - `Scannen`
- Voeg een productzoekwidget toe op home zonder extra navigatie.
- Toon per zoekresultaat:
  - productnaam;
  - totale hoeveelheid;
  - eenheid;
  - locaties waar het product te vinden is.
- Pagineer resultaten per 10 items.
- Vertaal de home-UI herkenbaar naar de aangeleverde mockups:
  - mobiel/card-hiërarchie volgens
    `.docs/analysis/stitch_responsive_bootstrap_process_design/home/code.html`
  - desktop/lijst-hiërarchie volgens
    `.docs/analysis/stitch_responsive_bootstrap_process_design/home_desktop/code.html`
- Gebruik voor resultaatgedrag dezelfde functionele semantiek als de bestaande
  productzoekflow in `Products.razor`:
  - 1 actieve locatie: direct navigeren naar die locatie;
  - meerdere actieve locaties: resultaten/keuzelijst tonen zonder directe navigatie;
  - geen actieve voorraad: verwachte locatie tonen en een actie `Voorraad toevoegen`
    aanbieden.
- Hergebruik bestaande inventory-services en bestaande locatie-route als bron van
  waarheid.

## Outside Scope

- Nieuwe dashboardinhoud of extra dashboardwidgets.
- Nieuwe logboekfunctionaliteit.
- Wijzigingen aan scanflows of scanroutering.
- Herontwerp van `Voorraadbeheer > Producten`; dat hoort bij `PILOT-INV-06`.
- Wijzigingen aan onderliggende voorraadbusinesslogica, behalve een minimale
  compile-time aanpassing als die aantoonbaar noodzakelijk is.
- Story-, release-, TODO-, legacy-, README-, handoff-, commit-, push-, branch-, PR-,
  merge-, release- of deploymentacties.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

- `BootManager.Web/Components/Pages/Home.razor`
- optioneel `BootManager.Web/Components/Pages/Home.razor.css`
- optioneel een klein nieuw presentatiedeel onder `BootManager.Web/Components/Shared/`
  of `BootManager.Web/Components/Inventory/` wanneer dat duplicatie beperkt
- gerichte home-componenttests onder `BootManager.UnitTests/`
- alleen minimaal noodzakelijke using/inject/DI-aanpassingen die direct bij de
  homepagina horen

Do not change by default:

- `BootManager.Web/Components/Pages/Inventory/Products.razor`
- application services in `BootManager.Application`
- scanpagina's
- navigatiestructuur buiten de normale linkdoelen van de home-tegels
- documentatiebestanden

Before changing an additional area, explain why it is required.

## Execution Boundaries

- Implement only application code, minimal styling and tests explicitly required by
  this packet.
- Before editing, verify that the active branch matches `codex/pilot-ux-01-home-hub`
  and is not `master`.
- Do not change story, release, TODO, legacy, README, handoff or other project
  documentation.
- Do not create commits, pushes, branches, PRs, merges, releases or deployments.
- Do not silently fall back to a generic bootstrap CRUD-home, generic table page or
  placeholder redirect behavior.
- If an existing design-system or authorization pattern forces a visible mockup
  deviation, keep it minimal and report it explicitly in completion notes.
- Do not broaden this slice into `PILOT-INV-06`.

## Minimal Context

Read:

- `CLAUDE.md`
- `.codex/PILOT-UX-01-implementation-packet.md`
- `.codex/claude-sources/ux/PILOT-UX-01.md`
- `BootManager.Web/Components/Pages/Home.razor`
- `BootManager.Web/Components/Pages/Inventory/Products.razor`
- `BootManager.Application/Inventory/Contracts/IProductService.cs`
- `BootManager.Application/Inventory/Contracts/IStockService.cs`
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`
- `.docs/analysis/stitch_responsive_bootstrap_process_design/home/code.html`
- `.docs/analysis/stitch_responsive_bootstrap_process_design/home_desktop/code.html`

Do not load by default:

- `AGENTS.md`
- `.codex/current-session-handoff.md`
- `.codex/working-agreement.md`
- full `.docs/TODO.md`
- `.docs/legacy-analysis/`
- unrelated release sections
- repository-wide source trees

## Existing Constraints

- Volg .NET 8 en de Clean Architecture-regels in `CLAUDE.md`.
- `Home.razor` is nu nog alleen een placeholder met `Doorsturen...`.
- De services die al bewezen zoekgedrag ondersteunen zijn:
  - `IProductService.SearchByNameOrDescriptionAsync`
  - `IStockService.GetActiveStocksByProductAsync`
  - `IStockService.GetExpectedLocationForProductAsync`
- De bestaande productzoekflow in `Products.razor` is de functionele referentie voor
  vervolginteractie, maar niet de visuele referentie voor de home-layout.
- De mockups zijn leidend voor hiërarchie en presentatie. Vrije terugval naar een
  standaard beheerlayout is niet acceptabel.
- De worktree bevat ongetrackte referentiemockups onder
  `.docs/analysis/stitch_responsive_bootstrap_process_design/`; gebruik ze als
  leesreferentie en wijzig ze niet.

## Acceptance Focus

- Home toont direct duidelijke primaire tegels voor `Logboek`, `Dashboard` en
  `Scannen`.
- De productzoekwidget is direct op home bruikbaar.
- Elk resultaat toont productnaam, hoeveelheid, eenheid en locaties.
- Resultaten zijn gepagineerd per 10.
- Desktop gebruikt lijstpresentatie; mobiel gebruikt cards.
- Resultaatinteractie volgt de afgesproken drie paden:
  - directe locatie bij exact één actieve voorraadlocatie;
  - keuzelijst bij meerdere actieve locaties;
  - verwachte locatie plus `Voorraad toevoegen` bij geen actieve voorraad.
- De UI volgt herkenbaar de mockup-hiërarchie en voelt niet als generieke
  bootstrap-lijst.

## Test Evidence Requirements

- Name the production behavior each new test executes.
- Require real bUnit rendering and real user interaction.
- Include tests that prove:
  - home renders the three primary tiles;
  - searching renders result content with productnaam, hoeveelheid, eenheid and
    locaties;
  - exactly one active location navigates directly to the location route;
  - multiple active locations show a location-choice result state without direct
    navigation;
  - no active stock shows the expected location and a `Voorraad toevoegen` action;
  - pagination limits visible search results to 10 items per page.
- If you introduce a shared presentation component, add tests against the real rendered
  home behavior rather than only testing helper methods.
- Inspect every new or changed test for defect sensitivity. No `Assert.True(true)`,
  empty tests, documentary assertions or async tests without relevant awaited behavior.

This story is not a bugfix, so formal red-green evidence is not required unless you
also fix an existing defect outside the planned feature work.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~Home"
```

If the new test class uses a different class name, replace the filter with the actual
new class name. If you reuse shared inventory presentation or logic that could affect
the existing product search flow, also run:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductsComponentTests"
```

Then run:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
```

Before accepting test results, inspect every new or changed test and confirm it can
fail for the behavior it claims to cover.

## Definition of Technical Completion

Report `ready for Codex review` only when:

- every scope item and acceptance focus point is technically implemented;
- `Home.razor` is no longer a placeholder;
- the home UI visibly follows the required mockup hierarchy;
- all targeted tests pass;
- build and `git diff --check` pass;
- no unexplained changes exist outside the expected write-set;
- any required mockup deviation is explicitly documented;
- remaining manual acceptance steps are listed explicitly.

Report `not ready` when any scope item is incomplete, the result interaction is
incomplete, pagination is missing, the UI regresses to a generic layout, tests/build/
diffcheck fail, or additional write areas cannot be justified.

## Completion Notes

Return only:

1. changed files and implemented behavior;
2. tests/checks and results;
3. exact new/changed test names and the production behavior they execute;
4. configuration or service impact;
5. remaining risks and manual test requirements;
6. final status: `ready for Codex review` or `not ready`.
