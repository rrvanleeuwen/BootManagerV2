# PILOT-LOC-01 Review Fix 3 Packet

## Task

- Required branch: `feature/pilot-loc-01-storage-locations`
- Trigger: handmatige acceptatietest
- Goal: verbeter uitsluitend twee geconstateerde UX-problemen in opslagbeheer.

De bestaande storagefunctionaliteit, autorisatie, service, migratie en overige UI zijn
goedgekeurd. Dit is een minimale UI-correctie.

## Start Check

Controleer branch en worktree. Stop met `niet gereed` wanneer de branch niet exact
klopt of de bestaande `PILOT-LOC-01`-implementatie ontbreekt. Reset, stash, checkout,
commit of verwijder niets.

Lees alleen:

- `CLAUDE.md`;
- dit packet;
- `BootManager.Web/Components/Settings/StorageManagement.razor`;
- `BootManager.UnitTests/Storage/StorageManagementComponentTests.cs`.

## Fix 1: Create Location in a Modal

### Existing problem

Na `Locatie toevoegen` verschijnt het formulier onder alle opslaggebieden. Bij veel
gebieden moet de gebruiker naar de onderkant van de pagina scrollen. Dit is tijdens de
handmatige acceptatietest afgekeurd.

### Required behavior

- Iedere gebiedssectie behoudt de knop `Locatie toevoegen`.
- Klikken opent direct één modal boven de huidige pagina, volgens hetzelfde bestaande
  Blazor/Bootstrappatroon als de verplaatsmodal in deze component.
- De modaltitel noemt het geselecteerde gebied, bijvoorbeeld
  `Locatie toevoegen in Kombuis`.
- De modal bevat locatienaam, optionele beschrijving, `Opslaan`, `Annuleren` en een
  sluitknop.
- De bestaande geselecteerde-area-state blijft de bron voor het area-id; introduceer
  geen state per gebied.
- `Opslaan` roept `CreateLocationAsync` exact één keer aan met het geselecteerde
  area-id en de ingevoerde waarden.
- Bij succes sluit de modal, worden invoer en fout gewist en worden gegevens herladen.
- Bij een servicefout blijft de modal open en verschijnt de fout precies één keer in
  de modal.
- `Annuleren` en de sluitknop sluiten de modal, wissen invoer en fout en voeren geen
  create-call uit.
- Klikken op `Locatie toevoegen` voor een ander gebied opent dezelfde modal met het
  nieuwe gebied en lege state.
- Het oude formulier onder de gebiedslijst wordt volledig verwijderd.

Gebruik geen Bootstrap-JavaScriptinterop, nieuw package of nieuw modalframework. De
bestaande conditioneel gerenderde `.modal.show.d-block`-aanpak is voldoende. Voeg een
achtergrondlaag toe zoals bij de bestaande verplaatsmodal. Houd create- en move-modal
als afzonderlijke toestanden en acties.

## Fix 2: Open Location Details in the Same Tab

### Existing problem

Een klik op een locatie opent de detailpagina in een tweede browsertabblad.

### Required behavior

- Verwijder `target="_blank"` van de locatielink.
- Open `/storage/locations/{id}` via normale Blazor-navigatie in hetzelfde tabblad.
- Behoud route, locatie-id en linktekst.
- Wijzig `StorageLocationDetails.razor` niet.

## Required bUnit Regression Tests

Werk de bestaande echte componenttests bij; vervang ze niet door schijntests.

Bewijs minimaal:

1. Initieel is geen create-modal zichtbaar.
2. Klik op `Locatie toevoegen` voor `Kombuis`: precies één zichtbare create-modal
   verschijnt en de titel noemt `Kombuis`.
3. De create-modal staat niet als kaart/formulier onder de volledige gebiedslijst.
4. Invullen en opslaan gebruikt exact het geselecteerde `Kombuis`-id en sluit de modal
   bij succes.
5. Een servicefout verschijnt precies één keer in de modal en de modal blijft open.
6. `Annuleren` sluit de modal en doet geen create-call.
7. De sluitknop sluit de modal en doet geen create-call.
8. De locatielink bevat de juiste detail-URL en heeft geen `target="_blank"`.
9. De bestaande update- en move-fouttests blijven slagen.

Gebruik echte `RenderComponent`, DOM-clicks/changes en concrete asserts. Voeg indien
nodig stabiele `data-testid`-attributen toe. Gebruik geen `Assert.True(true)`, lege test
of broncode-inspectie als vervanging voor componentgedrag.

Red-green-bewijs is hier reproduceerbaar met de huidige implementatie: de nieuwe tests
voor `.modal.show`, afwezigheid van `target="_blank"` en afwezigheid van het onderste
formulier moeten vóór de productwijziging falen. Rapporteer de rode testnamen en
daarna de groene resultaten.

## Allowed Write-Set

Wijzig uitsluitend:

- `BootManager.Web/Components/Settings/StorageManagement.razor`;
- `BootManager.UnitTests/Storage/StorageManagementComponentTests.cs`.

Wijzig geen service, entity, DTO, databasebestand, migratie, snapshot, DI,
`Settings.razor`, `StorageLocationDetails.razor`, authcode, documentatie of
projectbestand. Voeg geen dependency toe. Maak geen commit, push, branch, PR, merge of
deployment.

## Required Checks

Voer uit:

```powershell
rg -n "Assert\.True\(true\)|target=\"_blank\"" BootManager.UnitTests/Storage/StorageManagementComponentTests.cs BootManager.Web/Components/Settings/StorageManagement.razor
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StorageManagementComponentTests"
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~Storage"
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~Storage"
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
```

De eerste `rg`-opdracht moet na de fix geen matches geven. De volledige unitrun mag
alleen de bekende baselinefailure
`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect` bevatten.

## Completion

Meld alleen `gereed voor Codex-review` wanneer de modal- en same-tabvereisten met echte
bUnit-tests zijn bewezen, bestaande storage-tests blijven slagen, build/diffcheck
voldoen en alleen de toegestane write-set door deze ronde is gewijzigd.

Rapporteer exact gewijzigde bestanden, rode en groene testnamen, alle checkresultaten
en eindstatus. Noem de story niet `Done`, geaccepteerd of productierijp.
