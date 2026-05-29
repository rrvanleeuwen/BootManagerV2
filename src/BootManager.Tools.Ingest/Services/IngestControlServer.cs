using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BootManager.Tools.Ingest.Options;
using BootManager.Tools.Ingest.Policies;
using BootManager.Core.Enums;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BootManager.Tools.Ingest.Services;

/// <summary>
/// Lokale control API voor BootManager.Tools.Ingest.
/// Stelt Web in staat om Ingest-instellingen opnieuw in te laden zonder procesrestart.
/// De API luistert standaard alleen op 127.0.0.1 voor veiligheid.
/// Geimplementeerd met HttpListener (geen ASP.NET Core dependency).
/// </summary>
public class IngestControlServer : IHostedService
{
    private readonly IOptions<IngestOptions> _options;
    private readonly ILogger<IngestControlServer> _logger;
    private readonly IOperationalSettingsClientService _settingsClient;
    private readonly IIngestRuntimeSettings _runtimeSettings;
    private readonly IIngestSamplingPolicy _samplingPolicy;
    private HttpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _listeningTask;

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="IngestControlServer"/>.
    /// </summary>
    public IngestControlServer(
        IOptions<IngestOptions> options,
        ILogger<IngestControlServer> logger,
        IOperationalSettingsClientService settingsClient,
        IIngestRuntimeSettings runtimeSettings,
        IIngestSamplingPolicy samplingPolicy)
    {
        _options = options;
        _logger = logger;
        _settingsClient = settingsClient;
        _runtimeSettings = runtimeSettings;
        _samplingPolicy = samplingPolicy;
    }

    /// <summary>
    /// Start de control API server.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var controlApiOptions = _options.Value.ControlApi;

        if (!controlApiOptions.Enabled)
        {
            _logger.LogInformation("ControlApi is disabled; skipping startup.");
            return;
        }

        try
        {
            _listener = new HttpListener();
            var prefix = BuildHttpListenerPrefix(controlApiOptions.ListenAddress, controlApiOptions.ListenPort);

            _logger.LogInformation("Starting IngestControlServer on {Prefix}...", prefix);

            _listener.Prefixes.Add(prefix);
            _listener.Start();

            _cts = new CancellationTokenSource();
            _listeningTask = ListenAsync(_cts.Token);

            _logger.LogInformation("IngestControlServer started successfully on {Prefix}", prefix);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start IngestControlServer: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Bouwt de HttpListener-prefix voor de control API zonder netwerkresources te openen.
    /// Normaliseert een leeg adres naar localhost en vertaalt 0.0.0.0 naar de wildcardhost
    /// die HttpListener cross-platform verwacht.
    /// </summary>
    internal static string BuildHttpListenerPrefix(string? listenAddress, int listenPort)
    {
        var host = string.IsNullOrWhiteSpace(listenAddress)
            ? "127.0.0.1"
            : listenAddress.Trim();

        if (host == "0.0.0.0")
        {
            host = "*";
        }
        else if (host.Contains(':') && !host.StartsWith('[') && !host.EndsWith(']'))
        {
            host = $"[{host}]";
        }

        return $"http://{host}:{listenPort}/";
    }

    /// <summary>
    /// Stop de control API server.
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Stop de HttpListener eerst (dit maakt GetContextAsync() los)
            if (_listener is not null)
            {
                _listener.Stop();
                _listener.Close();
                (_listener as IDisposable)?.Dispose();
            }

            // Signaleer aan de luister-loop om te stoppen
            if (_cts is not null)
            {
                _cts.Cancel();
            }

            // Wacht op de listening task, maar niet langer dan een paar seconden
            if (_listeningTask is not null)
            {
                try
                {
                    // Gebruik een timeout om Ctrl+C niet te blokkeren
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts.CancelAfter(TimeSpan.FromSeconds(5));
                    await _listeningTask.WaitAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // Expected bij shutdown
                }
                catch (ObjectDisposedException)
                {
                    // Expected als listener reeds disposed is
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug(ex, "Exception during IngestControlServer shutdown (expected)");
        }
        finally
        {
            _cts?.Dispose();
            _logger.LogInformation("IngestControlServer stopped.");
        }
    }

    /// <summary>
    /// Luister naar inkomende HTTP-requests.
    /// </summary>
    private async Task ListenAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _listener is not null && _listener.IsListening)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = HandleRequestAsync(context); // Fire and forget with error handling
            }
            catch (HttpListenerException ex) when (ex.ErrorCode == 995)
            {
                // Operation aborted - expected during shutdown
                _logger.LogDebug("HttpListener aborted (expected during shutdown)");
                break;
            }
            catch (ObjectDisposedException)
            {
                // HttpListener was disposed/closed - expected during shutdown
                _logger.LogDebug("HttpListener disposed (expected during shutdown)");
                break;
            }
            catch (InvalidOperationException)
            {
                // HttpListener not listening - expected during shutdown
                _logger.LogDebug("HttpListener not listening (expected during shutdown)");
                break;
            }
            catch (Exception ex) when (cancellationToken.IsCancellationRequested)
            {
                // Any exception during shutdown is expected
                _logger.LogDebug("Exception during shutdown (expected): {Message}", ex.Message);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ControlServer listener");
            }
        }
    }

    /// <summary>
    /// Verwerk een inkomende HTTP-request.
    /// </summary>
    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            var response = context.Response;

            // Zet Content-Type op JSON
            response.ContentType = "application/json";

            // Route requests
            if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/status")
            {
                await HandleGetStatus(response);
            }
            else if (request.HttpMethod == "POST" && request.Url?.AbsolutePath == "/reload-settings")
            {
                await HandlePostReloadSettings(response);
            }
            else if (request.HttpMethod == "GET" && request.Url?.AbsolutePath == "/health")
            {
                response.StatusCode = 200;
                var healthJson = JsonSerializer.Serialize(new { status = "OK" });
                await response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(healthJson));
            }
            else
            {
                response.StatusCode = 404;
                var errorJson = JsonSerializer.Serialize(new { error = "Not found" });
                await response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(errorJson));
            }

            response.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ControlServer request");
        }
    }

    /// <summary>
    /// GET /status - geeft huidige runtime-instellingen en status terug.
    /// </summary>
    private async Task HandleGetStatus(HttpListenerResponse response)
    {
        try
        {
            var currentOptions = _options.Value;
            var statusResponse = new StatusResponse
            {
                Running = true,
                ApiBaseUrl = _runtimeSettings.ApiBaseUrl,
                RawStorageMode = _runtimeSettings.RawStorageMode.ToString(),
                DefaultSampleIntervalSeconds = _runtimeSettings.DefaultSampleIntervalSeconds,
                CaptureLoggingEnabled = _runtimeSettings.CaptureLoggingEnabled,
                IngestProcessingEnabled = _runtimeSettings.IngestProcessingEnabled,
                ListenAddress = _runtimeSettings.ListenAddress,
                ListenPort = _runtimeSettings.ListenPort,
                ListenAddressRestartRequired = _runtimeSettings.ListenAddress != currentOptions.ListenAddress,
                ListenPortRestartRequired = _runtimeSettings.ListenPort != currentOptions.ListenPort
            };

            response.StatusCode = 200;
            var json = JsonSerializer.Serialize(statusResponse);
            var buffer = System.Text.Encoding.UTF8.GetBytes(json);
            await response.OutputStream.WriteAsync(buffer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandleGetStatus");
            response.StatusCode = 500;
            var errorJson = JsonSerializer.Serialize(new { error = ex.Message });
            await response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(errorJson));
        }
    }

    /// <summary>
    /// POST /reload-settings - haalt nieuwste settings op en past ze live toe.
    /// Implementeert fallback-strategie: probeer eerst configured/bootstrap URL (stabiel),
    /// dan runtime ApiBaseUrl (mutable, kan fout zijn).
    /// </summary>
    private async Task HandlePostReloadSettings(HttpListenerResponse response)
    {
        try
        {
            _logger.LogInformation("Reload settings request received...");

            // Haal ingesteld configured/bootstrap ApiBaseUrl op
            var configuredApiBaseUrl = _options.Value.ApiBaseUrl;
            var runtimeApiBaseUrl = _runtimeSettings.ApiBaseUrl;

            // Stap 1: Probeer eerst met configured/bootstrap URL (stabiel, aanbevolen route)
            _logger.LogInformation("Attempting to fetch settings using configured ApiBaseUrl (bootstrap): {Url}", configuredApiBaseUrl);
            var newSettings = await _settingsClient.TryGetSettingsAsync(configuredApiBaseUrl);

            // Stap 2: Als configured URL faalt EN runtime URL verschilt, probeer runtime URL als fallback
            if (newSettings is null && runtimeApiBaseUrl != configuredApiBaseUrl)
            {
                _logger.LogInformation(
                    "Configured ApiBaseUrl ({ConfiguredUrl}) failed. Attempting fallback with runtime ApiBaseUrl: {RuntimeUrl}",
                    configuredApiBaseUrl, runtimeApiBaseUrl);

                newSettings = await _settingsClient.TryGetSettingsAsync(runtimeApiBaseUrl);

                if (newSettings is not null)
                {
                    _logger.LogInformation(
                        "Successfully fetched settings from fallback runtime ApiBaseUrl ({Url}).",
                        runtimeApiBaseUrl);
                }
            }

            if (newSettings is null)
            {
                _logger.LogWarning(
                    "Failed to fetch remote settings from configured ApiBaseUrl ({ConfiguredUrl}) or runtime ApiBaseUrl ({RuntimeUrl}). Reload failed.",
                    configuredApiBaseUrl, runtimeApiBaseUrl);
                response.StatusCode = 503; // Service unavailable
                var errorJson = JsonSerializer.Serialize(new { 
                    success = false, 
                    message = "Failed to fetch settings from BootManager.Web via configured or runtime API URLs"
                });
                await response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(errorJson));
                return;
            }

            var appliedFields = new List<string>();
            var restartRequired = new List<string>();
            var currentOptions = _options.Value;

            // Check voor listener-wijzigingen die herstart vereisen
            if (newSettings.ListenAddress != currentOptions.ListenAddress)
            {
                restartRequired.Add("ListenAddress");
            }

            if (newSettings.ListenPort != currentOptions.ListenPort)
            {
                restartRequired.Add("ListenPort");
            }

            // Update veilige runtime-instellingen
            if (newSettings.ApiBaseUrl != _runtimeSettings.ApiBaseUrl)
            {
                _runtimeSettings.ApiBaseUrl = newSettings.ApiBaseUrl;
                appliedFields.Add("ApiBaseUrl");
                _logger.LogInformation("Updated ApiBaseUrl to {Url}", newSettings.ApiBaseUrl);
            }

            // Update RawStorageMode en DefaultSampleIntervalSeconds
            if (Enum.TryParse<RawStorageMode>(newSettings.RawStorageMode, ignoreCase: true, out var parsedMode))
            {
                if (parsedMode != _runtimeSettings.RawStorageMode)
                {
                    _samplingPolicy.Update(parsedMode, newSettings.DefaultSampleIntervalSeconds);
                    _runtimeSettings.RawStorageMode = parsedMode;
                    appliedFields.Add("RawStorageMode");
                    _logger.LogInformation("Updated RawStorageMode to {Mode}", parsedMode);
                }
            }
            else
            {
                _logger.LogWarning("Could not parse RawStorageMode '{Mode}'", newSettings.RawStorageMode);
            }

            if (newSettings.DefaultSampleIntervalSeconds != _runtimeSettings.DefaultSampleIntervalSeconds)
            {
                _samplingPolicy.Update(_runtimeSettings.RawStorageMode, newSettings.DefaultSampleIntervalSeconds);
                _runtimeSettings.DefaultSampleIntervalSeconds = newSettings.DefaultSampleIntervalSeconds;
                appliedFields.Add("DefaultSampleIntervalSeconds");
                _logger.LogInformation("Updated DefaultSampleIntervalSeconds to {Interval}", newSettings.DefaultSampleIntervalSeconds);
            }

            // CaptureLoggingEnabled: voor nu niet live aanpasbaar, alleen rapporteren
            if (newSettings.CaptureLoggingEnabled != _runtimeSettings.CaptureLoggingEnabled)
            {
                restartRequired.Add("CaptureLoggingEnabled");
                _logger.LogWarning("CaptureLoggingEnabled would change but is not yet supported for live update. Restart required.");
            }

            // IngestProcessingEnabled: kan live worden aangepast
            if (newSettings.IngestProcessingEnabled != _runtimeSettings.IngestProcessingEnabled)
            {
                _runtimeSettings.IngestProcessingEnabled = newSettings.IngestProcessingEnabled;
                appliedFields.Add("IngestProcessingEnabled");
                var action = newSettings.IngestProcessingEnabled ? "enabled" : "disabled";
                _logger.LogInformation("Ingest processing {Action}", action);
            }

            var result = new ReloadSettingsResponse
            {
                Success = true,
                Message = "Settings reloaded successfully.",
                AppliedFields = appliedFields,
                RestartRequiredFields = restartRequired
            };

            _logger.LogInformation(
                "Settings reload completed. Applied: {Applied}, Restart required: {Restart}",
                string.Join(", ", appliedFields),
                string.Join(", ", restartRequired));

            response.StatusCode = 200;
            var json = JsonSerializer.Serialize(result);
            await response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(json));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in HandlePostReloadSettings: {Message}", ex.Message);
            response.StatusCode = 500;
            var errorJson = JsonSerializer.Serialize(new { 
                success = false, 
                message = ex.Message 
            });
            await response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes(errorJson));
        }
    }
}

/// <summary>
/// Response DTO voor GET /status endpoint.
/// </summary>
public class StatusResponse
{
    [JsonPropertyName("running")]
    public bool Running { get; set; }

    [JsonPropertyName("apiBaseUrl")]
    public string ApiBaseUrl { get; set; } = "";

    [JsonPropertyName("rawStorageMode")]
    public string RawStorageMode { get; set; } = "";

    [JsonPropertyName("defaultSampleIntervalSeconds")]
    public int DefaultSampleIntervalSeconds { get; set; }

    [JsonPropertyName("captureLoggingEnabled")]
    public bool CaptureLoggingEnabled { get; set; }

    [JsonPropertyName("ingestProcessingEnabled")]
    public bool IngestProcessingEnabled { get; set; }

    [JsonPropertyName("listenAddress")]
    public string ListenAddress { get; set; } = "";

    [JsonPropertyName("listenPort")]
    public int ListenPort { get; set; }

    [JsonPropertyName("listenAddressRestartRequired")]
    public bool ListenAddressRestartRequired { get; set; }

    [JsonPropertyName("listenPortRestartRequired")]
    public bool ListenPortRestartRequired { get; set; }
}

/// <summary>
/// Response DTO voor POST /reload-settings endpoint.
/// </summary>
public class ReloadSettingsResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = "";

    [JsonPropertyName("appliedFields")]
    public List<string> AppliedFields { get; set; } = new();

    [JsonPropertyName("restartRequiredFields")]
    public List<string> RestartRequiredFields { get; set; } = new();
}
