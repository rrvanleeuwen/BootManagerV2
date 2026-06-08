# Overdracht aan Codex: barcode- en QR-scanneronderzoek

## Context

Deze branch (`tmp-test-do-not-use-2`) bevat de experimenten van gisteren rond scannen met de telefooncamera in de webbrowser binnen BootManager.

De applicatie is een Blazor Server-applicatie. Camerabeelden en decodering moeten daarom volledig in de browser blijven. Alleen het uiteindelijke scanresultaat mag via JavaScript-interoperabiliteit naar Blazor/.NET gaan. Er mogen geen videoframes via SignalR worden verstuurd.

## Bevindingen ZXing

- QR-codes werden zonder problemen gelezen.
- Gewone 1D-barcodes werden niet betrouwbaar gelezen.
- Er is langdurig getest met verschillende camera's en instellingen.
- De losse testapp kreeg de geteste barcodes niet werkend met ZXing.
- Conclusie: behoud ZXing voorlopig alleen als bewezen QR-decoder, maar investeer nu niet verder in het tunen van ZXing voor 1D-barcodes.

## Bevindingen Quagga2

De officiële Quagga2-live-demo is getest op een telefoon met dezelfde soort fysieke barcodes die bij ZXing niet werkten.

Beide EAN-13-codes werden correct gelezen:

- `4007817310809`
- `3662168005289`

Werkende instellingen in de officiële demo:

- Barcode type: `EAN`
- Resolutiebreedte: `800px`
- Patch size: `large`
- Half sample: `false`
- Locate/barcode finder: `true`
- Camera: achtercamera (`facing back`)
- Torch: uit

Dit bewijst voor deze telefoon, browser en labels dat Quagga2 realtime EAN-13 uit de camerastream kan lokaliseren en decoderen.

## Belangrijke technische conclusie

Quagga2 ondersteunt geen QR-codes. Voor BootManager is daarom de voorlopige gratis combinatie:

- QR-code: ZXing
- 1D-productbarcode, te beginnen met EAN-13: Quagga2

De gebruiker hoeft niet vooraf te kiezen wat hij scant. Eén algemene scanner is mogelijk, mits de implementatie één camerastream gebruikt en de decoders gecontroleerd worden aangestuurd.

Voorkeursarchitectuur:

```text
één browser-camerastream
        ↓
scanner-adapter in JavaScript
        ├── ZXing: alleen QR
        └── Quagga2: alleen EAN, later eventueel UPC/Code 128
        ↓
uniform ScanResult naar Blazor
        ↓
Application-service bepaalt betekenis en vervolgactie
```

Open niet twee afzonderlijke camerastreams voor beide bibliotheken.

## Eerste volgende stap

Bouw nog geen volledige productfeature. Maak eerst een kleine, geïsoleerde Quagga2-proef binnen de bestaande applicatie.

Doel:

- bewijzen dat Quagga2 in onze eigen BootManager-omgeving dezelfde twee EAN-13-codes leest;
- exact beginnen met de bewezen instellingen van de officiële demo;
- nog geen productdatabase, externe product-API, routering of complete gebruikersinterface toevoegen.

Startconfiguratie Quagga2:

```javascript
{
  inputStream: {
    type: "LiveStream",
    constraints: {
      facingMode: "environment",
      width: { ideal: 800 }
    }
  },
  locator: {
    patchSize: "large",
    halfSample: false
  },
  decoder: {
    readers: ["ean_reader"]
  },
  locate: true
}
```

Controleer de precieze actuele Quagga2-API en pas de syntaxis alleen aan wanneer de gebruikte versie dat vereist. Behoud wel de functionele instellingen.

## Acceptatiecriteria voor de eerste Quagga2-slice

1. De achtercamera opent op dezelfde telefoon waarop de officiële demo is getest.
2. Barcode `4007817310809` wordt minimaal tien keer correct gelezen.
3. Barcode `3662168005289` wordt minimaal tien keer correct gelezen.
4. Er wordt geen onjuiste barcodewaarde als geldig resultaat doorgegeven.
5. Na een geldige scan stopt of pauzeert de scanner, zodat geen dubbele callbacks ontstaan.
6. De camerastream en Quagga2-resources worden vrijgegeven bij stoppen, navigeren en component-dispose.
7. Alleen het gevonden resultaat wordt via JS-interop naar .NET gestuurd.
8. Build en bestaande tests blijven slagen.
9. Nieuwe of aangepaste C#-code krijgt waar relevant Nederlandse XML-documentatie.

## Uniform resultaatmodel voor later

Nog niet volledig implementeren tenzij dit noodzakelijk is voor de proef, maar houd rekening met een toekomstig uniform resultaat, bijvoorbeeld:

```csharp
public sealed record ScanResult(
    string Value,
    string Format,
    string Engine);
```

Voorbeelden:

```text
Value: 4007817310809
Format: EAN_13
Engine: quagga2
```

```text
Value: BM1:COMPARTMENT:...
Format: QR_CODE
Engine: zxing
```

## Routering voor later

Na bewezen werking kan een Application-service bepalen wat de scan betekent:

- eigen BootManager-QR: open bijbehorend object;
- geldige EAN-13: zoek of voeg product toe;
- onbekende QR of barcode: toon gecontroleerde melding;
- open nooit automatisch een willekeurige externe URL uit een QR-code.

Valideer EAN-codes niet alleen op lengte en cijfers, maar ook op controlesom.

## Algemene scanner: nog niet in deze eerste stap

De uiteindelijke gebruiker moet één algemene knop kunnen krijgen, bijvoorbeeld `Scan QR-code of barcode`.

Mogelijke latere strategie:

- één gedeelde camerastream;
- ZXing alleen met QR-reader;
- Quagga2 alleen met EAN-reader;
- decoders afwisselend of met begrensde frequentie uitvoeren;
- na eerste betrouwbaar resultaat beide decoders stoppen;
- dubbele resultaten onderdrukken.

Implementeer dit pas nadat de geïsoleerde Quagga2-proef aantoonbaar werkt.

## Rol van Blazor WebAssembly

Een aparte Blazor WebAssembly-scanpagina is niet nodig om de herkenningskwaliteit te verbeteren. De officiële Quagga2-demo bewijst dat gewone browser-JavaScript voldoende is.

Een WebAssembly-scanroute kan later nuttig zijn voor:

- onafhankelijkheid van het Blazor Server-circuit;
- offline/PWA-gedrag;
- volledig client-side scannerstate.

Maar maak deze architectuurwijziging niet onderdeel van de eerste proef.

## Opdracht aan Codex voor de volgende sessie

1. Inspecteer eerst de bestaande code en wijzigingen op deze branch.
2. Beschrijf objectief wat de ZXing-testapp nu doet en welke delen bruikbaar blijven voor QR.
3. Stel één kleine implementatiestap voor om Quagga2 geïsoleerd toe te voegen.
4. Geef vóór het coderen aan welke bestanden worden aangepast en waarom.
5. Laat de implementatie zo klein mogelijk blijven.
6. Geef na implementatie concrete handmatige teststappen voor de twee genoemde EAN-13-codes.
7. Beoordeel daarna build, tests, browserconsole, cameracleanup en dubbele callbacks.
8. Na een geslaagde kleine wijziging is commit en push logisch.

## Niet doen

- Niet opnieuw langdurig ZXing tunen voor 1D zonder nieuw technisch bewijs.
- Niet direct een commerciële SDK invoeren.
- Niet direct de volledige algemene scanner bouwen.
- Niet twee libraries elk een eigen camerastream laten openen.
- Geen frames naar Blazor Server of .NET sturen.
- Geen productdatabase- of externe API-functionaliteit aan deze technische proef koppelen.
