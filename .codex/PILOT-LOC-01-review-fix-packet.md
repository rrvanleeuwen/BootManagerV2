# PILOT-LOC-01 Review Fix Packet

## Task

- Story ID: `PILOT-LOC-01`
- Task type: gerichte correctieronde na Codex-review
- Required branch: `feature/pilot-loc-01-storage-locations`
- Source implementation packet: `.codex/PILOT-LOC-01-implementation-packet.md`
- Goal: herstel uitsluitend de gedeelde Blazor-state voor locatie-aanmaak en lever
  het ontbrekende bewijs voor een in-place migratie vanaf de vorige migratiestand.

De bestaande storage-implementatie functioneert grotendeels en mag niet opnieuw
worden ontworpen. Behoud alle huidige domeinlogica, databaseconfiguratie, migratie,
autorisatie, routes en CRUD-semantiek. Dit is een minimale correctieronde, geen
refactor.

## Mandatory Start Check

Controleer vóór iedere wijziging:

1. De actieve branch is exact `feature/pilot-loc-01-storage-locations` en niet
   `master`.
2. De bestaande `PILOT-LOC-01`-implementatie staat nog on-gecommit in de worktree.
3. De index bevat geen onverwachte staged wijzigingen.

Stop en rapporteer `niet gereed` wanneer de branch niet klopt of de bestaande
implementatie ontbreekt. Reset, checkout, stash of verwijder geen bestaande
worktreewijzigingen.

## Minimal Context

Lees uitsluitend:

- `CLAUDE.md`;
- dit review-fix-packet;
- `.codex/PILOT-LOC-01-implementation-packet.md`;
- `BootManager.Web/Components/Settings/StorageManagement.razor`;
- de bestaande tests onder `BootManager.UnitTests/Storage/`;
- `BootManager.IntegrationTests/Storage/StorageMigrationAndConstraintsTests.cs`;
- alleen kleine, gerichte voorbeelden die nodig zijn om bestaande bUnit- of
  migratietestpatronen te volgen.

Lees geen brede source trees, storydocumentatie, TODO, legacy-analyse of
ongerelateerde features.

## Fix 1: Isolate Blazor State for Location Creation

### Existing defect

`StorageManagement.razor` rendert voor ieder opslaggebied invoervelden voor een
nieuwe locatie, maar alle rijen delen dezelfde componentvelden:

- `_newLocationName`;
- `_newLocationDesc`;
- `_locationError`.

Daardoor kunnen invoer en validatiefouten van het ene gebied in andere gebiedsrijen
verschijnen.

### Required behavior

Gebruik één expliciet geselecteerd gebied voor het aanmaken van een locatie:

- Toon per gebied een knop `Locatie toevoegen`.
- Klikken selecteert precies dat gebied als actief aanmaakgebied.
- Render precies één aanmaakformulier, uitsluitend bij het geselecteerde gebied.
- Bewaar voor dat formulier het geselecteerde area-id, locatienaam, beschrijving en
  de eventuele validatiefout.
- Roep `CreateLocationAsync` altijd aan met het id van het geselecteerde gebied.
- Na succesvol aanmaken: sluit het formulier, wis invoer en foutmelding en laad de
  lijsten opnieuw.
- Bij annuleren: sluit het formulier, wis invoer en foutmelding en voer geen
  servicecall uit.
- Bij overschakelen van gebied A naar gebied B: selecteer B en begin met lege invoer
  en zonder de foutmelding van A.
- Render een locatiegerelateerde foutmelding maximaal één keer.
- Laat bewerken, verplaatsen en verwijderen van bestaande locaties functioneel
  ongewijzigd.

Gebruik bij voorkeur minimale state van deze vorm:

- `Guid? _createLocationAreaId`;
- één naamveld;
- één beschrijvingsveld;
- één foutveld.

Introduceer geen dictionary met mutable formulierstate per gebied, tenzij de
geselecteerde-formulieroplossing aantoonbaar niet uitvoerbaar is. Meld dat dan vóór
een alternatieve implementatie.

### Blazor restrictions

- Behoud de bestaande globale `InteractiveServer`-opzet; voeg geen nieuw
  `@rendermode` toe.
- Behoud `StorageManagement` als childcomponent van de Owner-only `Settings.razor`.
- Verplaats opslagbeheer niet naar een controller, API-endpoint of JavaScriptmodule.
- Gebruik normale Blazor-eventhandlers en databinding.
- Voeg geen state-managementframework, package of dependency toe.
- Wijzig route of autorisatie van `StorageLocationDetails.razor` niet.
- Wijzig geen andere Settings-secties.
- Voer geen algemene Razor-, Bootstrap-, styling- of formattingrefactor uit.

## Fix 2: Prove Migration from the Previous Migration

### Missing proof

De bestaande migratietest maakt een lege database en migreert direct naar latest.
Dat bewijst niet dat een bestaande BootManager-database vanaf de vorige migratie
veilig wordt bijgewerkt en bestaande data behoudt.

### Required integration test

Voeg één gerichte integratietest toe die uitsluitend een unieke tijdelijke
SQLite-database gebruikt en exact deze volgorde uitvoert:

1. Maak een nieuwe tijdelijke SQLite-database.
2. Migreer expliciet naar
   `20260609204357_MigrateOwnerProfileToLocalUser`.
3. Voeg vóór de storagemigratie een bestaand record toe aan een reeds bestaande
   tabel. Gebruik bij voorkeur een `VesselProfile` met herkenbare waarden, waaronder
   bootnaam `Linde`.
4. Bewaar id en waarden voor latere asserts.
5. Dispose de eerste `BootManagerDbContext` volledig.
6. Open een nieuwe context op exact dezelfde tijdelijke database.
7. Migreer met EF Core migrations naar de nieuwste migratiestand.
8. Bewijs dat het bestaande `VesselProfile` nog aanwezig is en id en opgeslagen
   waarden ongewijzigd zijn.
9. Bewijs dat `StorageAreas` en `StorageLocations` bestaan door na de migratie een
   gebied en bijbehorende locatie persistent op te slaan en terug te lezen.
10. Verwijder de tijdelijke database in betrouwbare cleanup, ook bij een falende
    assert.

Gebruik bij voorkeur rechtstreeks `BootManagerDbContext`, SQLite-options en EF Core
`IMigrator`. Gebruik geen `WebApplicationFactory` als applicatiestartup de database
mogelijk al naar latest migreert.

### Migration restrictions

- Wijzig `20260618175732_AddStorageAreasAndLocations` niet.
- Genereer geen vervangende of extra migratie.
- Wijzig de model snapshot niet.
- Raak geen productie-, ontwikkel- of Raspberry Pi-database aan.
- Gebruik geen `EnsureCreated`; dat bewijst geen migratiepad.
- Gebruik geen EF InMemory-provider.
- Alleen bewijzen dat tabellen bestaan is onvoldoende; bestaand databehoud is
  verplicht.
- Verwijder of verzwak geen bestaande constrainttest.

## Required bUnit Tests

Voeg gerichte bUnit-tests toe voor `StorageManagement` en bewijs minimaal:

1. Met twee gebieden is aanvankelijk geen locatie-aanmaakformulier geopend.
2. `Locatie toevoegen` bij `Kombuis` toont precies één formulier voor `Kombuis`.
3. Invullen en opslaan roept `CreateLocationAsync` precies één keer aan met het id
   van `Kombuis`, de ingevoerde naam en de ingevoerde beschrijving.
4. Het id van een ander gebied, bijvoorbeeld `Salon`, wordt niet gebruikt.
5. Overschakelen van `Kombuis` naar `Salon` wist oude invoer en foutmelding.
6. Annuleren voert geen create-call uit.
7. Een servicevalidatiefout wordt precies één keer getoond en alleen bij het actieve
   formulier.

Gebruik een mock of kleine test-double van `IStorageService`. Test componentgedrag;
wijzig geen productieservice om tests eenvoudiger te maken.

Als bUnit door een concrete bestaande technische beperking niet uitvoerbaar is, stop
dan met `niet gereed` en rapporteer die beperking. Sla deze tests niet over en vervang
ze niet door tests van alleen de application-service.

## Allowed Write-Set

Wijzig uitsluitend:

- `BootManager.Web/Components/Settings/StorageManagement.razor`;
- nieuwe of bestaande gerichte tests onder `BootManager.UnitTests/Storage/`;
- `BootManager.IntegrationTests/Storage/StorageMigrationAndConstraintsTests.cs`.

Een wijziging buiten deze drie gebieden is niet toegestaan. Stop en rapporteer eerst
de exacte compile-time noodzaak als deze write-set aantoonbaar onvoldoende is.

## Explicitly Forbidden Changes

Wijzig niet:

- `StorageArea` of `StorageLocation`;
- `StorageService`, `IStorageService`, DTO's of operation-resultaten;
- EF-configuraties;
- bestaande migraties of de model snapshot;
- `BootManagerDbContext`;
- DI-registraties;
- `Settings.razor`;
- `StorageLocationDetails.razor`;
- authenticatie, routes, claims, middleware of rollen;
- andere tests om failures te onderdrukken;
- story-, release-, TODO-, legacy-, README-, handoff- of andere documentatie.

Voer geen brede rename, formattingpass, dependency-update of architectuurrefactor
uit. Maak geen branch, commit, push, PR, merge, release of deployment.

## Required Checks

Voer eerst uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~Storage"
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~Storage"
```

Voer daarna uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
```

De volledige unitrun mag uitsluitend de bekende baselinefailure bevatten:

`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect`

Iedere andere failure betekent `niet gereed`.

Controleer ten slotte:

```powershell
git status --short
git diff --stat
git diff -- BootManager.Web/Components/Settings/StorageManagement.razor
git diff -- BootManager.UnitTests/Storage
git diff -- BootManager.IntegrationTests/Storage/StorageMigrationAndConstraintsTests.cs
```

Bevestig dat deze herstelronde geen bestand buiten de toegestane write-set heeft
gewijzigd. Bestaande wijzigingen uit de oorspronkelijke implementation blijven
uiteraard in de worktree staan en mogen niet als nieuwe review-fixwijzigingen worden
geclaimd.

## Definition of Technical Completion

Rapporteer alleen `gereed voor Codex-review` wanneer:

- locatie-aanmaakstate ondubbelzinnig aan één geselecteerd gebied is gekoppeld;
- alle verplichte bUnit-scenario's slagen;
- migratie vanaf de vorige migratie met bestaand databehoud is bewezen;
- bestaande storageconstrainttests blijven slagen;
- de volledige test-, build- en diffchecks voldoen;
- geen verboden of onverklaarde wijziging is gemaakt.

Rapporteer `niet gereed` wanneer een eis ontbreekt, een nieuwe test faalt, de
migratieproef onvolledig is, de build/diffcheck faalt of de toegestane write-set
onvoldoende blijkt. Verlaag geen test- of acceptatie-eis en maskeer geen failure als
waarschuwing.

## Completion Notes

Retourneer uitsluitend:

1. exacte gewijzigde bestanden;
2. hoe de Blazor-state nu aan één geselecteerd gebied is gekoppeld;
3. bewezen bUnit-scenario's;
4. opzet en resultaat van de migratieproef vanaf de vorige migratie;
5. alle test-, build- en diffcheckresultaten;
6. eindstatus `gereed voor Codex-review` of `niet gereed`, met concrete reden.

Noem de story niet `Done`, geaccepteerd of productierijp.
