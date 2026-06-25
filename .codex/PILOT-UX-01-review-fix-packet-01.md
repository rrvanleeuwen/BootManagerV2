# Review Fix Packet

## Task

- Story ID: `PILOT-UX-01`
- Fix ID: `PILOT-UX-01-review-fix-01`
- Goal: herstel de twee functionele routeblokkades in de nieuwe homepagina en maak de
  home-tests sterker door de relevante interacties via echte UI-paden te laten lopen
- Required branch: `codex/pilot-ux-01-home-hub`

The original story is already approved. Do not restate it or ask for approval. Give a
short plan, implement directly, run the checks, and provide completion notes.

Claude must stop and report `not ready` when the active branch is `master` or does not
match the required branch.

## Defects To Fix

1. De `Logboek`-tegel op home linkt naar `/logs`, terwijl de bestaande logboekpagina op
   `/logbook` zit.
2. De actie `Voorraad toevoegen` in het home-pad `geen actieve voorraad` navigeert naar
   `/inventory/add-stock?productId=...`, maar daarvoor bestaat geen pagina of route.
   Dit pad moet landen op een bestaand werkend vervolgpad.
3. De nieuwe home-tests gebruiken reflection om `PerformSearch` direct aan te roepen in
   plaats van de bedoelde UI-interactie te bewijzen. Daardoor beschermen ze de
   gebruikersflow onvoldoende.

## Scope

- Corrigeer de `Logboek`-tegel zodat deze naar de bestaande logboekroute navigeert.
- Corrigeer het `Voorraad toevoegen` pad vanuit home zodat het een bestaand, werkend
  vervolg gebruikt.
- Volg daarbij bestaand gedrag uit de productzoekflow:
  - hergebruik bij voorkeur dezelfde modal- of componentroute als `Products.razor`
    wanneer dat binnen een kleine lokale aanpassing kan;
  - introduceer geen nieuwe `/inventory/add-stock` pagina alleen om deze fix te laten
    slagen.
- Versterk de home-tests zodat ze de relevante UI-interacties echt uitvoeren in plaats
  van private methodes via reflection aan te roepen.
- Voeg regressiebewijs toe voor:
  - correcte `Logboek`-link;
  - werkend `Voorraad toevoegen` vervolgpad;
  - zoekinteractie via de bedoelde UI-trigger.

## Outside Scope

- Geen bredere herbouw van de home-layout.
- Geen nieuwe inventorypagina's, routes of scanwijzigingen.
- Geen documentatie-, commit-, push-, branch-, PR-, merge- of deploymentacties.
- Geen uitbreiding naar `PILOT-INV-06`.

## Expected Write-Set

Only change these files unless a required compile-time dependency is discovered:

- `BootManager.Web/Components/Pages/Home.razor`
- optioneel `BootManager.Web/Components/Pages/Home.razor.css`
- `BootManager.UnitTests/Inventory/HomeComponentTests.cs`

Do not change by default:

- `BootManager.Web/Components/Pages/Inventory/Products.razor`
- application services
- routing or navigation outside the home fix
- story/docs/handoff/README/TODO/legacy files

Before changing an additional area, explain why it is required.

## Minimal Context

Read:

- `CLAUDE.md`
- `.codex/PILOT-UX-01-review-fix-packet-01.md`
- `.codex/PILOT-UX-01-implementation-packet.md`
- `.codex/claude-sources/ux/PILOT-UX-01.md`
- `BootManager.Web/Components/Pages/Home.razor`
- `BootManager.Web/Components/Pages/Inventory/Products.razor`
- `BootManager.Web/Components/Inventory/AddStockToProductModal.razor`
- `BootManager.UnitTests/Inventory/HomeComponentTests.cs`
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`
- `BootManager.Web/Components/Pages/Logbook.razor`

Do not load by default:

- `AGENTS.md`
- `.codex/current-session-handoff.md`
- `.codex/working-agreement.md`
- full `.docs/TODO.md`
- `.docs/legacy-analysis/`
- unrelated source trees

## Existing Constraints

- De homepagina moet mockup-geleid blijven; los de defecten lokaal op zonder de hele
  pagina opnieuw te ontwerpen.
- Het `Voorraad toevoegen` pad moet een bestaand werkend gedrag gebruiken. Een dood pad
  of niet-bestaande route is niet acceptabel.
- De tests moeten echte UI-interactie uitvoeren met het bestaande bUnit-framework.
- Een test die alleen slaagt door private methodes direct aan te roepen is hier geen
  voldoende regressiebewijs voor de gebruikersflow.

## Test Evidence Requirements

- Lever red-green-bewijs of gelijkwaardig defectgevoelig bewijs voor beide routefixes.
- Nieuwe of aangepaste tests moeten concreet bewijzen:
  - de home `Logboek`-tegel wijst naar `/logbook`;
  - het `Voorraad toevoegen` pad vanaf home landt op het bedoelde bestaande vervolgpad;
  - zoeken wordt via echte UI-interactie gestart, niet via reflection op
    `PerformSearch`.
- Gebruik echte componentrendering en gebruikersinteractie met bUnit.
- Geen `Assert.True(true)`, lege tests of documentaire assertions.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~HomeComponentTests"
```

If shared inventory behavior is touched, also run:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductsComponentTests"
```

Then run:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
```

## Definition of Technical Completion

Report `ready for Codex review` only when:

- both functional route defects are fixed;
- the new home tests prove the relevant UI paths without reflection-based private method
  invocation;
- targeted tests pass;
- build and `git diff --check` pass;
- no unexplained changes exist outside the expected write-set.

Report `not ready` when any route still points to a dead path, the add-stock path still
does not use a real existing flow, tests still rely on private method invocation for
the reviewed interaction, checks fail, or the write-set expands without justification.

## Completion Notes

Return only:

1. changed files and fixed behavior;
2. tests/checks and results;
3. exact new/changed test names and the behavior they execute;
4. remaining risks and manual retest notes;
5. final status: `ready for Codex review` or `not ready`.
