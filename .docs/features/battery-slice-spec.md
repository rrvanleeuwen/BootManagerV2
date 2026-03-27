# Battery Measurement Slice - Implementation Specification

**Status:** ✅ Fully Implemented  
**Date:** Original implementation (Documented: 2026-03-27)  
**Related PGN:** 127508 (Battery Status)  
**NMEA 2000 Spec:** IEC 61162-1

## Overview

The Battery measurement slice captures and stores battery voltage and state-of-charge (SOC) data from NMEA 2000 PGN 127508 messages.

## Payload Format (PGN 127508)

```
Byte 0:    Battery Instance (0x00 = Main, 0x01 = Secondary, etc.)
Bytes 1-2: Voltage in 0.01V units (uint16, little-endian)
Byte 3:    State of Charge (%) - 0-100 or 0xFF for unknown
Byte 4+:   (Additional fields not decoded in current version)
```

### Conversion Examples

**Voltage:**
- Raw payload bytes 1-2: `0x84 0x04` → uint16 `0x0484` = 1156
- Voltage: 1156 × 0.01 V = **11.56 V**

**State of Charge:**
- Byte 3: `0x50` = 80 (decimal)
- SOC: **80%**
- Unknown: `0xFF` → stored as **null**

## Implementation Components

### 1. Core Layer

**File:** `BootManager.Core/Entities/BatteryMeasurement.cs`

```csharp
public class BatteryMeasurement
{
    public int Id { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }
    public string Source { get; private set; }
    public string MessageId { get; private set; }
    public decimal Voltage { get; private set; }                    // 12.60 V
    public int? StateOfCharge { get; private set; }                 // 80%, nullable
}
```

- **Voltage:** decimal for precision (0.01V resolution)
- **StateOfCharge:** nullable int (0-100%, null if unknown)

### 2. Infrastructure Layer

**Configuration:** `BootManager.Infrastructure/Persistence/Configurations/BatteryMeasurementConfiguration.cs`

```csharp
b.ToTable("BatteryMeasurements");
b.HasKey(x => x.Id);
b.Property(x => x.Voltage).HasPrecision(10, 2);
b.HasIndex(x => x.RecordedAtUtc);
```

### 3. Application Layer

**DTOs:**
- `CreateBatteryMeasurementRequestDto` - Storage request
- `BatteryMessageInterpretationDto` - Parse result

**Services:**
- `IBatteryMeasurementService` - Interface
- `BatteryMeasurementService` - Validation & persistence
- `BatteryMessageInterpreterService` - NMEA 2000 decoding

### 4. Interpreter

**File:** `BootManager.Application/NetworkMessageInterpretation/Services/BatteryMessageInterpreterService.cs`

Decoding logic:
```
Voltage = (bytes[1] | (bytes[2] << 8)) / 100.0
SOC = bytes[3] == 0xFF ? null : bytes[3]
```

Error handling:
- Minimum 4 bytes required
- Invalid NMEA 2000 values return error DTO (non-fatal)

### 5. Parser Integration

PGN 127508 → `NetworkMessageType.Battery`

Recognized in `NetworkMessageParserService.PgnToType`:
```csharp
{ 127508, NetworkMessageType.Battery }
```

### 6. Data Storage

Example database record:
```sql
INSERT INTO BatteryMeasurements 
  (RecordedAtUtc, Source, MessageId, Voltage, StateOfCharge)
VALUES 
  ('2026-03-27T10:15:30Z', '127.0.0.1', '127508', 12.60, 80);
```

## Future Enhancements

### Phase 2: Multiple Batteries

Current: Single battery per message (instance 0x00 hardcoded)

Could extend:
- Store `BatteryInstance` (main, secondary, auxiliary)
- Track multiple batteries per vessel
- Separate tables or instance field in entity

### Phase 3: Extended Fields

Payload bytes 4+ contain (not currently decoded):
- Current (charge/discharge rate in amperes)
- Temperature
- State of Health (%)

### Phase 4: Alerts & Thresholds

- Low voltage warning (<11V for 12V systems)
- SOC warning (<20%)
- Unusual discharge/charge rates

## Files

| Path | Type | Status |
|------|------|--------|
| `BootManager.Core/Entities/BatteryMeasurement.cs` | Entity | ✅ |
| `BootManager.Infrastructure/Persistence/Configurations/BatteryMeasurementConfiguration.cs` | Config | ✅ |
| `BootManager.Application/BatteryMeasurements/DTOs/*` | DTOs | ✅ |
| `BootManager.Application/BatteryMeasurements/Services/*` | Services | ✅ |
| `BootManager.Application/NetworkMessageInterpretation/Services/BatteryMessageInterpreterService.cs` | Interpreter | ✅ |
| `BootManager.Infrastructure/Migrations/*_AddBatteryMeasurement.cs` | Migration | ✅ |

## Testing Notes

- Typical vessel: 12V or 24V systems
- Marine AGM/LiFePO4 batteries: 11-14.5V range
- SOC 0xFF (unknown) common if no BMU integrated
- Fast charge/discharge patterns observed during engine start

---

**Last Updated:** 2026-03-27 (Documented)  
**Implementation Status:** Production-ready
