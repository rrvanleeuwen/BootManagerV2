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
- Later scannen van dezelfde barcode opent het bekende product.
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

1. **PILOT-SCAN-01** — Camera-, QR- en barcode-proof-of-concept op de telefoons.
2. **PILOT-AUTH-01** — Owner/Crew-model en eigen login voor Carla.
3. **PILOT-LOC-01** — Opslaggebieden en opslaglocaties.
4. **PILOT-LOC-02** — QR-token genereren, koppelen en locatie openen.
5. **PILOT-INV-01** — Productcategorieën, producten en productbarcodes.
6. **PILOT-INV-02** — Voorraad per product en locatie, inclusief meerdere locaties per product.
7. **PILOT-INV-03** — Product aanmaken met gescande locatie-QR.
8. **PILOT-INV-04** — Product terugvinden via barcode.
9. **PILOT-INV-05** — Voorraadmutaties en eenvoudige historie.
10. **PILOT-LOG-01** — Handmatig logboekmoment met actuele NMEA-snapshot.
11. **PILOT-LOG-02** — Gebeurteniskeuze, weericonen en notitie.
12. **PILOT-E2E-01** — End-to-end gebruikstest door Roelof en Carla.
13. **PILOT-OPS-01** — Duur-, herstart-, opslag- en back-uptest.
14. **PILOT-REL-01** — Release-freeze en uitsluitend blockerfixes.

Codex kiest geen story buiten deze volgorde, tenzij:

- een blocker eerst opgelost moet worden;
- een afhankelijkheid aantoonbaar ontbreekt;
- de gebruiker expliciet een andere prioriteit vaststelt.

## Eerstvolgende story

### PILOT-SCAN-01 — Camera-, QR- en barcode-proof-of-concept

**Status:** Ready

**Als** gebruiker  
**wil ik** in de lokaal gehoste Blazor-app op mijn telefoon een QR-code en productbarcode kunnen scannen  
**zodat** vroeg duidelijk is of de beoogde scanflows technisch en praktisch uitvoerbaar zijn.

**Scope**

- Proof-of-concept binnen de bestaande .NET 8/Blazor-oplossing.
- Test op de telefoons van Roelof en Carla.
- Achtercamera als voorkeurscamera.
- Een QR-code en gangbare lineaire productbarcode herkennen.
- Herkende ruwe codewaarde zichtbaar tonen.
- Duidelijke cameratoestemming, start-, stop- en foutstatus.
- Handmatige code-invoer als fallback.
- Vaststellen welke lokale HTTPS/browser/netwerkvoorwaarden nodig zijn.

**Buiten scope**

- Nog geen opslaglocatie-entiteiten.
- Nog geen productentiteiten.
- Nog geen databaseopslag.
- Nog geen QR-generatie.
- Nog geen voorraadmutaties.
- Nog geen definitieve styling.

**Acceptatiecriteria**

- De scanpagina opent op beide telefoons.
- Cameratoestemming kan worden verleend of geeft een begrijpelijke fout.
- Een test-QR wordt op beide telefoons herkend.
- Minimaal één echte productbarcode wordt op beide telefoons herkend.
- De gedecodeerde waarde wordt zichtbaar getoond.
- Scannen kan worden gestopt en opnieuw gestart.
- Handmatige invoer werkt als fallback.
- De noodzakelijke hosting- en beveiligingsvoorwaarden zijn gedocumenteerd.
- `dotnet build BootManager.sln` slaagt.

**Handmatige acceptatietest**

Test op beide telefoons via dezelfde beoogde lokale boordnetwerkroute. Scan achtereenvolgens een BootManager-test-QR en een echte productbarcode, stop/herstart de camera en controleer de fallback-invoer.

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

1. dit release-document;
2. de relevante actuele epic of userstory;
3. `.docs/TODO.md`;
4. `.docs/legacy-analysis/legacy-coverage-register.md`;
5. de oorspronkelijke legacy-US wanneer dezelfde functionaliteit daar beschreven staat;
6. `.codex/current-session-handoff.md`;
7. README-projectstatus wanneer story- of epiccijfers wijzigen.

Als een pilotstory bestaande actuele of legacy-functionaliteit geheel of gedeeltelijk realiseert, worden die story/status en legacy-dekking in dezelfde administratieve afronding bijgewerkt. Er worden geen parallelle verhalen met tegenstrijdige statussen achtergelaten.

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
