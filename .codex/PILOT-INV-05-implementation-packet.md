# Implementation Packet

## Task

- Story ID: `PILOT-INV-05`
- Approved story: voorraad muteren en eenvoudige historie
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-INV-05`
- Goal: breid de bestaande inventory-basis uit zodat Owner en Crew voorraadverbruik, tellingen en correcties kunnen vastleggen op een expliciete product-locatiecombinatie, inclusief een fysieke scanroute, een administratieve fallback zonder scannen en een eenvoudige historiepagina met de vastgelegde mutaties.
- Required branch: `codex/pilot-inv-05-mutaties-historie`

De story is al goedgekeurd. Formuleer haar niet opnieuw en vraag geen nieuw akkoord.
Geef een kort uitvoeringsplan, implementeer direct, voer de checks uit en rapporteer
volgens `Completion Notes`.

## Scope

- Ondersteun exact drie mutatietypes: `Verbruik`, `Correctie` en `Telling`.
- Leg iedere mutatie vast op een expliciete bestaande product-locatieregel of op een
  expliciet gekozen product-locatiecombinatie.
- Voeg een fysieke verbruikflow toe die voortbouwt op de bestaande terugvind- en
  scanbasis:
  - product terugvinden;
  - naar de locatie gaan;
  - locatie scannen;
  - product scannen;
  - verbruikte hoeveelheid invoeren;
  - opslaan;
  - terugkeren naar het begin van die route.
- Voeg een administratieve fallback toe zonder scannen, waarbij de gebruiker eerst een
  product kiest en daarna een locatie, behalve wanneer er nog maar exact één actieve
  locatie voor dat product bestaat en BootManager die locatie dus automatisch kan kiezen.
- Vraag bij `Verbruik` om een afnamehoeveelheid.
- Vraag bij `Telling` en `Correctie` om de nieuwe feitelijke hoeveelheid.
- Sta bij iedere mutatie een vrije optionele notitie toe.
- Blokkeer `Verbruik` wanneer de gevraagde afname groter is dan de actuele voorraad op
  die locatie.
- Laat een actieve voorraadregel verdwijnen zodra de resulterende hoeveelheid exact `0`
  is, maar behoud de laatst gebruikte locatie als verwachte locatie voor later
  terugvinden of opnieuw inruimen.
- Voeg een aparte historiepagina toe die standaard alle voorraadmutaties nieuwste eerst
  toont met minimaal datum/tijd, mutatietype, productnaam, gebied + locatienaam, oude
  hoeveelheid, nieuwe hoeveelheid, gebruiker en optionele notitie.

## Outside Scope

- Geen negatieve voorraad.
- Geen mutaties zonder expliciete locatie.
- Geen batchverplaatsingen, samengestelde voorraadacties of bulkcorrecties.
- Geen inline historie op bestaande product- of locatiepagina's.
- Geen geavanceerde filters, export, rapportage of dashboardwidgets voor historie.
- Geen automatische verbruiksafleiding zonder expliciete gebruikersactie.
- Geen wijziging van QR-format, auth-opzet, algemene routering of brede UI-herbouw.
- Geen documentatie-, commit-, push-, branch-, PR-, merge-, release- of
  deploymentacties.

## Expected Write-Set

Wijzig alleen deze bestanden of modules, tenzij een noodzakelijke compile-time of
migratieafhankelijkheid wordt ontdekt:

- `BootManager.Core/Entities/Stock.cs`;
- optioneel een of meer kleine nieuwe inventory-entiteiten of enums onder
  `BootManager.Core/Entities/` voor voorraadmutaties;
- `BootManager.Infrastructure/Persistence/BootManagerDbContext.cs`;
- optioneel nieuwe EF-configuratie onder
  `BootManager.Infrastructure/Persistence/Configurations/`;
- een nieuwe EF Core migratie plus designer en snapshot onder
  `BootManager.Infrastructure/Migrations/`;
- `BootManager.Application/Inventory/Contracts/IStockService.cs`;
- optioneel kleine nieuwe inventory-contracten, DTO's of result-types onder
  `BootManager.Application/Inventory/Contracts/`, `.../DTOs/` of `.../Results/`;
- `BootManager.Application/Inventory/Services/StockService.cs`;
- optioneel `BootManager.Application/Inventory/Services/ProductService.cs` alleen
  wanneer de administratieve fallback extra productzoek- of selectiegedrag nodig heeft;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.Web/Components/Layout/NavMenu.razor`;
- optioneel een of meer kleine nieuwe inventory-componenten of pagina's onder
  `BootManager.Web/Components/Inventory/` of `BootManager.Web/Components/Pages/Inventory/`;
- gerichte tests onder `BootManager.UnitTests/Inventory/` en
  `BootManager.UnitTests/Storage/`;
- indien nodig een gerichte migratie- of persistence-test onder
  `BootManager.IntegrationTests/Inventory/`.

Wijzig geen ongerelateerde domeinen, algemene layoutstructuren of andere featuremodules.
Als een extra write-area nodig blijkt, licht dat eerst concreet toe in de
oplevernotities.

## Execution Boundaries

- Implementeer alleen applicatiecode, migraties, presentatiecode en tests die dit
  packet expliciet vereist.
- Controleer vóór bewerken dat de actieve branch exact
  `codex/pilot-inv-05-mutaties-historie` is en niet `master`. Rapporteer `not ready`
  als dat niet zo is.
- Wijzig geen story-, release-, TODO-, legacy-, README-, handoff- of andere
  projectdocumentatie.
- Maak geen commit, push, branch, PR, merge, release of deployment.
- Houd de nieuwe mutatiefuncties taakgericht; introduceer geen generiek auditframework,
  event-sourcinglaag of brede inventory-refactor als de story met kleine lokale
  uitbreidingen gerealiseerd kan worden.
- Gebruik bestaande product-, locatie- en voorraadgegevens als bron van waarheid; bouw
  geen parallelle opslag voor "huidige voorraad".
- Gebruik voor de historische gebruikersnaam bestaande auth-context of bestaande
  gebruikersgegevens; voeg geen nieuw identity-model toe.
- Noem de story nooit `Done`, geaccepteerd of productierijp. Meld alleen
  `ready for Codex review` wanneer de technische completion definition volledig is
  gehaald.

## Minimal Context

Lees:

- `CLAUDE.md`;
- `.codex/PILOT-INV-05-implementation-packet.md`;
- alleen de sectie `PILOT-INV-05` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Core/Entities/Stock.cs`;
- `BootManager.Application/Inventory/Contracts/IStockService.cs`;
- `BootManager.Application/Inventory/Services/StockService.cs`;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.Web/Components/Layout/NavMenu.razor`;
- `BootManager.Infrastructure/Persistence/BootManagerDbContext.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/StockConfiguration.cs`;
- laatste inventory-migraties:
  - `BootManager.Infrastructure/Migrations/20260620152203_AddStockEntities.cs`;
  - `BootManager.Infrastructure/Migrations/20260620181000_AddStockUpdatedAtTimestamp.cs`;
- `BootManager.UnitTests/Inventory/StockServiceTests.cs`;
- `BootManager.UnitTests/Inventory/ProductsComponentTests.cs`;
- `BootManager.UnitTests/Storage/ScanComponentTests.cs`;
- `BootManager.UnitTests/Storage/StorageLocationDetailsWithStockComponentTests.cs`.

Gebruik gerichte zoekopdrachten en kleine bestandssecties. Lees niet standaard:

- de volledige `.docs/TODO.md` of andere releaseverhalen;
- `.docs/legacy-analysis/` of `.docs/legacy-input/`;
- `.codex/current-session-handoff.md` of `.codex/working-agreement.md`;
- repositorybrede source trees.

## Existing Constraints

- Volg .NET 8 en de Clean Architecture-regels in `CLAUDE.md`.
- `Stock` bewaart nu alleen de actuele regel en `UpdatedAt`; voor historie is dus
  waarschijnlijk aanvullende opslag nodig.
- `PILOT-INV-04` levert al product-terugvinden, verwachte locatie en voorraad toevoegen
  vanuit scan- en zoekroutes; bouw daarop voort in plaats van een tweede parallelle
  terugvindflow te maken.
- `StorageLocationDetails.razor` toont al locatievoorraad en biedt de context waar de
  fysieke mutatieflow logisch kan landen.
- `Products.razor` bevat al een zoekfallback en productselectiegedrag dat voor de
  administratieve mutatieflow hergebruikt of klein uitgebreid kan worden.
- Het inventory-menu bevat nu alleen `Producten`, `Categorieen` en `Eenheden`; de
  historiepagina moet daar logisch in passen zonder de bestaande menu-items te breken.
- Omdat deze story dataopslag raakt, telt technische oplevering pas wanneer de migratie
  en upgradepad-aannames aantoonbaar zijn gecontroleerd.

## Acceptance Focus

- Owner en Crew kunnen muteren als `Verbruik`, `Correctie` en `Telling`.
- De fysieke scanroute dwingt expliciete locatie- en productcontext af voordat
  `Verbruik` wordt opgeslagen.
- Na een geslaagde fysieke verbruikmutatie keert de gebruiker terug naar het begin van
  de terugvind/verbruikflow.
- De administratieve fallback werkt zonder scannen via productkeuze en locatiekeuze, met
  automatische locatiekeuze bij exact één actieve locatie.
- `Verbruik` vraagt om afname; `Telling` en `Correctie` vragen om de nieuwe feitelijke
  hoeveelheid.
- Een optionele notitie wordt bewaard en zichtbaar in de historie.
- Oververbruik wordt geblokkeerd.
- Een resulterende hoeveelheid van `0` verwijdert de actieve voorraadregel maar laat de
  verwachte locatie intact voor bestaand terugvindgedrag.
- De historiepagina toont alle mutaties standaard nieuwste eerst met alle verplichte
  velden.

## Test Evidence Requirements

Voeg defectgevoelige tests toe die echte productcode/componenten uitvoeren en concreet
bewijzen:

- `StockService` verwerkt `Verbruik`, `Correctie` en `Telling` volgens de bedoelde
  rekenregels;
- `Verbruik` boven actuele voorraad faalt met een duidelijke fout en laat bestaande
  voorraad ongewijzigd;
- een mutatie naar exact `0` verwijdert de actieve voorraadregel, terwijl de verwachte
  locatie voor bestaand terugvindgedrag bruikbaar blijft;
- de mutatiehistorie wordt opgeslagen en standaard nieuwste eerst teruggegeven;
- de gelogde historieregel bevat product, locatie, oude hoeveelheid, nieuwe hoeveelheid,
  type, gebruiker en notitie;
- de fysieke scanflow alleen verdergaat wanneer eerst de juiste locatie en daarna het
  juiste product in context zijn gebracht;
- na een geslaagde fysieke verbruikmutatie keert `Scan.razor` terug naar het begin van
  die route;
- de administratieve fallback in de UI product- en locatiekeuze correct afdwingt of
  locatie automatisch kiest bij exact één actieve locatie;
- de historiepagina de verplichte kolommen en volgorde toont;
- bestaand `PILOT-INV-04`-gedrag voor terugvinden, verwachte locatie en voorraad
  toevoegen niet regressief is.

Inspecteer iedere nieuwe of gewijzigde test: geen `Assert.True(true)`, lege test,
bronvormtest als vervanging van gedrag of `async` test zonder relevante `await`.

Deze story is geen bugfix, dus formeel red-green-bewijs is niet verplicht. Als je een
bestaand defect tegenkomt en meeneemt, lever daarvoor alsnog expliciet red-green of
gelijkwaardig bewijs.

Voor de migratie geldt aanvullend:

- migreer expliciet vanaf `20260620181000_AddStockUpdatedAtTimestamp`;
- controleer welke migraties vóór en na de upgrade toegepast zijn;
- voeg bestaande voorraaddata in vóór de upgrade;
- bewijs na upgrade dat die data behouden blijft en dat nieuwe mutatieopslag bruikbaar
  is.

## Required Checks

Voer eerst gerichte checks uit, bijvoorbeeld:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StockServiceTests|FullyQualifiedName~ProductsComponentTests|FullyQualifiedName~ScanComponentTests|FullyQualifiedName~StorageLocationDetailsWithStockComponentTests"
```

Als je een nieuwe historie-componenttestklasse of andere gerichte testklasse toevoegt,
pas het filter aan zodat die klasse expliciet meedraait.

Voer daarna uit:

```powershell
dotnet build BootManager.sln --no-restore
git diff --check
```

Als je een integration test toevoegt voor migratie- of persistencebewijs, voer die
aparte run ook expliciet uit en noem die in je completion notes.

## Definition of Technical Completion

Meld uitsluitend `ready for Codex review` wanneer:

- ieder scopepunt en acceptatiecriterium technisch is geïmplementeerd;
- de fysieke verbruikflow en administratieve fallback beide aantoonbaar werken;
- alle drie mutatietypes correct werken;
- oververbruik aantoonbaar wordt geblokkeerd;
- de `0`-voorraadafhandeling zowel actieve regelverwijdering als behoud van verwachte
  locatie correct afdekt;
- de historiepagina de verplichte gegevens standaard nieuwste eerst toont;
- alle gerichte tests slagen en alle nieuwe of gewijzigde tests echte productcode
  uitvoeren;
- build en `git diff --check` slagen;
- migratie- en upgradegedrag aantoonbaar zijn bewezen;
- geen onverklaarde wijziging buiten de verwachte write-set staat;
- resterende handmatige acceptatiestappen expliciet zijn vermeld.

Meld `not ready` wanneer scope onvolledig is, een van de drie mutatietypes ontbreekt,
de scanroute of fallbackroute niet eenduidig werkt, historie of gebruiker/notitie niet
betrouwbaar wordt vastgelegd, de `0`-voorraadafhandeling het terugvindgedrag breekt, een
nieuwe of gewijzigde test faalt, build/diffcheck faalt, migratiebewijs ontbreekt, een
vereiste beslissing ontbreekt of extra write-area niet kan worden verantwoord.

## Completion Notes

Retourneer alleen:

1. gewijzigde bestanden en geimplementeerd gedrag;
2. tests/checks en resultaten;
3. exacte nieuwe/gewijzigde testnamen en welk productiegedrag zij uitvoeren;
4. migratie-, package- of configuratie-impact;
5. resterende risico's en exacte handmatige testvereisten;
6. eindstatus: `ready for Codex review` of `not ready`, met concrete reden.
