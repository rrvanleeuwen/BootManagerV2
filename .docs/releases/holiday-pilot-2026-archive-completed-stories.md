# BootManager Holiday Pilot 2026 — Archief afgeronde stories

Dit archief bevat de volledig uitgewerkte stories die functioneel en administratief
zijn afgerond. `holiday-pilot-2026.md` blijft daarmee compact voor actuele sturing,
terwijl historische scope, acceptatie, legacy-impact en implementatiestatus
beschikbaar blijven wanneer die opnieuw nodig zijn.

## Afgeronde stories

### PILOT-INV-08 — Product-zoekdetails en A4-tagbatchprint vanuit bestaande beheerflows

**Status:** Done; technisch gecontroleerd en handmatig geaccepteerd op 2026-07-16.

**Resultaat:** Productzoekresultaten in `Voorraadbeheer > Producten` hebben nu naast
het bestaande hoofdklikgedrag een compacte detailactie en een directe bewerkactie voor
het specifieke product. De detailactie opent in context een productpopup met naam,
standaardeenheid, gekoppelde code indien aanwezig en actieve voorraad-/locatiesamenvatting.
De bewerkactie opent via `editProductId` direct de bestaande productbewerkform met de
sectie `Gekoppelde code`, zodat een barcode sneller toegevoegd of gewijzigd kan worden
zonder door de lange productlijst te scrollen. `Opslag > Tagoverzicht` heeft daarnaast
een directe batchprintactie naar de bestaande A4-tagweergave voor alle beschikbare
locatietags.

**Als** Owner of Crew<br>
**wil ik** vanuit de bestaande productzoek- en tagbeheerflows sneller de juiste
vervolgactie kunnen kiezen<br>
**zodat** dagelijks gebruik aan boord minder omwegen vraagt.

**Scope**

- In `Voorraadbeheer > Producten` blijft het bestaande hoofdklikgedrag van een
  zoekresultaat behouden.
- Elk zoekresultaat krijgt daarnaast een expliciete zichtbare detailactie.
- Die detailactie opent dezelfde soort producteigenschappen-popup als al bruikbaar is
  vanuit locatiecontexten:
  - productnaam;
  - standaardeenheid;
  - gekoppelde code indien aanwezig;
  - relevante locatie- of voorraadsamenvatting voor het gekozen product.
- De popup opent in context zonder directe navigatie naar een locatiepagina.
- Elk zoekresultaat krijgt daarnaast een directe bewerkactie naar het specifieke
  product, zodat de bestaande code-/barcode-sectie direct bereikbaar is.
- In `Opslag > Tagoverzicht` komt een directe actie om alle beschikbare locatietags via
  de bestaande A4-printroute te openen.
- Als alle tags niet op één pagina passen, ondersteunt die printroute automatisch
  meerdere pagina's met dezelfde bestaande tagopmaak.

**Buiten scope**

- Brede herbouw van het volledige productoverzicht uit `PILOT-INV-06`.
- Nieuwe voorraadlogica, mutatietypes of scanroutes.
- Nieuwe QR-formaten, labeltypes of exportkanalen buiten de bestaande A4-tagroute.
- Grote herinrichting van productdetailinformatie buiten wat nodig is voor een compacte
  popup.
- Nieuwe barcode-scanflow binnen het productformulier.

**Acceptatiecriteria**

1. Een klik op een productzoekresultaat in `Voorraadbeheer > Producten` behoudt het
   bestaande hoofdgedrag voor navigatie of terugvinden.
2. Elk productzoekresultaat heeft daarnaast een aparte zichtbare detailactie.
3. Die detailactie opent een compacte producteigenschappen-popup zonder directe
   navigatie.
4. De popup toont minimaal productnaam, standaardeenheid en gekoppelde code indien
   aanwezig.
5. Elk productzoekresultaat heeft een aparte bewerkactie die direct de bestaande
   bewerkform voor dat product opent.
6. `Opslag > Tagoverzicht` biedt een directe batchprintactie voor alle beschikbare
   tags.
7. De batchprint gebruikt de bestaande A4-tagweergave en ondersteunt meerdere pagina's
   wanneer het aantal tags daar om vraagt.

**Handmatige acceptatietest**

1. Open `Voorraadbeheer > Producten` en zoek een bestaand product.
2. Klik op de nieuwe detailactie in een zoekresultaat en controleer dat een compacte
   productpopup opent met de afgesproken basisinformatie.
3. Klik daarna op het zoekresultaat zelf en controleer dat het bestaande
   navigatie-/terugvindgedrag ongewijzigd blijft.
4. Klik op de nieuwe bewerkactie en controleer dat de bestaande bewerkform voor het
   juiste product opent met de sectie `Gekoppelde code` zichtbaar.
5. Voeg of wijzig een code en sla op.
6. Open `Opslag > Tagoverzicht` en gebruik de nieuwe batchprintactie.
7. Controleer dat het bestaande A4-tagoverzicht nu voor alle tags tegelijk opent en bij
   grotere aantallen automatisch over meerdere pagina's verdeeld kan worden.

**Implementatiestatus 2026-07-16**

- technisch gecontroleerd door Codex;
- handmatige acceptatie door gebruiker akkoord bevonden;
- gerichte regressies groen:
  `dotnet test BootManager.UnitTests/BootManager.UnitTests.csproj --no-restore --filter "FullyQualifiedName~ProductsComponentTests"`;
- solution-build groen:
  `dotnet build BootManager.sln --no-restore`;
- diffcheck schoon:
  `git diff --check`.

### PILOT-INV-07 — Owner-only CSV-startimport voor echte vakantievoorraad, locatie-mapping en QR-tags

**Status:** Done; technisch gecontroleerd en handmatig geaccepteerd op 2026-07-16.

**Resultaat:** BootManager ondersteunt nu een Owner-only CSV-startimport voor de al
fysiek ingeladen vakantievoorraad. De import verwerkt het concrete
`voorraadoverzicht_boot_zomervakantie.csv`, bewaart bestaande eenheden en categorieën,
vraagt per nieuwe CSV-locatienaam een eenmalige mapping naar gebied en locatie,
maakt daarna producten, locaties, voorraadregels en QR-tokens aan en biedt direct een
printvriendelijk A4-overzicht van alle relevante locatietags. Geimporteerde producten
mogen zonder barcode en zonder verplichte categorie bestaan; een later onbekend
gescande barcode kan alsnog aan zo'n bestaand product gekoppeld worden via de
bestaande scanflow.

**Als** Owner<br>
**wil ik** een bestaand CSV-voorraadoverzicht van de al ingeladen
vakantieboodschappen kunnen importeren<br>
**zodat** de echte bootvoorraad snel in BootManager staat zonder alle producten,
locaties en beginhoeveelheden handmatig opnieuw in te voeren.

**Scope**

- Alleen de Owner kan een CSV-startimport uitvoeren.
- De import is gericht op het concrete bestand
  `.docs/extraInfo/voorraadoverzicht_boot_zomervakantie.csv` en hetzelfde kolommodel:
  `Aantal`, `Eenheid`, `Product`, `Locatie`.
- Voor de import verwijdert BootManager alle bestaande inventory-data behalve
  eenheden en categorieën.
- Bestaande eenheden blijven behouden; ontbrekende eenheden uit het CSV mogen tijdens
  de import worden aangemaakt.
- Voor iedere nieuwe CSV-locatienaam vraagt BootManager om gebied- en
  locatie-mapping, inclusief aanmaken waar nodig, en hergebruikt die mapping daarna
  voor alle regels met dezelfde locatietekst.
- Na bevestigde mapping maakt BootManager de benodigde gebieden, locaties, producten,
  voorraadregels en QR-tokens aan.
- Na een geslaagde import is een printvriendelijk A4-tagoverzicht beschikbaar met per
  QR-code minimaal de gebiedsnaam en locatienaam.
- Geimporteerde producten mogen zonder categorie en zonder barcode bestaan; later kan
  een onbekende gescande barcode aan een bestaand geimporteerd product gekoppeld
  worden.

**Buiten scope**

- Generieke import/exportmodule voor willekeurige CSV-formaten.
- Crew-toegang tot import of destructieve reset.
- Automatische afleiding van gebieden uit vrije locatietekst zonder Owner-bevestiging.
- Verplichte categorisering tijdens import.
- Nieuwe barcodeverwerking buiten de al bestaande scan- en koppelroutes.
- Opschonen of migreren van data buiten voorraadbeheer.

**Acceptatiecriteria**

1. Alleen de Owner kan de CSV-startimport openen en uitvoeren.
2. Voor de eigenlijke import verwijdert BootManager alle bestaande voorraadbeheerdata
   behalve eenheden en categorieën.
3. Ontbrekende eenheden uit het CSV worden aangemaakt; bestaande eenheden blijven
   bruikbaar.
4. Voor iedere nog onbekende CSV-locatienaam vraagt BootManager expliciet om gebied-
   en locatie-mapping, inclusief aanmaken waar nodig.
5. Eenzelfde CSV-locatienaam hoeft tijdens een import maar eenmalig gemapt te worden
   en wordt daarna consequent hergebruikt.
6. Na afronding bestaan alle geimporteerde producten, locaties en voorraadregels met
   de hoeveelheden uit het CSV.
7. Voor alle gebruikte locaties is direct een BootManager QR-token beschikbaar.
8. Het systeem biedt na import een printvriendelijk A4-overzicht van alle relevante
   QR-tags met minimaal gebied en locatienaam per tag.
9. Geimporteerde producten mogen zonder categorie bestaan en blijven later handmatig
   te categoriseren.
10. Een later gescande onbekende barcode kan aan een geimporteerd bestaand product
    worden gekoppeld zonder dat daarvoor een nieuw product hoeft te worden aangemaakt.

**Legacy-impact**

- `US2.15 Bulkimport/export voorraad` is nu `Partial`: de pilot dekt een beperkte
  eenmalige CSV-startimport voor vakantievoorraad af, maar generieke import/export
  blijft open.
- Verdiept `US1.12` en `US1.15` praktisch verder doordat alle nieuw gebruikte
  importlocaties direct QR-tokens en een batchprintbaar tagoverzicht krijgen.

**Handmatige acceptatietest**

1. Log in als Owner en open de CSV-startimport.
2. Bevestig dat de flow duidelijk waarschuwt dat bestaande voorraadbeheerdata wordt
   verwijderd, terwijl eenheden en categorieën behouden blijven.
3. Upload `voorraadoverzicht_boot_zomervakantie.csv`.
4. Doorloop voor alle nieuwe locatienamen de mappingstap door gebieden en locaties te
   kiezen of aan te maken.
5. Rond de import af en controleer dat de oude testvoorraad, testproducten,
   testlocaties en oude locatietokens verdwenen zijn.
6. Controleer dat de geimporteerde producten en voorraadregels overeenkomen met het
   CSV-bestand.
7. Controleer dat alle gebruikte locaties een QR-tag hebben en dat het A4-overzicht
   per tag gebied en locatienaam toont.
8. Open daarna `Scannen`, scan een nog onbekende productbarcode en koppel die aan een
   bestaand geimporteerd product.
9. Controleer dat die barcode daarna als bekende productcode werkt in de bestaande
   scanflow.

### PILOT-UX-01 — Home optimaliseren als snelle pilot-hub

**Status:** Done; technisch gecontroleerd en handmatig geaccepteerd op 2026-06-25.

**Resultaat:** home is nu de dagelijkse pilotstart in plaats van een directe
doorstuurroute naar dashboard. De pagina volgt de aangeleverde mockup-hiërarchie met
snelle tegels naar `Logboek`, `Dashboard` en `Scannen`, plus een directe
productzoekwidget met responsieve lijst-/cardweergave en paginering. Een klik op een
zoekresultaat in home opent nu eerst productinformatie in home-context, waarna de
gebruiker direct kan doorstarten naar de verbruiksflow voor dat product.

**Als** Owner of Crew<br>
**wil ik** op de homepagina direct naar `Logboek`, `Dashboard` en `Scannen` kunnen
gaan en meteen producten kunnen zoeken<br>
**zodat** de meest gebruikte pilotacties zonder omwegen bereikbaar zijn.

**Scope**

- Home is de standaard landingspagina na opstarten en inloggen.
- Drie duidelijke primaire tegels op home:
  - `Logboek`;
  - `Dashboard`;
  - `Scannen`.
- Directe productzoekwidget zonder extra navigatie.
- Per resultaat zichtbaar:
  - productnaam;
  - totale hoeveelheid;
  - eenheid;
  - locaties.
- Resultaten per 10 gepagineerd.
- Desktop als compacte lijst, mobiel als cards.
- Klikken op een productresultaat in home opent een productgerichte detailstap binnen
  home-context in plaats van directe navigatie naar een locatie.
- Vanuit die detailstap kan de gebruiker direct naar de verbruiksflow voor het
  gekozen product.

**Buiten scope**

- Nieuwe dashboardinhoud of extra dashboardwidgets.
- Nieuwe logboekfunctionaliteit.
- Brede herbouw van scanflows buiten de home-ingang en doorsteken.

**Acceptatiecriteria**

1. Home toont direct zichtbare primaire tegels voor `Logboek`, `Dashboard` en
   `Scannen`.
2. De productzoekwidget is op home bruikbaar zonder extra navigatie.
3. Elk resultaat toont productnaam, hoeveelheid, eenheid en locaties.
4. Resultaten zijn gepagineerd per 10 items.
5. Desktop toont resultaten als lijst; mobiel toont resultaten als cards.
6. Een klik op een product in home opent eerst productinformatie in plaats van direct
   een locatiepagina.
7. Vanuit die productinformatie is een directe route beschikbaar om verbruik voor dat
   product te registreren.
8. De resulterende UI volgt aantoonbaar de aangeleverde mockup-hiërarchie en voelt
   niet als generieke bootstrap-lijst.

**Legacy-impact**

- Verdiept `US2.12 Zoeken en filteren` met een extra dagelijkse zoekingang op home.
- Verdiept `US7.11 Interactieve navigatie` met directe pilotdoorklikken vanaf home.

**Handmatige acceptatietest**

1. Open home op desktop en controleer dat `Logboek`, `Dashboard` en `Scannen` als
   duidelijke primaire tegels zichtbaar zijn.
2. Zoek een product dat op meerdere locaties ligt en controleer dat productnaam,
   hoeveelheid, eenheid en alle locaties zichtbaar zijn.
3. Klik een product aan en controleer dat eerst productinformatie opent, niet direct
   een locatiepagina.
4. Start vanuit die productinformatie de verbruiksactie en controleer dat het product
   vooraf geselecteerd wordt in de verbruiksflow.
5. Controleer paginering na meer dan 10 resultaten.
6. Herhaal op mobiel en controleer dat dezelfde resultaten als cards verschijnen.

### PILOT-SCAN-05 — Onbekende-code-flow binnen nieuwe scanervaring

**Status:** Done; technisch gecontroleerd en handmatig geaccepteerd op 2026-06-25.

**Resultaat:** onbekende productcodes blijven nu volledig binnen de nieuwe
scanervaring. Vanuit `Scannen` opent een onbekende code een nieuw beslisscherm met
drie expliciete keuzes: nieuw product aanmaken, aan bestaand product koppelen of
annuleren. Zowel de koppelroute als de productaanmaakroute blijven binnen nieuwe
scanflow-schermen en vallen niet meer zichtbaar terug naar `/scan/old` of generieke
beheerpagina's. In de nieuwe productaanmaakroute kiest de gebruiker expliciet een
standaardeenheid voordat opslaan mogelijk is.

**Als** Owner of Crew<br>
**wil ik** dat een onbekende scan direct een kort, duidelijk en veilig beslispad opent
binnen de nieuwe scanervaring<br>
**zodat** ik een nieuwe productcode kan afhandelen zonder legacy-terugval of
contextverlies.

**Scope**

- Een nieuw onbekende-code-scherm binnen de nieuwe scanroutes.
- Expliciete keuze tussen:
  - nieuw product aanmaken;
  - aan bestaand product koppelen;
  - annuleren en terug naar scanstart.
- Behoud van de gescande code als leidende context totdat de gebruiker annuleert of
  opslaat.
- Doorzetten naar nieuwe vervolgstappen voor koppelen of productaanmaak zonder
  zichtbare legacy-terugval.
- UI-herbouw conform de scanflow- en UI-richtlijnen.
- Verplichte keuze van standaardeenheid in de nieuwe productaanmaakroute.

**Buiten scope**

- Definitieve verwijdering van de oude scanflow.
- Brede herbouw van al geaccepteerde bekende product- of locatieflows.
- Nieuwe beheer- of rapportagefuncties buiten de onbekende-code-afhandeling.

**Acceptatiecriteria**

1. Een onbekende scan eindigt niet meer zichtbaar op `/scan/old`.
2. De gebruiker ziet direct dat de code onbekend is en welke drie veilige keuzes er
   zijn.
3. `Nieuw product aanmaken` blijft binnen nieuwe scanflow-schermen en voelt niet als
   een terugval naar een oud CRUD-scherm.
4. `Aan bestaand product koppelen` blijft binnen nieuwe scanflow-schermen en leidt
   daarna logisch door naar vervolgwerk binnen dezelfde scanervaring.
5. `Annuleren` brengt de gebruiker bewust en begrijpelijk terug naar `/scan`.
6. De gescande code blijft zichtbaar of anderszins duidelijk leidend totdat de
   gebruiker annuleert of opslaat.
7. De schermen volgen zichtbaar de afgesproken scan-UI-richtlijnen op mobiel en
   desktop.
8. De gebruiker kan in de nieuwe productaanmaakroute expliciet een standaardeenheid
   kiezen en zonder die keuze niet opslaan.

**Legacy-impact**

- Rondt `US2.14 QR-scanner-modus` functioneel af in de nieuwe scanervaring.
- Vervangt voor dit pad de zichtbare oude scanflow-afhandeling uit `PILOT-INV-03`
  door een nieuwe scanroute, terwijl de onderliggende functionele keuzes
  `nieuw product`, `koppelen` en `annuleren` behouden blijven.

**Handmatige acceptatietest**

1. Open `/scan` als ingelogde Owner of Crew.
2. Scan of voer een onbekende productcode in.
3. Controleer dat de app niet eindigt op `/scan/old` of een generieke beheerpagina.
4. Controleer dat direct een nieuw onbekende-code-scherm zichtbaar is met de drie
   keuzes `Nieuw product aanmaken`, `Aan bestaand product koppelen` en `Annuleren`.
5. Kies `Annuleren` en controleer dat de gebruiker terugkomt op `/scan`.
6. Kies `Nieuw product aanmaken`, selecteer expliciet een eenheid, rond de minimale
   flow af en controleer dat de vervolgstappen binnen de nieuwe scanervaring blijven.
7. Herhaal met `Aan bestaand product koppelen` en controleer dat ook deze route
   binnen de nieuwe scanervaring blijft en logisch doorloopt.

### PILOT-INV-04 — Product terugvinden via scan of zoeken

**Status:** Done; technisch gecontroleerd en handmatig geaccepteerd op 2026-06-20.

**Resultaat:** BootManager ondersteunt nu een praktische terugvindflow voor bekende
producten via zowel `Scannen` als handmatig zoeken in `Voorraadbeheer > Producten`.
Bij precies één actieve locatie opent direct de juiste locatiepagina. Bij meerdere
actieve locaties verschijnt een compacte locatielijst met gebied, locatie, hoeveelheid
en eenheid. Als een product bekend is maar niet actief op voorraad ligt, toont
BootManager dit duidelijk, inclusief verwachte laatst gebruikte locatie wanneer die
beschikbaar is, en kan de gebruiker direct via een compacte modal nieuwe voorraad op
een gekozen locatie toevoegen.

**Als** Owner of Crew<br>
**wil ik** een product snel kunnen terugvinden via scannen of handmatig zoeken<br>
**zodat** ik direct zie op welke locatie of locaties het product ligt en daar
desgewenst naartoe kan navigeren of meteen opnieuw voorraad kan toevoegen.

**Scope**

- De primaire route start vanuit het bestaande menu `Scannen`.
- Een bekende productcode start direct de terugvindflow.
- Handmatige fallback is beschikbaar via `Voorraadbeheer > Producten`.
- Handmatig zoeken werkt op productnaam en productomschrijving, hoofdletterongevoelig
  en met deelmatches.
- Meerdere zoekresultaten tonen een compacte productlijst met productnaam,
  omschrijvingstekst en locatiesamenvatting zonder hoeveelheden.
- Een product met precies één actieve voorraadlocatie opent direct de bestaande
  locatiepagina.
- Een product met meerdere actieve voorraadlocaties toont een lijst met gebied,
  locatienaam, hoeveelheid en eenheid per locatie.
- Vanuit die lijst kan de gebruiker doorklikken naar een locatiepagina.
- Een bekend product zonder actieve voorraad toont een duidelijke melding.
- Als een laatst gebruikte locatie nog bekend is, wordt die als verwachte plek
  getoond met leesbare gebied- en locatienaam.
- Vanuit de `geen actieve voorraad`-situatie kan de gebruiker direct `Voorraad
  toevoegen` starten via een compacte modal waarin het product al vaststaat en alleen
  locatie en hoeveelheid nog gekozen worden.

**Buiten scope**

- Dashboard-zoekbalk of andere extra hoofdroutes buiten `Scannen` en
  `Voorraadbeheer > Producten`.
- Verbruik, correcties, tellingen of mutatiehistorie.
- Geavanceerde filters, fuzzy matching, synoniembeheer of typo-correctie.
- Hoeveelheden tonen in de eerste productresultatenlijst van handmatig zoeken.

**Acceptatiecriteria**

1. De gebruiker kan vanuit `Scannen` een bekende productcode scannen en direct de
   terugvindflow starten.
2. De gebruiker kan ook handmatig zoeken via `Voorraadbeheer > Producten`.
3. Handmatig zoeken doorzoekt productnaam en omschrijving, is hoofdletterongevoelig en
   ondersteunt deelmatches.
4. Als handmatig zoeken meerdere producten vindt, toont BootManager een compacte lijst
   met productnaam, omschrijvingstekst en locatiesamenvatting zonder hoeveelheden.
5. Als een gescand of gekozen product precies één actieve voorraadlocatie heeft,
   opent direct de locatiepagina van die locatie.
6. Als een gescand of gekozen product meerdere actieve voorraadlocaties heeft, toont
   BootManager direct een lijst met gebied, locatienaam, hoeveelheid en eenheid per
   locatie.
7. Vanuit die locatielijst kan de gebruiker doorklikken naar een locatiepagina.
8. Als een product bekend is maar geen actieve voorraadlocaties heeft, meldt
   BootManager dat duidelijk.
9. Als voor dat product nog een laatst gebruikte locatie bekend is, toont BootManager
   die locatie als verwachte plek waar het product normaal hoort te liggen.
10. In beide `geen actieve voorraad`-gevallen biedt BootManager een actie
    `Voorraad toevoegen`.
11. `Voorraad toevoegen` opent in die situatie direct een compacte modal voor
    locatiekeuze en hoeveelheid, zonder eerst via het locatieoverzicht te navigeren.

**Legacy-impact**

- `US2.6 Barcode scannen bij zoeken` is functioneel afgedekt: een bekende productcode
  in `Scannen` start nu het aparte terugvindpad.
- Verdiept `US2.9 Voorraad bekijken per locatie` met directe terugvindnavigatie vanaf
  productperspectief, bovenop de bestaande locatie- en productdetailweergave uit
  `PILOT-INV-02`.
- `US2.12 Zoeken en filteren` is nu gedeeltelijk afgedekt via eenvoudige zoekingang op
  naam en omschrijving; uitgebreide filters en bredere zoekmogelijkheden blijven open.
- Verdiept `US2.14 QR-scanner-modus` doordat het scanmenu nu naast inruimen ook
  product-terugvinden ondersteunt.
- Laat `US2.10`, `US2.13` en `US2.20` bewust open voor latere inventory-slices.

**Handmatige acceptatietest**

1. Open `Scannen` en scan een bekende productcode van een product dat op precies één
   locatie ligt; controleer dat direct de juiste locatiepagina opent.
2. Scan een bekende productcode van een product dat op meerdere locaties ligt en
   controleer dat direct een locatielijst opent met gebied, locatienaam, hoeveelheid
   en eenheid.
3. Klik vanuit die lijst door naar een locatiepagina en controleer dat de juiste
   locatie wordt geopend.
4. Open `Voorraadbeheer > Producten`, zoek handmatig op een productnaam met
   hoofdletterverschil en controleer dat het product wordt gevonden.
5. Zoek handmatig op tekst die alleen in de omschrijving voorkomt en controleer dat
   het product ook dan wordt gevonden.
6. Controleer dat meerdere zoekresultaten eerst een korte productlijst tonen met
   productnaam, omschrijvingstekst en locatiesamenvatting, zonder hoeveelheden.
7. Kies een product uit die lijst en controleer dat het vervolggedrag gelijk is aan de
   scanroute: direct locatiepagina bij één locatie of locatielijst bij meerdere
   locaties.
8. Open een bekend product zonder actieve voorraadlocaties en controleer dat
   BootManager meldt dat het momenteel niet op voorraad is.
9. Controleer dat, als voor dit product nog een laatst gebruikte locatie bekend is,
   BootManager die als verwachte plek toont.
10. Klik in die situatie op `Voorraad toevoegen`, kies in de modal een locatie, vul
    een hoeveelheid in, sla op en controleer dat direct de gekozen locatiepagina opent
    met de bijgewerkte voorraad.

### PILOT-INV-03 — Scan-gestuurde inruimflow met locatievoorstel

**Status:** Done; technisch gecontroleerd en handmatig geaccepteerd op 2026-06-20.

**Resultaat:** het bestaande menu `Scannen` is nu de praktische inventory-start voor
inruimen. Een bekende locatie-QR opent direct de locatiepagina; een bekende productcode
start een inruimflow met locatievoorstel, alternatieve locaties, handmatige fallback en
doorlopende scansessie. Onbekende productcodes kunnen in dezelfde flow leiden tot nieuw
product aanmaken of code koppelen aan bestaand product. Bij nieuw product aanmaken is
de gescande code vooraf ingevuld maar bewerkbaar en kiest de gebruiker expliciet een
standaardeenheid voordat de flow verdergaat.

**Als** Owner of Crew<br>
**wil ik** vanuit `Scannen` een productcode kunnen scannen en daarna snel locatie en
hoeveelheid kunnen bevestigen<br>
**zodat** ik meerdere producten achter elkaar praktisch kan inruimen zonder steeds
terug te vallen op handmatige beheerflows.

**Scope**

- `Scannen` herkent BootManager locatie-QR's en productcodes en kiest per type de
  juiste vervolgstap.
- Een bekende locatie-QR opent direct de bestaande locatiepagina.
- Een bekende productcode start de inruimflow voor dat product.
- Voor bekende producten stelt BootManager de laatst gebruikte locatie voor op basis
  van de meest recente voorraadtoevoeging of aanvulling.
- Andere bekende locaties voor dat product worden als leesbare alternatieven getoond.
- De gebruiker kan de voorgestelde locatie bevestigen, handmatig een andere locatie
  kiezen of een locatie-QR scannen binnen dezelfde flow.
- Als een product nog geen eerdere locatie heeft, kiest of scant de gebruiker direct
  een locatie.
- Daarna vult de gebruiker alleen een hoeveelheid in; de standaardeenheid is zichtbaar
  maar niet wijzigbaar in deze stap.
- Na opslaan wordt de voorraad additief verwerkt volgens `PILOT-INV-02`.
- Na succesvolle opslag kan de gebruiker direct nog een product scannen in dezelfde
  sessie, of stoppen op de gebruikte locatiepagina.
- Bij een onbekende productcode kan de gebruiker direct een nieuw product aanmaken,
  de code koppelen aan een bestaand product of annuleren.
- Nieuw product aanmaken binnen deze flow gebruikt een vooraf ingevulde maar
  bewerkbare code en een verplichte keuze van standaardeenheid.

**Buiten scope**

- Verbruik, correcties, overschrijven van hoeveelheden en mutatiehistorie.
- Een aparte dashboardstart buiten het bestaande menu `Scannen`.
- Volledige productbeheerflow buiten de minimale onbekende-code-afhandeling.
- Product terugvinden via een aparte scan-/zoekflow; dat volgt in `PILOT-INV-04`.

**Acceptatiecriteria**

1. Scannen vanuit het bestaande menu start voor locatie-QR en productcode de juiste
   flow.
2. Een bekende locatie-QR opent direct de bestaande locatiepagina.
3. Een bekende productcode start direct de inruimflow.
4. Voor een bekend product toont BootManager de laatst gebruikte locatie en eventuele
   alternatieve locaties als leesbare opties.
5. De gebruiker kan de voorgestelde locatie bevestigen of een andere locatie kiezen of
   scannen.
6. Zonder locatiegeschiedenis vraagt de flow direct om een locatie te kiezen of te
   scannen.
7. Daarna vult de gebruiker alleen een hoeveelheid in; de standaardeenheid is zichtbaar
   maar niet wijzigbaar.
8. Opslaan werkt additief volgens `PILOT-INV-02`.
9. Na opslaan kan de gebruiker direct nog een product scannen in dezelfde sessie of
   stoppen op de gebruikte locatiepagina.
10. Een onbekende productcode kan in dezelfde flow leiden tot nieuw product of code
    koppelen aan bestaand product.
11. Nieuw product aanmaken gebruikt een vooraf ingevulde maar bewerkbare code en een
    verplichte eenheidskeuze.
12. Na nieuw product aanmaken of code koppelen gaat de inruimflow direct verder met
    locatie en hoeveelheid.

**Legacy-impact**

- Rondt `US2.5` functioneel af door scan-gestuurde codekoppeling aan producten toe te
  voegen naast de bestaande handmatige cataloguskoppeling uit `PILOT-INV-01`.
- Verdiept `US2.6` en `US2.14` met een eerste echte inventory-scanflow voor inruimen,
  maar laat het aparte terugvind-/zoekpad bewust open voor `PILOT-INV-04`.
- Laat `US2.10`, `US2.13` en `US2.20` bewust open voor latere inventory-slices.

### PILOT-INV-02 — Taakgerichte voorraadbasis per locatie

**Status:** Done; technisch gecontroleerd en handmatig geaccepteerd op 2026-06-20.

**Resultaat:** de eerste bruikbare voorraadbasis per locatie is opgeleverd. Owner en
Crew kunnen nu vanaf een locatiepagina voorraad toevoegen, bestaand product zoeken op
naam of gekoppelde code, direct een nieuw product aanmaken binnen dezelfde locatieflow,
hoeveelheden additief opslaan per product-locatiecombinatie, actuele locatie-inhoud
tonen, regels verwijderen en op productniveau zien op welke locaties iets ligt. Na een
laat acceptatiepunt is ook de autorisatie gecorrigeerd: Crew ziet nu het hoofdmenu
`Opslag`, kan locatiepagina's openen en voorraad lezen/toevoegen, terwijl
opslagbeheer onder Owner-only beheer blijft.

**Als** Owner of Crew<br>
**wil ik** vanaf een locatiepagina voorraad aan die locatie kunnen toevoegen en
aanvullen<br>
**zodat** BootManager bruikbaar vastlegt wat waar ligt zonder mij door
administratieve CRUD-schermen te dwingen.

**Scope**

- Owner en Crew kunnen vanaf een locatiepagina de actie `Voorraad toevoegen` starten.
- De primaire route start vanaf een locatiepagina; dezelfde locatie blijft ook zonder
  scan handmatig bereikbaar via de bestaande locatienavigatie.
- Binnen `Voorraad toevoegen` kiest de gebruiker een bestaand product of maakt direct
  een nieuw product aan vanuit die locatiecontext.
- Als tijdens deze flow een nieuw product wordt aangemaakt, keert de gebruiker daarna
  automatisch terug naar dezelfde locatieflow met dat product geselecteerd.
- Een voorraadregel legt functioneel alleen `product`, `locatie` en `hoeveelheid`
  vast.
- Hoeveelheid is een vrij numerieke waarde in de standaard eenheid van het product.
- Hetzelfde product kan op meerdere locaties tegelijk voorraad hebben.
- Per locatie bestaat voor een product maximaal één actuele voorraadregel.
- Als een product op die locatie al bestaat, wordt dezelfde voorraadregel hergebruikt
  en wordt de hoeveelheid aangevuld.
- De locatiepagina toont de actuele inhoud van die locatie met minimaal productnaam,
  hoeveelheid en eenheid.
- De productpagina toont op welke locaties het product ligt, met minimaal gebied,
  locatienaam en hoeveelheid.
- Een voorraadregel kan vanaf de locatiepagina eenvoudig worden verwijderd na
  bevestiging wanneer het product daar niet meer ligt.

**Buiten scope**

- Scan-gestuurde dashboardstart of automatische keuze van de juiste voorraadactie.
- Verplichte locatie-QR als hoofdroute voor productaanmaak of inruimen.
- Barcode scannen om een product terug te vinden.
- Verbruik, correcties, overschrijven van hoeveelheden, negatieve hoeveelheden en
  mutatiehistorie.
- Voorraad verplaatsen tussen twee locaties als samengestelde actie.
- Slimme recente lijsten, voorkeursproducten per locatie of automatische suggesties.
- Categorie-filters in de handmatige productzoekflow.
- Meerdere aparte voorraadregels voor hetzelfde product op dezelfde locatie.

**Acceptatiecriteria**

1. Owner en Crew kunnen een locatiepagina handmatig openen zonder scan en daar de
   actie `Voorraad toevoegen` starten.
2. In `Voorraad toevoegen` kan de gebruiker een bestaand product zoeken op
   productnaam of gekoppelde code.
3. In dezelfde flow kan de gebruiker ook direct een nieuw product aanmaken; na
   opslaan keert de flow terug naar dezelfde locatie met dat product geselecteerd.
4. De gebruiker kan vervolgens een vrij numerieke hoeveelheid invoeren en opslaan
   voor die locatie.
5. Als het gekozen product nog niet op die locatie ligt, ontstaat een nieuwe
   voorraadregel voor die product-locatie-combinatie.
6. Als het gekozen product al op die locatie ligt, wordt geen tweede regel aangemaakt
   maar wordt de bestaande hoeveelheid aangevuld.
7. Een actieve voorraadregel met hoeveelheid `0` of lager is niet toegestaan in deze
   story; zulke invoer wordt geblokkeerd.
8. De locatiepagina toont na opslaan de actuele producten op die locatie met minimaal
   naam, hoeveelheid en eenheid.
9. De productpagina toont voor een product alle gekoppelde locaties met minimaal
   gebied, locatienaam en hoeveelheid.
10. Een voorraadregel kan vanaf de locatiepagina na bevestiging direct verwijderd
    worden als het product daar niet meer ligt.

**Legacy-impact**

- `US2.8 Product koppelen aan opslaglocatie` is functioneel afgedekt.
- `US2.9 Voorraad bekijken per locatie` is functioneel afgedekt via locatie-inhoud en
  product-locatieweergave.
- `US2.19 Voorraad automatisch ophogen bij aankoop` is gedeeltelijk afgedekt: dezelfde
  product-locatieregel wordt additief aangevuld, maar zonder aparte aankoopflow,
  mutatietypes of historie.
- `US2.10`, `US2.13`, `US2.14` en `US2.20` blijven bewust open voor latere
  inventory-slices.

**Handmatige acceptatietest**

Log in als Owner of Crew. Open handmatig een bestaande locatiepagina via de
locatienavigatie en controleer dat `Voorraad toevoegen` beschikbaar is. Voeg een
bestaand product toe met een positieve hoeveelheid en controleer dat de locatiepagina
daarna naam, hoeveelheid en eenheid toont. Voeg daarna op dezelfde locatie opnieuw
hetzelfde product toe en controleer dat de hoeveelheid wordt aangevuld in plaats van
gedupliceerd. Maak vervolgens vanuit dezelfde flow een nieuw product aan, controleer
dat je terugkeert naar dezelfde locatieflow en sla ook daarvoor een hoeveelheid op.
Open daarna de productpagina en controleer dat de gekoppelde locaties zichtbaar zijn.
Probeer tenslotte `0` of lager op te slaan en verwacht een blokkade; verwijder daarna
een voorraadregel vanaf de locatiepagina en controleer dat deze na bevestiging
verdwijnt.

### PILOT-INV-01 — Productcategorieen, producten en productbarcodes

**Status:** Done; technisch gecontroleerd en handmatig geaccepteerd op 2026-06-20.

**Resultaat:** een eerste lokale inventory-catalogus voor Owner en Crew is opgeleverd
met productcategorieën, eenheden, producten, één unieke gekoppelde code per product,
soft delete/heractiveren, additieve SQLite-migratie en een inventory-menu met
`Producten`, `Categorieen` en `Eenheden`. Handmatige acceptatie bevestigde de
catalogusflows, inline categorie-/eenheidaanmaak, code-uniciteit en
archiveringsblokkades. Direct barcode scannen binnen het handmatige productformulier
blijft nog als expliciet vervolgpunt open voor een latere scan-gerichte inventory-slice.

**Als** Owner of Crew<br>
**wil ik** productcategorieen, eenheden en producten met basisgegevens en een gekoppelde
code kunnen vastleggen en beheren<br>
**zodat** een lokale productcatalogus ontstaat die klaar is voor latere voorraad- en
scanflows.

**Scope**

- Owner en Crew beheren productcategorieen, eenheden en producten via
  `Voorraadbeheer`.
- Categorieen hebben unieke naam, optionele omschrijving, vaste icoonkeuze,
  archiveren en heractiveren.
- Eenheden hebben unieke naam, een defaultset, archiveren en heractiveren.
- Producten hebben naam, optionele omschrijving, verplichte standaardeenheid,
  maximaal één actieve categorie in de UI en soft delete via archiveren/reactiveren.
- Een gekoppelde productcode wordt als aparte entiteit bewaard, is
  hoofdletterongevoelig uniek binnen de volledige catalogus en blijft gereserveerd
  wanneer het product gearchiveerd is.
- Productformulier ondersteunt inline toevoegen van categorieen en eenheden.

**Buiten scope**

- Voorraadregels, hoeveelheden, mutaties en historie.
- Product-locatiekoppelingen en voorraadweergave per locatie.
- Product terugvinden via barcode of scan-gestuurde inruimflow.
- Meerdere gekoppelde codes per product.
- Externe EAN-productdatabase, automatische productherkenning, foto/label en
  minimumvoorraad.
- Direct barcode scannen binnen het handmatige productformulier; deze vervolgwens
  blijft open voor een latere scan-slice.

**Acceptatiecriteria**

- Owner en Crew kunnen categorieen beheren met unieke naam, optionele omschrijving en
  verplicht icoon uit een vaste set.
- Owner en Crew kunnen eenheden beheren met unieke naam en een aanwezige defaultset.
- Productaanmaak en -bewerking ondersteunen naam, categorie, standaardeenheid,
  optionele omschrijving en optionele gekoppelde code.
- Inline categorie-/eenheidaanmaak keert terug naar dezelfde productflow.
- Gekoppelde codes kunnen handmatig worden toegevoegd, vervangen en ontkoppeld.
- Dezelfde gekoppelde code kan niet aan twee producten tegelijk hangen, ook niet via
  een gearchiveerd product.
- Producten, categorieen en eenheden ondersteunen soft delete en heractiveren.
- Een categorie of eenheid met actieve productreferenties kan niet worden gearchiveerd.
- De catalogus werkt lokaal/offline-first binnen de bestaande BootManager-database.

**Legacy-impact**

- `US2.1 Categorieen beheren` is functioneel afgedekt.
- `US2.2 Categorie-icoontjes beheren` is gedeeltelijk afgedekt via een vaste
  ingebouwde icoonset; upload blijft open.
- `US2.3 Product aanmaken` is functioneel afgedekt voor de catalogusbasis.
- `US2.4 Product bewerken of verwijderen` is functioneel afgedekt via bewerken plus
  archiveren/reactiveren.
- `US2.5 Barcodes en QR-codes koppelen aan producten` is gedeeltelijk afgedekt:
  handmatige gekoppelde code-invoer is aanwezig; scan-ondersteunde code-invoer volgt
  later.
- `US2.6`, `US2.8`, `US2.9`, `US2.10`, `US2.11`, `US2.12`, `US2.13`, `US2.14`,
  `US2.19` en `US2.20` blijven open voor latere inventory-slices.

**Handmatige acceptatietest**

Log in als Owner of Crew. Open `Voorraadbeheer` en controleer dat `Producten`,
`Categorieen` en `Eenheden` bereikbaar zijn. Maak twee categorieen met verschillende
iconen aan, controleer dubbele-naamvalidatie en bevestig dat de eenheids-defaultset
zichtbaar is. Maak een product aan met naam, categorie, eenheid en optionele
omschrijving, voeg desgewenst inline een nieuwe categorie of eenheid toe en controleer
dat de productflow doorloopt. Voeg een gekoppelde code toe, probeer dezelfde code aan
een tweede product te koppelen en controleer dat dit geblokkeerd wordt. Ontkoppel de
code weer, archiveer daarna een product en controleer dat archiefweergave en
reactiveren werken. Probeer tenslotte een categorie of eenheid met een actief product
te archiveren en controleer dat dit wordt tegengehouden.

### PILOT-LOC-01 — Opslaggebieden en opslaglocaties

**Status:** Done; technisch gecontroleerd en handmatig geaccepteerd op 2026-06-18.

**Resultaat:** persistent Owner-beheer van gebieden en locaties, stabiele locatie-id's,
een door Owner en Crew leesbare detailpagina en een additieve SQLite-migratie zijn
opgeleverd. De handmatige acceptatie omvatte CRUD, verplaatsen, restrict-delete,
Crew-autorisatie, locatie-aanmaak via modal, navigatie in hetzelfde tabblad en correcte
terugnavigatie via browsergeschiedenis.

**Als** Owner<br>
**wil ik** opslaggebieden en opslaglocaties vastleggen<br>
**zodat** voorraad later aan fysieke plekken aan boord gekoppeld kan worden.

**Scope**

- Owner beheert opslaggebieden en opslaglocaties via `Instellingen > Opslag`.
- Owner kan opslaggebieden aanmaken, hernoemen en verwijderen.
- Owner kan opslaglocaties onder precies één gebied aanmaken, bewerken, verplaatsen
  naar een ander gebied en verwijderen.
- Een opslaglocatie heeft minimaal een naam en optioneel een korte beschrijving.
- De locatie-id blijft stabiel wanneer een locatie wordt hernoemd of naar een ander
  gebied wordt verplaatst.
- Owner en Crew kunnen een locatie-detailpagina openen waarop gebied, locatienaam
  en beschrijving zichtbaar zijn.
- Handmatige locatiekeuze via de beheer- en detailpagina vormt de basis voor latere
  QR- en voorraadflows.

**Buiten scope**

- QR-token genereren, koppelen, vervangen of ongeldig maken.
- Tagstatus, tagoverzicht, printen of exporteren van QR-codes.
- Scan-navigatie vanaf een QR-code naar een locatie.
- Producten, productbarcodes, voorraadregels, hoeveelheden en voorraadmutaties.
- Producten koppelen aan opslaglocaties.
- Voorraad bekijken per locatie, export/import en voorraadlogboek.
- Crew-beheerrechten voor opslaggebieden of opslaglocaties; Crew mag in deze story
  alleen de locatie-detailpagina lezen.

**Acceptatiecriteria**

- Owner ziet in `Instellingen` een sectie voor opslagbeheer.
- Owner kan een gebied aanmaken met een verplichte naam.
- Owner kan een gebied hernoemen.
- Owner kan een leeg gebied verwijderen.
- Een gebied met locaties kan niet per ongeluk worden verwijderd zonder eerst de
  locaties te verwijderen of te verplaatsen.
- Owner kan een locatie aanmaken onder een bestaand gebied met naam en optionele
  beschrijving.
- Owner kan de naam en beschrijving van een locatie aanpassen.
- Owner kan een locatie naar een ander gebied verplaatsen zonder dat de locatie-id
  verandert.
- Owner kan een locatie verwijderen.
- Locatienamen zijn binnen hetzelfde gebied niet dubbel; dezelfde locatienaam mag in
  een ander gebied opnieuw voorkomen.
- Owner en Crew kunnen de detailpagina van een bestaande locatie openen.
- Crew krijgt geen toegang tot `Instellingen > Opslag` of andere Owner-only
  beheerschermen.
- `dotnet build BootManager.sln` slaagt.

**Legacy-impact**

- `US1.9 Bootstructuurbeheer: gebieden en opslaglocaties` is gedeeltelijk afgedekt:
  BootManagerV2 heeft persistent beheer van gebieden en locaties, zonder QR/tag- en
  voorraadfunctionaliteit.
- `US1.10 Opslaglocatie aanmaken binnen gebied` is functioneel afgedekt voor
  aanmaken met naam en korte omschrijving.
- `US1.11 Opslaglocatie bewerken` is functioneel afgedekt voor naam, omschrijving,
  gebiedskoppeling en verwijderen.
- `US1.12 Tag genereren voor opslaglocatie` blijft open voor `PILOT-LOC-02` en
  `PILOT-LOC-03`.
- `US1.13 Locatie openen via QR-code` blijft open voor `PILOT-LOC-02`; deze story
  levert alleen de detailpagina die later door QR-scans geopend kan worden.
- `US1.14 Tag opnieuw koppelen of vervangen` blijft open voor `PILOT-LOC-04`.
- `US1.15 Overzicht van alle tags` blijft open voor `PILOT-LOC-04`.
- `US2.8 Product koppelen aan opslaglocatie` en `US2.9 Voorraad bekijken per
  locatie` blijven open voor de latere inventory-stories; `PILOT-LOC-01` levert
  alleen de locatiebasis.

**Handmatige acceptatietest**

Log in als Owner en open `Instellingen > Opslag`. Maak de gebieden `Kombuis`,
`Salon`, `Voorhut`, `Bakskist` en `Techniek` aan. Maak onder minimaal twee gebieden
een locatie met beschrijving aan. Hernoem een gebied, bewerk een locatiebeschrijving
en verplaats een locatie naar een ander gebied. Open de detailpagina van die locatie
en controleer dat de naam, beschrijving en het nieuwe gebied kloppen. Probeer een
gebied met locaties te verwijderen en controleer dat dit niet ongemerkt kan.

Log daarna in als Carla/Crew. Controleer dat `Instellingen` en opslagbeheer niet
toegankelijk zijn, maar dat een bestaande locatie-detailpagina wel leesbaar opent.

**Technische richting**

- Voeg een kleine opslagmodule toe binnen de bestaande Core/Application/Infrastructure
  en Web-laag; introduceer geen brede inventory-module in deze story.
- Gebruik persistente entiteiten voor `StorageArea` en `StorageLocation` met een
  verplichte relatie van locatie naar gebied.
- Houd validatie in de application-service: trim namen, blokkeer lege namen, blokkeer
  dubbele gebiedsnamen en blokkeer dubbele locatienamen binnen hetzelfde gebied.
- Plaats de beheer-UI onder de bestaande Owner-only `Settings`-route en gebruik waar
  nodig een apart component om `Settings.razor` beheersbaar te houden.
- Maak de locatie-detailpagina `Owner,Crew` toegankelijk zodat `PILOT-LOC-02` daar
  later bekende locatie-QR's naartoe kan routeren.

### PILOT-LOC-02 — QR-token genereren, koppelen en locatie openen

**Status:** Done; technisch gecontroleerd en handmatig geaccepteerd op 2026-06-19.

**Resultaat:** stabiele BootManager locatie-QR-tokens, Owner-only koppelen van
onbekende BootManager-QR's aan bestaande of nieuwe locaties, scanrouting naar de
bestaande locatie-detailpagina en SQLite-migratie-/constraintbewijs zijn opgeleverd.
De handmatige acceptatie bevestigde QR-generatie, direct openen van bekende locatie-QR's,
stabiel gedrag na hernoemen/verplaatsen, koppelen aan bestaande en nieuwe locaties en
het ontbreken van Crew-koppelacties. Een tijdens acceptatie gemelde afwijking bleek een
controle op een verkeerde dubbel voorkomende locatienaam en niet een productdefect.

**Als** Owner en Crew<br>
**wil ik** een locatie via een BootManager QR-code kunnen openen<br>
**zodat** een fysieke plek aan boord snel digitaal terug te vinden is.

**Scope**

- Owner kan voor een bestaande opslaglocatie een unieke BootManager QR-token aanmaken.
- Owner kan een onbekende BootManager QR-token koppelen aan een bestaande locatie.
- Owner kan na het scannen van een onbekende BootManager QR-token ook een nieuwe
  locatie aanmaken en de token daaraan koppelen.
- De QR-token is stabiel en niet gebaseerd op gebiedsnaam of locatienaam.
- De QR-code blijft geldig wanneer de locatie later wordt hernoemd of verplaatst.
- Scannen van een bekende locatie-QR opent direct de bestaande locatie-detailpagina.
- Crew kan bekende locatie-QR's scannen en openen.
- Crew kan onbekende QR's niet koppelen aan nieuwe of bestaande locaties.

**Buiten scope**

- QR-code printen of PNG exporteren; dat volgt in `PILOT-LOC-03`.
- QR-token vervangen, tagstatus en tagoverzicht; dat volgt in `PILOT-LOC-04`.
- Producten, productbarcodes, voorraadregels, hoeveelheden en voorraadmutaties.
- Voorraad bekijken of aanpassen vanaf een locatiepagina.
- Externe QR-diensten, cloud-sync of NFC.

**Acceptatiecriteria**

- Owner kan een unieke QR-token voor een locatie genereren.
- Dezelfde token kan niet aan twee locaties gekoppeld zijn.
- Een bekende locatie-QR opent voor Owner en Crew de juiste locatie-detailpagina.
- Hernoemen of verplaatsen van de locatie verandert de token niet.
- Een onbekende BootManager QR toont Owner een keuze om te koppelen aan een bestaande
  locatie of aan een nieuwe locatie.
- Een onbekende BootManager QR geeft Crew geen beheeractie.
- Niet-BootManager QR-waarden worden niet automatisch gekoppeld aan locaties.
- `dotnet build BootManager.sln` slaagt.

**Legacy-impact**

- `US1.12 Tag genereren voor opslaglocatie` wordt met deze story gedeeltelijk
  gepland: unieke token en QR-waarde per locatie; printen/exporteren volgt in
  `PILOT-LOC-03`.
- `US1.13 Locatie openen via QR-code` wordt met deze story gedeeltelijk gepland:
  QR opent de locatie-detailpagina; producten en aantallen blijven voor inventory.
- `US2.14 QR-scanner-modus` wordt verder gedeeltelijk gepland: locatie-QR routing
  komt in deze story; voorraad bekijken of wijzigen blijft voor latere inventory.

**Handmatige acceptatietest**

Log in als Owner, open een bestaande locatie en genereer een QR-token. Scan of voer de
QR-waarde handmatig in en controleer dat de locatie-detailpagina opent. Hernoem of
verplaats de locatie en controleer dat dezelfde QR nog steeds de locatie opent. Scan
een onbekende BootManager QR en koppel deze eerst aan een bestaande locatie en daarna
in een aparte test aan een nieuwe locatie. Log daarna in als Carla/Crew en controleer
dat bekende locatie-QR's openen, maar onbekende QR's geen koppelactie toestaan.

**Technische richting**

- Gebruik een stabiel BootManager-specifiek QR-value format, zodat de scanflow eigen
  locatie-QR's kan onderscheiden van productbarcodes en willekeurige QR-waarden.
- Sla token los van locatienaam en gebied op.
- Laat de bestaande generieke scanpagina de tokenwaarde herkennen en naar de juiste
  locatieflow routeren.
- Houd onbekende-token-koppeling Owner-only en laat Crew alleen lezen/openen.

**Implementatiestatus 2026-06-19**

- `StorageLocation` heeft nu een persistente nullable `QrToken` met een unieke
  gefilterde SQLite-index voor niet-null tokens.
- Owner kan op de locatie-detailpagina een QR-token genereren; generatie is idempotent
  en bestaande tokens worden in deze story niet vervangen.
- De scanpagina herkent exact `bootmanager:location:<32-lowercase-hex-token>` en opent
  bekende locatie-QR's direct of toont alleen voor Owner een koppelactie bij een
  onbekende geldige locatie-QR.
- Owner kan een onbekende geldige locatie-QR koppelen aan een bestaande locatie of een
  nieuwe locatie met token aanmaken; Crew krijgt geen koppelactie en de route blijft
  Owner-only.
- Integratietests bewijzen migratie vanaf
  `20260618175732_AddStorageAreasAndLocations`, databehoud, nullable tokenopslag,
  uniqueness en de acceptatieketen koppelen -> verse detail-load.
- Eindchecks: gerichte storage unit-tests 96/96; volledige unit-suite 292/293 met
  alleen de bekende owner-recoverybaseline rood; gerichte storage-integratietests
  24/24; volledige integratiesuite 36/36; `dotnet build BootManager.sln --no-restore`;
  `git diff --check`.

### PILOT-LOC-03 — QR-tag printen en PNG exporteren

**Status:** Done; technisch gecontroleerd en handmatig geaccepteerd op 2026-06-19.

**Resultaat:** Owner-only tagpagina's voor locaties met bestaande QR-token, compacte
printweergave rond 5x5 cm, QR-rendering via een vervangbare application-interface met
concrete `QRCoder`-adapter en scanbare PNG-download via stream zijn opgeleverd. De
handmatige acceptatie bevestigde dat browserprint werkt, PNG-download een bestand met
de locatienaam oplevert en dat zowel de zichtbare QR als de gedownloade PNG dezelfde
locatie via de bestaande scanflow openen.

**Als** Owner<br>
**wil ik** de QR-code van een opslaglocatie kunnen printen en als PNG downloaden<br>
**zodat** ik fysieke labels kan maken en in de boot kan aanbrengen.

**Scope**

- Owner kan de QR-code van een locatie openen op een printvriendelijke tagpagina.
- Owner kan vanuit de browser een printactie starten voor de QR-tag.
- Owner kan per locatie een PNG-bestand van de QR-code downloaden.
- De tagpagina toont minimaal gebied, locatienaam en QR-code.
- De QR-code gebruikt de stabiele BootManager QR-token uit `PILOT-LOC-02`.

**Buiten scope**

- Server-side PDF- of CSV-export.
- Geavanceerde labelvellen, snijtekens, printerprofielen of labelprinterintegratie.
- QR-token vervangen of ongeldig maken.
- Tagoverzicht en tagstatus.
- Producten, voorraad en voorraadmutaties.

**Acceptatiecriteria**

- Owner kan voor een locatie een printvriendelijke QR-tagpagina openen.
- De QR-code op de tagpagina bevat de bestaande stabiele tokenwaarde.
- Browserprint vanaf de tagpagina is beschikbaar.
- Owner kan een PNG-bestand downloaden.
- Het gedownloade of geprinte QR-label opent via de scanflow dezelfde locatie.
- Crew kan QR-tags niet printen of exporteren.
- `dotnet build BootManager.sln` slaagt.

**Legacy-impact**

- `US1.12 Tag genereren voor opslaglocatie` wordt met deze story functioneel
  afgerond: printen en exporteren als afbeelding worden afgedekt nadat
  `PILOT-LOC-02` de token- en QR-waarde levert.
- Vervangen van tags en tagoverzicht blijven voor `PILOT-LOC-04`.

**Handmatige acceptatietest**

Log in als Owner, open een locatie met bestaande QR-token en open de tagpagina. Start
browserprint en download de PNG. Scan daarna de zichtbare QR-code of de gedownloade
PNG vanaf een tweede scherm en controleer dat dezelfde locatiepagina opent. Controleer
dat Crew deze print/exportactie niet kan uitvoeren.

**Technische richting**

- Gebruik de bestaande browserprintstijl als patroon; voeg geen server-side PDF-export
  toe.
- Houd QR-generatie achter een application-interface zodat de concrete library later
  vervangbaar blijft.
- Gebruik voor PNG-export een robuuste downloadroute via stream; vermijd kritieke
  browserafhankelijkheid van client-side canvas/blob-conversies.

**Implementatiestatus 2026-06-19**

- `StorageLocationDetails` toont voor Owner bij bestaande `QrValue` een actie naar een
  Owner-only tagpagina; Crew blijft de detailpagina lezen zonder print/exportactie.
- QR-tag rendering loopt via `IStorageLocationQrTagRenderer` in
  `BootManager.Application` met een concrete `QRCoder`-implementatie in
  `BootManager.Infrastructure`, zodat de library later vervangbaar blijft.
- De tagpagina toont gebied, locatienaam, compacte QR-tagweergave en de bestaande
  `QrValue`, gebruikt `window.print` voor browserprint en downloadt PNG via
  `DotNetStreamReference` en de bestaande `downloadFileFromStream` helper.
- `QRCoder` levert zowel SVG voor scherm/print als PNG-bytes voor robuuste download;
  browserafhankelijke canvas/blob-downloadlogica is uit de kritieke route verwijderd.
- Gerichte component- en autorisatietests bewijzen de Owner/Crew-zichtbaarheid, de
  renderer-abstraction, het stream-downloadpad en failure-paden zonder QR-rendering.

### PILOT-LOC-04 — QR-token vervangen en tagoverzicht

**Status:** Done; technisch gecontroleerd en handmatig geaccepteerd op 2026-06-19.

**Resultaat:** Owner kan bestaande locatie-QR-tokens vervangen waarbij het oude token
ongeldig wordt, een Owner-only tagoverzicht toont gebied, locatie, QR-waarde en
handmatige tagstatus, en de opslagfunctionaliteit is via een Owner-only hoofdmenu
`Opslag` direct bereikbaar met `Locaties` en `Tagoverzicht`. Na acceptatie is de
oude dubbele ingang via `Instellingen > Opslag` verwijderd, zodat opslagbeheer nog
maar op één plek in de navigatie zit.

**Als** Owner<br>
**wil ik** locatie-QR's kunnen vervangen en de tagstatus per locatie kunnen zien<br>
**zodat** beschadigde of verplaatste QR-labels aan boord beheersbaar blijven.

**Scope**

- Owner kan het QR-token van een locatie vervangen.
- Het oude token wordt ongeldig en opent daarna geen locatie meer.
- Owner ziet een tagoverzicht met gebied, locatie, huidig token en tagstatus.
- Owner kan de tagstatus handmatig bijwerken naar `Niet geprint`, `Geprint`,
  `Gekoppeld` of `Vervangen`.
- Het tagoverzicht helpt bepalen welke fysieke labels nog gemaakt, aangebracht of
  vervangen moeten worden.

**Buiten scope**

- Automatische detectie of een label fysiek geprint of aangebracht is.
- Printerintegratie, labelprinterprofielen of automatische statuswijziging na print.
- Auditlog van tokenvervangingen.
- Producten, voorraad en voorraadmutaties.

**Acceptatiecriteria**

- Owner kan per locatie een nieuw token genereren waarmee de oude QR ongeldig wordt.
- Een scan of handmatige invoer van het oude token opent de locatie niet meer.
- Een scan of handmatige invoer van het nieuwe token opent de locatie wel.
- Het tagoverzicht toont alle locaties met gebied, locatienaam en tagstatus.
- Owner kan de tagstatus handmatig aanpassen en terugzien.
- Crew kan het tagoverzicht en vervangactie niet beheren.
- `dotnet build BootManager.sln` slaagt.

**Legacy-impact**

- `US1.14 Tag opnieuw koppelen of vervangen` is functioneel afgedekt: oude token
  ongeldig, nieuw token actief.
- `US1.15 Overzicht van alle tags` is functioneel afgedekt met een overzicht van
  locaties, tokeninformatie en handmatige tagstatus.
- Fysieke printerintegratie en auditlog blijven buiten scope.

**Handmatige acceptatietest**

Log in als Owner, open het tagoverzicht en kies een locatie met bestaande QR. Zet de
status op `Geprint` en daarna op `Gekoppeld`. Vervang vervolgens het token. Controleer
dat de status zichtbaar is, dat het oude token niet meer naar de locatie opent en dat
het nieuwe token wel werkt. Log daarna in als Crew en controleer dat beheer van
vervangen en tagstatus niet toegankelijk is.

**Technische richting**

- Bouw voort op het tokenmodel van `PILOT-LOC-02` en de print/exportweergave van
  `PILOT-LOC-03`.
- Gebruik een expliciet handmatig statusveld; leid status niet automatisch af uit
  print- of downloadacties.
- Houd tokenvervanging beperkt tot het actief maken van een nieuw token en het
  ongeldig maken van het vorige token.

**Implementatiestatus 2026-06-19**

- `StorageLocation` ondersteunt nu expliciete tokenvervanging, maar alleen voor
  locaties die al een bestaand token hebben; een vervangactie kan dus geen eerste
  token genereren.
- Het oude token wordt na vervanging niet meer geresolved; het nieuwe token opent
  direct dezelfde locatie.
- Tagstatus wordt per locatie handmatig opgeslagen als `Niet geprint`, `Geprint`,
  `Gekoppeld` of `Vervangen`.
- `StorageLocationTagOverview` biedt een Owner-only overzicht en beheerflow voor
  tokenvervanging en tagstatus.
- De hoofdnavigatie bevat nu een Owner-only menu `Opslag` met `Locaties` en
  `Tagoverzicht`; `Locaties` hergebruikt het bestaande opslagbeheerscherm op een
  eigen route en de dubbele storage-sectie in `Settings` is verwijderd.
- Migratie- en upgradepadtests bewijzen dat de bestaande QR-tokenmigratie correct
  doorloopt naar tagstatusopslag met databehoud.
- Eindchecks: gerichte storage/navigation unit-tests 138/138, gerichte storage
  integration-tests groen, `dotnet build BootManager.sln --no-restore` groen,
  `git diff --check` groen; de bekende owner-recoverybaseline buiten deze story
  blijft bestaan in de volledige unit-suite.

### PILOT-AUTH-01 — Lokale Owner- en Crew-accounts

**Status:** Done op 2026-06-17; technisch gecontroleerd en handmatig geaccepteerd.

**Als** Owner<br>
**wil ik** Carla een eigen lokaal Crew-account geven<br>
**zodat** zij zelfstandig kan inloggen en BootManager kan gebruiken zonder toegang tot systeembeheer.

**Scope**

- De bestaande Owner wordt zonder gegevensverlies opgenomen in één uniform lokaal gebruikersmodel.
- Het model ondersteunt de rollen `Owner` en `Crew` en technisch meerdere Crew-accounts; de pilot maakt alleen Carla aan.
- De loginpagina toont actieve lokale accounts als naamkeuze, gevolgd door het eigen wachtwoord en `Ingelogd blijven`.
- De zichtbare accountnaam is hoofdletterongevoelig uniek en is tevens de lokale loginidentiteit.
- Owner kan in `Instellingen > Account > Lokale gebruikers`:
  - een Crew-account met tijdelijk wachtwoord aanmaken;
  - het wachtwoord van Crew resetten naar een tijdelijk wachtwoord;
  - een Crew-account uitschakelen en opnieuw activeren.
- Een nieuw of gereset Crew-account moet bij de eerstvolgende login via een gedeelde pagina `Mijn account` een eigen wachtwoord kiezen.
- Owner en Crew kunnen via `Mijn account` hun eigen wachtwoord wijzigen.
- Crew kan dashboard, scannen en het volledige huidige logboek gebruiken.
- Alleen Owner kan Instellingen, Beheerder, shutdown, systeeminstellingen en lokale gebruikers beheren.
- Navigatie toont alleen functies die bij de ingelogde rol horen.
- Uitschakelen of wachtwoordreset maakt bestaande cookies en tokens van die gebruiker direct ongeldig.
- De bestaande bootstrap-Owner en verplichte Owner-onboarding blijven werken.

**Buiten scope**

- Uitnodigingen, e-mailverificatie of een externe identity provider.
- Meer rollen dan `Owner` en `Crew`, een uitgebreide rechtenmatrix of rolwijziging.
- Een tweede Owner aanmaken.
- Lokale gebruikers definitief verwijderen.
- Meerdere boten of accountselectie per boot.
- Pincode-, recovery- of master-keyfunctionaliteit terugbrengen in de normale gebruikersflow.
- In deze story uitvoerende gebruikers vastleggen op bestaande logboekentiteiten.
- Voorraad- of logboekmutatiehistorie; latere `PILOT-INV-*`- en `PILOT-LOG-*`-stories gebruiken daarvoor de stabiele lokale gebruikers-id.

**Acceptatiecriteria**

- Een bestaande database migreert zonder verlies van Owner-id, wachtwoord, profielgegevens en onboardingstatus.
- Roelof kan na migratie met zijn bestaande wachtwoord als Owner inloggen.
- Een lege database maakt nog steeds één bootstrap-Owner en dwingt de bestaande Owner-onboarding af.
- De loginselector toont alleen actieve lokale accounts en toont geen wachtwoord- of profielgegevens.
- Accountnamen zijn hoofdletterongevoelig uniek.
- Owner kan Carla als Crew aanmaken met een tijdelijk wachtwoord.
- Carla wordt na de eerste login verplicht naar `Mijn account` geleid en kan pas na een geslaagde wachtwoordwijziging de overige Crew-routes gebruiken.
- Na de verplichte wijziging werkt alleen Carla's nieuwe wachtwoord.
- Carla kan dashboard, scanpagina en het huidige logboek gebruiken.
- Carla ziet geen links naar Instellingen of Beheerder en krijgt bij directe toegang tot Owner-routes geen toegang.
- Owner kan Carla's wachtwoord resetten; alle bestaande Carla-sessies en tokens worden dan ongeldig en een nieuwe wachtwoordwijziging wordt verplicht.
- Owner kan Carla uitschakelen; bestaande sessies en tokens worden ongeldig en nieuwe login wordt geweigerd.
- Opnieuw activeren herstelt login met het laatst geldige wachtwoord en de bestaande wachtwoordwijzigingsstatus.
- Owner kan zichzelf niet uitschakelen en kan geen tweede Owner of andere rol aanmaken.
- Cookie- en JWT-claims bevatten de werkelijke gebruikers-id, zichtbare naam en rol en zijn niet langer hardcoded als Owner.
- `dotnet build BootManager.sln` slaagt.

**Legacy-impact**

- `US1.3 Gebruikers aanmaken en rollen toewijzen` is gedeeltelijk afgedekt: Owner kan Crew aanmaken met een vaste rol.
- `US1.4 Inloggen als bestaande gebruiker` is afgedekt voor lokale Owner- en Crew-accounts.
- `US1.7` en `US8.4` blijven grotendeels geparkeerd: er komt geen algemene rolwijziging.
- `US1.8` blijft geparkeerd: uitschakelen vervangt voor de pilot definitief verwijderen.
- `US8.7` is gedeeltelijk afgedekt: toevoegen, wachtwoord resetten, uitschakelen en reactiveren, zonder verwijderen.

**Implementatiestatus 2026-06-17**

- Bestaande Owner-gegevens migreren naar het uniforme lokale gebruikersmodel met behoud van wachtwoord, profielgegevens en onboardingstatus.
- Owner kan lokale Crew aanmaken, wachtwoorden resetten, Crew uitschakelen en opnieuw activeren.
- Crew moet bij tijdelijk of gereset wachtwoord eerst via `Mijn account` een nieuw wachtwoord kiezen.
- Crew heeft toegang tot Dashboard, Scannen en Logboek; Instellingen en beheer blijven Owner-only.
- Wachtwoordreset en uitschakelen trekken bestaande cookies, tokens en open Blazor-sessies in via credentialversiecontrole.
- Handmatige acceptatie is lokaal uitgevoerd met Owner en Carla: onboarding, Crew-aanmaak, wachtwoordwijziging, autorisatie, reset, uitschakelen, reactiveren en Owner-eindcontrole zijn geslaagd.
- Eindchecks: unit-tests 210/211 met alleen de bekende owner-recoverybaseline rood; integratietests 12/12; `dotnet build BootManager.sln --no-restore`; `git diff --check`.

**Handmatige acceptatietest**

Upgrade eerst een kopie van de actuele Raspberry Pi-database. Controleer dat Roelof met
zijn bestaande wachtwoord als Owner kan inloggen en dat de bestaande onboardingstatus
behouden is. Maak daarna in Instellingen een Crew-account voor Carla met een tijdelijk
wachtwoord. Log uit, kies Carla op de loginpagina en controleer dat zij verplicht via
`Mijn account` een ander wachtwoord moet instellen. Controleer daarna dashboard, scan en
logboek en probeer de directe URL's van Instellingen en Beheerder.

Log Carla vervolgens gelijktijdig in twee browsers in. Reset als Owner haar wachtwoord
en controleer dat beide bestaande sessies vervallen, het tijdelijke wachtwoord werkt en
opnieuw een wijziging wordt verplicht. Herhaal de sessiecontrole met uitschakelen,
controleer dat nieuwe login wordt geweigerd en activeer het account opnieuw. Sluit af
met een controle dat Roelofs Owner-routes, bootstrapflow en onboarding intact zijn.

**Technische richting**

- Gebruik één lokale gebruiker-entiteit met stabiele `Guid`-id, zichtbare en genormaliseerde accountnaam, rol, wachtwoordhash, actieve status, setupstatus en credentialversie.
- Migreer het bestaande Owner-record naar dit uniforme model; kopieer geen Owner naar een los Crew- of accountmodel.
- Gebruik de credentialversie samen met actieve status bij cookie- en JWT-validatie, zodat reset en uitschakelen direct alle oude authenticatiebewijzen intrekken.
- Houd `Ingelogd blijven` en de bestaande niet-persistente sessieopslag intact.
- Laat de verplichte Owner-onboarding en de verplichte Crew-wachtwoordwijziging als afzonderlijke gates werken.
- Laat een geslaagde eigen wachtwoordwijziging de huidige browser opnieuw aanmelden met de nieuwe credentialversie; andere sessies blijven ingetrokken.
- Sla de accountnaam leesbaar op omdat deze bewust op de anonieme lokale loginselector wordt getoond. Bestaande versleutelde Owner-profielgegevens blijven behouden.

### PILOT-SCAN-01 — Camera-, QR- en barcode-proof-of-concept

**Status:** Done

**Als** gebruiker  
**wil ik** in de lokaal gehoste Blazor-app op mijn telefoon een QR-code en productbarcode kunnen scannen  
**zodat** vroeg duidelijk is of de beoogde scanflows technisch en praktisch uitvoerbaar zijn.

**Scope**

- Proof-of-concept binnen de bestaande .NET 8/Blazor-oplossing.
- Test op de Samsung-telefoons van Roelof en Carla; het oudste toestel draait Android 16.
- Validatie in zowel Microsoft Edge als Google Chrome op Android.
- Achtercamera als voorkeurscamera.
- QR Code en de lineaire formaten EAN-13, EAN-8, UPC-A en Code 128 herkennen.
- Herkende ruwe codewaarde en het herkende formaat zichtbaar tonen.
- Na een herkenning stoppen met doorlopend detecteren, zodat dezelfde code niet herhaald wordt verwerkt.
- Duidelijke statussen voor niet gestart, toestemming aanvragen, actief scannen, herkend, handmatig ingevoerd en gestopt.
- Begrijpelijke fouten voor ontbrekende HTTPS/secure context, geweigerde toestemming, ontbrekende camera en decoder- of camerafouten.
- Scannen expliciet kunnen starten, stoppen en opnieuw starten.
- Handmatige code-invoer als fallback via hetzelfde zichtbare resultaatgebied.
- De bestaande route `http://bootmanager-pi:5000/` blijft beschikbaar voor gebruik zonder camera.
- Een aanvullende HTTPS-route voor cameragebruik vaststellen en de lokale certificaat-, browser- en netwerkvoorwaarden documenteren.

**Buiten scope**

- Nog geen opslaglocatie-entiteiten.
- Nog geen productentiteiten.
- Nog geen databaseopslag.
- Nog geen QR-generatie.
- Nog geen voorraadmutaties.
- Nog geen definitieve styling.
- Geen externe EAN-productdatabase of interpretatie van de gescande waarde.
- Geen automatische navigatie of functionele actie op basis van de gescande waarde.
- Geen brede wijziging van de bestaande Docker- of netwerkarchitectuur buiten wat voor de secure context aantoonbaar nodig is.

**Acceptatiecriteria**

- De beveiligde scanpagina opent op beide telefoons in Edge en Chrome.
- De scanpagina meldt op de bestaande HTTP-route begrijpelijk dat cameragebruik HTTPS vereist.
- Cameratoestemming kan via de HTTPS-route worden verleend of geeft een begrijpelijke fout.
- De achtercamera wordt bij voorkeur gebruikt.
- Een BootManager-test-QR wordt op beide telefoons herkend.
- Minimaal één echte EAN-13-productbarcode wordt op beide telefoons herkend.
- De gedecodeerde ruwe waarde en het herkende formaat worden zichtbaar getoond.
- Scannen kan worden gestopt en opnieuw gestart.
- Handmatige invoer werkt als fallback.
- Stoppen of verlaten van de pagina beëindigt de actieve camerastream.
- De noodzakelijke HTTPS-, certificaat-, hostname-, browser- en netwerkvoorwaarden zijn gedocumenteerd.
- `dotnet build BootManager.sln` slaagt.

**Handmatige acceptatietest**

Test eerst via `http://bootmanager-pi:5000/` dat de scanpagina de secure-contextbeperking begrijpelijk meldt. Open daarna dezelfde pagina via de aanvullende HTTPS-route op beide telefoons, in Edge en Chrome. Verleen cameratoestemming, controleer dat bij voorkeur de achtercamera actief is en scan achtereenvolgens een BootManager-test-QR en een echte EAN-13-productbarcode. Controleer ruwe waarde en formaat, stop en herstart de camera, verlaat de pagina en controleer dat de camera stopt. Weiger daarnaast eenmaal cameratoestemming en controleer de foutmelding. Sluit af met handmatige invoer via de fallback.

**Technische richting**

- Gebruik een lokaal meegeleverde browserdecoder met ondersteuning voor QR en meerdere lineaire formaten; de pilot mag niet afhankelijk zijn van internet of een CDN.
- Beheer camerastream en decoder in een afzonderlijke JavaScriptmodule die via bestaande Blazor-JavaScript-interop wordt aangeroepen.
- Gebruik de native `BarcodeDetector` API niet als enige decoder, omdat de pilot in zowel Edge als Chrome en op beide toestellen moet werken.
- Vraag video aan met voorkeur voor `facingMode: environment`, maar geef een begrijpelijke fout of bruikbare fallback wanneer het toestel die voorkeur niet exact kan leveren.
- Houd de bestaande interne webcontainer en HTTP-poort `5000` intact. HTTPS-terminatie is een aanvullende operationele ingang en geen wijziging van interne servicecommunicatie.

**Implementatiestatus 2026-06-09**

- De beveiligde `/scan`-pagina, camerastatussen, start/stop/herstart, resultaatweergave en handmatige fallback zijn via PR #88 gemerged naar `master`.
- De productiepagina gebruikt één gedeelde camerastream: lokale ZXing uitsluitend voor QR en native `BarcodeDetector` uitsluitend voor EAN-13. Browsers zonder native EAN-13-ondersteuning behouden QR en handmatige invoer.
- Laptopacceptatie is geslaagd via `http://localhost:5046/scan` en `https://localhost:7299/scan`, inclusief QR-code, productbarcode, stoppen/herstarten, handmatige invoer tijdens actief scannen en cameravrijgave bij navigatie.
- De bestaande ZXing-proef leest QR op de Samsung-telefoon, maar de geteste EAN-13-productbarcodes niet betrouwbaar. Daarom is een geïsoleerde, beveiligde `/scan-quagga-test`-pagina toegevoegd met lokaal meegeleverde Quagga2 1.12.1, uitsluitend voor EAN-13.
- De Quagga2-proef gebruikt een ideale camerastream van 1920×1080 en maakt de Quagga2-verwerkingsgrootte op de geïsoleerde testpagina vergelijkbaar met 800, 1280 (standaard) en 1600 px. `patchSize: large`, `halfSample: false`, locator en alleen `ean_reader` blijven actief. Geldige resultaten worden aanvullend gecontroleerd op 13 cijfers en een correct EAN-13-controlecijfer.
- Omdat Quagga2 de kleine EAN-13 niet betrouwbaar decodeerde, is daarnaast een geïsoleerde `/scan-native-barcode-test` toegevoegd. Native `BarcodeDetector` herkende EAN-13 `9789059965607` op de Samsung-telefoon direct vanaf circa 15 cm; die bewezen route is vervolgens in `/scan` geïntegreerd.
- Start, stop, herstart, camerawissel, resultaatstop en component-disposal zijn beschermd tegen achterhaalde callbacks, oude streams en overlappende sessies.
- De deterministische moduleharness laadt de echte productie-exportfuncties en bewijst zes scenario's: fallback zonder native support, geldige EAN-13-detectie, vroege QR-callback, sessie-isolatie, oplopende supportrevisions en idempotente cleanup bij pending native detecties.
- JavaScript-syntaxcontrole, de moduleharness (6/6), `dotnet build BootManager.sln --no-restore` en `git diff --check` slagen.
- Publishcontrole slaagt met bestaande waarschuwingen buiten deze story. De simulator-tests slagen 5/5; van de unit-tests slagen 147/148. Alleen de bestaande, ongerelateerde `OwnerRecoveryServiceTests.RestoreWithBackupCode_Succeeds_WhenCorrect` blijft rood.
- `master` is op de Raspberry Pi uitgerold. Op de Samsung-telefoons van Roelof en Carla is de geïntegreerde scanflow via HTTPS in Edge en Chrome geslaagd, inclusief QR-codes en verschillende productbarcodes.
- De webapp en login zijn via HTTP gecontroleerd. Op Roelofs telefoon moest eenmaal een oude browsercookie worden gewist. Op Carla's telefoon is de Caddy-root-CA geïnstalleerd en werkte de HTTPS-route zonder afwijkingen.
- Alle acceptatiecriteria van `PILOT-SCAN-01` zijn op 2026-06-09 behaald; de story is afgerond.
