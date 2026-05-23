using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BootManager.Web.Services;

/// <summary>
/// Interface voor communicatie met de BootManager.Tools.Ingest control API.
/// </summary>
public interface IIngestControlClient
{
    /// <summary>
    /// Haalt de huidige status van de Ingest control API op.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Het status-response, of null als de API niet bereikbaar is.</returns>
    Task<IngestControlStatusResponse?> GetStatusAsync(CancellationToken ct = default);

    /// <summary>
    /// Verzendt een reload-settings commando naar de Ingest control API.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Het reload-response, of null als de API niet bereikbaar is.</returns>
    Task<IngestControlReloadResponse?> ReloadSettingsAsync(CancellationToken ct = default);
}

/// <summary>
/// Client-service voor communicatie met de BootManager.Tools.Ingest control API.
/// Stelt Web in staat om Ingest-instellingen opnieuw in te laden zonder procesrestart.
/// </summary>
public class IngestControlClient : IIngestControlClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IngestControlClient> _logger;
    private readonly string _controlApiBaseUrl;

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="IngestControlClient"/>.
    /// </summary>
    /// <param name="httpClient">De HTTP-client voor API-aanroepen.</param>
    /// <param name="logger">De logger.</param>
    /// <param name="controlApiBaseUrl">De base URL van de Ingest control API (bijv. http://127.0.0.1:5010)</param>
    public IngestControlClient(HttpClient httpClient, ILogger<IngestControlClient> logger, string controlApiBaseUrl)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
        _logger = logger;
        _controlApiBaseUrl = controlApiBaseUrl.TrimEnd('/');
    }

    /// <summary>
    /// Haalt de huidige status van de Ingest control API op.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Het status-response, of null als de API niet bereikbaar is.</returns>
    public async Task<IngestControlStatusResponse?> GetStatusAsync(CancellationToken ct = default)
    {
        try
        {
            var url = $"{_controlApiBaseUrl}/status";
            _logger.LogDebug("Fetching Ingest status from {Url}...", url);

            var response = await _httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Ingest control API returned status {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var status = JsonSerializer.Deserialize<IngestControlStatusResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return status;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("Failed to connect to Ingest control API ({Url}): {Message}", 
                $"{_controlApiBaseUrl}/status", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching Ingest status");
            return null;
        }
    }

    /// <summary>
    /// Verzendt een reload-settings opdracht naar de Ingest control API.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Het reload-response, of null als de API niet bereikbaar is.</returns>
    public async Task<IngestControlReloadResponse?> ReloadSettingsAsync(CancellationToken ct = default)
    {
        try
        {
            var url = $"{_controlApiBaseUrl}/reload-settings";
            _logger.LogInformation("Sending reload-settings request to {Url}...", url);

            var response = await _httpClient.PostAsync(url, null, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                _logger.LogWarning("Ingest reload-settings returned 503 Service Unavailable");
                return new IngestControlReloadResponse
                {
                    Success = false,
                    Message = "Ingest failed to fetch remote settings (Service Unavailable)"
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Ingest reload-settings returned status {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<IngestControlReloadResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("Failed to connect to Ingest control API ({Url}): {Message}", 
                $"{_controlApiBaseUrl}/reload-settings", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending reload-settings to Ingest");
            return null;
        }
    }
}

/// <summary>
/// Response DTO voor GET /status endpoint van Ingest control API.
/// </summary>
public class IngestControlStatusResponse
{
    public bool Running { get; set; }
    public string ApiBaseUrl { get; set; } = "";
    public string RawStorageMode { get; set; } = "";
    public int DefaultSampleIntervalSeconds { get; set; }
    public bool CaptureLoggingEnabled { get; set; }
    public string ListenAddress { get; set; } = "";
    public int ListenPort { get; set; }
    public bool ListenAddressRestartRequired { get; set; }
    public bool ListenPortRestartRequired { get; set; }
}

/// <summary>
/// Response DTO voor POST /reload-settings endpoint van Ingest control API.
/// </summary>
public class IngestControlReloadResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public List<string> AppliedFields { get; set; } = new();
    public List<string> RestartRequiredFields { get; set; } = new();
}
