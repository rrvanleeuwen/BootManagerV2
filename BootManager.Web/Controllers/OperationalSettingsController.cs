using BootManager.Application.OperationalSettings.DTOs;
using BootManager.Application.OperationalSettings.Services;
using BootManager.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BootManager.Web.Controllers;

/// <summary>
/// API-controller voor het ophalen en bijwerken van operationele instellingen.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OperationalSettingsController : ControllerBase
{
    private readonly IOperationalSettingsService _operationalSettingsService;
    private readonly IOperationalSettingsWithReloadService _settingsWithReloadService;

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="OperationalSettingsController"/>.
    /// </summary>
    /// <param name="operationalSettingsService">De application-service voor operationele instellingen.</param>
    /// <param name="settingsWithReloadService">Service voor opslaan met Ingest reload.</param>
    public OperationalSettingsController(
        IOperationalSettingsService operationalSettingsService,
        IOperationalSettingsWithReloadService settingsWithReloadService)
    {
        _operationalSettingsService = operationalSettingsService;
        _settingsWithReloadService = settingsWithReloadService;
    }

    /// <summary>
    /// Geeft de operationele instellingen terug die BootManager.Tools.Ingest bij startup nodig heeft.
    /// </summary>
    /// <remarks>
    /// TODO: Dit endpoint is voorlopig anoniem bereikbaar voor interne tool-koppeling.
    /// Beveiliging moet in een volgende iteratie worden aangescherpt (bijv. API key of lokale netwerk-restrictie).
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>De ingest-instellingen vanuit de database.</returns>
    [HttpGet("ingest")]
    [AllowAnonymous]
    public async Task<ActionResult<IngestSettingsDto>> GetIngestSettings(CancellationToken ct)
    {
        var settings = await _operationalSettingsService.GetAsync(ct);

        var dto = new IngestSettingsDto
        {
            ListenAddress = settings.ListenAddress,
            ListenPort = settings.ListenPort,
            ApiBaseUrl = settings.ApiBaseUrl,
            CaptureLoggingEnabled = settings.CaptureLoggingEnabled,
            IngestProcessingEnabled = settings.IngestProcessingEnabled,
            RawStorageMode = settings.RawStorageMode.ToString(),
            DefaultSampleIntervalSeconds = settings.DefaultSampleIntervalSeconds
        };

        return Ok(dto);
    }

    /// <summary>
    /// Geeft alle operationele instellingen terug.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Alle operationele instellingen.</returns>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<OperationalSettingsDto>> GetSettings(CancellationToken ct)
    {
        var settings = await _operationalSettingsService.GetAsync(ct);
        return Ok(settings);
    }

    /// <summary>
    /// Slaat operationele instellingen op en verzendt een reload-commando naar Ingest.
    /// </summary>
    /// <param name="dto">De nieuwe instellingen.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Status van de save-operatie en Ingest reload.</returns>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<SaveOperationalSettingsResponse>> SaveSettings(
        OperationalSettingsDto dto,
        CancellationToken ct)
    {
        try
        {
            var response = await _settingsWithReloadService.SaveAndReloadAsync(dto, ct);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Fout bij opslaan instellingen.", details = ex.Message });
        }
    }
}
