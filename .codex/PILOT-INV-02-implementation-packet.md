# Implementation Packet

## Task

- Story ID: `PILOT-INV-02`
- Approved story: taakgerichte voorraadbasis per locatie
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-INV-02`
- Goal: verbind de bestaande productcatalogus en opslaglocaties met een eerste
  bruikbare voorraadbasis per locatie, zodat Owner en Crew vanaf een locatiepagina
  voorraad kunnen toevoegen, aanvullen, tonen en verwijderen zonder scan-hoofdflow.
- Required branch: `feature/pilot-inv-02-location-stock-basics`

De story is al goedgekeurd. Formuleer haar niet opnieuw en vraag geen nieuw akkoord.
Geef een kort uitvoeringsplan, implementeer direct, voer de checks uit en rapporteer
volgens `Completion Notes`.

## Scope

- Voeg persistente voorraadentiteiten toe voor een actuele product-locatie-voorraadregel
  met stabiele `Guid`-id, gekoppeld aan exact een bestaand product en exact een
  bestaande opslaglocatie.
- Leg per actieve voorraadregel functioneel alleen `product`, `locatie` en
  `hoeveelheid` vast; gebruik de bestaande standaardeenheid van het product voor
  presentatie.
- Bewaak dat per locatie maximaal een actuele voorraadregel per product bestaat.
- Voeg applicatielogica toe om:
  - voorraad aan een locatie toe te voegen voor een bestaand product;
  - een bestaande voorraadregel op dezelfde locatie aan te vullen in plaats van een
    tweede regel te maken;
  - een actuele voorraadregel te verwijderen na expliciete bevestigingsstap in de UI;
  - hoeveelheid `0` of lager te blokkeren.
- Maak productzoeking binnen deze flow mogelijk op productnaam en gekoppelde code.
- Ondersteun vanuit dezelfde locatiecontext directe productaanmaak via de bestaande
  inventory-basis, waarna de gebruiker functioneel terugkeert naar dezelfde
  locatieflow met het nieuw aangemaakte product geselecteerd.
- Laat de locatie-detailroute actuele locatie-inhoud tonen met minimaal productnaam,
  hoeveelheid en eenheid.
- Laat de productpagina per product de gekoppelde locaties tonen met minimaal gebied,
  locatienaam en hoeveelheid.
- Houd Owner en Crew bevoegd voor deze voorraadbasis; wijzig de bestaande
  autorisatierollen verder niet.
- Voeg precies een additieve EF Core-migratie toe en werk de model snapshot bij.
  Bestaande databases moeten zonder reset of dataverlies in-place kunnen migreren.

## Outside Scope

- Geen scan-gestuurde inruimflow, dashboardstart of automatische keuze van een
  voorraadactie vanuit `Scannen`.
- Geen verplichte locatie-QR in deze story en geen wijziging van de bestaande
  locatie-QR-routing.
- Geen product-terugvindflow via barcode, geen onbekende-code-afhandeling en geen
  scan-gebaseerde productkeuze.
- Geen verbruik, correcties, tellingen, overschrijven van hoeveelheden, negatieve
  hoeveelheden of mutatiehistorie.
- Geen samengestelde verplaatsactie tussen twee locaties.
- Geen categorie-filters, recente lijsten, voorkeursproducten of automatische
  suggesties in de productzoekflow.
- Geen meerdere losse voorraadregels voor hetzelfde product op dezelfde locatie.
- Geen wijzigingen aan story-, release-, TODO-, legacy-, README- of
  handoff-documentatie.
- Geen commits, pushes, branches, PR's, merges, releases of deployments.

## Expected Write-Set

Wijzig alleen deze bestanden of modules, tenzij een noodzakelijke compile-time
dependency wordt ontdekt:

- `BootManager.Core/Entities/` voor nieuwe voorraadentiteiten;
- `BootManager.Application/Inventory/` voor DTO's, resultaten, servicecontracten en
  applicatieservices voor voorraadregels of productkeuze in locatiecontext;
- `BootManager.Application/Storage/DTOs/StorageLocationDetailDto.cs` en alleen direct
  geraakte storage-servicecontracten of -serviceimplementatie om locatie-inhoud te
  leveren;
- `BootManager.Application/DependencyInjection.cs`;
- `BootManager.Infrastructure/Persistence/BootManagerDbContext.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/` voor nieuwe voorraadconfig;
- een nieuwe `BootManager.Infrastructure/Migrations/*Stock*` of vergelijkbare
  inventory-migratie plus `BootManagerDbContextModelSnapshot.cs`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor` en eventuele kleine
  inventory-subcomponenten of dialogen die direct nodig zijn voor `Voorraad toevoegen`
  en verwijderen;
- `BootManager.Web/Components/Pages/Inventory/Products.razor` voor tonen van
  voorraadlocaties per product en alleen direct benodigde ondersteunende componenten;
- alleen als functioneel nodig voor inline productaanmaak vanuit locatiecontext:
  minimale direct geraakte code in de bestaande inventory-productcomponenten;
- gerichte tests onder `BootManager.UnitTests/Inventory/`,
  `BootManager.UnitTests/Storage/`, `BootManager.UnitTests/Web/` en
  `BootManager.IntegrationTests/Inventory/`.

Leg vóór wijziging buiten deze write-set uit waarom die nodig is.

## Execution Boundaries

- Implementeer alleen applicatiecode, migratie, configuratie en tests die dit packet
  vereist.
- Controleer vóór bewerken dat de actieve branch exact
  `feature/pilot-inv-02-location-stock-basics` is en niet `master`. Rapporteer `niet
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
- `.codex/PILOT-INV-02-implementation-packet.md`;
- `.codex/claude-sources/inventory/PILOT-INV-02.md`;
- de sectie `PILOT-INV-02` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Core/Interfaces/IRepository.cs`;
- `BootManager.Infrastructure/Repositories/EfRepository.cs`;
- `BootManager.Infrastructure/Persistence/BootManagerDbContext.cs`, relevante
  configuratievoorbeelden en de actuele model snapshot;
- `BootManager.Application/DependencyInjection.cs`;
- `BootManager.Application/Storage/Services/IStorageService.cs`;
- `BootManager.Application/Storage/Services/StorageService.cs`;
- `BootManager.Application/Storage/DTOs/StorageLocationDetailDto.cs`;
- `BootManager.Application/Inventory/Contracts/IProductService.cs`;
- `BootManager.Application/Inventory/Services/ProductService.cs`;
- `BootManager.Application/Inventory/DTOs/ProductDto.cs`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- `BootManager.Web/Components/Pages/Inventory/Products.razor`;
- `BootManager.UnitTests/Storage/StorageLocationDetailsComponentTests.cs`;
- bestaande inventory-service- en componenttests als patroon;
- bestaande tijdelijke-SQLitepatronen in `BootManager.IntegrationTests/Inventory/`
  en `BootManager.IntegrationTests/Storage/`.

Gebruik gerichte zoekopdrachten en kleine bestandssecties. Lees niet standaard:

- de volledige `.docs/TODO.md` of ongerelateerde releaseverhalen;
- `.docs/legacy-analysis/` of `.docs/legacy-input/`;
- `.codex/current-session-handoff.md` of `.codex/working-agreement.md`;
- ongerelateerde source trees zoals logboek-, dashboard-, ingest- of NMEA-code.

## Existing Constraints

- Volg .NET 8 en de Clean Architecture-regels in `CLAUDE.md`.
- Hergebruik de bestaande inventory- en storage-servicepatronen voor trimmen,
  validatie, `InventoryOperationResult`/`StorageOperationResult` en repositorygebruik.
- `IRepository<T>` werkt met `Guid` en schrijft per mutatie direct via
  `SaveChangesAsync`; introduceer geen nieuwe repositoryabstractie of unit-of-worklaag.
- Gebruik echte numerieke validatie voor hoeveelheid; een actieve voorraadregel met
  waarde `0` of lager is functioneel ongeldig in deze slice.
- Bewaak uniqueness van `product + locatie` zowel functioneel als via relationele
  integriteit waar passend, zonder brede repositoryrefactor.
- Houd bestaande productcatalogusfunctionaliteit uit `PILOT-INV-01` intact; breid die
  uit waar nodig voor de locatieflow, maar verander geen afgeronde catalogusregels.
- Houd bestaande storage-detail-, QR- en autorisatiefunctionaliteit intact. De nieuwe
  locatievoorraad mag de huidige locatie-QR-flow niet regressief veranderen.
- Als terugkeer naar dezelfde locatieflow na inline productaanmaak technisch niet
  haalbaar blijkt binnen de bestaande componentstructuur zonder disproportionele
  refactor, stop en meld precies welk structuurbesluit ontbreekt in plaats van een
  halfwerkende alternatieve flow op te leveren.

## Acceptance Focus

- Owner en Crew kunnen handmatig een locatiepagina openen en daar `Voorraad toevoegen`
  starten.
- Binnen die flow werkt zoeken op productnaam en gekoppelde code.
- Nieuw product aanmaken vanuit locatiecontext keert bruikbaar terug naar dezelfde
  locatieflow met dat product geselecteerd.
- Nieuwe voorraad op een locatie maakt exact een regel aan; extra voorraad voor
  hetzelfde product op dezelfde locatie vult de bestaande hoeveelheid aan.
- Hoeveelheid `0` of lager wordt geblokkeerd vóór opslag.
- De locatiepagina toont na opslaan actuele inhoud met naam, hoeveelheid en eenheid.
- De productpagina toont alle gekoppelde locaties met gebied, locatienaam en
  hoeveelheid.
- Verwijderen van een voorraadregel vanaf de locatiepagina werkt alleen via expliciete
  bevestiging en laat de actuele inhoud daarna verdwijnen.
- Migratie en relationele constraints werken op echte tijdelijke SQLite, niet alleen
  via EF InMemory.
- Geen scan-, mutatiehistorie- of voorraadcorrectiegedrag van latere inventory-slices
  lekt deze slice binnen.

## Test Evidence Requirements

- Nieuwe tests moeten concreet deze productiegedragingen uitvoeren:
  - voorraadregel aanmaken voor nieuwe product-locatie-combinatie;
  - bestaande regel aanvullen op dezelfde locatie;
  - blokkade op hoeveelheid `0` of lager;
  - product zoeken op naam en gekoppelde code in locatiecontext;
  - voorraadregel verwijderen;
  - locatie-detailweergave met actuele inhoud;
  - productweergave met meerdere locaties;
  - inline terugkeerpad na nieuw product vanuit locatiecontext, als deze flow wordt
    aangepast of toegevoegd.
- Bewijs dat bestaande succes- en foutpaden van `PILOT-INV-01` en storage-detail niet
  regressief zijn geraakt waar de wijziging direct doorheen loopt.
- Voor UI-tests: render het echte component, voer echte interacties uit en assert
  zichtbare tekst, knopgedrag, servicecalls en statusverandering.
- Voor migratie/constrainttests: migreer expliciet vanaf
  `20260620120948_AddInventoryEntities`, controleer toegepaste migraties vóór en na,
  voeg vooraf geldige bestaande data toe en bewijs dat die data na upgrade behouden
  blijft naast de nieuwe voorraadtabellen en unieke product-locatie-beperking.
- Gebruik geen documentaire of placeholdertests.

## Required Checks

Voeg gerichte tests toe voor minimaal:

- servicegedrag voor aanmaken, aanvullen en verwijderen van voorraadregels;
- validatie van hoeveelheid en product-/locatiebestaan;
- zoeken op productnaam en gekoppelde code;
- componentgedrag op de locatiepagina voor `Voorraad toevoegen`, bevestigen en
  verwijderen;
- componentgedrag op de productpagina voor tonen van gekoppelde locaties;
- SQLite unique-indexen, foreign keys en toepassen van de nieuwe migratie op een
  tijdelijke database vanaf de vorige migratiestand, met behoud van bestaande data.

Voer eerst gerichte testfilters uit. Voer daarna uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~Inventory|FullyQualifiedName~Storage"
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~Inventory"
dotnet build BootManager.sln --no-restore
git diff --check
```

De bekende
`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect`-failure mag
alleen als ongerelateerde baseline worden gemeld wanneer exact die ene bestaande
unit-testfailure overblijft. Nieuwe of gewijzigde inventory-/storage-tests moeten
allemaal slagen.

## Definition of Technical Completion

Meld uitsluitend `gereed voor Codex-review` wanneer:

- ieder scopepunt en acceptatiecriterium technisch is geïmplementeerd;
- alle gerichte inventory-/storage-tests slagen;
- de volledige vereiste test- en buildruns geen nieuwe failure bevatten;
- build en `git diff --check` slagen;
- de migratie, product-locatie-uniqueness en relationele constraints aantoonbaar op
  tijdelijke SQLite zijn bewezen;
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
