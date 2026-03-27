# Motion Measurement Slice - Implementation Specification

**Status:** ✅ Fully Implemented  
**Date:** Original implementation (Documented: 2026-03-27)  
**Related PGN:** 129026 (COG & SOG, Rapid Update)  
**NMEA 2000 Spec:** IEC 61162-1

## Overview

The Motion measurement slice captures and stores Course Over Ground (COG) and Speed Over Ground (SOG) from NMEA 2000 PGN 129026 messages. These represent the vessel's **actual track and velocity relative to water/earth**, independent of heading or wind.

## Key Distinction: Motion vs. Heading vs. Wind

| Metric | Source | Measures | Reference |
|--------|--------|----------|-----------|
| **Heading** | Compass/Gyro | Direction vessel *points* | Magnetic/True |
| **Motion (COG/SOG)** | GPS/GNSS | Direction vessel *moves* + speed | Geographic (True North) |
| **Wind** | Anemometer | Wind direction & speed | Relative to vessel |

**Example:** Vessel heading 090° but SOG 085° = 5° drift (wind crab or current)

## Payload Format (PGN 129026)

```
Bytes 0-1: Course Over Ground in 1/10000 radians (uint16, little-endian)
Bytes 2-3: Speed Over Ground in 0.01 knots (uint16, little-endian)
Byte 4+:   (Additional fields - mode, mode type - not decoded in current version)
```

### Conversion Examples

**Course Over Ground:**
- Raw bytes 0-1: `0xDA 0x0B` → uint16 `0x0BDA` = 3034
- Radians: 3034 × 1e-4 = 0.3034 rad
- Degrees: 0.3034 × (180/π) ≈ **17.37°**
- Normalized: **17.37°** (from True North, 0-360°)

**Speed Over Ground:**
- Raw bytes 2-3: `0x64 0x00` → uint16 `0x0064` = 100
- Speed: 100 × 0.01 knots = **1.00 knot**
- In m/s: 1.00 × 0.5144 ≈ **0.51 m/s**

## Implementation Components

### 1. Core Layer

**File:** `BootManager.Core/Entities/MotionMeasurement.cs`

```csharp
public class MotionMeasurement
{
    public int Id { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }
    public string Source { get; private set; }
    public string MessageId { get; private set; }
    public decimal CourseOverGroundDegrees { get; private set; }    // 0-360°
    public decimal SpeedOverGround { get; private set; }             // knots
    public string SpeedUnit { get; private set; }                    // "knots"
}
```

- **CourseOverGroundDegrees:** 0° = North, 90° = East, 180° = South, 270° = West
- **SpeedOverGround:** in knots (nautical miles per hour)
- **SpeedUnit:** Always "knots" in current version

### 2. Infrastructure Layer

**Configuration:** `BootManager.Infrastructure/Persistence/Configurations/MotionMeasurementConfiguration.cs`

```csharp
b.ToTable("MotionMeasurements");
b.HasKey(x => x.Id);
b.Property(x => x.CourseOverGroundDegrees).HasPrecision(10, 2);
b.Property(x => x.SpeedOverGround).HasPrecision(10, 2);
b.HasIndex(x => x.RecordedAtUtc);
```

### 3. Application Layer

**DTOs:**
- `CreateMotionMeasurementRequestDto` - Storage request
- `MotionMessageInterpretationDto` - Parse result

**Services:**
- `IMotionMeasurementService` - Interface
- `MotionMeasurementService` - Validation (0-360° course, SOG ≥ 0) & persistence
- `MotionMessageInterpreterService` - NMEA 2000 decoding

### 4. Interpreter

**File:** `BootManager.Application/NetworkMessageInterpretation/Services/MotionMessageInterpreterService.cs`

Decoding logic:
```csharp
// COG: bytes 0-1, uint16 little-endian, 1e-4 rad per unit
ushort cogNMEA2000 = (ushort)(bytes[0] | (bytes[1] << 8));
double cogRadians = cogNMEA2000 / 10000.0;
decimal courseOverGroundDegrees = (decimal)(cogRadians * 180.0 / Math.PI);

// SOG: bytes 2-3, uint16 little-endian, 0.01 knots per unit
ushort sogCentiknots = (ushort)(bytes[2] | (bytes[3] << 8));
decimal speedOverGroundKnots = sogCentiknots / 100.0m;

// Normalize COG to [0, 360)
if (courseOverGroundDegrees < 0) courseOverGroundDegrees += 360;
if (courseOverGroundDegrees >= 360) courseOverGroundDegrees %= 360;
```

Error handling:
- Minimum 4 bytes required
- Invalid COG/SOG ranges caught by service

### 5. Parser Integration

PGN 129026 → `NetworkMessageType.Motion`

Recognized in `NetworkMessageParserService.PgnToType`:
```csharp
{ 129026, NetworkMessageType.Motion }
```

### 6. Data Storage

Example database record:
```sql
INSERT INTO MotionMeasurements 
  (RecordedAtUtc, Source, MessageId, CourseOverGroundDegrees, SpeedOverGround, SpeedUnit)
VALUES 
  ('2026-03-27T10:15:30Z', '127.0.0.1', '129026', 17.37, 1.00, 'knots');
```

## Compass Rose Reference (Geographic/True North)

```
         0° (N)
          N
          |
270° (W)--+--(90° E)
          |
        180° (S)
```

- **North:** 0° or 360°
- **Northeast:** 45°
- **East:** 90°
- **Southeast:** 135°
- **South:** 180°
- **Southwest:** 225°
- **West:** 270°
- **Northwest:** 315°

## Typical Navigation Scenarios

| Scenario | Course | Speed | Vessel Type |
|----------|--------|-------|------------|
| Harbor transit | Variable (meandering) | 0-5 knots | Any |
| Coastal passage | 045-180° | 5-10 knots | Cruising |
| Ocean crossing | Constant (great circle) | 6-8 knots | Sailboat |
| Motor yacht cruise | Variable | 12-20 knots | Motor |
| Racing | Tactical (variable) | 8-15 knots | Racer |

## Drift Detection

**Vessel Drift** = Heading - COG

Example:
- Heading (compass): 090° (pointing east)
- COG (GPS): 085° (moving southeast)
- Drift: 5° starboard (wind pushing vessel off course)

**Applications:**
- Current detection
- Leeway calculation (sailboats)
- Wind crab detection
- Navigation accuracy monitoring

## Future Enhancements

### Phase 2: Mode & Status Fields

Payload byte 4-5 contain:
- Navigation mode (autonomous, differential, RTK, etc.)
- Navigation data status (valid, invalid)
- Reserved fields

Could add:
```csharp
public string? NavigationMode { get; private set; }
public bool? IsNavigationDataValid { get; private set; }
```

### Phase 3: True/Magnetic Heading Integration

Combine with `HeadingMeasurement` to calculate:
- Magnetic variation (True heading - Magnetic heading)
- Drift (Heading - COG)

### Phase 4: Track Recording & Playback

- Store entire route/track in separate table
- Visualize track on map
- Replay voyage with time-compressed playback

## Performance Notes

- COG/SOG updates typically 1-10 Hz (100ms-1s intervals)
- Trigonometric conversions minimal (single division + multiplication)
- GPS accuracy affects COG noise (multipath in harbors)
- High-precision GPS (RTK) provides 1cm track accuracy

## NMEA 2000 Specifications

- **PGN 129026:** Course & Speed Over Ground (Rapid Update)
- **Update Rate:** High frequency (typically 1-10 Hz)
- **Precision:** COG ±0.01°, SOG ±0.01 knots
- **Coverage:** Global (requires GPS/GNSS)

## Files

| Path | Type | Status |
|------|------|--------|
| `BootManager.Core/Entities/MotionMeasurement.cs` | Entity | ✅ |
| `BootManager.Infrastructure/Persistence/Configurations/MotionMeasurementConfiguration.cs` | Config | ✅ |
| `BootManager.Application/MotionMeasurements/DTOs/*` | DTOs | ✅ |
| `BootManager.Application/MotionMeasurements/Services/*` | Services | ✅ |
| `BootManager.Application/NetworkMessageInterpretation/Services/MotionMessageInterpreterService.cs` | Interpreter | ✅ |
| `BootManager.Infrastructure/Migrations/*_AddMotionMeasurement.cs` | Migration | ✅ |

## References

- **NMEA 2000 PGN 129026:** IEC 61162-1
- **Maritime Navigation:** https://en.wikipedia.org/wiki/Course_over_ground
- **Nautical Units:** Knots = 1 NM/hour ≈ 0.5144 m/s

---

**Last Updated:** 2026-03-27 (Documented)  
**Implementation Status:** Production-ready  
**Next Phase:** Drift calculation & mode fields integration
