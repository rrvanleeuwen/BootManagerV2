using BootManager.Application.NetworkMessages.DTOs;
using BootManager.Application.NetworkMessages.Services;
using Microsoft.AspNetCore.Mvc;

namespace BootManager.Web.Controllers;

/// <summary>
/// API-controller voor NetworkMessage-operaties.
/// Biedt endpoints voor het opslaan en ophalen van netwerkberichten.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NetworkMessagesController : ControllerBase
{
    private readonly INetworkMessageService _networkMessageService;

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="NetworkMessagesController"/>.
    /// </summary>
    /// <param name="networkMessageService">De application-service voor NetworkMessage-operaties.</param>
    public NetworkMessagesController(INetworkMessageService networkMessageService)
    {
        _networkMessageService = networkMessageService;
    }

    /// <summary>
    /// Slaat een nieuw netwerkbericht op.
    /// </summary>
    /// <param name="request">De gegevens van het netwerkbericht om op te slaan.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Het ID van het nieuw aangemaakte bericht.</returns>
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateNetworkMessageRequestDto request, CancellationToken ct)
    {
        var id = await _networkMessageService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Create), new { id }, id);
    }

    /// <summary>
    /// Haalt de meest recente netwerkberichten op, begrensd door een optionele limiet.
    /// </summary>
    /// <param name="limit">Het maximale aantal berichten om op te halen. Standaard 50.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Een lijst van de meest recente netwerkberichten.</returns>
    [HttpGet("latest")]
    public async Task<ActionResult<IReadOnlyList<NetworkMessageDto>>> GetLatest([FromQuery] int limit = 50, CancellationToken ct = default)
    {
        // Validatie: limit mag niet negatief of onredelijk groot zijn
        if (limit < 1 || limit > 1000)
            limit = 50;

        var messages = await _networkMessageService.GetLatestAsync(limit, ct);
        return Ok(messages);
    }
}