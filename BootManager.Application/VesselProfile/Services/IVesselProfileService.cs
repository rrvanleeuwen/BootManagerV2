using BootManager.Application.VesselProfile.DTOs;

namespace BootManager.Application.VesselProfile.Services;

/// <summary>
/// Service voor het beheer van het bootprofiel (singleton per installatie).
/// </summary>
public interface IVesselProfileService
{
    /// <summary>
    /// Haalt het huidige bootprofiel op, of maakt een lege profiel aan als er nog geen bestaat.
    /// </summary>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>DTO van het bootprofiel.</returns>
    Task<VesselProfileDto> GetOrCreateVesselProfileAsync(CancellationToken ct = default);

    /// <summary>
    /// Werkt het bestaande bootprofiel bij met nieuwe gegevens.
    /// </summary>
    /// <param name="request">De bijgewerkte bootgegevens.</param>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>DTO van het bijgewerkte bootprofiel.</returns>
    /// <exception cref="ArgumentException">Als VesselName leeg of null is, of als velden langer zijn dan toegestaan.</exception>
    Task<VesselProfileDto> UpdateVesselProfileAsync(UpdateVesselProfileRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Voortschrijving van actuele motorurenstand en logstandwaarde op basis van reis-tellerwaarden.
    /// Verhoogt alleen als de reis-kandidaat hoger is dan de huidige profiel-waarde.
    /// Null, negatief, 0 of lager verhoogt geen bestaande waarde.
    /// Historische reizen worden niet opnieuw gescand.
    /// </summary>
    /// <param name="engineHoursCandidates">Array van kandidaatwaarden voor motoruren (EngineHoursStart, EngineHoursEnd).</param>
    /// <param name="logstandCandidates">Array van kandidaatwaarden voor logstand (LogstandStart, LogstandEnd).</param>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>Het bijgewerkte bootprofiel DTO.</returns>
    Task<VesselProfileDto> AdvanceCurrentMetersAsync(decimal?[] engineHoursCandidates, decimal?[] logstandCandidates, CancellationToken ct = default);
}
