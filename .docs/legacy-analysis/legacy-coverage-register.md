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
| US1.2 Bootinformatie bewerken | Partial | Bootgegevens wijzigen in Settings geïmplementeerd op 2026-05-25 via `IVesselProfileService`; velden (VesselName, HomePort, CallSign, MMSI) kunnen nu post-onboarding worden aangepast. Launchpad-gerelateerde bootbeheer (gebieden, opslaglocaties) is open. |
| US1.3 Gebruikers aanmaken en rollen toewijzen | Parked | Multi-user buiten huidige single-owner scope. |
| US1.4 Inloggen als bestaande gebruiker | Parked | Multi-user buiten huidige single-owner scope. |
| US1.5 Meerdere boten beheren | Parked | BootManagerV2 gebruikt voorlopig single-vessel installatie. |
| US1.6 Boot selecteren bij opstart | Parked | Multi-boot geparkeerd. |
| US1.7 Gebruikersrechten wijzigen | Parked | Rollenbeheer geparkeerd. |
| US1.8 Gebruiker verwijderen | Parked | Rollenbeheer geparkeerd. |
| US1.9 Bootstructuurbeheer: gebieden en opslaglocaties | Open | Toekomstige inventaris/opslaglocatie-epic. |
| US1.10 Opslaglocatie aanmaken binnen gebied | Open | Toekomstige inventaris/opslaglocatie-epic. |
| US1.11 Opslaglocatie bewerken | Open | Toekomstige inventaris/opslaglocatie-epic. |
| US1.12 Tag genereren voor opslaglocatie | Open | Latere QR/tag-slice binnen inventaris. |
| US1.13 Locatie openen via QR-code | Open | Latere QR/tag-slice binnen inventaris. |
| US1.14 Tag opnieuw koppelen of vervangen | Open | Latere QR/tag-slice binnen inventaris. |
| US1.15 Overzicht van alle tags | Open | Latere QR/tag-slice binnen inventaris. |
| US1.16 Bootgegevens exporteren/importeren | Open | Later bij backup/restore of systeemconfiguratie. |
| US1.17 Toekomstige cloud-bootselectie | Parked | Cloud/multi-boot geparkeerd. |

## Epic 2: Inventarisbeheer

| Legacy US | Status | BootManagerV2 dekking / open punt |
|---|---|---|
| US2.1 Categorieen beheren | Open | Eerste inventory-slice kandidaat. |
| US2.2 Categorie-icoontjes beheren | Open | Kan starten met vaste iconenset; upload later. |
| US2.3 Product aanmaken | Open | Inventory-module ontbreekt. |
| US2.4 Product bewerken of verwijderen | Open | Inventory-module ontbreekt. |
| US2.5 Barcodes en QR-codes koppelen aan producten | Open | Latere scanning-slice. |
| US2.6 Barcode scannen bij zoeken | Open | Latere scanning-slice. |
| US2.7 Barcodeherkenning via foto en AI | Parked | AI-herkenning lage prioriteit. |
| US2.8 Product koppelen aan opslaglocatie | Open | Afhankelijk van producten en opslaglocaties. |
| US2.9 Voorraad bekijken per locatie | Open | Afhankelijk van voorraad per locatie. |
| US2.10 Voorraad aanpassen | Open | Voorraadmutaties ontbreken. |
| US2.11 Minimumvoorraad en waarschuwing | Open | Afhankelijk van inventory en notificaties. |
| US2.12 Zoeken en filteren | Open | Inventory-module ontbreekt. |
| US2.13 Voorraadlogboek | Open | Voorraadmutaties ontbreken. |
| US2.14 QR-scanner-modus | Open | Latere QR/location-flow. |
| US2.15 Bulkimport/export voorraad | Open | Later na datamodel. |
| US2.16 Voorraadstatus in dashboard | Open | Afhankelijk van inventory en dashboard. |
| US2.17 Integratie met passageplanning | Open | Afhankelijk van inventory en passageplanning. |
| US2.18 Productfoto of label | Open | Later na productcatalogus. |
| US2.19 Voorraad automatisch ophogen bij aankoop | Open | Afhankelijk van voorraadmutaties. |
| US2.20 Voorraad verminderen bij verbruik via barcode | Open | Afhankelijk van voorraadmutaties en scanning. |
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
| US5.3 Motoruren en brandstof in header | Partial | Reisheader/samenvatting aanwezig; motoruren/brandstof afronding open. Pi-databaseanalyse op 2026-05-31 bevestigt tankniveau/brandstof als kandidaat via `PCDIN`/`MXPGN` PGN `127505`; motoruren zijn nog niet duidelijk zichtbaar. |
| US5.4 Notities en gebeurtenissen toevoegen | Done | Opmerkingen/zeilvoering per logregel aanwezig. |
| US5.5 Logboek koppelen aan passage | Open | Passageplanning ontbreekt. |
| US5.6 Logboekheader invullen | Partial | Trip header bestaat; vertrek- en aankomstmoment ondersteunen nu datum+tijd in logboek en printweergave. Volledige legacy-header nog nalopen. |
| US5.7 Logregels met nautische velden | Partial | Veel velden en measurement suggestions aanwezig; barometer/temperatuur/legacy-volledigheid open. |
| US5.8 Bijlagen toevoegen aan logregel | Done | Logbook attachments aanwezig. |
| US5.9 Klassiek format en routekaart | Partial | Tabel/printweergave aanwezig; routekaart ontbreekt. |
| US5.10 Exporteren van logboek | Partial | Browser print aanwezig; PDF/CSV open. |
| US5.11 Statistieken en samenvatting | Partial | Samenvattingsvelden deels aanwezig; uitgebreide statistieken open. Pi-databaseanalyse op 2026-05-31 bevestigt `YDVLW` als kandidaat voor logstand/afstand en PGN `127505` als kandidaat voor tankniveau. |
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
| US7.1 Dashboardweergave openen | Partial | Dashboard-pagina en live meetwaarden-sectie (DSH-LIVE-1) zijn geïmplementeerd met SVG-gauges en auto-polling gekoppeld aan het ingest-sample-interval. Uitgebreide widgets, personalisatie en multi-boot dashboard ontbreken. |
| US7.2 Actieve bootinformatie | Partial | `VesselProfile` bestaat; live meetwaarden (wind, heading, position, speed, COG/SOG, diepte, watertemperatuur, spanning) tonen op dashboard (DSH-LIVE-1). Bootfoto, multi-boot selector en extra boot-metafoto ontbreken. |
| US7.3 Waarschuwingen en meldingen | Partial | Logboek missing-moments signaal bestaat; `SYS-CTRL-1` voegt dashboard- en logboekwaarschuwingen toe wanneer NMEA-/ingest-verwerking uit staat. Generiek meldingenpaneel ontbreekt nog. Dashboard toont "Geen data" voor ontbrekende meettypen. |
| US7.4 Weerinformatie en getijden | Open | Niet aanwezig. |
| US7.5 Widget voor voorraadstatus | Open | Afhankelijk van inventory. |
| US7.6 Widget voor onderhoudsstatus | Open | Afhankelijk van onderhoud. |
| US7.7 Widget voor documentstatus | Open | Afhankelijk van documentbeheer. |
| US7.8 Widget voor passageplanning | Open | Afhankelijk van passageplanning. |
| US7.9 Widget voor logboekactiviteit | Open | Niet als dashboardwidget aanwezig; logboek-recordbrowser bestaat. |
| US7.10 Personaliseren van widgets | Parked | Later; niet nodig voor eerste dashboard. |
| US7.11 Interactieve navigatie | Partial | Gewone navigatie bestaat; widget-clickthrough ontbreekt; live meetwaarden via SVG gauges en timestamps. |
| US7.12 Offline weergave | Partial | Lokale app werkt offline; dashboard-last-known model voorkomt crashes bij ontbrekende data. Auto-refresh toont "Geen data" voor niet-beschikbare waarden. |
| US7.13 Automatische update van gegevens | Partial | DSH-LIVE-1 voegt auto-polling toe via `System.Threading.Timer`, gekoppeld aan `DefaultSampleIntervalSeconds` met veilige grenzen. Geen SignalR/live push; polling is acceptabel voor MVP. |
| US7.14 Cloud-synchronisatie | Parked | Cloud-sync geparkeerd. |

---

## Epic 8: Systeembeheer & Configuratie

| Legacy US | Status | BootManagerV2 dekking / open punt |
|---|---|---|
| US8.1 Instellingenpagina openen | Done | Settings-pagina aanwezig. |
| US8.2 Eenheden configureren | Open | Niet aanwezig. |
| US8.3 Taal en regio instellen | Open | Niet aanwezig. |
| US8.4 Gebruikersrollen beheren | Parked | Multi-user/rollen geparkeerd. |
| US8.5 Sensorintegratie configureren | Partial | Operationele ingest settings aanwezig; `SYS-CTRL-1` voegt een centrale `IngestProcessingEnabled` instelling toe waarmee verwerking bewust aan/uit kan. `SYS-CTRL-2` maakt reload van ingest-instellingen robuuster tegen foutieve runtime `ApiBaseUrl` en laat capture logging de runtime/database setting respecteren. Pi-databaseanalyse op 2026-05-31 laat zien dat de huidige `Source`-waarde Docker/UDP-endpointmetadata is en niet stabiel genoeg is voor fysieke bronvoorkeuren. Bredere sensorconfig blijft open. |
| US8.6 Raspberry Pi-configuratie beheren | Partial | Pi deployment runbook en resourcechecks gedocumenteerd; eerste metingen voor opslag/RAM/load vastgelegd. Eerste echte Pi-veldtest met bootdata op 2026-05-29 bevestigde health, ingest, API, parsing en measurement-opslag op `master @ 1db5534`. `SYS-ANALYSIS-1` voegt een technische analysepagina toe; `SYS-CTRL-1` voegt dashboardbediening voor ingest-verwerking en goedkope disabled-mode toe; `SYS-CTRL-2` voorkomt dat een foutieve `ApiBaseUrl` de ingest reload-flow blijvend blokkeert. `SYS-SHUTDOWN-1` voegt een veilige in-app BootManager Pi shutdown-flow toe via een begrensde Unix-domain-socket naar een host-side systemd helper; deze is op 2026-05-31 gevalideerd op de Pi vanaf `master @ b7818f8`. Volledige in-app Pi systeemstatus/configuratiebeheer, persistent warning/error-overzicht en langdurige observatie blijven open. |
| US8.7 Gebruikersbeheer | Parked | Multi-user geparkeerd. |
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
| US9.5 Sensorintegratie via Bluetooth of Wi-Fi | Partial | UDP/YDEN ingest bestaat; Bluetooth/Wi-Fi sensor onboarding ontbreekt. Pi-databaseanalyse op 2026-05-31 onderbouwt dat bronidentiteit later op protocol/talker/message-id/PGN en gebruikerslabels moet leunen, niet alleen op remote endpoint. |
| US9.6 Externe API-verbinding beheren | Open | Niet aanwezig. |
| US9.7 Synchronisatie met andere apparaten | Parked | Device/cloud sync geparkeerd. |

## Epic 10: Rapportage & Analyse

| Legacy US | Status | BootManagerV2 dekking / open punt |
|---|---|---|
| US10.1 Brandstofanalyse | Open | Afhankelijk van motoruren/brandstofdata. |
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
| US12.1 Barcodeherkenning | Open | Kan later als niet-AI inventory/scanning-feature. |
| US12.2 AI-herkenning via foto | Parked | Lage prioriteit; afhankelijk van inventory. |
| US12.3 Automatische categorisatie | Parked | AI/lage prioriteit. |
| US12.4 Suggesties voor aanvulling | Parked | Afhankelijk van verbruiksdata/trends. |
| US12.5 Predictief onderhoud | Parked | Afhankelijk van onderhouds- en gebruiksdata. |
| US12.6 Spraakondersteuning | Parked | Lage prioriteit. |
