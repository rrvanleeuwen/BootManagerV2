# Review Fix Packet

## Task

- Story ID: `PILOT-UX-01`
- Fix ID: `PILOT-UX-01-home-product-consumption-direct-fix`
- Goal: maak de home-productcontext af zodat `Verbruik registreren` het gekozen
  product direct meeneemt naar een bruikbare verbruiksflow, in plaats van de gebruiker
  opnieuw bij algemene productselectie te laten beginnen
- Required branch: `codex/pilot-ux-01-home-hub`

The story is already approved. Do not restate it or ask for approval. Give a short
plan, implement directly, run the checks, and provide completion notes.

Claude must stop and report `not ready` when the active branch is `master` or does not
match the required branch.

## Defect To Fix

De huidige home-productcontext toont wel een knop `Verbruik registreren`, maar die
navigeert naar de generieke mutatiepagina waar de gebruiker opnieuw een product moet
selecteren. Daarmee is de verbruiksactie niet echt direct vanuit home-productcontext.

## Scope

- Behoud de huidige productgerichte homeklik-context.
- Laat `Verbruik registreren` vanaf die context landen in een verbruiksflow waarin het
  geselecteerde product al vaststaat.
- De gebruiker mag vanuit home dus niet opnieuw bij stap `product selecteren`
  uitkomen voor hetzelfde gekozen product.
- Een bestaande mutatiepagina of component mag hiervoor worden uitgebreid met een kleine
  gerichte preselectieflow als dat het kleinste veilige pad is.
- Toon of gebruik daarna nog steeds de relevante locatiecontext voor het gekozen
  product.

## Hard Boundary

- Deze fix geldt alleen voor de home-productcontext en het directe vervolg op
  `Verbruik registreren`.
- Verander het normale gedrag van andere entry points niet:
  - `Voorraadbeheer > Producten`
  - de generieke mutatiepagina wanneer die buiten home wordt geopend
  - scanflows
- Voeg geen brede nieuwe productdetailarchitectuur of nieuwe losse flowfamilie toe als
  een kleine gerichte uitbreiding van bestaand gedrag volstaat.

## Outside Scope

- Geen herontwerp van de home-layout.
- Geen redesign van `Voorraadbeheer > Producten`.
- Geen wijziging van scanroutering.
- Geen documentatie-, commit-, push-, branch-, PR-, merge-, release- of
  deploymentacties.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

- `BootManager.Web/Components/Inventory/ProductDetailsFromHome.razor`
- `BootManager.Web/Components/Pages/Inventory/StockMutations.razor`
- optioneel `BootManager.Web/Components/Pages/Home.razor` alleen als een kleine
  parameter- of routeoverdracht daar noodzakelijk is
- gerichte tests onder `BootManager.UnitTests/Inventory/`

Do not change by default:

- `BootManager.Web/Components/Pages/Inventory/Products.razor`
- scanpagina's
- locatiepagina’s
- application services
- documentatiebestanden

Before changing an additional area, explain why it is required.

## Minimal Context

Read:

- `CLAUDE.md`
- `.codex/PILOT-UX-01-home-product-consumption-direct-fix-packet.md`
- `.codex/PILOT-UX-01-home-product-click-follow-up-packet.md`
- `.codex/PILOT-UX-01-user-preference-update-product-click-flow.md`
- `BootManager.Web/Components/Inventory/ProductDetailsFromHome.razor`
- `BootManager.Web/Components/Pages/Inventory/StockMutations.razor`
- `BootManager.Web/Components/Pages/Home.razor`
- `BootManager.UnitTests/Inventory/HomeComponentTests.cs`
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`

Do not load by default:

- `AGENTS.md`
- `.codex/current-session-handoff.md`
- `.codex/working-agreement.md`
- full `.docs/TODO.md`
- `.docs/legacy-analysis/`
- unrelated source trees

## Existing Constraints

- De gebruikerswens blijft leidend: alleen homeklikgedrag en direct homeverbruikspad
  aanpassen.
- De gekozen home-productcontext blijft product-eerst.
- `Verbruik registreren` moet nu concreet productdirect zijn, niet alleen visueel
  aanwezig.
- Andere entry points moeten functioneel hetzelfde blijven.

## Test Evidence Requirements

- Name the production behavior each new test executes.
- Use real bUnit rendering and user interaction.
- Include tests that prove:
  - clicking `Verbruik registreren` from the home product context no longer lands in a
    generic “select product” start for that same action;
  - the chosen product is already fixed or preselected in the resulting mutation flow;
  - the resulting flow is immediately usable for registering consumption;
  - the generic mutation page still works normally when opened outside home.
- Do not rely on documentary assertions or visibility-only checks for the critical
  action.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~HomeComponentTests|FullyQualifiedName~StockMutations"
```

If the actual mutation test class name differs, use the real class name in the filter.

Then run:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
```

## Definition of Technical Completion

Report `ready for Codex review` only when:

- `Verbruik registreren` from home carries the selected product directly into a usable
  mutation flow;
- the user does not have to reselect the product for that home-started action;
- non-home entry points remain functionally unchanged;
- targeted tests pass;
- build and `git diff --check` pass;
- no unexplained changes exist outside the expected write-set.

Report `not ready` when the home action still drops the user into generic product
selection, the product is not preselected/fixed, the flow is not directly usable for
consumption, checks fail, or the change spills into unrelated entry points.

## Completion Notes

Return only:

1. changed files and fixed behavior;
2. tests/checks and results;
3. exact new/changed test names and the behavior they execute;
4. remaining risks and manual retest notes;
5. final status: `ready for Codex review` or `not ready`.
