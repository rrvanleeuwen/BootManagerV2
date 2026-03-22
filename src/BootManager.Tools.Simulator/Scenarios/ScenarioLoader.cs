using BootManager.Tools.Simulator.Models;

namespace BootManager.Tools.Simulator.Scenarios;

/// <summary>
/// Laadt beschikbare scenario-definities voor de simulator.
/// </summary>
public class ScenarioLoader
{
    /// <summary>
    /// Laadt alle scenario's uit de opgegeven pad (vooralsnog een enkele ingebouwde).
    /// </summary>
    /// <param name="path">Pad waar scenario's normaal gesproken gezocht worden (wordt momenteel niet gebruikt).</param>
    /// <returns>Collectie van beschikbare scenario-definities.</returns>
    public IEnumerable<ScenarioDefinition> LoadAll(string path)
    {
        // Voor nu één realistisch IJsselmeer scenario teruggeven.
        return new[] { CreateSailingIjsselmeer() };
    }

    /// <summary>
    /// Probeert een scenario te laden op basis van de naam.
    /// </summary>
    /// <param name="name">Naam van het scenario.</param>
    /// <returns>ScenarioDefinition indien gevonden; anders null.</returns>
    public ScenarioDefinition? LoadByName(string name)
    {
        if (string.Equals(name, "SailingIjsselmeer", StringComparison.OrdinalIgnoreCase))
            return CreateSailingIjsselmeer();
        return null;
    }

    /// <summary>
    /// Creëert een voorbeeldscenario voor varen op het IJsselmeer.
    /// </summary>
    /// <returns>Een ingevulde ScenarioDefinition.</returns>
    private ScenarioDefinition CreateSailingIjsselmeer()
    {
        // Startpositie gekozen in open water nabij 52.6N, 5.3E (centraal IJsselmeer)
        return new ScenarioDefinition
        {
            Name = "SailingIjsselmeer",
            Description = "A single sailboat cruising in the middle of the IJsselmeer",
            StartLatitude = 52.6000,
            StartLongitude = 5.3000,
            StartSogKnots = 5.5,
            StartCogDegrees = 85.0,
            StartHeadingDegrees = 83.0,
            StartWindSpeedMps = 5.0,
            StartWindAngleDeg = 45.0,
            StartDepthMeters = 3.5,
            StartBatteryVoltage = 12.6,
            StartBatterySoc = 85.0
        };
    }
}
