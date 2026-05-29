using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using BootManager.Tools.Ingest.Options;
using BootManager.Tools.Ingest.Services;
using BootManager.Tools.Ingest.Policies;
using BootManager.Core.Enums;
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

        // Registreer client voor ophalen operationele instellingen bij BootManager.Web
        services.AddHttpClient<IOperationalSettingsClientService, OperationalSettingsClientService>();

        // Registreer HttpClient voor API-calls
        services.AddHttpClient<IngestService>();

        // Registreer runtime settings als singleton (thread-safe, live updateable)
        services.AddSingleton<IIngestRuntimeSettings>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<IngestOptions>>().Value;
            return new IngestRuntimeSettings(
                options.ApiBaseUrl,
                options.RawStorageMode,
                options.DefaultSampleIntervalSeconds,
                options.CaptureLogging.Enabled,
                true,  // IngestProcessingEnabled default to true
                options.ListenAddress,
                options.ListenPort);
        });

        // Registreer capture logger als singleton (één bestand per ingest-sessie)
        // Nu AFTER runtimeSettings, zodat CaptureLoggingEnabled kan worden gelezen
        services.AddSingleton<IIngestCaptureLogger>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<IngestOptions>>();
            var runtimeSettings = provider.GetRequiredService<IIngestRuntimeSettings>();
            var logger = provider.GetRequiredService<ILogger<IngestCaptureLogger>>();
            return new IngestCaptureLogger(options, runtimeSettings, logger);
        });

        // Registreer sampling policy als singleton
        services.AddSingleton<IIngestSamplingPolicy>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<IngestOptions>>().Value;
            var logger = provider.GetRequiredService<ILogger<IngestSamplingPolicy>>();
            return new IngestSamplingPolicy(options.RawStorageMode, options.DefaultSampleIntervalSeconds, logger);
        });

        services.AddHostedService<IngestService>();

        // Registreer control server als hosted service (start/stop with host)
        services.AddHostedService<IngestControlServer>();
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

// Haal runtime settings op (singleton geinitialiseerd met appsettings)
var runtimeSettings = host.Services.GetRequiredService<IIngestRuntimeSettings>();
var samplingPolicy = host.Services.GetRequiredService<IIngestSamplingPolicy>();

// Probeer operationele instellingen op te halen bij BootManager.Web
var settingsClient = host.Services.GetRequiredService<IOperationalSettingsClientService>();
var remoteSettings = await settingsClient.TryGetSettingsAsync(ingestOptions.ApiBaseUrl);

if (remoteSettings is not null)
{
    // Update runtime-instellingen met de database/Web-instellingen
    // Let op: ListenAddress en ListenPort worden NIET live aangepast; die hebben herstart nodig
    runtimeSettings.ApiBaseUrl = remoteSettings.ApiBaseUrl;
    runtimeSettings.CaptureLoggingEnabled = remoteSettings.CaptureLoggingEnabled;
    runtimeSettings.IngestProcessingEnabled = remoteSettings.IngestProcessingEnabled;

    // Parse RawStorageMode van string naar enum
    if (Enum.TryParse<RawStorageMode>(remoteSettings.RawStorageMode, ignoreCase: true, out var parsedMode))
    {
        runtimeSettings.RawStorageMode = parsedMode;
    }
    else
    {
        logger.LogWarning(
            "Could not parse RawStorageMode '{Mode}' from remote settings; using fallback All.",
            remoteSettings.RawStorageMode);
        runtimeSettings.RawStorageMode = RawStorageMode.All;
    }

    // Update sampling policy met nieuwe mode/interval
    samplingPolicy.Update(runtimeSettings.RawStorageMode, remoteSettings.DefaultSampleIntervalSeconds);
    runtimeSettings.DefaultSampleIntervalSeconds = remoteSettings.DefaultSampleIntervalSeconds;

    // Log initial configuration from database
    logger.LogInformation(
        "Runtime-instellingen overschreven vanuit BootManager.Web: ListenAddress={ListenAddress}, ListenPort={ListenPort}, ApiBaseUrl={ApiBaseUrl}, CaptureLoggingEnabled={CaptureLoggingEnabled}, IngestProcessingEnabled={IngestProcessingEnabled}, RawStorageMode={RawStorageMode}, DefaultSampleIntervalSeconds={SampleInterval}.",
        ingestOptions.ListenAddress, ingestOptions.ListenPort, runtimeSettings.ApiBaseUrl, runtimeSettings.CaptureLoggingEnabled, runtimeSettings.IngestProcessingEnabled, runtimeSettings.RawStorageMode, runtimeSettings.DefaultSampleIntervalSeconds);
    logger.LogInformation("Configuratiebron: BootManager.Web (database).");

    // Inform about CaptureLoggingEnabled combination
    if (remoteSettings.CaptureLoggingEnabled != ingestOptions.CaptureLogging.Enabled)
    {
        logger.LogInformation(
            "CaptureLoggingEnabled: Database={DatabaseValue}, Appsettings={AppSettingsValue}. Effective result: capture logging {EffectiveState}. " +
            "For capture logging to be active, BOTH appsettings CaptureLogging.Enabled AND database CaptureLoggingEnabled must be true.",
            remoteSettings.CaptureLoggingEnabled, ingestOptions.CaptureLogging.Enabled,
            (remoteSettings.CaptureLoggingEnabled && ingestOptions.CaptureLogging.Enabled) ? "ENABLED" : "DISABLED");
    }
}
else
{
    logger.LogWarning(
        "Operationele instellingen konden niet worden opgehaald bij BootManager.Web. Ingest draait met appsettings als fallback.");
    logger.LogInformation("Configuratiebron: appsettings.json (fallback).");
    logger.LogInformation(
        "RawStorageMode={RawStorageMode}, DefaultSampleIntervalSeconds={SampleInterval}.",
        runtimeSettings.RawStorageMode, runtimeSettings.DefaultSampleIntervalSeconds);
}

Console.WriteLine($"Listen address: {ingestOptions.ListenAddress}");
Console.WriteLine($"Listen port: {ingestOptions.ListenPort}");
Console.WriteLine($"API Base URL: {runtimeSettings.ApiBaseUrl}");

await host.RunAsync();
