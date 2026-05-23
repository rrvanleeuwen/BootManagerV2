using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BootManager.Tools.Ingest.Services;

/// <summary>
/// Client-service die bij startup operationele instellingen ophaalt bij BootManager.Web.
/// </summary>
public class OperationalSettingsClientService : IOperationalSettingsClientService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OperationalSettingsClientService> _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="OperationalSettingsClientService"/>.
    /// </summary>
    /// <param name="httpClient">De HTTP-client voor API-aanroepen.</param>
    /// <param name="logger">De logger.</param>
    public OperationalSettingsClientService(HttpClient httpClient, ILogger<OperationalSettingsClientService> logger)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(5);
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IngestRemoteSettings?> TryGetSettingsAsync(string baseUrl, CancellationToken ct = default)
    {
        var url = $"{baseUrl.TrimEnd('/')}/api/operationalsettings/ingest";

        try
        {
            _logger.LogInformation("Ophalen operationele instellingen via {Url}...", url);

            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var settings = JsonSerializer.Deserialize<IngestRemoteSettings>(json, _jsonOptions);

            if (settings is null)
            {
                _logger.LogWarning("Operationele instellingen ontvangen maar konden niet worden gedeserialiseerd.");
                return null;
            }

            _logger.LogInformation(
                "Operationele instellingen opgehaald via BootManager.Web: ListenAddress={ListenAddress}, ListenPort={ListenPort}, ApiBaseUrl={ApiBaseUrl}, CaptureLoggingEnabled={CaptureLoggingEnabled}, RawStorageMode={RawStorageMode} (nog niet toegepast), DefaultSampleIntervalSeconds={DefaultSampleIntervalSeconds} (nog niet toegepast).",
                settings.ListenAddress, settings.ListenPort, settings.ApiBaseUrl,
                settings.CaptureLoggingEnabled, settings.RawStorageMode, settings.DefaultSampleIntervalSeconds);

            return settings;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            _logger.LogWarning(
                "Kon operationele instellingen niet ophalen bij BootManager.Web ({Url}): {Message}. Ingest gebruikt appsettings als fallback.",
                url, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "Onverwachte fout bij ophalen operationele instellingen ({Url}): {Message}. Ingest gebruikt appsettings als fallback.",
                url, ex.Message);
            return null;
        }
    }
}
