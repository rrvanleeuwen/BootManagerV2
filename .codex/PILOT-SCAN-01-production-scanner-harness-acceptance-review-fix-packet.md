# PILOT-SCAN-01 Production Scanner Harness Acceptance Review Fix

## Task

Vervang de inhoudelijk onvoldoende verificaties in
`test-final-verification-harness.js` door deterministische tests van de echte exports uit
`BootManager.Web/wwwroot/js/barcodeScanner.js`.

De huidige harness laadt de productiemodule wel, maar bewijst scenario's 1-6 niet. Maak
geen productieaanpassing tenzij een eerst correct gemaakte test rood wordt door een echte
productiefout.

## Review Findings

1. De globale `requestAnimationFrame`- en `setTimeout`-mocks gebruiken `setImmediate`,
   slikken callbackfouten met `catch { }` en zijn niet bestuurbaar.
2. Er is geen `unhandledRejection`-bewaking.
3. Scenario 1 maakt geen stream of reader-resource aan en controleert niet dat een track
   en reader werkelijk worden gestopt. Ook ontbreekt de assert dat EAN unsupported is.
4. Scenario 2 laat `detect()` altijd `[]` retourneren. Er wordt geen EAN-13-resultaat
   geleverd en geen resultaat-, reader-, animation-frame- of trackcleanup bewezen.
5. Scenario 3 zet `videoEl.srcObject` op `null`. De vereiste stream vóór
   `localStream`-toekenning bestaat dus niet en kan niet worden gestopt.
6. Scenario 4 gebruikt geen deferred start/reject/callback uit sessie A en eindigt met
   `assert(true)`. Dit bewijst geen sessie-isolatie.
7. Scenario 5 accepteert `revisions.length >= 1`; daarmee worden noch meerdere unieke
   oplopende revisions noch de verplichte `detector_error`-callback bewezen.
8. Scenario 6 controleert alleen `srcObject === null`; reader-reset, track-stop,
   animation-frame-cancel en detectorcleanup worden niet geobserveerd.
9. Meerdere `startScan()`-promises worden niet awaited. Vaste wachttijden van 50-300 ms
   maken de harness timingafhankelijk.

## Required Implementation

- Behoud het laden en transformeren van de echte productiemodule.
- Laat parse-, transform-, evaluatie- en initialisatiefouten direct eindigen met exitcode 1.
- Installeer vóór de tests een `process.on('unhandledRejection', ...)` handler die de run
  laat falen.
- Laat mock-callbacks geen fouten inslikken.
- Gebruik deferred promises voor `decodeFromConstraints()` en `detect()`.
- Gebruik een handmatig leeg te halen `requestAnimationFrame`-queue en registreer
  aangevraagde en geannuleerde frame-ID's.
- Maak per scenario expliciete mockstreams, tracks en readers met counters/logs.
- Await alle gestarte `startScan()`-promises of rond ze deterministisch af via de
  deferreds.
- Isoleer scenario's volledig; achtergebleven globale module-state uit een vorig scenario
  mag geen volgend scenario laten slagen.

Test via de echte exports minimaal:

1. Zonder `BarcodeDetector` meldt `startScan()` QR beschikbaar en EAN unsupported;
   `stopScan()` reset de actuele reader, stopt de actuele track en wist `srcObject`.
2. Met native support levert een gecontroleerde `detect()` precies één geldige
   `9789059965607` op. Assert exact één `OnScanResult` met formaat `EAN_13`, reader-reset,
   geannuleerde RAF en gestopte track.
3. Laat de echte ZXing-callback synchroon of gecontroleerd plaatsvinden nadat
   `videoEl.srcObject` een echte mock-`MediaStream` bevat maar voordat
   `decodeFromConstraints()` resolveert en `localStream` wordt toegekend. Assert dat die
   track wordt gestopt en `srcObject` wordt gewist.
4. Start A met deferred `decodeFromConstraints()`, start daarna B zonder A expliciet via
   `stopScan()` af te ronden. Trigger vervolgens minimaal één late callback of reject uit
   A. Assert dat B's reader, stream, detector/RAF, supportstatus en resultaatcallbacks
   intact blijven.
5. Forceer binnen één sessie initial support, operationele support en drie opeenvolgende
   detectiefouten. Assert minimaal drie unieke strikt oplopende revisions en dat de laatste
   callback `nativeFailureReason === 'detector_error'` bevat.
6. Activeer reader, stream en native RAF. Roep via afzonderlijke subtests `stopScan()` en
   `dispose()` aan. Assert voor beide alle relevante reset-, track-stop- en RAF-cancel-
   effecten en dat herhaald cleanup aanroepen veilig is.

Assertions die altijd waar zijn, alleen lokale variabelen manipuleren of geen effect van
een echte export observeren zijn niet toegestaan.

## Expected Write-Set

Wijzig standaard uitsluitend:

- `test-final-verification-harness.js`

Wijzig `BootManager.Web/wwwroot/js/barcodeScanner.js` alleen wanneer een hierboven
beschreven geldige test eerst faalt door een aantoonbare productiefout. Rapporteer dan
welke test rood was en waarom de productiecorrectie nodig was.

Wijzig geen Razor, CSS, documentatie, commits of pushes.

## Required Checks

```powershell
node --check BootManager.Web/wwwroot/js/barcodeScanner.js
node --check test-final-verification-harness.js
node test-final-verification-harness.js
dotnet build BootManager.sln --no-restore
git diff --check
```

## Completion Notes

Retourneer uitsluitend:

1. hoe parse/runtime/unhandled-rejection failures hard falen;
2. per scenario 1-6 welke echte export en welk resource-effect is geassert;
3. of productiecode nodig was en welke eerst rode test dat aantoonde;
4. resultaten van alle checks;
5. bevestiging dat Razor, CSS, documentatie, commits en pushes niet zijn gewijzigd.
