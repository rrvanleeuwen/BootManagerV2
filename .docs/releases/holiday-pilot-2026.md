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
9. **PILOT-INV-03** — **Done** — Scan-gestuurde inruimflow met locatievoorstel, onbekende-code-afhandeling en doorlopende scansessie.
10. **PILOT-INV-04** — **Done** — Product terugvinden via scan of zoeken, locaties tonen en vanuit geen-voorraad direct voorraad toevoegen via een compacte modal.
11. **PILOT-INV-05** — **Done** — Verbruik, correcties en eenvoudige historie.
12. **PILOT-SCAN-02** — **Done** — Parallelle scan-reworkbasis met `old`-isolatie van de huidige flow.
13. **PILOT-SCAN-03** — **Done** — Nieuw scanstartscherm met code-routering, handmatige fallback en recente scans.
14. **PILOT-SCAN-03A** — **Done** — Product-scanwerkcontext zonder legacy-terugval.
15. **PILOT-SCAN-04** — **Done** — Locatiegerichte scanmodus met directe mutatie- en toevoegacties.
16. **PILOT-SCAN-05** — **Done** — Onbekende-code-flow volledig binnen nieuwe scanervaring afronden; regressiefix voor locatie-QR-scan na nieuw product is op 2026-06-25 ook handmatig gevalideerd op Raspberry Pi/mobiel.
17. **PILOT-UX-01** — **Done** — Home is nu de standaard pilotstart met snelle tegels, productzoekwidget en productgerichte doorklik vanuit home.
18. **PILOT-INV-07** — **Done** — Owner-only CSV-startimport voor echte vakantievoorraad, locatie-mapping en QR-tags.
19. **PILOT-INV-08** — **Done** — Product-zoekdetails, directe productbewerking en A4-tagbatchprint vanuit bestaande beheerflows.
20. **PILOT-LOG-01** — **Done** — Handmatig logboekmoment met actuele NMEA-snapshot is op 2026-07-17 geaccepteerd; verdere praktijktest van de logboekflow volgt tijdens de vakantie.
21. **PILOT-LOG-02** — **Done** — Gebeurteniskeuze, weericonen en notitie is op 2026-07-17 geaccepteerd; verdere praktijktest van de logboekflow volgt tijdens de vakantie.
22. **PILOT-INV-06** — **Done** — Productoverzicht gebruikt nu een responsieve, mockup-geleide zoek- en resultaatpresentatie; technisch gecontroleerd en handmatig geaccepteerd op 2026-07-17.
23. **PILOT-PERF-01** — **Gepland** — Productoverzicht met databasegestuurde filtering, paginering en een vast querybudget.
24. **PILOT-PERF-02** — **Gepland** — Home- en gedeelde productzoekflows zonder per-product voorraadqueries.
25. **PILOT-E2E-01** — **Gepland** — End-to-end gebruikstest door Roelof en Carla.
26. **PILOT-PERF-03** — **Gepland** — Overige voorraad-readpaden set-based en zonder N+1-querypatronen maken.
27. **PILOT-PERF-04** — **Gepland** — Inventory-DbContext-lifetime en transactionele write-flow verharden.
28. **PILOT-OPS-01** — **Gepland** — Duur-, herstart-, opslag- en back-uptest.
29. **PILOT-REL-01** — **Gepland** — Release-freeze en uitsluitend blockerfixes.

Codex kiest geen story buiten deze volgorde, tenzij:

- een blocker eerst opgelost moet worden;
- een afhankelijkheid aantoonbaar ontbreekt;
- de gebruiker expliciet een andere prioriteit vaststelt.

**Eerstvolgende stap:** `PILOT-PERF-01`; daarna volgen `PILOT-PERF-02` en
`PILOT-E2E-01`.

## Expliciete herprioritering

Op 2026-06-21 is binnen deze actieve release expliciet gekozen om de scanflows nog
vóór de vakantie te herdefiniëren en te implementeren. Daardoor schuiven
`PILOT-LOG-01` en `PILOT-LOG-02` tijdelijk naar achteren, ondanks de eerdere
standaardvolgorde.

Deze afwijking is toegestaan omdat de gebruiker deze prioriteit expliciet heeft
vastgesteld en de scanflow nu als directe pilotkritieke blocker voor gebruiksgemak en
acceptatie wordt beschouwd.

Op 2026-06-25 is daar een tweede expliciete pilot-herprioritering aan toegevoegd:
eerst extra gebruiksgemak in de dagelijkse pilotbediening via home en
productoverzicht, daarna pas terug naar de eerstvolgende logboekstories. Deze
afwijking is toegestaan omdat de gebruiker dit expliciet heeft gekozen als praktische
testversneller voor de vakantiepilot.

Op 2026-07-16 is daar een derde expliciete pilot-herprioritering aan toegevoegd:
eerst een Owner-only CSV-startimport voor de al fysiek aanwezige vakantievoorraad,
inclusief locatie-mapping en QR-tags, daarna pas verder met `PILOT-INV-06`. Deze
afwijking is toegestaan omdat de gebruiker dit expliciet heeft gekozen als directe
vakantiekritieke versneller voor ingebruikname op de Raspberry Pi.

Op 2026-07-16 is daar na handmatige acceptatie van `PILOT-INV-07` direct een vierde
expliciete pilot-herprioritering aan toegevoegd: eerst nog twee kleine maar
vakantiekritieke UX-verbeteringen in de bestaande beheerflows via `PILOT-INV-08`,
daarna pas de bredere layoutstory `PILOT-INV-06`. Deze afwijking is toegestaan omdat
de gebruiker deze twee resterende praktische bevindingen nog vóór vertrek opgelost wil
hebben.

Op 2026-07-16 is na afronding en merge van `PILOT-INV-08` opnieuw expliciet gekozen om
eerst terug te keren naar de logboekpilot: `PILOT-LOG-01` en daarna `PILOT-LOG-02`.
`PILOT-INV-06` blijft gepland, maar schuift achter deze twee logboekstories. Deze
afwijking is toegestaan omdat de gebruiker de logboekbasis nu als eerstvolgende
pilotfocus heeft vastgesteld.

Op 2026-07-17 is na performanceanalyse van het productoverzicht expliciet gekozen om
de direct zichtbare N+1-querypatronen vóór de end-to-endtest op te lossen. Daarom
schuiven `PILOT-PERF-01` en `PILOT-PERF-02` vóór `PILOT-E2E-01`. De bredere
voorraad-readpaden en DbContext-/write-hardening blijven afzonderlijke kleine stories
na de E2E-test, zodat de directe pilotverbetering niet uitgroeit tot één risicovolle
architectuurrefactor.

## Story-uitwerking en archief

Dit release-document blijft compact voor actuele pilotsturing. Volledig uitgewerkte
afgeronde stories staan in `.docs/releases/holiday-pilot-2026-archive-completed-stories.md`.

### Actieve werkset

- `PILOT-PERF-01` is nu de eerstvolgende implementatiestory.
- `PILOT-PERF-02` volgt direct daarna; vervolgens hervat `PILOT-E2E-01` de integrale
  praktijktest.
- `PILOT-PERF-03` en `PILOT-PERF-04` blijven afzonderlijke vervolgstories na de
  E2E-test en vóór de duur-/operationele test.
- Houd in dit document alleen de actuele releasekaders, prioriteitsvolgorde,
  eerstvolgende story en de actieve of direct geplande uitgewerkte stories.
- Verplaats een story na afronding en administratieve controle naar het archief,
  zodat de dagelijkse context klein blijft maar de historie beschikbaar blijft.
- Raadpleeg het archief alleen wanneer historische scope, acceptatie,
  implementatiestatus of legacy-impact opnieuw relevant is.

### PILOT-PERF-01 — Productoverzicht met vast querybudget

**Story:** Als gebruiker wil ik het productoverzicht en de productzoeking snel kunnen
openen en doorbladeren, zodat de responstijd niet lineair verslechtert door losse
databasequeries per product.

**Scope:**

- een gespecialiseerde EF Core-readquery voor het productoverzicht;
- databasegestuurde actieve/gearchiveerde filtering, zoeken, stabiele sortering en
  paginering per tien resultaten;
- directe DTO-projectie van product, eenheid, code en actieve categorie;
- voorraadtotalen en locaties in een gebatchte query voor alleen de huidige pagina;
- categorie- en eenheidlookups pas laden wanneer aanmaken of bewerken ze nodig heeft;
- SQLite-integratietest met een vast querybudget dat niet groeit met het totale aantal
  producten.

**Buiten scope:** UI-herontwerp, nieuwe voorraadregels, scanroutes, homezoeking en de
bredere DbContext-/write-refactor.

**Acceptatiecriteria:**

- bestaand productoverzicht, zoeken, archieftoggle, paginering, detail- en bewerkactie
  blijven functioneel gelijk;
- de eerste overzichtspagina wordt met maximaal vijf databasecommando's opgebouwd;
- meer opgeslagen producten verhogen het aantal databasecommando's voor dezelfde
  pagina niet;
- alleen de tien zichtbare producten en hun benodigde overzichtsdata worden geladen;
- lege voorraad blijft als `0` met de bestaande no-stockpresentatie zichtbaar.

**Handmatige acceptatietest:** Open `Voorraadbeheer > Producten` op desktop en mobiel,
zoek op een deel van naam en omschrijving, blader vooruit en terug, wissel naar
gearchiveerd en open detail en bewerken. Controleer tegelijk in de EF-log dat het
queryaantal per pagina begrensd blijft en dat aantallen, eenheden en locaties gelijk
zijn aan de bestaande gegevens.

**Legacy-impact:** Verbetert de bestaande dekking van `US2.12 Zoeken en filteren`
zonder nieuwe functionele filters toe te voegen.

### PILOT-PERF-02 — Home- en gedeelde productzoeking batchen

**Story:** Als gebruiker wil ik vanuit home en scan-/voorraadzoekflows snel producten
kunnen vinden, zodat een zoekopdracht niet voor ieder resultaat afzonderlijk product-
en voorraadgegevens hoeft op te halen.

**Scope:**

- de readquery uit `PILOT-PERF-01` hergebruiken voor de home-productzoekwidget;
- voorraad alleen voor de zichtbare zoekresultaatpagina batchgewijs ophalen;
- gedeelde naam-/omschrijvingzoeking databasegestuurd uitvoeren;
- gerichte regressietests voor homepaginering, totalen, locaties en vervolgacties;
- querybudget en responstijd met representatieve pilotdata vastleggen.

**Buiten scope:** nieuwe zoekvelden, fuzzy search, UI-redesign, scanroutering en nog
niet door deze flows gebruikte `StockService`-methoden.

**Acceptatiecriteria:**

- home toont dezelfde zoekresultaten, aantallen, eenheden, locaties en vervolgacties;
- alleen de zichtbare resultaatpagina krijgt voorraadgegevens;
- het databasecommandoaantal blijft begrensd wanneer het aantal zoekmatches groeit;
- productzoeken op naam en omschrijving blijft hoofdletterongevoelig en ondersteunt
  deelmatches.

**Handmatige acceptatietest:** Zoek vanaf home naar een term met meer dan tien matches,
blader door beide resultaatpagina's en open producten met nul, één en meerdere actieve
locaties. Vergelijk de gegevens met het productoverzicht en controleer het begrensde
queryaantal in de EF-log.

**Legacy-impact:** Verbetert de bestaande dekking van `US2.12 Zoeken en filteren`
zonder de functionele zoekscope uit te breiden.

### PILOT-PERF-03 — Overige voorraad-readpaden set-based maken

**Story:** Als gebruiker wil ik voorraad per product en locatie zonder oplopende
vertraging kunnen bekijken, zodat ook de overige inventoryflows bruikbaar blijven bij
groeiende pilotdata.

**Scope:** inventariseer de resterende readmethoden in `StockService`, vervang bewezen
N+1-lussen door gerichte projecties of gebatchte queries en borg per geraakte flow een
querybudget en resultaatregressietest.

**Buiten scope:** functionele voorraadwijzigingen, UI-redesign en DbContext-/write-
lifetimewijzigingen uit `PILOT-PERF-04`.

**Acceptatiecriteria:** alle door de pilot-UI gebruikte voorraadlijsten hebben een
begrensd queryaantal; product-, locatie-, gebieds- en eenheidsgegevens blijven correct;
geen readpad laadt onbegrensd alle inventorydata wanneer slechts één pagina of locatie
nodig is.

**Handmatige acceptatietest:** Doorloop product terugvinden, locatie-inhoud,
voorraadbijzonderheid en scan-gestuurde voorraadweergave met nul, één en meerdere
locaties en controleer gegevens en EF-log.

**Legacy-impact:** Technische verbetering van bestaande inventorydekking; geen nieuwe
legacy-functionaliteit.

### PILOT-PERF-04 — DbContext- en write-flowhardening

**Story:** Als gebruiker wil ik dat inventorybewerkingen betrouwbaar en atomair worden
opgeslagen, zodat een langlevende Blazor-sessie of een fout halverwege geen tracking-
conflict of gedeeltelijk opgeslagen bedrijfsbewerking veroorzaakt.

**Scope:** context-per-operatie voor inventory via de bestaande DbContext-factory,
expliciete transactieboundaries voor meerstaps writes, één `SaveChanges` per
bedrijfsbewerking waar passend en regressietests voor tracking, rollback en
gelijktijdige UI-acties.

**Buiten scope:** brede applicatiebrede repositoryvervanging, migraties zonder
aangetoonde noodzaak en functionele inventorywijzigingen.

**Acceptatiecriteria:** inventory-read- en write-operaties delen geen onbedoeld
langlevende circuitcontext; een mislukte meerstapsbewerking laat geen gedeeltelijke
product-/mapping-/code-/voorraadstatus achter; bestaande create-, edit-, scan- en
mutatieflows blijven werken.

**Handmatige acceptatietest:** Maak, wijzig en archiveer producten, voeg en vervang een
code, voeg voorraad toe en forceer de beschikbare validatiefoutpaden binnen één lange
ingelogde sessie. Controleer dat geen tracking-/concurrencyfouten ontstaan en dat
mislukte acties geen gedeeltelijke data opslaan.

**Legacy-impact:** Geen nieuwe legacy-functionaliteit; betrouwbaarheidshardening van
bestaande product- en voorraadflows.

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
