# Implementation Packet

## Task

- Story ID: `PILOT-SCAN-01`
- Approved story: Camera-, QR- en barcode-proof-of-concept
- Story source: `.docs/releases/holiday-pilot-2026.md`, sectie `PILOT-SCAN-01`
- Goal: Integreer de bewezen scannerstrategie in de bestaande `/scan`-pagina: ZXing voor QR en native `BarcodeDetector` voor EAN-13 op ondersteunde browsers, via één gedeelde camerastream.

De story is al goedgekeurd. Vraag niet opnieuw om goedkeuring. Implementeer direct binnen dit packet.

## Bewezen uitgangssituatie

- Bestaande `/scan`:
  - ZXing leest QR betrouwbaar;
  - camera-, selectie-, autofocus-, lifecycle-, resultaat- en handmatige-invoerflow bestaan;
  - ZXing leest kleine echte EAN-13 op Android niet betrouwbaar.
- Quagga2:
  - lokaliseert de kleine EAN-13;
  - decodeert hem niet, ook niet bij analyse-resoluties 720×1280 en 900×1600.
- Native `BarcodeDetector` op de geteste Samsung/Chromium-browser:
  - ondersteunt `ean_13`;
  - leest `9789059965607` op werkelijk productformaat vanaf circa 15 cm;
  - cameraresolutie 1080×1920;
  - directe herkenning bij detectiepoging 1.
- iPhone/Safari mag niet als native `BarcodeDetector`-ondersteund worden aangenomen.

## Architectuurbesluit

- Gebruik één camerastream en één bestaand video-element.
- ZXing blijft eigenaar van camera/start/stop en decodeert uitsluitend `QR_CODE`.
- Wanneer runtime `BarcodeDetector` én formaat `ean_13` ondersteunt, draait daarnaast een native detectieloop op hetzelfde video-element.
- De eerste geldige detectie van een van beide decoders wint; daarna stoppen beide detectielussen en de camerastream exact eenmaal.
- Wanneer native EAN-13 niet wordt ondersteund:
  - QR-scannen blijft volledig werken;
  - toon duidelijk dat productbarcodes op dit apparaat/browser handmatig moeten worden ingevoerd;
  - start geen Quagga2-fallback.

## Scope

### JavaScript

- Breid de bestaande `BootManager.Web/wwwroot/js/barcodeScanner.js` uit; maak geen tweede camera-eigenaar.
- Beperk ZXing-hints tot `QR_CODE`.
- Controleer vóór of tijdens start runtime:
  - `window.BarcodeDetector`;
  - `BarcodeDetector.getSupportedFormats()`;
  - aanwezigheid van `ean_13`.
- Maak bij support één `BarcodeDetector({ formats: ["ean_13"] })`.
- Start de native detectieloop pas wanneer ZXing de camera heeft geopend en het video-element bruikbare `videoWidth/videoHeight` heeft.
- Detecteer op het bestaande video-element.
- Voorkom overlappende native `detect()`-calls.
- Valideer native resultaat op:
  - exact 13 cijfers;
  - geldige EAN-13-checksum.
- Gebruik één gedeelde sessie-/resultaatguard zodat QR en EAN-13 niet beide een callback kunnen opleveren.
- Stop bij resultaat, handmatig stoppen, camerawissel, herstart en dispose:
  - ZXing-reader;
  - native animation/timer-loop;
  - actieve tracks;
  - native detectorreferentie.
- Behoud bestaande request-ID- en stale-sessionbescherming.
- Rapporteer decoderondersteuning naar Blazor met een compact object:
  - QR via ZXing beschikbaar;
  - native EAN-13 beschikbaar;
  - eventueel ondersteunde native formaten;
  - supportcheck geslaagd/mislukt.

### Blazor

- Behoud de bestaande route `/scan`, camerakeuze, diagnostics, resultaatweergave en handmatige invoer.
- Toon compact de actieve scanmogelijkheden:
  - `QR-code: beschikbaar`;
  - `EAN-13: beschikbaar` wanneer native support bewezen is;
  - anders `EAN-13: niet beschikbaar in deze browser; gebruik handmatige invoer`.
- Een mislukte native supportcheck mag QR-scannen niet blokkeren.
- Resultaatformaten:
  - ZXing QR: `QR_CODE`;
  - native productbarcode: `EAN_13`.
- Behoud automatische stop na de eerste herkenning.

## Outside Scope

- Geen Quagga2-integratie in `/scan`.
- Verwijder de geïsoleerde Quagga- en native testpagina's nog niet.
- Geen EAN-8, UPC-A of Code 128 via native integratie in deze wijziging.
- Geen iPhone-specifieke decoder.
- Geen productdatabase, opslag of navigatie op basis van de code.
- Geen wijzigingen aan HTTPS-, Docker- of netwerkconfiguratie.
- Geen documentatie-, commit-, push- of PR-werk.

## Expected Write-Set

Wijzig uitsluitend:

- `BootManager.Web/wwwroot/js/barcodeScanner.js`
- `BootManager.Web/Components/Pages/Scan.razor`
- `BootManager.Web/Components/Pages/Scan.razor.css` alleen indien nodig voor de compacte beschikbaarheidsmelding

Wijzig geen geïsoleerde testpagina's.

## Minimal Context

Lees:

- `CLAUDE.md`
- dit implementation packet
- `.docs/releases/holiday-pilot-2026.md`, alleen sectie `PILOT-SCAN-01`
- `BootManager.Web/Components/Pages/Scan.razor`
- `BootManager.Web/Components/Pages/Scan.razor.css`
- `BootManager.Web/wwwroot/js/barcodeScanner.js`
- `BootManager.Web/Components/Pages/NativeBarcodeScanTest.razor`, alleen voor support-DTO/checksumpatroon
- `BootManager.Web/wwwroot/js/nativeBarcodeScannerTest.js`, alleen voor native detectie- en cleanuppatroon

Laad geen brede documentatie- of source trees.

## Existing Constraints

- Geen tweede `getUserMedia()`-aanroep voor native detectie.
- Geen videoframes via JS-interop of SignalR naar .NET.
- De bestaande handmatige invoer blijft altijd beschikbaar.
- Secure-contextmelding blijft bestaan.
- Camera wisselen moet beide decoderloops correct herstarten.
- Callback na een oude sessie wordt genegeerd.
- Native supportcheck is capability-based, niet user-agent-based.
- Gebruik Nederlandse XML-documentatie waar DTO's of JSInvokable-methoden worden toegevoegd.

## Acceptance Focus

- Android Chromium met native EAN-13:
  - pagina toont QR en EAN-13 beschikbaar;
  - QR wordt herkend via ZXing;
  - `9789059965607` wordt herkend als `EAN_13`;
  - eerste resultaat stopt camera en beide loops.
- Browser zonder native EAN-13, inclusief te verifiëren iPhone/Safari:
  - pagina toont QR beschikbaar;
  - pagina meldt EAN-13 handmatige fallback;
  - QR-scannen blijft werken;
  - handmatige invoer blijft werken.
- Stoppen, opnieuw scannen, camera wisselen en navigeren laten geen camerastream of detectieloop achter.

## Required Checks

```powershell
node --check BootManager.Web/wwwroot/js/barcodeScanner.js
dotnet build BootManager.sln --no-restore
git diff --check
```

## Completion Notes

Retourneer uitsluitend:

1. gewijzigde bestanden en geïmplementeerd gedrag;
2. checks en resultaten;
3. configuratie-impact;
4. resterende risico's en exacte handmatige testpunten.
