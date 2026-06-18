# PILOT-LOC-01 Review Fix 4 Packet

## Task

- Required branch: `feature/pilot-loc-01-storage-locations`
- Trigger: handmatige acceptatietest
- Goal: laat de knop `Terug` op de locatie-detailpagina teruggaan naar de werkelijke
  vorige browserpagina in plaats van hardcoded naar `/`.

Dit is een minimale UI-correctie. Wijzig geen storagegedrag buiten de terugknop.

## Start Check

Controleer branch en worktree. Stop met `niet gereed` wanneer de branch niet exact
klopt of de bestaande implementatie ontbreekt. Reset, stash, checkout, commit of
verwijder niets.

Lees alleen:

- `CLAUDE.md`;
- dit packet;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- bestaande bUnit-tests onder `BootManager.UnitTests/Storage/` als patroon.

## Required Behavior

- De bestaande knoptekst `Terug` blijft behouden.
- Klikken gebruikt browsergeschiedenis (`history.back`) zodat de gebruiker terugkeert
  naar de pagina waarvandaan de detailpagina daadwerkelijk is geopend.
- Verwijder de hardcoded `Navigation.NavigateTo("/")`-navigatie.
- Gebruik de bestaande `IJSRuntime`-voorziening; voeg geen JavaScriptbestand, package,
  routerservice of return-urlmechanisme toe.
- Maak de handler asynchroon en wacht de JS-interopcall af.
- Wijzig route, autorisatie, detailweergave en storage-servicecalls niet.

Dit gedrag is bewust niet hardcoded naar `/settings`: `PILOT-LOC-02` zal dezelfde
detailpagina later vanuit QR-navigatie openen. Browsergeschiedenis bewaart de juiste
herkomst voor beide flows.

## Required bUnit Test

Voeg een gerichte echte bUnit-test toe onder `BootManager.UnitTests/Storage/`:

- render `StorageLocationDetails` met een geldig `LocationId`;
- registreer een test-double van `IStorageService` die een geldig detailresultaat
  teruggeeft;
- configureer bUnit JSInterop voor `history.back`;
- klik via de DOM op de knop `Terug`;
- assert dat `history.back` exact één keer is aangeroepen;
- assert niet alleen markup of methodenamen en gebruik geen `Assert.True(true)`.

Red-green-bewijs is reproduceerbaar: de nieuwe test moet vóór de productwijziging
falen omdat de huidige handler geen `history.back` aanroept, en erna slagen. Rapporteer
de rode en groene testnaam.

## Allowed Write-Set

Wijzig uitsluitend:

- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- één gerichte testfile onder `BootManager.UnitTests/Storage/`.

Wijzig geen andere Razor-component, service, DTO, entity, databasebestand, migratie,
snapshot, DI, authcode, documentatie of projectbestand. Voeg geen dependency toe. Maak
geen commit, push, branch, PR, merge of deployment.

## Required Checks

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StorageLocationDetails"
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~Storage"
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~Storage"
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
```

De volledige unitrun mag alleen de bekende baselinefailure
`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect` bevatten.

## Completion

Meld alleen `gereed voor Codex-review` wanneer de echte componenttest red-green-bewijs
levert, `history.back` exact één keer wordt aangeroepen, alle checks voldoen en alleen
de toegestane write-set door deze ronde is gewijzigd. Noem de story niet `Done`,
geaccepteerd of productierijp.
