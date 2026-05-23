using BootManager.Application.OperationalSettings.DTOs;
using BootManager.Application.OperationalSettings.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BootManager.Web.Controllers;

/// <summary>
/// API-controller voor het ophalen van operationele instellingen.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OperationalSettingsController : ControllerBase
{
    private readonly IOperationalSettingsService _operationalSettingsService;

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="OperationalSettingsController"/>.
    /// </summary>
    /// <param name="operationalSettingsService">De application-service voor operationele instellingen.</param>
    public OperationalSettingsController(IOperationalSettingsService operationalSettingsService)
    {
        _operationalSettingsService = operationalSettingsService;
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
            RawStorageMode = settings.RawStorageMode.ToString(),
            DefaultSampleIntervalSeconds = settings.DefaultSampleIntervalSeconds
        };

        return Ok(dto);
    }
}
