using BootManager.Tools.Simulator.Models;

namespace BootManager.Tools.Simulator.Scenarios;

public class ScenarioLoader
{
    public IEnumerable<ScenarioDefinition> LoadAll(string path)
    {
        // Placeholder: no real IO yet; return a sample scenario so project builds.
        return new[] { new ScenarioDefinition("Sample", "A sample scenario") };
    }
}