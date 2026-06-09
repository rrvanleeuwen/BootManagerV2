# PILOT-SCAN-01 — Handmatige acceptatietest

## Runtimevereisten

### Camera-API en beveiligde context

- Cameratoegang via `getUserMedia` vereist een **beveiligde browsercontext (HTTPS)**.
- De bestaande HTTP-route `http://bootmanager-pi:5000/` is **niet** geschikt voor cameragebruik.
- De scanpagina bepaalt bij het openen of de context beveiligd is en toont de HTTP-waarschuwing direct, zonder dat de gebruiker iets hoeft te doen.

### Productiedecoders

- `/scan` gebruikt één gedeelde camerastream.
- Lokale ZXing decodeert uitsluitend QR.
- Native `BarcodeDetector` decodeert uitsluitend EAN-13 en valideert het controlecijfer.
- Als de browser geen native EAN-13 ondersteunt, blijven QR en handmatige invoer beschikbaar.
- De pagina toont afzonderlijk of QR en EAN-13 beschikbaar zijn.
- Camera-enumeratie, 1920×1080 als ideale resolutie en continuous autofocus worden toegepast waar de browser dit ondersteunt.

### Ondersteunde browsers en toestellen

- Samsung Android-telefoon (Android 16 of nieuwer).
- Microsoft Edge voor Android.
- Google Chrome voor Android.
- Native EAN-13-ondersteuning moet per browser worden vastgesteld; QR blijft via lokale ZXing beschikbaar.

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
2. Controleer dat de decoderstatus **EAN-13: beschikbaar** toont.
3. Houd een echt product met EAN-13-barcode voor de camera.
4. Verwacht: scanner stopt automatisch na detectie.
5. Verwacht: 13-cijferige waarde en **Formaat** (EAN_13) worden zichtbaar getoond.

Als de decoderstatus meldt dat EAN-13 niet wordt ondersteund, noteer browser en toestel,
controleer dat QR blijft werken en voer de code handmatig in. De story slaagt pas wanneer
minimaal één echte EAN-13-productbarcode op beide telefoons in Edge en Chrome wordt
herkend.

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

## Quagga2 EAN-13 Scan Test (Historisch experiment)

### Doel

Dit gedeelte blijft alleen beschikbaar voor diagnose en vergelijking. Quagga2 is geen
onderdeel van de productie-acceptatie; `/scan` gebruikt native `BarcodeDetector` voor
EAN-13.

### Configuratie

- **Decoder**: Quagga2 1.12.1 (`@ericblade/quagga2`), lokaal gemirrord in `BootManager.Web/wwwroot/lib/quagga2/quagga.min.js`
- **Route**: `/scan-quagga-test` (geïsoleerd, geen menuvermelding)
- **Barcode-type**: EAN-13 alleen
- **Camera en verwerkingsinstellingen**:
  - `facingMode: "environment"` (achtercamera voorkeur)
  - cameraresolutie: `width: { ideal: 1920 }`, `height: { ideal: 1080 }` (voorkeur; browser mag fallback kiezen)
  - `inputStream.size`: testbaar met 800, 1280 (standaard) of 1600 px
  - `locator.patchSize: "large"`
  - `locator.halfSample: false`
  - `locate: true`
- **Validatie**: 13 decimale cijfers; controle van controleciffer via EAN-13 checksumalgoritme (alternerende vermenigvuldiging met 1 en 3 van de eerste 12 cijfers)

### Acceptatiecriteria

1. Pagina opent via HTTPS op beide telefoons in Edge en Chrome.
2. Camera start na toestemmingsverlening zonder fouten.
3. **Test-waarde 1**: `4007817310809` wordt 10 keer correct herkend en geaccepteerd.
4. **Test-waarde 2**: `3662168005289` wordt 10 keer correct herkend en geaccepteerd.
5. De ruwe detectie-log toont alle detecties (geldig en ongeldig).
6. Invalid detections (verkeerde lengte, ongeldig checksum) worden afgewezen en gelogd zonder doorscanning te stoppen.
7. Na elke geldige detectie stopt de scanner automatisch.
8. De acceptatie-tellers per waarde staan op exact 10 nadat de twintig scans voltooid zijn.
9. Camera en Quagga2 resources worden vrijgegeven bij Stoppen, Herstarten, navigatie en component-disposal.

### Handmatige testprocedure — twintig scans (10 per waarde)

**Voorbereiding:**

1. Verzamel de twee fysieke product-barcodes:
   - EAN-13: `4007817310809` (Haribo gummibeertjes, standaard Duitsland)
   - EAN-13: `3662168005289` (Mayonaise Amora, standaard Frankrijk)
2. Controleer dat beide barcodes onbeschadigd en leesbaar zijn.
3. Open de HTTPS-route `/scan-quagga-test` op de eerste telefoon in Edge.
4. Log in als nodig.
5. Controleer dat geen HTTP-waarschuwing zichtbaar is.
6. Controleer dat de twee test-waarden zichtbaar staan met tellers op 0.

**Scan Serie 1 — Waarde `4007817310809` (10 scans):**

1. Druk op **Starten**.
2. Verleen cameratoestemming.
3. Verwacht: statusregel toont "Camera actief"; videobeeld actief.
4. Houd barcode `4007817310809` op ~15 cm afstand voor de camera (goed belicht, recht).
5. Verwacht: scanner stopt automatisch; statusregel toont "EAN-13 barcode herkend"; waarde en formaat zichtbaar; teller "Geaccepteerd: 1×".
6. Controleer in de detectie-log dat de rauwe detectie aanwezig is en als "Geaccepteerd" gemarkeerd.
7. Druk op **Opnieuw scannen**.
8. Verwacht: scanner herstart; statusregel toont "Camera actief" opnieuw; resultaatblok verdwijnt.
9. Herhaal stap 4–8 nog **9 keer** (totaal 10 scans van `4007817310809`).
10. Controleer na 10 scans: teller voor `4007817310809` staat op **Geaccepteerd: 10×**.

**Scan Serie 2 — Waarde `3662168005289` (10 scans):**

1. Druk op **Herstarten** (of **Stoppen** en vervolgens **Starten**).
2. Verwacht: camera start opnieuw; statusregel "Camera actief".
3. Houd barcode `3662168005289` op ~15 cm afstand voor de camera.
4. Verwacht: scanner stopt; waarde `3662168005289` en formaat getoond; teller voor deze waarde wordt 1.
5. Druk op **Opnieuw scannen**.
6. Herhaal stap 3–5 nog **9 keer**.
7. Controleer na 10 scans: beide tellers staan op **Geaccepteerd: 10×**.

**Scannerdiagnostiek aflezen**:

Tijdens het scannen toont de pagina zes diagnostische velden:

- **Verwerkte frames**: aantal frames dat Quagga2 heeft verwerkt. Stijgt continu als scanner actief.
- **Gelokaliseerde boxen**: aantal potentiële barcode-kandidaten in het huidige frame. Groter dan nul wanneer Quagga2 streeppatronen ziet.
- **Camera resolutie**: werkelijke videobreedte × -hoogte van de actieve camera (bijvoorbeeld 1920×1440). Hangt af van telefoonmodel en browser.
- **Camera maximum**: maximale cameraresolutie die de browser voor de actieve track meldt, of `onbekend` als de browser dit niet ondersteunt.
- **Verwerkingsgrootte**: gekozen Quagga2 `inputStream.size`; standaard 1280 px.
- **Analyse resolutie**: berekende effectieve frame-afmetingen die Quagga2 analyseert na behoud van de cameraverhouding.

**Na twintig scans:**

1. Controleer beide tellers: `4007817310809` = 10, `3662168005289` = 10.
2. Controleer de detectie-log (begrensd tot de meest recente 50 invoeren):
   - Bevat alleen deze twee waarden geaccepteerd (groen/✓).
   - De twee tellers bewijzen dat exact 10 van elke waarde geaccepteerd zijn; de log is het bewijs van detecties inclusief afgewezen kandidaten.
3. Noteer de diagnostische velden:
   - Verwerkte frames eindstand
   - Aantal gelokaliseerde boxen in de laatste frame
   - Camera resolutie (werkelijk)
   - Camera maximum (indien beschikbaar)
   - Gekozen verwerkingsgrootte
   - Berekende analyse resolutie
4. Druk op **Stoppen**.
5. Verwacht: camera-indicatielampje op telefoon gaat uit.
6. Controleer dat de pagina nog voluit bruikbaar is (tellers, log intact).

**Aanvullende validatie:**

- Herhaal dezelfde twintig scans op de tweede telefoon, in dezelfde browser (Edge of Chrome).
- Test daarna in de andere browser op dezelfde telefoon (Chrome of Edge).
- Test ongeldig gedecodeerde waarden: houd een beschadigde of onleesbare barcode voor de camera zodat Quagga2 deze als kandidaat decodeert maar de validatie afwijst. Verwacht: detectie in de log met status "Afgewezen" en reden (bijv. "Niet 13 cijfers" of "Ongeldig EAN-13 checksum").

### Native BarcodeDetector EAN-13-proef

Gebruik route `/scan-native-barcode-test` als afzonderlijke vergelijking wanneer Quagga2 een kleine EAN-13 wel lokaliseert maar niet decodeert.

1. Open de route via HTTPS in Chrome en Edge op Android.
2. Controleer dat de pagina meldt of `BarcodeDetector` beschikbaar is en welke formaten de browser ondersteunt.
3. Als `ean_13` ontbreekt, noteer de zichtbare ondersteunde-formatenlijst en stop de test voor die browser.
4. Druk op **Starten** wanneer `ean_13` wordt ondersteund.
5. Houd de kleine EAN-13 `9789059965607` op normaal productformaat voor de camera.
6. Noteer:
   - cameraresolutie;
   - detectiepogingen na ongeveer tien seconden;
   - laatste aantal detecties per poging;
   - herkend ja/nee.
7. Bij herkenning verwacht: waarde `9789059965607`, formaat `EAN_13`, automatische stop en een geaccepteerde logregel.
8. Test **Stoppen**, **Herstarten** en navigeren; het camera-indicatielampje moet uitgaan.

**Bevindingen vastleggen:**

| Telefoon | Browser | 10× `4007817310809` | 10× `3662168005289` | Log volledig | Geen fouten |
|----------|---------|---------------------|---------------------|-------------|------------|
|          | Edge    | ☐                   | ☐                   | ☐          | ☐          |
|          | Chrome  | ☐                   | ☐                   | ☐          | ☐          |
|          | Edge    | ☐                   | ☐                   | ☐          | ☐          |
|          | Chrome  | ☐                   | ☐                   | ☐          | ☐          |
