# PILOT-LOC-02 Final Review Fix Packet

## Task

- Story ID: `PILOT-LOC-02`
- Task type: finale gerichte correctieronde na mislukte review-fix
- Required branch: `feature/pilot-loc-02-location-qr`
- Source implementation packet: `.codex/PILOT-LOC-02-implementation-packet.md`
- Previous fix packet: `.codex/PILOT-LOC-02-review-fix-packet.md`
- Goal: lever de exact ontbrekende bewijzen en tests zodat `PILOT-LOC-02` zonder
  verdere interpretatie volledig reviewbaar is.

De huidige codebasis bevat al de meeste QR-implementatie. Deze ronde is niet bedoeld
voor herontwerp. Je mag alleen de nog ontbrekende reviewrequirements invullen en
alleen gerichte correcties doen die daarvoor strikt nodig zijn.

## Mandatory Start Check

Controleer vóór iedere wijziging:

1. De actieve branch is exact `feature/pilot-loc-02-location-qr` en niet `master`.
2. De bestaande `PILOT-LOC-02` worktreewijzigingen staan nog lokaal aanwezig.
3. Er zijn geen staged wijzigingen.

Rapporteer `niet gereed` en stop direct als één van deze drie checks faalt.
Gebruik geen reset, checkout, stash of verwijderactie.

## Minimal Context

Lees uitsluitend:

- `CLAUDE.md`;
- dit packet;
- `.codex/PILOT-LOC-02-implementation-packet.md`;
- `.codex/PILOT-LOC-02-review-fix-packet.md`;
- alleen de sectie `PILOT-LOC-02` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/LinkLocationQr.razor`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- `BootManager.Application/Storage/Services/IStorageService.cs`;
- `BootManager.Application/Storage/Services/StorageService.cs`;
- `BootManager.Application/Storage/QrFormat/LocationQrValue.cs`;
- `BootManager.Application/Storage/Results/QrResolutionResult.cs`;
- `BootManager.Core/Entities/StorageLocation.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/StorageLocationConfiguration.cs`;
- de nieuwe QR-migratie en model snapshot;
- huidige tests onder `BootManager.UnitTests/Storage/`;
- `BootManager.UnitTests/Web/RouteAuthorizationTests.cs`;
- huidige tests onder `BootManager.IntegrationTests/Storage/`.

Lees geen bredere source trees, geen TODO, geen legacy-analyse, geen handoff.

## Exact Remaining Problems

Alleen deze drie probleemgroepen moeten nu nog worden opgelost:

### Problem 1: Required bUnit component coverage is still missing

De volgende bewijzen ontbreken nog:

- `Scan` Owner/Crew QR-behavior;
- `LinkLocationQr` full-QR query parsing en beide submitflows;
- `StorageLocationDetails` Owner/Crew QR-weergave en generate-flow;
- vervanging van de verwijderde `StorageLocationDetailsComponentTests`.

Alleen service- en integratietests zijn onvoldoende. Er moeten echte bUnit-tests
bij komen die de echte componenten renderen en gebruikersinteractie uitvoeren.

### Problem 2: Migration path from the previous migration is still unproven

De huidige integratietests migreren direct naar latest en bewijzen niet:

1. expliciete migratie naar `20260618175732_AddStorageAreasAndLocations`;
2. invoegen van bestaand area/location-data vóór de QR-migratie;
3. dispose van de eerste context;
4. heropenen van exact dezelfde tijdelijke SQLite-database;
5. upgrade naar latest;
6. databehoud na upgrade.

Dit upgradepad moet met een echte tijdelijke SQLite-database worden bewezen.

### Problem 3: Duplicate-token race translation still lacks regression evidence

De service vertaalt inmiddels duplicate-token exceptions, maar er is nog geen test die
bewijst dat een technische unique-conflict daadwerkelijk als functionele fout terugkomt.

Er moet dus minimaal één defectgevoelige test bij komen die dit expliciet uitvoert.

## Required Deliverables

Je mag pas `gereed voor Codex-review` melden als onderstaande deliverables allemaal
bestaan en groen zijn.

### Deliverable A: new Scan component test file

Maak een nieuw testbestand onder `BootManager.UnitTests/Storage/` voor `Scan.razor`.

Dat bestand moet echte bUnit-tests bevatten die minimaal bewijzen:

1. bekende QR navigeert direct naar `/storage/locations/{id}`;
2. onbekende geldige BootManager locatie-QR toont voor Owner een koppelactie;
3. die koppelactie navigeert met de volledige URL-encoded QR-value als `qrValue=...`;
4. dezelfde onbekende geldige QR toont voor Crew geen koppelactie;
5. een niet-BootManager waarde toont geen koppelactie.

Gebruik geen dummy-asserts op alleen tekststrings zonder interactie. Toon navigatie
via echte `NavigationManager`-state.

### Deliverable B: new LinkLocationQr component test file

Maak een nieuw testbestand onder `BootManager.UnitTests/Storage/` voor
`LinkLocationQr.razor`.

Dat bestand moet echte bUnit-tests bevatten die minimaal bewijzen:

1. ongeldige of verkeerd gevormde `qrValue` querystring toont de invalid-state;
2. geldige `qrValue` wordt opnieuw geparsed naar exact de token;
3. koppelen aan bestaande locatie roept `LinkQrToExistingLocationAsync` aan met exact
   die geparste token en het gekozen locatie-id;
4. nieuwe locatie aanmaken roept `CreateLocationWithQrTokenAsync` aan met exact de
   geparste token, area-id, naam en beschrijving;
5. beide succesflows navigeren naar de gekoppelde locatie-detailroute.

### Deliverable C: restored StorageLocationDetails component coverage

Herstel de verwijderde componentdekking door een bestaand of nieuw testbestand onder
`BootManager.UnitTests/Storage/` met echte bUnit-tests voor `StorageLocationDetails`.

Dat bestand moet minimaal bewijzen:

1. back button roept `history.back` aan;
2. Owner ziet een generate-knop als nog geen token bestaat;
3. na generate toont Owner de geretourneerde QR-value;
4. Crew ziet geen token en geen generate-knop;
5. een tweede render/reload met bestaande detaildata toont dezelfde QR-value zonder
   extra generatiecall.

`BootManager.UnitTests/Storage/StorageLocationDetailsComponentTests.cs` mag niet
verdwenen blijven zonder gelijkwaardige vervanging. Laat in je completion notes
expliciet zien in welk bestand deze dekking nu zit.

### Deliverable D: migration upgrade-path proof

Voeg in `BootManager.IntegrationTests/Storage/StorageQrTokenIntegrationTests.cs`
minimaal één echte upgrade-test toe die exact deze volgorde uitvoert:

1. maak een unieke tijdelijke SQLite databasefile;
2. open context A op die file;
3. migreer context A expliciet naar
   `20260618175732_AddStorageAreasAndLocations`;
4. verifieer dat de QR-migratie nog niet toegepast is;
5. voeg vóór upgrade een bestaand `StorageArea` en `StorageLocation` zonder token toe;
6. bewaar ids en waarden;
7. dispose context A volledig;
8. open context B op exact dezelfde file;
9. migreer context B naar latest;
10. verifieer dat de QR-migratie nu wel toegepast is;
11. lees de bestaande area/location terug;
12. bewijs dat ids, naam, beschrijving en area-relatie ongewijzigd zijn;
13. bewijs dat `QrToken` null is gebleven;
14. bewijs dat nieuwe tokenfunctionaliteit daarna nog bruikbaar is.

Gebruik hiervoor echte EF Core migrations. Geen `EnsureCreated`, geen InMemory provider.

### Deliverable E: race-translation regression evidence

Voeg minimaal één test toe die aantoont dat een duplicate-token unique-conflict niet
als technische exception doorlekt maar als functionele fout terugkomt.

Toegestane vormen:

- een gerichte unit-test die repository-update/add zodanig laat falen dat de service
  het unieke-tokenpad moet vertalen;
- of een echte SQLite-integratietest die een duplicate tokenconstraint triggert via de
  serviceflow en het functionele foutresultaat controleert.

Deze test moet het defect echt kunnen detecteren. Een test die alleen de pre-check
uitvoert telt niet.

## Allowed Write-Set

Wijzig uitsluitend:

- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/LinkLocationQr.razor`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- `BootManager.Application/Storage/Services/IStorageService.cs`;
- `BootManager.Application/Storage/Services/StorageService.cs`;
- `BootManager.Application/Storage/QrFormat/LocationQrValue.cs`;
- `BootManager.Application/Storage/Results/QrResolutionResult.cs`;
- `BootManager.Core/Entities/StorageLocation.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/StorageLocationConfiguration.cs`;
- `BootManager.Infrastructure/Migrations/20260618192723_AddStorageLocationQrToken*`;
- `BootManager.Infrastructure/Migrations/BootManagerDbContextModelSnapshot.cs`;
- gerichte bestanden onder `BootManager.UnitTests/Storage/`;
- `BootManager.UnitTests/Web/RouteAuthorizationTests.cs` alleen als nodig;
- `BootManager.IntegrationTests/Storage/StorageQrTokenIntegrationTests.cs`.

Wijzig niets buiten deze write-set. Voeg geen package toe.

## Explicitly Forbidden

Niet doen:

- story-, release-, README-, TODO-, legacy- of handoff-wijzigingen;
- commit, push, branch, PR, merge, release of deployment;
- verbreden van scope naar LOC-03 of LOC-04;
- package/dependency-wijzigingen;
- andere refactors of cleanups;
- testen vervangen door reflectie-, bronvorm- of placeholdertests.

## Required Checks

Voer eerst uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~Storage|FullyQualifiedName~RouteAuthorization"
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

## Hard Completion Gate

Je mag `gereed voor Codex-review` alleen melden als alle onderstaande uitspraken waar
zijn:

- er bestaat een echte bUnit-testfile voor `Scan`;
- er bestaat een echte bUnit-testfile voor `LinkLocationQr`;
- er bestaat herstelde echte bUnit-dekking voor `StorageLocationDetails`;
- de verwijderde detailcomponentdekking is aantoonbaar vervangen;
- er bestaat een echte upgrade-test vanaf
  `20260618175732_AddStorageAreasAndLocations`;
- er bestaat een defectgevoelige race-translation test;
- alle gerichte tests slagen;
- de volledige integratierun is groen;
- de volledige unitrun bevat alleen de bekende baselinefailure;
- build en `git diff --check` slagen;
- geen wijziging buiten de write-set is toegevoegd.

Ontbreekt één van deze punten, dan moet de eindstatus `niet gereed` zijn.

## Completion Notes

Retourneer uitsluitend:

1. exacte gewijzigde bestanden;
2. exacte nieuwe testbestanden;
3. per nieuw/gewijzigd testbestand de precieze testnamen en welk productiegedrag zij
   uitvoeren;
4. exacte naam van de upgrade-test vanaf
   `20260618175732_AddStorageAreasAndLocations`;
5. exacte naam van de race-translation test;
6. alle test-, build- en diffcheckresultaten;
7. eindstatus `gereed voor Codex-review` of `niet gereed`, met concrete reden.

Noem de story niet `Done`, geaccepteerd of productierijp.
