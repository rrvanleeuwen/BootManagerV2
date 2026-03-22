namespace BootManager.Tools.Simulator.Models;

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
}
