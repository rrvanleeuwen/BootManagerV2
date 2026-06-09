# PILOT-SCAN-01 Production Scanner Harness Final Evidence Fix

## Task

Corrigeer uitsluitend de resterende bewijsproblemen in
`test-final-verification-harness.js`. De harness is syntactisch groen, maar voldoet nog
niet aan het vorige packet.

## Required Fixes

### 1. Gebruik echte deferred promises en await alle starts

`Deferred`, `decodeFromConstraintsDeferreds` en `detectDeferreds` zijn gedeclareerd maar
worden niet gebruikt. Alle variabelen `p`, `pA` en `pB` blijven unawaited.

Vereist:

- gebruik bestuurbare deferred promises voor de relevante
  `decodeFromConstraints()`- en `detect()`-paden;
- resolve/reject ze expliciet vanuit het scenario;
- await iedere aangemaakte `startScan()`-promise nadat het scenario die deterministisch
  heeft afgerond of geannuleerd;
- verwijder ongebruikte deferred-infrastructuur;
- gebruik `setImmediate` niet als vervanging voor het afronden van een async contract.

### 2. Scenario 4 moet sessie B werkelijk intact bewijzen

De huidige test controleert alleen callbacks. Hij bewijst niet dat B's resources intact
blijven.

Vereist:

- start A met een deferred `decodeFromConstraints()`;
- start B zonder `stopScan()` voor A;
- zorg dat B een eigen reader, stream en actieve native detector/RAF heeft;
- trigger daarna een late callback of reject uit A;
- assert dat B's reader niet opnieuw is gereset, B's track niet is gestopt, B's RAF niet
  is geannuleerd en B geen support- of resultaatcallback van A ontvangt;
- ruim B daarna via de echte export op en await beide startpromises.

Gebruik element-ID's als sleutel in de documentmock; tel geen toevallige
`getElementById()`-aanroepen.

### 3. Scenario 5 moet exact het vereiste revisionbewijs leveren

Vereist:

- forceer initial support, operationele support en daarna drie opeenvolgende
  detectiefouten;
- assert minimaal drie unieke revisions;
- assert dat iedere revision exact groter is dan de vorige;
- assert dat de laatste supportcallback `nativeFailureReason === 'detector_error'` bevat;
- await `startScan()` na cleanup.

### 4. Scenario 6 moet native cleanup en idempotentie testen

De huidige subtests zetten `BarcodeDetector = null` en kunnen dus geen
detector-/RAF-cleanup bewijzen.

Vereist voor zowel `stopScan()` als `dispose()`:

- activeer een native detectorloop met een pending bestuurbare `detect()`-promise;
- observeer reader-reset, track-stop en annulering van het actuele RAF-ID;
- roep dezelfde cleanup-export nogmaals aan en assert dat dit niet gooit en geen nieuwe
  actieve resources achterlaat;
- resolve/reject eventuele pending detectpromises gecontroleerd;
- await de bijbehorende `startScan()`-promise.

## Expected Write-Set

Wijzig uitsluitend:

- `test-final-verification-harness.js`

Wijzig geen productiecode, Razor, CSS, documentatie, commits of pushes.

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

1. waar deferred promises worden bestuurd en alle startpromises worden awaited;
2. welke B-resources scenario 4 intact bewijst;
3. de revisionreeks en laatste failure reason uit scenario 5;
4. de native cleanup- en idempotentieassertions uit scenario 6;
5. resultaten van alle checks en bevestiging van de write-set.
