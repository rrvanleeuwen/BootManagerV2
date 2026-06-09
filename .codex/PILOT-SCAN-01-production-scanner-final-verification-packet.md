# PILOT-SCAN-01 Production Scanner Final Verification Packet

## Task

- Story ID: `PILOT-SCAN-01`
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-SCAN-01`
- Goal: corrigeer alle hieronder beschreven resterende productieproblemen in een wijziging en lever betrouwbaar regressiebewijs op dat de echte JavaScriptmodule uitvoert.

De story en gekozen decoderstrategie zijn al goedgekeurd. Vraag niet opnieuw om goedkeuring.

## Scope

### 1. Maak supportstatussen strikt monotoon binnen iedere scansessie

De huidige implementatie geeft `initializing`, `available` en foutstatussen dezelfde
`supportRevision`. Omdat de interop-calls fire-and-forget zijn, kan een laat afgeleverde
`initializing`-status een nieuwere operationele status alsnog overschrijven. De
`detector_error`-status gebruikt bovendien de hardcoded revisie `2`, die na herstarts
niet correct is.

Vereist gedrag:

- houd een sessielokale, monotoon oplopende supportrevision bij;
- ieder afzonderlijk supportbericht krijgt een unieke, hogere revision;
- gebruik een sessielokale helper voor alle supportcallbacks, inclusief:
  - initiële supportstatus;
  - `video_not_ready`;
  - operationeel beschikbaar;
  - `detector_init_failed`;
  - `detector_error`;
- verwijder de globale `_supportRevision` wanneer die daarna geen geldige functie meer heeft;
- gebruik nergens een hardcoded revision;
- een supportcallback wordt alleen verzonden als de bijbehorende sessie nog actueel is;
- laat Blazor updates met een revision kleiner dan of gelijk aan de laatst verwerkte
  revision negeren;
- request-ID-bescherming blijft daarnaast bestaan.

### 2. Maak native detectorfouten volledig sessielokaal

De huidige `_nativeConsecutiveErrors` is globale state. Een late afwijzing van
`detect()` uit sessie A kan daardoor de foutteller van sessie B wijzigen.

Vereist gedrag:

- houd de opeenvolgende native detectiefouten lokaal bij de detectorloop van de sessie;
- controleer na iedere `await detect()` en ook in het foutpad opnieuw:
  - `_sessionId === mySession`;
  - `_nativeDetector === localDetector`;
  - er is nog geen resultaat;
- een stale succes- of foutafhandeling uit sessie A wijzigt geen teller, detector,
  animation frame, reader, stream of callbackstatus van sessie B;
- een succesvolle actuele `detect()` zet alleen de lokale teller terug;
- na de foutdrempel stopt alleen de actuele lokale detector; QR-scannen blijft actief.

### 3. Stop de sessiestream expliciet bij een native EAN-13-resultaat

Het huidige EAN-resultaat reset de ZXing-reader, maar stopt de mediatracks niet expliciet.
Dat voldoet niet aan de lifecycle-eis en maakt cameravrijgave afhankelijk van intern
ZXing-gedrag.

Vereist gedrag:

- geef de native detectorloop toegang tot de stream die aantoonbaar bij dezelfde sessie hoort,
  bijvoorbeeld via een sessielokale getter;
- bij een geldig actueel EAN-13-resultaat:
  - zet de gedeelde resultaatguard;
  - stop uitsluitend de detector van dezelfde sessie;
  - reset uitsluitend de reader van dezelfde sessie;
  - stop expliciet alle tracks van de sessiestream;
  - wis `video.srcObject` alleen als die nog exact die sessiestream bevat;
  - stuur daarna precies een genormaliseerd `EAN_13`-resultaat naar Blazor;
- raak nooit een stream of reader van een nieuwere sessie aan.

### 4. Behoud en verifieer de bestaande QR- en algemene lifecyclegaranties

Controleer tijdens de wijziging ook expliciet dat:

- een actuele QR-hit vóór toekenning van `localStream` de stream defensief uit het
  actuele `videoEl.srcObject` haalt en expliciet stopt;
- een detached stream A altijd zijn eigen tracks stopt, maar `video.srcObject` B intact laat;
- stale start/catch/detect-callbacks geen globale resources van een nieuwere sessie opruimen;
- `stopScan()` en `dispose()` de actuele reader, detector, animation frame en stream opruimen;
- snelle start, stop, herstart en camerawissel geen dubbele resultaatcallback veroorzaken;
- ZXing uitsluitend `QR_CODE` decodeert;
- native `BarcodeDetector` uitsluitend `ean_13` decodeert;
- unsupported browsers QR plus handmatige invoer behouden.

## Expected Write-Set

Wijzig uitsluitend:

- `BootManager.Web/wwwroot/js/barcodeScanner.js`
- `BootManager.Web/Components/Pages/Scan.razor`

Wijzig CSS niet tenzij een aantoonbare compile- of functionele noodzaak ontstaat.
Wijzig geen documentatie, gitstatus, commits of pushes.

## Minimal Context

Lees:

- `CLAUDE.md`;
- dit packet;
- `.docs/releases/holiday-pilot-2026.md`, uitsluitend regels/sectie voor `PILOT-SCAN-01`;
- `BootManager.Web/wwwroot/js/barcodeScanner.js`;
- `BootManager.Web/Components/Pages/Scan.razor`;
- `test-race-harness.js`;
- `test-real-module-harness.js`.

Lees geen brede source tree of overige documentatiesets.

## Mandatory Real-Module Harness

De huidige `test-real-module-harness.js` is ongeldig regressiebewijs: ondanks zijn naam
kopieert hij vereenvoudigde logica en importeert of executeert hij
`BootManager.Web/wwwroot/js/barcodeScanner.js` niet.

Maak een tijdelijke Node-harness die aantoonbaar de echte productiebron laadt en de echte
`startScan`, `stopScan` en `dispose` uitvoert met browser-, ZXing-, stream-, detector-,
animation-frame- en .NET-interopmocks. Toegestaan:

- dynamische import van de productiemodule nadat globals zijn gemockt;
- of Node `vm` met minimale testinstrumentatie van de geladen bron;
- testinstrumentatie mag uitsluitend in de tijdelijke harness plaatsvinden;
- voeg geen testexports of test-only gedrag toe aan de productiemodule.

De harness moet asynchroon en deterministisch minimaal aantonen:

1. supportcallbacks die fysiek in omgekeerde volgorde afhandelen behouden in Blazor de
   nieuwste status; ieder bericht heeft een unieke oplopende revision;
2. `detector_error` gebruikt de volgende sessielokale revision en geen hardcoded waarde;
3. een geldige native EAN-13-hit stopt reader, detector, animation frame en alle tracks
   van de eigen stream, maakt `srcObject` leeg indien nog eigendom, en levert precies een
   `EAN_13`-callback;
4. een late `detect()`-reject uit sessie A na start van sessie B wijzigt geen foutteller
   of resources/status van B;
5. detached stream A wordt gestopt terwijl `video.srcObject` en tracks van B intact blijven;
6. een QR-callback vóór `localStream`-toekenning stopt de actuele stream expliciet en
   levert precies een QR-resultaat;
7. snelle start-stop-herstart en een pending oude callback laten alleen de nieuwste sessie
   actief;
8. `stopScan()` en `dispose()` ruimen de actuele reader, detector, animation frame en
   stream op;
9. een browser zonder `BarcodeDetector` houdt QR actief en rapporteert EAN-13 als
   unsupported zonder camerafout.

Een harness die helperlogica of pseudocode kopieert zonder de productiemodule te laden
geldt als mislukt, ook wanneer alle assertions groen zijn.

Laat de geldige tijdelijke harness na uitvoering in de worktree staan, zodat Codex hem
kan inspecteren en opnieuw uitvoeren. Codex verwijdert daarna:

- `test-race-harness.js`;
- `test-real-module-harness.js`;
- de nieuwe tijdelijke harness.

## Required Checks

Voer uit:

```powershell
node --check BootManager.Web/wwwroot/js/barcodeScanner.js
node <naam-van-de-echte-module-harness>
dotnet build BootManager.sln --no-restore
git diff --check
```

Controleer daarnaast met een gerichte zoekopdracht dat:

- geen `supportRevision: 2` resteert;
- geen globale native foutteller resteert;
- de tijdelijke harness de productiebron daadwerkelijk importeert of via `vm` uitvoert.

## Completion Notes

Retourneer uitsluitend:

1. gewijzigde productiebestanden en de concrete fixes;
2. naam van de tijdelijke echte-module-harness en hoe die de productiebron laadt;
3. scenario's 1-9 met resultaat;
4. overige checks en resultaten;
5. bevestiging dat documentatie, commits en pushes niet zijn uitgevoerd.
