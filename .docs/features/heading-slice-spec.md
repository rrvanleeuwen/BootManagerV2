# Heading Measurement Slice - Implementation Specification

**Status:** ✅ Implemented (Pending: EF Migration)  
**Date:** 2026-03-27  
**Related PGN:** 127250 (Vessel Heading)  
**NMEA 2000 Spec:** IEC 61162-1

## Overview

The Heading measurement slice captures and stores vessel heading (compass direction) data from NMEA 2000 PGN 127250 messages. This is distinct from Motion (COG/SOG) which represents actual course over ground.

## Key Difference: Heading vs. Motion

| Aspect | Heading (PGN 127250) | Motion (PGN 129026) |
|--------|----------------------|---------------------|
| **Source** | Compass/Gyro | GPS/GNSS |
| **Measures** | Direction vessel *points* | Direction vessel *moves* |
| **Use** | Steering, navigation display | Actual track, autopilot feedback |
| **Reference** | True/Magnetic | Geographic |
| **Payload** | 8 bytes (SID + Heading + Dev + Var + Ref) | 4 bytes (COG + SOG) |

**Example:** Vessel heading 090° (east) but SOG 085° (southeast) = 5° windward crab.

## Payload Format (PGN 127250)

```
Byte 0:    SID (Sequence ID, 0-255)
Bytes 1-2: Heading in 1/10000 radians (uint16, little-endian)
Bytes 3-4: Deviation in 1/10000 radians (uint16, little-endian)
Bytes 5-6: Variation in 1/10000 radians (uint16, little-endian)
Byte 7:    Reference (bits 0-1: 00=True, 01=Magnetic; bits 2-7: reserved)
```

### Conversion Examples

**Heading:**
- Raw payload bytes 1-2: `0x1F 0x0F` → uint16 `0x0F1F` = 3871
- NMEA 2000 units: 3871 × 1e-4 rad = 0.3871 radians
- Degrees: 0.3871 × (180/π) ≈ **22.18°**

**Range Normalization:**
- All heading values normalized to [0, 360)
- Negative degrees wrapped: -10° → 350°
- Overflow wrapped: 370° → 10°

## Implementation Components

### 1. Core Layer

**File:** `BootManager.Core/Entities/HeadingMeasurement.cs`

```csharp
public class HeadingMeasurement
{
    public int Id { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }
    public string Source { get; private set; }
    public string MessageId { get; private set; }
    public decimal HeadingDegrees { get; private set; }
}
```

- **Immutable:** Constructor sets values, no public setters
- **Precision:** `decimal` for astronomical accuracy (0.01° resolution)
- **Source tracking:** Identifies sensor/network origin
- **MessageId:** Links to raw `NetworkMessage` for audit trail

### 2. Infrastructure Layer

**Files:**
- `BootManager.Infrastructure/Persistence/Configurations/HeadingMeasurementConfiguration.cs`
- Updated: `BootManager.Infrastructure/Persistence/BootManagerDbContext.cs`

**Configuration:**
```csharp
b.ToTable("HeadingMeasurements");
b.HasKey(x => x.Id);
b.Property(x => x.HeadingDegrees).HasPrecision(10, 2); // 8 integer digits + 2 decimal
b.HasIndex(x => x.RecordedAtUtc).IsUnique(false);      // Query optimization
```

**DbSet added:**
```csharp
public DbSet<HeadingMeasurement> HeadingMeasurements => Set<HeadingMeasurement>();
```

### 3. Application Layer

#### DTOs

**File:** `BootManager.Application/HeadingMeasurements/DTOs/CreateHeadingMeasurementRequestDto.cs`

```csharp
public class CreateHeadingMeasurementRequestDto
{
    public DateTime RecordedAtUtc { get; init; }
    public string Source { get; init; }
    public string MessageId { get; init; }
    public decimal HeadingDegrees { get; init; }
}
```

**File:** `BootManager.Application/NetworkMessageInterpretation/DTOs/HeadingMessageInterpretationDto.cs`

```csharp
public class HeadingMessageInterpretationDto
{
    public bool IsSuccess { get; set; }
    public decimal? HeadingDegrees { get; set; }
    public string Unit { get; set; } = "°";
    public string? ErrorMessage { get; set; }
}
```

#### Services

**File:** `BootManager.Application/HeadingMeasurements/Services/IHeadingMeasurementService.cs`

```csharp
public interface IHeadingMeasurementService
{
    Task<int> SaveAsync(CreateHeadingMeasurementRequestDto request, 
                       CancellationToken cancellationToken = default);
}
```

**File:** `BootManager.Application/HeadingMeasurements/Services/HeadingMeasurementService.cs`

- Validates: Source/MessageId not empty, HeadingDegrees in [0, 360]
- Uses generic `IRepository<HeadingMeasurement>`
- Logs info on success, warnings on validation errors
- Exceptions propagate to caller (controller/orchestrator)

#### Interpreter

**File:** `BootManager.Application/NetworkMessageInterpretation/Services/HeadingMessageInterpreterService.cs`

```csharp
public class HeadingMessageInterpreterService : INetworkMessageInterpreter<HeadingMessageInterpretationDto>
{
    public bool CanInterpret(NetworkMessageParseResultDto parseResult)
    {
        return parseResult.IsSuccess 
            && parseResult.MessageType == NetworkMessageType.Heading
            && parseResult.PayloadBytes.Length >= 3; // SID + Heading minimum
    }

    public HeadingMessageInterpretationDto Interpret(NetworkMessageParseResultDto parseResult)
    {
        // Decode bytes 1-2 as uint16 (little-endian)
        ushort headingNMEA2000 = (ushort)(parseResult.PayloadBytes[1] 
                                        | (parseResult.PayloadBytes[2] << 8));
        
        // Convert: 1e-4 rad units → radians → degrees
        double headingRadians = headingNMEA2000 / 10000.0;
        decimal headingDegrees = (decimal)(headingRadians * 180.0 / Math.PI);
        
        // Normalize to [0, 360)
        if (headingDegrees < 0) headingDegrees += 360;
        if (headingDegrees >= 360) headingDegrees %= 360;
        
        return new HeadingMessageInterpretationDto
        {
            IsSuccess = true,
            HeadingDegrees = headingDegrees,
            Unit = "°"
        };
    }
}
```

### 4. Parser Integration

**File:** `BootManager.Application/NetworkMessageParsing/Enums/NetworkMessageType.cs`

Added enum value:
```csharp
/// <summary>
/// Koersgegevens (PGN 127250 - Vessel Heading).
/// </summary>
Heading = 6
```

**File:** `BootManager.Application/NetworkMessageParsing/Services/NetworkMessageParserService.cs`

Updated PGN mapping:
```csharp
private static readonly Dictionary<uint, NetworkMessageType> PgnToType = new()
{
    { 129025, NetworkMessageType.Position },
    { 129026, NetworkMessageType.Motion },
    { 127250, NetworkMessageType.Heading },  // ← NEW
    { 130306, NetworkMessageType.Wind },
    { 128267, NetworkMessageType.Depth },
    { 127508, NetworkMessageType.Battery }
};
```

### 5. Orchestration

**File:** `BootManager.Application/NetworkMessages/Services/NetworkMessageService.cs`

**Dependencies injected:**
```csharp
private readonly INetworkMessageInterpreter<HeadingMessageInterpretationDto> _headingInterpreter;
private readonly IHeadingMeasurementService _headingMeasurementService;
```

**Processing step in CreateAsync:**
```csharp
await TryInterpretAndSaveHeadingMessageAsync(parseResult, request, ct);
```

**Implementation follows identical error-handling pattern as other slices:**
- Parse errors logged, don't block raw storage
- Interpretation errors logged, don't block raw storage
- Storage errors logged, don't block raw storage
- All failures are non-fatal

### 6. Dependency Injection

**File:** `BootManager.Application/DependencyInjection.cs`

```csharp
// Interpreter (stateless, multiple calls OK)
services.AddTransient<INetworkMessageInterpreter<HeadingMessageInterpretationDto>, 
                      HeadingMessageInterpreterService>();

// Service (stateful, scoped to HTTP request/operation)
services.AddScoped<IHeadingMeasurementService, HeadingMeasurementService>();
```

## Data Flow Example

```
Raw Message (from Simulator):
  MessageId: "1F23"  (hex PGN 0x1F23 = 127250 decimal)
  PayloadHex: "00 1F 0F 00 00 00 00 00"
  
↓ Parser.Parse()
  MessageType = Heading (matched via PGN)
  PayloadBytes = [0x00, 0x1F, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00]
  
↓ HeadingMessageInterpreterService.CanInterpret()
  ✓ IsSuccess=true, MessageType=Heading, Length>=3
  
↓ HeadingMessageInterpreterService.Interpret()
  bytes[1:2] = 0x1F0F = 3871
  radians = 3871 / 10000 = 0.3871
  degrees = 0.3871 × 180/π = 22.18°
  
↓ HeadingMeasurementService.SaveAsync()
  Validates: HeadingDegrees ∈ [0, 360] ✓
  Creates entity
  Saves to DB via IRepository<HeadingMeasurement>
  
↓ Database
  INSERT INTO HeadingMeasurements 
    (RecordedAtUtc, Source, MessageId, HeadingDegrees)
  VALUES ('2026-03-27T10:15:30Z', '127.0.0.1', '1F23', 22.18)
```

## Testing Strategy

### Unit Tests (Planned)

- **Parser:** PGN 127250 → `NetworkMessageType.Heading`
- **Interpreter:** Various NMEA 2000 heading values → correct degree conversion
- **Service:** DTO validation, range checks, entity creation
- **Error handling:** Invalid payloads, out-of-range values

### Integration Tests (Planned)

- Full flow: raw message → parse → interpret → store → query
- Concurrent message processing
- Database consistency

## Future Enhancements

### Phase 2: Extended Heading Fields

Currently stored: `HeadingDegrees` (required)

Could add (bytes available in payload):
- `DeviationDegrees` (magnetic deviation correction)
- `VariationDegrees` (magnetic variation correction)
- `ReferenceType` (True vs. Magnetic heading flag)

**Impact:** New migration, entity properties, DTO fields, interpreter expansion.

### Phase 3: Historical Comparison

- Store deviation/variation trends
- Compass rose visualization
- Heading vs. Motion drift analysis

### Phase 4: NMEA 0183 Support

- PGN 127250 is NMEA 2000
- Legacy NMEA 0183 has: `$HEHDT` (True heading), `$HEHDM` (Magnetic)
- Parallel parser/interpreter for 0183 source messages

## Files Modified/Created

| Path | Type | Status |
|------|------|--------|
| `BootManager.Core/Entities/HeadingMeasurement.cs` | NEW | ✅ |
| `BootManager.Infrastructure/Persistence/Configurations/HeadingMeasurementConfiguration.cs` | NEW | ✅ |
| `BootManager.Infrastructure/Persistence/BootManagerDbContext.cs` | MODIFY | ✅ |
| `BootManager.Application/HeadingMeasurements/DTOs/CreateHeadingMeasurementRequestDto.cs` | NEW | ✅ |
| `BootManager.Application/NetworkMessageInterpretation/DTOs/HeadingMessageInterpretationDto.cs` | NEW | ✅ |
| `BootManager.Application/HeadingMeasurements/Services/IHeadingMeasurementService.cs` | NEW | ✅ |
| `BootManager.Application/HeadingMeasurements/Services/HeadingMeasurementService.cs` | NEW | ✅ |
| `BootManager.Application/NetworkMessageInterpretation/Services/HeadingMessageInterpreterService.cs` | NEW | ✅ |
| `BootManager.Application/NetworkMessageParsing/Enums/NetworkMessageType.cs` | MODIFY | ✅ |
| `BootManager.Application/NetworkMessageParsing/Services/NetworkMessageParserService.cs` | MODIFY | ✅ |
| `BootManager.Application/NetworkMessages/Services/NetworkMessageService.cs` | MODIFY | ✅ |
| `BootManager.Application/DependencyInjection.cs` | MODIFY | ✅ |
| `BootManager.Infrastructure/Migrations/[timestamp]_AddHeadingMeasurement.cs` | NEW | ⏳ (Pending) |

## Build Status

```
✅ Solution compiles successfully
⏳ EF Migration pending: dotnet ef migrations add AddHeadingMeasurement
⏳ Database migration pending: dotnet ef database update
```

## References

- NMEA 2000 PGN 127250: IEC 61162-1 Industrial networks
- NMEA 2000 to Degrees conversion: https://www.nmea.de/
- BootManager Architecture: `.docs/ARCHITECTURE.md`
- Original requirements (Dutch): `.github/copilot-instructions.md`

---

**Implementation Date:** 2026-03-27  
**Implemented By:** GitHub Copilot  
**Status:** Code-complete, pending migration & testing
