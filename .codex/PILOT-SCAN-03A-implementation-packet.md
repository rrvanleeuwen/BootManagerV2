# Implementation Packet

## Task

- Story ID: `PILOT-SCAN-03A`
- Approved story: Product-scanwerkcontext zonder legacy-terugval
- Story source: `.docs/releases/holiday-pilot-2026.md`
- Goal: bouw een echte product-scanwerkcontext voor bekende productcodes, zodat de
  nieuwe scanflow niet meer eindigt in `/scan/old` of een generieke beheer/detailpagina
  als eindervaring
- Required branch: `codex/pilot-scan-03a-product-scan-context`

The story is already approved. Do not restate it or ask for approval. Give a
short plan, implement directly, run the checks, and provide completion notes.

Claude must stop and report `not ready` when the active branch is `master` or does not
match the required branch.

## Scope

- Bouw een nieuwe product-scanwerkcontext voor bekende productcodes.
- Zorg dat een bekende productscan vanuit `/scan` naar deze nieuwe werkcontext gaat.
- Toon minimaal:
  - productkop met naam en code;
  - compacte voorraad per locatie;
  - één duidelijke hoofdactie voor het vervolg van de scanflow.
- Vertaal de UI expliciet naar de scanflow-richtlijnen:
  - rustige kaartopbouw;
  - minimale informatie;
  - duidelijke hiërarchie;
  - compact op mobiel;
  - overzichtelijk op desktop.
- Houd de ervaring taakgericht; geen klassiek beheer- of detailscherm.
- Implementeer deze route als nieuwe scanflow-implementatie end-to-end voor het
  bekende-product-pad; gebruik geen oude scanflow-pagina's of -componenten als
  vervolgstap.
- Maak de zichtbare vervolgacties ook echt werkend binnen deze story:
  - `Muteren op bestaande locatie`;
  - `Voorraad op andere locatie toevoegen`;
  - `Voorraad toevoegen` bij geen actieve voorraad.

## Outside Scope

- Volledige locatie-scanwerkcontext uit `PILOT-SCAN-04`.
- Definitieve onbekende-code-flow uit `PILOT-SCAN-05`.
- Volledig productbeheer, productedit-formulieren of brede beheerinformatie.
- Definitieve verwijdering van `/scan/old`.
- Wijzigingen aan release-, TODO-, README-, handoff- of legacy-documentatie.

## Hard UX Boundaries

- Een bekende productscan mag NIET eindigen in `/scan/old` als eindervaring.
- Een bekende productscan mag NIET eindigen op een generieke beheer- of CRUD-achtige
  product- of locatiepagina als eindervaring.
- Een bekende productscan mag ook NIET voor vervolgstappen terugvallen op oude
  scanflow-pagina's, oude scanflow-componenten, generieke locatiepagina's of andere
  legacy-bouwstenen uit de bestaande flow.
- Gebruik dus geen enkele pagina of component van de oude flow voor dit pad:
  - niet `ScanOld`;
  - niet oude scan-gerelateerde child components;
  - niet de bestaande generieke locatiepagina als vervanger van scancontext;
  - niet een verborgen technische handoff naar legacy die later alsnog zichtbaar wordt.
- Vlieg dit pad aan als nieuwe implementatie, overal waar de gebruiker binnen deze
  productscanroute komt.
- Een technisch werkende route zonder duidelijke scan-UX geldt als `not ready`.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

- `BootManager.Web\Components\Pages\Scan.razor`
- `BootManager.Web\Components\Pages\Scan.razor.css`
- one or more new scan-specific page/components under `BootManager.Web\Components\Pages\`
  or a clearly justified nearby scan-specific folder for the new product workcontext
- any new scan-specific supporting components required to keep this path independent
  from the old flow
- specifically allowed when needed for completion:
  - `ScanProduct.razor`
  - `ScanProduct.razor.css`
  - `ScanProductMutate.razor`
  - a new route page for the selected mutation target, e.g.
    `ScanProductMutateLocation.razor` with route
    `/scan/product/{ProductId}/mutate/{StorageLocationId}`
  - `ScanProductAddStock.razor`
- targeted tests under `BootManager.UnitTests\Storage\`

Do not change by default:

- `BootManager.Web\Components\Layout\NavMenu.razor`
- `BootManager.Web\wwwroot\js\barcodeScanner.js`
- broad inventory management pages unless strictly required for the new scan-specific
  workcontext
- existing old-flow pages/components as reusable building blocks for the known-product
  path
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
- Do not silently fall back to `/scan/old`, old scanflow components, or old detail
  screens for the known-product success path.
- Do not implement the primary or secondary action by navigating back into legacy.
- Do not leave visible actions half-built:
  - no route to a page that does not exist;
  - no form submit that only navigates away;
  - no button that represents a task but performs no real stock operation.
- If a complete new known-product path cannot be finished within the write-set, report
  `not ready` instead of mixing old and new.
- Never declare the story `Done`, accepted or production-ready. Only report
  `ready for Codex review` after satisfying the technical completion definition.

## Minimal Context

Read:

- `.codex\PILOT-SCAN-03A-implementation-packet.md`
- the `PILOT-SCAN-03A` section in `.docs/releases/holiday-pilot-2026.md`
- `.docs/analysis/ScannenFlow/scanflow-herdefinitie.md`
- `.docs/analysis/ScannenFlow/scanflow-ui-richtlijnen.md`
- `BootManager.Web\Components\Pages\Scan.razor`
- `BootManager.Web\Components\Pages\Scan.razor.css`
- `BootManager.Web\Components\Pages\ScanProduct.razor`
- `BootManager.Web\Components\Pages\ScanProduct.razor.css`
- `BootManager.Web\Components\Pages\ScanProductMutate.razor`
- `BootManager.Web\Components\Pages\ScanProductAddStock.razor`
- `BootManager.UnitTests\Storage\ScanStartComponentTests.cs`
- `BootManager.UnitTests\Storage\ScanComponentTests.cs`
- `BootManager.UnitTests\Storage\ScanProductComponentTests.cs`
- `BootManager.Web\Components\Inventory\StockMutationModal.razor` only as service/API
  behavior reference, not as reusable old-flow UI
- `BootManager.Web\Components\Inventory\AddStockToProductModal.razor` only as
  service/API behavior reference, not as reusable old-flow UI

Do not load by default:

- full repository trees;
- unrelated epic documents;
- `.docs/legacy-analysis/`;
- `.codex/current-session-handoff.md`.
- Existing modified docs in the worktree are already known Codex-prep changes. Ignore
  them and do not edit them; they are not a blocker by themselves.

## Acceptance Focus

- The user immediately understands which product was scanned.
- The user immediately sees the relevant location/stock context.
- The screen has one obvious next action.
- The screen feels like a scan workcontext, not a management record page.
- Mobile and desktop both preserve clarity and low-noise hierarchy.
- The entire known-product route stays inside the new implementation and does not reuse
  old-flow pages/components anywhere the user can reach in this slice.
- Visible actions are operational, not illustrative.

## Test Evidence Requirements

- Name the production behavior each new test executes.
- Require real component rendering and user interaction through bUnit.
- Include tests that prove:
  - a known product scan from `/scan` lands in the new product-scan workcontext;
  - the new workcontext renders product identity and stock/location context;
  - the known-product success path no longer ends in `/scan/old`;
  - the primary action is present and visible in the rendered workcontext;
  - activating the primary action does not navigate into any old-flow page or generic
    legacy detailscreen.
- Add tests that prove:
  - selecting a location in the mutate flow navigates to a real new scan-specific
    mutation route that exists;
  - saving a mutation calls `StockService.MutateStockAsync(...)`;
  - saving add-stock calls `StockService.AddOrIncrementStockAsync(...)`;
  - add-stock does not merely navigate away without saving.
- Preserve or adjust old tests only where needed to keep `/scan/old` valid for temporary
  non-product fallback behavior.
- Forbid placeholder or documentary tests.

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

## Definition of Technical Completion

Report `ready for Codex review` only when:

- the known-product route from `/scan` lands in a new scan-specific product workcontext;
- the rendered screen follows the agreed UI direction with minimal information and a
  clear primary action;
- the reachable known-product continuation path also remains inside the new
  implementation and does not bounce into legacy pages/components;
- the visible continuation actions execute real stock behavior where they claim to do
  so;
- targeted tests pass and execute real behavior;
- build and `git diff --check` pass;
- no unexplained change exists outside the expected write-set;
- remaining manual acceptance steps are listed explicitly.

Report `not ready` when:

- the known-product route still ends in `/scan/old` or a generic old detailscreen;
- any reachable action inside the known-product path still routes into old-flow pages,
  old-flow components or generic legacy detail pages;
- a visible action still points to a non-existent route;
- an add/mutate form still submits without calling the required stock service method;
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
