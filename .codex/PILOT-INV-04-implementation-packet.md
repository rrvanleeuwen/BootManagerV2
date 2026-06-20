# Implementation Packet

## Task

- Story ID: `PILOT-INV-04`
- Approved story: product terugvinden via scan of zoeken
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-INV-04`
- Goal: breid de bestaande inventory- en scanbasis uit zodat Owner en Crew een bekend product snel kunnen terugvinden via `Scannen` of via handmatig zoeken in `Voorraadbeheer > Producten`, waarna BootManager direct de juiste locatiepagina opent bij exact één actieve locatie, of een compacte locatielijst toont bij meerdere actieve locaties, inclusief duidelijke afhandeling voor producten zonder actieve voorraad maar met een verwachte laatst gebruikte locatie.
- Required branch: `codex/pilot-inv-04-product-terugvinden`

De story is al goedgekeurd. Formuleer haar niet opnieuw en vraag geen nieuw akkoord.
Geef een kort uitvoeringsplan, implementeer direct, voer de checks uit en rapporteer
volgens `Completion Notes`.

## Scope

- Behoud `Scannen` als primaire start voor de scanroute.
- Laat een bekende productcode in `Scannen` direct de terugvindflow starten in plaats
  van de inruimflow van `PILOT-INV-03`.
- Behoud bestaand gedrag waarbij een bekende locatie-QR direct naar de locatiepagina
  navigeert.
- Gebruik `Voorraadbeheer > Producten` als handmatige fallback voor productzoeken.
- Ondersteun handmatig zoeken op productnaam en productomschrijving.
- Maak handmatig zoeken hoofdletterongevoelig en gebaseerd op deelmatches.
- Toon bij meerdere zoekresultaten een compacte productresultatenlijst met minimaal:
  - productnaam;
  - eerste omschrijvingstekens wanneer aanwezig;
  - samenvatting van bekende locaties als komma-gescheiden tekst.
- Toon in die eerste productresultatenlijst geen hoeveelheden.
- Open bij een gescand of gekozen product met exact één actieve voorraadlocatie direct
  de bestaande locatiepagina van die locatie.
- Toon bij een gescand of gekozen product met meerdere actieve voorraadlocaties direct
  een compacte lijst met gebied, locatienaam, hoeveelheid en eenheid per locatie.
- Maak vanuit die locatielijst doorklikken naar de bestaande locatiepagina mogelijk.
- Meld duidelijk wanneer een product wel bekend is maar geen actieve voorraadlocaties
  heeft.
- Toon in dat geval de laatst gebruikte locatie als verwachte plek wanneer die nog
  bekend is, weergegeven met leesbare gebied- en locatienaam.
- Bied in zowel het "geen actieve voorraad" geval als het "verwachte locatie"
  vervolggedrag een actie `Voorraad toevoegen`.
- Gebruik bestaande inventory-opslag en locatiepagina's als bron van waarheid; bouw
  geen parallelle terugvindadministratie.

## Outside Scope

- Geen nieuwe dashboard-zoekbalk of andere extra hoofdroute buiten `Scannen` en
  `Voorraadbeheer > Producten`.
- Geen voorraadmutaties, verbruik, correcties, tellingen of historie in deze story.
- Geen fuzzy matching, synoniembeheer, typo-correctie of uitgebreide filters.
- Geen hoeveelheden in de eerste productresultatenlijst van handmatig zoeken.
- Geen wijziging van QR-format, auth-opzet, algemene routering of brede UI-herbouw.
- Geen documentatie-, commit-, push-, branch-, PR-, merge-, release- of
  deploymentacties.

## Expected Write-Set

Wijzig alleen deze bestanden of modules, tenzij een noodzakelijke compile-time
dependency wordt ontdekt:

- `BootManager.Application/Inventory/Contracts/IProductService.cs`;
- `BootManager.Application/Inventory/Contracts/IStockService.cs`;
- `BootManager.Application/Inventory/Services/ProductService.cs`;
- `BootManager.Application/Inventory/Services/StockService.cs`;
- optioneel kleine nieuwe DTO's of result-types onder
  `BootManager.Application/Inventory/DTOs/` of `.../Results/`;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- optioneel een of enkele kleine nieuwe inventory-componenten onder
  `BootManager.Web/Components/Inventory/` voor zoekresultaten of locatielijst;
- optioneel `BootManager.Web/Components/Pages/StorageLocationDetails.razor` alleen
  wanneer de actie `Voorraad toevoegen` zonder onnodige omweg niet met bestaand gedrag
  kan landen;
- gerichte tests onder `BootManager.UnitTests/Storage/`,
  `BootManager.UnitTests/Inventory/` en indien echt nodig
  `BootManager.IntegrationTests/Inventory/`.

Wijzig geen migraties of database-schema tenzij je tijdens implementatie aantoont dat
de bestaande opslag de vereiste "verwachte locatie" technisch niet kan dragen. Stop dan
en rapporteer `not ready` met de concrete ontbrekende beslissing.

## Execution Boundaries

- Implementeer alleen applicatiecode, presentatiecode en tests die dit packet
  expliciet vereist.
- Controleer vóór bewerken dat de actieve branch exact
  `codex/pilot-inv-04-product-terugvinden` is en niet `master`. Rapporteer `not ready`
  als dat niet zo is.
- Wijzig geen story-, release-, TODO-, legacy-, README-, handoff- of andere
  projectdocumentatie.
- Maak geen commit, push, branch, PR, merge, release of deployment.
- Houd de terugvindflow compact en taakgericht; voeg geen generiek zoekframework of
  nieuwe modulearchitectuur toe als bestaande componenten en services volstaan.
- Behoud `PILOT-INV-03`-gedrag voor locatie-QR-routing en onbekende-code/inruimflow
  tenzij een expliciete flowscheiding nodig is om `PILOT-INV-04` correct te laten
  werken; leg zo'n scheiding dan klein en lokaal vast.
- Gebruik echte voorraadregels en bestaande laatst-gebruikte-locatiebepaling als basis
  voor locatiebeslissingen; introduceer geen losse favorieten-, cache- of
  voorkeurslocatie-opslag.
- Noem de story nooit `Done`, geaccepteerd of productierijp. Meld alleen
  `ready for Codex review` wanneer de technische completion definition volledig is
  gehaald.

## Minimal Context

Lees:

- `CLAUDE.md`;
- `.codex/PILOT-INV-04-implementation-packet.md`;
- alleen de sectie `PILOT-INV-04` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.Web/Components/Inventory/AddStockDialog.razor`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- `BootManager.Application/Inventory/Contracts/IProductService.cs`;
- `BootManager.Application/Inventory/Contracts/IStockService.cs`;
- `BootManager.Application/Inventory/Services/ProductService.cs`;
- `BootManager.Application/Inventory/Services/StockService.cs`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`;
- `BootManager.UnitTests/Storage/StorageLocationDetailsWithStockComponentTests.cs`;
- `BootManager.UnitTests/Inventory/ProductServiceTests.cs`;
- `BootManager.UnitTests/Inventory/StockServiceTests.cs`.

Gebruik gerichte zoekopdrachten en kleine bestandssecties. Lees niet standaard:

- de volledige `.docs/TODO.md` of andere releaseverhalen;
- `.docs/legacy-analysis/` of `.docs/legacy-input/`;
- `.codex/current-session-handoff.md` of `.codex/working-agreement.md`;
- repositorybrede source trees.

## Existing Constraints

- Volg .NET 8 en de Clean Architecture-regels in `CLAUDE.md`.
- `Scan.razor` onderscheidt nu al locatie-QR's, bekende productcodes en onbekende
  productcodes; breid of splits het bestaande gedrag uit zonder de bewezen
  scanroutering te breken.
- `PILOT-INV-03` gebruikt bestaande service-methoden voor gekoppelde productcodes en
  laatst gebruikte / alternatieve locaties; hergebruik die basis waar mogelijk.
- `Products.razor` is nu vooral catalogusbeheer met uitklapbare locatiedetails; de
  handmatige zoekfallback moet daarop voortbouwen zonder de beheerflow te vervangen
  door een zware nieuwe pagina.
- `StockService.SearchProductsInLocationAsync` zoekt nu al op naam en gekoppelde code
  in een locatiecontext; voor deze story is zoekgedrag op catalogusniveau nodig op naam
  en omschrijving, zonder dat de bestaande locatiegerichte flow regressief wordt.
- De actie `Voorraad toevoegen` moet landen op een bestaand of minimaal uitgebreid pad
  dat logisch blijft binnen de bestaande locatiecontext.

## Acceptance Focus

- Een bekende productcode uit `Scannen` start de terugvindflow en niet de
  inruimflow.
- Een bekende locatie-QR blijft direct de locatiepagina openen.
- Handmatig zoeken via `Voorraadbeheer > Producten` werkt op naam en omschrijving,
  hoofdletterongevoelig en met deelmatches.
- Meerdere handmatige matches tonen eerst een compacte productresultatenlijst zonder
  hoeveelheden maar met locatiesamenvatting.
- Exact één actieve voorraadlocatie opent direct de locatiepagina.
- Meerdere actieve voorraadlocaties tonen direct een locatielijst met gebied,
  locatienaam, hoeveelheid en eenheid.
- Geen actieve voorraad toont een duidelijke melding, en indien beschikbaar de
  verwachte laatst gebruikte locatie als leesbare plek.
- In beide "niet direct op een enkele locatie uitkomen"-gevallen is een actie
  `Voorraad toevoegen` beschikbaar.

## Test Evidence Requirements

Voeg defectgevoelige tests toe die echte productcode/componenten uitvoeren en concreet
bewijzen:

- `Scan.razor` routeert een bekende locatie-QR nog steeds naar de locatiepagina;
- `Scan.razor` routeert een bekende productcode nu naar de terugvindflow en niet meer
  naar de inruimflow;
- een product met exact één actieve voorraadlocatie direct de juiste locatiepagina
  opent;
- een product met meerdere actieve voorraadlocaties een locatielijst toont met gebied,
  locatienaam, hoeveelheid en eenheid;
- handmatig zoeken op de productpagina naam en omschrijving echt doorzoekt,
  hoofdletterongevoelig is en deelmatches ondersteunt;
- de eerste productresultatenlijst geen hoeveelheden maar wel locatiesamenvatting toont;
- een bekend product zonder actieve voorraad een duidelijke melding toont en, wanneer
  beschikbaar, de verwachte laatst gebruikte locatie leesbaar weergeeft;
- de actie `Voorraad toevoegen` vanaf de terugvindflow naar het bedoelde bestaande
  vervolgpad leidt;
- bestaand `PILOT-INV-03`-gedrag voor onbekende productcode en locatie-QR niet
  regressief is.

Inspecteer iedere nieuwe of gewijzigde test: geen `Assert.True(true)`, lege test,
bronvormtest als vervanging van gedrag of `async` test zonder relevante `await`.

Deze story is geen bugfix, dus formeel red-green-bewijs is niet verplicht. Als je een
bestaand defect tegenkomt en meeneemt, lever daarvoor alsnog expliciet red-green of
gelijkwaardig bewijs.

## Required Checks

Voer eerst gerichte checks uit, bijvoorbeeld:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ScanComponentTests|FullyQualifiedName~StockServiceTests|FullyQualifiedName~StorageLocationDetailsWithStockComponentTests|FullyQualifiedName~Products"
```

Als je geen `Products`-tests toevoegt maar bestaande componenttests uitbreidt, vervang
dat laatste filterdeel door de feitelijke nieuwe of aangepaste testklassenaam.

Voer daarna uit:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
```

Als tijdens implementatie blijkt dat een echte integration test nodig is voor
query-/mappinggedrag, voeg die gerichte run expliciet toe in je completion notes.

## Definition of Technical Completion

Meld uitsluitend `ready for Codex review` wanneer:

- ieder scopepunt en acceptatiecriterium technisch is geïmplementeerd;
- scanroute en handmatige zoekroute beide aantoonbaar werken;
- locatie-QR-routing uit bestaand gedrag niet regressief is;
- de enkelvoudige locatie-open, meervoudige locatielijst en "geen actieve voorraad"
  paden alle drie aantoonbaar correct werken;
- de actie `Voorraad toevoegen` in de bedoelde gevallen beschikbaar is en naar een
  logisch bestaand vervolgpad leidt;
- alle gerichte tests slagen en alle nieuwe of gewijzigde tests echte productcode
  uitvoeren;
- build en `git diff --check` slagen;
- geen onverklaarde wijziging buiten de verwachte write-set staat;
- resterende handmatige acceptatiestappen expliciet zijn vermeld.

Meld `not ready` wanneer scope onvolledig is, de scan- of zoekroute niet eenduidig is,
een van de drie locatie-uitkomsten ontbreekt, `Voorraad toevoegen` niet logisch kan
landen, een nieuwe of gewijzigde test faalt, build/diffcheck faalt, een vereiste
beslissing ontbreekt of extra write-area niet kan worden verantwoord.

## Completion Notes

Retourneer alleen:

1. gewijzigde bestanden en geimplementeerd gedrag;
2. tests/checks en resultaten;
3. exacte nieuwe/gewijzigde testnamen en welk productiegedrag zij uitvoeren;
4. migratie-, package- of configuratie-impact;
5. resterende risico's en exacte handmatige testvereisten;
6. eindstatus: `ready for Codex review` of `not ready`, met concrete reden.
