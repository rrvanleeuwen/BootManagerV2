using BootManager.Application.OwnerRegistration.DTOs;

namespace BootManager.Application.OwnerRegistration.Services;

/// <summary>
/// Service dat de setup-status van gebruikers bepaalt.
/// </summary>
public interface IOwnerSetupStateService
{
    /// <summary>
    /// Haalt de setup-status op (parameterloze variant).
    /// </summary>
    Task<OwnerSetupStateDto> GetSetupStateAsync(CancellationToken ct = default);

    /// <summary>
    /// Haalt de setup-status op voor een specifieke gebruiker.
    /// </summary>
    Task<OwnerSetupStateDto> GetSetupStateAsync(Guid? userId, CancellationToken ct = default);
}
