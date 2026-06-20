# BootManager Holiday Pilot 2026

**Status:** actief en leidend voor de eerstvolgende ontwikkelperiode  
**Doelomgeving:** Linde, Raspberry Pi, lokaal/offline-first  
**Gebruikers:** Roelof en Carla  
**Pilotduur:** drie weken tijdens de zomervakantie 2026

## Doel

BootManager moet vóór vertrek geschikt zijn voor drie weken dagelijks praktisch gebruik door Roelof en Carla op dezelfde boot, dezelfde Raspberry Pi en dezelfde database.

De pilot valideert niet de volledige productvisie. De pilot moet vooral aantonen dat twee kernproposities in de praktijk werken:

1. een bruikbaar digitaal logboek op basis van echte boorddata en snelle handmatige momenten;
2. weten wat aan boord is en waar het ligt, ondersteund door locatie-QR-codes en productbarcodes.

Bestaande Raspberry Pi-, NMEA-, dashboard- en logboekfunctionaliteit blijft het technische fundament. Nieuwe NMEA-uitbreidingen hebben geen prioriteit tenzij een blocker op Linde wordt aangetoond.

## Leidende productprincipes

- Gebruiksgemak en directe begrijpelijkheid gaan voor technische volledigheid.
- Roelof en Carla moeten de kernflows zelfstandig kunnen uitvoeren.
- De applicatie blijft lokaal en offline-first werken.
- QR identificeert een opslaglocatie.
- Barcode identificeert een product dat BootManager al kent.
- Een voorraadregel koppelt product, locatie en hoeveelheid.
- Hetzelfde product kan op meerdere locaties tegelijk liggen.
- Scannen is de voorkeursroute; handmatige keuze blijft alleen als fallback.
- Iedere story blijft klein, testbaar en verticaal.

## MVP-scope

### 1. Platform en betrouwbaarheid

- Raspberry Pi start BootManager zelfstandig.
- Webapp is bruikbaar op telefoon, tablet en laptop.
- Bestaande NMEA 0183 UDP-keten op Linde blijft werken.
- Live dashboard toont actuele boordwaarden en data-actualiteit.
- Database en bijlagen blijven behouden na herstart.
- Back-up kan worden gemaakt en herstel wordt minimaal eenmaal getest.
- Minimaal 72 uur duurtest vóór release-freeze.

### 2. Lokale gebruikers

- Roelof behoudt de Owner-rol.
- Carla krijgt een eigen lokale login met Crew-rol.
- Owner en Crew gebruiken dezelfde boot en database.
- Crew kan dashboard, logboek en voorraad gebruiken.
- Alleen Owner beheert systeeminstellingen en lokale gebruikers.
- Handmatige logboekacties en voorraadmutaties leggen de uitvoerende gebruiker vast.
- Geen uitgebreide rollenmatrix, uitnodigingsflow of externe identity provider.

### 3. Logboek

- Reis starten, bekijken en afsluiten.
- Bestaande automatische conceptmomenten blijven beschikbaar.
- Duidelijke knop `Moment vastleggen` op een logische plek in de actieve-reisflow.
- Bij indrukken wordt een nieuwe concept-logregel gemaakt met de actuele beschikbare NMEA-waarden als snapshot.
- Gebruiker kan een gebeurtenis kiezen, minimaal:
  - overstag;
  - gijp;
  - zeil gewijzigd;
  - motor gestart;
  - motor gestopt;
  - vertrek;
  - aankomst;
  - voor anker;
  - bijzonder moment;
  - alleen momentopname.
- Gebruiker kan een weerconditie kiezen via grote pictogrammen, minimaal:
  - zonnig;
  - licht bewolkt;
  - half bewolkt;
  - bewolkt;
  - buien;
  - regen;
  - onweer;
  - mist;
  - veel wind.
- Gebruiker kan een korte vrije notitie toevoegen.
- Nieuwe regel wordt als Draft opgeslagen en kan later worden geaccordeerd.
- Weerconditie wordt als stabiele domeinwaarde opgeslagen; icoon en label zijn presentatie.

### 4. Opslaggebieden en locaties

- Gebruiker kan opslaggebieden vastleggen, bijvoorbeeld kombuis, salon, voorhut, bakskist en techniek.
- Iedere opslaglocatie hoort bij precies één gebied.
- Locatie heeft minimaal naam en optionele beschrijving.
- Iedere locatie kan aan een unieke BootManager QR-token worden gekoppeld.
- QR-code blijft geldig wanneer de locatie later wordt hernoemd.
- Scannen van een bekende locatie-QR opent direct de locatiepagina.
- Een onbekende BootManager-QR kan aan een nieuwe of bestaande locatie worden gekoppeld.
- Handmatige locatiekeuze blijft beschikbaar als fallback.

### 5. Producten, barcodes en voorraad

- Product heeft minimaal naam, categorie, standaard eenheid en optionele omschrijving.
- Bij productaanmaak vult de gebruiker de productgegevens zelf in.
- Bij productaanmaak kan de productbarcode worden gescand en lokaal aan het product worden gekoppeld.
- Er wordt geen externe EAN-database of automatische productherkenning gebruikt.
- Later scannen van dezelfde productcode helpt het bekende product direct terug te
  vinden of opnieuw in te ruimen.
- Onbekende barcode biedt:
  - nieuw product aanmaken;
  - aan bestaand product koppelen;
  - annuleren.
- Product kan op meerdere opslaglocaties tegelijk voorraad hebben.
- Voorraadregel bevat minimaal product, locatie en hoeveelheid.
- Voorraad kan worden verhoogd, verlaagd en gecorrigeerd.
- Als een product maar op één locatie ligt, opent scannen direct de voorraadregel.
- Als een product op meerdere locaties ligt, toont BootManager alle locaties en hoeveelheden.
- Eenvoudige mutatiehistorie bevat gebruiker, datum/tijd, product, locatie, oude hoeveelheid, nieuwe hoeveelheid en reden.

### 6. Verplichte locatie-scan bij de hoofdflow

Voorkeursflow bij productaanmaak:

1. gebruiker vult productgegevens in;
2. gebruiker scant productbarcode;
3. gebruiker scant de QR-code van de opslaglocatie;
4. BootManager toont gebied en locatie ter controle;
5. gebruiker vult hoeveelheid in;
6. gebruiker slaat op.

Als productaanmaak vanaf een reeds gescande locatiepagina wordt gestart, is die locatie automatisch ingevuld en hoeft niet opnieuw te worden gescand.

Voor extra voorraad van hetzelfde product op een andere plek:

1. scan productbarcode;
2. kies `Voorraad op andere locatie toevoegen`;
3. scan de nieuwe locatie-QR;
4. vul hoeveelheid in;
5. sla op.

## Prioriteitsvolgorde

1. **PILOT-SCAN-01** — **Done** — Camera-, QR- en barcode-proof-of-concept op de telefoons.
2. **PILOT-AUTH-01** — **Done** — Owner/Crew-model en eigen login voor Carla.
3. **PILOT-LOC-01** — **Done** — Opslaggebieden en opslaglocaties.
4. **PILOT-LOC-02** — **Done** — QR-token genereren, koppelen en locatie openen.
5. **PILOT-LOC-03** — **Done** — QR-tag printen en PNG exporteren.
6. **PILOT-LOC-04** — **Done** — QR-token vervangen, tagoverzicht en opslagnavigatie.
7. **PILOT-INV-01** — **Done** — Productcategorieën, producten en productbarcodes.
8. **PILOT-INV-02** — **Done** — Taakgerichte voorraadbasis: product en locatie koppelen, hoeveelheid vastleggen en voorraad handmatig tonen/beheren.
9. **PILOT-INV-03** — **Gepland** — Scan-gestuurde inruimflow met locatievoorstel en handmatige fallback.
10. **PILOT-INV-04** — **Gepland** — Product terugvinden via scan of zoeken en locaties tonen.
11. **PILOT-INV-05** — **Gepland** — Verbruik, correcties en eenvoudige historie.
12. **PILOT-LOG-01** — **Gepland** — Handmatig logboekmoment met actuele NMEA-snapshot.
13. **PILOT-LOG-02** — **Gepland** — Gebeurteniskeuze, weericonen en notitie.
14. **PILOT-E2E-01** — **Gepland** — End-to-end gebruikstest door Roelof en Carla.
15. **PILOT-OPS-01** — **Gepland** — Duur-, herstart-, opslag- en back-uptest.
16. **PILOT-REL-01** — **Gepland** — Release-freeze en uitsluitend blockerfixes.

Codex kiest geen story buiten deze volgorde, tenzij:

- een blocker eerst opgelost moet worden;
- een afhankelijkheid aantoonbaar ontbreekt;
- de gebruiker expliciet een andere prioriteit vaststelt.

**Eerstvolgende story:** `PILOT-INV-03` — Scan-gestuurde inruimflow met locatievoorstel.

## Story-uitwerking en archief

Dit release-document blijft compact voor actuele pilotsturing. Volledig uitgewerkte
afgeronde stories staan in `.docs/releases/holiday-pilot-2026-archive-completed-stories.md`.

### Actieve werkset

- Houd als actieve inventory-uitwerkingen nu `PILOT-INV-03`, `PILOT-INV-04` en
  `PILOT-INV-05` aan; voeg daarna alleen de eerstvolgende geplande stories toe
  wanneer ze werkelijk aan de beurt zijn.
- Houd in dit document alleen de actuele releasekaders, prioriteitsvolgorde,
  eerstvolgende story en de actieve of direct geplande uitgewerkte stories.
- Verplaats een story na afronding en administratieve controle naar het archief,
  zodat de dagelijkse context klein blijft maar de historie beschikbaar blijft.
- Raadpleeg het archief alleen wanneer historische scope, acceptatie,
  implementatiestatus of legacy-impact opnieuw relevant is.

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

### PILOT-INV-04 — Product terugvinden via scan of zoeken

**Storyzin**  
Als Owner of Crew wil ik een product snel kunnen terugvinden via scannen of handmatig
zoeken, zodat ik direct zie op welke locatie of locaties het product ligt en daar
desgewenst naartoe kan navigeren.

**Waarom deze slice nu**  
Na de catalogusbasis van `PILOT-INV-01`, de locatiegebonden voorraadbasis van
`PILOT-INV-02` en de scan-gestuurde inruimflow van `PILOT-INV-03` is de volgende
praktische vraag: waar ligt iets? Deze story maakt het terugvinden van producten snel via
de voorkeursroute scannen en via een handmatige zoekfallback, zonder al voorraadmutaties
of dashboardzoekingangen te introduceren.

**Scope**

- De primaire route start vanuit het bestaande menu `Scannen`.
- Als in `Scannen` een bekende productcode wordt gescand, start direct de
  terugvindflow.
- Handmatige fallback is beschikbaar via `Voorraadbeheer > Producten`.
- Handmatig zoeken werkt op productnaam en productomschrijving.
- Handmatig zoeken is hoofdletterongevoelig en ondersteunt deelmatches.
- Als handmatig zoeken meerdere producten vindt, toont BootManager een korte
  productresultatenlijst waaruit de gebruiker een product kiest.
- Die resultatenlijst toont per product minimaal:
  - productnaam;
  - de eerste tekens van de omschrijving als die bestaat;
  - de bekende locaties van dat product als komma-gescheiden samenvatting.
- Hoeveelheden worden nog niet in die eerste resultatenlijst getoond.
- Als een gescand of gekozen product precies een actieve voorraadlocatie heeft, opent
  BootManager direct de locatiepagina van die locatie.
- Als een gescand of gekozen product meerdere actieve voorraadlocaties heeft, toont
  BootManager direct een lijst met die locaties.
- Die lijst toont minimaal gebied, locatienaam, hoeveelheid en eenheid per locatie.
- Vanuit die lijst kan de gebruiker doorklikken naar de locatiepagina van een gekozen
  locatie.
- Als een product bekend is maar momenteel geen actieve voorraadlocaties heeft, meldt
  BootManager dat duidelijk.
- Als voor dat product nog een laatst gebruikte locatie bekend is, toont BootManager
  die echte locatieverwijzing als verwachte plek waar het product normaal hoort te
  liggen, weergegeven als leesbare gebied- en locatienaam.
- In beide gevallen biedt BootManager een vervolgstap `Voorraad toevoegen`.

**Buiten scope**

- Dashboard-zoekbalk of andere nieuwe dashboardingangen.
- Verbruik, correcties, voorraadhistorie of andere voorraadmutaties vanuit de
  terugvindflow.
- Uitgebreide filters op categorie, gebied of andere velden.
- Echte typo-correctie, fuzzy matching of synoniembeheer.
- Hoeveelheden tonen in de eerste productresultatenlijst van handmatig zoeken.

**Acceptatiecriteria**

1. De gebruiker kan vanuit `Scannen` een bekende productcode scannen en direct de
   terugvindflow starten.
2. De gebruiker kan ook handmatig zoeken via `Voorraadbeheer > Producten`.
3. Handmatig zoeken doorzoekt productnaam en omschrijving, is hoofdletterongevoelig en
   ondersteunt deelmatches.
4. Als handmatig zoeken meerdere producten vindt, toont BootManager een korte lijst met
   productnaam, eerste omschrijvingstekens en locatiesamenvatting, waarna de gebruiker
   een product kiest.
5. Als een gescand of gekozen product precies een actieve voorraadlocatie heeft, opent
   direct de locatiepagina van die locatie.
6. Als een gescand of gekozen product meerdere actieve voorraadlocaties heeft, toont
   BootManager direct een lijst met gebied, locatienaam, hoeveelheid en eenheid per
   locatie.
7. Vanuit die locatielijst kan de gebruiker doorklikken naar een locatiepagina.
8. Als een product bekend is maar geen actieve voorraadlocaties heeft, meldt
   BootManager dat duidelijk.
9. Als voor dat product nog een laatst gebruikte locatie bekend is, toont BootManager
   die locatie als verwachte plek waar het product normaal hoort te liggen.
10. In beide gevallen biedt BootManager een actie `Voorraad toevoegen`.

**Legacy-impact**

- Dekt primair `US2.6` barcode scannen bij zoeken en de product-terugvindkant van
  `US2.9` voorraad bekijken per locatie.
- Bouwt voort op de gekoppelde codes uit `PILOT-INV-01` en de voorraadregels per
  locatie uit `PILOT-INV-02`.
- Laat `US2.10` voorraad aanpassen, `US2.12` breder zoeken en filteren, `US2.13`
  voorraadlogboek en `US2.20` verbruik via barcode bewust open voor latere
  inventory-slices.

**Handmatige acceptatietest**

1. Open `Scannen` en scan een bekende productcode van een product dat op precies een
   locatie ligt; controleer dat direct de juiste locatiepagina opent.
2. Scan een bekende productcode van een product dat op meerdere locaties ligt;
   controleer dat direct een locatielijst opent met gebied, locatienaam, hoeveelheid en
   eenheid.
3. Klik vanuit die lijst door naar een locatiepagina en controleer dat de juiste
   locatie wordt geopend.
4. Open `Voorraadbeheer > Producten` en zoek handmatig op een productnaam met
   hoofdletterverschil, bijvoorbeeld `rijst` versus `Rijst`; controleer dat het product
   gevonden wordt.
5. Zoek handmatig op tekst die alleen in de omschrijving voorkomt; controleer dat het
   product gevonden wordt.
6. Controleer dat meerdere zoekresultaten eerst een korte productlijst tonen met
   productnaam, omschrijvingstekst en locatiesamenvatting, zonder hoeveelheden.
7. Kies een product uit die lijst en controleer dat het vervolggedrag gelijk is aan de
   scanroute: direct locatiepagina bij een locatie, of locatielijst bij meerdere
   locaties.
8. Open een bekend product zonder actieve voorraadlocaties en controleer dat
   BootManager meldt dat het momenteel niet op voorraad is.
9. Controleer dat, als voor dit product nog een laatst gebruikte locatie bekend is,
   BootManager die als verwachte plek toont.
10. Controleer dat in beide gevallen een actie `Voorraad toevoegen` beschikbaar is.

### PILOT-INV-05 — Voorraad muteren en eenvoudige historie

**Storyzin**  
Als Owner of Crew wil ik voorraadverbruik, tellingen en correcties kunnen verwerken en
later in een eenvoudig logboek kunnen terugzien, zodat de werkelijke voorraad actueel
blijft zonder de context van product en locatie te verliezen.

**Waarom deze slice nu**  
Na catalogus, voorraadbasis, inruimen en terugvinden ontbreekt nog het dagelijks
bijhouden van voorraad wanneer producten gebruikt worden of aantallen niet meer kloppen.
Deze story voegt daarom zowel een fysieke verbruikflow op locatie als een
administratieve fallback toe, plus een eenvoudige historie voor controle achteraf.

**Scope**

- Deze story ondersteunt drie expliciete mutatietypes:
  - `Verbruik`
  - `Correctie`
  - `Telling`
- `Verbruik` verlaagt voorraad altijd op een expliciete product-locatieregel.
- De fysieke hoofdflow is:
  - product terugvinden;
  - naar de locatie gaan;
  - locatie scannen;
  - product scannen;
  - verbruikte hoeveelheid invoeren;
  - opslaan;
  - terugkeren naar het begin van de terugvind/verbruikflow.
- De administratieve fallback werkt zonder scannen.
- In die fallback kiest de gebruiker eerst een product en daarna een locatie.
- Als dat product maar op een actieve locatie ligt, kiest BootManager die locatie
  automatisch.
- Bij `Verbruik` voert de gebruiker de afname in.
- Bij `Telling` voert de gebruiker de feitelijk aanwezige nieuwe hoeveelheid in.
- Bij `Correctie` voert de gebruiker ook de feitelijk nieuwe hoeveelheid in.
- De gebruiker kan bij iedere mutatie een hele vrije optionele notitie toevoegen.
- Verbruik dat meer afneemt dan de actuele voorraad op die locatie wordt geblokkeerd.
- Als een mutatie de actieve voorraad van een product op een locatie op `0` brengt,
  verdwijnt de actieve voorraadregel van die locatie.
- De laatst gebruikte locatie van het product blijft daarbij als echte
  locatieverwijzing bewaard als verwachte locatie, zodat BootManager later nog kan tonen
  waar het product normaal hoort te liggen.
- Een aparte historiepagina toont alle voorraadmutaties.
- Die historiepagina toont standaard alle mutaties, nieuwste eerst.
- Een historieregel toont minimaal:
  - datum/tijd;
  - mutatietype;
  - productnaam;
  - gebied en locatienaam;
  - oude hoeveelheid;
  - nieuwe hoeveelheid;
  - gebruiker;
  - optionele notitie.

**Buiten scope**

- Negatieve voorraad.
- Mutaties zonder expliciete locatie.
- Inline historie op product- of locatiepagina's.
- Geavanceerde filters, export of rapportage op de historiepagina.
- Dashboardintegratie voor voorraadmutaties.
- Automatische verbruiksafleiding zonder expliciete gebruikersactie.

**Acceptatiecriteria**

1. Owner en Crew kunnen voorraadmutaties uitvoeren als `Verbruik`, `Correctie` of
   `Telling`.
2. De fysieke verbruikflow ondersteunt: product terugvinden, naar de locatie gaan,
   locatie scannen, product scannen, afname invoeren en opslaan.
3. Na afronding van die fysieke verbruikflow keert de gebruiker terug naar het begin van
   die terugvind/verbruikroute.
4. De administratieve fallback ondersteunt muteren zonder scannen door eerst een product
   en daarna een locatie te kiezen.
5. Als een product in die fallback maar op een actieve locatie ligt, kiest BootManager
   die locatie automatisch.
6. `Verbruik` vraagt om een afnamehoeveelheid; `Telling` en `Correctie` vragen om de
   nieuwe feitelijke hoeveelheid.
7. Een optionele vrije notitie kan bij iedere mutatie worden opgeslagen.
8. Verbruik boven de actuele voorraad op die locatie wordt duidelijk geblokkeerd.
9. Als een mutatie de actieve voorraad op `0` brengt, verdwijnt de actieve voorraadregel
   maar blijft de laatst gebruikte locatie van het product als verwachte locatie
   bewaard.
10. De aparte historiepagina toont alle mutaties standaard nieuwste eerst met minimaal
    datum/tijd, type, product, gebied + locatie, oude hoeveelheid, nieuwe hoeveelheid,
    gebruiker en optionele notitie.

**Legacy-impact**

- Dekt primair `US2.10` voorraad aanpassen en `US2.13` voorraadlogboek.
- Dekt ook de verbruikskant van `US2.20` voorraad verminderen via barcode, maar binnen
  de pilot nog in combinatie met expliciete locatiecontext en zonder bredere
  automatisering.
- Bouwt voort op `PILOT-INV-02` voor product-locatieregels en op `PILOT-INV-04` voor
  het terugvinden van producten voordat verbruik wordt geboekt.
- Laat geavanceerde filters, analyses en dashboardsignalering bewust open voor latere
  inventory-slices.

**Handmatige acceptatietest**

1. Zoek een product via de terugvindflow, ga naar de juiste locatie, scan daar de
   locatiecode en daarna de productcode.
2. Kies `Verbruik`, voer een afname in en sla op.
3. Controleer dat de voorraad op die locatie is verlaagd en dat de flow terugkeert naar
   het begin van de terugvind/verbruikroute.
4. Herhaal via de administratieve fallback zonder scannen: kies eerst een product en
   daarna een locatie, of laat de locatie automatisch kiezen als er maar een actief is.
5. Voer een `Telling` uit en controleer dat de nieuwe feitelijke hoeveelheid direct wordt
   opgeslagen.
6. Voer een `Correctie` uit met een andere nieuwe hoeveelheid en controleer dat ook deze
   wordt opgeslagen.
7. Voeg bij minstens een mutatie een vrije notitie toe en controleer dat die later in de
   historie zichtbaar is.
8. Probeer meer te verbruiken dan op de gekozen locatie aanwezig is; verwacht een
   duidelijke blokkade.
9. Verbruik een voorraadregel precies naar `0` en controleer dat de actieve regel
   verdwijnt, maar dat het product later nog zijn laatst gebruikte locatie als
   verwachte plek behoudt voor terugvinden of opnieuw inruimen.
10. Open de aparte historiepagina en controleer dat alle mutaties nieuwste eerst worden
    getoond met datum/tijd, type, product, gebied + locatie, oude hoeveelheid, nieuwe
    hoeveelheid, gebruiker en eventuele notitie.

### Afgeronde stories in archief

- `PILOT-SCAN-01` — scan proof-of-concept, handmatig geaccepteerd op 2026-06-09.
- `PILOT-AUTH-01` — lokale Owner/Crew-accounts, handmatig geaccepteerd op 2026-06-17.
- `PILOT-LOC-01` — opslaggebieden en opslaglocaties, geaccepteerd op 2026-06-18.
- `PILOT-LOC-02` — locatie-QR genereren, koppelen en openen, geaccepteerd op 2026-06-19.
- `PILOT-LOC-03` — QR-tag printen en PNG exporteren, geaccepteerd op 2026-06-19.
- `PILOT-LOC-04` — tokenvervanging, tagoverzicht en opslagnavigatie, geaccepteerd op 2026-06-19.
- `PILOT-INV-01` — productcategorieën, producten en gekoppelde codes, geaccepteerd op 2026-06-20.
- `PILOT-INV-02` — taakgerichte voorraadbasis per locatie, geaccepteerd op 2026-06-20.

## Niet-doelen voor deze pilot

- Nieuwe NMEA-sentence-types, tenzij noodzakelijk voor een blocker op Linde.
- Routekaart.
- Passageplanning.
- Algemene documentmodule.
- Onderhoudsmodule.
- Connect/cloud.
- NFC.
- Externe EAN-productdatabase.
- Automatische productinformatie, foto of AI-herkenning.
- Voedingsinformatie.
- Houdbaarheidsdatums.
- Minimumvoorraad en automatische bestellijsten.
- Geavanceerde rollenmatrix.
- Server-side PDF/CSV-export.
- Uitgebreide statistieken.
- Algemene UI-frameworkmodernisering.
- Grote architectuurrefactors zonder directe pilotnoodzaak.

## Go/no-go voor installatie en vakantiegebruik

- BootManager draait minimaal 72 uur stabiel met echte of representatieve datastroom.
- Herstart en gesimuleerde stroomonderbreking verliezen geen functionele data.
- Web- en ingestservices starten automatisch opnieuw.
- Dashboard toont actuele boorddata.
- Roelof en Carla kunnen zelfstandig inloggen.
- Volledige logboekflow werkt.
- `Moment vastleggen` werkt met NMEA-snapshot, gebeurtenis, weer en notitie.
- Locatie-QR werkt op beide telefoons.
- Productbarcode werkt op beide telefoons.
- Product kan op meerdere gescande locaties liggen.
- Voorraad kan door beide gebruikers worden aangepast.
- Back-up is gemaakt en herstel is minimaal eenmaal getest.
- Geen bekende release-blocking fouten.

## Pilotvragen

Tijdens de drie weken worden minimaal deze vragen beantwoord:

- Gebruiken Roelof en Carla ieder hun eigen account?
- Kan Carla zonder technische hulp een moment vastleggen?
- Welke gebeurtenissen worden werkelijk gebruikt?
- Zijn weericonen sneller en duidelijker dan vrije tekst?
- Wordt eten- en drinkenvoorraad werkelijk bijgehouden?
- Blijft de voorraad na een week betrouwbaar?
- Is bij verbruiksvoorraad hoeveelheid belangrijker dan locatie?
- Is bij onderdelen locatie belangrijker dan hoeveelheid?
- Is scannen sneller dan navigeren en zoeken?
- Waar moeten QR-codes fysiek worden geplaatst?
- Werkt scannen in donkere of krappe kastjes?
- Wordt barcode vooral gebruikt bij inruimen, verbruik of beide?
- Welke invoer voelt als onnodige administratie?
- Welke flows vragen nog uitleg of veroorzaken fouten?

## Documentatie- en legacyregel

Bij voorbereiding én afronding van iedere pilotstory controleert Codex gericht:

1. dit release-document, inclusief status in de prioriteitsvolgorde;
2. `README.md`, inclusief pilotvoortgang en eerstvolgende story;
3. de relevante actuele epic of userstory;
4. `.docs/TODO.md`;
5. `.docs/legacy-analysis/legacy-coverage-register.md`;
6. de oorspronkelijke legacy-US wanneer dezelfde functionaliteit daar beschreven staat;
7. `.codex/current-session-handoff.md`.

Als een pilotstory bestaande actuele of legacy-functionaliteit geheel of gedeeltelijk realiseert, worden die story/status en legacy-dekking in dezelfde administratieve afronding bijgewerkt. Er worden geen parallelle verhalen met tegenstrijdige statussen achtergelaten.

Documentatiewijzigingen worden na controle automatisch gecommit en naar de actuele
remote branch gepusht, tenzij de gebruiker expliciet anders vraagt of dit door de
branch/worktreestatus onveilig is.

## Werkwijze per story

1. Codex controleert feitelijke code- en documentatiestatus.
2. Codex kiest de eerstvolgende kleine story binnen deze release.
3. Story krijgt scope, buiten scope, acceptatiecriteria, legacy-impact en handmatige test.
4. Gebruiker keurt de story goed.
5. Codex maakt een compact implementation packet voor Claude Code.
6. Claude Code implementeert uitsluitend het packet.
7. Codex reviewt code, build, tests en architectuur.
8. Roelof voert de handmatige acceptatietest uit.
9. Codex werkt release, epic, TODO, legacy-dekking en handoff bij.
10. Na een geslaagde kleine wijziging is commit/push en PR logisch.
