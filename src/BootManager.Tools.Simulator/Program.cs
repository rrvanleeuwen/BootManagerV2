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
        services.AddHostedService<SimulationService>();
    });

var host = builder.Build();

var config = host.Services.GetRequiredService<IConfiguration>();
Console.WriteLine("BootManager.Tools.Simulator starting...");
Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}");
Console.WriteLine($"Active scenario: {config["Simulator:Scenario"]}");

await host.RunAsync();
