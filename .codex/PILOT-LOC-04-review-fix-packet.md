# PILOT-LOC-04 Review Fix Packet

## Task

- Story ID: `PILOT-LOC-04`
- Task type: gerichte correctieronde na Codex-review
- Required branch: `feature/pilot-loc-04-token-replacement-tag-overview`
- Source implementation packet: `.codex/PILOT-LOC-04-implementation-packet.md`
- Goal: herstel uitsluitend de door Codex vastgestelde LOC-04-afwijkingen zodat
  tokenvervanging alleen op al getagde locaties werkt, het migratie-upgradepad echt
  bewezen is en de worktree weer binnen de toegestane write-set valt.

De bestaande LOC-04-implementatie bevat bruikbare bouwstenen en mag niet opnieuw
worden ontworpen. Behoud de gekozen architectuurrichting, de tagstatusset, de
Owner-only overzichtsroute, het bestaande QR-tokenformaat en alle niet-gerelateerde
storage-, scan- en print/exportgedragingen. Dit is een minimale herstelronde, geen
refactor.

## Mandatory Start Check

Controleer vóór iedere wijziging:

1. De actieve branch is exact
   `feature/pilot-loc-04-token-replacement-tag-overview` en niet `master`.
2. De bestaande `PILOT-LOC-04`-implementatie staat nog on-gecommit in de worktree.
3. De index bevat geen onverwachte staged wijzigingen.

Stop en rapporteer `niet gereed` wanneer de branch niet klopt of de bestaande
implementatie ontbreekt. Reset, checkout, stash of verwijder geen bestaande
worktreewijzigingen.

## Minimal Context

Lees uitsluitend:

- `CLAUDE.md`;
- dit review-fix-packet;
- `.codex/PILOT-LOC-04-implementation-packet.md`;
- alleen de sectie `PILOT-LOC-04` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Core/Entities/StorageLocation.cs`;
- `BootManager.Core/Enums/TagStatus.cs`;
- `BootManager.Application/Storage/DTOs/StorageLocationDetailDto.cs`;
- `BootManager.Application/Storage/DTOs/StorageLocationOverviewDto.cs`;
- `BootManager.Application/Storage/Services/IStorageService.cs`;
- `BootManager.Application/Storage/Services/StorageService.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/StorageLocationConfiguration.cs`;
- de nieuwe migratie `20260619134724_AddStorageLocationTagStatus*` en de actuele model snapshot;
- `BootManager.Web/Components/Pages/StorageLocationTagOverview.razor`;
- de huidige LOC-04 storage-, route- en integratietests die al zijn toegevoegd of gewijzigd.

Lees geen brede source trees, TODO, legacy-analyse, handoff of ongerelateerde
featuredocumentatie.

## Defects To Fix

### Fix 1: Token replacement must require an existing token

#### Existing defect

De huidige vervangflow accepteert ook locaties zonder bestaand `QrToken`. Daardoor kan
de vervangactie impliciet een eerste token genereren en de locatie direct op
`Vervangen` zetten.

Dat schendt de goedgekeurde storyrichting: `PILOT-LOC-04` vervangt alleen een bestaand
label/token; eerste tokenaanmaak blijft de bestaande generate-flow uit `PILOT-LOC-02`.

#### Required behavior

- `ReplaceQrTokenAsync` weigert tokenvervanging voor een locatie zonder bestaand token
  met een functioneel foutresultaat.
- Het domeinmodel moet expliciet beschermen dat `ReplaceQrToken(...)` alleen gebruikt
  kan worden wanneer al een token bestaat.
- Locaties met bestaand token blijven wel vervangbaar; oud token wordt ongeldig en
  nieuw token wordt actief.
- De bestaande generate-flow en bestaande linkflow blijven ongewijzigd in betekenis.

### Fix 2: Restore real migration upgrade proof

#### Existing defect

De nieuwe integratietests migreren een lege database direct naar latest en voegen de
testdata pas daarna toe. Daarmee is niet bewezen dat een bestaande SQLite-database vanaf
de vorige migratie veilig upgrade naar `AddStorageLocationTagStatus` met behoud van
bestaande locaties en tokens.

#### Required behavior

- Bewijs expliciet het upgradepad vanaf
  `20260618192723_AddStorageLocationQrToken`.
- Gebruik één echte tijdelijke SQLite-database.
- Migreer eerst expliciet naar `20260618192723_AddStorageLocationQrToken`.
- Voeg vóór de nieuwe migratie bestaand `StorageArea` + meerdere `StorageLocation`
  records in, inclusief ten minste:
  - één locatie met bestaand token;
  - één locatie zonder token.
- Dispose de eerste context volledig.
- Open exact dezelfde database opnieuw.
- Migreer naar latest.
- Bewijs daarna:
  - bestaande locaties zijn behouden;
  - bestaande tokenwaarden zijn ongewijzigd;
  - het nieuwe `TagStatus`-veld is bruikbaar en default op bestaande rijen;
  - vervolgacties op de geüpgradede data blijven werken.

Direct latest op een lege database migreren blijft onvoldoende als enig migratiebewijs.

### Fix 3: Remove forbidden documentation change from Claude's write-set

#### Existing defect

De worktree bevat nog een wijziging in `.codex/current-session-handoff.md`, terwijl het
LOC-04-packet documentatiewijzigingen expliciet verbood.

#### Required behavior

- Neem `.codex/current-session-handoff.md` niet op in Claude's wijzigingsset.
- Laat de handoff exact buiten deze correctieronde.
- Rapporteer `niet gereed` als je denkt dat een documentatiewijziging alsnog nodig is;
  voer die niet zelf uit.

## Preserve These Behaviors

- De vier tagstatuswaarden blijven exact: `Niet geprint`, `Geprint`, `Gekoppeld`,
  `Vervangen`.
- De Owner-only route voor het tagoverzicht blijft intact.
- Het oude token resolveert na geldige vervanging niet meer; het nieuwe token wel.
- Locaties zonder token tonen in het overzicht nog steeds geen vervangknop.
- `PILOT-LOC-03` print/exportpad en bestaande tagpagina blijven functioneel.
- `PILOT-LOC-02` generate/link/resolvegedrag blijft ongewijzigd buiten de nieuwe
  vervangguard.

## Allowed Write-Set

Wijzig uitsluitend:

- `BootManager.Core/Entities/StorageLocation.cs`;
- `BootManager.Application/Storage/Services/IStorageService.cs` alleen als nodig voor
  signatuur- of foutresultaatconsistentie;
- `BootManager.Application/Storage/Services/StorageService.cs`;
- `BootManager.IntegrationTests/Storage/StorageTokenReplacementIntegrationTests.cs`;
- gerichte unittests onder `BootManager.UnitTests/Storage/` die de vervangguard
  bewijzen.

Laat bestaande LOC-04-bestanden buiten deze lijst ongemoeid tenzij een concrete
compile-time dependency dat aantoonbaar vereist. Wijzig `.codex/current-session-handoff.md`
niet.

## Explicitly Forbidden Changes

Wijzig niet:

- story-, release-, TODO-, legacy-, README-, handoff- of andere documentatie;
- `TagStatus`-namen of -semantiek;
- de nieuwe overzichtsroute, autorisatierichting of UI-opzet, behalve wanneer een
  kleine compile-time aanpassing onvermijdelijk is;
- scanroutering, print/export, producten of voorraad;
- andere tests om failures te verbergen;
- projectstructuur, DI-registraties, package-references of architectuurlagen.

Maak geen branch, commit, push, PR, merge, release of deployment.

## Test Evidence Requirements

Iedere nieuwe of gewijzigde test moet echte productcode of componenten uitvoeren en
concreet kunnen falen bij het bedoelde defect.

### Required unit/service tests

Bewijs minimaal:

- `ReplaceQrTokenAsync` retourneert een functionele fout voor een locatie zonder
  bestaand token;
- `ReplaceQrTokenAsync` blijft succesvol voor een locatie met bestaand token;
- de domein-/serviceguard voorkomt dat een tokenloze locatie via de vervangroute op
  `Replaced` eindigt.

### Required integration/migration tests

Bewijs minimaal:

1. migratie naar `20260618192723_AddStorageLocationQrToken`;
2. invoegen van bestaand area/location-data vóór de nieuwe migratie;
3. dispose van de eerste context;
4. heropenen van exact dezelfde tijdelijke database;
5. migreren naar latest;
6. bestaand databehoud van locaties en tokens;
7. default `TagStatus` op geüpgradede bestaande rijen;
8. bruikbaarheid van de vervang- en/of statusflow na de upgrade.

Gebruik geen `EnsureCreated` en geen EF InMemory-provider. Direct latest op een lege
database migreren is geen upgradebewijs.

Inspecteer iedere nieuwe/gewijzigde test:

- geen `Assert.True(true)`;
- geen lege test;
- geen bronvormtest als vervanging van gedrag;
- geen `async` test zonder relevante `await`.

## Required Checks

Voer eerst gerichte checks uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~StorageServiceQrTokenTests|FullyQualifiedName~StorageServiceTagStatusTests"
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore --filter "FullyQualifiedName~StorageTokenReplacementIntegrationTests"
```

Voer daarna uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
git status --short
git diff --stat
```

De volledige unitrun mag uitsluitend de bekende baselinefailure bevatten:

`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect`

Iedere andere failure betekent `niet gereed`.

Controleer daarna expliciet dat `.codex/current-session-handoff.md` niet meer als
Claude-wijziging in de worktree staat.

## Definition of Technical Completion

Rapporteer alleen `gereed voor Codex-review` wanneer:

- tokenvervanging voor een locatie zonder bestaand token functioneel wordt geweigerd;
- locaties met bestaand token nog steeds correct vervangbaar zijn;
- het migratie-upgradepad vanaf
  `20260618192723_AddStorageLocationQrToken` met databehoud is bewezen;
- alle gerichte tests slagen en de nieuwe tests defectgevoelig zijn;
- build en `git diff --check` slagen;
- geen verboden of onverklaarde wijziging is gemaakt;
- `.codex/current-session-handoff.md` buiten Claude's wijzigingsset blijft.

Rapporteer `niet gereed` wanneer een eis ontbreekt, een nieuwe test faalt, het
migratiebewijs onvolledig is, de build/diffcheck faalt of de toegestane write-set
onvoldoende blijkt. Verlaag geen test- of acceptatie-eis en maskeer geen failure als
waarschuwing.

## Completion Notes

Retourneer uitsluitend:

1. exacte gewijzigde bestanden;
2. hoe tokenloze locaties nu uit de vervangflow worden geweerd;
3. exacte nieuwe/gewijzigde testnamen en welk productiegedrag of defect zij uitvoeren;
4. de opzet en uitkomst van het migratie-upgradebewijs;
5. alle test-, build- en diffcheckresultaten;
6. bevestiging dat `.codex/current-session-handoff.md` niet door jou is gewijzigd;
7. eindstatus `gereed voor Codex-review` of `niet gereed`, met concrete reden.

Noem de story niet `Done`, geaccepteerd of productierijp.
