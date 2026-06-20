# Review Correction Packet

## Task

- Story ID: `PILOT-INV-02`
- Correction round: `review-correction-01`
- Required branch: `feature/pilot-inv-02-location-stock-basics`
- Goal: los uitsluitend de door Codex vastgestelde reviewbevindingen op in de nieuwe
  voorraadbasis, zonder scope-uitbreiding of nieuwe zijpaden.

De oorspronkelijke story en het implementation packet zijn al goedgekeurd. Formuleer
de story niet opnieuw. Geef een kort plan, implementeer direct alleen deze correcties,
voer de checks uit en rapporteer exact volgens `Completion Notes`.

## Fix Scope

Los alleen deze punten op:

1. Zorg dat de nieuwe voorraad-dialog compileerbaar en resolveerbaar is vanuit
   `StorageLocationDetails.razor`.
2. Implementeer de goedgekeurde flow `Nieuw product aanmaken` vanuit de locatiecontext,
   zodat de gebruiker daarna terugkeert naar dezelfde locatieflow met het nieuw
   aangemaakte product geselecteerd.
3. Repareer `StockService` zodat DTO-mapping niet afhankelijk is van ongeladen
   navigatie-eigenschappen voor opslaggebied, eenheid of gekoppelde code.
4. Zorg dat product zoeken op gekoppelde code echt werkt met de feitelijke
   persistence-/repositoryopzet van dit project.
5. Repareer het migratiebewijs zodat het expliciet de upgrade vanaf migratie
   `20260620120948_AddInventoryEntities` bewijst met bestaande data vóór de upgrade.

## Out Of Scope

- Geen nieuwe functionele uitbreiding buiten bovenstaande vijf punten.
- Geen UI-herontwerp, geen extra UX-verbeteringen, geen scanflow, geen historie, geen
  verbruik/correcties, geen extra filters of suggesties.
- Geen refactor van repositoryarchitectuur, geen nieuwe generieke data-accesslaag en
  geen vervanging van bestaande servicestructuur.
- Geen wijzigingen aan README, release-, TODO-, legacy-, handoff- of andere
  projectdocumentatie.
- Geen commits, pushes, branches, PR's, merges, releases of deployments.

## Allowed Write-Set

Beperk wijzigingen tot alleen wat nodig is binnen deze gebieden:

- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`
- `BootManager.Web/Components/Inventory/AddStockDialog.razor`
- eventueel direct benodigde bestaande inventory-componenten voor het terugkeerpad van
  productaanmaak binnen locatiecontext
- `BootManager.Web/Components/_Imports.razor` alleen als dat de kleinste correcte fix is
  voor componentresolutie
- `BootManager.Application/Inventory/Services/StockService.cs`
- eventueel direct benodigde inventory-contracten/DTO's die onmisbaar zijn voor de fix
- eventueel direct benodigde storage DTO/servicecode die al door deze slice geraakt werd
- `BootManager.IntegrationTests/Inventory/StockMigrationTests.cs`
- gerichte unit/componenttests die deze correcties bewijzen

Wijzig niets buiten deze write-set tenzij een compile-time dependency dat dwingend
vereist; meld dat dan expliciet in je oplevering.

## Execution Boundaries

- Controleer vóór bewerken dat de actieve branch exact
  `feature/pilot-inv-02-location-stock-basics` is en niet `master`.
- Werk uitsluitend aan deze correctieronde; raak geen andere open punten aan.
- Als een punt niet oplosbaar blijkt zonder bredere scope, stop en rapporteer `niet
  gereed` met precies dat ene ontbrekende besluit.
- Verberg geen defect door UI-tekst aan te passen terwijl het onderliggende gedrag nog
  fout is.
- Houd bestaande afgeronde `PILOT-INV-01`-regels intact.

## Minimal Context

Lees alleen:

- `CLAUDE.md`
- `.codex/PILOT-INV-02-implementation-packet.md`
- `.codex/PILOT-INV-02-review-correction-packet-01.md`
- `.codex/claude-sources/inventory/PILOT-INV-02.md`
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`
- `BootManager.Web/Components/Inventory/AddStockDialog.razor`
- `BootManager.Web/Components/Pages/Inventory/Products.razor` alleen voor bestaand
  productbeheer/hergebruik
- `BootManager.Web/Components/_Imports.razor`
- `BootManager.Application/Inventory/Services/StockService.cs`
- `BootManager.Application/Inventory/Services/ProductService.cs`
- `BootManager.Core/Interfaces/IRepository.cs`
- `BootManager.Infrastructure/Repositories/EfRepository.cs`
- `BootManager.IntegrationTests/Inventory/StockMigrationTests.cs`
- direct relevante bestaande tests rond storage details, inventory products en stock

Lees geen bredere documentatie of ongerelateerde source trees.

## Required Behavior

- De voorraad-dialog moet echt compileerbaar zijn vanuit de locatiepagina.
- `Nieuw product aanmaken` mag geen placeholder of dode knop meer zijn.
- Na succesvolle productaanmaak vanuit locatiecontext moet de gebruiker terugkeren naar
  dezelfde locatieflow met dat nieuwe product geselecteerd, zodat alleen hoeveelheid en
  opslaan nog resteren.
- Zoeken op productnaam én gekoppelde code moet werken tegen echte productdata in deze
  codebase; geen schijnimplementatie die alleen werkt als navigatie-eigenschappen
  toevallig geladen zijn.
- `StockDto`-mapping moet robuust zijn wanneer repositories losse entiteiten zonder
  eager-loaded navigaties teruggeven.
- Het migratiebewijs moet aantonen:
  - expliciete start op migratie `20260620120948_AddInventoryEntities`;
  - bestaande data invoegen vóór upgrade;
  - upgrade naar latest;
  - bestaande data blijft behouden;
  - nieuwe `Stocks`-tabel en unieke `product + locatie` constraint werken daarna.

## Test Evidence Requirements

Voeg of wijzig alleen tests die direct deze correcties bewijzen:

- componenttest voor render/resolve van de locatiepagina met voorraad-dialog;
- componenttest voor de nieuwe productaanmaak-terugkeerflow in locatiecontext;
- unittest of equivalent bewijs dat zoeken op gekoppelde code echt werkt;
- unittest of equivalent bewijs dat stock-mapping niet breekt zonder geladen navigaties;
- integratietest voor expliciete upgrade vanaf `20260620120948_AddInventoryEntities`.

De tests moeten echte productcode/componenten uitvoeren en concreet assertief zijn.
Geen placeholdertests, geen `Assert.True(true)`, geen commentaar als bewijs.

## Required Checks

Voer minimaal uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~Stock|FullyQualifiedName~StorageLocationDetails|FullyQualifiedName~Products"
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~StockMigration"
dotnet build BootManager.sln --no-restore
git diff --check
```

## Definition Of Ready For Review

Meld alleen `gereed voor Codex-review` wanneer alle vijf fixpunten aantoonbaar opgelost
zijn, de nieuwe tests dat bewijs leveren, de migratie-upgradetest echt vanaf de vorige
migratie vertrekt en build plus diffcheck slagen.

Meld anders `niet gereed` met de concrete resterende blokkade.

## Completion Notes

Retourneer alleen:

1. gewijzigde bestanden en exact welk reviewpunt elk bestand oplost;
2. uitgevoerde tests/checks en resultaten;
3. exacte nieuwe of gewijzigde testnamen en welk productiegedrag ze bewijzen;
4. eventuele noodzakelijke write-set-uitbreiding met reden;
5. eindstatus: `gereed voor Codex-review` of `niet gereed`.
