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

- [ ] **Extended Heading Fields**
  - Deviation storage (magnetic correction)
  - Variation storage (declination)
  - Reference type tracking (True vs. Magnetic)

- [ ] **NMEA 0183 Support** *(epic – gefaseerd)*
  - **Aanleiding:** YDEN-03 gateway zendt NMEA 2000-data als NMEA 0183 sentences uit via UDP (poort 2000 en 10110) en TCP (poort 1456).
  - **Fase 1 – Foundation:** tweede UDP listener in Ingest, protocol tagging op `NetworkMessage`, raw NMEA 0183 opslag
  - **Fase 2 – Parser laag:** `Nmea0183ParserService` in Application voor sentence-type herkenning
  - **Fase 3 – Interpreters:** per sentence-type (VHW, MWV, DBT/DPT, RMC/GGA, HDT/HDM, MTW) een verticale slice
  - Bestaande NMEA2000 slices blijven intact
  - Raw opslag altijd leidend; onbekende sentences worden opgeslagen maar niet verwerkt
  - Zie: [.docs/epics/nmea0183-support.md](epics/nmea0183-support.md)

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

### 1. Documentatie
- API docs bijwerken als Swagger wordt toegevoegd
- Voorbeeld-API-calls toevoegen voor nieuwe slices

### 2. Backlog prioritering
- Barometric Pressure (PGN 130314) als volgende verticale slice uitwerken
- NMEA 0183 Support evalueren als toekomstige uitbreiding

## Deployment Checklist (Future)

- [ ] Production database setup (not SQLite)
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

**Last Updated:** 2026-05-17  
**Maintained By:** Development Team
