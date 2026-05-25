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

Word-bron verwerkt:

- `BootManager_Epic0_Installatie_Authenticatie.docx`
- Status: volledig leesbaar, geen OCR-gap voor Epic 0.

Legacy user stories:

- US0.1 Installatie uitvoeren.
- US0.2 Registratie eerste eigenaar.
- US0.3 Inloggen als eigenaar.
- US0.4 Wachtwoord of pincode wijzigen.
- US0.5 Herstel van toegang.
- US0.6 Eigenaarprofiel beheren.

Functionele nuance uit Word-bron:

- US0.1 richtte zich op installatie op Raspberry Pi of laptop en daarna lokale setup starten.
- US0.2 vroeg oorspronkelijk om naam, e-mail en wachtwoord/pincode; BootManagerV2 heeft dit vervangen door bootstrap owner plus onboarding.
- US0.3 verwachtte laden van persoonlijke boten; BootManagerV2 gebruikt voorlopig een single-vessel/single-owner model.
- US0.4 blijft relevant voor wachtwoord wijzigen; pincode is niet langer normale scope.
- US0.5 back-upcode/beheersleutel is vervangen door operationele resetprocedure.
- US0.6 eigenaarprofiel beheren blijft relevant en is opgenomen in `Owner Profile & Vessel Settings`.

## Epic 1: Bootbeheer & Gebruikersbeheer

Word-bron verwerkt:

- `BootManager_Epic1_Bootbeheer_en_Gebruikersbeheer.docx`
- Status: volledig leesbaar, eerdere OCR-gap US1.9 t/m US1.13 is opgelost.

Legacy user stories:

- US1.1 Eerste opstart en bootaanmaak.
- US1.2 Bootinformatie bewerken.
- US1.3 Gebruikers aanmaken en rollen toewijzen.
- US1.4 Inloggen als bestaande gebruiker.
- US1.5 Meerdere boten beheren.
- US1.6 Boot selecteren bij opstart.
- US1.7 Gebruikersrechten wijzigen.
- US1.8 Gebruiker verwijderen.
- US1.9 Bootstructuurbeheer: gebieden en opslaglocaties.
- US1.10 Opslaglocatie aanmaken binnen gebied.
- US1.11 Opslaglocatie bewerken.
- US1.12 Tag genereren voor opslaglocatie.
- US1.13 Locatie openen via QR-code.
- US1.14 Tag opnieuw koppelen of vervangen.
- US1.15 Overzicht van alle tags.
- US1.16 Bootgegevens exporteren/importeren.
- US1.17 Toekomstige cloud-bootselectie.

Functionele nuance uit Word-bron:

- Bootprofiel bevat naam, type, bouwjaar, afmetingen en foto.
- Boot verwijderen was legacy-scope, maar vraagt in BootManagerV2 extra voorzichtigheid vanwege single-vessel installatie.
- Multi-user rollen zijn eigenaar, bemanning en alleen-lezen.
- Multi-boot-context laadt voorraad, documenten en andere gegevens per boot.
- Bootstructuur is hiërarchisch: gebied -> opslaglocatie.
- QR-tags verwijzen naar opslaglocatie-ID's en kunnen worden geprint/geexporteerd.
- Scannen van QR-code opent de detailpagina van de opslaglocatie met actuele voorraad.

## Epic 2: Inventarisbeheer

Word-bron verwerkt:

- `BootManager_Epic2_Inventarisbeheer.docx`
- Status: volledig leesbaar, eerdere OCR-gap US2.1 en US2.2 is opgelost.

Legacy user stories:

- US2.1 Categorieën beheren.
- US2.2 Categorie-icoontjes beheren.
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

Functionele nuance uit Word-bron:

- Inventarisbeheer is bedoeld voor eigenaar en bemanning, met lokale/offline werking als basis.
- Productcategorieën hebben naam, korte omschrijving en een icoon; icoontjes kunnen uit een bibliotheek komen of als PNG/SVG worden toegevoegd.
- Producten bevatten naam, omschrijving, categorie, eenheid, minimumvoorraad, barcode(s), optioneel foto/label en opslaglocatie.
- Een product kan aan één of meerdere opslaglocaties gekoppeld worden met hoeveelheid per locatie.
- Voorraadmutaties moeten worden gelogd met datum, gebruiker, product, oude en nieuwe hoeveelheid.
- QR-scanner-modus richt zich primair op opslaglocaties: scan opent de voorraadlijst van die locatie en laat direct aantallen aanpassen.
- Barcode-scanning richt zich primair op productherkenning, zoeken en verbruiksregistratie.
- Passageplanning-integratie berekent later benodigdheden op basis van reisduur en aantal personen.
- AI-herkenning en cloud-synchronisatie zijn expliciet toekomstfunctionaliteit.

## Epic 3: Passageplanning

Word-bron verwerkt:

- `BootManager_Epic3_PassagePlanning.docx`
- Status: volledig leesbaar, geen OCR-gap voor Epic 3.

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

Functionele nuance uit Word-bron:

- Passageplanning is bedoeld voor voorbereiding van zeiltochten met vertrekdatum, bestemming(en), duur en opvarenden.
- Bemanningslijst bevat volledige naam en geboortedatum en moet later in exportdocumenten terechtkomen.
- Benodigdhedenberekening hangt af van verbruiksprofielen: brandstof per motoruur, water per persoon per dag en voedsel via menuplanning.
- Voorraadvergelijking en boodschappenlijst zijn expliciet afhankelijk van Epic 2 Inventarisbeheer.
- Documentkoppeling en waarschuwingen zijn afhankelijk van een algemene documentmodule.
- Menuplanning werkt per dag en maaltijdtype met gerechten, productkoppelingen en hoeveelheden.
- Logboekkoppeling hoort bij het starten van de reis: positie, tijd en verbruik worden binnen dezelfde passagecontext vastgelegd.
- Cloud-synchronisatie blijft toekomstfunctionaliteit; offline beschikbaarheid is wel kernverwachting.

## Epic 4: Documentbeheer

Word-bron verwerkt:

- `BootManager_Epic4_Documentbeheer.docx`
- Status: volledig leesbaar, eerdere OCR-gap US4.1 is opgelost; Word-bron bevat ook US4.13.

Legacy user stories:

- US4.1 Document toevoegen en categoriseren.
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
- US4.13 Document openen, printen of delen.

Functionele nuance uit Word-bron:

- Documentbeheer is een algemene documentkluis voor scheeps- en reisgerelateerde documenten, niet hetzelfde als logboekbijlagen.
- Ondersteunde bestandstypen waren breed bedoeld: PDF, JPG, PNG, DOCX enzovoort.
- Documenten hebben naam, beschrijving, categorie, optionele vervaldatum en status.
- Categorie kan bestaand zijn of direct tijdens upload nieuw worden aangemaakt met naam, beschrijving en optioneel icoon.
- Documenten kunnen gekoppeld worden aan boot, bemanningslid of passage.
- Vervaldatumwaarschuwingen gebruiken een instelbare herinneringstermijn, bijvoorbeeld 14, 30 of 60 dagen.
- Documentstatus omvat geldig, bijna verlopen en verlopen.
- Offline beschikbaarheid is kernscope; cloud-synchronisatie is toekomstscope.
- Audit trail registreert documentacties zoals toevoegen, wijzigen, verwijderen en openen/delen.
- Delen via mail is afhankelijk van internet en hoort niet bij de eerste lokale/offline slice.

## Epic 5: Logboek

Word-bron verwerkt:

- `BootManager_Epic5_Logboek.docx`
- Status: volledig leesbaar, eerdere OCR-gap US5.1 is opgelost.

Legacy user stories:

- US5.1 Handmatig logboek invoeren met weerinformatie.
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

Functionele nuance uit Word-bron:

- Het logboek is expliciet een nautisch journaal tijdens een passage en hoort later aan passageplanning gekoppeld te worden.
- Handmatige logregels bevatten tijd, positie, koers, snelheid, wind en opmerkingen.
- Automatische logging gebruikt GPS/NMEA-data voor tijd, positie, koers, snelheid en windinformatie op instelbaar interval.
- Header bevat vertrek, aankomst, bemanning, brandstof, motoruren, logstand en tijdsduur.
- Per logregel horen windrichting, windsnelheid, temperatuur, barometer en opmerkingen tot de legacy-scope.
- Samenvatting berekent later reisduur, afstand, gemiddelde snelheid, brandstofverbruik en windstatistieken.
- Klassiek nautisch format vraagt ook een routekaart; BootManagerV2 heeft nu wel tabel/print, maar nog geen kaart.
- Offline lokale opslag is kernscope; cloud-synchronisatie is toekomstscope.
- Afronden bij aankomst vraagt een expliciete flow met aankomsthaven, motoruren-eind, brandstofniveau, eind-logstand en totale afstand.

## Epic 6: Onderhoudsbeheer

Word-bron verwerkt:

- `BootManager_Epic6_OnderhoudsbeheerL.docx`
- Status: volledig leesbaar, geen OCR-gap voor Epic 6.

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

Functionele nuance uit Word-bron:

- Onderhoudsbeheer is een technisch logboek voor gepland, aankomend en uitgevoerd onderhoud.
- Taken bevatten naam, beschrijving, onderdeel, prioriteit, status, verwachte uitvoerdatum en interval.
- Intervallen kunnen gebaseerd zijn op tijd of gebruik, zoals draaiuren; vaarkilometers waren ook legacy-scope.
- Onderdelen omvatten bijvoorbeeld motor, romp, zeilen, accu's, elektronica en filters.
- Uitgevoerd onderhoud bevat datum, omschrijving, kosten, monteur, gebruikte onderdelen en bijlagen.
- Dashboard toont openstaande, bijna vervallende en uitgevoerde taken per status en onderdeel.
- Documentbeheer is nodig voor facturen, handleidingen en certificaten bij onderhoud.
- Herinneringen/waarschuwingen hangen samen met de notificatie-epic.
- Offline lokale werking is kernscope; cloud-synchronisatie is toekomstscope.

## Epic 7: Dashboard

Word-bron verwerkt:

- `BootManager_Epic7_Dashboard.docx`
- Status: volledig leesbaar, eerdere OCR-gap US7.1 t/m US7.8 is opgelost.

Legacy user stories:

- US7.1 Dashboardweergave openen.
- US7.2 Actieve bootinformatie.
- US7.3 Waarschuwingen en meldingen.
- US7.4 Weerinformatie en getijden.
- US7.5 Widget voor voorraadstatus.
- US7.6 Widget voor onderhoudsstatus.
- US7.7 Widget voor documentstatus.
- US7.8 Widget voor passageplanning.
- US7.9 Widget voor logboekactiviteit.
- US7.10 Personaliseren van widgets.
- US7.11 Interactieve navigatie.
- US7.12 Offline weergave.
- US7.13 Automatische update van gegevens.
- US7.14 Cloud-synchronisatie.

Functionele nuance uit Word-bron:

- Dashboard is de centrale startpagina voor bootstatus, voorraad, onderhoud, passage, documenten, logboek, waarschuwingen en weer.
- Bootstatus bevat actieve boot, locatie, datum, tijd, stroomvoorziening en netwerkstatus.
- Actieve bootinformatie verwacht naam, type, foto en locatie; meerdere boten blijven voor BootManagerV2 voorlopig geparkeerd.
- Waarschuwingen hebben type, urgentie en link naar de betreffende module.
- Weer/getijden kunnen uit internet of lokale sensordata komen; relevante velden zijn windrichting, windsnelheid, luchtdruk, temperatuur en optioneel getijden.
- Widgets klikken door naar modules en kunnen later gepersonaliseerd worden qua zichtbaarheid en volgorde.
- Offline dashboard toont laatste bekende data met melding dat informatie mogelijk verouderd is.
- Automatische updates kunnen later via modulewijzigingen, polling of realtime UI; cloud-synchronisatie blijft toekomstscope.

## Epic 8: Systeembeheer & Configuratie

Word-bron verwerkt:

- `BootManager_Epic8_Systeembeheer.docx`
- Status: volledig leesbaar, geen OCR-gap voor Epic 8.

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

Functionele nuance uit Word-bron:

- Systeembeheer richt zich op eigenaar/beheerder en omvat eenheden, taal/regio, sensoren, gebruikersrechten, back-up, synchronisatie en systeeminformatie.
- Eenheden omvatten afstand, gewicht, volume, temperatuur, snelheid en druk.
- Sensorintegratie omvat GPS, NMEA, tankniveaus, motordata, barometer, windmeter en weerstation.
- Raspberry Pi-configuratie gaat over CPU, geheugen, opslag en netwerkstatus.
- Back-up/herstel verwacht een exportbestand zoals `.zip` of `.json`.
- Systeemactielog bevat datum, type actie, resultaat en foutmeldingen voor acties zoals back-ups, updates en synchronisaties.
- Offline-modusbeheer gaat in legacy vooral over het stoppen van automatische synchronisatie.
- Cloudaccounts, synchronisatieplanning en multi-user rollen blijven voor BootManagerV2 voorlopig geparkeerd.

## Epic 9: Integraties & Synchronisatie

Word-bron verwerkt:

- `BootManager_Epic9_Integratie.docx`
- Status: volledig leesbaar, eerdere OCR-gap US9.1 t/m US9.5 is opgelost.

Legacy user stories:

- US9.1 Weerdata koppelen.
- US9.2 AIS integratie.
- US9.3 Navionics/GPX import.
- US9.4 Haveninformatie koppelen.
- US9.5 Sensorintegratie via Bluetooth of Wi-Fi.
- US9.6 Externe API-verbinding beheren.
- US9.7 Synchronisatie met andere apparaten.

Functionele nuance uit Word-bron:

- Integratie-epic gaat over externe en lokale koppelingen: weer-API's, AIS, navigatiebronnen, havendatabases, lokale sensoren en apparaat-synchronisatie.
- Weerdata omvat wind, temperatuur, luchtdruk en getijden.
- AIS-integratie toont schepen in de omgeving wanneer AIS-data beschikbaar is.
- Navionics/GPX-import voegt routepunten toe.
- Haveninformatie omvat voorzieningen, tarieven en contactgegevens.
- Bluetooth/Wi-Fi sensoren leveren realtime data aan BootManager.
- Externe API-verbindingbeheer omvat API-sleutels en koppelingen.
- Synchronisatie tussen apparaten is lokale Wi-Fi/device-sync en blijft los van de al geparkeerde cloud-sync.

## Epic 10: Rapportage & Analyse

Word-bron verwerkt:

- `BootManager_Epic10_Rapportage.docx`
- Status: volledig leesbaar, geen OCR-gap voor Epic 10.

Legacy user stories:

- US10.1 Brandstofanalyse.
- US10.2 Voorraadanalyse.
- US10.3 Onderhoudsrapportage.
- US10.4 Kostenanalyse per tocht.
- US10.5 Export naar PDF/CSV.
- US10.6 Visuele trends en grafieken.

Functionele nuance uit Word-bron:

- Rapportage is module-overstijgend en gebruikt data uit logboek, inventaris, onderhoud en passageplanning.
- Brandstofanalyse vraagt geregistreerde motoruren en brandstofverbruik per passage.
- Voorraadanalyse vraagt voorraadmutaties over tijd.
- Onderhoudsrapportage vraagt uitgevoerde en geplande onderhoudsdata met kosten.
- Kostenanalyse per tocht vraagt kostenregistratie per passage of categorie.
- Export naar PDF/CSV geldt voor gegenereerde rapportages.
- Visuele trends en grafieken vragen historische data en interactieve rapportagepagina's.

## Epic 11: Notificaties & Waarschuwingen

Word-bron verwerkt:

- `BootManager_Epic11_Notificaties.docx`
- Status: volledig leesbaar, geen OCR-gap voor Epic 11.

Legacy user stories:

- US11.1 Waarschuwing bij lage voorraad.
- US11.2 Documentvervalmelding.
- US11.3 Onderhoudsherinnering.
- US11.4 Passageplanning waarschuwing.
- US11.5 Instellingen voor notificaties beheren.
- US11.6 Notificatiegeschiedenis bekijken.

Functionele nuance uit Word-bron:

- Notificaties omvatten push- en e-mailmeldingen, maar ook dashboardindicatoren en kleurwaarschuwingen.
- Bronnen zijn lage voorraad, bijna verlopen documenten, gepland onderhoud en naderende passagevertrekdatum.
- Notificatiefrequentie is instelbaar per gebruiker in legacy-scope.
- Verzonden meldingen worden gelogd zodat de gebruiker notificatiegeschiedenis kan bekijken.
- Voor BootManagerV2 zijn push/e-mail en per-user voorkeuren latere scope; in-app waarschuwingen kunnen eerder.

## Epic 12: Slimme Herkenning & AI-Ondersteuning

Word-bron verwerkt:

- `BootManager_Epic12_AI.docx`
- Status: volledig leesbaar, geen OCR-gap voor Epic 12.

Legacy user stories:

- US12.1 Barcodeherkenning.
- US12.2 AI-herkenning via foto.
- US12.3 Automatische categorisatie.
- US12.4 Suggesties voor aanvulling.
- US12.5 Predictief onderhoud.
- US12.6 Spraakondersteuning.

Functionele nuance uit Word-bron:

- AI-scope richt zich op herkenning, categorisatie en voorspelling voor voorraad en onderhoud.
- Barcode- en QR-herkenning gebruikt camera-input om productinformatie aan te vullen.
- Fotoherkenning toont suggesties voor productherkenning.
- Automatische categorisatie stelt categorieën voor bij nieuwe producten.
- Aanvulsuggesties vragen verbruiksdata en trendanalyse.
- Predictief onderhoud vraagt gebruiksdata en detectie van patronen.
- Spraakondersteuning is optioneel en voert uitgesproken commando's uit.
- Voor BootManagerV2 blijft dit lage prioriteit; barcode/QR zonder AI kan eerder als inventarisfeature.
