using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BootManager.Tools.Simulator.Options;
using BootManager.Tools.Simulator.Services;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<SimulatorOptions>(context.Configuration.GetSection("Simulator"));

        var modeStr = context.Configuration["Simulator:OutputMode"] ?? "NMEA0183";
        if (!Enum.TryParse<SimulatorOutputMode>(modeStr, ignoreCase: true, out var mode))
            mode = SimulatorOutputMode.NMEA0183;

        if (mode == SimulatorOutputMode.NMEA2000 || mode == SimulatorOutputMode.Both)
            services.AddHostedService<SimulationService>();

        if (mode == SimulatorOutputMode.NMEA0183 || mode == SimulatorOutputMode.Both)
            services.AddHostedService<Nmea0183SimulationService>();
    });

var host = builder.Build();

var config = host.Services.GetRequiredService<IConfiguration>();
var outputMode = config["Simulator:OutputMode"] ?? "NMEA0183";
Console.WriteLine("BootManager.Tools.Simulator starting...");
Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}");
Console.WriteLine($"Active scenario: {config["Simulator:Scenario"]}");
Console.WriteLine($"Output mode: {outputMode}");

await host.RunAsync();
