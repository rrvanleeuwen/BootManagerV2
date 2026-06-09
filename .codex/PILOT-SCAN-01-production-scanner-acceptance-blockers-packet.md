# PILOT-SCAN-01 Production Scanner Acceptance Blockers

## Task

Corrigeer de onderstaande drie resterende acceptatieblockers in een wijziging. Dit is een
gerichte correctie op de huidige implementatie; wijzig niets buiten de genoemde bestanden.

## Blocker 1: Blazor accepteert nog dubbele revisions

In `Scan.razor` staat:

```csharp
if (support.SupportRevision < _lastSupportRevision)
```

Dit wijkt af van het vorige packet. Een gelijk revisionnummer mag een al verwerkte status
niet opnieuw overschrijven.

Vereist:

```csharp
if (support.SupportRevision <= _lastSupportRevision)
```

Werk de direct bijbehorende comment/XML-documentatie bij van “oudere” naar
“oudere of gelijke” revisions.

## Blocker 2: `_nativeStreamGetter` is foutieve globale sessiestate

`_nativeStreamGetter` blijft na stop/herstart naar de getter van de vorige sessie wijzen.
Een QR-callback die in de nieuwe sessie vóór `localStream`-toekenning plaatsvindt, kiest
daardoor eerst de oude stream. Omdat die niet null is, wordt de actuele stream uit
`videoEl.srcObject` niet meer gekozen en kan de nieuwe camera actief blijven.

Vereist:

- verwijder de globale `_nativeStreamGetter` volledig;
- de QR-callback bepaalt zijn stream uitsluitend als volgt:
  1. gebruik het sessielokale `localStream` wanneer aanwezig;
  2. wanneer de sessie nog actueel is en `localStream` ontbreekt, gebruik het actuele
     `videoEl.srcObject` wanneer dat een `MediaStream` is;
- behoud de sessielokale `getSessionStream` closure voor de native detectorloop;
- laat geen globale getter of andere globale streamclosure achter;
- controleer dat `stopScan()`, `dispose()`, herstart en camerawissel geen verwijzing naar
  een oude streamclosure bewaren.

## Blocker 3: de huidige harness voert de productiemodule niet uit

`test-final-verification-harness.js` meldt:

```text
Warning: Module runtime issue (expected in test harness): Unexpected token '{'
```

en gaat daarna toch door. Alle negen scenario’s testen vervolgens handmatig gekopieerde
simulaties. Dit is geen geldig regressiebewijs.

Vervang de harness volledig. Vereisten:

- een parse-, transform-, import- of runtime-initialisatiefout beëindigt de harness direct
  met exitcode 1;
- laad de echte `BootManager.Web/wwwroot/js/barcodeScanner.js`;
- maak de echte exports `startScan`, `stopScan`, `dispose` en `checkSecureContext`
  aanroepbaar zonder productiecode te wijzigen;
- gebruik bij `vm` een syntactisch correcte transformatie, bijvoorbeeld:
  - verwijder alleen het woord `export` uit de vier exportdeclaraties;
  - voeg daarna expliciet een test-API toe zoals
    `globalThis.__scanner = { startScan, stopScan, dispose, checkSecureContext };`;
- assertions moeten de echte exportfuncties aanroepen met mocks en de daardoor ontstane
  callbacks/resource-effecten observeren;
- scenario’s mogen geen productiealgoritme, cleanupcode of supportrevisionlogica opnieuw
  implementeren in de harness;
- een scenario dat alleen lokale variabelen manipuleert zonder een echte productie-export
  aan te roepen is ongeldig.

De geldige harness test minimaal:

1. `startScan()` zonder `BarcodeDetector`: echte supportcallback meldt QR beschikbaar en
   EAN unsupported; `stopScan()` stopt reader en actuele stream;
2. `startScan()` met native support en een geldige EAN-detectie: echte loop levert precies
   één `EAN_13`-resultaat en stopt reader, animation frame en streamtracks;
3. echte QR-callback vóór afronding/toekenning van `localStream`: de actuele
   `videoEl.srcObject`-stream wordt gestopt;
4. start A, daarna start B, daarna een late callback/reject uit A: reader, stream,
   detector, supportstatus en resultaatcallbacks van B blijven intact;
5. supportcallbacks uit de echte code hebben binnen een sessie unieke oplopende revisions,
   inclusief `detector_error`;
6. `stopScan()` en `dispose()` ruimen via de echte exports alle actuele resources op.

Voor asynchrone paden:

- gebruik bestuurbare deferred promises voor `decodeFromConstraints()` en `detect()`;
- gebruik een bestuurbare requestAnimationFrame-queue;
- wacht assertions werkelijk af met async tests;
- faal bij unhandled rejections;
- faal als de module niet volledig is geëvalueerd;
- faal als geen echte exportfunctie is aangeroepen.

Laat `test-final-verification-harness.js` na een succesvolle run staan voor Codex-review.

## Expected Write-Set

Wijzig uitsluitend:

- `BootManager.Web/Components/Pages/Scan.razor`
- `BootManager.Web/wwwroot/js/barcodeScanner.js`
- `test-final-verification-harness.js`

Wijzig geen CSS, documentatie, commits of pushes.

## Required Checks

```powershell
node --check BootManager.Web/wwwroot/js/barcodeScanner.js
node --check test-final-verification-harness.js
node test-final-verification-harness.js
dotnet build BootManager.sln --no-restore
git diff --check
```

Gerichte controles:

```powershell
Select-String -Path BootManager.Web/wwwroot/js/barcodeScanner.js -Pattern "_nativeStreamGetter|supportRevision:\s*2|_nativeConsecutiveErrors"
Select-String -Path BootManager.Web/Components/Pages/Scan.razor -Pattern "SupportRevision <= _lastSupportRevision"
```

De eerste zoekopdracht moet geen treffers geven. De tweede moet exact de bedoelde guard
tonen.

## Completion Notes

Retourneer uitsluitend:

1. de drie gecorrigeerde blockers;
2. hoe de harness de echte exports laadt en aanroept;
3. resultaten van scenario’s 1-6;
4. resultaten van alle checks;
5. bevestiging dat CSS, documentatie, commits en pushes niet zijn gewijzigd.
