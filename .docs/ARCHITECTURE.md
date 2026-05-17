# BootManager Architecture

## Overview

BootManager is een .NET 8 applicatie voor het ontvangen, parseren en opslaan van NMEA 2000-achtige netwerkberichten van maritieme sensoren.

## Layered Architecture

```
┌─────────────────────────────────────────────┐
│  BootManager.Web (API / Presentation)       │
│  - Controllers, Blazor endpoints            │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│  BootManager.Application (Business Logic)   │
│  - Services, DTOs, Interpreters, Parsers    │
│  - Feature-oriented organization            │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│  BootManager.Infrastructure (Data Access)   │
│  - EF Core, DbContext, Repositories         │
│  - Configurations, Migrations                │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│  BootManager.Core (Domain)                   │
│  - Entities, Interfaces, Value Objects      │
└─────────────────────────────────────────────┘
```

## Data Flow: Raw Message → Storage

```
Simulator/Sensor
      ↓
   Ingest Tool (reads raw lines)
      ↓
BootManager.Web API (CreateNetworkMessage endpoint)
      ↓
NetworkMessageService (raw message persisted)
      ↓
NetworkMessageParserService (technical parsing)
      ├─ MessageId → PGN classification
      ├─ PayloadHex → byte array
      └─ → NetworkMessageType enum
           ↓
    Type-specific Interpreter Service
      ├─ Payload bytes → semantic values
      └─ → InterpretationDto
           ↓
    Type-specific Measurement Service
      ├─ DTO validation
      ├─ Entity creation
      └─ → Repository.AddAsync()
           ↓
    Database (SQLite)
```

## Vertical Slices Pattern

Each data type (Battery, Depth, Wind, Motion, Position, Heading) follows an identical pattern:

### Slice Components

**Core Layer:**
- `{Type}Measurement` entity (e.g., `HeadingMeasurement`)

**Infrastructure Layer:**
- `{Type}MeasurementConfiguration` (EF Core mapping)
- `DbSet<{Type}Measurement>` in DbContext
- Database migration

**Application Layer:**
- `Create{Type}MeasurementRequestDto` (storage request)
- `{Type}MessageInterpretationDto` (parse result)
- `I{Type}MeasurementService` interface
- `{Type}MeasurementService` implementation
- `{Type}MessageInterpreterService` (PGN decoder)

**Integration:**
- Parser recognizes PGN → `NetworkMessageType` enum value
- `NetworkMessageService` orchestrates parse + interpret + store
- DI registration (Transient interpreter, Scoped service)

## Key Design Decisions

### Why separate Heading from Motion?

- **Motion (PGN 129026):** Course Over Ground (COG) + Speed Over Ground (SOG)
  - Navigation-focused: where the vessel is *moving*
  
- **Heading (PGN 127250):** Vessel Heading + Deviation + Variation
  - Compass-focused: which direction the vessel *points*
  - Can differ from motion (e.g., crabbing into wind)
  - Extensible for magnetic correction fields

### Payload Decoding Strategy

All NMEA 2000 payloads decoded as:
- **Little-endian** byte ordering
- **Scaled integers** (e.g., 1e-4 radians, centiknots, centimeters)
- Converted to **decimal/double** for storage and calculations
- **Range normalization** (e.g., headings 0-360°)

### Repository Pattern

All measurement services use **generic `IRepository<T>`** → `EfRepository<T>`:
- No per-entity repository classes
- Consistent persistence layer
- Logging integrated per service

### Parser → Interpreter Separation

- **Parser:** Technical only (PGN → type classification, hex → bytes)
- **Interpreter:** Semantic only (bytes → domain values)
- **Parser errors** don't block raw message storage
- **Interpreter errors** don't block database persistence

## NMEA 0183 Support (Epic)

### Aanleiding

De fysieke boot gebruikt een **YDEN-03 gateway** die NMEA 2000-busdata omzet naar NMEA 0183 sentences via UDP/TCP.
In de huidige YDEN-03-configuratie worden sentences verzonden op UDP-poort 2000 en 10110, en TCP-poort 1456.
BootManager moet daardoor NMEA 0183 sentences kunnen ontvangen en verwerken.

Zie: [.docs/extraInfo/yden-03.md](extraInfo/yden-03.md) en [.docs/epics/nmea0183-support.md](epics/nmea0183-support.md)

### Parallelle input

NMEA 0183 wordt als **parallelle inputstroom** naast de bestaande NMEA2000-keten ondersteund.
De bestaande verticale slices blijven intact.

### Data Flow: NMEA 0183 → Storage

```
YDEN-03 (UDP poort 2000 / 10110)
      ↓
   Ingest Tool (één gecombineerde UDP listener; protocolherkenning per regel)
      ↓
BootManager.Web API (CreateNetworkMessage, Protocol=NMEA0183)
      ↓
NetworkMessageService (raw sentence opgeslagen)
      ↓
[Fase 2] Nmea0183ParserService (sentence-type herkenning)
      ↓
[Fase 3] Sentence-specifieke Interpreter Service
      └─ → Type-specifieke Measurement Service → Database
```

Onbekende of niet-parsebare sentences worden raw opgeslagen en niet verder verwerkt.
Raw opslag is altijd leidend.

### Protocol tagging

`NetworkMessage` krijgt een `Protocol`-veld (`NMEA2000` / `NMEA0183`).
Het `Protocol`-veld op measurement entities is een expliciete latere ontwerpkeuze, niet nu automatisch ingevoerd.

### Gefaseerde uitwerking

| Fase | Inhoud |
|------|--------|
| **1 – Foundation** ✅ | Eén gecombineerde UDP listener in Ingest, protocolherkenning op regelinhoud, raw NMEA 0183 opslag |
| **2 – Parser laag** ✅ | `Nmea0183ParserService` in Application, sentence-type herkenning en veldextractie |
| **3 – Interpreters** ✅ | Per sentence-type: VHW, MWV, DBT/DPT, RMC/GGA, HDT/HDM, MTW |
| **Simulator** ✅ | Configureerbare NMEA 0183 output via settings; standaard `NMEA0183` |
| **Runtime-test** ✅ | Handmatige runtime/SQLite acceptatietest fase 3a-3c uitgevoerd |

### Parser/Interpreter scheiding voor NMEA 0183

Conform het bestaande NMEA2000-principe:

- **`Nmea0183ParserService` (technisch):** Herkent sentence-type, valideert structuur, extraheert velden.
- **Sentence-specifieke InterpreterService (semantisch):** Leidt meetwaarden af en produceert een `InterpretationDto`.

`NetworkMessageService` routeert op basis van `Protocol`:

```
NetworkMessageService
    ├─ Protocol == NMEA2000 → NetworkMessageParserService → type-specifieke interpreter
    └─ Protocol == NMEA0183 → Nmea0183ParserService → sentence-specifieke interpreter
```

### Sentence-prioriteiten (Fase 3)

| Prioriteit | Sentence | Entity |
|-----------|----------|--------|
| 1 | `VHW` | `SpeedThroughWaterMeasurement` + `HeadingMeasurement` |
| 1 | `DBT` / `DPT` | `DepthMeasurement` |
| 1 | `MTW` | `WaterTemperatureMeasurement` |
| 2 | `MWV` | `WindMeasurement` |
| 2 | `HDT` / `HDM` | `HeadingMeasurement` |
| 3 | `RMC` | `PositionMeasurement` + `MotionMeasurement` |
| 3 | `GGA` | `PositionMeasurement` |

### Ontwerpdocument

Zie voor volledig architectuurontwerp (open vragen, entity-mappings, interpreter-overzicht):
[.docs/features/nmea0183-parser-interpreter-architecture.md](features/nmea0183-parser-interpreter-architecture.md)

---

## Future Extensions

### Heading Slice Extensibility

Current fields stored:
- `HeadingDegrees` (primary)

Fields available in payload (not yet stored):
- Deviation (bytes 3-4)
- Variation (bytes 5-6)
- Reference type (byte 7, bit flags)

**To add:** Extend `HeadingMeasurement` entity, update DTO and interpreter.

### New Measurement Types

To add a new PGN:
1. Create entity in Core
2. Add Configuration + DbSet in Infrastructure
3. Create DTOs in Application
4. Implement service + interpreter in Application
5. Add `NetworkMessageType` enum value
6. Update parser PGN mapping
7. Integrate into `NetworkMessageService`
8. Register in DI
9. Create migration

---

*Last updated: 2026-05-17 (NMEA 0183 epic toegevoegd)*
