# Implementation Packet

## Task

- Story ID: `PILOT-LOC-02`
- Approved story: QR-token genereren, koppelen en locatie openen
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-LOC-02`
- Goal: geef iedere opslaglocatie optioneel een stabiele BootManager QR-token, routeer
  bekende locatie-QR's naar de bestaande detailpagina en laat alleen Owner een
  onbekende BootManager-locatie-QR koppelen.
- Required branch: `feature/pilot-loc-02-location-qr`

De story is al goedgekeurd. Formuleer haar niet opnieuw en vraag geen nieuw akkoord.
Geef een kort uitvoeringsplan, implementeer direct, voer de checks uit en rapporteer
volgens `Completion Notes`.

## Scope

- Voeg aan `StorageLocation` een nullable, stabiele `QrToken` toe. Gebruik een opaque,
  cryptografisch willekeurige token van 16 bytes, gecodeerd als exact 32 lowercase
  hextekens. De token staat los van locatie-id, gebiedsnaam en locatienaam.
- Gebruik als volledig QR-value format exact
  `bootmanager:location:<32-lowercase-hex-token>`. Houd parsing en formatting centraal
  in application code; accepteer alleen dit exacte prefix en tokenformaat als een
  BootManager locatie-QR.
- Borg met een unieke gefilterde database-index dat een niet-null token aan maximaal
  één locatie is gekoppeld. Bestaande locaties blijven na migratie bestaan met
  `QrToken = null`.
- Breid de Storage application-service en DTO's/resultaten gericht uit voor:
  token/value opvragen; voor een bestaande locatie een token genereren; een QR-value
  herkennen en als invalid, onbekend of gekoppeld classificeren; een onbekende geldige
  token aan een bestaande locatie koppelen; en een nieuwe locatie met die token
  aanmaken.
- Token genereren is idempotent: wanneer een locatie al een token heeft, retourneert
  de service dezelfde token/value. Vervang nooit een bestaande token; vervangen hoort
  bij `PILOT-LOC-04`.
- Controleer vóór koppelen opnieuw dat de token nog onbekend is. Weiger koppelen als
  zij inmiddels aan enige locatie gekoppeld is. Laat de database-unique constraint de
  laatste integriteitslaag zijn en vertaal een praktische race naar een functioneel
  foutresultaat zonder repositoryrefactor.
- Maak de scanverwerking voor cameraresultaten en handmatige invoer gelijk: een bekende
  locatie-QR navigeert direct naar `/storage/locations/{locationId}`; een onbekende
  geldige BootManager locatie-QR toont een duidelijke onbekendstatus; een willekeurige
  of anders gevormde QR/barcode blijft een generiek scanresultaat en krijgt geen
  locatie-koppelactie.
- Toon bij een onbekende geldige BootManager locatie-QR alleen aan Owner een actie naar
  een aparte koppelpagina. Maak die pagina route-technisch Owner-only, zodat Crew de
  mutatieflow ook niet via een directe URL kan openen.
- Laat de Owner op de koppelpagina kiezen tussen koppelen aan een bestaande locatie en
  een nieuwe locatie aanmaken onder één bestaand gebied. Voor de nieuwe locatie gelden
  dezelfde naam-, beschrijving- en uniquenessregels als in `PILOT-LOC-01`. Gebieden
  aanmaken blijft in opslagbeheer.
- Toon op de bestaande locatie-detailpagina voor Owner de huidige QR-value of een knop
  om deze te genereren. Crew mag de bekende QR gebruiken om de pagina te openen, maar
  ziet geen genereer- of koppelactie. Voeg nog geen QR-afbeelding toe.
- Voeg één additieve EF Core-migratie toe en werk de model snapshot bij. Bestaande
  databases moeten vanaf `20260618175732_AddStorageAreasAndLocations` in-place kunnen
  migreren zonder reset of dataverlies.

## Outside Scope

- Geen QR-afbeelding, QR-library, printweergave, PNG/PDF/CSV-export of printerkoppeling;
  dat volgt in `PILOT-LOC-03`.
- Geen token vervangen, oude token ongeldig maken, tagstatus, tagoverzicht of auditlog;
  dat volgt in `PILOT-LOC-04`.
- Geen producten, productbarcodes, voorraad, hoeveelheden of voorraadmutaties.
- Geen gebied aanmaken vanuit de QR-koppelpagina en geen wijzigingen aan de bestaande
  Owner/Crew-rollen of loginflow.
- Geen externe QR-dienst, cloud-sync, NFC, nieuwe package dependency,
  architectuurrefactor, documentatie, commit, push, PR, release of deployment.

## Expected Write-Set

Wijzig alleen deze bestanden of modules, tenzij een noodzakelijke compile-time
dependency wordt ontdekt:

- `BootManager.Core/Entities/StorageLocation.cs`;
- `BootManager.Application/Storage/` voor QR-format/parser, DTO's/resultaten en
  servicecontract/-implementatie;
- `BootManager.Infrastructure/Persistence/Configurations/StorageLocationConfiguration.cs`;
- één nieuwe `BootManager.Infrastructure/Migrations/*AddStorageLocationQrToken*`
  migratie plus `BootManagerDbContextModelSnapshot.cs`;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `BootManager.Web/Components/Pages/StorageLocationDetails.razor`;
- één gerichte Owner-only koppelpagina onder `BootManager.Web/Components/Pages/`;
- gerichte tests onder `BootManager.UnitTests/Storage/` en
  `BootManager.IntegrationTests/Storage/`;
- `BootManager.UnitTests/Web/RouteAuthorizationTests.cs` alleen als de nieuwe
  Owner-only route daar wordt bewezen.

Voeg geen QR-package toe. Leg vóór wijziging buiten deze write-set uit waarom die nodig
is.

## Execution Boundaries

- Implementeer alleen applicatiecode, migratie en tests die dit packet vereist.
- Controleer vóór bewerken dat de actieve branch exact
  `feature/pilot-loc-02-location-qr` is en niet `master`. Rapporteer `niet gereed` als
  dat niet zo is.
- Wijzig geen story-, release-, TODO-, legacy-, README-, handoff- of andere
  projectdocumentatie.
- Maak geen commit, push, branch, PR, merge, release of deployment.
- Verander scope, acceptatiecriteria, tokenformaat of architectuurrichting niet. Stop
  en meld de kleinste ontbrekende beslissing als deze richting niet uitvoerbaar is.
- Voer geen database-reset uit en raak geen productie- of Raspberry Pi-database aan.
  Gebruik uitsluitend tijdelijke SQLite-databases voor migratie- en constrainttests.
- Noem de story nooit `Done`, geaccepteerd of productierijp. Meld alleen `gereed voor
  Codex-review` wanneer de technische completion definition volledig is gehaald.

## Minimal Context

Lees:

- `CLAUDE.md`;
- `.codex/PILOT-LOC-02-implementation-packet.md`;
- alleen de sectie `PILOT-LOC-02` in
  `.docs/releases/holiday-pilot-2026.md`;
- `BootManager.Core/Entities/StorageLocation.cs`;
- `BootManager.Application/Storage/`;
- `BootManager.Infrastructure/Persistence/Configurations/StorageLocationConfiguration.cs`,
  de actuele model snapshot en migratie `20260618175732_AddStorageAreasAndLocations`;
- `BootManager.Web/Components/Pages/Scan.razor` en
  `StorageLocationDetails.razor`;
- `BootManager.Web/Components/Settings/StorageManagement.razor` alleen voor bestaande
  locatie-aanmaakvelden en UI-patronen;
- bestaande Storage unit/component/integratietests en route-autorisatietests.

Gebruik gerichte zoekopdrachten en kleine bestandssecties. Lees niet standaard:

- de volledige `.docs/TODO.md` of andere releaseverhalen;
- `.docs/legacy-analysis/` of `.docs/legacy-input/`;
- `.codex/current-session-handoff.md` of `.codex/working-agreement.md`;
- ongerelateerde source trees.

## Existing Constraints

- Volg .NET 8 en de Clean Architecture-regels in `CLAUDE.md`.
- Gebruik de bestaande `IRepository<StorageLocation>`; introduceer geen nieuwe
  repositoryabstractie of unit-of-worklaag.
- Houd QR-format/parsing en tokenlogica uit Razor-componenten. De UI roept
  application-services aan en navigeert op hun getypeerde resultaat.
- `Scan.razor` is gedeeld door Owner en Crew en behoudt camerastart, camerawissel,
  request-id/racebescherming, foutafhandeling, diagnostics, decoderstatus en
  handmatige invoer uit `PILOT-SCAN-01`.
- Verwerk camera- en handmatige waarden via dezelfde async productcode. Negeer nog
  steeds resultaten van verouderde scanrequests vóór serviceaanroep of navigatie.
- De koppelpagina is expliciet `[Authorize(Roles = "Owner")]`; verberg de Owner-actie
  op de scan- en detailpagina daarnaast met role-aware rendering. Alleen UI-verbergen
  is niet voldoende autorisatiebewijs.
- URL-encode de QR-value bij navigatie naar de koppelpagina en parse/valideer haar daar
  opnieuw; vertrouw geen querystring als reeds gevalideerde token.
- Nieuwe-locatie-plus-token wordt als één nieuwe entityinsert opgeslagen. Een
  bestaande locatie krijgt haar token via één update. Vermijd een half aangemaakte
  locatie bij een mislukte koppeling.
- Geen tokenweergave voor Crew op de detailpagina: Crew hoeft alleen bekende QR's te
  kunnen scannen en de locatie te lezen.

## Acceptance Focus

- Exacte formatdetectie onderscheidt BootManager locatie-QR's van willekeurige QR's en
  productbarcodes.
- Iedere niet-null token is uniek; generatie is idempotent en hernoemen/verplaatsen
  laat token en volledige QR-value onveranderd.
- Bekende QR's navigeren voor Owner en Crew direct naar de juiste bestaande
  detailroute.
- Alleen Owner kan een onbekende token koppelen aan een bestaande of nieuwe locatie;
  Crew ziet geen beheeractie en kan de koppelroute niet openen.
- De bestaande scanner-, opslag-CRUD- en detailflow blijven werken.
- Migratie en databaseconstraint werken op echte tijdelijke SQLite en behouden
  bestaande locaties.

## Test Evidence Requirements

Voeg defectgevoelige tests toe die echte productcode/componenten uitvoeren en concreet
bewijzen:

- formatter/parser accepteert exact een geldig BootManager locatieformat en weigert
  verkeerde prefix, ontbrekende/te lange token en niet-lowercase/non-hex token;
- generatie geeft een token met het afgesproken formaat, is idempotent en maakt geen
  nieuwe token na hernoemen/verplaatsen;
- resolve onderscheidt invalid, onbekend en bekend en retourneert bij bekend de juiste
  locatie-id;
- dezelfde token kan niet aan twee locaties worden gekoppeld; koppelen aan bestaande
  en atomair aan nieuwe locatie werkt met de bestaande validatieregels;
- de unieke nullable SQLite-index staat meerdere nullen toe maar weigert dubbele
  niet-null tokens;
- migratie vanaf `20260618175732_AddStorageAreasAndLocations` behoudt vooraf ingevoegde
  area/location-data en voegt nullable tokenopslag plus index toe;
- echte bUnit-interactie op `Scan`: bekende camera- of handmatige waarde navigeert,
  onbekende geldige waarde toont alleen voor Owner een koppelactie, Crew ziet die niet,
  en een niet-BootManager waarde blijft generiek zonder beheeractie;
- echte bUnit-interactie op de koppelpagina roept voor beide keuzes de juiste
  serviceflow met exacte token, locatie/gebied, naam en beschrijving aan en navigeert
  na succes naar het gekoppelde locatie-id;
- echte bUnit-interactie op de detailpagina genereert voor Owner eenmaal en toont
  dezelfde value na herladen, terwijl Crew geen genereeractie of token ziet;
- route-autorisatie bewijst `Owner` voor de koppelpagina en `Owner,Crew` blijft gelden
  voor de locatie-detailpagina.

Inspecteer iedere nieuwe/gewijzigde test: geen `Assert.True(true)`, lege test,
bronvormtest als vervanging van gedrag of `async` test zonder relevante `await`.

## Required Checks

Voer eerst de gerichte Storage- en routechecks uit. Voer daarna uit:

```powershell
dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore
dotnet test BootManager.IntegrationTests/BootManager.IntegrationTests.csproj --no-restore
dotnet build BootManager.sln --no-restore
git diff --check
```

De bekende
`OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect`-failure mag
alleen als ongerelateerde baseline worden gemeld wanneer exact die ene bestaande
unit-testfailure overblijft. Alle nieuwe of gewijzigde tests moeten slagen.

## Definition of Technical Completion

Meld uitsluitend `gereed voor Codex-review` wanneer:

- ieder scopepunt en acceptatiecriterium technisch is geïmplementeerd;
- alle gerichte Storage-, scan-, component-, autorisatie- en migratietests slagen;
- iedere nieuwe/gewijzigde test echte productcode uitvoert en het geclaimde gedrag kan
  detecteren;
- volledige unit- en integratietestruns geen nieuwe failure bevatten;
- build en `git diff --check` slagen;
- migratie vanaf de genoemde vorige migratie, databehoud en SQLite-tokenconstraint op
  tijdelijke databases bewezen zijn;
- geen onverklaarde wijziging buiten de verwachte write-set staat;
- resterende handmatige Owner/Crew-acceptatiestappen expliciet zijn vermeld.

Meld `niet gereed` wanneer scope onvolledig is, migratie/compatibiliteit niet bewezen
is, een test documentair of defectongevoelig is, een nieuwe/gewijzigde test faalt,
build/diffcheck faalt, een vereiste beslissing ontbreekt of extra write-area niet kan
worden verantwoord. Verlaag geen test- of acceptatie-eis en maskeer geen failure als
waarschuwing.

## Completion Notes

Retourneer alleen:

1. gewijzigde bestanden en geïmplementeerd gedrag;
2. tests/checks en resultaten;
3. exacte nieuwe/gewijzigde testnamen en welk productiegedrag zij uitvoeren;
4. migratie- en configuratie-impact;
5. resterende risico's en exacte handmatige testvereisten;
6. eindstatus: `gereed voor Codex-review` of `niet gereed`, met concrete reden.
