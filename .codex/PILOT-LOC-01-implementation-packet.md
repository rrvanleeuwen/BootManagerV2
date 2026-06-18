# Implementation Packet

## Task

- Story ID: `PILOT-LOC-01`
- Approved story: opslaggebieden en opslaglocaties
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-LOC-01`
- Goal: lever persistent beheer van opslaggebieden en opslaglocaties en een gedeelde
  leesbare locatie-detailpagina als basis voor latere QR- en voorraadflows.
- Required branch: `feature/pilot-loc-01-storage-locations`

De story is al goedgekeurd. Formuleer haar niet opnieuw en vraag geen nieuw akkoord.
Geef een kort uitvoeringsplan, implementeer direct, voer de checks uit en rapporteer
volgens `Completion Notes`.

## Scope

- Voeg de persistente entiteiten `StorageArea` en `StorageLocation` toe. Gebruik
  stabiele `Guid`-id's; iedere locatie hoort verplicht bij precies één gebied.
- Een gebied heeft een verplichte naam. Een locatie heeft een verplichte naam en een
  optionele korte beschrijving.
- Laat de application-service invoer trimmen, lege namen weigeren en lege
  beschrijvingen als `null` opslaan.
- Maak gebiedsnamen hoofdletterongevoelig uniek en locatienamen
  hoofdletterongevoelig uniek binnen hetzelfde gebied. Dezelfde locatienaam in een
  ander gebied is toegestaan.
- Borg die uniqueness zowel in de application-service als met genormaliseerde velden
  en unieke database-indexen. Gebruik maximaal 100 tekens voor namen en 500 tekens
  voor de beschrijving.
- Ondersteun gebied aanmaken, lijst tonen, hernoemen en verwijderen. Weiger het
  verwijderen van een gebied zolang het locaties bevat; gebruik in EF/SQLite
  `DeleteBehavior.Restrict` en geen cascade-delete.
- Ondersteun locatie aanmaken, lijst tonen, bewerken, naar een ander bestaand gebied
  verplaatsen en verwijderen. Hernoemen of verplaatsen behoudt de locatie-id.
- Lever DTO's voor beheerweergave en locatie-detail zonder EF-entiteiten aan de UI
  bloot te stellen. Geef voorspelbare, gebruikersvriendelijke validatie- en
  not-found-resultaten terug; gebruik geen exceptions voor normale validatiefouten.
- Registreer de application-service via de bestaande DI-laag en gebruik de bestaande
  generieke repositories; introduceer geen nieuwe repositoryabstractie.
- Voeg opslagbeheer als aparte Razor-component toe onder een nieuwe sectie `Opslag`
  in de bestaande Owner-only pagina `/settings`. Houd CRUD-interactie in de component
  en domein-/validatielogica in de application-service.
- Voeg een route `/storage/locations/{LocationId:guid}` toe met autorisatie
  `Owner,Crew`. Toon gebiedsnaam, locatienaam en beschrijving en handel een onbekende
  id duidelijk af.
- Maak vanuit opslagbeheer iedere bestaande locatie handmatig aanklikbaar naar de
  detailpagina. Voeg voor Crew geen Settings- of opslagbeheerlink toe.
- Voeg één additieve EF Core-migratie toe en werk de model snapshot bij. Bestaande
  databases moeten zonder reset of dataverlies in-place kunnen migreren.

## Outside Scope

- Geen QR-token, tagstatus, QR-generatie, print/export of scan-routing.
- Geen product-, barcode-, voorraad-, hoeveelheid-, mutatie- of voorraadlogboekmodel.
- Geen product-locatiekoppeling of voorraadweergave op de locatie-detailpagina.
- Geen opslagbeheer voor Crew en geen wijziging aan de bestaande rollen of authflow.
- Geen generieke inventory-module, nieuwe dependency, architectuurrefactor,
  documentatie, commit, push, PR, release of deployment.

## Expected Write-Set

Wijzig alleen deze bestanden of modules, tenzij een noodzakelijke compile-time
dependency wordt ontdekt:

- `BootManager.Core/Entities/StorageArea.cs` en `StorageLocation.cs`;
- `BootManager.Application/Storage/` voor DTO's, resultaten, servicecontract en
  application-service;
- `BootManager.Application/DependencyInjection.cs`;
- `BootManager.Infrastructure/Persistence/BootManagerDbContext.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/Storage*Configuration.cs`;
- één nieuwe `BootManager.Infrastructure/Migrations/*AddStorageAreasAndLocations*`
  migratie plus `BootManagerDbContextModelSnapshot.cs`;
- `BootManager.Web/Components/Pages/Settings.razor`;
- een gerichte beheercomponent onder `BootManager.Web/Components/Settings/`;
- een locatie-detailpagina onder `BootManager.Web/Components/Pages/`;
- gerichte tests onder `BootManager.UnitTests/Storage/` en, voor relationele
  constraints/migratiebewijs, `BootManager.IntegrationTests/Storage/`.

Leg vóór wijziging buiten deze write-set uit waarom die nodig is.

## Execution Boundaries

- Implementeer alleen applicatiecode, migratie en tests die dit packet vereist.
- Controleer vóór bewerken dat de actieve branch exact
  `feature/pilot-loc-01-storage-locations` is en niet `master`. Rapporteer `niet
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
- `.codex/PILOT-LOC-01-implementation-packet.md`;
- de sectie `PILOT-LOC-01` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Core/Interfaces/IRepository.cs` en alleen relevante entityvoorbeelden;
- `BootManager.Infrastructure/Repositories/EfRepository.cs`;
- `BootManager.Infrastructure/Persistence/BootManagerDbContext.cs`, relevante
  configuratievoorbeelden en de actuele model snapshot;
- `BootManager.Application/DependencyInjection.cs` en één vergelijkbare CRUD-service
  met tests;
- alleen de opslag-invoegpunten in `BootManager.Web/Components/Pages/Settings.razor`;
- `BootManager.Web/Components/Pages/LogbookEntryDetails.razor` als
  `Owner,Crew`-routevoorbeeld;
- bestaande tijdelijke-SQLitepatronen in `BootManager.IntegrationTests/Authentication/`.

Gebruik gerichte zoekopdrachten en kleine bestandssecties. Lees niet standaard:

- de volledige `.docs/TODO.md` of andere releaseverhalen;
- `.docs/legacy-analysis/` of `.docs/legacy-input/`;
- `.codex/current-session-handoff.md` of `.codex/working-agreement.md`;
- ongerelateerde source trees, logboek-, scan-, dashboard- of NMEA-code.

## Existing Constraints

- Volg .NET 8 en de Clean Architecture-regels in `CLAUDE.md`.
- `IRepository<T>` werkt met `Guid` en schrijft per mutatie direct via
  `SaveChangesAsync`; ontwerp de service zonder een nieuwe unit-of-worklaag.
- De generieke repository retourneert entiteiten zonder navigaties. Bouw beheer- en
  detail-DTO's daarom met gerichte area/location-queries en expliciete mapping; breid
  de generieke repository niet uit met feature-specifieke includes.
- Gebruik genormaliseerde namen (`Trim().ToLowerInvariant()`) voor consistente
  hoofdletterongevoelige uniqueness op SQLite.
- De databaseconstraint is de laatste integriteitslaag. Vertaal een eventuele race bij
  een unieke index naar hetzelfde functionele validatieresultaat waar dit praktisch
  binnen de bestaande architectuur kan, zonder brede repositoryrefactor.
- De bestaande `/settings`-pagina is Owner-only. De nieuwe detailpagina moet expliciet
  `Owner,Crew` zijn en mag geen mutatieacties voor Crew bevatten.
- De migratie is additief: twee nieuwe tabellen, foreign key, restrict-delete en de
  vereiste unieke indexen. Er bestaat nog geen opslagdata om te backfillen.

## Acceptance Focus

- Correcte CRUD-flow met getrimde, hoofdletterongevoelig unieke namen.
- Een locatie blijft dezelfde `Guid` houden bij hernoemen en verplaatsen.
- Een gevuld gebied kan niet stilzwijgend of via cascade worden verwijderd.
- Owner kan beheren; Owner en Crew kunnen details lezen; Crew kan Settings niet
  openen en ziet op detail geen beheeracties.
- Migratie en relationele constraints werken op echte tijdelijke SQLite, niet alleen
  via EF InMemory.
- Geen QR-, tag- of inventoryfunctionaliteit lekt deze slice binnen.

## Required Checks

Voeg gerichte tests toe voor minimaal:

- gebied aanmaken/hernoemen/verwijderen en blokkade bij een gevuld gebied;
- trimmen, lege/te lange namen en hoofdletterongevoelige dubbele gebiedsnaam;
- locatie aanmaken/bewerken/verplaatsen/verwijderen met stabiele id;
- dubbele locatienaam binnen hetzelfde gebied blokkeren en dezelfde naam in een ander
  gebied toestaan;
- onbekende area/location-id en een genormaliseerde optionele beschrijving;
- SQLite unique-indexen, verplichte foreign key en restrict-delete;
- toepassen van de nieuwe migratie op een tijdelijke database vanaf de vorige
  migratiestand, met behoud van bestaande tabellen/data;
- waar praktisch met bUnit: opslagcomponent roept de juiste serviceflow aan en de
  detailpagina rendert gebied, naam, beschrijving en not-found-status.

Voer eerst de gerichte Storage-testfilters uit. Voer daarna uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
```

De bekende
`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect`-failure mag
alleen als ongerelateerde baseline worden gemeld wanneer exact die ene bestaande
unit-testfailure overblijft. Nieuwe of gewijzigde Storage-tests moeten allemaal slagen.

## Definition of Technical Completion

Meld uitsluitend `gereed voor Codex-review` wanneer:

- ieder scopepunt en acceptatiecriterium technisch is geïmplementeerd;
- alle gerichte Storage-tests slagen;
- de volledige unit- en integratietestruns geen nieuwe failure bevatten;
- build en `git diff --check` slagen;
- de migratie vanaf de vorige migratiestand en de SQLite-constraints aantoonbaar op
  tijdelijke databases zijn bewezen;
- geen onverklaarde wijziging buiten de verwachte write-set staat;
- de resterende handmatige Owner/Crew-acceptatiestappen expliciet zijn vermeld.

Meld `niet gereed` wanneer scope onvolledig is, migratie/constraints niet bewezen zijn,
een nieuwe of gewijzigde test faalt, build/diffcheck faalt, een vereiste beslissing
ontbreekt of extra write-area niet kan worden verantwoord. Verlaag geen test- of
acceptatie-eis en maskeer geen failure als waarschuwing.

## Completion Notes

Retourneer alleen:

1. gewijzigde bestanden en geïmplementeerd gedrag;
2. tests/checks en resultaten;
3. migratie- en configuratie-impact;
4. resterende risico's en exacte handmatige testvereisten;
5. eindstatus: `gereed voor Codex-review` of `niet gereed`, met concrete reden.
