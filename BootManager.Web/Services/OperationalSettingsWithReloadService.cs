using BootManager.Application.OperationalSettings.DTOs;
using BootManager.Application.OperationalSettings.Services;
using BootManager.Web.Controllers;
using Microsoft.Extensions.Logging;

namespace BootManager.Web.Services;

/// <summary>
/// Service voor het opslaan van operationele instellingen met Ingest reload-triggering.
/// Zowel de Web API controller als Blazor UI kunnen deze service gebruiken.
/// </summary>
public interface IOperationalSettingsWithReloadService
{
    /// <summary>
    /// Haalt operationele instellingen op.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>De huidige operationele instellingen.</returns>
    Task<OperationalSettingsDto> GetOperationalSettingsAsync(CancellationToken ct = default);

    /// <summary>
    /// Slaat operationele instellingen op en triggert Ingest reload.
    /// </summary>
    /// <param name="dto">De te bewaren instellingen.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Response met save-status en reload-resultaat.</returns>
    Task<SaveOperationalSettingsResponse> SaveAndReloadAsync(
        OperationalSettingsDto dto,
        CancellationToken ct = default);
}

/// <summary>
/// Implementatie van IOperationalSettingsWithReloadService.
/// </summary>
public class OperationalSettingsWithReloadService : IOperationalSettingsWithReloadService
{
    private readonly IOperationalSettingsService _operationalSettingsService;
    private readonly IIngestControlClient _ingestControlClient;
    private readonly ILogger<OperationalSettingsWithReloadService> _logger;

    public OperationalSettingsWithReloadService(
        IOperationalSettingsService operationalSettingsService,
        IIngestControlClient ingestControlClient,
        ILogger<OperationalSettingsWithReloadService> logger)
    {
        _operationalSettingsService = operationalSettingsService;
        _ingestControlClient = ingestControlClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<OperationalSettingsDto> GetOperationalSettingsAsync(CancellationToken ct = default)
    {
        return await _operationalSettingsService.GetAsync(ct);
    }

    /// <inheritdoc />
    public async Task<SaveOperationalSettingsResponse> SaveAndReloadAsync(
        OperationalSettingsDto dto,
        CancellationToken ct = default)
    {
        try
        {
            // Opslaan in database
            await _operationalSettingsService.SaveAsync(dto, ct);

            var response = new SaveOperationalSettingsResponse
            {
                SettingsSaved = true,
                SaveMessage = "Instellingen opgeslagen."
            };

            _logger.LogInformation("Settings saved to database. Attempting to reload Ingest...");

            // Probeer Ingest te instrueren om settings opnieuw in te laden
            var reloadResult = await _ingestControlClient.ReloadSettingsAsync(ct);

            if (reloadResult == null)
            {
                // Ingest is niet bereikbaar
                _logger.LogWarning("Ingest control API not reachable");
                response.IngestReloadMessage = "Ingest is niet bereikbaar. Herstart Ingest/Raspberry Pi handmatig.";
                response.IngestReloadStatus = "unreachable";
            }
            else if (reloadResult.Success)
            {
                // Reload succesvol
                _logger.LogInformation(
                    "Ingest settings reloaded successfully. Applied: {Applied}. Restart required: {RestartRequired}",
                    string.Join(", ", reloadResult.AppliedFields),
                    string.Join(", ", reloadResult.RestartRequiredFields));

                response.IngestReloadMessage = "Ingest-instellingen opnieuw geladen.";
                response.IngestReloadStatus = "success";
                response.AppliedFields = reloadResult.AppliedFields;
                response.RestartRequiredFields = reloadResult.RestartRequiredFields;

                // Detecteer wijzigingen in listener-instellingen
                if (reloadResult.RestartRequiredFields.Contains("ListenAddress") || 
                    reloadResult.RestartRequiredFields.Contains("ListenPort"))
                {
                    response.IngestReloadMessage += " Let op: UDP-luisterinstellingen zijn gewijzigd. Herstart Ingest/Raspberry Pi om de wijzigingen actief te maken.";
                }
            }
            else
            {
                // Reload mislukt, maar settings zijn opgeslagen
                _logger.LogWarning(
                    "Ingest reload failed: {Message}",
                    reloadResult.Message);

                response.IngestReloadMessage = $"Ingest kon instellingen niet opnieuw laden: {reloadResult.Message} Herstart Ingest/Raspberry Pi handmatig.";
                response.IngestReloadStatus = "failed";
            }

            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error saving settings: {Message}", ex.Message);
            throw; // Let caller handle validation errors
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in SaveAndReloadAsync: {Message}", ex.Message);
            throw;
        }
    }
}
