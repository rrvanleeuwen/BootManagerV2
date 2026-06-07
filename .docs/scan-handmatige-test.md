# PILOT-SCAN-01 — Handmatige acceptatietest

## Runtimevereisten

### Camera-API en beveiligde context

- Cameratoegang via `getUserMedia` vereist een **beveiligde browsercontext (HTTPS)**.
- De bestaande HTTP-route `http://bootmanager-pi:5000/` is **niet** geschikt voor cameragebruik.
- De scanpagina bepaalt bij het openen of de context beveiligd is en toont de HTTP-waarschuwing direct, zonder dat de gebruiker iets hoeft te doen.

### Bekende testbevinding — EAN-13 op mobiel

Pi-test (Samsung Android, Chrome en Edge): QR-codes werden herkend; meerdere echte EAN-13-productbarcodes werden **niet** herkend, terwijl dezelfde barcodes op de laptop wel werkten.

Werkhypothese: de standaard videoconstraints leveren op deze telefoons onvoldoende bruikbare details of scherpte voor smalle lineaire barcodes.

Toegepaste correctie in `barcodeScanner.js`:
- `DecodeHintType.TRY_HARDER = true` toegevoegd: ZXing doet uitgebreidere beeldanalyse per frame.
- Camera-constraints uitgebreid met `width: { ideal: 1920 }` en `height: { ideal: 1080 }`: hogere resolutie vergroot de pixeldichtheid op de barcode en verbetert de decoderingsbetrouwbaarheid.
- Camera-enumeratie na toestemmingsverlening: de gebruiker kan expliciet een camera kiezen.
- Continuous autofocus (verplichte constraint `{ exact: 'continuous' }`): automatisch toegepast als ondersteund, met verificatie na toepassing.

Beide resolutiewaarden zijn `ideal`, geen `exact`: de browser kiest de best beschikbare resolutie zonder een fout te geven als het toestel de voorkeur niet exact kan leveren.

### Ondersteunde browsers en toestellen

- Samsung Android-telefoon (Android 16 of nieuwer).
- Microsoft Edge voor Android.
- Google Chrome voor Android.
- De native `BarcodeDetector`-API wordt **niet** als enige decoder gebruikt; de lokale ZXing-decoder werkt in beide browsers.

### Lokale barcodedecoder

De decoder (`@zxing/library` v0.20.0, Apache License 2.0) is lokaal opgeslagen in de repository en heeft geen internetverbinding nodig.

Bestandslocatie: `BootManager.Web/wwwroot/lib/zxing/zxing.min.js`
Licentietekst: `BootManager.Web/wwwroot/lib/zxing/LICENSE.txt` (Apache 2.0)
Pakketgegevens: `BootManager.Web/wwwroot/lib/zxing/NOTICE.txt`

Het bestand is al aanwezig in de repository. Als het ontbreekt, kan het eenmalig worden gedownload met:

```powershell
$dir = "BootManager.Web\wwwroot\lib\zxing"
New-Item -ItemType Directory -Force $dir | Out-Null
Invoke-WebRequest "https://cdn.jsdelivr.net/npm/@zxing/library@0.20.0/umd/index.min.js" `
    -OutFile "$dir\zxing.min.js" -UseBasicParsing
```

### HTTPS-route instellen

De Raspberry Pi heeft een HTTPS-eindpunt nodig voor cameratoegang op Android.
De specifieke poort en configuratie worden als afzonderlijke operationele stap vastgesteld door Codex en Roelof.
Mogelijke opties:

- Kestrel HTTPS met een zelfondertekend certificaat dat eenmalig handmatig op de telefoons wordt vertrouwd.
- Een lokale reverse proxy (bijv. Caddy of nginx) die TLS termineert op een geconfigureerde HTTPS-poort.
- mDNS-hostnaam (bijv. `bootmanager.local`) zodat Chrome/Edge het certificaat als lokaal herkent.

## Acceptatieprocedure op Android Edge en Chrome

Voer de volgende stappen uit op beide Samsung-telefoons, in zowel Edge als Chrome.

### 1. HTTP-route — secure-contextmelding bij pagina-opening

1. Open `http://bootmanager-pi:5000/scan`.
2. Log in indien gevraagd.
3. Verwacht: pagina toont **direct** een **rode alert** met melding dat cameratoegang HTTPS vereist (zonder dat de gebruiker op Starten hoeft te drukken).
4. Verwacht: de knop Starten is **niet** zichtbaar; in een onveilige context worden geen scancontrols aangeboden.
5. Verwacht: handmatige invoer onderaan de pagina is beschikbaar en werkt.

### 2. HTTPS-route — cameratoestemming verlenen

1. Open de operationeel geconfigureerde HTTPS-route, bijv. `https://bootmanager-pi/scan`.
2. Log in indien gevraagd.
3. Verwacht: geen HTTP-waarschuwing; statusregel toont "Druk op Starten om de camera te activeren."
4. Druk op **Starten**.
5. Verwacht: browser vraagt om cameratoestemming — verleen die.
6. Verwacht: achtercamera wordt actief (statusregel toont "Camera actief").
7. Verwacht: camera-beeld zichtbaar in de videopreviewer.

### 3. QR-code scannen

1. Houd een BootManager-test-QR of een willekeurige QR-code voor de camera.
2. Verwacht: scanner stopt automatisch na detectie.
3. Verwacht: **Waarde** en **Formaat** (QR_CODE) worden zichtbaar getoond.

### 4. EAN-13-barcode scannen

1. Druk op **Opnieuw scannen**.
2. Houd een echt product met EAN-13-barcode voor de camera.
3. Verwacht: scanner stopt automatisch na detectie.
4. Verwacht: 13-cijferige waarde en **Formaat** (EAN_13) worden zichtbaar getoond.

### 5. Stoppen en opnieuw starten

1. Druk op **Starten**; laat de camera kort actief.
2. Druk op **Stoppen**.
3. Verwacht: camera stopt, statusregel toont "Scanner gestopt".
4. Druk opnieuw op **Starten** en verifieer dat de camera opnieuw start.

### 6. Toestemming weigeren

1. Open een verse browsertab en ga naar de HTTPS-scanpagina.
2. Druk op **Starten**.
3. Weiger de cameratoestemming in de browserpop-up.
4. Verwacht: **oranje waarschuwing** met melding "Cameratoestemming geweigerd."
5. Verwacht: handmatige invoer blijft beschikbaar.

### 7. Handmatige invoer

**7a. Handmatige invoer terwijl scanner actief is:**

1. Druk op **Starten**; laat de camera actief.
2. Typ een willekeurige code in het invoerveld onderaan.
3. Druk op **Toepassen** of druk Enter.
4. Verwacht: camera stopt automatisch.
5. Verwacht: ingevoerde waarde en "Handmatig ingevoerd" worden getoond.

**7b. Handmatige invoer zonder actieve scanner:**

1. Zorg dat de scanner gestopt is.
2. Typ een willekeurige code in het invoerveld.
3. Druk op **Toepassen** of druk Enter.
4. Verwacht: waarde en "Handmatig ingevoerd" worden getoond.

### 8. Camera vrijgeven bij weggaan

1. Start de scanner (camera actief).
2. Navigeer via de navbar naar een andere pagina.
3. Verwacht: camera-indicatielampje op de telefoon gaat uit (camera vrijgegeven).
4. Keer terug naar de scanpagina en verifieer dat de pagina opnieuw correct opent.

### 9. Diagnosticsblok en focusmodi

#### 9a. Diagnosticsblok controleren na starten

1. Open de HTTPS-route en druk op **Starten**.
2. Verleen cameratoestemming.
3. Verwacht: kort na het activeren van de camera verschijnt een rij kleine labels onder de knoppen.
4. Noteer per telefoon en browser welke labels worden getoond, voor zover zichtbaar:
   - cameranaam (bijv. "Back Camera" of een Android-label);
   - resolutie (bijv. "1920×1080" of een lagere waarde als het toestel dit niet ondersteunt);
   - richting (bijv. "environment");
   - **actieve focusmodus** (bijv. "AF: auto", "AF: continuous" of leeg als niet ondersteund);
   - **ondersteunde focusmodi** (bijv. "Modes: continuous,fixed" of leeg als niet beschikbaar);
   - **continuous autofocus resultaat**: één van de volgende labels:
     - groen "↻ continu": continuous focus succesvol toegepast en geverifieerd;
     - oranje "✗ mislukt": continuous focus geprobeerd maar niet geverifieerd na toepassing;
     - grijs "— n.v.t.": continuous focus niet ondersteund door deze camera.
5. Leg de waarden vast in de tabel onderaan dit document.

#### 9b. Focusmodusgegevens interpreteren

De volgende waarden zijn relevant voor EAN-13-scherpte:

- **Actieve focusmodus** ("AF: auto", "AF: continuous", "AF: manual", etc.): geeft de huidige stand van de track na alle constraint-toepassingen.
- **Ondersteunde focusmodi** ("Modes: continuous,fixed", "Modes: auto,continuous,fixed,macro", etc.): welke modi stelt `getCapabilities()` ter beschikking.
- **Continuous autofocus resultaat**:
  - **groen "↻ continu"**: `applyConstraints({ focusMode: { exact: 'continuous' } })` is geslaagd; `getSettings()` na de toepassing bevestigt `focusMode === 'continuous'`.
  - **oranje "✗ mislukt"**: `continuous` staat in de ondersteunde modi, de constraint-aanroep is uitgevoerd, maar `getSettings()` toont daarna niet `'continuous'` (bijv. "auto", "fixed", "macro", of onbekend).
  - **grijs "— n.v.t."**: de browser ondersteunt geen `continuous` focusmodus (niet in SupportedFocusModes) of `getCapabilities()` was niet beschikbaar.

#### 9c. Camera-selector — meerdere camera's

1. Als het diagnosticsblok zichtbaar is en de telefoon meer dan één camera heeft, verwacht: direct boven het diagnosticsblok verschijnt een compacte keuzelijst met het label **Camera**.
2. De keuzelijst bevat een optie "Automatisch (achtercamera voorkeur)" en de beschikbare camera's met hun browsernaam.
3. Als de telefoon slechts één camera heeft, verwacht: **geen keuzelijst** zichtbaar.

#### 9d. Camerawissel terwijl scanner actief is

1. Zorg dat de scanner actief is (camera-beeld zichtbaar).
2. Kies een andere camera in de keuzelijst.
3. Verwacht: de camera wisselt automatisch (videostream stopt en start opnieuw zonder extra klik op Starten).
4. Verwacht: de status blijft "Camera actief" (geen tussenstap via Gestopt).
5. Verwacht: het diagnosticsblok wordt bijgewerkt met de gegevens van de nieuwe camera (label, resolutie, focusmodusgegevens).
6. Verwacht: er verschijnt geen foutmelding; de scanner blijft scannen.

#### 9e. Camerawissel terwijl scanner gestopt is

1. Zorg dat de scanner gestopt is.
2. Kies een andere camera in de keuzelijst (als zichtbaar).
3. Druk op **Starten**.
4. Verwacht: de geselecteerde camera wordt gestart (niet de automatische keuze).
5. Verwacht: het diagnosticsblok toont de gegevens van de geselecteerde camera.

#### 9f. Cameravergelijking voor EAN-13 scherpte en focusmodus

1. Zorg dat de scanner gestopt is.
2. Stel de camera-selector in op de eerste camera of op "Automatisch".
3. Druk op **Starten**.
4. Observeer het diagnosticsblok:
   - Noteer cameranaam, resolutie, actieve focusmodus en het continuous autofocus-resultaat (groen, oranje of afwezig).
5. Houd een EAN-13-barcode op ongeveer 15 cm afstand voor de camera.
6. Beoordeel visueel of het beeld scherp is.
7. Probeer de barcode te scannen; noteer of deze wordt herkend.
8. Als er meerdere camera's beschikbaar zijn: zet de keuzelijst op de volgende camera, laat de scanner automatisch doorgaan (9d) of stop en start opnieuw (9e), en herhaal stap 4–7.
9. Vul per camera de bevindingstabel onderaan in: cameranaam, resolutie, focusmodusgegevens en of EAN-13 scherp en herkend is.

## Bevindingstabel (in te vullen tijdens pilot)

| Telefoon | Browser | Camera (label) | Resolutie | Actieve AF | Ondersteunde AF | Continuous AF | Beeld scherp | EAN-13 herkend |
|----------|---------|----------------|-----------|------------|-----------------|----------------|--------------|----------------|
|          | Edge    |                |           |            |                 |                | ☐            | ☐              |
|          | Edge    |                |           |            |                 |                | ☐            | ☐              |
|          | Chrome  |                |           |            |                 |                | ☐            | ☐              |
|          | Chrome  |                |           |            |                 |                | ☐            | ☐              |

**Kolummen toelichting:**
- **Actieve AF**: de waarde uit het diagnosticsblok (bijv. "auto", "continuous", "fixed", leeg als niet beschikbaar).
- **Ondersteunde AF**: de modi uit het diagnosticsblok (bijv. "continuous,fixed", "auto,continuous,fixed", leeg als niet beschikbaar).
- **Continuous AF**: "groen ↻" (succesvol), "oranje ✗" (mislukt) of "grijs —" (niet ondersteund).

## Acceptatiechecklist

| # | Stap | Edge | Chrome |
|---|------|------|--------|
| 1 | HTTP-route toont direct de secure-contextmelding bij openen | ☐ | ☐ |
| 2 | HTTPS-route: cameratoestemming verlenen start camera | ☐ | ☐ |
| 3 | QR-code herkend (waarde + formaat) | ☐ | ☐ |
| 4 | EAN-13-barcode herkend (waarde + formaat) | ☐ | ☐ |
| 5 | Stoppen en opnieuw starten werkt | ☐ | ☐ |
| 6 | Geweigerde toestemming geeft begrijpelijke fout | ☐ | ☐ |
| 7a | Handmatige invoer stopt actieve scanner en toont resultaat | ☐ | ☐ |
| 7b | Handmatige invoer zonder actieve scanner toont resultaat | ☐ | ☐ |
| 8 | Camera stopt bij verlaten van de pagina | ☐ | ☐ |
| 9a | Diagnosticsblok verschijnt na starten (label, resolutie, AF-modi, AF-resultaat) | ☐ | ☐ |
| 9b | Focusmoduswaarden correct geïnterpreteerd (applied/failed/unsupported) | ☐ | ☐ |
| 9c | Camera-selector zichtbaar bij meerdere camera's; verborgen bij één | ☐ | ☐ |
| 9d | Camerawissel terwijl actief herstart automatisch, status blijft "Camera actief" | ☐ | ☐ |
| 9e | Camerawissel terwijl gestopt gebruikt gekozen camera bij Starten | ☐ | ☐ |
| 9f | Bevindingstabel ingevuld: scherpste camera, AF-modi en AF-resultaat vastgesteld | ☐ | ☐ |
