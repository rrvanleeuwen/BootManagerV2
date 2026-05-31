using BootManager.Application.Administration.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BootManager.Web.Controllers;

/// <summary>
/// API-controller voor systeemacties (shutdown, etc.).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Owner")]
public class SystemController : ControllerBase
{
    private readonly IShutdownService _shutdownService;
    private readonly ILogger<SystemController> _logger;

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="SystemController"/>.
    /// </summary>
    /// <param name="shutdownService">Service voor shutdown-acties.</param>
    /// <param name="logger">Logger voor diagnostische informatie.</param>
    public SystemController(IShutdownService shutdownService, ILogger<SystemController> logger)
    {
        _shutdownService = shutdownService;
        _logger = logger;
    }

    /// <summary>
    /// Initieert een veilige shutdown van de BootManager Pi.
    /// </summary>
    /// <remarks>
    /// Dit endpoint mag alleen door geautoriseerde Owner-gebruikers worden aangeroepen.
    /// Na bevestigen toont de UI een 20-secondenwarschuwing voordat de Pi werkelijk afsluit.
    /// </remarks>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Status van de shutdown-initialisering.</returns>
    [HttpPost("shutdown")]
    public async Task<ActionResult<object>> Shutdown(CancellationToken ct)
    {
        var userId = User?.FindFirst("sub")?.Value ?? User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userName = User?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown";

        _logger.LogInformation("Shutdown initiated by user: {UserId} ({UserName})", userId, userName);

        try
        {
            await _shutdownService.InitiateShutdownAsync(ct);

            return Ok(new
            {
                status = "initiated",
                message = "De BootManager Pi wordt afgesloten. Wacht 20 seconden voordat je de BootManager Pi uitzet."
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Shutdown helper not available: {Message}", ex.Message);
            return StatusCode(503, new { error = "Shutdown-helper niet beschikbaar. Controleer Pi-configuratie." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during shutdown: {Message}", ex.Message);
            return StatusCode(500, new { error = "Onverwachte fout bij shutdown." });
        }
    }
}
