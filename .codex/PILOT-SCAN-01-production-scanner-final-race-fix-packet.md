# Final Race Fix Packet

## Task

Corrigeer uitsluitend de resterende releaseblokkers uit de volledige eindreview van `/scan`. Dit is één gebundelde laatste correctieronde.

## Required Fixes

### 1. Een stale sessie mag nooit de nieuwe stream stoppen

De huidige stale paden roepen `_stopVideoTracks()` aan op basis van het globale video-element:

- na `decodeFromConstraints` als `_sessionId !== mySession`;
- na `_waitVideoReady` als `_sessionId !== mySession`;
- in het stale `catch`-pad.

Omdat een nieuwere sessie hetzelfde video-element gebruikt, kan een oude async-terugkeer zo de nieuwe `srcObject` stoppen.

Vereisten:

- maak cleanup sessie-/resource-owned;
- leg na succesvolle camerastart de streamreferentie van deze sessie lokaal vast;
- voeg een helper toe die uitsluitend die lokale streamtracks stopt;
- wis `videoEl.srcObject` alleen wanneer het nog exact die lokale stream is;
- stale paden mogen nooit globale `_reader`, `_nativeDetector`, `_videoElementId` of de stream van een nieuwere sessie opruimen;
- `localReader.reset()` blijft toegestaan voor de eigen oude reader;
- actuele `stopScan()` en `dispose()` mogen wel globale actieve resources opruimen.

Controleer ook native cleanup: een stale pending `detect()` mag geen nieuwere detector of reader resetten.

### 2. Ruim de vorige actieve sessie direct op bij een nieuwe start

`startScan()` voert nu eerst async ZXing-load/supportcheck uit en ruimt eerdere resources pas later op.

Vereisten:

- na reserveren van de nieuwe sessie-id direct de vorige actieve reader/native loop/stream opruimen;
- doe dit vóór async supportcheck;
- voorkom meerdere seconden overlap wanneer `getSupportedFormats()` traag is;
- behoud de nieuwe sessie-id tijdens cleanup.

### 3. Meld EAN-13 pas beschikbaar wanneer operationeel

De initiële supportcallback zet `nativeEan13Available = true` zodra `getSupportedFormats()` `ean_13` meldt. Dat is nog geen operationele beschikbaarheid.

Vereisten:

- de initiële callback rapporteert capability-informatie, maar `nativeEan13Available` blijft `false`;
- gebruik `NativeFailureReason` of een extra expliciet veld om toestand `initializing` te representeren;
- zet `nativeEan13Available = true` pas nadat:
  - video ready is;
  - `new BarcodeDetector(...)` slaagt;
  - de native loop is ingepland;
- stuur dan een tweede supportcallback.
- Bij unsupported formaat blijft supportcheck geslaagd en beschikbaarheid false.

### 4. UI onderscheidt unsupported van operationele fout

De Blazor-UI kijkt nu alleen naar `NativeSupportCheckSucceeded`.
Daardoor worden `detector_init_failed`, `video_not_ready` en `detector_error` getoond als “niet ondersteund”.

Vereisten:

- gebruik `NativeFailureReason` in de UI;
- toon:
  - operationeel: `EAN-13: beschikbaar`;
  - initialiserend: een neutrale compacte melding;
  - API/format ontbreekt: `EAN-13: niet ondersteund in deze browser; gebruik handmatige invoer`;
  - supportcheck, video-ready, constructor of runtime detectie mislukt: `EAN-13: decoder niet beschikbaar; gebruik handmatige invoer`;
- QR-status blijft op `QrAvailable` gebaseerd.

### 5. Cancelled video-ready pad eindigt expliciet

Na `_waitVideoReady()`:

- bij `cancelled` door een reeds gevonden QR-resultaat: return zonder diagnostics of supportupdates;
- bij stale sessie: resource-owned cleanup en return;
- ga niet door naar `_reportDiagnosticsAndCameras` wanneer de sessie geannuleerd of het resultaat al ontvangen is.

### 6. Timeouttimer van supportcheck opruimen

De `Promise.race`-timeout blijft nu aflopen wanneer `getSupportedFormats()` snel voltooit.

Vereisten:

- implementeer een bounded helper die zijn timeout wist in `finally`;
- voorkom achterblijvende timers per start/herstart;
- gedrag bij timeout blijft: QR werkt, EAN handmatige fallback.

## Expected Write-Set

Wijzig uitsluitend:

- `BootManager.Web/wwwroot/js/barcodeScanner.js`
- `BootManager.Web/Components/Pages/Scan.razor`
- `BootManager.Web/Components/Pages/Scan.razor.css` alleen indien nodig voor neutrale initialisatiestatus

Wijzig geen documentatie of testpagina's.

## Required Checks

```powershell
node --check BootManager.Web/wwwroot/js/barcodeScanner.js
dotnet build BootManager.sln --no-restore
git diff --check
```

## Mandatory Mock Race Harness

Maak een tijdelijke of inline Node-harness, zonder productietestframework toe te voegen, en bewijs minimaal:

1. sessie A wacht op camera/video;
2. sessie B start en krijgt een nieuwe stream op hetzelfde video-element;
3. sessie A keert stale terug;
4. stream B heeft daarna geen gestopte tracks en blijft `srcObject`;
5. een stale native `detect()` van A reset geen reader/detector van B;
6. EAN-status wordt niet als beschikbaar gemeld vóór operationele loopstart;
7. `detector_init_failed` en `detector_error` leveren operationele-foutstatus, niet unsupported;
8. QR-resultaat tijdens video-ready-wacht beëindigt de startflow zonder latere diagnostics/supportcallback.

Verwijder een tijdelijk harnessbestand na uitvoering; commit het niet.

## Completion Notes

Retourneer uitsluitend:

1. gewijzigde bestanden en fixes;
2. checks en resultaten;
3. mock-races 1-8 en resultaten;
4. resterende handmatige telefoonchecks.
