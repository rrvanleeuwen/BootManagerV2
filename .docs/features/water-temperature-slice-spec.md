# Water Temperature Slice - Implementation Specification

**Status:** ✅ Complete  
**Date:** 2026-05-17  
**Related PGN:** 130312 (Temperature / Temperature, Water)  
**Branch:** feature/water-temperature-slice

## Overview

The Water Temperature slice captures and stores water temperature readings from NMEA 2000 PGN 130312 messages. The simulator broadcasts this PGN as hex message ID `01FD08`.

## Payload Format (PGN 130312)

```
Byte 0:    SID (Sequence ID, 0-255)
Byte 1:    Temperature Instance
		   0 = Sea/Water Temperature
		   1 = Outside Temperature
		   2 = Inside Temperature
		   3 = Engine Room Temperature
		   4 = Main Cabin Temperature
		   5 = Live Well Temperature
		   6 = Bait Well Temperature
		   7 = Refrigeration Temperature
		   8 = Heating System Temperature
Bytes 2-3: Temperature in 0.01 Kelvin (uint16, little-endian)
```

### Conversion Example

**Payload:** `01 00 88 71`

- SID = `0x01` = 1
- Temperature Instance = `0x00` = Sea/Water Temperature
- Raw temperature = `0x7188` = 29064 → 29064 × 0.01 = **290.64 K**
- Temperature in Celsius = 290.64 − 273.15 = **17.49 °C**

## Implementation Components

### 1. Core Layer

**File:** `BootManager.Core/Entities/WaterTemperatureMeasurement.cs`

Fields: `Id`, `RecordedAtUtc`, `Source`, `MessageId`, `TemperatureInstance`, `TemperatureKelvin`, `TemperatureCelsius`

### 2. Application Layer

**Interpretation DTO:** `BootManager.Application/NetworkMessageInterpretation/DTOs/WaterTemperatureMessageInterpretationDto.cs`  
Fields: `IsSuccess`, `Sid`, `TemperatureInstance`, `TemperatureKelvin`, `TemperatureCelsius`, `ErrorMessage`

**Interpreter:** `BootManager.Application/NetworkMessageInterpretation/Services/WaterTemperatureMessageInterpreterService.cs`  
- `CanInterpret`: checks `MessageType == WaterTemperature && PayloadBytes.Length >= 4`
- `Interpret`: decodes SID (byte 0), temperature instance (byte 1), temperature (bytes 2-3, uint16 LE × 0.01 K), Celsius (K − 273.15)

**Request DTO:** `BootManager.Application/WaterTemperatureMeasurements/DTOs/CreateWaterTemperatureMeasurementRequestDto.cs`

**Service interface:** `BootManager.Application/WaterTemperatureMeasurements/Services/IWaterTemperatureMeasurementService.cs`

**Service implementation:** `BootManager.Application/WaterTemperatureMeasurements/Services/WaterTemperatureMeasurementService.cs`  
- Defensive validation: Source and MessageId not empty, TemperatureKelvin not negative
- Uses generic `IRepository<WaterTemperatureMeasurement>`

### 3. Infrastructure Layer

**EF Configuration:** `BootManager.Infrastructure/Persistence/Configurations/WaterTemperatureMeasurementConfiguration.cs`  
- Table: `WaterTemperatureMeasurements`
- `TemperatureKelvin`: precision(10,4)
- `TemperatureCelsius`: precision(10,4)
- Index on `RecordedAtUtc`

**DbContext:** `WaterTemperatureMeasurements` DbSet added to `BootManagerDbContext`

**Migration:** `AddWaterTemperatureMeasurement`

### 4. Integration

**Parser:** PGN `130312` added to `NetworkMessageParserService.PgnToType` mapping → `NetworkMessageType.WaterTemperature`

**NetworkMessageType enum:** `WaterTemperature = 8` added to `NetworkMessageParsing.Enums.NetworkMessageType`

**NetworkMessageService:** `TryInterpretAndSaveWaterTemperatureMessageAsync` added and dispatched alongside other interpreter calls. Raw message opslag blijft onafhankelijk van interpretatie- of opslagfouten.

**DependencyInjection:** Interpreter (`INetworkMessageInterpreter<WaterTemperatureMessageInterpretationDto>`) en service (`IWaterTemperatureMeasurementService`) geregistreerd via `AddApplicationServices`.

## Notes

- Temperatuur wordt opgeslagen in zowel Kelvin als Celsius voor directe bruikbaarheid.
- De parser en interpreter zijn strikt gescheiden: de parser herkent PGN en extraheert bytes; de interpreter decodeert semantische waarden.
- Raw message opslag slaagt altijd, ook als interpretatie of afgeleide opslag faalt.
