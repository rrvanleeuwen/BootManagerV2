# Word Source Processing Progress

Status: gestart op 2026-05-25.

Deze lijst bewaakt de afgesproken werkwijze: telkens één Word-bestand analyseren, verwerken en daarna stoppen voor akkoord voordat het volgende bestand wordt opgepakt.

## Verwerkt

### BootManager_Softwarevisie_v0.7.docx

Status: verwerkt.

Resultaat:

- Bevestigt de brede functionele scope van BootManager als offline boordbeheersysteem.
- Bevestigt hoofdmodules: boot- en locatiebeheer, voorraadbeheer, passageplanning, documentbeheer, reislogboek, onderhoudsbeheer, gebruikersbeheer, rapportage, notificaties en AI/herkenning.
- Bevestigt niet-functionele doelen: offline gebruik, Raspberry Pi/laptop, SQLite, eenvoudige back-ups, responsive UI en snelle zoek/filterfuncties.
- Technische architectuur uit dit document is bewust genegeerd voor BootManagerV2; alleen functionele scope is meegenomen.

Verwerkt in:

- `README.md`
- `scope-inventory.md`

### BootManager_Epic0_Installatie_Authenticatie.docx

Status: verwerkt.

Resultaat:

- Bevestigt US0.1 t/m US0.6 volledig: installatie, eerste eigenaar, login, wachtwoord/pincode wijzigen, toegang herstellen en eigenaarprofiel beheren.
- Geen aanvullende user stories gevonden buiten de eerdere OCR-inventarisatie.
- Bevestigt dat pincode en back-upcode/beheersleutel legacy-scope zijn; BootManagerV2 houdt wachtwoord-only login en operationele resetprocedure aan.
- Bevestigt dat eigenaarprofiel beheren open blijft en al is ondergebracht in `Owner Profile & Vessel Settings`.

Verwerkt in:

- `scope-inventory.md`
- `mapped-epics.md`
- `implemented-or-obsolete.md`

### BootManager_Epic1_Bootbeheer_en_Gebruikersbeheer.docx

Status: verwerkt.

Resultaat:

- Bevestigt US1.1 t/m US1.17 volledig.
- Lost de eerdere OCR-gap US1.9 t/m US1.13 op.
- US1.9 t/m US1.13 gaan over bootstructuur, gebieden, opslaglocaties, QR-tag generatie en QR-scan naar opslaglocatie-detail.
- Bevestigt dat multi-user en multi-boot legacy-scope blijven, maar voor BootManagerV2 voorlopig geparkeerd zijn.
- Bevestigt dat opslaglocaties en QR-tags als basis onder de toekomstige inventaris/opslaglocatie-epic vallen.

Verwerkt in:

- `scope-inventory.md`
- `mapped-epics.md`
- `implemented-or-obsolete.md`
- `proposed-backlog.md`

### BootManager_Epic2_Inventarisbeheer.docx

Status: verwerkt.

Resultaat:

- Bevestigt US2.1 t/m US2.21 volledig.
- Lost de eerdere OCR-gap US2.1 en US2.2 op.
- US2.1 gaat over categorieën beheren met naam, omschrijving en icoon.
- US2.2 gaat over categorie-icoontjes beheren via bibliotheek of PNG/SVG-upload.
- Bevestigt de kernvolgorde voor BootManagerV2: categorieën, opslaglocaties, productcatalogus, voorraad per locatie, voorraadmutaties en zoeken/filteren.
- Bevestigt dat barcode/QR, AI-herkenning, passageplanning-integratie en cloud-synchronisatie latere uitbreidingen zijn.

Verwerkt in:

- `scope-inventory.md`
- `mapped-epics.md`
- `implemented-or-obsolete.md`
- `proposed-backlog.md`
- `README.md`

### BootManager_Epic3_PassagePlanning.docx

Status: verwerkt.

Resultaat:

- Bevestigt US3.1 t/m US3.14 volledig.
- Geen aanvullende user stories gevonden buiten de eerdere OCR-inventarisatie.
- Bevestigt dat passageplanning bestaat uit passagebeheer, bemanningslijst, voorraadberekening, menuplanning, documentkoppeling, export, dashboard/status en logboekkoppeling.
- Bevestigt afhankelijkheden: inventaris voor voorraadvergelijking, documentbeheer voor certificaten/vervaldatums en logboek voor werkelijk reisverloop.
- Bevestigt dat bemanning in passageplanning volledige naam en geboortedatum vraagt, maar niet noodzakelijk direct multi-user accounts betekent.
- Bevestigt dat cloud-synchronisatie toekomstscope blijft; lokale/offline planning is de relevante kern.

Verwerkt in:

- `scope-inventory.md`
- `mapped-epics.md`
- `implemented-or-obsolete.md`
- `proposed-backlog.md`

### BootManager_Epic4_Documentbeheer.docx

Status: verwerkt.

Resultaat:

- Bevestigt US4.1 t/m US4.13.
- Lost de eerdere OCR-gap US4.1 op: document toevoegen en categoriseren.
- Voegt ten opzichte van de OCR-lijst US4.13 toe: document openen, printen of delen.
- Bevestigt dat algemene documentbeheer-scope breder is dan logboekbijlagen.
- Bevestigt kernscope: lokale/offline documentopslag, metadata, categorieën, optionele vervaldatum, zoeken/filteren en statusoverzicht.
- Bevestigt afhankelijkheden: passageplanning voor documentkoppeling, notificaties voor waarschuwingen en toekomstige cloud/mail-functionaliteit voor delen.

Verwerkt in:

- `scope-inventory.md`
- `mapped-epics.md`
- `implemented-or-obsolete.md`
- `proposed-backlog.md`
- `README.md`

### BootManager_Epic5_Logboek.docx

Status: verwerkt.

Resultaat:

- Bevestigt US5.1 t/m US5.14 volledig.
- Lost de eerdere OCR-gap US5.1 op: handmatig logboek invoeren met weerinformatie.
- Bevestigt dat BootManagerV2 al een substantieel deel dekt: logboekreis, logregels, nautische velden, draft/confirmed flow, bijlagen, missing moments en printweergave.
- Bevestigt open legacy-scope: passagekoppeling, routekaart, motoruren/brandstof afronding, uitgebreide statistieken, PDF/CSV-export en afrondingsflow bij aankomst.
- Bevestigt dat cloud-synchronisatie toekomstscope blijft; offline lokale logboekwerking is kern.

Verwerkt in:

- `scope-inventory.md`
- `mapped-epics.md`
- `implemented-or-obsolete.md`
- `proposed-backlog.md`
- `README.md`

### BootManager_Epic6_OnderhoudsbeheerL.docx

Status: verwerkt.

Resultaat:

- Bevestigt US6.1 t/m US6.14 volledig.
- Geen aanvullende user stories gevonden buiten de eerdere OCR-inventarisatie.
- Bevestigt dat onderhoudsbeheer een technisch logboek is voor taken, onderdelen, intervallen, uitvoering, kosten, monteur, bijlagen en historiek.
- Bevestigt afhankelijkheden: documentbeheer voor facturen/handleidingen en notificaties voor herinneringen/waarschuwingen.
- Bevestigt dat tijdsinterval een goede eerste stap is; gebruiksinterval op motoruren/vaarkilometers vraagt latere koppeling met logboek/metingen.
- Bevestigt dat cloud-synchronisatie toekomstscope blijft; lokale/offline onderhoudsregistratie is kern.

Verwerkt in:

- `scope-inventory.md`
- `mapped-epics.md`
- `implemented-or-obsolete.md`
- `proposed-backlog.md`

### BootManager_Epic7_Dashboard.docx

Status: verwerkt.

Resultaat:

- Bevestigt US7.1 t/m US7.14 volledig.
- Lost de eerdere OCR-gap US7.1 t/m US7.8 op.
- US7.1 t/m US7.8 gaan over dashboard openen, actieve bootinformatie, waarschuwingen, weer/getijden en widgets voor voorraad, onderhoud, documenten en passageplanning.
- Bevestigt dat het dashboard een centrale startpagina is met doorklikbare widgets.
- Bevestigt afhankelijkheden: voorraad-, onderhouds-, document- en passagewidgets wachten op die modules.
- Bevestigt dat widgetpersonalisatie, automatische updates en cloud-synchronisatie latere uitbreidingen zijn.

Verwerkt in:

- `scope-inventory.md`
- `mapped-epics.md`
- `implemented-or-obsolete.md`
- `proposed-backlog.md`
- `README.md`

### BootManager_Epic8_Systeembeheer.docx

Status: verwerkt.

Resultaat:

- Bevestigt US8.1 t/m US8.14 volledig.
- Geen aanvullende user stories gevonden buiten de eerdere OCR-inventarisatie.
- Bevestigt dat BootManagerV2 al deels aansluit via bestaande Settings en operationele ingest/sampling configuratie.
- Bevestigt open systeemscope: eenheden, taal/regio, back-up/herstel UI, Raspberry Pi systeeminfo, systeemactielog, instellingen export/import en standaardinstellingen herstellen.
- Bevestigt dat gebruikersrollen, cloudaccounts, synchronisatieplanning en offline-sync toggle voorlopig geparkeerd blijven.

Verwerkt in:

- `scope-inventory.md`
- `mapped-epics.md`
- `implemented-or-obsolete.md`
- `proposed-backlog.md`

### BootManager_Epic9_Integratie.docx

Status: verwerkt.

Resultaat:

- Bevestigt US9.1 t/m US9.7 volledig.
- Lost de eerdere OCR-gap US9.1 t/m US9.5 op.
- US9.1 t/m US9.5 gaan over weerdata, AIS, Navionics/GPX-import, haveninformatie en Bluetooth/Wi-Fi sensoren.
- Bevestigt dat BootManagerV2 al deels aansluit via NMEA/YDEN ingest en raw/parser-herkenning van AIS `!AIVDM`/`!AIVDO`.
- Bevestigt open integratiescope: weer/getijden API, GPX/Navionics-import, haveninformatie, generiek API-sleutelbeheer, Bluetooth/Wi-Fi sensor onboarding en device-sync.
- Bevestigt dat lokale GPX-import en AIS-semantiek betere latere BootManagerV2-slices zijn dan brede externe API/cloud-sync.

Verwerkt in:

- `scope-inventory.md`
- `mapped-epics.md`
- `implemented-or-obsolete.md`
- `proposed-backlog.md`
- `README.md`

### BootManager_Epic10_Rapportage.docx

Status: verwerkt.

Resultaat:

- Bevestigt US10.1 t/m US10.6 volledig.
- Geen aanvullende user stories gevonden buiten de eerdere OCR-inventarisatie.
- Bevestigt dat rapportage module-overstijgend is en afhankelijk is van logboek, inventaris, onderhoud en passageplanning.
- Bevestigt dat BootManagerV2 alleen logboek browser-print deels afdekt; CSV/PDF-export, grafieken en analyse blijven open.
- Bevestigt dat brede rapportage pas zinvol is wanneer onderliggende modules betrouwbare historische data leveren.

Verwerkt in:

- `scope-inventory.md`
- `mapped-epics.md`
- `implemented-or-obsolete.md`
- `proposed-backlog.md`

### BootManager_Epic11_Notificaties.docx

Status: verwerkt.

Resultaat:

- Bevestigt US11.1 t/m US11.6 volledig.
- Geen aanvullende user stories gevonden buiten de eerdere OCR-inventarisatie.
- Bevestigt notificatiebronnen: lage voorraad, documentverval, onderhoud en passageplanning.
- Bevestigt notificatievormen: push, e-mail, dashboardindicatoren en kleuren.
- Bevestigt dat BootManagerV2 alleen in-app logboeksignalen deels heeft; generieke notificatiemodule, voorkeuren en geschiedenis ontbreken.
- Bevestigt dat push/e-mail later moeten komen dan een lokaal in-app waarschuwingenmodel.

Verwerkt in:

- `scope-inventory.md`
- `mapped-epics.md`
- `implemented-or-obsolete.md`
- `proposed-backlog.md`

### BootManager_Epic12_AI.docx

Status: verwerkt.

Resultaat:

- Bevestigt US12.1 t/m US12.6 volledig.
- Geen aanvullende user stories gevonden buiten de eerdere OCR-inventarisatie.
- Bevestigt AI-scope: barcode/QR-herkenning, fotoherkenning, automatische categorisatie, aanvulsuggesties, predictief onderhoud en spraakondersteuning.
- Bevestigt dat deze scope afhankelijk is van inventaris-, verbruiks- en onderhoudsdata.
- Bevestigt dat barcode/QR zonder AI eerder als inventarisfeature kan worden opgepakt dan brede AI-functionaliteit.

Verwerkt in:

- `scope-inventory.md`
- `mapped-epics.md`
- `implemented-or-obsolete.md`
- `proposed-backlog.md`

## Nog Te Verwerken

Geen. Alle beschikbare Word-exportbestanden zijn verwerkt.
