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

1. **PILOT-SCAN-01** — **Done** — Camera-, QR- en barcode-proof-of-concept op de telefoons.
2. **PILOT-AUTH-01** — **Done** — Owner/Crew-model en eigen login voor Carla.
3. **PILOT-LOC-01** — **Done** — Opslaggebieden en opslaglocaties.
4. **PILOT-LOC-02** — **Done** — QR-token genereren, koppelen en locatie openen.
5. **PILOT-LOC-03** — **Gepland** — QR-tag printen en PNG exporteren.
6. **PILOT-LOC-04** — **Gepland** — QR-token vervangen en tagoverzicht.
7. **PILOT-INV-01** — **Gepland** — Productcategorieën, producten en productbarcodes.
8. **PILOT-INV-02** — **Gepland** — Voorraad per product en locatie, inclusief meerdere locaties per product.
9. **PILOT-INV-03** — **Gepland** — Product aanmaken met gescande locatie-QR.
10. **PILOT-INV-04** — **Gepland** — Product terugvinden via barcode.
11. **PILOT-INV-05** — **Gepland** — Voorraadmutaties en eenvoudige historie.
12. **PILOT-LOG-01** — **Gepland** — Handmatig logboekmoment met actuele NMEA-snapshot.
13. **PILOT-LOG-02** — **Gepland** — Gebeurteniskeuze, weericonen en notitie.
14. **PILOT-E2E-01** — **Gepland** — End-to-end gebruikstest door Roelof en Carla.
15. **PILOT-OPS-01** — **Gepland** — Duur-, herstart-, opslag- en back-uptest.
16. **PILOT-REL-01** — **Gepland** — Release-freeze en uitsluitend blockerfixes.

Codex kiest geen story buiten deze volgorde, tenzij:

- een blocker eerst opgelost moet worden;
- een afhankelijkheid aantoonbaar ontbreekt;
- de gebruiker expliciet een andere prioriteit vaststelt.

**Eerstvolgende story:** `PILOT-LOC-03` — QR-tag printen en PNG exporteren.

## Uitgewerkte stories

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

**Status:** Gepland; story uitgewerkt op 2026-06-18.

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

- `US1.12 Tag genereren voor opslaglocatie` wordt met deze story verder gepland:
  printen en exporteren als afbeelding worden afgedekt nadat `PILOT-LOC-02` de
  token- en QR-waarde levert.
- Vervangen van tags en tagoverzicht blijven voor `PILOT-LOC-04`.

**Handmatige acceptatietest**

Log in als Owner, open een locatie met bestaande QR-token en open de tagpagina. Start
browserprint en download de PNG. Scan daarna de zichtbare QR-code of de gedownloade
PNG vanaf een tweede scherm en controleer dat dezelfde locatiepagina opent. Controleer
dat Crew deze print/exportactie niet kan uitvoeren.

**Technische richting**

- Gebruik de bestaande browserprintstijl als patroon; voeg geen server-side PDF-export
  toe.
- Gebruik browserdownload voor PNG-export, aansluitend op bestaande downloadpatronen
  in de webapp.
- Als een QR-generator nodig is, voeg alleen een kleine lokale dependency of
  client-side module toe die offline werkt.

### PILOT-LOC-04 — QR-token vervangen en tagoverzicht

**Status:** Gepland; story uitgewerkt op 2026-06-18.

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

- `US1.14 Tag opnieuw koppelen of vervangen` wordt met deze story gepland voor
  functionele dekking: oude token ongeldig, nieuw token actief.
- `US1.15 Overzicht van alle tags` wordt met deze story gepland voor functionele
  dekking van een overzicht met locaties, tokeninformatie en handmatige tagstatus.
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
