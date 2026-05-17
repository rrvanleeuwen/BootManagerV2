namespace BootManager.Tools.Simulator.Models;

/// <summary>
/// Definitie van een simulatie-scenario: beginwaarden voor positie, beweging en omgeving.
/// </summary>
public class ScenarioDefinition
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Start position and motion
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    public double StartSogKnots { get; set; }
    public double StartCogDegrees { get; set; }
    public double StartHeadingDegrees { get; set; }

    // Environment
    public double StartWindSpeedMps { get; set; }
    public double StartWindAngleDeg { get; set; }
    public double StartDepthMeters { get; set; }

    // Power
    public double StartBatteryVoltage { get; set; }
    public double StartBatterySoc { get; set; }

    // Water
    /// <summary>Startwaarde voor snelheid door water in knoten (PGN 128259).</summary>
    public double StartSpeedThroughWaterKnots { get; set; }

    /// <summary>Startwaarde voor watertemperatuur in graden Celsius (PGN 130312).</summary>
    public double StartWaterTemperatureCelsius { get; set; }
}
