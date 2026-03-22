namespace BootManager.Tools.Simulator.Models;

public class BoatState
{
    public DateTime TimestampUtc { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double SogKnots { get; set; }
    public double CogDegrees { get; set; }
    public double HeadingDegrees { get; set; }
    public double WindSpeedMps { get; set; }
    public double WindAngleDeg { get; set; }
    public double DepthMeters { get; set; }
    public double BatteryVoltage { get; set; }
    public double BatterySoc { get; set; }

    public BoatState Clone() => (BoatState)MemberwiseClone();
}
