# Implementation Packet

## Task

- Story ID: `PILOT-INV-04`
- Task type: gerichte follow-up binnen bestaande storyscope
- Required branch: `codex/pilot-inv-04-product-terugvinden`
- Source packet: `.codex/PILOT-INV-04-implementation-packet.md`
- Goal: vervang in de `geen actieve voorraad`-situatie van de product-terugvindflow de
  huidige navigatie naar het locatieoverzicht door een nieuwe compacte modal waarmee de
  gebruiker direct een locatie kiest en een hoeveelheid opgeeft, zonder eerst via het
  overzicht van locaties te hoeven navigeren.

Dit is geen brede redesign en geen herinterpretatie van de story. Houd de wijziging
klein, taakgericht en consistent met de bestaande inventory-UI.

## Mandatory Start Check

Controleer vóór iedere wijziging:

1. De actieve branch is exact `codex/pilot-inv-04-product-terugvinden` en niet
   `master`.
2. De bestaande `PILOT-INV-04`-implementatie staat nog on-gecommit in de worktree.
3. De index bevat geen onverwachte staged wijzigingen.

Stop en rapporteer `not ready` wanneer de branch niet klopt of de bestaande
implementatie ontbreekt. Reset, checkout, stash of verwijder geen bestaande
worktreewijzigingen.

## Minimal Context

Lees uitsluitend:

- `CLAUDE.md`;
- dit packet;
- `.codex/PILOT-INV-04-implementation-packet.md`;
- alleen de sectie `PILOT-INV-04` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Web/Components/Inventory/AddStockDialog.razor`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- `BootManager.Application/Storage/Services/IStorageService.cs`;
- `BootManager.Application/Storage/Services/StorageService.cs`;
- `BootManager.Application/Storage/DTOs/StorageLocationOverviewDto.cs`;
- `BootManager.Application/Inventory/Contracts/IStockService.cs`;
- `BootManager.Application/Inventory/Services/StockService.cs`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`;
- `BootManager.UnitTests/Storage/StorageLocationDetailsWithStockComponentTests.cs`.

Lees geen brede source trees, TODO, legacy-analyse, handoff of ongerelateerde
featuredocumentatie.

## Required Behavior

- In de `geen actieve voorraad`-situatie blijft de actie `Voorraad toevoegen`
  beschikbaar in zowel:
  - de scanroute;
  - de handmatige zoekroute in `Voorraadbeheer > Producten`.
- Die actie mag niet meer naar het locatieoverzicht navigeren als primaire vervolgstap.
- In plaats daarvan opent die actie een nieuwe modal of een nieuwe kleine herbruikbare
  component in modalvorm.
- Die nieuwe modal moet sterk lijken op `AddStockDialog` qua opzet en visuele stijl,
  maar is functioneel niet dezelfde flow.
- De nieuwe modal ondersteunt minimaal:
  - tonen van het geselecteerde product;
  - kiezen van een opslaglocatie uit bestaande locaties;
  - invoeren van een hoeveelheid;
  - opslaan via de bestaande additieve voorraadservice;
  - sluiten/annuleren zonder side effects.
- Als er voor het product een verwachte locatie bekend is, mag die in de modal visueel
  als voorstel of voorkeurskeuze worden getoond, maar de gebruiker moet nog steeds een
  locatie kunnen kiezen.
- Na succesvol opslaan moet de gebruiker logisch verderkomen:
  - bij scanroute: navigeer naar de gekozen locatiepagina;
  - bij handmatige zoekroute: navigeer naar de gekozen locatiepagina.

## Important Clarification

- Gebruik `AddStockDialog.razor` niet direct als oplossing voor deze situatie.
- Maak ook geen tweede volledig afwijkende UX.
- Maak een nieuwe modal die sterk op `AddStockDialog` lijkt in layout en interactiestijl,
  maar specifiek bedoeld is voor:
  - product is al bekend;
  - locatie is nog niet gekozen;
  - gebruiker moet direct locatie + hoeveelheid vastleggen.

## Scope

- Nieuwe compacte modal/component voor `product zonder actieve voorraad -> voorraad
  toevoegen`.
- Integratie van die modal in:
  - `Scan.razor`;
  - `Products.razor`.
- Hergebruik waar logisch bestaande services en bestaande opslaglocatie-data.
- Gebruik de bestaande additieve voorraadlogica (`AddOrIncrementStockAsync`) voor
  opslaan.

## Outside Scope

- Geen wijziging van bestaande `AddStockDialog` voor de locatiepaginaflow, behalve
  minieme styling- of helperextractie als dat strikt nodig is.
- Geen brede refactor van inventory-pagina's.
- Geen wijziging van de scan-inruimflow van `PILOT-INV-03`.
- Geen migraties, database-schema-aanpassingen, DI-verbreding of package-wijzigingen.
- Geen documentatie-updates.
- Geen commit, push, branch, PR, merge, release of deployment.

## Allowed Write-Set

Wijzig uitsluitend:

- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- één nieuwe component onder `BootManager.Web/Components/Inventory/` voor deze modal;
- optioneel een kleine shared helper of kleine aanpassing in
  `BootManager.Web/Components/Inventory/AddStockDialog.razor` als dat nodig is om
  styling/opmaak consistent te houden, maar verander de bestaande locatiepaginaflow niet
  functioneel;
- `BootManager.Application/Storage/Services/IStorageService.cs` en
  `BootManager.Application/Storage/Services/StorageService.cs` alleen als een kleine
  ondersteunende locatie-ophaalactie nog ontbreekt;
- `BootManager.Application/Storage/DTOs/StorageLocationOverviewDto.cs` alleen als de
  modal daarvoor echt een bestaand veld mist;
- gerichte tests onder `BootManager.UnitTests/Inventory/` en
  `BootManager.UnitTests/Storage/`.

Laat alle andere bestanden ongemoeid tenzij een concrete compile-time dependency dat
aantoonbaar vereist.

## Explicitly Forbidden Changes

Wijzig niet:

- story-, release-, TODO-, legacy-, README-, handoff- of andere documentatie;
- algemene product-terugvindlogica buiten deze `Voorraad toevoegen`-vervolgstap;
- bekende locatie-QR-routing;
- directe navigatie bij precies één actieve locatie;
- meervoudige-locatiegedrag;
- bestaande `AddStockDialog`-locatiepaginaflow in betekenis;
- voorraadmutatietypen anders dan bestaande additieve toevoeging;
- projectstructuur, migraties, packages of architectuurlagen.

Maak geen branch, commit, push, PR, merge, release of deployment.

## Exact Implementation Rules

1. Maak een nieuwe modalcomponent voor deze situatie; hergebruik de bestaande modal niet
   als eindoplossing.
2. De nieuwe modal moet visueel en interactioneel duidelijk familie zijn van
   `AddStockDialog`.
3. De modal verwacht een bekend `ProductDto` of product-id als invoer en laat daarna de
   locatie kiezen.
4. De modal toont geen productzoekstap; het product staat al vast.
5. De modal moet een locatiekeuze tonen op basis van bestaande opslaglocaties.
6. De modal moet hoeveelheidinvoer tonen met de standaardeenheid van het product.
7. De modal moet via bestaande services opslaan op de gekozen locatie.
8. Na succesvol opslaan sluit de modal en navigeert de caller naar de gekozen
   locatiepagina.
9. Als opslaan mislukt, blijft de modal open en toont een duidelijke foutmelding.
10. Als een verwachte locatie beschikbaar is, mag die voorgeselecteerd of opvallend
    getoond worden, maar niet hard geforceerd worden.

## Test Evidence Requirements

Iedere nieuwe of gewijzigde test moet echte componentinteractie of echte servicecalls
uitvoeren en concreet kunnen falen bij het bedoelde defect.

### Required tests

Bewijs minimaal:

- in `Scan.razor` opent `Voorraad toevoegen` bij `geen actieve voorraad` nu de nieuwe
  modal in plaats van navigatie naar het locatieoverzicht;
- in `Products.razor` opent `Voorraad toevoegen` bij `geen actieve voorraad` nu de
  nieuwe modal in plaats van navigatie naar het locatieoverzicht;
- de modal toont het geselecteerde product en laat een locatie kiezen;
- de modal toont hoeveelheidinvoer met de standaardeenheid van het product;
- succesvol opslaan roept `AddOrIncrementStockAsync` aan met product, gekozen locatie
  en hoeveelheid;
- succesvol opslaan navigeert daarna naar de gekozen locatiepagina;
- annuleren sluit de modal zonder opslaan;
- een fout bij opslaan blijft zichtbaar in de modal en navigeert niet;
- bestaand `AddStockDialog`-gedrag vanaf `StorageLocationDetails.razor` blijft
  functioneel intact.

Inspecteer iedere nieuwe/gewijzigde test:

- geen `Assert.True(true)`;
- geen lege test;
- geen bronvormtest als vervanging van gedrag;
- geen `async` test zonder relevante `await`.

## Required Checks

Voer eerst gerichte checks uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ScanComponentTests|FullyQualifiedName~ProductsComponentTests|FullyQualifiedName~StorageLocationDetailsWithStockComponentTests"
```

Voer daarna uit:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
git status --short
git diff --stat
```

Controleer na de testrun expliciet:

1. `Voorraad toevoegen` in beide terugvindroutes opent de modal en niet het
   locatieoverzicht;
2. succesvol opslaan navigeert naar de gekozen locatie;
3. de bestaande locatiepaginaflow met `AddStockDialog` niet kapot is gegaan;
4. er geen wijziging buiten de toegestane write-set in `git status --short` staat.

## Definition of Technical Completion

Rapporteer alleen `ready for Codex review` wanneer:

- de nieuwe modal bestaat en in beide terugvindroutes correct gebruikt wordt;
- de modal locatiekeuze en hoeveelheidinvoer ondersteunt;
- succesvol opslaan additief werkt via bestaande voorraadservice;
- na succesvol opslaan naar de gekozen locatiepagina wordt genavigeerd;
- de bestaande `AddStockDialog`-flow op de locatiepagina intact blijft;
- alle vereiste tests defectgevoelig zijn en slagen;
- build en `git diff --check` slagen;
- geen verboden of onverklaarde wijziging is gemaakt.

Rapporteer `not ready` wanneer de oplossing toch naar het locatieoverzicht navigeert,
de bestaande modal direct is hergebruikt zonder locatiekeuze, een test alleen markup
bewijst waar navigatie of servicecall vereist is, een nieuwe test faalt, build/diffcheck
faalt of extra write-area nodig blijkt.

## Completion Notes

Retourneer uitsluitend:

1. exacte gewijzigde bestanden;
2. naam en rol van de nieuwe modalcomponent;
3. hoe `Scan.razor` de modal nu opent;
4. hoe `Products.razor` de modal nu opent;
5. exacte nieuwe/gewijzigde testnamen en welk productiegedrag zij uitvoeren;
6. alle test-, build- en diffcheckresultaten;
7. bevestiging dat de bestaande `AddStockDialog`-locatiepaginaflow functioneel intact
   is gebleven;
8. eindstatus `ready for Codex review` of `not ready`, met concrete reden.
