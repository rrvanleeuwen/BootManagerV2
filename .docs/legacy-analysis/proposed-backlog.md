# Proposed BootManagerV2 Backlog From Legacy Scope

Status: eerste voorstel (2026-05-25).

Dit document vertaalt de legacy PDF naar BootManagerV2-epics in de huidige stijl.
Het is een voorstel, geen definitieve roadmap.

## Prioriteit 1: Beheer Na Onboarding

Epic bestaat al:

- `.docs/epics/owner-profile-settings.md`

Voorgestelde volgorde:

1. Bootgegevens wijzigen in instellingen.
2. Eigenaargegevens wijzigen in instellingen.
3. Wachtwoord wijzigen runtime/UX verifiëren.
4. Settings logisch ordenen.

Reden:

- Direct voortgekomen uit handmatige test.
- Sluit aan op afgeronde onboarding.
- Kleine, gecontroleerde slices.

## Prioriteit 2: Logboek Verder Afronden

Bestaande epic:

- `.docs/epics/digital-logbook.md`

Open legacy-scope die nog relevant is:

### US-LB1: Logboek Afronden Bij Aankomst

Als eigenaar wil ik bij aankomst ontbrekende eindgegevens invullen, zodat een reislogboek compleet wordt afgesloten.

Velden kunnen later bevatten:

- aankomsthaven;
- eind-logstand;
- motoruren eind;
- brandstofniveau eind;
- totale afstand;
- opmerkingen.

Bronverfijning uit Epic 5 Word:

- afrondingsscherm opent wanneer de gebruiker aangeeft dat de passage is voltooid;
- na bevestiging wordt het logboek als voltooid gemarkeerd;
- afgeronde logboeken tellen mee in statistieken.

### US-LB2: Motoruren En Brandstof In Reisheader

Als eigenaar wil ik motoruren en brandstofgegevens in de reisheader vastleggen, zodat verbruik later inzichtelijk wordt.

### US-LB3: Routekaart Op Basis Van Positiemetingen

Als gebruiker wil ik de gevaren route als kaart/track zien, zodat het logboek visueel bruikbaar wordt.

Bronverfijning uit Epic 5 Word:

- klassieke logboekweergave verwacht naast tabel/header ook een routekaart.

### US-LB4: Export Naar PDF/CSV

Als eigenaar wil ik logboekgegevens exporteren, zodat ik reizen kan archiveren of delen.

Let op:

- Browser print bestaat al.
- Server-side PDF en CSV zijn aparte stories.

### US-LB5: Logboekstatistieken Uitbreiden

Als eigenaar wil ik reisstatistieken zien, zoals afstand, duur, gemiddelde snelheid en eventueel brandstofverbruik.

Bronverfijning uit Epic 5 Word:

- samenvatting bevat reisduur, afstand, gemiddelde snelheid, brandstofverbruik en windstatistieken.

### US-LB6: Passage Aan Logboek Koppelen

Als eigenaar wil ik een logboekreis aan een passageplan koppelen, zodat planning en werkelijk reisverloop bij elkaar blijven.

## Prioriteit 3: Inventory & Storage Locations

Nieuwe epic voorgesteld:

- `.docs/epics/inventory-management.md`

Eerste slices:

### US-INV1: Productcategorieën Modelleren

Als eigenaar wil ik productcategorieën beheren met naam, omschrijving en herkenbaar icoon, zodat voorraad overzichtelijk kan worden ingedeeld.

Bronverfijning uit Epic 2 Word:

- categorie heeft naam, korte omschrijving en icoon;
- icoontjes kunnen later uit een bibliotheek of upload komen;
- eerste BootManagerV2-slice mag beginnen met een vaste iconenset en upload uitstellen.

### US-INV2: Opslaglocaties Modelleren

Als eigenaar wil ik opslaggebieden en opslaglocaties vastleggen, zodat voorraad later aan fysieke plekken gekoppeld kan worden.

Voorbeelden:

- kajuit;
- kombuis;
- machinekamer;
- kast;
- lade;
- bak.

Bronverfijning uit Epic 1 Word:

- opslaglocatie heeft minimaal naam en korte omschrijving;
- opslaglocatie hangt onder een gebied;
- locaties moeten later een detailpagina krijgen omdat QR-scans daar direct naartoe openen.

### US-INV3: Productcatalogus Aanmaken

Als eigenaar wil ik producten kunnen aanmaken met naam, categorie en eenheid, zodat een inventarisbasis ontstaat.

Bronverfijning uit Epic 2 Word:

- productgegevens omvatten naam, omschrijving, categorie, eenheid, minimumvoorraad, barcode(s), optioneel foto/label en opslaglocatie;
- productfoto, barcode/QR en AI-herkenning kunnen later worden toegevoegd als aparte slices.

### US-INV4: Product Aan Opslaglocatie Koppelen

Als eigenaar wil ik producten aan locaties koppelen met hoeveelheden, zodat ik weet wat waar ligt.

### US-INV5: Voorraad Aanpassen En Loggen

Als gebruiker wil ik voorraad kunnen corrigeren of verbruik registreren, zodat aantallen actueel blijven.

Bronverfijning uit Epic 2 Word:

- mutatielog toont datum, gebruiker, product, oude hoeveelheid en nieuwe hoeveelheid;
- aankoop verhoogt bestaande voorraad;
- verbruik verlaagt bestaande voorraad en kan later via barcode worden gestart.

### US-INV6: Zoeken En Filteren

Als gebruiker wil ik producten kunnen zoeken op naam, categorie en locatie.

Later:

- minimumvoorraad;
- QR/barcode;
- QR-tag genereren, printen/exporteren en vervangen per opslaglocatie;
- QR-scan naar opslaglocatie-detail;
- import/export;
- voorraadstatus dashboard;
- passageplanning-integratie.

## Prioriteit 4: General Document Management

Nieuwe epic voorgesteld:

- `.docs/epics/document-management.md`

Eerste slices:

### US-DOC1: Document Toevoegen

Als eigenaar wil ik documenten uploaden met titel, type, beschrijving en optionele vervaldatum.

Bronverfijning uit Epic 4 Word:

- upload ondersteunt later PDF, JPG, PNG, DOCX enzovoort;
- document krijgt naam, beschrijving, categorie en optionele vervaldatum;
- categorie kan bestaand zijn of tijdens upload worden aangemaakt;
- eerste slice moet lokale/offline opslag als uitgangspunt nemen.

### US-DOC2: Documenten Zoeken En Filteren

Als eigenaar wil ik documenten snel terugvinden op type, titel, status of vervaldatum.

### US-DOC3: Vervaldatumstatus Tonen

Als eigenaar wil ik zien welke documenten bijna verlopen of verlopen zijn.

### US-DOC4: Document Openen En Downloaden

Als eigenaar wil ik een opgeslagen document kunnen openen of downloaden, zodat ik het aan boord zonder internet kan raadplegen.

Let op:

- Printen kan via browser/device-functionaliteit.
- Mailen of extern delen vraagt internet en blijft later.

Later:

- koppelen aan passage;
- koppelen aan onderhoud;
- audit trail;
- export documentlijst.
- instelbare herinneringstermijn;
- documentdashboard met geldig/bijna verlopen/verlopen;
- printen/mailen/delen.

## Prioriteit 5: Maintenance Log

Nieuwe epic voorgesteld:

- `.docs/epics/maintenance-log.md`

Eerste slices:

### US-MAINT1: Onderhoudstaak Aanmaken

Als eigenaar wil ik onderhoudstaken kunnen vastleggen met onderdeel, beschrijving en geplande datum.

Bronverfijning uit Epic 6 Word:

- taak bevat naam, beschrijving, onderdeel, prioriteit en interval;
- status en verwachte uitvoerdatum horen bij de taak;
- eerste slice mag beginnen met tijdsinterval; gebruiksinterval op motoruren kan later.

### US-MAINT2: Uitgevoerd Onderhoud Registreren

Als eigenaar wil ik een taak als uitgevoerd registreren met datum, kosten, monteur en opmerkingen.

Bronverfijning uit Epic 6 Word:

- uitgevoerd onderhoud bevat datum, beschrijving, kosten, monteur en gebruikte onderdelen;
- taak wordt daarna onderdeel van de onderhoudshistoriek.

### US-MAINT3: Onderhoudshistorie Bekijken

Als eigenaar wil ik onderhoud per onderdeel of periode kunnen bekijken.

### US-MAINT4: Onderhoud Zoeken En Filteren

Als eigenaar wil ik onderhoudstaken kunnen zoeken en filteren op status, onderdeel of periode.

Later:

- intervalplanning;
- herinneringen;
- bijlagen;
- export;
- dashboard.
- documentkoppeling met facturen/handleidingen;
- gebruiksinterval op motoruren of vaarkilometers.

## Prioriteit 6: Passageplanning

Nieuwe epic voorgesteld:

- `.docs/epics/passage-planning.md`

Niet starten voordat inventory/document basics duidelijk zijn.

Eerste slices:

### US-PASS1: Passage Aanmaken

Als eigenaar wil ik een passageplan aanmaken met vertrekdatum, bestemming en duur, zodat ik een reis kan voorbereiden.

### US-PASS2: Bemanningslijst Vastleggen

Als eigenaar wil ik bemanningsleden met volledige naam en geboortedatum aan een passage koppelen, zodat exportdocumenten later compleet zijn.

Let op:

- Dit hoeft niet direct gekoppeld te zijn aan multi-user accounts.
- Bemanning in passageplanning is eerst reisdata, geen autorisatiemodel.

### US-PASS3: Passage Koppelen Aan Logbook Trip

Als eigenaar wil ik een passage aan een logboekreis koppelen, zodat planning en werkelijk reisverloop bij elkaar blijven.

### US-PASS4: Verbruiksprofielen Voor Reisberekening

Als eigenaar wil ik brandstofverbruik per motoruur en waterverbruik per persoon per dag instellen, zodat latere berekeningen realistisch zijn.

Later:

- voorraadberekening op basis van inventaris;
- boodschappenlijst exporteren;
- menuplanning per dag/maaltijd;
- documenten koppelen en vervaldatumwaarschuwingen;
- passage-dashboard;
- reisplan exporteren naar PDF/CSV;
- passage dupliceren als template.

## Prioriteit 7: Systeembeheer, Backup En Device Status

Uitbreiding op bestaande settings/deployment-docs.

Mogelijke stories:

### US-SYS1: Backup Maken Van Lokale Data

Als eigenaar wil ik een back-up kunnen maken van SQLite database, bijlagen en configuratie, zodat ik geen data verlies.

Bronverfijning uit Epic 8 Word:

- legacy noemt export als `.zip` of `.json`;
- BootManagerV2 moet rekening houden met SQLite, capture logs en attachment/document-bestanden.

### US-SYS2: Restoreprocedure Voor Lokale Data

Als eigenaar wil ik een back-up gecontroleerd kunnen herstellen, zodat ik na storing of herinstallatie verder kan.

### US-SYS3: Raspberry Pi Systeemstatus Tonen

Als eigenaar wil ik CPU, geheugen, opslag en netwerkstatus zien, zodat ik de installatie kan monitoren.

### US-SYS4: Systeemactie-logboek

Als eigenaar wil ik recente systeemacties zien, zoals back-ups, updates en fouten.

### US-SYS5: Eenheden En Regio-instellingen

Als eigenaar wil ik eenheden en datum/tijdnotatie instellen, zodat metingen consistent worden weergegeven.

Later:

- veilige shutdown-flow;
- instellingen exporteren/importeren;
- standaardinstellingen herstellen;
- taalkeuze;
- cloud/offline-sync instellingen pas wanneer sync bestaat.

## Prioriteit 8: Dashboard & Overzicht

Nieuwe epic voorgesteld:

- `.docs/epics/dashboard-overview.md`

Niet starten als volledige legacy-widgetset voordat de onderliggende modules bestaan.

Eerste slices met huidige BootManagerV2-data:

### US-DASH1: Dashboard Met Actieve Boot En Systeemstatus

Als eigenaar wil ik op de startpagina de actieve boot, lokale tijd, ingest/status en relevante systeemmeldingen zien.

Bronverfijning uit Epic 7 Word:

- legacy-dashboard toont actieve boot, locatie, datum, tijd, stroomvoorziening en netwerkstatus;
- BootManagerV2 kan eerst focussen op single-vessel status en bestaande ingest/settings-status.

### US-DASH2: Logboekactiviteit Widget

Als gebruiker wil ik recente logboekvermeldingen en open concept/missing-moment signalen zien, zodat ik direct weet of actie nodig is.

### US-DASH3: Modulewaarschuwingen Paneel

Als gebruiker wil ik waarschuwingen met urgentie en link naar de betreffende module zien.

Let op:

- eerste waarschuwingen kunnen uit bestaande logboekflow komen;
- voorraad, onderhoud en documenten sluiten later aan wanneer die modules bestaan.

Later:

- voorraadstatus widget;
- onderhoudsstatus widget;
- documentstatus widget;
- passageplanning widget;
- weer/getijden widget;
- widgetpersonalisatie;
- automatische live updates;
- cloud-afgeleide status.

## Geparkeerd / Lage Prioriteit

Deze legacy-scope is bewust niet voor de korte termijn:

- multi-user rollenmodel;
- meerdere boten;
- cloud-synchronisatie;
- volledige notificatie-infrastructuur;
- dashboard widgetpersonalisatie.

## Lage Prioriteit: Slimme Herkenning & AI

Nieuwe epic voorgesteld:

- `.docs/epics/smart-recognition-ai.md`

Niet starten voordat inventaris en onderhoud genoeg data leveren.

Mogelijke latere slices:

### US-AI1: Barcode/QR Herkenning Voor Inventaris

Als gebruiker wil ik een barcode of QR-code kunnen scannen, zodat BootManager productinformatie of locatie-informatie kan aanvullen.

Let op:

- Dit hoeft niet direct AI te zijn.
- Deze story hoort waarschijnlijk eerst bij inventory/scanning.

### US-AI2: Productherkenning Via Foto

Als gebruiker wil ik een productfoto kunnen laten herkennen, zodat BootManager suggesties kan doen voor naam of categorie.

Later:

- automatische categorisatie;
- aanvulsuggesties op basis van verbruiksdata;
- predictief onderhoud;
- spraakinput.

## Latere Integraties

Nieuwe epic voorgesteld:

- `.docs/epics/integrations.md`

Niet starten voordat lokale kernflows stabiel zijn, behalve waar een integratie direct nut heeft voor bestaande data.

Mogelijke eerste slices:

### US-INT1: GPX Import Voor Routes

Als eigenaar wil ik een GPX-bestand kunnen importeren, zodat routepunten lokaal beschikbaar zijn zonder afhankelijkheid van een externe dienst.

### US-INT2: AIS Schepenoverzicht Uit NMEA 0183

Als gebruiker wil ik AIS-berichten uit bestaande NMEA 0183 input kunnen omzetten naar een eenvoudig schepenoverzicht.

Let op:

- BootManagerV2 herkent `!AIVDM`/`!AIVDO` al raw/parser-technisch.
- Semantische AIS-decodering is een aparte slice.

### US-INT3: Weer/Getijden Integratie

Als gebruiker wil ik weer- en getijdeninformatie kunnen raadplegen voor dashboard en passageplanning.

Later:

- haveninformatie API;
- API-sleutelbeheer;
- Bluetooth/Wi-Fi sensor onboarding;
- synchronisatie met andere apparaten.

## Latere Rapportage & Analyse

Nieuwe epic voorgesteld:

- `.docs/epics/reporting-analysis.md`

Niet breed starten voordat inventaris, onderhoud en passagekosten voldoende data leveren.

Mogelijke eerste slices:

### US-REP1: Logboek CSV Export

Als eigenaar wil ik logboekregels als CSV kunnen exporteren, zodat ik reizen buiten BootManager kan analyseren of archiveren.

### US-REP2: Reisstatistieken Uit Logboek

Als eigenaar wil ik afstand, duur, gemiddelde snelheid en eventueel brandstofverbruik per reis zien.

Later:

- brandstofanalyse per passage;
- voorraadanalyse op basis van voorraadmutaties;
- onderhoudsrapportage met kosten;
- kostenanalyse per tocht;
- PDF-export van rapportages;
- grafieken en visuele trends.

## Latere Notificaties & Waarschuwingen

Nieuwe epic voorgesteld:

- `.docs/epics/notifications-alerts.md`

Niet starten als push/e-mail-infrastructuur voordat de waarschuwingsbronnen bestaan.

Mogelijke eerste slice:

### US-NOTIF1: Generiek In-App Waarschuwingenpaneel

Als gebruiker wil ik actuele waarschuwingen in BootManager kunnen zien, zodat ik actie kan nemen zonder externe push of e-mail.

Bronverfijning uit Epic 11 Word:

- bronnen zijn lage voorraad, documentverval, onderhoud en passageplanning;
- BootManagerV2 kan beginnen met bestaande logboek/missing-moment waarschuwingen en later bronnen toevoegen.

Later:

- notificatiegeschiedenis;
- notificatievoorkeuren;
- documentvervalmeldingen;
- onderhoudsherinneringen;
- lage-voorraadmeldingen;
- passagevertrek-waarschuwingen;
- browser push;
- e-mail.
