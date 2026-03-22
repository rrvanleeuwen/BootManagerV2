using BootManager.Tools.Simulator.Models;
using BootManager.Tools.Simulator.Scenarios;

namespace BootManager.Tools.Simulator.Services;

public class SimulationService
{
    private readonly ScenarioLoader _loader = new();

    public void Run()
    {
        var scenarios = _loader.LoadAll("Scenarios");
        foreach (var s in scenarios)
        {
            Console.WriteLine($"Found scenario: {s.Name} - {s.Description}");
        }
    }
}