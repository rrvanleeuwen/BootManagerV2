# PILOT-LOC-01 Review Fix 2 Packet

## Task

- Required branch: `feature/pilot-loc-01-storage-locations`
- Goal: corrigeer de afgekeurde eerste herstelronde zonder de werkende storagebasis
  opnieuw te ontwerpen.
- Status: de huidige tests zijn groen, maar de nieuwe componenttests zijn schijntests
  en de migratietest migreert niet echt vanaf de vorige migratie.

Voer alleen dit packet uit. De oorspronkelijke implementation en de huidige
Blazor-statefix blijven behouden, behalve waar dit packet expliciet een correctie
vereist.

## Start Check

Controleer eerst branch en worktree. Stop met `niet gereed` wanneer de branch niet
exact klopt of de bestaande `PILOT-LOC-01`-wijzigingen ontbreken. Reset, stash,
checkout, commit of verwijder niets.

Lees alleen:

- `CLAUDE.md`;
- dit packet;
- `BootManager.Web/Components/Settings/StorageManagement.razor`;
- `BootManager.UnitTests/Storage/StorageManagementComponentTests.cs`;
- `BootManager.IntegrationTests/Storage/StorageMigrationAndConstraintsTests.cs`;
- alleen een bestaand bUnit-voorbeeld of EF-migratie-API-documentatie uit de lokale
  code wanneer dat nodig is voor compilatie.

## Fix A: Replace the Fake Component Tests

Vervang alle tests in `StorageManagementComponentTests.cs` die alleen commentaar,
`Assert.True(true)` of inspectie van veldnamen bevatten door echte bUnit-tests.

Verplicht:

- Gebruik bUnit `TestContext` en `RenderComponent<StorageManagement>()`.
- Registreer een test-double van `IStorageService` in DI.
- De test-double retourneert twee gebieden: `Kombuis` en `Salon`.
- De test-double registreert iedere `CreateLocationAsync`-call inclusief area-id,
  naam en beschrijving en kan een configureerbaar foutresultaat teruggeven.
- Interacteer met de gerenderde DOM via bUnit; roep private componentmethoden niet via
  reflectie aan.
- Voeg zo nodig stabiele `data-testid`-attributen toe aan uitsluitend
  `StorageManagement.razor`. Gebruik geen selectors die afhankelijk zijn van de
  volgorde van alle Bootstrapknoppen.

Bewijs met echte DOM-asserts en call-asserts minimaal:

1. Initieel is geen locatie-aanmaakformulier zichtbaar.
2. Klik op `Locatie toevoegen` voor `Kombuis`: precies één formulier verschijnt en
   de kop noemt `Kombuis`.
3. Vul naam en beschrijving in en klik `Opslaan`: exact één create-call bevat het
   `Kombuis`-id en de ingevoerde waarden; het `Salon`-id is niet gebruikt.
4. Open eerst `Kombuis`, vul waarden in en laat de test-double een fout teruggeven;
   de fout wordt precies één keer getoond.
5. Schakel daarna naar `Salon`: naam, beschrijving en oude fout zijn gewist en de kop
   noemt `Salon`.
6. Klik `Annuleren`: het formulier sluit en er volgt geen extra create-call.

Geen enkele nieuwe test mag `Assert.True(true)` bevatten. Iedere test moet het echte
component renderen en kan aantoonbaar falen wanneer area-selectie, state-reset,
serviceargumenten of foutrendering defect zijn.

Omdat de statefix al in de on-gecommitte worktree staat, is een oorspronkelijke rode
run niet meer betrouwbaar reproduceerbaar. Meld dit als concrete reden en lever als
gelijkwaardig bewijs de echte defectgevoelige bUnit-tests plus de exacte DOM- en
call-asserts. Revert de werkende fix niet om kunstmatig rood te produceren.

## Fix B: Restore Existing Operation Errors

De eerste herstelronde heeft foutafhandeling verwijderd uit:

- `SaveLocation`;
- `DeleteLocation`;
- `MoveLocation`.

Herstel dit zonder de service of DTO's te wijzigen:

- Voeg één afzonderlijke foutstate toe voor mutaties van bestaande locaties,
  bijvoorbeeld `_locationOperationError`.
- Render die melding maximaal één keer buiten de gebiedslus.
- Wis de melding bij de start van iedere update-, delete- en move-operatie.
- Bij `result.Success == false` toon je `result.ErrorMessage` met dezelfde fallback
  `Onbekende fout.` als vóór de refactor.
- Laat bij een mislukte update de editmodus open.
- Laat bij een mislukte move de move-dialoog open.
- Verwijder of herlaad niets na een mislukte delete.
- Verander het bestaande succesgedrag niet.

Voeg echte bUnit-regressietests toe voor minimaal een mislukte update en mislukte
move. Bewijs dat de servicefout zichtbaar is en dat respectievelijk editmodus en
move-dialoog open blijven. De test-double mag hiervoor configureerbare resultaten
voor update en move ondersteunen.

## Fix C: Implement a Real Previous-Migration Upgrade Test

Corrigeer uitsluitend de test
`Migration_PreservesExistingDataFromPreviousMigration`.

Verplicht testverloop:

1. Maak een unieke tijdelijke SQLite-database.
2. Maak een eerste `BootManagerDbContext`.
3. Verkrijg EF Core `IMigrator` via `context.Database.GetService<IMigrator>()`.
4. Voer exact uit:
   `MigrateAsync("20260609204357_MigrateOwnerProfileToLocalUser")`.
5. Assert met `GetAppliedMigrationsAsync()` dat deze vorige migratie toegepast is en
   `20260618175732_AddStorageAreasAndLocations` nog niet.
6. Voeg een `VesselProfile` voor `Linde` toe en bewaar id en alle testwaarden.
7. Dispose de eerste context volledig.
8. Open een tweede context op hetzelfde databasebestand en migreer naar latest.
9. Assert dat `20260618175732_AddStorageAreasAndLocations` nu wel toegepast is.
10. Assert dat het bestaande `VesselProfile` met hetzelfde id en dezelfde waarden
    aanwezig is.
11. Sla daarna een `StorageArea` en gekoppelde `StorageLocation` op en lees beide
    terug.
12. Verwijder het tijdelijke databasebestand in `finally`.

In de eerste context is `Database.MigrateAsync()` zonder doelmigratie verboden. Gebruik
geen `EnsureCreated`, WebApplicationFactory, InMemory-provider of productieconfiguratie.
Wijzig geen migratie of snapshot.

## Allowed Write-Set

Wijzig uitsluitend:

- `BootManager.Web/Components/Settings/StorageManagement.razor`;
- `BootManager.UnitTests/Storage/StorageManagementComponentTests.cs`;
- `BootManager.IntegrationTests/Storage/StorageMigrationAndConstraintsTests.cs`.

Stop vóór iedere wijziging buiten deze set en rapporteer de concrete compile-time
noodzaak. Wijzig geen service, entity, DTO, DI, DbContext, migratie, snapshot, andere
Razor-pagina, authcode, documentatie of projectbestand. Voeg geen package toe. Maak
geen commit, push, branch, PR, merge of deployment.

## Required Checks

Voer uit:

```powershell
rg -n "Assert\.True\(true\)|test documents|test documentation" BootManager.UnitTests/Storage/StorageManagementComponentTests.cs
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StorageManagementComponentTests"
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~Storage"
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~Migration_PreservesExistingDataFromPreviousMigration"
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~Storage"
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
```

De eerste `rg`-opdracht moet geen matches geven. De volledige unitrun mag alleen de
bekende baselinefailure
`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect` bevatten.
Iedere andere failure betekent `niet gereed`.

## Completion

Meld alleen `gereed voor Codex-review` wanneer:

- alle schijntests vervangen zijn door echte bUnit-interactietests;
- create-, state-reset- en foutgedrag concreet via DOM en servicecalls is bewezen;
- update- en move-foutafhandeling hersteld en getest is;
- de migratietest expliciet vanaf de vorige migratie werkt en applied migrations plus
  databehoud assert;
- alle checks voldoen en alleen de toegestane bestanden door deze ronde zijn geraakt.

Rapporteer exact gewijzigde bestanden, testnamen en wat iedere test werkelijk
uitvoert, migratiebewijs, checkresultaten en eindstatus. Noem de story niet `Done`,
geaccepteerd of productierijp.
