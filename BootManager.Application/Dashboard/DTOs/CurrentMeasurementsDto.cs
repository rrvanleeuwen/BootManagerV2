namespace BootManager.Application.Dashboard.DTOs;

/// <summary>
/// DTO voor een individuele windmeting op het dashboard.
/// </summary>
public class WindMeasurementDto
{
    /// <summary>
    /// Windhoek in graden (0-360).
    /// </summary>
    public decimal? AngleDegrees { get; init; }

    /// <summary>
    /// Windsnelheid in m/s.
    /// </summary>
    public decimal? SpeedMetersPerSecond { get; init; }

    /// <summary>
    /// Moment waarop de meting is geregistreerd (UTC).
    /// </summary>
    public DateTime? RecordedAtUtc { get; init; }

    /// <summary>
    /// Geeft aan of wind data beschikbaar is.
    /// </summary>
    public bool HasData => AngleDegrees.HasValue && SpeedMetersPerSecond.HasValue;
}

/// <summary>
/// DTO voor een individuele koersmeting op het dashboard.
/// </summary>
public class HeadingMeasurementDto
{
    /// <summary>
    /// Koers in graden (0-360).
    /// </summary>
    public decimal? HeadingDegrees { get; init; }

    /// <summary>
    /// Moment waarop de meting is geregistreerd (UTC).
    /// </summary>
    public DateTime? RecordedAtUtc { get; init; }

    /// <summary>
    /// Geeft aan of heading data beschikbaar is.
    /// </summary>
    public bool HasData => HeadingDegrees.HasValue;
}

/// <summary>
/// DTO voor een individuele positiemeting op het dashboard.
/// </summary>
public class PositionMeasurementDto
{
    /// <summary>
    /// Breedtegraad in decimale graden.
    /// </summary>
    public decimal? Latitude { get; init; }

    /// <summary>
    /// Lengtegraad in decimale graden.
    /// </summary>
    public decimal? Longitude { get; init; }

    /// <summary>
    /// Moment waarop de meting is geregistreerd (UTC).
    /// </summary>
    public DateTime? RecordedAtUtc { get; init; }

    /// <summary>
    /// Geeft aan of position data beschikbaar is.
    /// </summary>
    public bool HasData => Latitude.HasValue && Longitude.HasValue;
}

/// <summary>
/// DTO voor een individuele snelheid-door-water-meting op het dashboard.
/// </summary>
public class SpeedThroughWaterMeasurementDto
{
    /// <summary>
    /// Snelheid door water in knopen.
    /// </summary>
    public decimal? SpeedKnots { get; init; }

    /// <summary>
    /// Moment waarop de meting is geregistreerd (UTC).
    /// </summary>
    public DateTime? RecordedAtUtc { get; init; }

    /// <summary>
    /// Geeft aan of speed through water data beschikbaar is.
    /// </summary>
    public bool HasData => SpeedKnots.HasValue;
}

/// <summary>
/// DTO voor een individuele bewegingsmeting (COG/SOG) op het dashboard.
/// </summary>
public class MotionMeasurementDto
{
    /// <summary>
    /// Koers over grond in graden (0-359.99).
    /// </summary>
    public decimal? CourseOverGroundDegrees { get; init; }

    /// <summary>
    /// Snelheid over grond in knopen.
    /// </summary>
    public decimal? SpeedOverGroundKnots { get; init; }

    /// <summary>
    /// Moment waarop de meting is geregistreerd (UTC).
    /// </summary>
    public DateTime? RecordedAtUtc { get; init; }

    /// <summary>
    /// Geeft aan of motion data beschikbaar is.
    /// </summary>
    public bool HasData => CourseOverGroundDegrees.HasValue && SpeedOverGroundKnots.HasValue;
}

/// <summary>
/// DTO voor een individuele dieptemeting op het dashboard.
/// </summary>
public class DepthMeasurementDto
{
    /// <summary>
    /// Diepte in meters.
    /// </summary>
    public decimal? DepthMeters { get; init; }

    /// <summary>
    /// Moment waarop de meting is geregistreerd (UTC).
    /// </summary>
    public DateTime? RecordedAtUtc { get; init; }

    /// <summary>
    /// Geeft aan of depth data beschikbaar is.
    /// </summary>
    public bool HasData => DepthMeters.HasValue;
}

/// <summary>
/// DTO voor een individuele watertemperatuurmeting op het dashboard.
/// </summary>
public class WaterTemperatureMeasurementDto
{
    /// <summary>
    /// Watertemperatuur in graden Celsius.
    /// </summary>
    public decimal? TemperatureCelsius { get; init; }

    /// <summary>
    /// Moment waarop de meting is geregistreerd (UTC).
    /// </summary>
    public DateTime? RecordedAtUtc { get; init; }

    /// <summary>
    /// Geeft aan of water temperature data beschikbaar is.
    /// </summary>
    public bool HasData => TemperatureCelsius.HasValue;
}

/// <summary>
/// DTO voor een individuele batterijmeting op het dashboard.
/// </summary>
public class BatteryMeasurementDto
{
    /// <summary>
    /// Spanning in volts.
    /// </summary>
    public decimal? Voltage { get; init; }

    /// <summary>
    /// Laadtoestand in procenten (0-100), kan null zijn.
    /// </summary>
    public int? StateOfCharge { get; init; }

    /// <summary>
    /// Moment waarop de meting is geregistreerd (UTC).
    /// </summary>
    public DateTime? RecordedAtUtc { get; init; }

    /// <summary>
    /// Geeft aan of battery data beschikbaar is.
    /// </summary>
    public bool HasData => Voltage.HasValue;
}

/// <summary>
/// DTO voor alle huidige/recentste meetwaarden op het dashboard.
/// </summary>
public class CurrentMeasurementsDto
{
    /// <summary>
    /// Recentste windmeting.
    /// </summary>
    public WindMeasurementDto Wind { get; init; } = new();

    /// <summary>
    /// Recentste koersmeting.
    /// </summary>
    public HeadingMeasurementDto Heading { get; init; } = new();

    /// <summary>
    /// Recentste positiemeting.
    /// </summary>
    public PositionMeasurementDto Position { get; init; } = new();

    /// <summary>
    /// Recentste snelheid-door-water-meting.
    /// </summary>
    public SpeedThroughWaterMeasurementDto SpeedThroughWater { get; init; } = new();

    /// <summary>
    /// Recentste bewegingsmeting (COG/SOG).
    /// </summary>
    public MotionMeasurementDto Motion { get; init; } = new();

    /// <summary>
    /// Recentste dieptemeting.
    /// </summary>
    public DepthMeasurementDto Depth { get; init; } = new();

    /// <summary>
    /// Recentste watertemperatuurmeting.
    /// </summary>
    public WaterTemperatureMeasurementDto WaterTemperature { get; init; } = new();

    /// <summary>
    /// Recentste batterijmeting.
    /// </summary>
    public BatteryMeasurementDto Battery { get; init; } = new();

    /// <summary>
    /// Geeft aan of er überhaupt meetgegevens beschikbaar zijn.
    /// </summary>
    public bool HasAnyData =>
        Wind.HasData || Heading.HasData || Position.HasData ||
        SpeedThroughWater.HasData || Motion.HasData || Depth.HasData ||
        WaterTemperature.HasData || Battery.HasData;
}
