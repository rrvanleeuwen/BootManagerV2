# Legacy Coverage Register

Status: initiele coverage-registratie op 2026-05-25.

Doel: dit bestand voorkomt dat legacy-scope opnieuw geanalyseerd of dubbel als nieuwe user story opgepakt wordt. Bij iedere afgeronde BootManagerV2-functionaliteit moet Codex dit register controleren en zo nodig bijwerken.

## Statuslegenda

- `Done`: functioneel afgedekt in BootManagerV2.
- `Partial`: deels afgedekt, maar er blijft expliciete legacy-scope open.
- `Open`: nog relevant, niet geimplementeerd.
- `Parked`: bewust geparkeerd voor huidige roadmap.
- `Replaced`: legacy-aanpak is bewust vervangen door een BootManagerV2-aanpak.
- `Obsolete`: niet meer relevant in oude vorm.

## Onderhoudsregel

Bij afronding van een story of PR:

1. Controleer welke legacy US-nummers geraakt worden.
2. Werk de status hieronder bij.
3. Noteer kort welke BootManagerV2-functionaliteit de dekking levert.
4. Werk indien nodig ook `mapped-epics.md`, `implemented-or-obsolete.md` en `proposed-backlog.md` bij.
5. Als een story slechts deels gedekt is, laat de status `Partial` en benoem wat nog open blijft.

## Epic 0: Installatie & Authenticatie

| Legacy US | Status | BootManagerV2 dekking / open punt |
|---|---|---|
| US0.1 Installatie uitvoeren | Done | Eerste Raspberry Pi 4 Docker Compose deployment-smoke-test geslaagd op 2026-05-26: OS Lite 64-bit, SSH, GitHub SSH clone, `.env`, ARM64 Docker build, Web healthy, Ingest up, app via netwerk bereikbaar en reboot-test geslaagd. Productiehardening zoals backup/restore en veilige shutdown blijft onder systeembeheer open. |
| US0.2 Registratie eerste eigenaar | Replaced | Vervangen door bootstrap owner + verplichte onboarding. Legacy `/register-owner` route en menu-item verwijderd via PR #64 op 2026-05-26. |
| US0.3 Inloggen als eigenaar | Done | Wachtwoord-only login aanwezig. |
| US0.4 Wachtwoord of pincode wijzigen | Done | Wachtwoord wijzigen in Settings is technisch aanwezig en handmatig positief getest op 2026-05-25. Pincode is bewust uit de normale BootManagerV2-flow verwijderd/vervangen. |
| US0.5 Herstel van toegang | Replaced | Back-upcode/master-key vervangen door operationele resetprocedure via `scripts/reset-database.sh`. Op 2026-05-27 handmatig gevalideerd op Raspberry Pi op `master`: timestamped backup gemaakt, actieve database gereset, `/health` opnieuw OK, bootstrap login werkte, onboarding werd opnieuw afgedwongen en daarna werkte alleen het nieuw gekozen wachtwoord. |
| US0.6 Eigenaarprofiel beheren | Done | Wachtwoord wijzigen beschikbaar in Settings; bootgegevens wijzigen op 2026-05-25 via IVesselProfileService; eigenaargegevens (naam/e-mail) wijzigen in Settings geïmplementeerd op 2026-05-25 via IOwnerSettingsService met encrypted payload. Naam verplicht, e-mail optioneel. |

## Epic 1: Bootbeheer & Gebruikersbeheer

| Legacy US | Status | BootManagerV2 dekking / open punt |
|---|---|---|
| US1.1 Eerste opstart en bootaanmaak | Done | Afgedekt door onboarding + `VesselProfile`. |
| US1.2 Bootinformatie bewerken | Partial | Bootgegevens wijzigen in Settings geïmplementeerd via `IVesselProfileService`; naast VesselName, HomePort, CallSign en MMSI beheert `LOG-TRIP-AUTO-1A` nu ook actuele motoruren- en logstandwaarden met expliciete reset/leegmaakflow. Launchpad-gerelateerd bootbeheer (gebieden, opslaglocaties) blijft open. |
| US1.3 Gebruikers aanmaken en rollen toewijzen | Partial | `PILOT-AUTH-01` dekt lokale Crew-aanmaak met vaste Crew-rol af. Algemene rolwijziging, meerdere owners en volledige rollenmatrix blijven buiten scope. |
| US1.4 Inloggen als bestaande gebruiker | Done | `PILOT-AUTH-01` dekt lokale login voor bestaande Owner- en Crew-accounts af, inclusief actieve accountselector en wachtwoordcontrole. |
| US1.5 Meerdere boten beheren | Parked | BootManagerV2 gebruikt voorlopig single-vessel installatie. |
| US1.6 Boot selecteren bij opstart | Parked | Multi-boot geparkeerd. |
| US1.7 Gebruikersrechten wijzigen | Parked | Algemene rolwijziging blijft buiten `PILOT-AUTH-01`; alleen de vaste rollen Owner en Crew worden gebruikt. |
| US1.8 Gebruiker verwijderen | Parked | Definitief verwijderen blijft buiten `PILOT-AUTH-01`; uitschakelen wordt de pilotroute voor het intrekken van toegang. |
| US1.9 Bootstructuurbeheer: gebieden en opslaglocaties | Partial | `PILOT-LOC-01` levert persistent beheer van gebieden en opslaglocaties met stabiele locatie-id en gedeelde detailpagina. QR/tag- en voorraadfunctionaliteit blijven open. |
| US1.10 Opslaglocatie aanmaken binnen gebied | Done | `PILOT-LOC-01` dekt locatie aanmaken onder precies één gebied met naam en korte omschrijving af. |
| US1.11 Opslaglocatie bewerken | Done | `PILOT-LOC-01` dekt locatie hernoemen, beschrijving aanpassen, verplaatsen naar een ander gebied en verwijderen af. |
| US1.12 Tag genereren voor opslaglocatie | Done | `PILOT-LOC-02` levert stabiele unieke token- en QR-waarden per locatie; `PILOT-LOC-03` voegt Owner-only printvriendelijke QR-tags en scanbare PNG-export per locatie toe. |
| US1.13 Locatie openen via QR-code | Partial | `PILOT-LOC-02` levert scanrouting van bekende locatie-QR's naar de locatie-detailpagina voor Owner en Crew; producten en aantallen blijven later voor inventory. |
| US1.14 Tag opnieuw koppelen of vervangen | Done | `PILOT-LOC-04` dekt Owner-only tokenvervanging af: oude QR ongeldig, nieuwe QR direct actief voor dezelfde locatie. |
| US1.15 Overzicht van alle tags | Done | `PILOT-LOC-04` levert een Owner-only tagoverzicht met gebied, locatie, QR-waarde en handmatige tagstatus, plus directe navigatie via hoofdmenu `Opslag`. |
| US1.16 Bootgegevens exporteren/importeren | Open | Later bij backup/restore of systeemconfiguratie. |
| US1.17 Toekomstige cloud-bootselectie | Parked | Cloud/multi-boot geparkeerd. |

## Epic 2: Inventarisbeheer

| Legacy US | Status | BootManagerV2 dekking / open punt |
|---|---|---|
| US2.1 Categorieen beheren | Done | `PILOT-INV-01` levert lokaal beheer van categorieën met unieke naam, optionele omschrijving, vaste icoonset, archiveren en heractiveren voor Owner en Crew. |
| US2.2 Categorie-icoontjes beheren | Partial | `PILOT-INV-01` levert een vaste ingebouwde icoonset voor categorieën; upload of een vrije iconbibliotheek blijft later open. |
| US2.3 Product aanmaken | Done | `PILOT-INV-01` levert lokale productaanmaak met naam, categorie, standaardeenheid, optionele omschrijving en optionele gekoppelde code. |
| US2.4 Product bewerken of verwijderen | Done | `PILOT-INV-01` levert productbewerking plus soft delete via archiveren en heractiveren; harde delete is niet nodig voor deze pilotdekking. |
| US2.5 Barcodes en QR-codes koppelen aan producten | Done | `PILOT-INV-01` levert één unieke gekoppelde productcode per product via handmatige invoer in de catalogus; `PILOT-INV-03` voegt scan-gestuurde onbekende-code-afhandeling toe waarmee een gescande code direct aan een bestaand product kan worden gekoppeld of via nieuw product kan worden vastgelegd. |
| US2.6 Barcode scannen bij zoeken | Done | `PILOT-INV-04` dekt het aparte terugvindpad functioneel af: een bekende productcode in `Scannen` opent direct de product-terugvindflow met enkelvoudige directe navigatie of meervoudige locatielijst. |
| US2.7 Barcodeherkenning via foto en AI | Parked | AI-herkenning lage prioriteit. |
| US2.8 Product koppelen aan opslaglocatie | Done | `PILOT-INV-02` dekt voorraadregels per product-locatiecombinatie af via `Voorraad toevoegen` vanaf de locatiepagina, inclusief direct nieuw product aanmaken binnen dezelfde locatiecontext. |
| US2.9 Voorraad bekijken per locatie | Done | `PILOT-INV-02` toont actuele locatie-inhoud per opslaglocatie en laat op productniveau zien op welke locaties voorraad ligt met hoeveelheid en eenheid. |
| US2.10 Voorraad aanpassen | Done | `PILOT-INV-05` levert verbruik, tellingen en correcties op expliciete product-locatieregels met scan-gestuurde hoofdflow en administratieve fallback. |
| US2.11 Minimumvoorraad en waarschuwing | Open | Afhankelijk van inventory en notificaties. |
| US2.12 Zoeken en filteren | Partial | `PILOT-INV-04` levert eenvoudige handmatige zoekfallback in `Voorraadbeheer > Producten` op productnaam en omschrijving, hoofdletterongevoelig en met deelmatches. `PILOT-UX-01` voegt dezelfde basiszoeking toe aan home als dagelijkse pilot-ingang, inclusief productgerichte vervolgactie vanuit het zoekresultaat. Uitgebreide filters, categorie-/gebiedfilters en bredere zoekopties blijven open. |
| US2.13 Voorraadlogboek | Done | `PILOT-INV-05` voegt een aparte historiepagina toe met datum/tijd, type, product, gebied + locatie, oude hoeveelheid, nieuwe hoeveelheid, gebruiker en notitie. |
| US2.14 QR-scanner-modus | Done | `PILOT-SCAN-01` levert de generieke QR-/barcodebasis, `PILOT-LOC-02` voegt BootManager locatie-QR routing toe, `PILOT-SCAN-03` maakt `Scannen` de nieuwe centrale scan-ingang, `PILOT-SCAN-03A` levert de nieuwe productscanwerkcontext, `PILOT-SCAN-04` levert de nieuwe locatie-scanwerkcontext en `PILOT-SCAN-05` rondt de onbekende-code-flow af met nieuwe keuzes, nieuwe vervolgroutes en geen zichtbare legacy-terugval. |
| US2.15 Bulkimport/export voorraad | Open | Later na datamodel. |
| US2.16 Voorraadstatus in dashboard | Open | Afhankelijk van inventory en dashboard. |
| US2.17 Integratie met passageplanning | Open | Afhankelijk van inventory en passageplanning. |
| US2.18 Productfoto of label | Open | Later na productcatalogus. |
| US2.19 Voorraad automatisch ophogen bij aankoop | Partial | `PILOT-INV-02` vult een bestaande product-locatieregel additief aan wanneer op dezelfde locatie opnieuw voorraad wordt toegevoegd. Een expliciete aankoopflow, mutatietypes en historie ontbreken nog. |
| US2.20 Voorraad verminderen bij verbruik via barcode | Partial | `PILOT-INV-05` ondersteunt verbruik via de scanflow `product -> Voorraadbijzonderheid -> locatie -> mutatie`, met expliciete locatiecontext en blokkade op oververbruik. Verdere automatisering en bredere barcodeverbruikpaden blijven open. |
| US2.21 Cloud-synchronisatie | Parked | Cloud-sync geparkeerd. |

## Epic 3: Passageplanning

| Legacy US | Status | BootManagerV2 dekking / open punt |
|---|---|---|
| US3.1 Passage aanmaken | Open | Passageplanningmodule ontbreekt. |
| US3.2 Bemanning toevoegen | Open | Kan als passage-data zonder multi-user accounts. |
| US3.3 Benodigdheden berekenen | Open | Afhankelijk van inventory/verbruiksprofielen. |
| US3.4 Vergelijking met voorraad | Open | Afhankelijk van inventory. |
| US3.5 Boodschappenlijst genereren | Open | Afhankelijk van voorraadvergelijking. |
| US3.6 Menu's plannen en beheren | Open | Afhankelijk van passageplanning/inventory. |
| US3.7 Documenten koppelen | Open | Afhankelijk van documentbeheer. |
| US3.8 Statusdashboard | Open | Afhankelijk van passageplanning en dashboard. |
| US3.9 Export reisplan | Open | Later na passagebasis. |
| US3.10 Synchronisatie met logboek | Open | BootManagerV2 heeft logbook trips, maar geen passagekoppeling. |
| US3.11 Herbruikbare passage templates | Open | Later na passagebasis. |
| US3.12 Synchronisatie en offline modus | Parked | Cloud-sync geparkeerd; lokale offline werking blijft uitgangspunt. |
| US3.13 Verbruiksinstellingen beheren | Open | Later bij passageberekening/settings. |
| US3.14 Menu's koppelen aan verbruiksberekening | Open | Afhankelijk van menuplanning/inventory. |

## Epic 4: Documentbeheer

| Legacy US | Status | BootManagerV2 dekking / open punt |
|---|---|---|
| US4.1 Document toevoegen en categoriseren | Open | Algemene documentkluis ontbreekt. |
| US4.2 Document bewerken of verwijderen | Open | Algemene documentkluis ontbreekt. |
| US4.3 Document koppelen aan boot, bemanningslid of passage | Open | Afhankelijk van documentbeheer/passage. |
| US4.4 Vervaldatum en waarschuwingen | Open | Afhankelijk van documentbeheer/notificaties. |
| US4.5 Documentstatusoverzicht | Open | Afhankelijk van documentbeheer/dashboard. |
| US4.6 Zoeken, filteren en sorteren | Open | Algemene documentkluis ontbreekt. |
| US4.7 Offline beschikbaarheid | Open | Moet onderdeel zijn van documentkluis. |
| US4.8 Cloud-synchronisatie | Parked | Cloud-sync geparkeerd. |
| US4.9 Exporteren van documentlijst | Open | Later na documentbasis. |
| US4.10 Documentgeschiedenis/audit trail | Open | Later na documentbasis. |
| US4.11 Herinneringsinstellingen beheren | Open | Afhankelijk van document/notificaties. |
| US4.12 Documenten koppelen aan passageplanning | Open | Afhankelijk van documentbeheer/passageplanning. |
| US4.13 Document openen, printen of delen | Open | Openen/downloaden hoort bij documentbasis; mailen/delen later. |

## Epic 5: Logboek

| Legacy US | Status | BootManagerV2 dekking / open punt |
|---|---|---|
| US5.1 Handmatig logboek invoeren met weerinformatie | Partial | Handmatige logregels bestaan; weer/barometer/temperatuur volledig nalopen. |
| US5.2 Automatisch loggen en intervalinstelling | Partial | Missing moments + Draft-regels bestaan; echte automatische logging op NMEA interval is beperkt. `SYS-CTRL-1` voegt bewuste ingest-verwerking aan/uit toe met logboekwaarschuwing en nieuwe-reis-popup wanneer automatische meetdata uit staat. |
| US5.3 Motoruren en brandstof in header | Partial | `LOG-TRIP-AUTO-1A` dekt motoruren start/eind, actuele motorurenstand in bootinstellingen, expliciete overname en voortschrijving van hogere waarden af. Brandstof/tankniveau is technisch beschikbaar via Fluid Level en dashboard, maar nog niet geïntegreerd in de logboekheader. |
| US5.4 Notities en gebeurtenissen toevoegen | Done | Opmerkingen/zeilvoering per logregel aanwezig. |
| US5.5 Logboek koppelen aan passage | Open | Passageplanning ontbreekt. |
| US5.6 Logboekheader invullen | Partial | Trip header ondersteunt vertrek/aankomst met datum+tijd. `LOG-TRIP-AUTO-1A` voegt bootnaam- en boordtijddefaults, expliciete tellerstandovername, logstand eind, berekende gelogde mijlen en consistente printweergave toe. Volledige legacy-header blijft nog open. |
| US5.7 Logregels met nautische velden | Partial | Veel velden en measurement suggestions aanwezig; barometer/temperatuur/legacy-volledigheid open. |
| US5.8 Bijlagen toevoegen aan logregel | Done | Logbook attachments aanwezig. |
| US5.9 Klassiek format en routekaart | Partial | Tabel/printweergave aanwezig; routekaart ontbreekt. |
| US5.10 Exporteren van logboek | Partial | Browser print aanwezig; PDF/CSV open. |
| US5.11 Statistieken en samenvatting | Partial | `LOG-TRIP-AUTO-1A` berekent reisduur en gelogde mijlen uit logstand begin/eind en toont deze in logboek en print. Uitgebreide statistieken, `YDVLW`-automatisering, tank/verbruik en bredere rapportage blijven open. |
| US5.12 Offline werking | Done | Lokale SQLite-first werking. |
| US5.13 Cloud-synchronisatie | Parked | Cloud-sync geparkeerd. |
| US5.14 Logboek afronden bij aankomst | Done | Lopende reizen kunnen nu expliciet administratief worden afgerond met verplicht aankomstmoment; afgesloten reizen krijgen status `Completed` en worden niet meer als open reis behandeld in logboekflow. |

## Epic 6: Onderhoudsbeheer

| Legacy US | Status | BootManagerV2 dekking / open punt |
|---|---|---|
| US6.1 Onderhoudstaak aanmaken | Open | Onderhoudsmodule ontbreekt. |
| US6.2 Onderhoud koppelen aan onderdeel | Open | Onderhoudsmodule ontbreekt. |
| US6.3 Onderhoud op interval | Open | Tijdsinterval kan eerst; gebruiksinterval later. |
| US6.4 Automatische herinneringen en waarschuwingen | Open | Afhankelijk van onderhoud/notificaties. |
| US6.5 Uitgevoerd onderhoud registreren | Open | Onderhoudsmodule ontbreekt. |
| US6.6 Bijlagen toevoegen aan onderhoud | Open | Afhankelijk van onderhoud/documentbeheer. |
| US6.7 Onderhoud wijzigen of verwijderen | Open | Onderhoudsmodule ontbreekt. |
| US6.8 Onderhoudshistoriek per onderdeel | Open | Onderhoudsmodule ontbreekt. |
| US6.9 Dashboard met onderhoudsstatus | Open | Afhankelijk van onderhoud/dashboard. |
| US6.10 Zoeken en filteren | Open | Onderhoudsmodule ontbreekt. |
| US6.11 Exporteren van onderhoudslogboek | Open | Later na onderhoudbasis. |
| US6.12 Koppeling met documentbeheer | Open | Afhankelijk van onderhoud/documentbeheer. |
| US6.13 Offline werking | Open | Moet onderdeel zijn van lokale onderhoudsmodule. |
| US6.14 Cloud-synchronisatie | Parked | Cloud-sync geparkeerd. |

## Epic 7: Dashboard

| Legacy US | Status | BootManagerV2 dekking / open punt |
|---|---|---|
| US7.1 Dashboardweergave openen | Partial | Dashboard-pagina en live meetwaarden-sectie (DSH-LIVE-1) zijn geïmplementeerd met SVG-gauges en auto-polling gekoppeld aan het ingest-sample-interval. `DSH-LIVE-5` toont alleen beschikbare meetwaarden en laat tegels verbergen/herstellen. `DSH-BUG-DBCTX-1` isoleert dashboardqueries per operatie en voorkomt concurrencyfouten bij polling en navigatie. Uitgebreide widgets, personalisatie en multi-boot dashboard ontbreken. |
| US7.2 Actieve bootinformatie | Partial | `VesselProfile` bestaat; live meetwaarden (wind, heading, position, speed, COG/SOG, diepte, watertemperatuur, spanning) tonen op dashboard (DSH-LIVE-1). `DSH-LIVE-5` voegt tankniveaus toe voor aanwezige `FluidLevelMeasurements`. Bootfoto, multi-boot selector en extra boot-metafoto ontbreken. |
| US7.3 Waarschuwingen en meldingen | Partial | Logboek missing-moments signaal bestaat; `SYS-CTRL-1` voegt dashboard- en logboekwaarschuwingen toe wanneer NMEA-/ingest-verwerking uit staat. Generiek meldingenpaneel ontbreekt nog. Dashboard toont "Geen data" voor ontbrekende meettypen. |
| US7.4 Weerinformatie en getijden | Open | Niet aanwezig. |
| US7.5 Widget voor voorraadstatus | Open | Afhankelijk van inventory. |
| US7.6 Widget voor onderhoudsstatus | Open | Afhankelijk van onderhoud. |
| US7.7 Widget voor documentstatus | Open | Afhankelijk van documentbeheer. |
| US7.8 Widget voor passageplanning | Open | Afhankelijk van passageplanning. |
| US7.9 Widget voor logboekactiviteit | Open | Niet als dashboardwidget aanwezig; logboek-recordbrowser bestaat. |
| US7.10 Personaliseren van widgets | Parked | Later; niet nodig voor eerste dashboard. |
| US7.11 Interactieve navigatie | Partial | Gewone navigatie bestaat; dashboardtegels kunnen met `DSH-LIVE-5` interactief verborgen en teruggezet worden. `PILOT-UX-01` voegt op home directe doorkliktegels naar `Logboek`, `Dashboard` en `Scannen` toe plus productgerichte doorklik vanuit de home-zoekresultaten. Widget-clickthrough vanaf dashboard zelf ontbreekt nog. |
| US7.12 Offline weergave | Partial | Lokale app werkt offline; dashboard-last-known model voorkomt crashes bij ontbrekende data. `DSH-LIVE-5` toont geen lege tegels voor niet-beschikbare waarden en bewaart verborgen tegelkeuzes lokaal in de browser. |
| US7.13 Automatische update van gegevens | Partial | DSH-LIVE-1 voegt auto-polling toe, gekoppeld aan `DefaultSampleIntervalSeconds` met veilige grenzen. `DSH-BUG-DBCTX-1` vervangt de overlappende async `System.Threading.Timer`-callback door een sequentiële, annuleerbare `PeriodicTimer`-loop en gebruikt een eigen context per dashboardload. `DSH-LIVE-5` behoudt deze refresh-flow voor zichtbare en verborgen beschikbare meetwaarden. Geen SignalR/live push; polling is acceptabel voor MVP. |
| US7.14 Cloud-synchronisatie | Parked | Cloud-sync geparkeerd. |

---

## Epic 8: Systeembeheer & Configuratie

| Legacy US | Status | BootManagerV2 dekking / open punt |
|---|---|---|
| US8.1 Instellingenpagina openen | Done | Settings-pagina aanwezig. |
| US8.2 Eenheden configureren | Open | Niet aanwezig. |
| US8.3 Taal en regio instellen | Open | Niet aanwezig. |
| US8.4 Gebruikersrollen beheren | Parked | `PILOT-AUTH-01` introduceert vaste Owner/Crew-rollen, maar geen algemene rolwijziging of rollenmatrix. |
| US8.5 Sensorintegratie configureren | Partial | Operationele ingest settings aanwezig; `SYS-CTRL-1` voegt een centrale `IngestProcessingEnabled` instelling toe waarmee verwerking bewust aan/uit kan. `SYS-CTRL-2` maakt reload van ingest-instellingen robuuster tegen foutieve runtime `ApiBaseUrl` en laat capture logging de runtime/database setting respecteren. Pi-databaseanalyse op 2026-05-31 laat zien dat de huidige `Source`-waarde Docker/UDP-endpointmetadata is en niet stabiel genoeg is voor fysieke bronvoorkeuren. Story 8 legt vast dat bronidentiteit uit NMEA-inhoud moet komen, niet uit UDP/YDEN transport. Story 9 past dat uitgangspunt toe voor Fluid Level gatewayberichten door PGN/tanktype/instance uit de NMEA-inhoud te gebruiken. Bredere sensorconfig en bronvoorkeuren-UI blijven open. |
| US8.6 Raspberry Pi-configuratie beheren | Partial | Pi deployment runbook en resourcechecks gedocumenteerd; eerste metingen voor opslag/RAM/load vastgelegd. Eerste echte Pi-veldtest met bootdata op 2026-05-29 bevestigde health, ingest, API, parsing en measurement-opslag op `master @ 1db5534`. `SYS-ANALYSIS-1` voegt een technische analysepagina toe; `SYS-CTRL-1` voegt dashboardbediening voor ingest-verwerking en goedkope disabled-mode toe; `SYS-CTRL-2` voorkomt dat een foutieve `ApiBaseUrl` de ingest reload-flow blijvend blokkeert. `SYS-SHUTDOWN-1` voegt een veilige in-app BootManager Pi shutdown-flow toe via een begrensde Unix-domain-socket naar een host-side systemd helper; deze is op 2026-05-31 gevalideerd op de Pi vanaf `master @ b7818f8`. Volledige in-app Pi systeemstatus/configuratiebeheer, persistent warning/error-overzicht en langdurige observatie blijven open. |
| US8.7 Gebruikersbeheer | Partial | `PILOT-AUTH-01` dekt Crew toevoegen, wachtwoord resetten, toegang uitschakelen en opnieuw activeren af. Definitief verwijderen, algemene rolwijziging en uitgebreide beheerflows blijven buiten scope. |
| US8.8 Back-up maken en herstellen | Open | Procedureel beschreven; UI/helper open. |
| US8.9 Cloudinstellingen beheren | Parked | Cloud-sync geparkeerd. |
| US8.10 Automatische synchronisatie plannen | Parked | Cloud-sync geparkeerd. |
| US8.11 Logboek van systeemacties bekijken | Open | Niet aanwezig als persistent systeemactie-logboek. `SYS-ANALYSIS-1` toont technische analyse en export; `SYS-CTRL-1` toont operationele waarschuwingen; `SYS-CTRL-2` verbetert runtime logging rond reload/capture logging, maar warnings/errors en systeemacties worden nog niet duurzaam als logboek opgeslagen. |
| US8.12 Offline modus beheren | Parked | Pas zinvol bij sync; lokale-first werking bestaat. |
| US8.13 Systeeminstellingen exporteren/importeren | Open | Niet aanwezig. |
| US8.14 Standaardinstellingen herstellen | Open | Niet aanwezig. |

## Epic 9: Integraties & Synchronisatie

| Legacy US | Status | BootManagerV2 dekking / open punt |
|---|---|---|
| US9.1 Weerdata koppelen | Open | Niet aanwezig. |
| US9.2 AIS integratie | Partial | Raw/parser-herkenning voor `!AIVDM`/`!AIVDO`; AIS-semantiek ontbreekt. |
| US9.3 Navionics/GPX import | Open | Niet aanwezig; GPX is logische lokale eerste slice. |
| US9.4 Haveninformatie koppelen | Open | Niet aanwezig. |
| US9.5 Sensorintegratie via Bluetooth of Wi-Fi | Partial | UDP/YDEN ingest bestaat; Bluetooth/Wi-Fi sensor onboarding ontbreekt. Pi-databaseanalyse op 2026-05-31 onderbouwt dat bronidentiteit later op protocol/talker/message-id/PGN en gebruikerslabels moet leunen, niet op remote endpoint; Story 8 werkt dit uit als transportbron versus databron. |
| US9.6 Externe API-verbinding beheren | Open | Niet aanwezig. |
| US9.7 Synchronisatie met andere apparaten | Parked | Device/cloud sync geparkeerd. |

## Epic 10: Rapportage & Analyse

| Legacy US | Status | BootManagerV2 dekking / open punt |
|---|---|---|
| US10.1 Brandstofanalyse | Partial | Story 9 legt de eerste technische basis voor brandstof-/tankniveau-opslag via PGN `127505` Fluid Level. `DSH-LIVE-5` toont tankniveaus op het dashboard. Analyse, trends, logboekgebruik en motoruren blijven open. |
| US10.2 Voorraadanalyse | Open | Afhankelijk van inventorymutaties. |
| US10.3 Onderhoudsrapportage | Open | Afhankelijk van onderhoudsmodule. |
| US10.4 Kostenanalyse per tocht | Open | Afhankelijk van passage/logboek kostenregistratie. |
| US10.5 Export naar PDF/CSV | Partial | Logboek browser print bestaat; echte PDF/CSV open. |
| US10.6 Visuele trends en grafieken | Open | Niet aanwezig. |

## Epic 11: Notificaties & Waarschuwingen

| Legacy US | Status | BootManagerV2 dekking / open punt |
|---|---|---|
| US11.1 Waarschuwing bij lage voorraad | Open | Afhankelijk van inventory. |
| US11.2 Documentvervalmelding | Open | Afhankelijk van documentbeheer. |
| US11.3 Onderhoudsherinnering | Open | Afhankelijk van onderhoud. |
| US11.4 Passageplanning waarschuwing | Open | Afhankelijk van passageplanning. |
| US11.5 Instellingen voor notificaties beheren | Open | Notificatiemodule ontbreekt. |
| US11.6 Notificatiegeschiedenis bekijken | Open | Notificatiemodule ontbreekt. |

## Epic 12: Slimme Herkenning & AI-Ondersteuning

| Legacy US | Status | BootManagerV2 dekking / open punt |
|---|---|---|
| US12.1 Barcodeherkenning | Partial | Niet-AI herkenning is in `/scan` met ZXing voor QR en native `BarcodeDetector` voor EAN-13 op de Pi en beide telefoons in Edge en Chrome geaccepteerd; productintegratie volgt in de inventarispilotstories. |
| US12.2 AI-herkenning via foto | Parked | Lage prioriteit; afhankelijk van inventory. |
| US12.3 Automatische categorisatie | Parked | AI/lage prioriteit. |
| US12.4 Suggesties voor aanvulling | Parked | Afhankelijk van verbruiksdata/trends. |
| US12.5 Predictief onderhoud | Parked | Afhankelijk van onderhouds- en gebruiksdata. |
| US12.6 Spraakondersteuning | Parked | Lage prioriteit. |
