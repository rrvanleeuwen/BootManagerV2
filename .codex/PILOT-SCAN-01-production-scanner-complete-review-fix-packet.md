# Complete Review Fix Packet

## Task

Corrigeer in één ronde alle onderstaande reviewbevindingen in de geïntegreerde `/scan`-implementatie. Behoud de architectuur: één ZXing-camerastream, ZXing alleen voor QR en native `BarcodeDetector` alleen voor EAN-13.

Vraag niet om afzonderlijke goedkeuring per punt. Implementeer alle punten, voer alle checks uit en rapporteer ze samen.

## Bevindingen en vereiste fixes

### 1. Video-ready-wachtlus kan onbeperkt blijven lopen

De huidige `checkReady()` met `setTimeout`:

- controleert de sessie niet tijdens het pollen;
- heeft geen timeout;
- blijft eeuwig lopen als Stoppen/navigeren wordt gebruikt voordat video-afmetingen beschikbaar zijn;
- blijft ook hangen wanneer ZXing zeer snel een QR-resultaat vindt en de stream stopt voordat `videoWidth/videoHeight` positief worden.

Vervang dit door één bounded, sessieveilige helper.

Vereisten:

- controleer bij iedere poll:
  - `_sessionId === mySession`;
  - `_resultReceived === false`;
  - video-element bestaat;
- gebruik een korte totale timeout, bijvoorbeeld 5 seconden;
- ruim de timer altijd op;
- retourneer een expliciete uitkomst zoals `ready`, `cancelled` of `timeout`;
- bij `cancelled` of reeds ontvangen resultaat: normale stille cleanup/return;
- bij timeout terwijl sessie nog actief is:
  - QR-scanning mag blijven werken;
  - start geen native loop;
  - rapporteer EAN-13 als niet operationeel met handmatige fallback;
  - laat `startScan` afronden zodat Blazor niet in `RequestingPermission` blijft hangen.

### 2. Huidige-sessie-foutpad ruimt ZXing/stream niet volledig op

In de `catch` rond `decodeFromConstraints` wordt bij de actuele sessie `_reader` alleen op `null` gezet.

Vereisten:

- voer voor zowel stale als actuele sessie defensief uit:
  - `localReader.reset()`;
  - native loop stoppen;
  - videotracks stoppen;
- wis `_reader` alleen wanneer die nog naar `localReader` verwijst;
- rapporteer de camerafout alleen voor de actuele sessie;
- laat geen camerastream of ZXing-loop achter na permission-, camera- of decoderstartfouten.

### 3. Decoder-supportcontract is onvolledig

Het packet vereiste onderscheid tussen:

- native API ontbreekt;
- supportcheck mislukt;
- `ean_13` ontbreekt;
- `ean_13` wordt ondersteund en is operationeel.

Breid het JS/C# DTO-contract minimaal uit met:

- `qrAvailable`;
- `nativeSupportCheckSucceeded`;
- `nativeEan13Available`;
- `supportedFormats`;
- optioneel een compacte `nativeFailureReason` voor UI-onderscheid.

Gedrag:

- ZXing succesvol geladen: `qrAvailable = true`.
- `BarcodeDetector` ontbreekt: supportcheck is afgerond, EAN-13 niet beschikbaar.
- `getSupportedFormats()` ontbreekt, gooit, geen array retourneert of binnen een korte bounded timeout niet antwoordt: supportcheck mislukt; QR blijft werken.
- `ean_13` ontbreekt: supportcheck geslaagd, EAN-13 niet beschikbaar.
- constructor en detectieloop starten werkelijk: EAN-13 beschikbaar.

Gebruik capability-detectie, geen user-agentdetectie.

### 4. UI claimt EAN-13 beschikbaar vóór operationele detectorstart

De huidige UI kan `EAN-13: beschikbaar` tonen op basis van `getSupportedFormats()`, terwijl:

- `new BarcodeDetector(...)` kan gooien;
- video nooit ready wordt;
- de native loop niet start.

Vereisten:

- beschouw EAN-13 pas als operationeel beschikbaar nadat:
  - formaatondersteuning is vastgesteld;
  - detectorconstructie slaagt;
  - video ready is;
  - native loop is gestart.
- Laat `_startNativeDetectionLoop` een expliciete succesuitkomst retourneren.
- Rapporteer een gecorrigeerde supportstatus wanneer constructor/video-ready faalt.
- QR blijft in alle gevallen scannen.

### 5. Native detectiefouten mogen niet onbeperkt stil worden genegeerd

Een permanente fout in `detector.detect(videoEl)` leidt nu tot een eindeloze foutloop terwijl de UI EAN-13 beschikbaar blijft noemen.

Vereisten:

- houd een teller voor opeenvolgende native detectiefouten per sessie;
- reset de teller na een succesvolle `detect()`-aanroep;
- schakel de native loop na een kleine grens, bijvoorbeeld 3 opeenvolgende fouten, uit;
- rapporteer EAN-13 daarna als niet operationeel en toon handmatige fallback;
- laat ZXing QR ongewijzigd doorlopen;
- stale sessies mogen geen supportupdate sturen.

### 6. Supportstatus moet onafhankelijk van cameradiagnostics worden gerapporteerd

`OnDecoderSupport` wordt nu pas aan het einde van `_reportDiagnosticsAndCameras` aangeroepen en kan daardoor worden vertraagd door enumeratie/autofocus.

Vereisten:

- rapporteer de initiële native supportcheck direct na die check voor de actuele request;
- rapporteer later opnieuw zodra de native detector operationeel gestart is of operationeel faalt;
- houd camera-/autofocusdiagnostics hiervan gescheiden;
- callbacks blijven request-ID- en sessieveilig.

### 7. Blazor UI moet het DTO werkelijk volgen

De UI toont QR nu hardcoded als beschikbaar en maakt geen onderscheid tussen unsupported en supportcheck-fout.

Vereisten:

- toon QR-status op basis van `QrAvailable`;
- toon voor EAN-13 afzonderlijk:
  - beschikbaar;
  - niet ondersteund in deze browser, handmatige invoer;
  - ondersteuning kon niet worden vastgesteld of decoder kon niet starten, handmatige invoer;
- maak de melding compact en Nederlands;
- `SupportedFormats` hoeft niet breed zichtbaar te zijn op de productiepagina.

### 8. Verwijder stale decoderstatus bij een nieuwe start/camerawissel

Vereisten:

- zet `_decoderSupport = null` bij `StartScan`;
- zet `_decoderSupport = null` vóór de nieuwe start bij actieve camerawissel;
- voorkom dat oude operationele EAN-status zichtbaar blijft terwijl een nieuwe sessie nog wordt opgebouwd.

### 9. Normaliseer het geaccepteerde EAN-resultaat

De checksumvalidatie trimt de waarde, maar de callback stuurt nu `rawValue`.

Vereisten:

- laat native validatie bij succes ook de getrimde/genormaliseerde waarde teruggeven;
- stuur die genormaliseerde 13-cijferige waarde naar `OnScanResult`;
- format blijft exact `EAN_13`.

### 10. Eén resultaat en volledige cleanup blijven leidend

Controleer en behoud:

- QR en native EAN delen één resultaatguard;
- de eerste geldige detectie wint;
- QR-resultaat stopt native loop, ZXing en tracks;
- EAN-resultaat stopt native loop, ZXing en tracks;
- Stoppen, camerawissel, handmatige invoer en dispose stoppen beide loops;
- een pending native `detect()` van een oude sessie kan geen callback of cleanup op een nieuwere sessie uitvoeren.

## Expected Write-Set

Wijzig uitsluitend:

- `BootManager.Web/wwwroot/js/barcodeScanner.js`
- `BootManager.Web/Components/Pages/Scan.razor`
- `BootManager.Web/Components/Pages/Scan.razor.css` alleen als de bestaande compacte statussen dit nodig hebben

Wijzig geen geïsoleerde testpagina's en geen documentatie.

## Required Static Checks

```powershell
node --check BootManager.Web/wwwroot/js/barcodeScanner.js
dotnet build BootManager.sln --no-restore
git diff --check
```

## Required Lifecycle Verification

Controleer de implementatie expliciet tegen deze scenario's en vermeld per scenario het resultaat:

1. Stoppen terwijl ZXing/camera nog start.
2. Stoppen terwijl video-ready polling loopt.
3. QR wordt gevonden vóór native loop start.
4. QR en EAN worden vrijwel gelijktijdig gevonden.
5. Native `detect()` is pending tijdens Stoppen/herstart.
6. Camerawissel tijdens actief scannen.
7. Handmatige invoer tijdens actief scannen.
8. `BarcodeDetector` ontbreekt.
9. `getSupportedFormats()` faalt of hangt.
10. `ean_13` ontbreekt.
11. Constructor van `BarcodeDetector` faalt.
12. Native `detect()` faalt permanent.
13. Navigatie/dispose tijdens actief scannen.

Gebruik waar zinvol een kleine Node-test/harness met mocks voor timers, detector en callbacks. Voeg geen productietestframework toe alleen voor dit packet.

## Completion Notes

Retourneer uitsluitend:

1. gewijzigde bestanden en fixes;
2. static checks en resultaten;
3. lifecycle-scenario's 1-13 met resultaat;
4. resterende risico's en handmatige telefoonchecks.
