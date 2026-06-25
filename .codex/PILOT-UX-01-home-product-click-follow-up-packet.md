# Implementation Packet

## Task

- Story ID: `PILOT-UX-01`
- Follow-up ID: `PILOT-UX-01-home-product-click-follow-up`
- Goal: verander uitsluitend het klikgedrag van een product in de homezoekresultaten,
  zodat home niet meer direct naar een locatie(flow) gaat maar eerst een
  productgerichte context opent van waaruit verbruik direct bereikbaar is
- Required branch: `codex/pilot-ux-01-home-hub`

The story is already approved. Do not restate it or ask for approval. Give a short
plan, implement directly, run the checks, and provide completion notes.

Claude must stop and report `not ready` when the active branch is `master` or does not
match the required branch.

## Scope

- Wijzig alleen het klikgedrag van producten in de zoekresultaten op home.
- Een klik op een product in home mag niet meer direct:
  - naar een locatiepagina navigeren; of
  - eindigen op een pure locatiekeuzestaat als primaire werkcontext.
- Open vanuit home eerst een productgerichte pagina of werkcontext.
- Die productcontext toont minimaal:
  - productnaam;
  - totale hoeveelheid;
  - eenheid;
  - relevante locaties;
  - een duidelijke directe actie om verbruik te registreren.
- Als meerdere locaties bestaan, blijven die zichtbaar als context van het product,
  maar de primaire ervaring blijft product-eerst.
- Als geen actieve voorraad bestaat, blijft verwachte locatie-informatie bruikbaar waar
  relevant, maar nog steeds binnen productcontext.

## Hard Boundary

- Deze wijziging geldt **alleen** voor klikken op een product in home.
- Andere entry points mogen niet functioneel van gedrag veranderen:
  - `Voorraadbeheer > Producten`
  - scanflows
  - bestaande locatiepagina’s
  - bestaande mutatiepagina’s
- Die andere plekken mogen alleen als bestaand vervolgpad, hergebruikte component of
  bestaande doelpagina dienen als dat de homefollow-up helpt.
- Pas geen andere gebruikersflow stilzwijgend aan om deze homewens mee te nemen.

## Outside Scope

- Geen brede herbouw van home-layout buiten wat nodig is voor deze klikflow.
- Geen redesign van `Voorraadbeheer > Producten`.
- Geen wijziging van scanroutering of scancontext.
- Geen generieke productdetailarchitectuur voor de hele app als een kleine lokale
  oplossing volstaat.
- Geen documentatie-, commit-, push-, branch-, PR-, merge-, release- of
  deploymentacties.

## Expected Write-Set

Only change these files or modules unless a required compile-time dependency is
discovered:

- `BootManager.Web/Components/Pages/Home.razor`
- optioneel `BootManager.Web/Components/Pages/Home.razor.css`
- optioneel een klein nieuw home-specifiek of gedeeld presentatiedeel onder
  `BootManager.Web/Components/`
- optioneel een bestaand product- of mutatiecomponent als bestaand vervolgpad, maar
  alleen wanneer de homefollow-up dat direct vereist
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
- `.codex/PILOT-UX-01-home-product-click-follow-up-packet.md`
- `.codex/PILOT-UX-01-user-preference-update-product-click-flow.md`
- `.codex/PILOT-UX-01-implementation-packet.md`
- `.codex/claude-sources/ux/PILOT-UX-01.md`
- `BootManager.Web/Components/Pages/Home.razor`
- `BootManager.Web/Components/Pages/Inventory/Products.razor`
- `BootManager.Web/Components/Pages/Inventory/StockMutations.razor`
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`
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

- De notitie `.codex/PILOT-UX-01-user-preference-update-product-click-flow.md` is
  leidend voor deze follow-up.
- De huidige homeflow behandelt productklik nog als locatie-eerst; dat moet juist
  vervangen worden.
- De gewenste UX-correctie geldt alleen voor home.
- Houd de oplossing klein en lokaal als dat kan; vermijd brede ripple-effects.

## Acceptance Focus

- Klikken op een product in home opent een productgerichte context.
- Die productcontext voelt niet als een vermomde locatiepagina.
- Verbruik registreren is direct bereikbaar vanuit die productcontext.
- Productinformatie en locatiecontext zijn samen zichtbaar.
- Het gedrag van `Voorraadbeheer > Producten` blijft functioneel ongewijzigd.
- Het gedrag van scanflows blijft functioneel ongewijzigd.

## Test Evidence Requirements

- Name the production behavior each new test executes.
- Use real bUnit rendering and user interaction.
- Include tests that prove:
  - clicking a product in home no longer navigates directly to a location page;
  - clicking a product in home opens the new product-first context;
  - the product-first context shows product information plus location context;
  - the direct action to register consumption is present and routes to the intended
    existing follow-up path;
  - `Products.razor` behavior remains unchanged when not explicitly modified.
- Do not rely on documentary assertions or private-method invocation as proof of the
  user flow.

## Required Checks

Run targeted checks first:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~HomeComponentTests"
```

If any shared inventory component or behavior is touched, also run:

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

- the home product click flow is product-first;
- home no longer jumps directly from product result to location page;
- direct consumption action is present from the product-first context;
- non-home entry points remain functionally unchanged;
- targeted tests pass;
- build and `git diff --check` pass;
- no unexplained changes exist outside the expected write-set.

Report `not ready` when the change still routes home clicks directly to location,
changes other entry points, lacks direct consumption access, fails checks, or expands
outside the intended area without justification.

## Completion Notes

Return only:

1. changed files and implemented behavior;
2. tests/checks and results;
3. exact new/changed test names and the behavior they execute;
4. remaining risks and manual retest notes;
5. final status: `ready for Codex review` or `not ready`.
