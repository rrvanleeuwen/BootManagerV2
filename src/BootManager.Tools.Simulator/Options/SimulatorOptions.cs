namespace BootManager.Tools.Simulator.Options;

/// <summary>
/// Beschrijft beschikbare NMEA0183-profielen voor de simulator.
/// </summary>
public enum Nmea0183Profile
{
    /// <summary>Behoudt bestaand gedrag (talker "II").</summary>
    Default,

    /// <summary>YDEN03-achtig profiel: YD-talkers, extra raw-only sentences en AIS raw-achtige regels.</summary>
    YDEN03
}

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
/// Standaard outputmodus is <see cref="SimulatorOutputMode.NMEA0183"/>, omdat de echte YDEN-03
/// route UDP NMEA 0183 gebruikt. Beide output-stromen sturen standaard naar poort 10110
/// zodat de gecombineerde Ingest UDP-listener per regel het protocol herkent.
/// Alternatief: gebruik poort 2000 als Ingest en simulator beiden expliciet op die poort zijn geconfigureerd.
/// </summary>
public class SimulatorOptions
{
    public string TargetIp { get; set; } = "127.0.0.1";

    /// <summary>
    /// UDP-poort waarnaar NMEA2000/raw-like regels worden verstuurd.
    /// Standaard 10110 (gecombineerde Ingest listener). Alternatief: 2000.
    /// </summary>
    public int TargetPort { get; set; } = 10110;
    public int IntervalMs { get; set; } = 1000;
    public string Scenario { get; set; } = "SailingIjsselmeer";
    public string? ScenarioPath { get; set; }

    /// <summary>
    /// Bepaalt welke outputmodus actief is: NMEA0183 (standaard), NMEA2000 of Both.
    /// Standaard NMEA0183, omdat de echte YDEN-03 route UDP NMEA 0183 gebruikt.
    /// Bij Both sturen beide stromen naar dezelfde ingestpoort; Ingest herkent het protocol per regel.
    /// <c>TargetPort</c> is alleen relevant bij OutputMode NMEA2000 of Both.
    /// </summary>
    public SimulatorOutputMode OutputMode { get; set; } = SimulatorOutputMode.NMEA0183;

    /// <summary>
    /// IP-adres waarnaar NMEA 0183 sentences worden verstuurd.
    /// Standaard 127.0.0.1.
    /// </summary>
    public string Nmea0183TargetIp { get; set; } = "127.0.0.1";

    /// <summary>
    /// UDP-poort waarnaar NMEA 0183 sentences worden verstuurd.
    /// Standaard 10110 (gecombineerde Ingest listener). Alternatief: 2000.
    /// </summary>
    public int Nmea0183TargetPort { get; set; } = 10110;

    /// <summary>
    /// Wanneer true worden ook negatieve testvarianten meegestuurd:
    /// MWV status V, RMC status V, GGA fixkwaliteit 0 en een sentence met ongeldige checksum.
    /// Deze veroorzaken raw opslag maar geen measurement-opslag.
    /// </summary>
    public bool IncludeNegativeTestSentences { get; set; } = false;

    /// <summary>
    /// Selecteert een NMEA0183 profiel dat bepaalt hoe NMEA0183-sentences gegenereerd worden.
    /// Default behoudt bestaand gedrag; YDEN03 zorgt voor YD-talker-prefixen en extra raw-only sentences.
    /// </summary>
    public Nmea0183Profile Nmea0183Profile { get; set; } = Nmea0183Profile.Default;
}