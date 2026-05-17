namespace BootManager.Tools.Simulator.Models;

/// <summary>
/// Houdt de huidige toestand van de boot bij (positie, snelheid, sensorgegevens, stroom).
/// </summary>
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

    /// <summary>Snelheid door het water in knoten (PGN 128259, licht afwijkend van SOG).</summary>
    public double SpeedThroughWaterKnots { get; set; }

    /// <summary>Watertemperatuur in graden Celsius (PGN 130312).</summary>
    public double WaterTemperatureCelsius { get; set; }

    /// <summary>
    /// Maakt een ondiepe kopie van de huidige toestand.
    /// </summary>
    /// <returns>Een kloon van deze BoatState.</returns>
    public BoatState Clone() => (BoatState)MemberwiseClone();
}
