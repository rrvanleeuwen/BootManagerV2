# Feature Specifications Index

Complete documentation of all implemented vertical slices in BootManager.

## Overview Table

| Slice | PGN | Entity | Status | Spec Link |
|-------|-----|--------|--------|-----------|
| **Battery** | 127508 | `BatteryMeasurement` | ✅ Complete | [battery-slice-spec.md](battery-slice-spec.md) |
| **Depth** | 128267 | `DepthMeasurement` | ✅ Complete | [depth-slice-spec.md](depth-slice-spec.md) |
| **Wind** | 130306 | `WindMeasurement` | ✅ Complete | [wind-slice-spec.md](wind-slice-spec.md) |
| **Motion** (COG/SOG) | 129026 | `MotionMeasurement` | ✅ Complete | [motion-slice-spec.md](motion-slice-spec.md) |
| **Position** | 129025 | `PositionMeasurement` | ✅ Complete | [position-slice-spec.md](position-slice-spec.md) |
| **Heading** | 127250 | `HeadingMeasurement` | ✅ Code-complete | [heading-slice-spec.md](heading-slice-spec.md) |

## Quick Reference: Payload Formats

### Battery (PGN 127508)
```
Byte 0:   Battery Instance
Bytes 1-2: Voltage (0.01V units)
Byte 3:   State of Charge (%)
```
**Key Fields:** Voltage, StateOfCharge

### Depth (PGN 128267)
```
Bytes 0-2: Depth (0.01m units, uint24)
Bytes 3-4: Offset & Range
```
**Key Fields:** DepthMeters

### Wind (PGN 130306)
```
Bytes 0-1: Wind Speed (0.01 m/s)
Bytes 2-3: Wind Angle (1e-4 rad)
Byte 4:   Wind Reference
```
**Key Fields:** WindAngleDegrees, WindSpeed (m/s)

### Motion (PGN 129026)
```
Bytes 0-1: COG (1e-4 rad)
Bytes 2-3: SOG (0.01 knots)
Byte 4+:  Mode, Status
```
**Key Fields:** CourseOverGroundDegrees, SpeedOverGround (knots)

### Position (PGN 129025)
```
Bytes 0-3: Latitude (1e-7 degrees, int32)
Bytes 4-7: Longitude (1e-7 degrees, int32)
```
**Key Fields:** Latitude, Longitude

### Heading (PGN 127250)
```
Byte 0:   SID
Bytes 1-2: Heading (1e-4 rad)
Bytes 3-4: Deviation (1e-4 rad)
Bytes 5-6: Variation (1e-4 rad)
Byte 7:   Reference
```
**Key Fields:** HeadingDegrees

## Common Implementation Pattern

Each slice follows this identical structure:

### 1. **Entity** (BootManager.Core)
- Immutable properties (private setters)
- Constructor for creation
- Minimal business logic

### 2. **Configuration** (BootManager.Infrastructure)
- EF Core `IEntityTypeConfiguration<T>`
- Table mapping, key definition
- Precision & constraints
- Indexes for common queries

### 3. **DTOs** (BootManager.Application)
- Request DTO: `Create{Type}MeasurementRequestDto`
- Interpretation DTO: `{Type}MessageInterpretationDto`
- Public init properties (immutable)

### 4. **Services** (BootManager.Application)
- Interface: `I{Type}MeasurementService`
- Implementation: `{Type}MeasurementService`
  - Dependency injection via constructor
  - Defensible validation
  - Logging (ILogger<T>)
  - Persistence via `IRepository<T>`

### 5. **Interpreter** (BootManager.Application)
- Implements: `INetworkMessageInterpreter<{Type}MessageInterpretationDto>`
- Methods:
  - `CanInterpret()` - Type & payload size check
  - `Interpret()` - Decoding logic

### 6. **Integration**
- **Parser:** PGN → `NetworkMessageType` enum mapping
- **Orchestrator:** `NetworkMessageService.TryInterpretAndSave{Type}MessageAsync()`
- **DI:** Register interpreter (Transient) & service (Scoped)

## Payload Decoding Patterns

### Little-Endian Multi-Byte Values

**uint16 (2 bytes):**
```csharp
ushort value = (ushort)(bytes[0] | (bytes[1] << 8));
```

**uint24 (3 bytes):**
```csharp
uint value = (uint)(bytes[0] | (bytes[1] << 8) | (bytes[2] << 16));
```

**int32 (4 bytes):**
```csharp
int value = (int)(bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24));
```

### NMEA 2000 Unit Conversions

**1e-4 Radians → Degrees:**
```csharp
double radians = scaledValue / 10000.0;
double degrees = radians * (180.0 / Math.PI);
```

**0.01 Units (Centi/Deci):**
```csharp
decimal value = scaledValue / 100.0m;  // Centimeters → meters
decimal value = scaledValue / 100.0m;  // Centiknots → knots
```

**Range Normalization (0-360°):**
```csharp
if (degrees < 0) degrees += 360;
if (degrees >= 360) degrees %= 360;
```

## Error Handling Philosophy

All slices follow non-fatal error handling:

1. **Parse Error** (PGN not recognized):
   - Log warning
   - Raw message still stored
   - Skip interpretation

2. **Interpreter Error** (Payload invalid):
   - Log warning
   - Return `IsSuccess = false` DTO
   - Raw message still stored

3. **Service Validation Error** (Value out of range):
   - Log warning
   - Exception propagated
   - Raw message already stored

4. **Storage Error** (Database failure):
   - Log error
   - Exception propagated
   - Raw message already stored

**Principle:** No error blocks raw message persistence.

## Feature Checklists

### Adding a New Slice (Template)

- [ ] Create entity in Core/Entities/
- [ ] Add Configuration in Infrastructure/Persistence/Configurations/
- [ ] Add DbSet in DbContext
- [ ] Create DTOs in Application/{Type}/DTOs/
- [ ] Create service interface in Application/{Type}/Services/
- [ ] Create service implementation
- [ ] Create interpreter service
- [ ] Add NetworkMessageType enum value
- [ ] Update PgnToType mapping in parser
- [ ] Create TryInterpretAndSave{Type}MessageAsync() in orchestrator
- [ ] Add DI registrations
- [ ] Create and apply migration
- [ ] Write feature spec (`.docs/features/{type}-slice-spec.md`)
- [ ] Update this index

### Extending a Slice

- [ ] Add property to entity (with migration)
- [ ] Update DTO
- [ ] Update interpreter decoding logic
- [ ] Update service validation
- [ ] Add test cases
- [ ] Update feature spec documentation

## Future Roadmap

### High-Priority Additions

- [ ] **True Wind** - Derived from Apparent Wind + Motion
- [ ] **Water Temperature** (PGN 130312)
- [ ] **Barometric Pressure** (PGN 130314)

### Medium-Priority Additions

- [ ] **Extended Heading Fields** - Deviation, Variation, Reference type
- [ ] **NMEA 0183 Support** - Parallel parser for legacy sentences
- [ ] **Magnetic Declination** - Integration with Heading/Position

### Low-Priority Additions

- [ ] **AIS Data** - Vessel identification & tracking
- [ ] **Weather Data** - Sea state, visibility
- [ ] **Fuel/Water** - Tank level monitoring

## References

- **Full Architecture:** [../ARCHITECTURE.md](../ARCHITECTURE.md)
- **Development Guide:** [../DEVELOPMENT.md](../DEVELOPMENT.md)
- **Project Status:** [../TODO.md](../TODO.md)
- **NMEA 2000 Specs:** https://www.nmea.de/
- **GitHub Copilot Instructions:** [../../.github/copilot-instructions.md](../../.github/copilot-instructions.md)

---

**Last Updated:** 2026-03-27  
**Maintenance:** Keep in sync with codebase as new slices are added
