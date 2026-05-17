namespace BootManager.Tools.Simulator.Options;

/// <summary>
/// Bepaalt welke outputmodus de simulator gebruikt.
/// </summary>
public enum SimulatorOutputMode
{
    /// <summary>Verstuurt alleen NMEA2000-achtige raw regels (bestaand gedrag).</summary>
    NMEA2000,
    /// <summary>Verstuurt alleen NMEA 0183 sentences.</summary>
    NMEA0183,
    /// <summary>Verstuurt zowel NMEA2000-achtige raw regels als NMEA 0183 sentences.</summary>
    Both
}

/// <summary>
/// Opties/configuratie voor de simulator (doel-UDP, interval, scenario en outputmodus).
/// </summary>
public class SimulatorOptions
{
    public string TargetIp { get; set; } = "127.0.0.1";
    public int TargetPort { get; set; } = 2000;
    public int IntervalMs { get; set; } = 1000;
    public string Scenario { get; set; } = "SailingIjsselmeer";
    public string? ScenarioPath { get; set; }

    /// <summary>
    /// Bepaalt welke outputmodus actief is: NMEA2000 (standaard), NMEA0183 of Both.
    /// </summary>
    public SimulatorOutputMode OutputMode { get; set; } = SimulatorOutputMode.NMEA2000;

    /// <summary>
    /// IP-adres waarnaar NMEA 0183 sentences worden verstuurd.
    /// Standaard 127.0.0.1 (passend bij de Ingest NMEA0183 listener).
    /// </summary>
    public string Nmea0183TargetIp { get; set; } = "127.0.0.1";

    /// <summary>
    /// UDP-poort waarnaar NMEA 0183 sentences worden verstuurd.
    /// Standaard 10110 (passend bij de Ingest NMEA0183 listener).
    /// </summary>
    public int Nmea0183TargetPort { get; set; } = 10110;

    /// <summary>
    /// Wanneer true worden ook negatieve testvarianten meegestuurd:
    /// MWV status V, RMC status V, GGA fixkwaliteit 0 en een sentence met ongeldige checksum.
    /// Deze veroorzaken raw opslag maar geen measurement-opslag.
    /// </summary>
    public bool IncludeNegativeTestSentences { get; set; } = false;
}