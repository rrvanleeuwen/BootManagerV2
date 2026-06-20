# PILOT-INV-03

Bron: `.docs/releases/holiday-pilot-2026.md`

### PILOT-INV-03 — Scan-gestuurde inruimflow met locatievoorstel

**Storyzin**  
Als Owner of Crew wil ik vanuit het bestaande menu `Scannen` een productcode kunnen
scannen en daarna snel de juiste locatie en hoeveelheid kunnen bevestigen, zodat ik
meerdere producten achter elkaar praktisch kan inruimen zonder steeds opnieuw door
beheerflows te lopen.

**Waarom deze slice nu**  
`PILOT-INV-02` levert de handmatige voorraadbasis per locatie. Deze story bouwt daarop
voort door de voorkeursroute voor echt gebruik aan boord scan-gestuurd te maken. De
focus ligt op snel product inruimen met locatievoorstel, alternatieve locaties en een
doorlopende scansessie. Verbruik, correcties en historie blijven later.

**Scope**

- De primaire start voor deze story is het bestaande menu `Scannen`.
- De scanner herkent productcodes en locatie-QR's en kiest op basis daarvan de juiste
  vervolgstap.
- Als een locatie-QR wordt gescand, opent BootManager direct de bestaande locatiepagina.
- Als een productcode wordt gescand, start BootManager de inruimflow voor dat product.
- Voor een bekend product stelt BootManager de laatst gebruikte locatie voor; dit is de
  echte locatieverwijzing naar de locatie waar voor dat product de meest recente
  voorraadtoevoeging of aanvulling is opgeslagen.
- De UI toont die voorgestelde of verwachte locatie altijd als leesbare gebied- en
  locatienaam, niet als interne code of identifier.
- Als het product ook op andere locaties bekend is, toont BootManager daarnaast een
  kleine lijst met alternatieve locaties.
- De gebruiker kan de voorgestelde locatie alleen bevestigen of een andere locatie
  kiezen of scannen.
- Als een product nog geen eerdere locatie heeft, vraagt BootManager direct om een
  locatie te kiezen of te scannen.
- Handmatige fallback blijft beschikbaar: de gebruiker kan naast locatie scannen ook
  handmatig een andere locatie kiezen.
- Na locatiekeuze vult de gebruiker alleen een hoeveelheid in; de standaard eenheid van
  het product is wel zichtbaar maar niet wijzigbaar in deze flow.
- Na opslaan wordt de voorraad op die locatie toegevoegd of aangevuld volgens de regels
  van `PILOT-INV-02`.
- Na succesvol opslaan vraagt BootManager direct of de gebruiker nog een product wil
  scannen.
- Bij bevestiging van die vraag keert de flow terug naar de scanner binnen dezelfde
  scansessie.
- Bij stoppen van die vraag eindigt de flow op de locatiepagina waar het product is
  weggelegd.
- Als een gescande productcode onbekend is, kan de gebruiker in deze flow direct:
  - een nieuw product aanmaken;
  - de gescande code koppelen aan een bestaand product;
  - annuleren.
- Nieuw product aanmaken gebeurt in een modaal venster binnen de scanflow; de gescande
  code is vooraf ingevuld maar bewerkbaar.
- Na nieuw product aanmaken of code koppelen aan bestaand product gaat de inruimflow
  direct verder met locatie en hoeveelheid.

**Buiten scope**

- Een aparte dashboardstart buiten het bestaande menu `Scannen`.
- Detailnavigatie naar product- of locatieoverzichten midden in de primaire
  inruimstappen.
- Verbruik, correcties, overschrijven van hoeveelheden en mutatiehistorie.
- Automatische productherkenning via externe EAN-database, fotoherkenning of AI.
- Volledige productbeheerflow buiten de minimale modal die nodig is voor onbekende
  codes in deze scansessie.
- Batchverplaatsingen tussen locaties of andere samengestelde voorraadacties.

**Acceptatiecriteria**

1. De gebruiker kan vanuit het bestaande menu `Scannen` een code scannen en BootManager
   bepaalt op basis van het type code welke flow moet starten.
2. Een gescande locatie-QR opent direct de bestaande locatiepagina.
3. Een gescande bekende productcode start direct de inruimflow voor dat product.
4. Voor een bekend product met eerdere voorraadlocaties stelt BootManager de laatst
   gebruikte locatie voor en toont het daarnaast eventuele alternatieve locaties in een
   kleine lijst.
5. De gebruiker kan de voorgestelde locatie bevestigen of een andere locatie kiezen of
   scannen.
6. Als het product nog geen eerdere locatie heeft, vraagt de flow direct om een locatie
   te kiezen of te scannen.
7. De gebruiker vult daarna alleen een hoeveelheid in; de eenheid van het product is
   zichtbaar maar niet wijzigbaar.
8. Na opslaan wordt de voorraad op de gekozen locatie volgens `PILOT-INV-02` toegevoegd
   of aangevuld.
9. Na succesvol opslaan vraagt BootManager direct of nog een product gescand moet
   worden; bij `ja` keert de flow terug naar de scanner binnen dezelfde sessie en bij
   `nee` eindigt de flow op de gebruikte locatiepagina.
10. Als een gescande productcode onbekend is, kan de gebruiker in dezelfde scanflow een
    nieuw product aanmaken of de code aan een bestaand product koppelen.
11. Nieuw product aanmaken voor een onbekende code gebeurt in een modaal venster met de
    gescande code vooraf ingevuld maar bewerkbaar.
12. Na nieuw product aanmaken of code koppelen aan een bestaand product gaat de
    inruimflow direct verder met locatie en hoeveelheid.

**Legacy-impact**

- Dekt de scan- en productidentificatiekant van `US2.5` barcodes koppelen aan producten
  en `US2.6` barcode scannen bij zoeken/inventorygebruik, maar nu specifiek gericht op
  de inruimflow.
- Levert een eerste praktische invulling voor `US2.14` QR-scanner-modus doordat het
  bestaande scanmenu nu voorraadgerichte vervolgstappen kan starten op basis van product-
  of locatiecodes.
- Laat `US2.10` voorraad aanpassen, `US2.13` voorraadlogboek en `US2.20` verbruik via
  barcode bewust open voor latere inventory-slices.

**Handmatige acceptatietest**

1. Open het bestaande menu `Scannen`.
2. Scan een bekende locatie-QR en controleer dat direct de juiste locatiepagina opent.
3. Ga terug naar `Scannen`, scan een bekende productcode en controleer dat de
   inruimflow start.
4. Controleer dat de laatst gebruikte locatie wordt voorgesteld en dat eventuele andere
   bekende locaties zichtbaar zijn in een kleine lijst.
5. Bevestig de voorgestelde locatie of kies handmatig een andere locatie, vul een
   hoeveelheid in en sla op.
6. Controleer dat direct na opslaan de vraag verschijnt of nog een product gescand moet
   worden.
7. Kies `Ja` en controleer dat de scanner in dezelfde sessie opnieuw actief wordt.
8. Scan een onbekende productcode en controleer dat je kunt kiezen voor nieuw product
   aanmaken, code koppelen aan bestaand product of annuleren.
9. Kies `Nieuw product`, controleer dat een modaal productformulier opent met de
   gescande code vooraf ingevuld maar bewerkbaar, rond dit af en controleer dat de
   inruimflow daarna direct verdergaat.
10. Herhaal met `Code koppelen aan bestaand product` en controleer dat de inruimflow
    daarna ook direct verdergaat.
11. Rond een inruimactie af en kies daarna `Nee`; controleer dat de flow eindigt op de
    locatiepagina waar het product is weggelegd.
