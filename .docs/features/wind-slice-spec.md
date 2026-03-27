# Wind Measurement Slice - Implementation Specification

**Status:** ✅ Fully Implemented  
**Date:** Original implementation (Documented: 2026-03-27)  
**Related PGN:** 130306 (Wind Data)  
**NMEA 2000 Spec:** IEC 61162-1

## Overview

The Wind measurement slice captures and stores wind angle and speed data from NMEA 2000 PGN 130306 messages. Wind is measured **relative to the vessel** (apparent wind), not true wind relative to earth.

## Wind Reference Types

**Apparent Wind** (Current implementation):
- Wind as experienced by moving vessel
- Combines true wind + vessel motion
- Primary for sail trimming

**True Wind** (Future):
- Wind relative to earth/water
- Requires integration with Motion (COG/SOG)
- Better for weather analysis

## Payload Format (PGN 130306)

```
Bytes 0-1: Wind Speed in 0.01 m/s units (uint16, little-endian)
Bytes 2-3: Wind Angle in 1/10000 radians (uint16, little-endian)
Byte 4:    Wind Reference (bits 0-2: 0=True, 1=Magnetic, 2=Apparent, 3=North)
Byte 5+:   (Additional fields - not decoded in current version)
```

### Conversion Examples

**Wind Speed:**
- Raw bytes 0-1: `0x34 0x04` → uint16 `0x0434` = 1076
- Speed: 1076 × 0.01 m/s = **10.76 m/s**
- In knots: 10.76 × 1.944 ≈ **20.9 knots**

**Wind Angle:**
- Raw bytes 2-3: `0x10 0x0B` → uint16 `0x0B10` = 2832
- Radians: 2832 × 1e-4 = 0.2832 rad
- Degrees: 0.2832 × (180/π) ≈ **16.23°**
- Normalized: **16.23°** (from bow, 0-360°)

## Implementation Components

### 1. Core Layer

**File:** `BootManager.Core/Entities/WindMeasurement.cs`

```csharp
public class WindMeasurement
{
    public int Id { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }
    public string Source { get; private set; }
    public string MessageId { get; private set; }
    public decimal WindAngleDegrees { get; private set; }   // 0-360°
    public decimal WindSpeed { get; private set; }          // m/s
    public string SpeedUnit { get; private set; }           // "m/s"
}
```

- **WindAngleDegrees:** 0° = bow, 90° = starboard beam, 180° = stern, 270° = port beam
- **WindSpeed:** in meters per second (m/s)
- **SpeedUnit:** Always "m/s" in current version

### 2. Infrastructure Layer

**Configuration:** `BootManager.Infrastructure/Persistence/Configurations/WindMeasurementConfiguration.cs`

```csharp
b.ToTable("WindMeasurements");
b.HasKey(x => x.Id);
b.Property(x => x.WindAngleDegrees).HasPrecision(10, 2);
b.Property(x => x.WindSpeed).HasPrecision(10, 2);
b.HasIndex(x => x.RecordedAtUtc);
```

### 3. Application Layer

**DTOs:**
- `CreateWindMeasurementRequestDto` - Storage request with angle, speed, unit
- `WindMessageInterpretationDto` - Parse result with both fields

**Services:**
- `IWindMeasurementService` - Interface
- `WindMeasurementService` - Validation (0-360°, speed ≥ 0) & persistence
- `WindMessageInterpreterService` - NMEA 2000 decoding

### 4. Interpreter

**File:** `BootManager.Application/NetworkMessageInterpretation/Services/WindMessageInterpreterService.cs`

Decoding logic:
```csharp
// Wind speed: bytes 0-1, uint16 little-endian, 0.01 m/s per unit
ushort windSpeedCentiMps = (ushort)(bytes[0] | (bytes[1] << 8));
decimal windSpeedMps = windSpeedCentiMps / 100.0m;

// Wind angle: bytes 2-3, uint16 little-endian, 1e-4 rad per unit
ushort windAngleRadiansScaled = (ushort)(bytes[2] | (bytes[3] << 8));
double windAngleRadians = windAngleRadiansScaled / 10000.0;
double windAngleDegrees = windAngleRadians * (180.0 / Math.PI);

// Normalize to [0, 360)
if (windAngleDegrees < 0) windAngleDegrees += 360;
if (windAngleDegrees >= 360) windAngleDegrees %= 360;
```

Error handling:
- Minimum 4 bytes required
- Invalid angle/speed ranges caught by service

### 5. Parser Integration

PGN 130306 → `NetworkMessageType.Wind`

Recognized in `NetworkMessageParserService.PgnToType`:
```csharp
{ 130306, NetworkMessageType.Wind }
```

### 6. Data Storage

Example database record:
```sql
INSERT INTO WindMeasurements 
  (RecordedAtUtc, Source, MessageId, WindAngleDegrees, WindSpeed, SpeedUnit)
VALUES 
  ('2026-03-27T10:15:30Z', '127.0.0.1', '130306', 45.50, 8.25, 'm/s');
```

## Compass Rose Reference

```
         0° (BOW)
           N
           |
270° (PORT)-+--(90° STARBOARD) E
           |
        180° (STERN)
```

- **Close-hauled:** 45° ± 15° (upwind, close to centerline)
- **Beam reach:** 90° ± 10° (perpendicular to centerline)
- **Broad reach:** 135° ± 15° (favorable downwind angle)
- **Running:** 180° ± 10° (dead downwind)

## True Wind Calculation (Future)

To derive true wind from apparent + motion:
```
True Wind Speed = sqrt(AWS² + SOG² - 2·AWS·SOG·cos(AWA - COG))
True Wind Angle = atan2(...)  # Trigonometric formula
```

Requires:
- Apparent Wind Speed & Angle (current)
- Course Over Ground & Speed Over Ground (PGN 129026 Motion)
- Vessel heading offset

## Future Enhancements

### Phase 2: Wind Reference Type

Currently: Assumed apparent wind

Could add:
- `WindReferenceType` enum (True, Magnetic, Apparent, North)
- Store reference from byte 4 of payload
- UI indication of reference type

### Phase 3: True Wind Calculation

Add separate fields:
- `TrueWindSpeedMps` (calculated)
- `TrueWindAngleDegrees` (calculated)
- Requires Motion slice integration

### Phase 4: Wind Statistics

Aggregate fields:
- Average wind speed (last 10 min)
- Wind gust detection (peak vs. average)
- Wind direction stability

## Typical Sailing Scenarios

| Scenario | Wind Angle | Wind Speed | Sail Configuration |
|----------|------------|------------|-------------------|
| Light air | Variable | 0-2 m/s | All sails, light wind trim |
| Moderate | 45° (upwind) | 4-6 m/s | Normal configuration |
| Strong | 90° (beam) | 8-12 m/s | Reduced sail, reefed |
| Gale | 180° (running) | 12+ m/s | Storm jib only |

## Files

| Path | Type | Status |
|------|------|--------|
| `BootManager.Core/Entities/WindMeasurement.cs` | Entity | ✅ |
| `BootManager.Infrastructure/Persistence/Configurations/WindMeasurementConfiguration.cs` | Config | ✅ |
| `BootManager.Application/WindMeasurements/DTOs/*` | DTOs | ✅ |
| `BootManager.Application/WindMeasurements/Services/*` | Services | ✅ |
| `BootManager.Application/NetworkMessageInterpretation/Services/WindMessageInterpreterService.cs` | Interpreter | ✅ |
| `BootManager.Infrastructure/Migrations/*_AddWindMeasurements.cs` | Migration | ✅ |

## Performance Notes

- Wind updates typically 1-2 Hz (500ms-1s intervals)
- Trigonometric conversions (sin/cos/atan) cached where possible
- Storage I/O is primary bottleneck
- Historical wind trending useful for forecasting sail changes

## References

- **NMEA 2000 PGN 130306:** IEC 61162-1
- **Wind Reference Standards:** Apparent vs. True wind physics
- **Sailing Theory:** https://en.wikipedia.org/wiki/Points_of_sail

---

**Last Updated:** 2026-03-27 (Documented)  
**Implementation Status:** Production-ready (apparent wind only)  
**Next Phase:** True wind calculation (requires Motion integration)
