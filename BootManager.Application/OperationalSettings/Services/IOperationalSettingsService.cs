using BootManager.Application.OperationalSettings.DTOs;

namespace BootManager.Application.OperationalSettings.Services;

/// <summary>
/// Service-interface voor het beheren van operationele instellingen.
/// </summary>
public interface IOperationalSettingsService
{
    /// <summary>
    /// Haalt de huidige operationele instellingen op. Maakt standaardinstellingen aan als er nog geen record bestaat.
    /// </summary>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>Het huidige settings-DTO.</returns>
    Task<OperationalSettingsDto> GetAsync(CancellationToken ct = default);

    /// <summary>
    /// Slaat gewijzigde operationele instellingen op.
    /// </summary>
    /// <param name="dto">De nieuwe instellingen.</param>
    /// <param name="ct">Annuleringstoken.</param>
    Task SaveAsync(OperationalSettingsDto dto, CancellationToken ct = default);
}
