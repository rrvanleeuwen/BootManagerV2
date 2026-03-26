using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BootManager.Tools.Ingest.Options;
using BootManager.Tools.Ingest.Services;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<IngestOptions>(context.Configuration.GetSection("Ingest"));
        
        // Registreer HttpClient voor API-calls
        services.AddHttpClient<IngestService>();
        
        services.AddHostedService<IngestService>();
    });

var host = builder.Build();

var config = host.Services.GetRequiredService<IConfiguration>();
Console.WriteLine("BootManager.Tools.Ingest starting...");
Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}");

var ingestConfig = config.GetSection("Ingest");
Console.WriteLine($"Listen address: {ingestConfig["ListenAddress"]}");
Console.WriteLine($"Listen port: {ingestConfig["ListenPort"]}");
Console.WriteLine($"API Base URL: {ingestConfig["ApiBaseUrl"]}");

await host.RunAsync();