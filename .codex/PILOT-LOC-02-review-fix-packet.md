# PILOT-LOC-02 Review Fix Packet

## Task

- Story ID: `PILOT-LOC-02`
- Task type: gerichte correctieronde na Codex-review
- Required branch: `feature/pilot-loc-02-location-qr`
- Source implementation packet: `.codex/PILOT-LOC-02-implementation-packet.md`
- Goal: herstel uitsluitend de door Codex vastgestelde QR-defecten en lever het
  ontbrekende bewijs zodat `PILOT-LOC-02` in één reviewronde technisch volledig kan
  worden beoordeeld.

De bestaande implementatie bevat bruikbare QR-bouwstenen en mag niet opnieuw worden
ontworpen. Behoud de gekozen architectuurrichting, het tokenformaat
`bootmanager:location:<32-lowercase-hex-token>`, de bestaande routes, de Owner/Crew
rollen, de LOC-02-scope en alle niet-gerelateerde scanner- en storagegedragingen.
Dit is een minimale herstelronde, geen refactor.

## Mandatory Start Check

Controleer vóór iedere wijziging:

1. De actieve branch is exact `feature/pilot-loc-02-location-qr` en niet `master`.
2. De bestaande `PILOT-LOC-02`-implementatie staat nog on-gecommit in de worktree.
3. De index bevat geen onverwachte staged wijzigingen.

Stop en rapporteer `niet gereed` wanneer de branch niet klopt of de bestaande
implementatie ontbreekt. Reset, checkout, stash of verwijder geen bestaande
worktreewijzigingen.

## Minimal Context

Lees uitsluitend:

- `CLAUDE.md`;
- dit review-fix-packet;
- `.codex/PILOT-LOC-02-implementation-packet.md`;
- de sectie `PILOT-LOC-02` in `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Core/Entities/StorageLocation.cs`;
- `BootManager.Application/Storage/DTOs/StorageLocationDetailDto.cs`;
- `BootManager.Application/Storage/Services/IStorageService.cs`;
- `BootManager.Application/Storage/Services/StorageService.cs`;
- `BootManager.Application/Storage/QrFormat/LocationQrValue.cs`;
- `BootManager.Application/Storage/Results/QrResolutionResult.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/StorageLocationConfiguration.cs`;
- de nieuwe migratie `20260618192723_AddStorageLocationQrToken*` en de actuele model snapshot;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/LinkLocationQr.razor`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- de huidige storage-, route- en integratietests die al voor LOC-02 zijn toegevoegd of gewijzigd.

Lees geen brede source trees, TODO, legacy-analyse, handoff of ongerelateerde
featuredocumentatie.

## Defects To Fix

### Fix 1: Link flow must pass and revalidate the full QR value

#### Existing defect

De huidige linkflow navigeert met alleen een ruwe `token` queryparameter en de
koppelpagina valideert alleen `IsValidToken(token)`.

Daardoor kan de Owner-route direct worden geopend met willekeurige 32-char hex zonder
bewijs dat deze waarde ooit uit een geldige BootManager locatie-QR kwam. Dit schendt
de goedgekeurde richting dat de scanflow de volledige QR-value doorgeeft en dat de
koppelpagina die opnieuw parse/valideert.

#### Required behavior

- `Scan.razor` navigeert bij een onbekende geldige locatie-QR naar de koppelpagina
  met de volledige QR-value, URL-encoded.
- De koppelpagina accepteert een queryparameter die de volledige QR-value bevat.
- De koppelpagina parse/valideert die value opnieuw via centrale application-code.
- Alleen exact `bootmanager:location:<32-lowercase-hex-token>` wordt geaccepteerd.
- Een ontbrekende, verkeerd geprefixte, uppercase, te korte, te lange of anderszins
  ongeldige value toont een ongeldige-status en start geen linkflow.
- Gebruik geen tweede, afwijkende parsepad in Razor; gebruik de bestaande centrale
  formatter/parser.

### Fix 2: Existing QR token may never be replaced

#### Existing defect

De huidige linkservice kan een onbekende token koppelen aan een locatie die al een
`QrToken` heeft. `SetQrToken` overschrijft de bestaande waarde direct.

Dat schendt LOC-02: genereren is idempotent en bestaande tokens mogen in deze story
niet worden vervangen.

#### Required behavior

- Een locatie zonder token kan worden gekoppeld.
- Een locatie met een bestaande token weigert een nieuwe koppeling met een functioneel
  foutresultaat.
- Een bestaande token blijft onveranderd bij hernoemen of verplaatsen.
- De generate-flow blijft idempotent en retourneert bij een bestaande token exact
  dezelfde QR-value.
- Introduceer geen vervang-, revoke- of resetfunctionaliteit; dat blijft buiten scope.

### Fix 3: Translate token races into functional failures

#### Existing defect

De huidige service doet alleen een pre-check op bestaande koppeling. Wanneer een
concurrente insert of update dezelfde token nét eerder opslaat, kan de database-unique
constraint alsnog vuren en lekt een technische `DbUpdateException` door.

#### Required behavior

- Behoud de huidige pre-check vóór koppelen/aanmaken.
- Laat de database-unique constraint de laatste integriteitslaag blijven.
- Vertaal een duplicate-token race bij update of insert naar een functioneel
  foutresultaat in application-servicecode.
- Doe dit zonder repositoryrefactor, zonder nieuwe unit-of-worklaag en zonder brede
  infrastructuurwijziging.
- Maskeer geen andere databasefouten als token-race; detecteer alleen het relevante
  duplicate-token-scenario.

### Fix 4: Restore missing defect-sensitive evidence

#### Existing defect

De verplichte defectgevoelige bewijsvoering is onvolledig:

- de nieuwe SQLite-integratietests falen nu structureel;
- het migratie-upgradepad vanaf
  `20260618175732_AddStorageAreasAndLocations` is nog niet bewezen;
- de unieke nullable index is niet betrouwbaar bewezen;
- de vereiste bUnit-tests voor `Scan`, `LinkLocationQr` en de aangepaste
  `StorageLocationDetails` ontbreken;
- de eerder bestaande detailcomponenttest is verwijderd zonder gelijkwaardige vervanging.

#### Required behavior

- Herstel de integratietestopzet zodat tijdelijke SQLite-databases en migrations echt
  bruikbaar en reproduceerbaar zijn.
- Bewijs het upgradepad vanaf
  `20260618175732_AddStorageAreasAndLocations` met bestaand area/location-data dat na
  migratie behouden blijft.
- Bewijs dat meerdere `null`-tokens zijn toegestaan en dubbele niet-null tokens worden
  geweigerd.
- Voeg echte bUnit-tests toe voor `Scan`, `LinkLocationQr` en
  `StorageLocationDetails`.
- Herstel of vervang de verwijderde detailcomponentdekking zodanig dat back-navigatie
  plus de nieuwe Owner/Crew QR-weergave en generatieflow aantoonbaar zijn gedekt.

## Preserve These Behaviors

- Het tokenformaat en de parser/formatter blijven exact ongewijzigd in betekenis.
- Bekende locatie-QR's navigeren nog steeds direct naar
  `/storage/locations/{locationId}`.
- Willekeurige QR/barcodewaarden blijven generieke scanresultaten zonder linkactie.
- Crew ziet geen token op de detailpagina en krijgt geen koppelactie.
- De bestaande scan-racebescherming, camera/handmatige gelijkloop en diagnostics uit
  `PILOT-SCAN-01` blijven behouden.
- De Owner-only routeautorisatie voor de koppelpagina blijft intact.
- De LOC-01 storageregels voor naam, beschrijving en uniqueness blijven gelden.

## Allowed Write-Set

Wijzig uitsluitend:

- `BootManager.Core/Entities/StorageLocation.cs`;
- `BootManager.Application/Storage/DTOs/StorageLocationDetailDto.cs`;
- `BootManager.Application/Storage/QrFormat/`;
- `BootManager.Application/Storage/Results/QrResolutionResult.cs`;
- `BootManager.Application/Storage/Services/IStorageService.cs`;
- `BootManager.Application/Storage/Services/StorageService.cs`;
- `BootManager.Infrastructure/Persistence/Configurations/StorageLocationConfiguration.cs`;
- `BootManager.Infrastructure/Migrations/20260618192723_AddStorageLocationQrToken*`;
- `BootManager.Infrastructure/Migrations/BootManagerDbContextModelSnapshot.cs`;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/LinkLocationQr.razor`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- gerichte tests onder `BootManager.UnitTests/Storage/`;
- `BootManager.UnitTests/Web/RouteAuthorizationTests.cs` alleen als nodig;
- gerichte tests onder `BootManager.IntegrationTests/Storage/`.

Voeg geen package toe. Wijzig niets buiten deze write-set zonder vooraf concrete
compile-time noodzaak te melden.

## Explicitly Forbidden Changes

Wijzig niet:

- story-, release-, TODO-, legacy-, README- of handoff-documentatie;
- rollen, loginflow, middleware of niet-gerelateerde routes;
- de gekozen tokenprefix, tokenlengte of tokenencoding;
- andere storage- of scanfeatures buiten de genoemde defecten;
- andere tests om failures te verbergen;
- projectstructuur, DI-registraties, package-references of architectuurlagen.

Maak geen branch, commit, push, PR, merge, release of deployment.

## Test Evidence Requirements

Iedere nieuwe of gewijzigde test moet echte productcode of componenten uitvoeren en
concreet kunnen falen bij het bedoelde defect.

### Red-green proof expected

Voor iedere defectgroep hierboven lever je regressiebewijs:

- Noem per nieuwe/gewijzigde test welk defect of gedrag wordt uitgevoerd.
- Als je geen echte pre-fix rode run kunt vastleggen omdat de testfile nu nog niet
  compileert of de defectopzet eerst moet worden hersteld, meld dat expliciet vóór de
  fix met de concrete technische reden en lever een gelijkwaardig bewijs.
- Een groene suite zonder aantoonbare defectgevoeligheid telt niet.

### Required unit/service tests

Bewijs minimaal:

- full-QR queryflow: onbekende geldige QR levert op de scanpagina alleen voor Owner een
  linkactie die de volledige QR-value encodeert;
- invalid/non-BootManager values tonen geen linkactie;
- `LinkQrToExistingLocationAsync` weigert koppelen aan een locatie met bestaand token;
- `GenerateOrGetQrTokenAsync` blijft idempotent;
- duplicate-token race-afhandeling retourneert een functionele fout in plaats van een
  doorgelekte technische exception;
- create-with-token behoudt de bestaande naam-/beschrijvingsvalidaties.

### Required bUnit tests

Voeg echte componenttests toe die minimaal bewijzen:

1. `Scan` navigeert bij een bekende QR direct naar de locatie-detailroute.
2. `Scan` toont bij een onbekende geldige BootManager locatie-QR alleen voor Owner een
   koppelactie.
3. `Scan` toont voor Crew geen koppelactie voor dezelfde onbekende geldige QR.
4. `Scan` behandelt een niet-BootManager waarde als generiek scanresultaat zonder
   beheeractie.
5. De koppelactie gebruikt de volledige QR-value in de querystring en niet alleen de
   token.
6. `LinkLocationQr` weigert een ongeldige of verkeerd gevormde QR-value.
7. `LinkLocationQr` roept voor koppelen aan bestaande locatie de service aan met exact
   de geparste token en het gekozen locatie-id.
8. `LinkLocationQr` roept voor nieuwe locatie de service aan met exact de geparste
   token, gebied, naam en beschrijving en navigeert na succes naar het nieuwe
   locatie-id.
9. `StorageLocationDetails` toont voor Owner een generate-actie wanneer nog geen token
   bestaat.
10. `StorageLocationDetails` toont na genereren dezelfde QR-value zonder tweede
    generatiecall.
11. `StorageLocationDetails` toont voor Crew geen token en geen generate-actie.
12. `StorageLocationDetails` behoudt de back-button `history.back`-interactie.

Gebruik echte rendering en gebruikersinteractie met het bestaande componenttestframework.

### Required integration/migration tests

Gebruik alleen echte tijdelijke SQLite-databases.

Bewijs minimaal:

1. migratie naar
   `20260618175732_AddStorageAreasAndLocations`;
2. invoegen van bestaand `StorageArea` + `StorageLocation` zonder token vóór de
   nieuwe migratie;
3. dispose van de eerste context;
4. heropenen van exact dezelfde tijdelijke database;
5. migreren naar latest;
6. bestaand databehoud na migratie;
7. aanwezigheid en bruikbaarheid van nullable tokenopslag;
8. filtered/unique tokenconstraint: meerdere `null`, geen dubbele niet-null token;
9. functionele serviceworkflow op echte SQLite voor bekende, onbekende en gekoppelde
   tokens.

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

De volledige unitrun mag uitsluitend de bekende baselinefailure bevatten:

`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect`

Iedere andere failure betekent `niet gereed`.

Controleer daarna expliciet dat de correctieronde geen nieuwe wijziging buiten de
toegestane write-set heeft aangebracht.

## Definition of Technical Completion

Rapporteer alleen `gereed voor Codex-review` wanneer:

- de koppelroute de volledige QR-value doorgeeft en opnieuw valideert;
- tokenvervanging op bestaande locaties technisch onmogelijk is binnen LOC-02;
- duplicate-token races naar functionele foutresultaten worden vertaald;
- alle verplichte bUnit-, service-, route- en integratietests aanwezig zijn en slagen;
- het migratie-upgradepad vanaf
  `20260618175732_AddStorageAreasAndLocations` met databehoud is bewezen;
- build en `git diff --check` slagen;
- geen verboden of onverklaarde wijziging is gemaakt.

Rapporteer `niet gereed` wanneer een eis ontbreekt, een nieuwe test faalt, het
migratiebewijs onvolledig is, een regressietest defectongevoelig blijkt, de build of
diffcheck faalt, of de write-set onvoldoende blijkt. Verlaag geen test- of
acceptatie-eis en maskeer geen failure als waarschuwing.

## Completion Notes

Retourneer uitsluitend:

1. exacte gewijzigde bestanden;
2. hoe de full-QR linkflow nu werkt en opnieuw valideert;
3. hoe tokenvervanging en duplicate-token races nu functioneel worden afgehandeld;
4. exacte nieuwe/gewijzigde testnamen en welk productiegedrag of defect zij uitvoeren;
5. migratie-/constraintbewijs en SQLite-opzet;
6. alle test-, build- en diffcheckresultaten;
7. resterende risico's en exacte handmatige Owner/Crew-testvereisten;
8. eindstatus `gereed voor Codex-review` of `niet gereed`, met concrete reden.

Noem de story niet `Done`, geaccepteerd of productierijp.
