# Speed Through Water Slice - Implementation Specification

**Status:** ✅ Complete  
**Date:** 2026-05-17
**Related PGN:** 128259 (Speed Through Water / Speed, Water Referenced)  
**Branch:** feature/speed-through-water-slice

## Overview

The Speed Through Water slice captures and stores vessel speed relative to the water surface from NMEA 2000 PGN 128259 messages. This is distinct from Speed Over Ground (SOG, part of Motion/PGN 129026), which represents actual speed relative to the ground.

## Key Difference: Speed Through Water vs. Speed Over Ground

| Aspect | Speed Through Water (PGN 128259) | Speed Over Ground (PGN 129026) |
|--------|----------------------------------|-------------------------------|
| **Source** | Paddle wheel / Pitot / Doppler | GPS/GNSS |
| **Measures** | Speed relative to water | Speed relative to ground |
| **Use** | Leeway, current detection | Navigation, ETA |
| **Reference** | Water | Geographic |

## Payload Format (PGN 128259)

```
Byte 0:    SID (Sequence ID, 0-255)
Bytes 1-2: Speed in 0.01 m/s (uint16, little-endian)
Byte 3:    Speed Water Reference Type
		   0 = Paddle wheel
		   1 = Pitot tube
		   2 = Doppler
		   3 = Correlation (ultra sound)
		   4 = Electromagnetic
```

### Conversion Example

**Payload:** `01 16 01 00`

- SID = `0x01` = 1
- Raw speed = `0x0116` = 278 → 278 × 0.01 = **2.78 m/s**
- Speed in knots = 2.78 × 1.94384 ≈ **5.40 kn**
- Reference type = `0x00` = **Paddle wheel**

## Implementation Components

### 1. Core Layer

**File:** `BootManager.Core/Entities/SpeedThroughWaterMeasurement.cs`

Fields: `Id`, `RecordedAtUtc`, `Source`, `MessageId`, `SpeedMetersPerSecond`, `SpeedKnots`, `SpeedWaterReferenceType`

### 2. Application Layer

**Interpretation DTO:** `BootManager.Application/NetworkMessageInterpretation/DTOs/SpeedThroughWaterMessageInterpretationDto.cs`  
Fields: `IsSuccess`, `Sid`, `SpeedMetersPerSecond`, `SpeedKnots`, `SpeedWaterReferenceType`, `ErrorMessage`

**Interpreter:** `BootManager.Application/NetworkMessageInterpretation/Services/SpeedThroughWaterMessageInterpreterService.cs`  
- `CanInterpret`: checks `MessageType == SpeedThroughWater && PayloadBytes.Length >= 4`
- `Interpret`: decodes SID, speed (uint16 LE × 0.01), knots (× 1.94384), reference type

**Request DTO:** `BootManager.Application/SpeedThroughWaterMeasurements/DTOs/CreateSpeedThroughWaterMeasurementRequestDto.cs`

**Service interface:** `BootManager.Application/SpeedThroughWaterMeasurements/Services/ISpeedThroughWaterMeasurementService.cs`

**Service implementation:** `BootManager.Application/SpeedThroughWaterMeasurements/Services/SpeedThroughWaterMeasurementService.cs`  
- Defensive validation: Source and MessageId not empty, speeds not negative
- Uses generic `IRepository<SpeedThroughWaterMeasurement>`

### 3. Infrastructure Layer

**EF Configuration:** `BootManager.Infrastructure/Persistence/Configurations/SpeedThroughWaterMeasurementConfiguration.cs`  
- Table: `SpeedThroughWaterMeasurements`
- `SpeedMetersPerSecond`: precision(10,4)
- `SpeedKnots`: precision(10,4)
- Index on `RecordedAtUtc`

**DbContext:** `SpeedThroughWaterMeasurements` DbSet added to `BootManagerDbContext`

**Migration:** `AddSpeedThroughWaterMeasurement`

### 4. Integration

**Parser:** PGN `128259` mapped to `NetworkMessageType.SpeedThroughWater` in `NetworkMessageParserService`

**NetworkMessageService:** `TryInterpretAndSaveSpeedThroughWaterMessageAsync` added — follows the same non-fatal pattern as all other slices (interpretation/storage failures do not block raw message storage)

**DI registration:** Interpreter and service registered in `BootManager.Application/DependencyInjection.cs`

## Notes

- Raw message storage remains non-fatal with respect to interpretation errors
- No changes were made to the simulator
- Existing slices are unaffected
