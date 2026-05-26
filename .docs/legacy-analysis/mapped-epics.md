# Legacy Epics Mapped To BootManagerV2

Status: eerste mapping (2026-05-25).

Legenda:

- ✅ Geïmplementeerd in BootManagerV2.
- 🟡 Deels geïmplementeerd.
- ⏳ Nog relevant / niet geïmplementeerd.
- 🧊 Geparkeerd of bewust buiten huidige scope.
- ❌ Niet meer relevant in oude vorm.

## Epic 0: Installatie & Authenticatie

Status: 🟡 deels geïmplementeerd / deels vervangen.

Word-verificatie:

- `BootManager_Epic0_Installatie_Authenticatie.docx` is verwerkt.
- De Word-bron bevestigt US0.1 t/m US0.6 volledig en vervangt de eerdere OCR-afhankelijkheid voor deze epic.

Mapping:

- Installatie uitvoeren: ✅ eerste Raspberry Pi 4 Docker Compose smoke test is geslaagd op 2026-05-26; productiehardening blijft onder systeembeheer/back-up/shutdown open.
- Eerste eigenaar registreren: ✅ vervangen door bootstrap owner + verplichte onboarding.
- Inloggen als eigenaar: ✅ wachtwoord-only login bestaat.
- Wachtwoord/pincode wijzigen: 🟡 pincode is bewust uit de normale flow verwijderd; wachtwoord wijzigen bestaat technisch in Settings maar moet UX/runtime opnieuw gevalideerd worden.
- Herstel van toegang: ❌ oude back-upcode/master-key flow is bewust uit normale UI verwijderd; operationele resetprocedure is gedocumenteerd.
- Eigenaarprofiel beheren: ⏳ vastgelegd als `Owner Profile & Vessel Settings` US1.

## Epic 1: Bootbeheer & Gebruikersbeheer

Status: 🟡 deels geïmplementeerd / oude multi-user delen geparkeerd.

Word-verificatie:

- `BootManager_Epic1_Bootbeheer_en_Gebruikersbeheer.docx` is verwerkt.
- De eerdere OCR-gap US1.9 t/m US1.13 is opgelost.
- Deze ontbrekende stories gaan over gebieden, opslaglocaties en QR-tags.

Mapping:

- Eerste bootaanmaak: ✅ via onboarding + `VesselProfile`.
- Bootinformatie bewerken: ⏳ vastgelegd als `Owner Profile & Vessel Settings` US2.
- Gebruikers/rollen/bemanning login: 🧊 bewust buiten single-owner scope.
- Meerdere boten/bootselectie: 🧊 buiten huidige single-installation/single-vessel scope.
- Gebieden/opslaglocaties/tags: ⏳ relevant als basis voor toekomstige inventaris-epic; Word-bron noemt expliciet gebied, opslaglocatie, omschrijving, QR-generatie, QR-scan en tagbeheer.
- Export/import bootgegevens: ⏳ relevant later als backup/restore of system config.
- Cloud-bootselectie: 🧊 geparkeerd.

## Epic 2: Inventarisbeheer

Status: ⏳ niet geïmplementeerd in BootManagerV2.

Word-verificatie:

- `BootManager_Epic2_Inventarisbeheer.docx` is verwerkt.
- De eerdere OCR-gap US2.1 en US2.2 is opgelost.
- US2.1 gaat over categorieën beheren; US2.2 gaat over categorie-icoontjes beheren.

Mapping:

- Oude repo had DTO's voor inventaris en opberglocaties, maar BootManagerV2 bevat nog geen inventarismodule.
- Functionele scope blijft relevant, maar moet opnieuw worden gesneden in kleinere BootManagerV2-slices:
  - productcategorieën;
  - categorie-icoontjes of een beperkte vaste iconenset;
  - opslaglocaties;
  - productcatalogus;
  - voorraad per locatie;
  - voorraadmutaties;
  - zoeken/filteren;
  - minimumvoorraad;
  - barcode/QR later.
- AI-herkenning, cloud-synchronisatie en passageplanning-integratie blijven latere afhankelijkheden, niet de eerste inventarisslice.

## Epic 3: Passageplanning

Status: ⏳ grotendeels niet geïmplementeerd.

Word-verificatie:

- `BootManager_Epic3_PassagePlanning.docx` is verwerkt.
- De Word-bron bevestigt US3.1 t/m US3.14 volledig.
- Er zijn geen aanvullende stories buiten de OCR-inventarisatie gevonden.

Mapping:

- BootManagerV2 heeft logbook trips, maar geen passageplanningmodule.
- Koppeling passageplanning ↔ voorraad/documenten/logboek blijft relevant voor later.
- Menuplanning en berekening van benodigdheden zijn afhankelijk van inventaris.
- Bemanningslijst in legacy-vorm vraagt naam en geboortedatum, maar hoeft niet direct samen te vallen met multi-user accounts.
- Verbruiksinstellingen voor brandstof en water passen later bij Settings, maar moeten pas worden gebouwd wanneer passageberekening wordt opgepakt.
- Cloud-synchronisatie blijft geparkeerd; lokale/offline planning is de relevante kern.

## Epic 4: Documentbeheer

Status: 🟡 deels geïmplementeerd als logboekbijlagen, niet als algemene documentkluis.

Word-verificatie:

- `BootManager_Epic4_Documentbeheer.docx` is verwerkt.
- De eerdere OCR-gap US4.1 is opgelost: document toevoegen en categoriseren.
- De Word-bron bevat ook US4.13: document openen, printen of delen.

Mapping:

- Logboekbijlagen: ✅ aanwezig.
- Algemeen documentbeheer met categorie, vervaldatum, zoek/filter, dashboard en documenthistorie: ⏳ niet geïmplementeerd.
- Vervaldatummeldingen hangen samen met notificatie-epic.
- Passage/onderhoud-documentkoppeling hangt samen met toekomstige modules.
- Eerste BootManagerV2-slice moet lokale/offline opslag, metadata en basis zoeken/filteren doen; cloud-sync en mailen/delen blijven later.
- Documentcategorieën overlappen conceptueel met inventariscategorieën, maar moeten niet automatisch hetzelfde model delen zonder ontwerpkeuze.

## Epic 5: Logboek

Status: 🟡 substantieel deels geïmplementeerd.

Word-verificatie:

- `BootManager_Epic5_Logboek.docx` is verwerkt.
- De eerdere OCR-gap US5.1 is opgelost: handmatig logboek invoeren met weerinformatie.
- De Word-bron bevestigt US5.1 t/m US5.14 volledig.

Mapping:

- Logboek/reis aanmaken: ✅ aanwezig.
- Automatische/draft logmomenten op interval: ✅ missing moments + draft-aanmaak aanwezig.
- Nautische velden: 🟡 veel velden aanwezig, maar motoruren/brandstof/barometer en afsluitflow moeten verder worden bekeken.
- Bijlagen aan logregel: ✅ aanwezig.
- Klassiek format: 🟡 tabel/printweergave aanwezig; routekaart ontbreekt.
- Export: 🟡 browser print aanwezig; server-side PDF/CSV ontbreekt.
- Statistieken/samenvatting: 🟡 trips hebben samenvattingsvelden; brandstof/verbruik/statistiek nog beperkt.
- Passagekoppeling: ⏳ niet aanwezig.
- Cloud-sync: 🧊 geparkeerd.
- Offline lokale werking is grotendeels passend bij BootManagerV2; later moet worden bewaakt dat logboekacties geen internetafhankelijkheid krijgen.
- US5.14 logboek afronden bij aankomst is een goede vervolgslice voor het bestaande digitale logboek.

## Epic 6: Onderhoudsbeheer

Status: ⏳ niet geïmplementeerd.

Word-verificatie:

- `BootManager_Epic6_OnderhoudsbeheerL.docx` is verwerkt.
- De Word-bron bevestigt US6.1 t/m US6.14 volledig.
- Er zijn geen aanvullende stories buiten de OCR-inventarisatie gevonden.

Mapping:

- Volledige onderhoudsmodule is open.
- Afhankelijkheden: documentbeheer voor facturen/handleidingen, notificaties voor herinneringen.
- Kan zelfstandig beginnen met basis onderhoudstaak CRUD en historiek.
- Onderdeelmodel hoeft in de eerste slice niet meteen de volledige bootstructuur of inventaris te zijn; een eenvoudige onderdeelnaam/categorie kan genoeg zijn om onderhoudshistoriek te starten.
- Intervalplanning op gebruiksdata zoals motoruren vraagt later koppeling met logboek/metingen; tijdsinterval kan eerst.
- Cloud-synchronisatie blijft geparkeerd; lokale/offline onderhoudsregistratie is relevant.

## Epic 7: Dashboard

Status: 🟡 deels aanwezig als eenvoudige dashboard/home, maar legacy widgetscope niet geïmplementeerd.

Word-verificatie:

- `BootManager_Epic7_Dashboard.docx` is verwerkt.
- De eerdere OCR-gap US7.1 t/m US7.8 is opgelost.
- De Word-bron bevestigt US7.1 t/m US7.14 volledig.

Mapping:

- Dashboard openen/centrale startpagina: 🟡 basis bestaat, maar niet als uitgebreide widgetstartpagina.
- Actieve bootinformatie: 🟡 single-vessel informatie bestaat via `VesselProfile`, maar geen multi-boot wissel of bootfoto-dashboard.
- Waarschuwingen en meldingen: 🟡 logboek missing-moments waarschuwing bestaat; generiek waarschuwingenpaneel ontbreekt.
- Weer/getijden: ⏳ niet aanwezig; lokale NMEA wind/luchtdruk kan later deels voeden, internet/getijden is aparte integratie.
- Voorraad/onderhoud/document/passage widgets: ⏳ afhankelijk van toekomstige modules.
- Logboekactiviteit widget: ⏳ niet als widgetmodule.
- Personaliseren widgets: ⏳ niet aanwezig.
- Interactieve navigatie: 🟡 gewone navigatie bestaat, widget-navigatie niet.
- Automatische updates: ⏳ geen SignalR/live widgetmodel.
- Cloud-sync: 🧊 geparkeerd.
- Eerste BootManagerV2-dashboardstap moet waarschijnlijk klein blijven: actuele boot/logboek/ingest-status en waarschuwingen uit bestaande modules, niet meteen alle legacy widgets.

## Epic 8: Systeembeheer & Configuratie

Status: 🟡 deels geïmplementeerd.

Word-verificatie:

- `BootManager_Epic8_Systeembeheer.docx` is verwerkt.
- De Word-bron bevestigt US8.1 t/m US8.14 volledig.
- Er zijn geen aanvullende stories buiten de OCR-inventarisatie gevonden.

Mapping:

- Instellingenpagina: ✅ aanwezig.
- Operationele ingest/sampling/settings: ✅ aanwezig.
- Sensorintegratie configureren: 🟡 ingest settings deels aanwezig.
- Raspberry Pi/Docker docs: ✅ documentatie aanwezig en eerste Pi 4 Docker Compose smoke test geslaagd op 2026-05-26.
- Back-up/herstel: ⏳ alleen procedureel beschreven, geen UI.
- Eenheden/taal/regio: ⏳ niet aanwezig.
- Gebruikersrollen: 🧊 buiten single-owner scope.
- Cloudinstellingen/synchronisatie/offline toggle: 🧊 geparkeerd.
- Systeemactie-logboek/export/import/default reset: ⏳ relevant later.
- Raspberry Pi systeeminformatie is deels procedureel afgedekt met resourcechecks uit de eerste deploymenttest; in-app Pi status en veilige device-operaties blijven open.
- Offline-modusbeheer is pas zinvol als er echte synchronisatieprocessen zijn; huidige lokale-first werking is al offlinevriendelijk.

## Epic 9: Integraties & Synchronisatie

Status: 🟡 gedeeltelijk vervangen door NMEA ingest.

Word-verificatie:

- `BootManager_Epic9_Integratie.docx` is verwerkt.
- De eerdere OCR-gap US9.1 t/m US9.5 is opgelost.
- US9.1 t/m US9.5 gaan over weerdata, AIS, Navionics/GPX-import, haveninformatie en Bluetooth/Wi-Fi sensorintegratie.

Mapping:

- Sensor/NMEA integratie: ✅ NMEA 0183/NMEA2000 ingest en measurement pipeline zijn veel verder dan legacy voor UDP/NMEA.
- AIS raw sentence support: 🟡 NMEA 0183 `!AIVDM`/`!AIVDO` kan al raw/parsing-technisch herkend worden, maar AIS-semantiek en schepenoverzicht ontbreken.
- Weer/getijden API: ⏳ niet aanwezig.
- Navionics/GPX-import: ⏳ niet aanwezig.
- Haveninformatie API: ⏳ niet aanwezig.
- Bluetooth/Wi-Fi sensor onboarding: ⏳ niet aanwezig; huidige ingest is UDP/Web API gericht.
- Externe API-verbindingen: ⏳ niet generiek aanwezig.
- Synchronisatie met andere apparaten/cloud: 🧊 geparkeerd.
- GPX-import kan later nuttiger zijn dan Navionics-specifieke integratie, omdat GPX open en lokaal/offlinevriendelijker is.

## Epic 10: Rapportage & Analyse

Status: 🟡 zeer beperkt aanwezig.

Word-verificatie:

- `BootManager_Epic10_Rapportage.docx` is verwerkt.
- De Word-bron bevestigt US10.1 t/m US10.6 volledig.
- Er zijn geen aanvullende stories buiten de OCR-inventarisatie gevonden.

Mapping:

- Logboek print/export: 🟡 browser print aanwezig.
- Brandstofanalyse, voorraadanalyse, onderhoudsrapportage, kostenanalyse: ⏳ afhankelijk van toekomstige modules.
- Grafieken/trends: ⏳ niet aanwezig.
- Rapportage moet pas breed worden opgepakt wanneer de onderliggende data betrouwbaar bestaat.
- Eerste realistische BootManagerV2-stap blijft logboekexport/statistieken; overige analyses volgen na inventaris/onderhoud/passagekosten.

## Epic 11: Notificaties & Waarschuwingen

Status: ⏳ grotendeels niet geïmplementeerd.

Word-verificatie:

- `BootManager_Epic11_Notificaties.docx` is verwerkt.
- De Word-bron bevestigt US11.1 t/m US11.6 volledig.
- Er zijn geen aanvullende stories buiten de OCR-inventarisatie gevonden.

Mapping:

- Logboek missing moments banner: 🟡 een in-app waarschuwing bestaat voor logboek.
- Lage voorraad/documentverval/onderhoud/passage notificaties: ⏳ afhankelijk van toekomstige modules.
- Browser push/e-mail: 🧊 later; eerdere docs parkeren browser push voor logboek bewust.
- Notificatiegeschiedenis en voorkeuren zijn niet aanwezig.
- Eerste BootManagerV2-stap moet waarschijnlijk een generiek in-app waarschuwingenmodel zijn, niet meteen push/e-mail.

## Epic 12: Slimme Herkenning & AI-Ondersteuning

Status: ⏳ niet geïmplementeerd / lage prioriteit.

Word-verificatie:

- `BootManager_Epic12_AI.docx` is verwerkt.
- De Word-bron bevestigt US12.1 t/m US12.6 volledig.
- Er zijn geen aanvullende stories buiten de OCR-inventarisatie gevonden.

Mapping:

- Barcode/QR-herkenning kan later onderdeel worden van inventaris.
- AI-herkenning, automatische categorisatie, predictief onderhoud en spraakinput zijn toekomstscope.
- Niet nodig voor huidige kernwaarde.
- AI-aanvulsuggesties en predictief onderhoud zijn afhankelijk van voldoende historische voorraad-, verbruiks- en onderhoudsdata.
- Niet-AI barcode/QR flows moeten eerder worden behandeld dan AI-herkenning.
