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

Mapping:

- Installatie uitvoeren: 🟡 Docker/Raspberry Pi documentatie is aanwezig; echte Docker/Pi smoke test staat nog open.
- Eerste eigenaar registreren: ✅ vervangen door bootstrap owner + verplichte onboarding.
- Inloggen als eigenaar: ✅ wachtwoord-only login bestaat.
- Wachtwoord/pincode wijzigen: 🟡 pincode is bewust uit de normale flow verwijderd; wachtwoord wijzigen bestaat technisch in Settings maar moet UX/runtime opnieuw gevalideerd worden.
- Herstel van toegang: ❌ oude back-upcode/master-key flow is bewust uit normale UI verwijderd; operationele resetprocedure is gedocumenteerd.
- Eigenaarprofiel beheren: ⏳ vastgelegd als `Owner Profile & Vessel Settings` US1.

## Epic 1: Bootbeheer & Gebruikersbeheer

Status: 🟡 deels geïmplementeerd / oude multi-user delen geparkeerd.

Mapping:

- Eerste bootaanmaak: ✅ via onboarding + `VesselProfile`.
- Bootinformatie bewerken: ⏳ vastgelegd als `Owner Profile & Vessel Settings` US2.
- Gebruikers/rollen/bemanning login: 🧊 bewust buiten single-owner scope.
- Meerdere boten/bootselectie: 🧊 buiten huidige single-installation/single-vessel scope.
- Gebieden/opslaglocaties/tags: ⏳ relevant als basis voor toekomstige inventaris-epic.
- Export/import bootgegevens: ⏳ relevant later als backup/restore of system config.
- Cloud-bootselectie: 🧊 geparkeerd.

## Epic 2: Inventarisbeheer

Status: ⏳ niet geïmplementeerd in BootManagerV2.

Mapping:

- Oude repo had DTO's voor inventaris en opberglocaties, maar BootManagerV2 bevat nog geen inventarismodule.
- Functionele scope blijft relevant, maar moet opnieuw worden gesneden in kleinere BootManagerV2-slices:
  - opslaglocaties;
  - productcatalogus;
  - voorraad per locatie;
  - voorraadmutaties;
  - zoeken/filteren;
  - minimumvoorraad;
  - barcode/QR later.

## Epic 3: Passageplanning

Status: ⏳ grotendeels niet geïmplementeerd.

Mapping:

- BootManagerV2 heeft logbook trips, maar geen passageplanningmodule.
- Koppeling passageplanning ↔ voorraad/documenten/logboek blijft relevant voor later.
- Menuplanning en berekening van benodigdheden zijn afhankelijk van inventaris.

## Epic 4: Documentbeheer

Status: 🟡 deels geïmplementeerd als logboekbijlagen, niet als algemene documentkluis.

Mapping:

- Logboekbijlagen: ✅ aanwezig.
- Algemeen documentbeheer met categorie, vervaldatum, zoek/filter, dashboard en documenthistorie: ⏳ niet geïmplementeerd.
- Vervaldatummeldingen hangen samen met notificatie-epic.
- Passage/onderhoud-documentkoppeling hangt samen met toekomstige modules.

## Epic 5: Logboek

Status: 🟡 substantieel deels geïmplementeerd.

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

## Epic 6: Onderhoudsbeheer

Status: ⏳ niet geïmplementeerd.

Mapping:

- Volledige onderhoudsmodule is open.
- Afhankelijkheden: documentbeheer voor facturen/handleidingen, notificaties voor herinneringen.
- Kan zelfstandig beginnen met basis onderhoudstaak CRUD en historiek.

## Epic 7: Dashboard

Status: 🟡 deels aanwezig als eenvoudige dashboard/home, maar legacy widgetscope niet geïmplementeerd.

Mapping:

- Logboekactiviteit widget: ⏳ niet als widgetmodule.
- Personaliseren widgets: ⏳ niet aanwezig.
- Interactieve navigatie: 🟡 gewone navigatie bestaat, widget-navigatie niet.
- Automatische updates: ⏳ geen SignalR/live widgetmodel.
- Cloud-sync: 🧊 geparkeerd.

## Epic 8: Systeembeheer & Configuratie

Status: 🟡 deels geïmplementeerd.

Mapping:

- Instellingenpagina: ✅ aanwezig.
- Operationele ingest/sampling/settings: ✅ aanwezig.
- Sensorintegratie configureren: 🟡 ingest settings deels aanwezig.
- Raspberry Pi/Docker docs: ✅ documentatie aanwezig, smoke test nog open.
- Back-up/herstel: ⏳ alleen procedureel beschreven, geen UI.
- Eenheden/taal/regio: ⏳ niet aanwezig.
- Gebruikersrollen: 🧊 buiten single-owner scope.
- Cloudinstellingen/synchronisatie/offline toggle: 🧊 geparkeerd.
- Systeemactie-logboek/export/import/default reset: ⏳ relevant later.

## Epic 9: Integraties & Synchronisatie

Status: 🟡 gedeeltelijk vervangen door NMEA ingest.

Mapping:

- Sensor/NMEA integratie: ✅ NMEA 0183/NMEA2000 ingest en measurement pipeline zijn veel verder dan legacy.
- Externe API-verbindingen: ⏳ niet generiek aanwezig.
- Synchronisatie met andere apparaten/cloud: 🧊 geparkeerd.

## Epic 10: Rapportage & Analyse

Status: 🟡 zeer beperkt aanwezig.

Mapping:

- Logboek print/export: 🟡 browser print aanwezig.
- Brandstofanalyse, voorraadanalyse, onderhoudsrapportage, kostenanalyse: ⏳ afhankelijk van toekomstige modules.
- Grafieken/trends: ⏳ niet aanwezig.

## Epic 11: Notificaties & Waarschuwingen

Status: ⏳ grotendeels niet geïmplementeerd.

Mapping:

- Logboek missing moments banner: 🟡 een in-app waarschuwing bestaat voor logboek.
- Lage voorraad/documentverval/onderhoud/passage notificaties: ⏳ afhankelijk van toekomstige modules.
- Browser push/e-mail: 🧊 later; eerdere docs parkeren browser push voor logboek bewust.

## Epic 12: Slimme Herkenning & AI-Ondersteuning

Status: ⏳ niet geïmplementeerd / lage prioriteit.

Mapping:

- Barcode/QR-herkenning kan later onderdeel worden van inventaris.
- AI-herkenning, automatische categorisatie, predictief onderhoud en spraakinput zijn toekomstscope.
- Niet nodig voor huidige kernwaarde.
