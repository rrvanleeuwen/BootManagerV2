using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BootManager.Tools.Ingest.Options;

namespace BootManager.Tools.Ingest.Services;

/// <summary>
/// Background service voor ingest van netwerkgegevens.
/// Dit is de basis; latere versies zullen UDP-listening en verwerking implementeren.
/// </summary>
public class IngestService : BackgroundService
{
    private readonly IOptions<IngestOptions> _options;
    private readonly ILogger<IngestService> _logger;

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="IngestService"/>.
    /// </summary>
    /// <param name="options">De ingest-opties uit configuratie.</param>
    /// <param name="logger">Logger-instantie.</param>
    public IngestService(IOptions<IngestOptions> options, ILogger<IngestService> logger)
    {
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Voert de ingest-service uit in de achtergrond.
    /// </summary>
    /// <param name="stoppingToken">Token om de service tot stoppen.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IngestService is starting...");
        _logger.LogInformation("Configured to listen on {Address}:{Port}", _options.Value.ListenAddress, _options.Value.ListenPort);
        _logger.LogInformation("Queue size: {MaxQueueSize}, Batch size: {BatchSize}", _options.Value.MaxQueueSize, _options.Value.BatchSize);

        // TODO: UDP-listener implementeren
        // TODO: Berichtenverwerking implementeren
        // TODO: Connectie naar BootManager.Web API implementeren

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("IngestService is stopping.");
        }
    }
}