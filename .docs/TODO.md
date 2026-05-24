# BootManager Status & Roadmap

## Current Implementation Status

### ✅ Completed Vertical Slices

| Measurement Type | PGN | Core Entity | Parser | Interpreter | Service | Storage | Status |
|------------------|-----|-------------|--------|-------------|---------|---------|--------|
| **Battery** | 127508 | ✅ | ✅ | ✅ | ✅ | ✅ | Complete |
| **Depth** | 128267 | ✅ | ✅ | ✅ | ✅ | ✅ | Complete |
| **Wind** | 130306 | ✅ | ✅ | ✅ | ✅ | ✅ | Complete |
| **Motion** (COG/SOG) | 129026 | ✅ | ✅ | ✅ | ✅ | ✅ | Complete |
| **Position** | 129025 | ✅ | ✅ | ✅ | ✅ | ✅ | Complete |
| **Heading** | 127250 | ✅ | ✅ | ✅ | ✅ | ✅ | Complete |
| **Speed Through Water** | 128259 | ✅ | ✅ | ✅ | ✅ | ✅ | Complete |
| **Water Temperature** | 130312 | ✅ | ✅ | ✅ | ✅ | ✅ | Complete |

### 📋 Backlog

#### High Priority

- [ ] **Operationele instellingen via UI/database** *(fundament voor ingest/sampling)*
  - **Status 2026-05-23:** basis geïmplementeerd: `/settings` bevat operationele instellingen voor luisteradres, poorten, API basis-URL, raw opslagmodus, standaard sample-interval en capture logging.
  - **Status 2026-05-23 (slice 2):** Ingest haalt bij startup operationele instellingen op via `GET /api/operationalsettings/ingest`. appsettings.json blijft fallback als Web niet bereikbaar is. Settings worden niet live herladen tijdens runtime.
  - **Status 2026-05-23 (slice 3):** `RawStorageMode` en `DefaultSampleIntervalSeconds` zijn volledig toegepast:
    - `IIngestSamplingPolicy` en `IngestSamplingPolicy` implementeren per-stream-key sampling.
    - `RawStorageMode.All`: alle berichten naar API/database (bestaand gedrag).
    - `RawStorageMode.Sampled`: maximaal 1 bericht per stream key per interval.
    - `RawStorageMode.OffAfterSuccessfulParse`: voorlopig gelijk aan Sampled; post-parse cleanup volgt later.
    - Stream key is `Protocol:MessageId` (genormaliseerd). Capture logging onafhankelijk van sampling.
    - Bij interval ≤ 0 wordt fallback naar 10 seconden met waarschuwing.
    - Unit tests dekken alle modes en edge cases.
  - **Status 2026-05-23 (slice 4 - Ingest Control API & Settings Reload):** Live settings reload zonder procesrestart geïmplementeerd:
    - **Ingest control server:** HttpListener-gebaseerde minimale API op localhost (127.0.0.1:5010 standaard, configureerbaar).
    - **Endpoints:**
      - `GET /status`: geeft huidige runtime settings + restart-required flags voor UDP listener.
      - `POST /reload-settings`: haalt settings opnieuw op, appliceert veilige settings live (ApiBaseUrl, RawStorageMode, DefaultSampleIntervalSeconds), rapporteert restart-required voor ListenAddress/ListenPort/CaptureLoggingEnabled.
    - **Runtime settings service:** IIngestRuntimeSettings + IngestRuntimeSettings voor thread-safe live updates.
    - **Sampling policy updates:** IIngestSamplingPolicy.Update(mode, interval) voor live RawStorageMode en interval wijzigingen met state reset bij mode-wijziging.
    - **Web-side client:** IngestControlClient om control API aan te roepen.
    - **Settings endpoint:** POST /api/operationalsettings naast GET; appliceert settings en verzendt reload-commando naar Ingest via control client.
    - **UI feedback:** duidelijke melding van applied fields, restart-required fields en connection status (unreachable).
    - **Thread-safety:** UDP loop en reload kunnen gelijktijdig plaatsvinden via lock-based runtime settings.
    - **Security:** control API bindt standaard alleen op 127.0.0.1; geen binding op 0.0.0.0.
    - Unit tests valideren runtime settings thread-safety en live policy updates.
  - **Vervolg:** post-parse raw-retentie toepassen (Web moet succesvol parsen rapporteren).

- [ ] **Digitaal Logboek** *(epic – UI richting eindgebruiker)*
  - **Aanleiding:** Dataverwerking voor ondersteunde YDEN/NMEA0183-data werkt voldoende om richting een bruikbaar logboek te gaan.
  - **Referentie:** bestaand logboekvoorbeeld in `.docs/extraInfo/LogboekVoorbeeld.png`.
  - **Status:** basislogboek, reis-samenvatting, meetdatasuggesties, read-only detailpagina per logboekregel, browser-printweergave, akkoordflow (LogbookEntryStatus: Draft/Confirmed), en ontbrekende logmomenten feature (banner met overzicht en bulk-aanmaak tot 24 Draft-regels) geïmplementeerd op 2026-05-24. Delete-functionaliteit per regel toegevoegd.
  - **Status 2026-05-24 (accordeer-slice):** Detailpagina herontworpen als accordeerhulpmiddel:
    - **Context-header:** reisnaam, logboektijd lokaal, **logtijdvak lokaal** (HH:mm — HH:mm), status badge.
    - **Waarschuwingen:** duidelijke alerts voor ontbrekende periode-start en geen meetdata in logtijdvak.
    - **Logregelwaarden:** compacte overzicht van alle velden (Barometer, Log, Koers, Gem. SOG, Wind, GPS-status, Breedtegraad, Lengtegraad, Opmerkingen), met "Nog niet ingevuld" voor lege waarden.
    - **Meetdata-overzicht:** samplecounts per meettype in visueel 6-kolom grid (Positie, COG/SOG, Heading, Wind, Diepte, Watertemperatuur).
    - **Sampletabellen:** secundair onder "Samples"-kopje, alleen zichtbaar als samples beschikbaar.
    - **Verwijderde elementen:** "Bronmetingen voor automatische suggesties" sectie verwijderd; `CourseBron`, `WindBron`, `PositieBron` DTOs en bijbehorende service-methodes verwijderd. Reden: oude bronmetingen vóór logtijdvak conflicteren met domeinregel dat Draft-regels alleen periode-data gebruiken.
    - **DTO-cleanup:** `LogbookSourceMeasurementDto` klasse en properties uit `LogbookEntryDetailDto` verwijderd.
    - **Service-cleanup:** `BepaalCourseSourceAsync()`, `BepaalWindSourceAsync()`, `BepaalPositieSourceAsync()` verwijderd.
  - **Huidige detailweergave:** `/logbook/entries/{entryId:int}/details` toont accordeer-layout met waarschuwingen, logregelwaarden, meetdata-overzicht en sampletabellen.
  - **Huidige printweergave:** `/logbook/trips/{tripId:int}/print` toont alleen logboekinhoud in een printvriendelijke layout; PDF loopt via browser print. Alleen Confirmed-regels worden afgedrukt.
  - **Huidige missing-moments-feature (2026-05-24):** banner boven logboektabel toont totaalaantal gemiste logmomenten en compacte lijst (max 5 zichtbaar + "+ meer"); knop "Conceptregels aanmaken" maakt tot 24 Draft-regels in één beurt aan met automatische meetdatasuggesties (alleen periode-data); herberekening van banner na batch. Delete-knop (🗑) per regel met bevestigingsdialoog; na verwijdering herbereken banner.
  - **Volgende slice:** gerelateerde features als detailweergave verbeteren of browser push notifications.
  - Zie: [.docs/epics/digital-logbook.md](epics/digital-logbook.md)

- [ ] **Authentication & Authorization**
  - JWT-based API authentication
  - Role-based access control (Admin, User, Viewer)
  - Secure owner profile management

- [ ] **Query API Enhancements**
  - Date range filtering on measurements
  - Pagination support
  - Aggregation queries (avg, max, min over time windows)

- [ ] **Data Visualization**
  - Blazor dashboard with real-time charts
  - Historical trend display
  - Map integration for Position data

#### Medium Priority

- [ ] **Raspberry Pi/Docker deployment & veilige shutdown**
  - **Aanleiding 2026-05-23:** BootManager moet later op een Raspberry Pi in Docker kunnen draaien. Bij direct stroomloos maken kan een Raspberry Pi/SD-kaart en SQLite-database corrupt raken.
  - **Doel:** Docker deployment ontwerpen met correcte netwerkkeuzes voor UDP ingest, persistente volumes voor database/logs en een veilige afsluitflow.
  - **Aandachtspunten:**
    - UDP-poorten correct mappen of bewust `host networking` gebruiken.
    - Web en Ingest praten binnen Docker niet vanzelf via `localhost`; gebruik service names/netwerkconfiguratie of host networking.
    - SQLite/database en capture logs moeten op persistente volumes staan.
    - Ingest/Web moeten netjes reageren op container shutdown (`SIGTERM`) en open writes afsluiten.
    - Control API blijft intern/lokaal bereikbaar, niet publiek.
  - **Latere UI-story:** owner/admin knop "Systeem afsluiten" met bevestiging. Web mag niet rechtstreeks vrije shell-commando's uitvoeren; gebruik een beperkte lokale helper/service die Docker/OS veilig afsluit.
  - **Gebruikersmelding:** "Wacht tot de Raspberry Pi volledig uit is voordat je de stroom loshaalt."

- [ ] **Extended Heading Fields**
  - Deviation storage (magnetic correction)
  - Variation storage (declination)
  - Reference type tracking (True vs. Magnetic)

- [ ] **NMEA 0183 Support** *(epic – gefaseerd)*
  - **Aanleiding:** YDEN-03 gateway zendt NMEA 2000-data als NMEA 0183 sentences uit via UDP (poort 2000 en 10110) en TCP (poort 1456).
  - **Fase 1 – Foundation ✅:** één gecombineerde UDP listener in Ingest, protocoldetectie op regelinhoud, raw NMEA 0183 opslag
  - **Fase 2 – Parser laag ✅:** `Nmea0183ParserService` in Application voor sentence-type herkenning, veldextractie en checksum-validatie; integratie in `NetworkMessageService` voor `Protocol == NMEA0183`
  - **Fase 3 – Interpreters ✅:** VHW, MWV, DBT/DPT, RMC/GGA, HDT/HDM en MTW verticale slices
  - **Simulator NMEA 0183 ✅:** standaard NMEA 0183 output via `BootManager.Tools.Simulator`
  - **Runtime/SQLite acceptatietest ✅:** handmatig uitgevoerd via simulator NMEA0183-modus
  - **Echte boot-test 2026-05-23:** UDP/raw opslag werkt; vervolgstories zijn deels afgerond voor AIS `!`-sentences, NMEA0183 `MessageId` en realistischer simulatorprofiel; capture replay is geparkeerd
  - [x] Story 1: Ingest herkent `$...` én `!...` als `Protocol = "NMEA0183"`; raw-like simulatorregels blijven `NMEA2000`
  - [x] Story 2: `Nmea0183ParserService` accepteert `$` en `!`; `!AIVDM` parsed als talker `AI`, type `VDM`
  - [x] Story 3: NMEA0183 krijgt stabiele niet-lege `MessageId` op basis van sentence-id, zodat derived measurements opgeslagen worden
  - [x] Story 4: Simulator krijgt een YDEN03-achtig profiel met `YD` talker-prefixen, AIS `!AIVDM`/`!AIVDO` en raw-only YDEN-sentences
  - [ ] Story 5: Replay-validatie voor echte NDJSON capture naar API/SQLite
  - [ ] Future story: Ingest haalt operationele instellingen op uit BootManager.Web en gebruikt die voor configureerbare ingest/sampling-retentie; ruwe niet-geparseerde data kan na succesvolle periodieke parsing optioneel worden opgeschoond
  - Bestaande NMEA2000 slices blijven intact
  - Raw opslag altijd leidend; onbekende sentences worden opgeslagen maar niet verwerkt
  - Zie: [.docs/epics/nmea0183-support.md](epics/nmea0183-support.md) en [.docs/features/nmea0183-parser-interpreter-architecture.md](features/nmea0183-parser-interpreter-architecture.md)

- [ ] **Logging & Diagnostics**
  - Serilog integration with structured logging
  - Correlation IDs for message tracing
  - Performance metrics collection

#### Low Priority

- [ ] **Additional PGN Support**
  - Barometric Pressure (PGN 130314)
  - Additional navigation data

- [ ] **Export/Reporting**
  - CSV export of measurements
  - Time-series reports
  - Compliance reporting (if needed)

- [ ] **Historical Data Management**
  - Archive old data
  - Data retention policies
  - Backup/restore procedures

## Architecture Status

### Strengths ✅

- **Clear layering:** Core → Application → Infrastructure → Web
- **Consistent patterns:** All measurement types follow identical structure
- **Error resilience:** Parse/interpret/store errors are non-fatal
- **Extensibility:** New PGN support requires minimal changes
- **Type safety:** Enums, DTOs, strong typing throughout

### Improvements Needed 🔧

- **API documentation:** Swagger/OpenAPI not yet configured
- **Integration tests:** Only the vertical slice pattern is validated
- **Performance metrics:** No benchmarking or load testing done
- **Error codes:** API returns generic HTTP status, needs specific error codes
- **Audit logging:** No audit trail for data changes
- **Data validation:** Range checks are service-level, not schema-level

## Known Limitations

1. **No persistent authentication state** - Every API call should include credentials (implement session/token caching)

2. **Simulator is independent** - Not integrated with API; requires Ingest tool as intermediary

3. **No real-time updates** - Blazor components don't get WebSocket notifications; need SignalR or polling

4. **Single-instance database** - No replication or clustering support

5. **Heading payload incomplete** - Deviation/Variation/Reference fields available but not decoded

## Recent Changes

### 2026-05-17: NMEA 0183 Fase 2 – Parser laag geïmplementeerd

**Toegevoegd:**
- `BootManager.Application/NetworkMessageParsing/DTOs/Nmea0183ParseResultDto.cs` – DTO met TalkerPrefix, SentenceType, Fields, ChecksumValid, ErrorMessage
- `BootManager.Application/NetworkMessageParsing/Services/INmea0183ParserService.cs` – interface voor NMEA 0183 parser
- `BootManager.Application/NetworkMessageParsing/Services/Nmea0183ParserService.cs` – implementatie: talker-prefix herkenning, veldextractie, XOR-checksum validatie

**Bijgewerkt:**
- `BootManager.Application/NetworkMessages/Services/NetworkMessageService.cs` – `INmea0183ParserService` geïnjecteerd; parse-aanroep toegevoegd voor `Protocol == "NMEA0183"`; parse-fouten blokkeren raw opslag niet
- `BootManager.Application/DependencyInjection.cs` – `INmea0183ParserService` als Scoped geregistreerd

**Gedrag:**
- Sentences zoals `$IIVHW,...`, `$GPRMC,...`, `$WIMWV,...` worden herkend op Talker + SentenceType
- Ongeldige of onbekende sentences worden gelogd; raw opslag blijft altijd leidend
- Bestaande NMEA2000 flow (via MessageId/PayloadHex) is ongewijzigd
- Runtime-tests zijn niet uitgevoerd; `dotnet build` en `dotnet test` zijn geslaagd (1 niet-gerelateerde authenticatietest gefaald)

**Status:** Geïmplementeerd, geen EF migrations

### 2026-05-17: NMEA 0183 Parser/Interpreter Architectuur – Documentatie

**Toegevoegd:**
- `.docs/features/nmea0183-parser-interpreter-architecture.md` – gedetailleerd ontwerpdocument voor Fase 2/3: parser/interpreter-aanpak, sentence-prioriteiten, entity-mappings, vaststaande keuzes en open ontwerpvragen

**Bijgewerkt:**
- `.docs/epics/nmea0183-support.md` – Fase 2 uitgebreid met scope, acceptatiecriteria en link naar architectuurdoc; sectie open ontwerpvragen toegevoegd
- `.docs/ARCHITECTURE.md` – Fase 1 als done gemarkeerd, parser/interpreter routeringschema toegevoegd, sentence-prioriteitstabel, link naar ontwerpdoc
- `.docs/TODO.md` – Fase 1 als done gemarkeerd, Fase 2 als eerstvolgende implementatie-story aangeduid
- `.docs/features/README.md` – verwijzing naar nmea0183-parser-interpreter-architecture.md toegevoegd
- `docs/bootmanager_codex_handoff.md` – eerstvolgende story en open vragen bijgewerkt

**Status:** Documentatie bijgewerkt, geen codewijzigingen

### 2026-05-17: NMEA 0183 Epic – Documentatie

**Toegevoegd:**
- `.docs/epics/nmea0183-support.md` – volledig epicdocument met gefaseerde aanpak
- `.docs/extraInfo/yden-03.md` – YDEN-03 gateway configuratie en context

**Bijgewerkt:**
- `.docs/ARCHITECTURE.md` – NMEA 0183 sectie toegevoegd (parallelle flow, protocol tagging, fasering)
- `.docs/TODO.md` – NMEA 0183 backlog-item uitgebreid
- `.docs/features/README.md` – NMEA 0183 epic en YDEN-03 context toegevoegd
- `docs/bootmanager_codex_handoff.md` – hardwarecontext en NMEA 0183 epic opgenomen

**Status:** Documentatie bijgewerkt, geen codewijzigingen

### 2026-03-27: Heading Slice Implementation

**Added:**
- `HeadingMeasurement` entity + EF configuration
- `HeadingMessageInterpreterService` (PGN 127250 decoder)
- `IHeadingMeasurementService` + implementation
- Parser recognition of PGN 127250
- Integration into `NetworkMessageService`
- Full dependency injection setup

**Documentation:**
- `.docs/ARCHITECTURE.md` - System design overview
- `.docs/DEVELOPMENT.md` - Dev workflow & guidelines
- `.docs/features/heading-slice-spec.md` - Heading slice detailed spec

**Status:** Complete (inclusief EF migratie `AddHeadingMeasurement`)

**Build:** ✅ Compileert succesvol

## Next Steps (Immediate)

### 1. Digitaal Logboek – vervolgslices

De basis voor het eindgebruikerslogboek is geïmplementeerd:

- `LogbookTrip` entity voor reis-header en reis-samenvatting (nu met per-reis `LogIntervalMinutes`).
- `LogbookEntry` entity voor logboekregels per uur/event (nu met `Status: Draft | Confirmed`).
- `/logbook` Blazor-pagina met kolommen uit `.docs/extraInfo/LogboekVoorbeeld.png`.
  - Banner met "Volgende logmoment verstreken" indien vervallen moment gedetecteerd.
  - Knop "Conceptregel maken" → maakt automatische Draft-regel voor gemist moment.
- Handmatige invoer voor opmerkingen/zeilvoering en basisvelden.
- Meetdatasuggesties op basis van bestaande measurements.
  - **2026-05-24:** Draft-regels gebruiken nu ALLEEN meetdata BINNEN het logtijdvak (kritieke veiligheidsfix).
  - Handmatige regels behouden "laatst bekende vóór logmoment" voor gebruikergemak.
- Read-only detailpagina per logboekregel met samples en samenvattingen.
  - Toont opgeslagen waarden, periode-samples en bronmetingen apart.
  - Lege Draft-regels tonen correct "Geen data" wanneer geen meetdata in logtijdvak.
- Browser-printweergave per reis zonder app-menu/topbar in de afdruk (Confirmed-only).

Zie: [.docs/epics/digital-logbook.md](epics/digital-logbook.md)

### 2. Later

- Sampling/raw-retentiebeleid toepassen op database-opslag.
- Server-side PDF-generatie.
- Bijlagen uploaden.
- Query API enhancements voor meetdata over tijdvakken.

## Deployment Checklist (Future)

- [ ] Production database setup (not SQLite)
- [ ] Raspberry Pi Docker deployment ontwerp
- [ ] Persistent volumes voor database en logs
- [ ] UDP ingest netwerkkeuze: port mapping versus host networking
- [ ] Veilige shutdown-flow voor Raspberry Pi vanuit UI/helper-service
- [ ] Authentication implementation
- [ ] Rate limiting & API security
- [ ] Monitoring & alerting
- [ ] Backup & disaster recovery
- [ ] Performance load testing
- [ ] User acceptance testing (UAT)

## Metrics & Health Checks

### Current State
- **Lines of Code:** ~15,000 (excluding tests)
- **Test Coverage:** TBD (no unit tests yet)
- **Database Size:** ~1MB (typical, depends on message volume)
- **API Response Time:** <100ms (typical, unload tested)

### Desired State
- **Test Coverage:** >80%
- **API Response Time:** <50ms (p95)
- **Uptime:** 99.9%
- **Data Consistency:** 100% (ACID compliance)

---

**Last Updated:** 2026-05-24
**Maintained By:** Development Team
