using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        // Register services here
    });
var host = builder.Build();
var config = host.Services.GetRequiredService<IConfiguration>();
Console.WriteLine("BootManager.Tools.Simulator starting...");
Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}");
Console.WriteLine($"Simulator setting example: {config["Simulator:Example"]}");
await host.RunAsync();