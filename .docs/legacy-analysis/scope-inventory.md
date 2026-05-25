# Legacy Scope Inventory

Bron: OCR van `.docs/legacy-input/BootmanagerEPICS.pdf` en globale inspectie van `BootNETManager`.

Aanvulling 2026-05-25:

- `BootManager_Softwarevisie_v0.7.docx` is als eerste Word-bron verwerkt.
- Dit bestand bevestigt de brede functionele scope uit de OCR-analyse.
- Technische architectuur uit deze visie is bewust niet overgenomen als richting voor BootManagerV2.

## Hoofdscope Uit Legacy PDF

De oude applicatievisie beschrijft BootManager als integraal boordbeheersysteem met:

- boot- en locatiebeheer;
- voorraadbeheer;
- passageplanning;
- documentbeheer;
- reislogboek;
- onderhoudsbeheer;
- gebruikersbeheer;
- offline werking met SQLite;
- optionele cloud/synchronisatie;
- rapportage, notificaties en AI/herkenning.

De Word-versie bevestigt aanvullend deze functionele accenten:

- meerdere boten, bootfoto's en visuele bootindeling waren legacy-scope;
- opslaglocaties zijn hiërarchisch bedoeld: gebied plus opslagruimte;
- voorraad bevat hoeveelheid, eenheid, houdbaarheidsdatum, locatie, foto, barcode/QR en export;
- passageplanning berekent benodigde voorraad op basis van reisduur, bemanning en verbruik;
- documenten hebben type, beschrijving, vervaldatum en lokale opslag;
- onderhoud bevat kosten, onderdelen, uitvoerder, schema's en herinneringen;
- offline werking, back-up naar USB/netwerkschijf en responsive gebruik waren expliciete niet-functionele doelen.

## Epic 0: Installatie & Authenticatie

Legacy user stories:

- US0.1 Installatie uitvoeren.
- US0.2 Registratie eerste eigenaar.
- US0.3 Inloggen als eigenaar.
- US0.4 Wachtwoord of pincode wijzigen.
- US0.5 Herstel van toegang.
- US0.6 Eigenaarprofiel beheren.

## Epic 1: Bootbeheer & Gebruikersbeheer

Legacy user stories:

- US1.1 Eerste opstart en bootaanmaak.
- US1.2 Bootinformatie bewerken.
- US1.3 Gebruikers aanmaken en rollen toewijzen.
- US1.4 Inloggen als bestaande gebruiker.
- US1.5 Meerdere boten beheren.
- US1.6 Boot selecteren bij opstart.
- US1.7 Gebruikersrechten wijzigen.
- US1.8 Gebruiker verwijderen.
- US1.14 Tag opnieuw koppelen of vervangen.
- US1.15 Overzicht van alle tags.
- US1.16 Bootgegevens exporteren/importeren.
- US1.17 Toekomstige cloud-bootselectie.

OCR-gap:

- US1.9 t/m US1.13 ontbreken waarschijnlijk in OCR. Op basis van context horen deze vermoedelijk bij gebieden/opslaglocaties/QR-tags.

## Epic 2: Inventarisbeheer

Legacy user stories zichtbaar in OCR:

- US2.3 Product aanmaken.
- US2.4 Product bewerken of verwijderen.
- US2.5 Barcodes en QR-codes koppelen aan producten.
- US2.6 Barcode scannen bij zoeken.
- US2.7 Barcodeherkenning via foto en AI.
- US2.8 Product koppelen aan opslaglocatie.
- US2.9 Voorraad bekijken per locatie.
- US2.10 Voorraad aanpassen.
- US2.11 Minimumvoorraad en waarschuwing.
- US2.12 Zoeken en filteren.
- US2.13 Voorraadlogboek.
- US2.14 QR-scanner-modus.
- US2.15 Bulkimport/export voorraad.
- US2.16 Voorraadstatus in dashboard.
- US2.17 Integratie met passageplanning.
- US2.18 Productfoto of label.
- US2.19 Voorraad automatisch ophogen bij aankoop.
- US2.20 Voorraad verminderen bij verbruik via barcode.
- US2.21 Cloud-synchronisatie.

OCR-gap:

- US2.1 en US2.2 ontbreken waarschijnlijk. Context wijst op categoriebeheer en/of opslaglocatiebasis.

## Epic 3: Passageplanning

Legacy user stories:

- US3.1 Passage aanmaken.
- US3.2 Bemanning toevoegen.
- US3.3 Benodigdheden berekenen met instellingen.
- US3.4 Vergelijking met voorraad.
- US3.5 Boodschappenlijst genereren.
- US3.6 Menu's plannen en beheren.
- US3.7 Documenten koppelen.
- US3.8 Statusdashboard.
- US3.9 Export reisplan.
- US3.10 Synchronisatie met logboek.
- US3.11 Herbruikbare passage templates.
- US3.12 Synchronisatie en offline modus.
- US3.13 Verbruiksinstellingen beheren.
- US3.14 Menu's koppelen aan verbruiksberekening.

## Epic 4: Documentbeheer

Legacy user stories zichtbaar in OCR:

- US4.2 Document bewerken of verwijderen.
- US4.3 Document koppelen aan boot, bemanningslid of passage.
- US4.4 Vervaldatum en waarschuwingen.
- US4.5 Documentstatusoverzicht.
- US4.6 Zoeken, filteren en sorteren.
- US4.7 Offline beschikbaarheid.
- US4.8 Cloud-synchronisatie.
- US4.9 Exporteren van documentlijst.
- US4.10 Documentgeschiedenis/audit trail.
- US4.11 Herinneringsinstellingen beheren.
- US4.12 Documenten koppelen aan passageplanning.

OCR-gap:

- US4.1 ontbreekt waarschijnlijk en is vermoedelijk document toevoegen/uploaden.

## Epic 5: Logboek

Legacy user stories zichtbaar in OCR:

- US5.2 Automatisch loggen en intervalinstelling.
- US5.3 Motoruren en brandstof in header.
- US5.4 Notities en gebeurtenissen toevoegen.
- US5.5 Logboek koppelen aan passage.
- US5.6 Logboekheader invullen.
- US5.7 Logregels met nautische velden.
- US5.8 Bijlagen toevoegen aan logregel.
- US5.9 Klassiek format en routekaart.
- US5.10 Exporteren van logboek.
- US5.11 Statistieken en samenvatting.
- US5.12 Offline werking.
- US5.13 Cloud-synchronisatie.
- US5.14 Logboek afronden bij aankomst.

OCR-gap:

- US5.1 ontbreekt waarschijnlijk en is vermoedelijk logboek/reis starten of aanmaken.

## Epic 6: Onderhoudsbeheer

Legacy user stories:

- US6.1 Onderhoudstaak aanmaken.
- US6.2 Onderhoud koppelen aan onderdeel.
- US6.3 Onderhoud op interval.
- US6.4 Automatische herinneringen en waarschuwingen.
- US6.5 Uitgevoerd onderhoud registreren.
- US6.6 Bijlagen toevoegen aan onderhoud.
- US6.7 Onderhoud wijzigen of verwijderen.
- US6.8 Onderhoudshistoriek per onderdeel.
- US6.9 Dashboard met onderhoudsstatus.
- US6.10 Zoeken en filteren.
- US6.11 Exporteren van onderhoudslogboek.
- US6.12 Koppeling met documentbeheer.
- US6.13 Offline werking.
- US6.14 Cloud-synchronisatie.

## Epic 7: Dashboard

Legacy user stories zichtbaar in OCR:

- US7.9 Widget voor logboekactiviteit.
- US7.10 Personaliseren van widgets.
- US7.11 Interactieve navigatie.
- US7.12 Offline weergave.
- US7.13 Automatische update van gegevens.
- US7.14 Cloud-synchronisatie.

OCR-gap:

- US7.1 t/m US7.8 ontbreken waarschijnlijk. Context wijst op dashboardwidgets voor voorraad, documenten, onderhoud, passage, systeemstatus en mogelijk weer/sensoren.

## Epic 8: Systeembeheer & Configuratie

Legacy user stories:

- US8.1 Instellingenpagina openen.
- US8.2 Eenheden configureren.
- US8.3 Taal en regio instellen.
- US8.4 Gebruikersrollen beheren.
- US8.5 Sensorintegratie configureren.
- US8.6 Raspberry Pi-configuratie beheren.
- US8.7 Gebruikersbeheer toevoegen/verwijderen.
- US8.8 Back-up maken en herstellen.
- US8.9 Cloudinstellingen beheren.
- US8.10 Automatische synchronisatie plannen.
- US8.11 Logboek van systeemacties bekijken.
- US8.12 Offline modus beheren.
- US8.13 Systeeminstellingen exporteren/importeren.
- US8.14 Standaardinstellingen herstellen.

## Epic 9: Integraties & Synchronisatie

Legacy user stories zichtbaar in OCR:

- US9.6 Externe API-verbinding beheren.
- US9.7 Synchronisatie met andere apparaten.

OCR-gap:

- US9.1 t/m US9.5 ontbreken waarschijnlijk. Context wijst op externe koppelingen/cloud/sync.

## Epic 10: Rapportage & Analyse

Legacy user stories:

- US10.1 Brandstofanalyse.
- US10.2 Voorraadanalyse.
- US10.3 Onderhoudsrapportage.
- US10.4 Kostenanalyse per tocht.
- US10.5 Export naar PDF/CSV.
- US10.6 Visuele trends en grafieken.

## Epic 11: Notificaties & Waarschuwingen

Legacy user stories:

- US11.1 Waarschuwing bij lage voorraad.
- US11.2 Documentvervalmelding.
- US11.3 Onderhoudsherinnering.
- US11.4 Passageplanning waarschuwing.
- US11.5 Instellingen voor notificaties beheren.
- US11.6 Notificatiegeschiedenis bekijken.

## Epic 12: Slimme Herkenning & AI-Ondersteuning

Legacy user stories:

- US12.1 Barcodeherkenning.
- US12.2 AI-herkenning via foto.
- US12.3 Automatische categorisatie.
- US12.4 Suggesties voor aanvulling.
- US12.5 Predictief onderhoud.
- US12.6 Spraakondersteuning.
