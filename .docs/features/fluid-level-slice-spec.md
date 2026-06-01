# Fluid Level Slice - Implementation Specification

**Status:** ✅ Complete  
**Date:** 2026-06-01  
**Related PGN:** 127505 (Fluid Level)  
**Gateway input:** NMEA 0183-like `$PCDIN` / `$MXPGN` with PGN `01F211`  
**Branch:** codex/fluid-level-interpreter

## Overview

The Fluid Level slice captures tank-level data from NMEA 2000 PGN 127505 when it is forwarded by the YDEN-style gateway as `PCDIN` or `MXPGN` NMEA 0183 sentences.

The first goal is reliable technical storage for later dashboard, logbook and reporting use. It does not yet add dashboard widgets, logbook integration, source-preference UI, motor hours or historical backfill.

## Payload Format

```
Byte 0:    Tank identity
           high nibble = raw fluid type
           low nibble  = fluid instance
Bytes 1-2: Level in 0.004% units, uint16 little-endian
           0x7FFF = invalid/unknown level
Bytes 3-6: Capacity in 0.1 liter units, uint32 little-endian
Byte 7:    Reserved / gateway padding
```

### Fluid Types

The interpreter stores both the raw numeric fluid type and the normalized application fluid type.

Known mappings in this slice:

- `0` => fuel
- `1` => fresh water
- `2` => gray water
- `5` => oil

Unknown or future raw types must not crash parsing. They are stored with the raw type so later source-specific behavior can be added.

### Conversion Examples

`00A861DC050000FF`

- Raw fluid type: `0` fuel
- Fluid instance: `0`
- Level raw: `0x61A8` = 25000
- Level percent: 25000 × 0.004 = 100%
- Capacity raw: `0x000005DC` = 1500
- Capacity liters: 1500 × 0.1 = 150L

`014A61DC050000FF`

- Raw fluid type: `0` fuel
- Fluid instance: `1`
- Level raw: `0x614A` = 24906
- Level percent: 24906 × 0.004 = 99.624%
- Capacity liters: 150L

`104A11D0070000FF`

- Raw fluid type: `1` fresh water
- Fluid instance: `0`
- Level raw: `0x114A` = 4426
- Level percent: 4426 × 0.004 = 17.704%
- Capacity raw: `0x000007D0` = 2000
- Capacity liters: 200L

`50FF7FE8030000FF`

- Raw fluid type: `5` oil
- Fluid instance: `0`
- Level raw: `0x7FFF`
- Level is invalid/unknown and must not be stored as a percentage above 100%
- Capacity liters: 100L

## Implementation Components

### Core Layer

**File:** `BootManager.Core/Entities/FluidLevelMeasurement.cs`

Fields include `RecordedAtUtc`, `Source`, `MessageId`, `GatewaySentence`, `Pgn`, `FluidInstance`, `RawFluidType`, `FluidType`, `LevelPercent`, `CapacityLiters`, `IsLevelInvalid`, and `RawPayloadHex`.

### Application Layer

**Interpreter:** `BootManager.Application/NetworkMessageInterpretation/Services/FluidLevelMessageInterpreterService.cs`

- Handles PGN `127505` payloads from `PCDIN` and `MXPGN`.
- Decodes tank identity, level, invalid-level marker and capacity.
- Keeps raw type and instance for multiple tanks per type.

**Measurement service:** `BootManager.Application/FluidLevelMeasurements/Services/FluidLevelMeasurementService.cs`

- Stores decoded fluid-level measurements.
- Applies pragmatic duplicate handling for parallel `PCDIN` and `MXPGN`: `MXPGN` is skipped when a matching `PCDIN` measurement already exists in the same minute for the same tank.

Duplicate handling is intentionally not the final source-preference model. Later source-registry/source-preference stories should normalize this more robustly.

### Infrastructure Layer

**EF Configuration:** `BootManager.Infrastructure/Persistence/Configurations/FluidLevelMeasurementConfiguration.cs`

**DbContext:** `FluidLevelMeasurements` DbSet added to `BootManagerDbContext`

**Migration:** `AddFluidLevelMeasurement`

### Simulator

The `YDEN03` simulator profile emits normal navigation, wind, depth, water-temperature and AIS sentences, plus representative PGN 127505 `PCDIN`/`MXPGN` fluid-level gateway sentences for:

- two fuel tanks;
- two fresh-water tanks;
- gray water;
- invalid/unknown oil level.

## Verification

- `dotnet build BootManager.sln`
- `dotnet test BootManager.UnitTests\BootManager.UnitTests.csproj --filter FluidLevel`
- `dotnet test src\BootManager.Tools.Simulator.Tests\BootManager.Tools.Simulator.Tests.csproj`
- Manual local Web + Ingest + YDEN03 simulator validation confirmed raw `PCDIN`/`MXPGN` storage and populated `FluidLevelMeasurements`.
