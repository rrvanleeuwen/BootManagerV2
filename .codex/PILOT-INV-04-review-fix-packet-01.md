# Review Fix Packet

## Task

- Story ID: `PILOT-INV-04`
- Task type: gerichte correctieronde na Codex-review
- Required branch: `codex/pilot-inv-04-product-terugvinden`
- Source implementation packet: `.codex/PILOT-INV-04-implementation-packet.md`
- Goal: herstel uitsluitend de door Codex vastgestelde `PILOT-INV-04`-afwijkingen zodat
  de terugvindflow exact voldoet aan de goedgekeurde story: direct openen bij precies
  één actieve locatie, een locatielijst bij meerdere actieve locaties, en een duidelijke
  `geen actieve voorraad`-afhandeling met verwachte locatie en actie
  `Voorraad toevoegen`.

De bestaande `PILOT-INV-04`-implementatie bevat bruikbare bouwstenen en mag niet
opnieuw worden ontworpen. Dit is een minimale herstelronde, geen herinterpretatie van
de story en geen brede refactor.

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
- dit review-fix-packet;
- `.codex/PILOT-INV-04-implementation-packet.md`;
- alleen de sectie `PILOT-INV-04` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.Application/Inventory/Contracts/IProductService.cs`;
- `BootManager.Application/Inventory/Contracts/IStockService.cs`;
- `BootManager.Application/Inventory/Services/ProductService.cs`;
- `BootManager.Application/Inventory/Services/StockService.cs`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`;
- `BootManager.UnitTests/Inventory/ProductServiceTests.cs`;
- `BootManager.UnitTests/Inventory/StockServiceTests.cs`;
- `BootManager.IntegrationTests/Storage/NullStockService.cs`.

Lees geen brede source trees, TODO, legacy-analyse, handoff of ongerelateerde
featuredocumentatie.

## Defects To Fix

### Fix 1: Directe navigatie ontbreekt bij exact één actieve locatie in scanroute

#### Existing defect

De huidige scanroute toont bij precies één actieve voorraadlocatie eerst een tussenkaart
met een knop `Naar locatie`.

Dat schendt het acceptatiecriterium van `PILOT-INV-04`: bij een gescand product met
precies één actieve voorraadlocatie moet BootManager direct de locatiepagina openen,
zonder extra bevestigingsstap.

#### Required behavior

- Als `Scan.razor` een bekende productcode verwerkt en exact één actieve locatie vindt,
  navigeert de component direct naar `/storage/locations/{id}`.
- Er wordt in dit pad geen extra kaart, knop of tussenstap getoond.
- Het bestaande gedrag voor:
  - bekende locatie-QR;
  - meerdere actieve locaties;
  - geen actieve voorraad;
  blijft intact.

### Fix 2: Directe navigatie ontbreekt bij exact één actieve locatie in handmatige zoekroute

#### Existing defect

De huidige handmatige zoekroute in `Voorraadbeheer > Producten` toont na productkeuze
bij precies één actieve locatie ook eerst een tussenkaart met een knop `Naar locatie`.

Dat schendt hetzelfde acceptatiecriterium voor de fallbackroute: na handmatige selectie
van een product moet BootManager bij exact één actieve voorraadlocatie direct de
locatiepagina openen.

#### Required behavior

- Als de gebruiker in `Voorraadbeheer > Producten` een product kiest en exact één
  actieve locatie wordt gevonden, navigeert de component direct naar
  `/storage/locations/{id}`.
- Er wordt in dit pad geen extra detailkaart of bevestigingsknop getoond.
- De bestaande paden voor:
  - meerdere actieve locaties;
  - geen actieve voorraad;
  - verwachte locatie tonen;
  - actie `Voorraad toevoegen`;
  blijven intact.

### Fix 3: Testbewijs moet directe navigatie bewijzen, niet de huidige tussen-UI

#### Existing defect

De huidige tests bewijzen vooral dat de tussenkaart wordt getoond, terwijl de story
directe navigatie vereist. Daardoor kan de suite groen zijn terwijl de functionele eis
nog steeds fout staat.

#### Required behavior

- De scancomponenttests moeten expliciet asserten dat de `NavigationManager` direct naar
  de locatiepagina gaat bij exact één actieve locatie.
- Eventuele bestaande tests die de tussenkaart als gewenst gedrag bevestigen moeten
  worden aangepast of vervangen.
- De handmatige zoekroute moet eveneens een test hebben die directe navigatie bewijst,
  niet alleen render-output.

## Preserve These Behaviors

- Een bekende locatie-QR vanuit `Scannen` navigeert nog steeds direct naar de
  locatiepagina.
- Een product met meerdere actieve locaties toont nog steeds een locatielijst met
  gebied, locatienaam, hoeveelheid en eenheid.
- Een bekend product zonder actieve voorraad toont nog steeds een duidelijke melding.
- Als een verwachte locatie beschikbaar is, blijft die leesbaar zichtbaar als
  `StorageAreaName - StorageLocationName`.
- De actie `Voorraad toevoegen` blijft beschikbaar in de gevallen zonder directe
  enkelvoudige locatienavigatie.
- Zoekgedrag op naam en omschrijving, hoofdletterongevoelig en met deelmatches, blijft
  intact.
- De eerder toegevoegde service-methoden voor actieve voorraad en verwachte locatie
  mogen blijven bestaan als ze al correct zijn.

## Allowed Write-Set

Wijzig uitsluitend:

- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`;
- één gerichte aanvullende componenttest onder `BootManager.UnitTests/` voor de
  handmatige zoekroute, alleen als bestaande tests daar nog geen geschikt anker bieden;
- `BootManager.IntegrationTests/Storage/NullStockService.cs` alleen als een compile-time
  aanpassing nodig is door testuitbreiding;
- `BootManager.Application/Inventory/Contracts/IProductService.cs`,
  `BootManager.Application/Inventory/Contracts/IStockService.cs`,
  `BootManager.Application/Inventory/Services/ProductService.cs` en
  `BootManager.Application/Inventory/Services/StockService.cs` alleen wanneer een
  minimale aanpassing strikt nodig is om directe navigatie of testbaarheid correct af
  te handelen.

Laat alle andere bestanden ongemoeid tenzij een concrete compile-time dependency dat
aantoonbaar vereist.

## Explicitly Forbidden Changes

Wijzig niet:

- story-, release-, TODO-, legacy-, README-, handoff- of andere documentatie;
- de algemene opzet van `PILOT-INV-04`;
- scanroutering voor bekende locatie-QR's;
- inruimflowgedrag van `PILOT-INV-03` behalve als een minieme compile-time correctie
  onvermijdelijk is;
- migraties, database-schema, DI-registraties, package-references of projectstructuur;
- voorraadmutatielogica, verbruik, correcties, tellingen of historie;
- andere tests om failures te verbergen;
- brede UI-herbouw of nieuwe generieke zoek-/workflow-infrastructuur.

Maak geen branch, commit, push, PR, merge, release of deployment.

## Exact Change Rules

Pas deze regels letterlijk toe:

1. `Scan.razor`
   - Bij exact één actieve locatie: direct `NavigateTo(...)`.
   - Geen renderpad meer dat eerst een kaart met `Naar locatie` toont voor dit geval.
   - Het meervoudige-locatiepad en `geen actieve voorraad`-pad blijven bestaan.
2. `Products.razor`
   - Bij exact één actieve locatie na productkeuze: direct `NavigateTo(...)`.
   - Geen tussenkaart of extra knop voor dit geval.
   - De productresultatenlijst met omschrijving en locatiesamenvatting blijft bestaan.
3. Tests
   - Assert niet alleen op markup.
   - Assert expliciet op de uiteindelijke `NavigationManager.Uri` voor beide directe
     navigatiepaden.
   - Bewijs daarnaast dat de meervoudige-locatiepaden juist niet direct navigeren maar
     de locatielijst tonen.

## Test Evidence Requirements

Iedere nieuwe of gewijzigde test moet echte productcode of componenten uitvoeren en
concreet kunnen falen bij het bedoelde defect.

### Required scan tests

Bewijs minimaal:

- bekende locatie-QR blijft direct navigeren;
- bekende productcode met exact één actieve locatie navigeert direct naar de juiste
  locatiepagina;
- bekende productcode met meerdere actieve locaties navigeert niet direct maar toont de
  locatielijst;
- bekende productcode zonder actieve voorraad toont de melding en eventueel de
  verwachte locatie;
- de actie `Voorraad toevoegen` blijft beschikbaar in het `geen actieve voorraad`-pad.

### Required handmatige zoektests

Bewijs minimaal:

- handmatig zoeken vindt producten op naam en omschrijving;
- de eerste resultatenlijst toont geen hoeveelheden;
- na productkeuze met exact één actieve locatie navigeert de pagina direct naar de
  juiste locatiepagina;
- na productkeuze met meerdere actieve locaties toont de pagina een locatielijst en
  navigeert niet direct;
- na productkeuze zonder actieve voorraad toont de pagina de melding, verwachte locatie
  en actie `Voorraad toevoegen`.

Inspecteer iedere nieuwe/gewijzigde test:

- geen `Assert.True(true)`;
- geen lege test;
- geen bronvormtest als vervanging van gedrag;
- geen `async` test zonder relevante `await`.

## Required Checks

Voer eerst gerichte checks uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ScanComponentTests|FullyQualifiedName~ProductServiceTests|FullyQualifiedName~StockServiceTests|FullyQualifiedName~Products"
```

Als je een nieuwe componenttestklasse voor `Products.razor` toevoegt, gebruik dan de
echte klassenaam in de filter.

Voer daarna uit:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
git status --short
git diff --stat
```

Controleer na de testrun expliciet:

1. de scanroute-test voor exact één locatie assert op directe navigatie;
2. de handmatige zoektest voor exact één locatie assert op directe navigatie;
3. er geen wijziging buiten de toegestane write-set in `git status --short` staat.

## Definition of Technical Completion

Rapporteer alleen `ready for Codex review` wanneer:

- de scanroute bij exact één actieve locatie direct navigeert;
- de handmatige zoekroute bij exact één actieve locatie direct navigeert;
- de meervoudige-locatie- en `geen actieve voorraad`-paden intact blijven;
- alle vereiste tests defectgevoelig zijn en slagen;
- build en `git diff --check` slagen;
- geen verboden of onverklaarde wijziging is gemaakt;
- de worktree binnen de toegestane write-set blijft.

Rapporteer `not ready` wanneer een eis ontbreekt, een test nog alleen markup bewijst,
een nieuwe test faalt, de build/diffcheck faalt of extra write-area nodig blijkt.
Verlaag geen test- of acceptatie-eis en maskeer geen failure als waarschuwing.

## Completion Notes

Retourneer uitsluitend:

1. exacte gewijzigde bestanden;
2. hoe de directe navigatie in `Scan.razor` nu werkt;
3. hoe de directe navigatie in `Products.razor` nu werkt;
4. exacte nieuwe/gewijzigde testnamen en welk productiegedrag of defect zij uitvoeren;
5. alle test-, build- en diffcheckresultaten;
6. bevestiging dat geen verboden bestanden zijn gewijzigd;
7. eindstatus `ready for Codex review` of `not ready`, met concrete reden.

Noem de story niet `Done`, geaccepteerd of productierijp.
