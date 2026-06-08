# Barcode- en QR-scanneronderzoek

## Bewezen resultaten

De bestaande ZXing-proef op deze branch leest QR-codes goed, maar de geteste 1D-productbarcodes niet betrouwbaar. Meerdere camera's en instellingen hebben dat niet opgelost.

De officiële Quagga2-live-demo is daarna op dezelfde telefoon getest. Beide EAN-13-codes werden correct gelezen:

- `4007817310809`
- `3662168005289`

Werkende Quagga2-instellingen:

- barcode type: EAN
- resolutie: 800 px breed
- patch size: large
- half sample: uit
- locator: aan
- achtercamera
- torch: uit

## Voorlopige technische keuze

Gebruik voorlopig twee gespecialiseerde decoders achter één scannerinterface:

- ZXing voor QR-codes
- Quagga2 voor EAN-13 en later eventueel UPC of Code 128

De gebruiker hoeft niet vooraf te kiezen wat hij scant. De uiteindelijke scanner kan één camerastream gebruiken en de gevonden code als uniform resultaat aan Blazor doorgeven. Open geen twee afzonderlijke camerastreams en stuur geen videoframes via SignalR naar .NET.

## Eerstvolgende kleine stap

Maak eerst een geïsoleerde Quagga2-proef binnen de bestaande scanpilot. Voeg nog geen productdatabase, externe product-API of volledige routering toe.

Gebruik als startconfiguratie:

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

Controleer de exacte syntaxis tegen de gebruikte Quagga2-versie, maar behoud deze functionele instellingen.

## Acceptatiecriteria

1. De achtercamera opent op dezelfde telefoon als bij de officiële demo.
2. Beide genoemde EAN-13-codes worden elk minimaal tien keer correct gelezen.
3. Er worden geen foutieve waarden geaccepteerd.
4. Na een geldige scan stopt of pauzeert de scanner om dubbele callbacks te voorkomen.
5. Camera en scannerresources worden vrijgegeven bij stoppen, navigeren en component-dispose.
6. Alleen het scanresultaat gaat via JS-interop naar .NET.
7. Build en bestaande tests blijven slagen.
8. Nieuwe of aangepaste C#-code krijgt waar relevant Nederlandse XML-documentatie.

## Aanwijzing voor Codex

Inspecteer eerst de bestaande code op `feature/pilot-scan-01` en beschrijf welke ZXing-delen bruikbaar blijven voor QR. Stel daarna één kleine implementatiestap voor om Quagga2 geïsoleerd toe te voegen, inclusief de te wijzigen bestanden en handmatige teststappen. Bouw nog niet direct de volledige algemene scanner. Na een geslaagde kleine wijziging is commit en push logisch.
