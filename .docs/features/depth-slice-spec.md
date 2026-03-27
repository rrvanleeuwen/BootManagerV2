# Depth Measurement Slice - Implementation Specification

**Status:** ✅ Fully Implemented  
**Date:** Original implementation (Documented: 2026-03-27)  
**Related PGN:** 128267 (Water Depth, Rapid Update)  
**NMEA 2000 Spec:** IEC 61162-1

## Overview

The Depth measurement slice captures and stores water depth data from NMEA 2000 PGN 128267 messages.

## Payload Format (PGN 128267)

```
Bytes 0-2: Depth in 0.01m units (uint24, little-endian)
Bytes 3-4: (Additional fields - offset, range - not decoded in current version)
```

### Conversion Examples

**Depth:**
- Raw payload bytes 0-2: `0x64 0x01 0x00` → uint24 `0x000164` = 356
- Depth: 356 × 0.01 m = **3.56 meters**

**Range:**
- Shallow (0.01m): Raw = 1
- Deep (100m): Raw = 10,000
- Max uint24: ~16,777 meters (unrealistic)

## Implementation Components

### 1. Core Layer

**File:** `BootManager.Core/Entities/DepthMeasurement.cs`

```csharp
public class DepthMeasurement
{
    public int Id { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }
    public string Source { get; private set; }
    public string MessageId { get; private set; }
    public decimal DepthMeters { get; private set; }        // 3.50 m
}
```

- **DepthMeters:** decimal for precision (0.01m resolution)
- **Range:** Validated 0-1000m typical (extensible)

### 2. Infrastructure Layer

**Configuration:** `BootManager.Infrastructure/Persistence/Configurations/DepthMeasurementConfiguration.cs`

```csharp
b.ToTable("DepthMeasurements");
b.HasKey(x => x.Id);
b.Property(x => x.DepthMeters).HasPrecision(10, 2);
b.HasIndex(x => x.RecordedAtUtc);
```

### 3. Application Layer

**DTOs:**
- `CreateDepthMeasurementRequestDto` - Storage request
- `DepthMessageInterpretationDto` - Parse result

**Services:**
- `IDepthMeasurementService` - Interface
- `DepthMeasurementService` - Validation & persistence
- `DepthMessageInterpreterService` - NMEA 2000 decoding

### 4. Interpreter

**File:** `BootManager.Application/NetworkMessageInterpretation/Services/DepthMessageInterpreterService.cs`

Decoding logic (uint24, little-endian):
```csharp
uint depthCentimeters = (uint)(
    bytes[0] 
    | (bytes[1] << 8) 
    | (bytes[2] << 16)
);
decimal depthMeters = depthCentimeters / 100.0m;
```

Error handling:
- Minimum 3 bytes required (24-bit value)
- Invalid ranges caught by service validation

### 5. Parser Integration

PGN 128267 → `NetworkMessageType.Depth`

Recognized in `NetworkMessageParserService.PgnToType`:
```csharp
{ 128267, NetworkMessageType.Depth }
```

### 6. Data Storage

Example database record:
```sql
INSERT INTO DepthMeasurements 
  (RecordedAtUtc, Source, MessageId, DepthMeters)
VALUES 
  ('2026-03-27T10:15:30Z', '127.0.0.1', '128267', 3.50);
```

## NMEA 2000 Notes

### uint24 Encoding

Standard NMEA 2000 uses little-endian uint24 for 3-byte values:
- **Byte 0:** Bits 0-7 (LSB)
- **Byte 1:** Bits 8-15
- **Byte 2:** Bits 16-23 (MSB)

Example: Value 1000 (= 0x0003E8)
- Encoded: `0xE8 0x03 0x00`
- Decoded: 0xE8 | (0x03 << 8) | (0x00 << 16) = 1000 ✓

### Transducer Offset

Real PGN 128267 includes transducer offset (bytes 3-4):
- Allows compensation for transducer installation depth
- Not currently stored (future enhancement)

## Future Enhancements

### Phase 2: Transducer Offset

Add fields:
- `TransducerOffsetMeters` (e.g., 0.5m for surface mounting)
- `DepthBelowTransducer` vs. `DepthBelowKeelline`

```csharp
public decimal? TransducerOffsetMeters { get; private set; }
```

### Phase 3: Range/Mode Fields

PGN bytes 3-4 also contain:
- Maximum range setting
- Transducer measurement type (sounder, echo sounder, etc.)

### Phase 4: Alerts & Trending

- Shallow water alarm (<2m)
- Bottom tracking (close to seabed)
- Depth trend analysis (rising/falling)
- Ground/navigation warnings

## Typical Scenarios

| Scenario | Depth Range | Update Rate |
|----------|-------------|-------------|
| Harbor/mooring | 0.5 - 10 m | 10 Hz |
| Coastal | 10 - 100 m | 10 Hz |
| Offshore | 100+ m | Variable |
| Shallow water alarm | < 2 m | 10 Hz (high priority) |

## Files

| Path | Type | Status |
|------|------|--------|
| `BootManager.Core/Entities/DepthMeasurement.cs` | Entity | ✅ |
| `BootManager.Infrastructure/Persistence/Configurations/DepthMeasurementConfiguration.cs` | Config | ✅ |
| `BootManager.Application/DepthMeasurements/DTOs/*` | DTOs | ✅ |
| `BootManager.Application/DepthMeasurements/Services/*` | Services | ✅ |
| `BootManager.Application/NetworkMessageInterpretation/Services/DepthMessageInterpreterService.cs` | Interpreter | ✅ |
| `BootManager.Infrastructure/Migrations/*_AddDepthMeasurement.cs` | Migration | ✅ |

## Performance Notes

- Depth updates typically 10 Hz (100ms intervals)
- uint24 parsing is fast (3-byte read + bit shift)
- Minimal validation (range check only)
- Storage I/O is bottleneck (not decode)

---

**Last Updated:** 2026-03-27 (Documented)  
**Implementation Status:** Production-ready
