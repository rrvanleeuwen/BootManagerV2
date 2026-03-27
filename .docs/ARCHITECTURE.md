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

*Last updated: 2026-03-27 (Heading slice implementation)*
