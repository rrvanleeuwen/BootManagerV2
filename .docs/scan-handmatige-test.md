# PILOT-SCAN-01 — Handmatige acceptatietest

## Runtimevereisten

### Camera-API en beveiligde context

- Cameratoegang via `getUserMedia` vereist een **beveiligde browsercontext (HTTPS)**.
- De bestaande HTTP-route `http://bootmanager-pi:5000/` is **niet** geschikt voor cameragebruik.
- De scanpagina bepaalt bij het openen of de context beveiligd is en toont de HTTP-waarschuwing direct, zonder dat de gebruiker iets hoeft te doen.

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

## Checklist

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
