namespace BootManager.Tools.Simulator.Options;

/// <summary>
/// Opties/configuratie voor de simulator (doel-UDP, interval en scenario-instellingen).
/// </summary>
public class SimulatorOptions
{
    public string TargetIp { get; set; } = "127.0.0.1";
    public int TargetPort { get; set; } = 2000;
    public int IntervalMs { get; set; } = 1000;
    public string Scenario { get; set; } = "SailingIjsselmeer";
    public string? ScenarioPath { get; set; }
}