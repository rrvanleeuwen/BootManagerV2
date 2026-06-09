# Module-Level Final Fix Packet

## Task

Corrigeer alle resterende bevindingen hieronder in één wijziging. De bestaande `test-race-harness.js` is niet geldig als regressiebewijs omdat die vereenvoudigde logica kopieert in plaats van de werkelijke `barcodeScanner.js` uit te voeren.

## Required Fixes

### 1. Sessie-owned streamcleanup stopt altijd de eigen tracks

De huidige `_stopSessionOwnedVideoTracks(localStream)` stopt tracks alleen wanneer `video.srcObject === localStream`.
Daardoor blijft een oude, losgeraakte stream actief wanneer een nieuwere stream al op hetzelfde video-element staat.

Vereist gedrag:

- stop altijd alle tracks van `localStream`;
- wis `video.srcObject` uitsluitend wanneer `video.srcObject === localStream`;
- raak een nieuwere `srcObject` nooit aan.

### 2. Stale catch mag geen globale native resources opruimen

De `catch` van `startScan()` roept vóór de stale-check `_stopNativeDetector()` aan.
Een fout uit sessie A kan daardoor detector/animation-frame van sessie B stoppen.

Vereist gedrag:

- voer vóór de stale-check uitsluitend lokale cleanup uit:
  - `localReader.reset()`;
  - eigen `localStream` stoppen;
- voer globale `_stopNativeDetector()` alleen uit wanneer `_sessionId === mySession`;
- wis `_reader` alleen als `_reader === localReader`;
- stale foutpad retourneert daarna zonder globale state te wijzigen.

### 3. Native detectorloop krijgt eigen sessiereferenties

De native loop gebruikt globale `_nativeDetector`, `_reader` en `_nativeAnimationFrameId`.
Maak ownership expliciet:

- leg de voor deze sessie gemaakte detector vast als `localDetector`;
- elke loopcontrole vereist:
  - `_sessionId === mySession`;
  - `_nativeDetector === localDetector`;
- na een pending `detect()` opnieuw dezelfde controles;
- cleanup vanuit een geldige detectie of permanente detectorfout mag globale native state alleen aanpassen als die nog eigendom is van `localDetector`;
- reset de ZXing-reader alleen wanneer `_reader` nog de bijbehorende `localReader` is; geef `localReader` daarom expliciet aan de native loop door;
- een stale loop uit A mag reader, detector, animation-frame of stream van B niet wijzigen.

### 4. QR-resultaat stopt expliciet de actuele lokale stream

Een QR-callback kan plaatsvinden voordat `localStream` na `decodeFromConstraints()` is vastgelegd.

Vereist gedrag:

- in de geldige actuele QR-callback:
  - bepaal de sessiestream defensief uit `localStream` of, uitsluitend omdat de sessie nog actueel is, uit `videoEl.srcObject`;
  - reset `localReader`;
  - stop die sessiestream;
  - stop alleen de native detector die bij dezelfde sessie hoort;
- voorkom afhankelijkheid van alleen `reader.reset()` voor trackcleanup.

### 5. Supportcallbacks mogen niet terug in de tijd

Initiële `initializing`-status en latere operationele/foutstatus worden fire-and-forget verzonden.

Vereisten:

- voeg een per-sessie oplopende supportrevision toe aan JS en C# DTO, of serializeer/await supportcallbacks;
- garandeer dat een late `initializing`-callback nooit een latere `available` of foutstatus overschrijft;
- Blazor negeert een oudere revision binnen dezelfde request;
- request-ID-bescherming blijft bestaan.

### 6. Verwijder ongebruikte of misleidende state

- Gebruik `_activeSessionStream` aantoonbaar voor actuele globale cleanup, of verwijder hem.
- Corrigeer `huidge` naar `huidige`.

## Expected Write-Set

Wijzig uitsluitend:

- `BootManager.Web/wwwroot/js/barcodeScanner.js`
- `BootManager.Web/Components/Pages/Scan.razor`

CSS alleen als strikt nodig.

## Required Checks

```powershell
node --check BootManager.Web/wwwroot/js/barcodeScanner.js
dotnet build BootManager.sln --no-restore
git diff --check
```

## Mandatory Real-Module Harness

Vervang de bestaande harness door een tijdelijke harness die de werkelijke functies/state uit `BootManager.Web/wwwroot/js/barcodeScanner.js` uitvoert. Toegestaan:

- importeer een tijdelijke geïnstrumenteerde kopie;
- of laad de module via Node `vm` en expose uitsluitend testhooks;
- wijzig productie-exports niet alleen voor tests.

De harness moet met mocks aantonen:

1. detached stream A: tracks A worden gestopt, `video.srcObject` B blijft staan en tracks B blijven actief;
2. stale catch A: detector, animation-frame en reader B blijven intact;
3. pending native `detect()` A na start B: geen cleanup of callback uit A;
4. QR A vóór `localStream`-toekenning: actuele stream A wordt expliciet gestopt;
5. late supportrevision `initializing` wordt na `available` genegeerd;
6. Stoppen/dispose ruimt de actuele reader, detector, animation-frame en stream op.

Voer de harness uit en verwijder daarna `test-race-harness.js` en eventuele tijdelijke/instrumentatiebestanden. Laat geen harnessbestand in de worktree achter.

## Completion Notes

Retourneer uitsluitend:

1. gewijzigde bestanden en fixes;
2. checks en resultaten;
3. echte-module-harness scenario's 1-6 en resultaten;
4. bevestiging dat alle tijdelijke harnessbestanden verwijderd zijn.
