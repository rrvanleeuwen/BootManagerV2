using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using BootManager.Tools.Ingest.Options;
using BootManager.Tools.Ingest.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        var outputDirectory = AppContext.BaseDirectory;
        var environment = context.HostingEnvironment.EnvironmentName;

        // dotnet run --project keeps the caller's working directory. Load the
        // copied appsettings from the tool output directory so local edits apply.
        config.AddJsonFile(Path.Combine(outputDirectory, "appsettings.json"), optional: true, reloadOnChange: true);
        config.AddJsonFile(Path.Combine(outputDirectory, $"appsettings.{environment}.json"), optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
        config.AddCommandLine(args);
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<IngestOptions>(context.Configuration.GetSection("Ingest"));

        // Registreer capture logger als singleton (één bestand per ingest-sessie)
        services.AddSingleton<IIngestCaptureLogger, IngestCaptureLogger>();

        // Registreer client voor ophalen operationele instellingen bij BootManager.Web
        services.AddHttpClient<IOperationalSettingsClientService, OperationalSettingsClientService>();

        // Registreer HttpClient voor API-calls
        services.AddHttpClient<IngestService>();

        services.AddHostedService<IngestService>();
    });

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILoggerFactory>()
    .CreateLogger("BootManager.Tools.Ingest.Startup");

Console.WriteLine("BootManager.Tools.Ingest starting...");
Console.WriteLine($"Environment: {Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}");

// Haal appsettings-opties op als startpunt (fallback)
var ingestOptions = host.Services.GetRequiredService<IOptions<IngestOptions>>().Value;
logger.LogInformation(
    "Configuratie geladen uit appsettings: ListenAddress={ListenAddress}, ListenPort={ListenPort}, ApiBaseUrl={ApiBaseUrl}.",
    ingestOptions.ListenAddress, ingestOptions.ListenPort, ingestOptions.ApiBaseUrl);

// Probeer operationele instellingen op te halen bij BootManager.Web
var settingsClient = host.Services.GetRequiredService<IOperationalSettingsClientService>();
var remoteSettings = await settingsClient.TryGetSettingsAsync(ingestOptions.ApiBaseUrl);

if (remoteSettings is not null)
{
    // Pas runtime-opties aan met de database/Web-instellingen
    ingestOptions.ListenAddress = remoteSettings.ListenAddress;
    ingestOptions.ListenPort = remoteSettings.ListenPort;
    ingestOptions.ApiBaseUrl = remoteSettings.ApiBaseUrl;
    ingestOptions.CaptureLogging.Enabled = remoteSettings.CaptureLoggingEnabled;

    // rawStorageMode en defaultSampleIntervalSeconds worden gelogd maar nog niet toegepast
    logger.LogInformation(
        "Runtime-instellingen overschreven vanuit BootManager.Web: ListenAddress={ListenAddress}, ListenPort={ListenPort}, ApiBaseUrl={ApiBaseUrl}, CaptureLoggingEnabled={CaptureLoggingEnabled}.",
        ingestOptions.ListenAddress, ingestOptions.ListenPort, ingestOptions.ApiBaseUrl, ingestOptions.CaptureLogging.Enabled);
    logger.LogInformation(
        "RawStorageMode={RawStorageMode} en DefaultSampleIntervalSeconds={DefaultSampleIntervalSeconds} ontvangen maar nog niet toegepast (volgende slice).",
        remoteSettings.RawStorageMode, remoteSettings.DefaultSampleIntervalSeconds);
    logger.LogInformation("Configuratiebron: BootManager.Web (database).");
}
else
{
    logger.LogWarning(
        "Operationele instellingen konden niet worden opgehaald bij BootManager.Web. Ingest draait met appsettings als fallback.");
    logger.LogInformation("Configuratiebron: appsettings.json (fallback).");
}

Console.WriteLine($"Listen address: {ingestOptions.ListenAddress}");
Console.WriteLine($"Listen port: {ingestOptions.ListenPort}");
Console.WriteLine($"API Base URL: {ingestOptions.ApiBaseUrl}");

await host.RunAsync();
