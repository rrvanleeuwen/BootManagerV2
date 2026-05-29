# Epic: System Operations & Recovery

Status: SYS-RESET-1 geïmplementeerd, gemerged naar `master` en handmatig gevalideerd op Raspberry Pi op 2026-05-27. Eerste echte Raspberry Pi-veldtest met bootdata gevalideerd op 2026-05-29. SYS-ANALYSIS-1 is gemerged en op de Pi gevalideerd. SYS-CTRL-1 is gemerged en op de Pi gevalideerd op 2026-05-29. SYS-CTRL-2 is gemerged en op de Pi gevalideerd op 2026-05-29.

## Aanleiding

BootManager draait nu succesvol op een Raspberry Pi 4 met Docker Compose. Voor ontwikkel-, test- en helpdeskscenario's is een gecontroleerde manier nodig om een installatie terug te zetten naar de eerste-start toestand zonder handmatig Docker volumes of SQLite-bestanden te verwijderen.

Handmatig de database verwijderen of `docker compose down -v` gebruiken is op de Raspberry Pi ongewenst:

- het risico op onbedoeld dataverlies is groot;
- bijlagen, logs of capturebestanden kunnen onnodig verdwijnen;
- het is lastig reproduceerbaar voor helpdesk of ontwikkeltests;
- GitHub `master` moet leidend blijven en lokale afwijkingen op de Pi zijn niet gewenst.

## Doel

BootManager krijgt kleine, gecontroleerde operationele hulpmiddelen voor Raspberry Pi/Docker beheer:

- testinstallatie gecontroleerd opnieuw initialiseren;
- bestaande data eerst veiligstellen;
- bootstrap/onboarding opnieuw starten;
- latere uitbreiding richting back-up, restore, systeemstatus en veilige shutdown mogelijk houden.

## Uitgangspunten

- De eerste slice is bedoeld voor ontwikkelaar/helpdesk/operator via SSH of lokale beheercontext.
- Geen publieke webknop voor factory reset.
- Geen reset zonder expliciete waarschuwing en bevestiging.
- `.env` blijft bestaan en blijft lokaal per apparaat.
- Secrets worden niet gelogd, niet geback-upt naar Git en niet in documentatie opgenomen.
- De bestaande Docker Compose deployment blijft leidend.

## User Stories

### SYS-RESET-1: Gecontroleerde Database Reset Voor Pi Testinstallatie

**Status:** ✅ Geïmplementeerd, gemerged naar `master` en handmatig gevalideerd op 2026-05-27.

**Implementation Status (2026-05-27):**

Code, script, and documentation are complete. The following have been delivered:

**Implementation Components:**

1. **`scripts/reset-database.sh`** (new)
   - Bash operator script for Raspberry Pi
   - Moet expliciet met `sudo` worden gestart
   - Dynamically detects Docker volume name (not hardcoded)
   - Safety checks: Docker/Compose availability, docker-compose.yml present
   - User confirmation prompt before any destructive action
   - Stops containers cleanly with `docker compose stop`
   - Creates timestamped backup of SQLite database
   - Removes active database file only
   - Restarts containers and validates health check
   - Full error handling and operator-friendly output

2. **`.docs/docker-deployment.md`** (updated)
   - "Gecontroleerde Database Reset (Testinstallatie)" section
   - Usage scenarios, procedures, verification steps
   - Troubleshooting guide, safety warnings
   - Exact Pi commands documented

3. **`.docs/pi-first-install-runbook.md`** (updated)
   - Section 18a: "Gecontroleerde Database Reset"
   - References automated reset script
   - Backup location and naming documentation

4. **`.docs/raspberry-pi-deployment.md`** (updated)
   - "Onderhoud en Operaties" section
   - Reset procedure overview and scenarios
   - Cross-references to detailed docs

**Pi Validatie Uitgevoerd (2026-05-27):**

Handmatige validatie is uitgevoerd op een Raspberry Pi Docker Compose installatie op `master`.

- [x] Operator kon reset script uitvoeren zonder `docker compose down -v`
- [x] Script detecteerde correct het Docker volume met project-prefix (`bootmanagerv2_bootmanager-db`)
- [x] Timestamped database backup werd succesvol aangemaakt
- [x] Actieve database file werd clean verwijderd
- [x] Containers startten opnieuw en bereikten gezonde status
- [x] Health check gaf opnieuw HTTP 200
- [x] Login met `BOOTMANAGER_BOOTSTRAP_PASSWORD` werkte op de verse database
- [x] Eerste login stuurde correct naar `/onboarding`
- [x] Na onboarding werkte bootstrap-wachtwoord niet meer
- [x] Nieuw wachtwoord werkte voor daaropvolgende login
- [x] `.env`, Git checkout, attachments en logs bleven ongemoeid voor zover gevalideerd in deze resetflow
- [x] Backupbestand bleef aanwezig met verwacht timestamp-formaat

**Manual Validation Steps (to be performed on Pi):**

```bash
# 1. Verify current state before reset
cd ~/BootManagerV2
docker compose ps
curl -i http://localhost:5000/health
docker volume ls | grep bootmanager-db

# 2. Execute reset script
sudo bash scripts/reset-database.sh
# Type 'yes' at confirmation prompt
# Observe container stop, database backup, removal, and restart

# 3. Verify reset completion
docker compose ps
curl -i http://localhost:5000/health

# 4. Check backup created with timestamp
PROJECT_NAME=$(docker compose config 2>/dev/null | grep -m1 "name:" | sed 's/.*name: //' | xargs) || PROJECT_NAME=$(basename "$(pwd)" | tr '[:upper:]' '[:lower:]' | tr -cd 'a-z0-9')
VOLUME_PATH=$(docker volume inspect "${PROJECT_NAME}_bootmanager-db" -f '{{.Mountpoint}}')
ls -lh $VOLUME_PATH/bootmanager.db*
# Should see: bootmanager.db and bootmanager.db.backup.YYYYMMDD_HHMMSS

# 5. Test login flow
# Access http://localhost:5000
# Login with BOOTMANAGER_BOOTSTRAP_PASSWORD (from .env)
# Verify forced redirect to /onboarding
# Complete onboarding with test data
# Set new password during onboarding
# Logout and login with new password (should work)
# Try login with BOOTMANAGER_BOOTSTRAP_PASSWORD (should fail)

# 6. Verify resources preserved
cat .env | grep BOOTMANAGER  # Should be intact
git status  # Should be clean
ls -la BootManager.Web/data/logbook-attachments 2>/dev/null || echo "No attachments yet"
docker volume ls | grep bootmanager-logs  # Should exist
```

**Administratieve Status:**

Deze story is administratief afgerond:

1. Implementatie is via PR #65 naar `master` gemerged.
2. Raspberry Pi validatie is succesvol uitgevoerd op `master`.
3. Reset, health check, bootstrap login, onboarding en nieuw wachtwoord zijn handmatig bevestigd.

**Legacy Coverage Impact:**

- `US0.5 Herstel van toegang`: `Replaced` en nu handmatig gevalideerd via operationele resetprocedure op de Pi
- `US8.8 Back-up maken en herstellen`: blijft `Open` (dit is reset-backup, geen volledige restore)
- `US8.14 Standaardinstellingen herstellen`: blijft `Open` (CLI reset, geen algemene UI-instellingen-reset)
- `US8.11 Systeemactie-logboek`: blijft `Open` (logging nog niet geïmplementeerd)

**User Story:** Als ontwikkelaar/helpdesk wil ik via een veilige onderhoudsprocedure de lokale BootManager database kunnen resetten, zodat ik een Raspberry Pi testinstallatie opnieuw door bootstrap login en onboarding kan laten lopen zonder handmatig Docker volumes of databasebestanden te verwijderen.

## Relatie Tot Latere Stories

Deze story is een kleine operator-slice en vervangt niet:

- volledige back-up maken van database, bijlagen en configuratie;
- restore vanuit een gekozen back-up;
- Raspberry Pi systeemstatus in de UI;
- systeemactie-logboek;
- veilige shutdown vanuit UI/helper-service.

Die onderwerpen blijven aparte systeembeheerstories.

---

### SYS-FIELD-1: Eerste Pi-veldtest met echte bootdata documenteren en vervolgwerk snijden

**Status:** Goedgekeurd voor documentatie-uitwerking op 2026-05-29.

**User Story:** Als ontwikkelaar/operator wil ik de eerste echte Raspberry Pi-veldtest met boordnetwerkdata zakelijk vastleggen en vertalen naar concrete vervolgstories, zodat de gevalideerde Pi-status, beperkingen en vervolgstappen betrouwbaar in de projectdocumentatie staan.

**Aanleiding:**

Op 2026-05-29 is aan boord de eerste echte Raspberry Pi-veldtest uitgevoerd met BootManagerV2 via Docker Compose op `master` commit `1db5534`.

Bevestigd tijdens deze test:

- `bootmanager-web` was healthy via `/health`.
- `bootmanager-ingest` ontving echte boordnetwerkdata op UDP `10110`.
- Ingest postte berichten succesvol naar `http://bootmanager-web:5000/api/networkmessages`.
- De Web API antwoordde herhaaldelijk met `HTTP 201 Created`.
- Ruwe `NetworkMessages` werden opgeslagen in SQLite.
- Meerdere NMEA 0183 sentence-types werden geparset uit echte bootdata.
- Meerdere measurement-tabellen werden gevuld, onder meer `HeadingMeasurements`, `WindMeasurements`, `SpeedThroughWaterMeasurements`, `WaterTemperatureMeasurements`, `PositionMeasurements` en `MotionMeasurements`.
- Er zijn geen recente `error`, `exception` of `fail` meldingen gezien.

Waargenomen aandachtspunten:

- `sqlite3` ontbrak zowel op de Pi-host als in de container, waardoor directe SQL-inspectie niet mogelijk was.
- Herhaalde waarschuwingen voor `GGA` met fixkwaliteit `0` veroorzaakten logruis, maar geen crash of API-fout.
- UI-validatie van live meetdata is nog niet uitgevoerd.
- Langdurige observatie van databasegroei, WAL-bestand, retentie en capture logs staat nog open.
- Terminologie moet consequent blijven: fysieke bron is NMEA2000/SeaTalkNG via gateway; BootManager ontvangt momenteel NMEA 0183 UDP-sentences.

**Scope:**

- De veldtest van `2026-05-29` documenteren met expliciete Pi-, branch- en commitcontext.
- Zakelijk vastleggen wat `groen`, `geel` en `open` is aan deze test.
- Huidige Raspberry Pi/Docker documentatie en handoff bijwerken met deze gevalideerde status.
- Concrete vervolgstories formuleren voor diagnostics, logging, UI-validatie, retentie/WAL, capture logs en veldtestprocedure.

**Buiten scope:**

- Geen applicatiecodewijzigingen.
- Geen nieuwe Pi-runtime-test uitvoeren als onderdeel van deze documentatiestap.
- Geen reset, loggingherconfiguratie of diagnostics-endpoint implementeren.
- Geen wijziging aan de master-only Pi-updateafspraak.

**Acceptatiecriteria:**

- De documentatie noemt expliciet: datum `2026-05-29`, platform `Raspberry Pi + Docker Compose`, branch/commit `master @ 1db5534`, resultaat `geslaagd`.
- De documentatie noemt expliciet dat ingest, API, parsing, interpretatie en database-opslag tijdens de echte boot-test bevestigd zijn.
- De documentatie noemt expliciet dat geen recente `error`, `exception` of `fail` meldingen zijn gezien.
- De documentatie noemt expliciet dat `sqlite3` ontbrak en dat `GGA` met fixkwaliteit `0` veel warnings gaf.
- De documentatie noemt expliciet dat UI-validatie en langdurige duurtest nog openstaan.
- Vervolgstories zijn vastgelegd met acceptatiecriteria en testnotities.
- De werkstap blijft documentatie-only.

**Legacy coverage impact:**

- Onderbouwt bestaande `Partial` dekking van `US8.6 Raspberry Pi-configuratie beheren`.
- Raakt `US8.11 Systeemactie-logboek` en `US8.8 Back-up maken en herstellen` als open vervolggebied, zonder ze af te vinken.
- Verandert geen legacy-user-story direct naar `Done`.

**Handmatige testnotities:**

- Deze story zelf vraagt alleen documentatiecontrole en `git diff`-controle.
- Nieuwe runtime-tests volgen in aparte stories hieronder.

---

### SYS-DIAG-1: Pi diagnostics zonder handmatige sqlite3-inspectie

**Status:** Voorgesteld op 2026-05-29.

**User Story:** Als operator wil ik op de Raspberry Pi eenvoudig aantallen `NetworkMessages` en relevante measurement-tabellen kunnen zien zonder handmatige sqlite3-installatie, zodat ik veldtests en supportchecks sneller kan uitvoeren.

**Scope:**

- Een veilige read-only diagnostics route kiezen: runbook-hulpmiddel, CLI-hulpscript of intern endpoint.
- Minimaal aantallen tonen voor `NetworkMessages` en de relevante measurement-tabellen die nu tijdens veldtests gebruikt worden.
- Duidelijk documenteren hoe deze diagnostics op de Pi uitgevoerd worden.

**Buiten scope:**

- Geen algemene database-browser.
- Geen write-acties.
- Geen publieke internet-exposure.

**Acceptatiecriteria:**

- Operator kan op de Pi zonder losse sqlite3-installatie een overzicht opvragen van `NetworkMessages` en kern-measurements.
- De gekozen route is read-only.
- Runbook/documentatie beschrijft exact hoe de check uitgevoerd wordt.
- Handmatige Pi-test toont dat de aantallen zichtbaar zijn na ontvangst van echte of gesimuleerde data.

**Handmatige testnotities:**

- Pi-veldtest of lokale Pi-achtige Docker-test.
- Verifieer dat na draaiende ingest de aantallen oplopen.

---

### SYS-LOG-1: Loggingprofiel voor Pi-veldtest en productie aanscherpen

**Status:** Voorgesteld op 2026-05-29.

**User Story:** Als operator wil ik tijdens Pi-veldtests minder logruis en beter bruikbare operationele logging zien, zodat relevante waarschuwingen niet verdrinken in verwachte validatieberichten en EF SQL-noise.

**Scope:**

- Evalueren van logniveau voor EF SQL logging in Pi/veldtestscenario's.
- Evalueren van logniveau of throttling voor verwachte NMEA-validatie-afwijzingen, met name `GGA` fixkwaliteit `0`.
- Heldere operationele logging behouden voor echte fouten, health en ingest-doorvoer.

**Buiten scope:**

- Geen volledige logging-stackmigratie.
- Geen observability-platform of externe logaggregatie.

**Acceptatiecriteria:**

- Herhaalde verwachte `GGA`-afwijzingen veroorzaken niet langer onnodig veel waarschuwingen.
- EF SQL-logging is in Pi/veldtestmodus aantoonbaar minder dominant of beter afgeschermd.
- Echte errors/exceptions blijven zichtbaar.
- Een handmatige Pi-logcheck laat zien dat de logs compacter maar nog bruikbaar zijn.

**Handmatige testnotities:**

- Pi-veldtest met live bootdata of representatieve replay.
- Controleer zowel ingest- als web-logs vóór en na de aanpassing.

---

### SYS-GPS-1: GPS-fix diagnostics voor GGA/RMC-validiteit

**Status:** Voorgesteld op 2026-05-29.

**User Story:** Als operator wil ik kunnen zien welke positieberichten geldig of ongeldig zijn en wat de laatste GPS-fixkwaliteit was, zodat ik tijdens boot-tests sneller begrijp waarom positie-opslag wel of niet plaatsvindt.

**Scope:**

- Inzicht geven in laatste GPS-fixkwaliteit en aantallen geldige/ongeldige positie-updates.
- Relatie verduidelijken tussen `GGA`-fixkwaliteit, `RMC`-status en het al dan niet opslaan van `PositionMeasurements`.
- Resultaat zichtbaar maken via diagnostics of ondersteunende logging.

**Buiten scope:**

- Geen brede kaart- of dashboardfeature.
- Geen wijziging aan parser/interpreter-semantiek zonder aparte story.

**Acceptatiecriteria:**

- Operator kan de laatste fixkwaliteit en recente geldige/ongeldige positie-updates uitlezen.
- Documentatie legt uit waarom `fixkwaliteit 0` geen positie-opslag oplevert.
- Handmatige test met data zonder fix en data met geldige fix toont onderscheid.

**Handmatige testnotities:**

- Bij voorkeur Pi-test of replay met bekende mix van `GGA` en `RMC`.

---

### SYS-UI-1: Live-data UI-validatie na Pi-veldtest

**Status:** Voorgesteld op 2026-05-29.

**User Story:** Als gebruiker wil ik kunnen bevestigen dat live opgeslagen meetdata ook correct zichtbaar wordt in de BootManager-UI, zodat de technische ingestketen en de gebruikersweergave beide gevalideerd zijn.

**Scope:**

- Handmatige validatiestap ontwerpen voor dashboard, logboek of relevante overzichtspagina's met live data.
- Controleren dat opgeslagen metingen zichtbaar, plausibel en consistent zijn met database/logs.

**Buiten scope:**

- Geen nieuwe UI bouwen.
- Geen grafiek- of dashboardrefactor.

**Acceptatiecriteria:**

- Er is een concrete handmatige testprocedure voor live UI-validatie met Pi-data.
- Minimaal één relevante UI-weergave is gevalideerd tegen live opgeslagen data.
- Bevindingen worden vastgelegd als groen/geel/open.

**Handmatige testnotities:**

- Uitvoeren op de Pi of via browser tegen de Pi-installatie.
- Vergelijken met logs en, waar mogelijk, diagnostics-aantallen.

---

### SYS-DATA-1: Databasegroei, WAL en retentie op Pi monitoren

**Status:** Voorgesteld op 2026-05-29.

**User Story:** Als operator wil ik zicht hebben op databasegroei, WAL-gedrag en retentie op de Raspberry Pi, zodat langdurige logging het device niet stilzwijgend vol laat lopen.

**Scope:**

- Observeren en documenteren hoe `bootmanager.db`, `bootmanager.db-wal` en logs groeien tijdens langere tests.
- Eisen formuleren voor retentie, checkpointing en opslagwaarschuwingen.
- Bepalen welke minimale health-indicatoren nodig zijn voor langdurige Pi-draaiuren.

**Buiten scope:**

- Geen volledige archiveringsoplossing in deze story.
- Geen productiebreed backup/restore-systeem.

**Acceptatiecriteria:**

- Er is een concrete langdurige Pi-test of observatieplan voor database- en WAL-groei.
- Documentatie benoemt drempels of aandachtspunten voor opslaggroei.
- Vervolgkeuzes voor retentie/checkpointing zijn expliciet gemaakt of open vragen zijn helder vastgelegd.

**Handmatige testnotities:**

- Pi-duurtest over meerdere uren of dagdelen.
- Vastleggen van bestandsgroottes vóór en na de test.

---

### SYS-CAPTURE-1: Capture logs op Pi valideren voor rotatie en replay

**Status:** Voorgesteld op 2026-05-29.

**User Story:** Als operator wil ik zeker weten dat capture logs op de Pi correct worden geschreven, terug te vinden zijn en bruikbaar blijven voor replay, zodat velddata later reproduceerbaar onderzocht kan worden.

**Scope:**

- Controleren dat NDJSON capture logs tijdens Pi-veldtests daadwerkelijk op het verwachte volume ontstaan.
- Controleren hoe bestandsnaam, locatie en rotatie zich gedragen.
- Bepalen of de capture logs bruikbaar zijn voor latere replay-validatie.

**Buiten scope:**

- Geen volledige replay-implementatie als die nog ontbreekt.
- Geen algemene logviewer-UI.

**Acceptatiecriteria:**

- Tijdens een Pi-test ontstaat een capture logbestand op de verwachte locatie.
- Documentatie beschrijft hoe operator dit bestand controleert.
- Er is duidelijk of huidige rotatie/retentie voldoende is of vervolgwerk nodig heeft.
- Capture logs zijn aantoonbaar bruikbaar of expliciet nog niet bruikbaar voor replay.

**Handmatige testnotities:**

- Pi-veldtest of Pi-like Docker test met `CaptureLogging.Enabled=true`.

---

### SYS-FIELD-2: Veldtestprocedure voor vergelijking met boordinstrumenten

**Status:** Voorgesteld op 2026-05-29.

**User Story:** Als tester wil ik een vaste veldtestchecklist hebben om BootManager-metingen te vergelijken met Raymarine/Axiom/boordinstrumenten, zodat inhoudelijke juistheid naast technische ketenwerking wordt gevalideerd.

**Scope:**

- Een handmatige checklist opstellen voor vergelijking van onder meer heading, wind, snelheid, watertemperatuur en positie.
- Vastleggen welke bronleidend is en welke toleranties acceptabel zijn.
- Onderscheid maken tussen technische ketencheck en inhoudelijke kalibratie/plausibiliteitscheck.

**Buiten scope:**

- Geen automatische kalibratie.
- Geen wijziging aan sensor- of gatewayconfiguratie.

**Acceptatiecriteria:**

- Er is een herbruikbare checklist voor veldtests aan boord.
- De checklist benoemt welke waarden vergeleken worden en hoe afwijkingen worden genoteerd.
- Minimaal één toekomstige veldtest kan deze checklist direct gebruiken.

**Handmatige testnotities:**

- Uitvoeren aan boord met echte instrumenten.

---

### SYS-ANALYSIS-1: Technische analysepagina in de webinterface

**Status:** ✅ Geïmplementeerd, gemerged naar `master` via PR #70 en handmatig gevalideerd op Raspberry Pi op 2026-05-29.

**User Story:** Als beheerder wil ik in de webinterface kunnen zien wat er in een gekozen tijdsbestek is binnengekomen, wat is verwerkt, wat in de database staat en welke warnings/errors optraden, zodat ik Pi-tests zonder SSH of losse shellcommando's kan analyseren.

**Scope:**

- Een webpagina voor technische analyse of beheeranalyse toevoegen.
- Een tijdsvenster kunnen kiezen voor analyse.
- Minimaal zichtbaar maken:
  - welke ruwe data binnenkwam;
  - welke data is verwerkt;
  - welke measurement-typen zijn opgeslagen;
  - welke warnings/errors relevant waren;
  - samenvattende status van databasevulling voor de belangrijkste tabellen.
- Download van relevante analyse-uitvoer mogelijk maken, zodat deze later samen onderzocht kan worden.

**Buiten scope:**

- Geen algemene databasebeheerpagina.
- Geen vrije shelltoegang vanuit de webinterface.
- Geen volledige logviewer voor alle infrastructuurlogs van het systeem.

**Acceptatiecriteria:**

- Beheerder kan in de webinterface een tijdsvenster selecteren en analysegegevens zien.
- De pagina toont minimaal raw/verwerkt/opgeslagen/error-samenvatting.
- Relevante analyse-informatie kan worden gedownload.
- Verwachte warnings zoals GPS-fixproblemen zijn herkenbaar in de output.
- Handmatige test op Pi of lokale testomgeving bevestigt dat de pagina helpt om dezelfde soort checks te doen als eerder via SSH.

**Handmatige testnotities:**

- Bij voorkeur uitvoeren op de Pi met live of recent opgeslagen data.
- Verifiëren dat downloadbestanden bruikbaar zijn voor latere gezamenlijke analyse.

**Implementation Status (2026-05-29):**

- Analysefunctionaliteit is toegevoegd via een Application-service (`IAnalysisService`) en Blazor-pagina; de pagina gebruikt geen self-HTTP-call naar de eigen applicatie.
- De analysepagina is bereikbaar vanuit de webinterface en toont per gekozen tijdsvenster aantallen voor ruwe `NetworkMessages` en opgeslagen measurement-typen.
- Export naar JSON en CSV werkt via de bestaande Blazor JavaScript-downloadhelper.
- Database-aantallen worden via `IRepository.CountAsync(...)` opgehaald, zodat de analyse geen volledige lijsten hoeft te laden om alleen aantallen te bepalen.
- Test-export is vastgelegd als `.docs/extraInfo/analysis-20260528-1546-20260529-1546.json`.
- Lokale handmatige test bevestigde dat de pagina aantallen per meettype toont en dat export na harde browser-refresh (`Ctrl+F5`) werkt.
- Pi-validatie na merge naar `master` bevestigde dat de analysepagina op de Raspberry Pi exporteert naar JSON en CSV. Bewijsbestanden:
  - `veldtests/analysis-20260528-1635-20260529-1635.json`
  - `veldtests/analysis-20260528-1635-20260529-1635.csv`
- Pi-export over het gekozen 24-uursvenster bevatte `431021` ruwe `NetworkMessages` en consistente measurement-aantallen in JSON en CSV.

**Beperkingen / vervolgwerk:**

- Datum/tijd-velden op de analysepagina hebben een UX-/bindingprobleem: waarden zijn niet betrouwbaar handmatig te typen of via picker te wijzigen; bij verlaten van het veld komt de oude waarde terug. Vastgelegd als `SYS-ANALYSIS-2`.
- Warnings en errors worden nog niet persistent als analyseerbare events opgeslagen; de pagina meldt dit expliciet.
- Diepere loganalyse, GPS-fixdiagnostics en langdurige opslagobservatie blijven aparte vervolgstories (`SYS-LOG-1`, `SYS-GPS-1`, `SYS-DATA-1`, `SYS-CAPTURE-1`).

**Legacy coverage impact:**

- Verbetert `US8.6 Raspberry Pi-configuratie beheren` door in-app technische analyse/diagnostics toe te voegen, maar status blijft `Partial` zolang volledige Pi-systeemstatus/configuratiebeheer en langdurige observatie open zijn.
- Raakt `US8.11 Logboek van systeemacties bekijken`, maar vinkt deze niet af omdat systeemacties, warnings en errors nog niet persistent als logboek beschikbaar zijn.

---

### SYS-ANALYSIS-2: Datum/tijd invoer op analysepagina betrouwbaar maken

**Status:** Vastgelegd als bugfix-kandidaat op 2026-05-29.

**User Story:** Als beheerder wil ik op de analysepagina de begin- en einddatum/tijd betrouwbaar kunnen wijzigen via toetsenbord en picker, zodat ik zelf een exact analysevenster kan kiezen zonder dat de velden terugvallen naar oude waarden.

**Aanleiding:**

Tijdens gebruik van de analysepagina op 2026-05-29 bleek dat beide datum/tijd-velden niet goed werken:

- handmatig typen in de velden doet niets of wordt niet zichtbaar verwerkt;
- wijzigen via de browser/picker lijkt mogelijk, maar na verlaten van het veld komt de oude waarde terug;
- Enter bevestigt de wijziging niet;
- Tab bevestigt de wijziging niet;
- het probleem treedt op bij beide velden.

**Scope:**

- Onderzoeken hoe de Blazor binding/formatting/validatie van de analysepagina-datumvelden nu werkt.
- Begin- en einddatum/tijd invoer betrouwbaar maken voor toetsenbordgebruik.
- Begin- en einddatum/tijd invoer betrouwbaar maken voor browser/pickergebruik.
- Zorgen dat verlaten van het veld, Enter of Tab geen geldige invoer stilzwijgend terugzet naar de oude waarde.
- Foutieve of incomplete datum/tijd-invoer moet duidelijk zichtbaar worden afgewezen, zonder oude waarden ongemerkt terug te plaatsen alsof er niets gebeurd is.

**Buiten scope:**

- Geen nieuwe analysefunctionaliteit.
- Geen wijziging aan de analysequery's, export JSON/CSV of database-aantallen.
- Geen algemene UI-frameworkmigratie.
- Geen persistent warning/error-logboek.

**Acceptatiecriteria:**

- Beheerder kan startdatum en starttijd handmatig typen en de waarde blijft staan na blur, Enter en Tab.
- Beheerder kan einddatum en eindtijd handmatig typen en de waarde blijft staan na blur, Enter en Tab.
- Wijzigen via de browser/picker werkt voor beide velden en wordt correct gebruikt bij analyse ophalen.
- Ongeldige of incomplete invoer geeft duidelijke validatiefeedback.
- Analyse ophalen gebruikt het gekozen tijdsvenster en valt niet terug naar het oude venster zonder melding.
- Regressiecheck: JSON/CSV export blijft werken voor het gekozen tijdsvenster.

**Legacy coverage impact:**

- Verbetert de bruikbaarheid van `SYS-ANALYSIS-1` en daarmee de `Partial` dekking van `US8.6 Raspberry Pi-configuratie beheren`.
- Geen statuswijziging voor `US8.11 Systeemactie-logboek`; persistent events/logboek blijven open.

**Handmatige testnotities:**

- Test lokaal in de browser met toetsenbord: type begin/einddatum en tijd, gebruik Tab en Enter, en controleer dat de waarden blijven staan.
- Test lokaal met de browser/picker voor beide velden.
- Klik daarna analyse ophalen en controleer dat het gekozen venster zichtbaar in de resultaten/export terugkomt.
- Herhaal bij voorkeur op de Pi of tegen de Pi-webinterface na merge.

---

### SYS-CTRL-1: Ingest verwerken aan of uit kunnen zetten via de webinterface

**Status:** ✅ Geïmplementeerd, gemerged naar `master` via PR #71 en handmatig gevalideerd op Raspberry Pi op 2026-05-29.

**User Story:** Als gebruiker wil ik ingest-verwerking via het dashboard kunnen aan- of uitzetten en in dashboard/logboek duidelijk gewaarschuwd worden wanneer verwerking uit staat, zodat ik bewust havenlogging kan stoppen zonder per ongeluk een reis te starten of bij te houden zonder automatische meetdata.

**Scope:**

- Een centrale operationele boolean introduceren of hergebruiken, bijvoorbeeld `IngestProcessingEnabled`.
- De dashboardpagina krijgt een duidelijke toggle voor ingest-verwerking aan/uit.
- Dashboard toont de actuele status en een opvallende informatie-/waarschuwingsbalk zolang verwerking uit staat.
- Logboek toont ook een opvallende permanente balk zolang verwerking uit staat.
- Bij het aanmaken van een nieuwe reis verschijnt een opvallende popup/waarschuwing als ingest-verwerking uit staat.
- `BootManager.Tools.Ingest` gebruikt dezelfde centrale instelling als de UI.
- Als `IngestProcessingEnabled = false`, verwerkt Ingest geen nieuwe binnenkomende regels richting API/database.
- Als verwerking weer aan staat, hervat Ingest het posten naar de Web API zonder containerrestart.
- Waarschuwingstekst maakt duidelijk dat automatische meetdata/logboekondersteuning niet actueel wordt gevuld zolang verwerking uit staat.

**Buiten scope:**

- Geen brede scheduler of automatische havenmodus.
- Geen complexe regels op basis van locatie, tijd of beweging in deze eerste slice.
- Geen live dashboard-meters in deze story.
- Geen persistent systeemactie-logboek.

**Acceptatiecriteria:**

- Gebruiker kan ingest-verwerking via het dashboard uitzetten en weer aanzetten.
- De actuele status is zichtbaar op het dashboard.
- Dashboard toont een opvallende informatie-/waarschuwingsbalk zolang verwerking uit staat.
- Logboek toont een opvallende permanente balk zolang verwerking uit staat.
- Als gebruiker een nieuwe reis aanmaakt terwijl verwerking uit staat, verschijnt een opvallende popup/waarschuwing.
- De dashboardbalk en logboekbalk verdwijnen zodra verwerking weer aan staat.
- De UI-status en het feitelijke ingest-gedrag gebruiken dezelfde bron en spreken elkaar niet tegen.
- Als verwerking uit staat, mag de UDP listener nog verkeer ontvangen, maar Ingest post niets naar de Web API en er ontstaat geen nieuwe raw/measurement database-opslag.
- Als verwerking weer aan staat, hervat Ingest posten naar de Web API zonder containerrestart.
- Handmatige dev- of Pi-test bevestigt toggle, opslaggedrag, dashboardbalk, logboekbalk en popup.

**Legacy coverage impact:**

- Sluit direct aan op `US-SYS7 Ingest verwerken aan of uit zetten` uit de proposed backlog.
- Raakt `US5.2 Automatisch loggen en intervalinstelling`, omdat logboekondersteuning afhankelijk is van beschikbare meetdata; verwachte status blijft `Partial`.
- Raakt `US7.3 Waarschuwingen en meldingen`, omdat dashboard/logboek waarschuwingen tonen; verwachte status blijft `Partial`.
- Raakt `US8.5 Sensorintegratie configureren` en `US8.6 Raspberry Pi-configuratie beheren`; verwachte status blijft `Partial` omdat brede sensorconfiguratie en volledig Pi-beheer niet worden afgerond.
- Vinkt `US8.11 Logboek van systeemacties bekijken` niet af; er komt geen persistent systeemactie-logboek.

**Handmatige testnotities:**

- Test bij voorkeur lokaal eerst en daarna op Raspberry Pi.
- Controleer database-aantallen via analysepagina vóór/na uitschakelen.
- Controleer dat uitgeschakelde verwerking geen nieuwe `NetworkMessages` of measurements oplevert.
- Controleer dat inschakelen zonder containerrestart nieuwe opslag laat hervatten.
- Controleer dashboardbalk, logboekbalk en popup bij nieuwe reis.

**Implementation Status (2026-05-29):**

- `IngestProcessingEnabled` is toegevoegd aan de operationele instellingen en via de ingest-settings API beschikbaar gemaakt.
- Dashboard toont alleen de ingest-verwerking status/toggle en verwijst voor geavanceerde instellingen naar `/settings`.
- Dashboard-toggle gebruikt de reload-flow zodat de draaiende Ingest runtime-instellingen live worden bijgewerkt.
- Logboek toont een waarschuwing zolang ingest-verwerking uit staat.
- Nieuwe reis aanmaken toont een bevestigingsmodal wanneer ingest-verwerking uit staat.
- `BootManager.Tools.Ingest` gebruikt geen database- of Infrastructure-reference; Ingest haalt settings via Web API/control-flow op en post meetdata alleen via de bestaande Web API.
- Disabled-mode is geoptimaliseerd: na UDP receive en line extraction wordt vroeg geskipte data geteld, zonder parsing, capture logging, sampling of API-post; logging is gethrottled.
- Lokale handmatige test door gebruiker bevestigde dashboard-toggle, logboekwaarschuwing, popup bij nieuwe reis, live stop/start van Ingest en disabled-mode gedrag.

**Open vervolg:**

- Volledige sensorconfiguratie, automatische havenmodus, scheduler, live dashboardmeters en persistent systeemactie-logboek blijven aparte stories.

---

### SYS-CTRL-2: Ingest reload robuust maken tegen foutieve ApiBaseUrl

**Status:** ✅ Geïmplementeerd, gemerged naar `master` via PR #72 en handmatig gevalideerd op Raspberry Pi op 2026-05-29.

**User Story:** Als beheerder wil ik dat Ingest operationele instellingen betrouwbaar kan herladen, ook als de opgeslagen `ApiBaseUrl` fout staat, zodat de Pi niet vastloopt in een situatie waarin de UI wel instellingen opslaat maar de draaiende Ingest-runtime ze niet meer kan ophalen.

**Aanleiding:**

Tijdens Pi-validatie na `SYS-CTRL-1` bleek dat de operationele instellingen in de database nog de dev-URL `http://localhost:5046` bevatten. Binnen de Docker-container betekent `localhost` de ingest-container zelf, waardoor Ingest geen Web API kon bereiken. De UI kon de instelling wel opslaan en de Ingest control API bereiken, maar `POST /reload-settings` probeerde de nieuwe settings op te halen via de foutieve runtime `ApiBaseUrl` en gaf daardoor `Service Unavailable`.

Na handmatig herstellen van `ApiBaseUrl` naar `http://bootmanager-web:5000` en restart van `bootmanager-ingest` werkte de Pi-correct:

- Ingest haalde settings op via `http://bootmanager-web:5000/api/operationalsettings/ingest`.
- `IngestProcessingEnabled=False` werd toegepast.
- Nieuwe UDP-regels werden geteld als skipped en niet naar de API gepost.

Er bleef één extra observatie over: ondanks `CaptureLoggingEnabled=False` werd bij startup nog "Capture logging ingeschakeld" gelogd.

**Scope:**

- `POST /reload-settings` in Ingest mag niet uitsluitend afhankelijk zijn van de mutable runtime `ApiBaseUrl`.
- Reload gebruikt de vaste ingest-config/bootstrap URL uit `Ingest__ApiBaseUrl` als primaire of fallback route naar BootManager.Web.
- Als de database `ApiBaseUrl` een dev-waarde zoals `http://localhost:5046` bevat op Docker/Pi, moet dit duidelijker gelogd/gefaald worden of herstelbaar blijven via reload/restart.
- Controleer waarom `CaptureLoggingEnabled=False` wordt opgehaald maar startup toch "Capture logging ingeschakeld" logt; fix dit als het functioneel fout is.
- Geen directe database-access vanuit `BootManager.Tools.Ingest`; alleen Web API/control-flow blijft toegestaan.

**Buiten scope:**

- Geen nieuwe UI voor settings.
- Geen automatische migratie van bestaande foutieve Pi-settings.
- Geen persistent systeemactie-logboek.
- Geen wijziging aan UDP listener gedrag of dashboard-toggle scope.

**Acceptatiecriteria:**

- Ingest reload kan settings ophalen via de vaste compose/config URL `http://bootmanager-web:5000`, ook als runtime `ApiBaseUrl` fout staat.
- Foutieve of onbereikbare runtime `ApiBaseUrl` blokkeert herstel via reload niet blijvend.
- Ingest blijft architecturaal gescheiden van database/Infrastructure.
- `CaptureLoggingEnabled=False` leidt niet tot actieve capture logging, of de logging verklaart correct waarom restart nodig is.
- Build en relevante ingest/settings tests slagen.
- Handmatige test op Pi: foutieve `ApiBaseUrl` herstellen naar `http://bootmanager-web:5000`, reload/restart controleren, daarna toggle uit en geen nieuwe API-posts zien.

**Legacy coverage impact:**

- `US8.5 Sensorintegratie configureren`: blijft `Partial`, maar betrouwbaarheid van operationele ingest-config verbetert.
- `US8.6 Raspberry Pi-configuratie beheren`: blijft `Partial`, Pi-beheer wordt robuuster.
- `US8.11 Systeemactie-logboek`: blijft `Open`, want deze story slaat runtime events nog niet persistent op.

**Handmatige testnotities:**

- Lokaal of op Pi simuleren dat runtime `ApiBaseUrl` fout staat en dat reload alsnog via de vaste config/bootstrap URL kan herstellen.
- Op de Pi controleren dat `curl -s http://localhost:5000/api/operationalsettings/ingest` `apiBaseUrl` als `http://bootmanager-web:5000` teruggeeft.
- Na `docker compose restart bootmanager-ingest` controleren dat logs settings via `http://bootmanager-web:5000` ophalen.
- Met ingest-verwerking uit controleren dat logs alleen skipped summaries tonen en geen nieuwe `POST /api/networkmessages` regels.

**Implementation Status (2026-05-29):**

- `POST /reload-settings` probeert nu eerst de vaste configured/bootstrap `Ingest__ApiBaseUrl` en gebruikt de mutable runtime `ApiBaseUrl` alleen als fallback wanneer de configured URL faalt.
- Hiermee kan een foutieve runtime/databasewaarde zoals `http://localhost:5046` herstel via reload niet blijvend blokkeren zolang de compose/config bootstrap URL correct is.
- `IngestCaptureLogger` gebruikt nu de effectieve combinatie van appsettings en runtime/database: capture logging is alleen actief wanneer `Ingest__CaptureLogging__Enabled=true` én `CaptureLoggingEnabled=true`.
- Als `CaptureLoggingEnabled=false` uit de database komt, wordt geen capture-logbestand aangemaakt en schrijft Ingest niets naar capture logs, ook niet wanneer compose capture logging technisch aan heeft staan.
- Nieuwe unit tests dekken capture logging aan/uit-combinaties en de reload URL-volgorde/fallback.
- Architectuurcontrole: `BootManager.Tools.Ingest` heeft geen Infrastructure/database-reference en gebruikt voor instellingen alleen Web API/control-flow.
- Verificatie: `dotnet build BootManager.sln` geslaagd met `0` warnings/errors; gerichte tests `IngestTools|OperationalSettings` geslaagd met `35/35`.
- Handmatige lokale test door gebruiker is akkoord bevonden.
- Pi-validatie na merge bevestigde:
  - containers draaiden gezond na `git pull`, build en restart;
  - `GET /api/operationalsettings/ingest` gaf `apiBaseUrl=http://bootmanager-web:5000`, `CaptureLoggingEnabled=False` en `IngestProcessingEnabled=False`;
  - Ingest startup haalde settings op via `http://bootmanager-web:5000`;
  - effectieve capture logging stond uit ondanks appsettings/compose `true`, omdat database/runtime `false` was;
  - bij uitgeschakelde verwerking werden ontvangen UDP-regels alleen als skipped geteld en verschenen geen nieuwe `POST /api/networkmessages` regels.

---

### SYS-DEPLOY-LEAN-1: Pi Deployment Zonder Ontwikkel- En Documentatiebestanden

**Status:** Goedgekeurd voor latere uitwerking op 2026-05-27.

**User Story:** Als beheerder van de Raspberry Pi-installatie wil ik een deployment-checkout gebruiken zonder projectdocumentatie, legacy-analyse en andere niet-benodigde ontwikkelbestanden in de actieve Pi-werkmap, zodat de Pi alleen de minimaal benodigde BootManager-bestanden bevat voor build en runtime.

**Scope:**

- Bepalen en vastleggen welke repo-inhoud echt nodig is op de Pi voor `master`-pull, `docker compose build` en `docker compose up`.
- Een concrete deployment-aanpak kiezen voor een “lean” Pi-checkout.
- De gekozen aanpak documenteren en opneembaar maken in de bestaande Pi/deployment-runbooks.
- Expliciet benoemen wat het effect is op werkmap-inhoud, update-commando’s en beheerbaarheid.

**Buiten scope:**

- Geen brede herstructurering van de hele repository.
- Geen verandering aan functionele applicatiefeatures.
- Geen onmiddellijke overstap naar een volledige CI/CD- of container-registry-oplossing, tenzij dat expliciet de gekozen aanpak wordt in een latere story.
- Geen automatische verwijdering van bestanden op bestaande Pi-installaties zonder duidelijke operatorstappen.

**Acceptatiecriteria:**

- Er is een expliciete keuze gemaakt tussen bijvoorbeeld sparse-checkout, deploy-artifact of andere afgeslankte deployment-aanpak.
- De documentatie legt uit wat wel en niet op de Pi terechtkomt.
- De documentatie legt uit hoe een Pi-update daarna exact uitgevoerd moet worden.
- Bekend is of `.md`/legacy-bestanden alleen uit de werkmap verdwijnen of ook echt niet meer via de deploymentstroom meegaan.
- De gekozen aanpak past bij de afspraak dat de Pi alleen `master` volgt.

**Legacy coverage impact:**

- Geen directe legacy-user-story die hiermee volledig wordt afgevinkt.
- Raakt het dichtst aan `US8.6 Raspberry Pi-configuratie beheren`, maar vooral als BootManagerV2-specifieke deployment-hardening.
- Verwachte legacy-status: waarschijnlijk geen directe statuswijziging, hoogstens extra onderbouwing bij bestaande `Partial` system/deployment-dekking.

**Handmatige testnotities:**

- Verifiëren welke bestanden na eerste setup of update echt in de Pi-werkmap staan.
- Verifiëren dat `docker compose build` en `docker compose up -d` blijven werken met de gekozen aanpak.
- Verifiëren dat een update vanaf `master` nog reproduceerbaar is zonder handmatige repo-reparaties.

**Planning-opmerking:**

- Deze story moet opnieuw expliciet in beeld komen zodra BootManager richting een eerste deployment voor een andere bootbezitter en dus een andere Raspberry Pi gaat.
