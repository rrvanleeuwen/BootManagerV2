using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BootManager.Tools.Ingest.Models;
using BootManager.Tools.Ingest.Options;

namespace BootManager.Tools.Ingest.Services;

/// <summary>
/// Implementatie van <see cref="IIngestCaptureLogger"/> die per ontvangen ingest-regel
/// een NDJSON-record wegschrijft naar een timestamped logbestand.
/// Aangemaakt bestand: <c>{Directory}/{FilePrefix}-yyyyMMdd-HHmmss.ndjson</c>.
/// Als capture logging uitgeschakeld is, worden er geen bestanden aangemaakt of geschreven.
/// </summary>
public class IngestCaptureLogger : IIngestCaptureLogger
{
    private readonly CaptureLoggingOptions _options;
    private readonly ILogger<IngestCaptureLogger> _logger;
    private StreamWriter? _writer;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="IngestCaptureLogger"/>.
    /// </summary>
    /// <param name="options">Ingest-opties inclusief capture logging configuratie.</param>
    /// <param name="logger">Logger voor diagnostische meldingen.</param>
    public IngestCaptureLogger(IOptions<IngestOptions> options, ILogger<IngestCaptureLogger> logger)
    {
        _options = options.Value.CaptureLogging;
        _logger = logger;
    }

    /// <inheritdoc/>
    public bool IsEnabled => _options.Enabled;

    /// <inheritdoc/>
    public Task InitializeAsync()
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Capture logging is uitgeschakeld.");
            return Task.CompletedTask;
        }

        try
        {
            var directory = string.IsNullOrWhiteSpace(_options.Directory)
                ? "logs/ingest-capture"
                : _options.Directory;
            var absoluteDirectory = Path.GetFullPath(directory);
            System.IO.Directory.CreateDirectory(absoluteDirectory);

            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var fileName = $"{_options.FilePrefix}-{timestamp}.ndjson";
            var filePath = Path.Combine(absoluteDirectory, fileName);

            _writer = new StreamWriter(filePath, append: false, encoding: Encoding.UTF8)
            {
                AutoFlush = true
            };

            _logger.LogInformation("Capture logging ingeschakeld. Logbestand: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij initialiseren van capture logging. Capture logging wordt uitgeschakeld.");
            _writer = null;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task WriteAsync(CaptureRecord record)
    {
        if (_writer is null)
        {
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(record, JsonOptions);
            await _writer.WriteLineAsync(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij schrijven naar capture log. Verwerking gaat door.");
        }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_writer is not null)
        {
            try
            {
                await _writer.FlushAsync();
                await _writer.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij sluiten van capture logbestand.");
            }
            finally
            {
                _writer = null;
            }
        }
    }
}
