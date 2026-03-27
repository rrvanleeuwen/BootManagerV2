# Position Measurement Slice - Implementation Specification

**Status:** ✅ Fully Implemented  
**Date:** Original implementation (Documented: 2026-03-27)  
**Related PGN:** 129025 (Position, Rapid Update)  
**NMEA 2000 Spec:** IEC 61162-1

## Overview

The Position measurement slice captures and stores latitude and longitude data from NMEA 2000 PGN 129025 messages. This provides the vessel's geographic location with high-frequency updates.

## Payload Format (PGN 129025)

```
Bytes 0-3: Latitude in 1e-7 degrees (int32, little-endian, signed)
Bytes 4-7: Longitude in 1e-7 degrees (int32, little-endian, signed)
```

### Conversion Examples

**Latitude:**
- Raw bytes 0-3: `0x0E 0x6F 0x62 0x04` → int32 `0x04626F0E` = 73706254
- Degrees: 73706254 × 1e-7 = **7.3706254° N**
- Format: **7°22'14"N** (7 degrees, 22 minutes, 14 seconds North)

**Longitude:**
- Raw bytes 4-7: `0x90 0x1A 0xFF 0x00` → int32 `0x00FF1A90` = 16689808
- Degrees: 16689808 × 1e-7 = **1.6689808° E**
- Format: **1°40'8"E** (1 degree, 40 minutes, 8 seconds East)

**Range:**
- Latitude: -90° (South Pole) to +90° (North Pole)
- Longitude: -180° (West) to +180° (East)
- Resolution: 1e-7° ≈ 1.1 meters at equator

## Implementation Components

### 1. Core Layer

**File:** `BootManager.Core/Entities/PositionMeasurement.cs`

```csharp
public class PositionMeasurement
{
    public int Id { get; private set; }
    public DateTime RecordedAtUtc { get; private set; }
    public string Source { get; private set; }
    public string MessageId { get; private set; }
    public decimal Latitude { get; private set; }           // -90 to +90
    public decimal Longitude { get; private set; }          // -180 to +180
}
```

- **Latitude:** Signed decimal, negative = South, positive = North
- **Longitude:** Signed decimal, negative = West, positive = East
- **Precision:** decimal(10, 7) for 1.1m resolution

### 2. Infrastructure Layer

**Configuration:** `BootManager.Infrastructure/Persistence/Configurations/PositionMeasurementConfiguration.cs`

```csharp
b.ToTable("PositionMeasurements");
b.HasKey(x => x.Id);
b.Property(x => x.Latitude).HasPrecision(10, 7);
b.Property(x => x.Longitude).HasPrecision(10, 7);
b.HasIndex(x => x.RecordedAtUtc);
// Could add spatial index for geographic queries (future)
```

### 3. Application Layer

**DTOs:**
- `CreatePositionMeasurementRequestDto` - Storage request
- `PositionMessageInterpretationDto` - Parse result

**Services:**
- `IPositionMeasurementService` - Interface
- `PositionMeasurementService` - Validation & persistence
- `PositionMessageInterpreterService` - NMEA 2000 decoding

### 4. Interpreter

**File:** `BootManager.Application/NetworkMessageInterpretation/Services/PositionMessageInterpreterService.cs`

Decoding logic (int32, little-endian):
```csharp
// Latitude: bytes 0-3, int32 little-endian, 1e-7 degrees per unit
int latitudeScaled = (int)(
    bytes[0] 
    | (bytes[1] << 8) 
    | (bytes[2] << 16) 
    | (bytes[3] << 24)
);
decimal latitude = latitudeScaled * 1e-7m;

// Longitude: bytes 4-7, int32 little-endian, 1e-7 degrees per unit
int longitudeScaled = (int)(
    bytes[4] 
    | (bytes[5] << 8) 
    | (bytes[6] << 16) 
    | (bytes[7] << 24)
);
decimal longitude = longitudeScaled * 1e-7m;
```

Error handling:
- Minimum 8 bytes required
- Validate: -90 ≤ lat ≤ 90, -180 ≤ lon ≤ 180

### 5. Parser Integration

PGN 129025 → `NetworkMessageType.Position`

Recognized in `NetworkMessageParserService.PgnToType`:
```csharp
{ 129025, NetworkMessageType.Position }
```

### 6. Data Storage

Example database record:
```sql
INSERT INTO PositionMeasurements 
  (RecordedAtUtc, Source, MessageId, Latitude, Longitude)
VALUES 
  ('2026-03-27T10:15:30Z', '127.0.0.1', '129025', 52.3667, 4.9945);
```

Example: Amsterdam harbor (52°22'00.1"N 4°59'40.2"E)

## Geographic Coordinate Systems

### Decimal Degrees (DD)
```
52.3667° N, 4.9945° E
```

### Degrees Minutes Seconds (DMS)
```
52° 22' 0.1" N, 4° 59' 40.2" E
```

### Conversion (DD ↔ DMS)
```
DMS → DD: 52 + (22 / 60) + (0.1 / 3600) = 52.3667°
DD → DMS: 0.3667 × 60 = 22 minutes, 0.002 × 60 = 0.1 seconds
```

## World Regions

| Region | Latitude | Longitude | Example Port |
|--------|----------|-----------|--------------|
| Northern Europe | 50-60°N | 0-10°E | Amsterdam (52.37°N, 4.99°E) |
| Mediterranean | 30-45°N | 0-40°E | Barcelona (41.39°N, 2.17°E) |
| Caribbean | 15-25°N | 60-80°W | St. Thomas (18.34°N, 64.90°W) |
| Pacific | ±30° | 120-180° | Fiji (18.13°S, 178.07°E) |
| Southern Ocean | 60-70°S | Any | Antarctic (66°S, 0°) |

## Accuracy & Precision

| GPS Mode | Horizontal Accuracy | Typical Update Rate |
|----------|-------------------|-------------------|
| Standard GPS | 5-10 meters | 1-5 Hz |
| DGPS (Differential) | 1-2 meters | 1-5 Hz |
| RTK (Real-Time Kinematic) | 1-5 cm | 5-10 Hz |
| Post-processing | 2-5 cm | Offline |

Current storage (1e-7°) = 1.1m resolution at equator → Suitable for Standard/DGPS

## Future Enhancements

### Phase 2: Altitude & Datum

Add fields:
- `AltitudeMeters` (height above sea level or WGS-84 ellipsoid)
- `HorizontalDOP` (Dilution of Precision - GPS accuracy metric)
- `GeoidalSeparation` (difference between ellipsoid and mean sea level)

```csharp
public decimal? AltitudeMeters { get; private set; }
public decimal? GeoidalSeparationMeters { get; private set; }
```

### Phase 3: Track Visualization

- Store entire voyage track
- Map overlay (Leaflet, OpenStreetMap, etc.)
- Anchor detection (position stability analysis)

### Phase 4: Geofencing & Alerts

- Define zones (harbor, anchorage, waypoint)
- Alert when entering/exiting zones
- Navigation waypoint proximity detection

## Performance Notes

- Position updates typically 1-5 Hz (200ms-1s intervals)
- int32 parsing is very fast (4-byte read + multiplication)
- Storage I/O is primary bottleneck (not decode)
- Spatial indexing beneficial for large historical datasets

## NMEA 2000 Specifications

- **PGN 129025:** Position (Rapid Update)
- **Update Rate:** High frequency (typically 1-5 Hz, can be 10 Hz)
- **Precision:** ±1.1 meters (1e-7 degree resolution)
- **Coverage:** Global (requires GPS/GNSS)
- **Datum:** WGS-84 (World Geodetic System 1984)

## Coordinate Validation

```csharp
bool IsValidPosition(decimal lat, decimal lon)
{
    return lat >= -90m && lat <= 90m 
        && lon >= -180m && lon <= 180m;
}

// Invalid: 52.3667, 195.0 (lon > 180)
// Valid:   52.3667, 4.9945  (Amsterdam)
// Valid:   -33.8688, 151.2093 (Sydney)
```

## Files

| Path | Type | Status |
|------|------|--------|
| `BootManager.Core/Entities/PositionMeasurement.cs` | Entity | ✅ |
| `BootManager.Infrastructure/Persistence/Configurations/PositionMeasurementConfiguration.cs` | Config | ✅ |
| `BootManager.Application/PositionMeasurements/DTOs/*` | DTOs | ✅ |
| `BootManager.Application/PositionMeasurements/Services/*` | Services | ✅ |
| `BootManager.Application/NetworkMessageInterpretation/Services/PositionMessageInterpreterService.cs` | Interpreter | ✅ |
| `BootManager.Infrastructure/Migrations/*_AddPositionMeasurement.cs` | Migration | ✅ |

## References

- **NMEA 2000 PGN 129025:** IEC 61162-1
- **WGS-84 Datum:** https://en.wikipedia.org/wiki/World_Geodetic_System
- **Coordinate Systems:** https://en.wikipedia.org/wiki/Geographic_coordinate_system
- **OpenStreetMap:** https://www.openstreetmap.org/
- **MarineTraffic (live AIS):** https://www.marinetraffic.com/

---

**Last Updated:** 2026-03-27 (Documented)  
**Implementation Status:** Production-ready  
**Next Phase:** Altitude/DGPS fields & track visualization
