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
/// Als capture logging uitgeschakeld is (via appsettings of runtime), worden er geen bestanden aangemaakt of geschreven.
///
/// De effectieve status wordt bepaald door: appsettings CaptureLogging.Enabled AND runtime CaptureLoggingEnabled.
/// Beide moeten true zijn voor capture logging om actief te zijn.
/// </summary>
public class IngestCaptureLogger : IIngestCaptureLogger
{
    private readonly CaptureLoggingOptions _options;
    private readonly IIngestRuntimeSettings _runtimeSettings;
    private readonly ILogger<IngestCaptureLogger> _logger;
    private StreamWriter? _writer;
    private bool _isEffectivelyEnabled;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="IngestCaptureLogger"/>.
    /// </summary>
    /// <param name="options">Ingest-opties inclusief capture logging configuratie (appsettings).</param>
    /// <param name="runtimeSettings">Runtime settings die CaptureLoggingEnabled bevatten (uit database).</param>
    /// <param name="logger">Logger voor diagnostische meldingen.</param>
    public IngestCaptureLogger(
        IOptions<IngestOptions> options,
        IIngestRuntimeSettings runtimeSettings,
        ILogger<IngestCaptureLogger> logger)
    {
        _options = options.Value.CaptureLogging;
        _runtimeSettings = runtimeSettings;
        _logger = logger;
        _isEffectivelyEnabled = false;
    }

    /// <inheritdoc/>
    public bool IsEnabled => _isEffectivelyEnabled;

    /// <inheritdoc/>
    public Task InitializeAsync()
    {
        // Determine effective state: both appsettings AND runtime must be true
        var appSettingsEnabled = _options.Enabled;
        var runtimeEnabled = _runtimeSettings.CaptureLoggingEnabled;
        _isEffectivelyEnabled = appSettingsEnabled && runtimeEnabled;

        // If either is disabled, do not create file
        if (!_isEffectivelyEnabled)
        {
            var reason = appSettingsEnabled
                ? "database/runtime setting disabled"
                : "appsettings CaptureLogging.Enabled=false";
            _logger.LogInformation(
                "Capture logging is disabled ({Reason}). Appsettings={AppSettings}, Database={Database}",
                reason, appSettingsEnabled, runtimeEnabled);
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

            _logger.LogInformation(
                "Capture logging enabled (appsettings={AppSettings}, database={Database}). Output file: {FilePath}",
                appSettingsEnabled, runtimeEnabled, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing capture logging. Capture logging will be disabled.");
            _writer = null;
            _isEffectivelyEnabled = false;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task WriteAsync(CaptureRecord record)
    {
        // Check both conditions at write time (runtime can change)
        if (!_options.Enabled || !_runtimeSettings.CaptureLoggingEnabled || _writer is null)
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
            _logger.LogError(ex, "Error writing to capture log. Processing continues.");
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
