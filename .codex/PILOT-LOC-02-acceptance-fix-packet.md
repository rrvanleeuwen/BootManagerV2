# PILOT-LOC-02 Acceptance Fix Packet

## Task

- Story ID: `PILOT-LOC-02`
- Task type: gerichte correctieronde op basis van handmatige acceptatiebevinding
- Required branch: `feature/pilot-loc-02-location-qr`
- Source implementation packet: `.codex/PILOT-LOC-02-implementation-packet.md`
- Existing review packets: `.codex/PILOT-LOC-02-review-fix-packet.md`, `.codex/PILOT-LOC-02-final-review-fix-packet.md`
- Goal: herstel exact de acceptatiefout waarbij een onbekende BootManager locatie-QR
  aan een bestaande locatie wordt gekoppeld, maar diezelfde locatie daarna nog steeds
  geen gekoppelde QR toont en opnieuw de knop `QR-token genereren` laat zien.

Dit is geen nieuwe story en geen herontwerp. Werk uitsluitend dit concrete defect af en
lever defectgevoelig regressiebewijs dat exact deze Owner-flow afdekt.

## Mandatory Start Check

Controleer vóór iedere wijziging:

1. De actieve branch is exact `feature/pilot-loc-02-location-qr` en niet `master`.
2. De bestaande `PILOT-LOC-02` worktreewijzigingen staan nog lokaal aanwezig.
3. Er zijn geen staged wijzigingen.

Rapporteer direct `niet gereed` en stop als één van deze drie checks faalt. Gebruik
geen reset, checkout, stash of verwijderactie.

## Minimal Context

Lees uitsluitend:

- `CLAUDE.md`;
- dit packet;
- `.codex/PILOT-LOC-02-implementation-packet.md`;
- alleen de sectie `PILOT-LOC-02` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Application/Storage/Services/IStorageService.cs`;
- `BootManager.Application/Storage/Services/StorageService.cs`;
- `BootManager.Application/Storage/DTOs/StorageLocationDetailDto.cs`;
- `BootManager.Application/Storage/QrFormat/LocationQrValue.cs`;
- `BootManager.Application/Storage/Results/QrResolutionResult.cs`;
- `BootManager.Core/Entities/StorageLocation.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/StorageLocationConfiguration.cs`;
- de QR-migratie `20260618192723_AddStorageLocationQrToken*` en model snapshot;
- `BootManager.Infrastructure/Repositories/EfRepository.cs`;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/LinkLocationQr.razor`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- huidige QR-gerelateerde tests onder `BootManager.UnitTests/Storage/`;
- `BootManager.IntegrationTests/Storage/StorageQrTokenIntegrationTests.cs`.

Lees geen bredere source trees, geen TODO, geen handoff, geen legacy-analyse en geen
ongerelateerde auth-, dashboard- of logboekcode.

## Exact Defect

### Observed acceptance failure

De handmatige Owner-flow op donderdag 18 juni 2026 gaf dit resultaat:

1. Open `Scannen`.
2. Voer een onbekende BootManager locatie-QR handmatig in.
3. Kies de Owner-koppelactie.
4. Koppel de QR aan een bestaande locatie.
5. Open daarna die locatie-detailpagina.

Verwacht:

- de locatie toont nu de gekoppelde QR-value;
- de knop `QR-token genereren` is verdwenen.

Feitelijk:

- de locatie-detailpagina toont opnieuw `QR-token genereren`;
- de zojuist gekoppelde QR lijkt dus niet duurzaam aan die locatie verbonden.

### Required interpretation

Behandel dit als een echte functionele bug, niet als een test- of acceptatiemisverstand.
Het defect zit in de bestaande link/persist/reload-keten totdat het tegendeel hard is
bewezen. Een groene testset zonder exacte reproductiedekking is onvoldoende.

## Required Behavior

Na jouw fix moet exact dit gelden:

- een onbekende geldige BootManager locatie-QR die aan een bestaande locatie wordt
  gekoppeld, blijft daarna aan die locatie gekoppeld;
- direct daarna én na een verse detail-load geeft `GetLocationDetailAsync` de
  gekoppelde `QrValue` terug;
- de detailpagina toont voor Owner daarna de QR-value en niet meer de knop
  `QR-token genereren`;
- hetzelfde gekoppelde token opent via de scanflow als bekende QR direct dezelfde
  locatie;
- de bestaande LOC-02-regels blijven gelden:
  - tokenvervanging blijft verboden;
  - generatie blijft idempotent;
  - Crew ziet geen token en geen generate-actie;
  - niet-BootManager waarden blijven generieke scanresultaten.

## Preserve These Behaviors

Behoud expliciet:

- QR-format exact `bootmanager:location:<32-lowercase-hex-token>`;
- Owner-only autorisatie op de koppelpagina;
- bekende QR navigeert direct naar `/storage/locations/{locationId}`;
- onbekende geldige QR toont alleen voor Owner een koppelactie;
- `StorageLocationDetails` back-button `history.back`;
- bestaande scan-racebescherming en handmatige invoerflow;
- verbod op tokenvervanging in `PILOT-LOC-02`.

## Deliverables

### Deliverable A: exact regression proof for the acceptance path

Voeg minimaal één defectgevoelige geautomatiseerde test toe die exact deze keten
uitvoert:

1. bestaande locatie zonder token;
2. onbekende geldige BootManager QR;
3. koppelen aan bestaande locatie;
4. verse herlaad- of nieuwe service/detail-read;
5. assert dat de locatie daarna de token/QR-value werkelijk heeft;
6. assert dat generate daarna niet meer van toepassing is.

Deze test moet echte productcode uitvoeren. Een test die alleen een mock op
`LinkQrToExistingLocationAsync` verifyt telt niet als bewijs voor deze bug.

### Deliverable B: root-cause fix

Herstel de daadwerkelijke oorzaak in de bestaande QR-link/persist/detailketen.

Toegestane oorzaken kunnen bijvoorbeeld zitten in:

- de koppelservice;
- repository persist/update-gedrag;
- detail-readmapping;
- componentflow rond link + vervolgload.

Je hoeft geen architectuur te verbreden. Los alleen de concrete oorzaak op.

### Deliverable C: acceptance-sensitive component coverage

Breid de bestaande bUnit-dekking uit of corrigeer haar zodanig dat ook werkelijk
bewijs bestaat voor:

- een locatie met bestaande `QrValue` toont voor Owner geen generateknop meer;
- de detailcomponent toont na een verse load de door de service geretourneerde
  `QrValue`;
- de bestaande “mock returns success” componenttests worden niet gepresenteerd als
  bewijs voor duurzame koppeling.

## Red-Green Evidence Requirement

Voor deze correctie geldt expliciet red-green bewijs:

- leg vast welke nieuwe of gewijzigde test eerst rood was of rood had moeten worden
  tegen het defect;
- als een echte pre-fix rode run technisch niet reproduceerbaar is, meld vóór de fix
  concreet waarom en lever een gelijkwaardig bewijs dat het defect echt gevoelig maakt;
- een groene test die alleen mocks of handmatig ingestelde DTO's controleert is geen
  regressiebewijs voor deze acceptatiefout.

## Allowed Write-Set

Wijzig uitsluitend:

- `BootManager.Application/Storage/Services/IStorageService.cs`;
- `BootManager.Application/Storage/Services/StorageService.cs`;
- `BootManager.Application/Storage/DTOs/StorageLocationDetailDto.cs`;
- `BootManager.Application/Storage/QrFormat/`;
- `BootManager.Application/Storage/Results/QrResolutionResult.cs`;
- `BootManager.Core/Entities/StorageLocation.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/StorageLocationConfiguration.cs`;
- `BootManager.Infrastructure/Migrations/20260618192723_AddStorageLocationQrToken*`;
- `BootManager.Infrastructure/Migrations/BootManagerDbContextModelSnapshot.cs`;
- `BootManager.Infrastructure/Repositories/EfRepository.cs` alleen als dit aantoonbaar
  de concrete root cause is;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/LinkLocationQr.razor`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- gerichte bestanden onder `BootManager.UnitTests/Storage/`;
- `BootManager.IntegrationTests/Storage/StorageQrTokenIntegrationTests.cs`.

Wijzig niets buiten deze write-set zonder vooraf concrete compile-time noodzaak te
melden. Voeg geen package toe.

## Explicitly Forbidden

Niet doen:

- story-, release-, README-, TODO-, legacy- of handoff-wijzigingen;
- commit, push, branch, PR, merge, release of deployment;
- verbreden naar `PILOT-LOC-03` of `PILOT-LOC-04`;
- auth-, layout-, dashboard-, logboek- of andere ongerelateerde refactors;
- test-only workarounds die de echte productketen omzeilen;
- het verlagen van acceptatie- of regressie-eisen.

## Required Checks

Voer eerst gerichte checks uit:

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
git status --short
git diff --stat
```

De volledige unitrun mag alleen de bekende baselinefailure bevatten:

`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect`

Elke andere failure betekent `niet gereed`.

## Definition of Technical Completion

Meld alleen `gereed voor Codex-review` wanneer:

- de acceptatiefout exact is gereproduceerd en afgedekt met defectgevoelig bewijs;
- een koppeling van onbekende QR naar bestaande locatie daarna ook bij verse detail-load
  zichtbaar blijft;
- `StorageLocationDetails` na die koppeling de QR-value toont en niet opnieuw
  `QR-token genereren`;
- bekende scan van dezelfde QR daarna direct dezelfde locatie blijft openen;
- alle vereiste tests, build en `git diff --check` acceptabel zijn;
- geen wijziging buiten de write-set is toegevoegd.

Meld `niet gereed` wanneer:

- de root cause niet hard is vastgesteld of opgelost;
- regressiebewijs alleen uit mocks of handmatig gebouwde DTO's bestaat;
- een nieuwe of gewijzigde test faalt;
- de acceptatieketen nog niet exact is afgedekt;
- build of `git diff --check` faalt;
- een extra write-area nodig blijkt zonder concrete rechtvaardiging.

Noem de story niet `Done`, geaccepteerd of productierijp.

## Completion Notes

Retourneer uitsluitend:

1. exacte gewijzigde bestanden;
2. exacte root cause van de acceptatiefout;
3. exacte nieuwe of gewijzigde testnamen en welk productiegedrag zij echt uitvoeren;
4. of een echte pre-fix rode run mogelijk was, en zo niet waarom niet;
5. alle test-, build- en diffcheckresultaten;
6. eindstatus `gereed voor Codex-review` of `niet gereed`, met concrete reden.
