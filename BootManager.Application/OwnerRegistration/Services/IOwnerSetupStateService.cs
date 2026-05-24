using BootManager.Application.OwnerRegistration.DTOs;

namespace BootManager.Application.OwnerRegistration.Services;

/// <summary>
/// Service dat de huidige setup-status van de eigenaar bepaalt.
/// </summary>
public interface IOwnerSetupStateService
{
    /// <summary>
    /// Haalt de huidige setup-status op.
    /// </summary>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>OwnerSetupStateDto met de huidige setup-status.</returns>
    Task<OwnerSetupStateDto> GetSetupStateAsync(CancellationToken ct = default);
}
