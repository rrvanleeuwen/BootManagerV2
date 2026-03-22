using BootManager.Tools.Simulator.Models;

namespace BootManager.Tools.Simulator.Scenarios;

public class ScenarioLoader
{
    public IEnumerable<ScenarioDefinition> LoadAll(string path)
    {
        // For now return a single realistic IJsselmeer sailing scenario.
        return new[] { CreateSailingIjsselmeer() };
    }

    public ScenarioDefinition? LoadByName(string name)
    {
        if (string.Equals(name, "SailingIjsselmeer", StringComparison.OrdinalIgnoreCase))
            return CreateSailingIjsselmeer();
        return null;
    }

    private ScenarioDefinition CreateSailingIjsselmeer()
    {
        // Chosen start position: open water near 52.6N, 5.3E (central IJsselmeer)
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
