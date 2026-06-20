# Implementation Packet

## Task

- Story ID: `PILOT-INV-01`
- Approved story: productcategorieen, producten en productbarcodes
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-INV-01`
- Goal: lever de eerste lokale inventory-catalogus met categorieen, eenheden,
  producten en maximaal een gekoppelde code per product, inclusief navigatie,
  lokale persistence, soft delete en inline beheerflows.
- Required branch: `feature/pilot-inv-01-product-catalog`

De story is al goedgekeurd. Formuleer haar niet opnieuw en vraag geen nieuw akkoord.
Geef een kort uitvoeringsplan, implementeer direct, voer de checks uit en rapporteer
volgens `Completion Notes`.

## Scope

- Voeg persistente inventory-entiteiten toe voor productcategorieen, eenheden,
  producten, product-categorie-koppelingen en gekoppelde productcodes. Gebruik
  stabiele `Guid`-id's.
- Modelleer de UI-regel "exact een actieve categorie per product" via een aparte
  product-categorie-koppeling, zodat de dataopzet later meerdere categorieen kan
  ondersteunen zonder deze nu functioneel open te zetten.
- Een categorie heeft een verplichte unieke naam, een optionele korte omschrijving,
  een verplichte icoonsleutel uit een kleine vaste ingebouwde set en ondersteunt
  archiveren en heractiveren.
- Een eenheid heeft een verplichte unieke naam, ondersteunt archiveren en
  heractiveren en de applicatie levert een kleine defaultset via additieve seed of
  startup-initialisatie zonder bestaande data te resetten.
- Een product heeft een verplichte naam, exact een verplichte standaardeenheid, een
  optionele omschrijving, ondersteunt archiveren en heractiveren en kan nul of een
  gekoppelde code hebben.
- Een gekoppelde code is een aparte entiteit met minimaal genormaliseerde waarde en
  formaat/type. Bewaar uniqueness catalogusbreed, case-onafhankelijk en ook wanneer
  het gekoppelde product gearchiveerd is.
- Laat de application-service invoer trimmen, lege verplichte velden weigeren en lege
  optionele tekst als `null` opslaan.
- Blokkeer het archiveren van een categorie of eenheid zolang er nog actieve
  producten naar verwijzen.
- Ondersteun in de UI:
  `Voorraadbeheer`/inventory-navigatie met submenu-items `Producten`,
  `Categorieen` en `Eenheden`;
  eenvoudige beheerpagina's voor categorieen en eenheden met simpele modals voor
  aanmaken en bewerken;
  een aparte productbeheerpagina met lijst, archieffilter en apart formulier of
  apart scherm voor aanmaken en bewerken;
  inline modal-aanmaak van een nieuwe categorie of eenheid vanuit het
  productformulier, waarna de productflow met de nieuwe keuze doorgaat;
  beheer van nul of een gekoppelde code binnen het productformulier, inclusief
  handmatige invoer en hergebruik van de bestaande scaninfrastructuur voor code-invoer
  als dat binnen deze slice haalbaar is zonder nieuwe scanarchitectuur.
- Houd Owner en Crew beide bevoegd voor catalogusbeheer; wijzig de bestaande
  autorisatierollen verder niet.
- Voeg precies een additieve EF Core-migratie toe en werk de model snapshot bij.
  Bestaande databases moeten zonder reset of dataverlies in-place kunnen migreren.

## Outside Scope

- Geen voorraadregels, hoeveelheden, mutaties, historie of product-locatiekoppelingen.
- Geen terugvindflow, scan-gestuurde inruimflow of onbekende-code-afhandeling tijdens
  het bestaande menu `Scannen`.
- Geen meerdere actieve categorieen per product in de UI.
- Geen meerdere gekoppelde codes per product, product-QR-codes, productfoto's,
  labels, minimumvoorraad, merk/fabrikant, SKU's, bulkimport/export of
  dashboardintegratie.
- Geen vrij uploadbare categorie-iconen; gebruik alleen een kleine vaste set.
- Geen nieuwe externe dependencies of brede architectuurrefactor.
- Geen wijzigingen aan story-, release-, TODO-, legacy-, README- of
  handoff-documentatie.
- Geen commits, pushes, branches, PR's, merges, releases of deployments.

## Expected Write-Set

Wijzig alleen deze bestanden of modules, tenzij een noodzakelijke compile-time
dependency wordt ontdekt:

- `BootManager.Core/Entities/` voor nieuwe inventory-entiteiten;
- `BootManager.Application/Inventory/` voor DTO's, resultaten, servicecontracten en
  application-services;
- `BootManager.Application/DependencyInjection.cs`;
- `BootManager.Infrastructure/Persistence/BootManagerDbContext.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/` voor inventory-configuratie;
- een nieuwe `BootManager.Infrastructure/Migrations/*Inventory*` migratie plus
  `BootManagerDbContextModelSnapshot.cs`;
- `BootManager.Web/Components/Layout/NavMenu.razor` en alleen de direct geraakte
  layouttests;
- nieuwe inventory-pagina's en kleine ondersteunende componenten onder
  `BootManager.Web/Components/Pages/` en eventueel een gerichte submap voor
  inventory-componenten;
- alleen als nodig voor scaninvoer: het minimale inventory-gerelateerde invoegpunt in
  `BootManager.Web/Components/Pages/Scan.razor` of een herbruikbare scannerhelper,
  zonder de bestaande locatie-QR-flow functioneel te veranderen;
- gerichte tests onder `BootManager.UnitTests/Inventory/`, `BootManager.UnitTests/Web/`
  en, voor relationele constraints/migratiebewijs, onder
  `BootManager.IntegrationTests/Inventory/`.

Leg vóór wijziging buiten deze write-set uit waarom die nodig is.

## Execution Boundaries

- Implementeer alleen applicatiecode, migratie, configuratie en tests die dit packet
  vereist.
- Controleer vóór bewerken dat de actieve branch exact
  `feature/pilot-inv-01-product-catalog` is en niet `master`. Rapporteer `niet
  gereed` als dat niet zo is.
- Wijzig geen story-, release-, TODO-, legacy-, README-, handoff- of andere
  projectdocumentatie.
- Maak geen commit, push, branch, PR, merge, release of deployment.
- Verander scope, acceptatiecriteria of architectuurrichting niet. Stop en meld de
  kleinste ontbrekende beslissing als de goedgekeurde richting niet uitvoerbaar is.
- Voer geen database-reset uit en raak geen productie- of Raspberry Pi-database aan.
  Gebruik uitsluitend tijdelijke SQLite-databases voor migratie- en constrainttests.
- Noem de story nooit `Done`, geaccepteerd of productierijp. Meld alleen `gereed voor
  Codex-review` wanneer de technische completion definition volledig is gehaald.

## Minimal Context

Lees:

- `CLAUDE.md`;
- `.codex/PILOT-INV-01-implementation-packet.md`;
- `.codex/claude-sources/inventory/PILOT-INV-01.md`;
- de sectie `PILOT-INV-01` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Core/Interfaces/IRepository.cs`;
- `BootManager.Infrastructure/Repositories/EfRepository.cs`;
- `BootManager.Infrastructure/Persistence/BootManagerDbContext.cs`, relevante
  configuratievoorbeelden en de actuele model snapshot;
- `BootManager.Application/DependencyInjection.cs`;
- `BootManager.Application/Storage/Services/StorageService.cs` als patroon voor
  trimmen, validatie, operation results en repositorygebruik;
- `BootManager.Web/Components/Layout/NavMenu.razor`;
- `BootManager.Web/Components/Pages/Scan.razor` alleen voor bestaand scaninvoegpunt;
- `BootManager.UnitTests/Web/NavMenuComponentTests.cs`;
- bestaande storage-componenttests als bUnit-patroon;
- bestaande tijdelijke-SQLitepatronen in `BootManager.IntegrationTests/`.

Gebruik gerichte zoekopdrachten en kleine bestandssecties. Lees niet standaard:

- de volledige `.docs/TODO.md` of ongerelateerde releaseverhalen;
- `.docs/legacy-analysis/` of `.docs/legacy-input/`;
- `.codex/current-session-handoff.md` of `.codex/working-agreement.md`;
- ongerelateerde source trees zoals logboek-, dashboard-, ingest- of NMEA-code.

## Existing Constraints

- Volg .NET 8 en de Clean Architecture-regels in `CLAUDE.md`.
- `IRepository<T>` werkt met `Guid` en schrijft per mutatie direct via
  `SaveChangesAsync`; introduceer geen nieuwe repositoryabstractie of unit-of-worklaag.
- Gebruik genormaliseerde waarden (`Trim()` plus hoofdletterongevoelige normalisatie)
  voor consistente uniqueness op SQLite.
- De databaseconstraint is de laatste integriteitslaag. Vertaal een eventuele race bij
  een unieke index of unieke gekoppelde code naar hetzelfde functionele
  validatieresultaat waar dat praktisch binnen de bestaande architectuur kan, zonder
  brede repositoryrefactor.
- Houd de bestaande scanbasis en locatie-QR-routing intact. Als scaninvoer voor
  productcodes deze story onnodig complex maakt, implementeer dan minimaal de
  handmatige codeflow volledig en rapporteer concreet welk scanstuk nog ontbreekt als
  `niet gereed`; verzwak de acceptatie niet stilzwijgend.
- Bestaande storage- en authfunctionaliteit mag niet regressief geraakt worden door
  nieuwe navigatie of autorisatie.

## Acceptance Focus

- Correcte lokale catalogusbasis met categorieen, eenheden, producten en maximaal een
  actieve gekoppelde code per product.
- Soft delete en heractiveren werken voor categorieen, eenheden en producten; gewone
  lijsten en keuzelijsten verbergen standaard gearchiveerde records.
- Een categorie of eenheid met actieve productreferenties kan niet gearchiveerd worden.
- Een gekoppelde code blijft uniek over de hele catalogus, ook als het product
  gearchiveerd is.
- Productformulier ondersteunt inline categorie-/eenheidaanmaak en behoudt daarna de
  productflow.
- Navigatie en autorisatie kloppen voor Owner en Crew.
- Migratie en relationele constraints werken op echte tijdelijke SQLite, niet alleen
  via EF InMemory.
- Geen voorraad-, locatie- of scanrouteringsgedrag van latere inventory-slices lekt
  deze slice binnen.

## Required Checks

Voeg gerichte tests toe voor minimaal:

- aanmaken, bewerken, archiveren en heractiveren van categorieen, eenheden en
  producten;
- trimmen, lege/te lange invoer en hoofdletterongevoelige duplicates;
- blokkade op archiveren van categorie of eenheid met actieve productreferenties;
- product opslaan met exact een categorie in de UI en exact een verplichte
  standaardeenheid;
- gekoppelde code toevoegen, vervangen, ontkoppelen, normaliseren en uniek houden;
- uniqueness van gekoppelde code terwijl het gekoppelde product gearchiveerd is;
- defaultset van eenheden zonder datareset;
- bUnit-gedrag voor inventory-navigatie, categorie-/eenheidmodals en inline
  terugkeer naar het productformulier;
- SQLite unique-indexen, foreign keys, soft-delete-relevante constraints en toepassen
  van de nieuwe migratie op een tijdelijke database vanaf de vorige migratiestand,
  met behoud van bestaande data/tabellen.

Voer eerst gerichte inventory-testfilters uit. Voer daarna uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
```

De bekende
`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect`-failure mag
alleen als ongerelateerde baseline worden gemeld wanneer exact die ene bestaande
unit-testfailure overblijft. Nieuwe of gewijzigde inventory-tests moeten allemaal
slagen.

## Definition of Technical Completion

Meld uitsluitend `gereed voor Codex-review` wanneer:

- ieder scopepunt en acceptatiecriterium technisch is geïmplementeerd;
- alle gerichte inventory-tests slagen;
- de volledige unit- en integratietestruns geen nieuwe failure bevatten;
- build en `git diff --check` slagen;
- de migratie, uniqueness en relationele constraints aantoonbaar op tijdelijke SQLite
  zijn bewezen;
- geen onverklaarde wijziging buiten de verwachte write-set staat;
- de resterende handmatige Owner/Crew-acceptatiestappen expliciet zijn vermeld.

Meld `niet gereed` wanneer scope onvolledig is, migratie/constraints niet bewezen zijn,
een nieuwe of gewijzigde test faalt, build/diffcheck faalt, een vereiste beslissing
ontbreekt of extra write-area niet kan worden verantwoord. Verlaag geen test- of
acceptatie-eis en maskeer geen failure als waarschuwing.

## Completion Notes

Retourneer alleen:

1. gewijzigde bestanden en geimplementeerd gedrag;
2. tests/checks en resultaten;
3. exacte nieuwe of gewijzigde testnamen en welk productgedrag zij uitvoeren;
4. migratie- en configuratie-impact;
5. resterende risico's en exacte handmatige testvereisten;
6. eindstatus: `gereed voor Codex-review` of `niet gereed`, met concrete reden.
