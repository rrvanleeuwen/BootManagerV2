using BootManager.Application.Authentication.DTOs;

namespace BootManager.Application.Authentication.Services;

/// <summary>
/// Beheerst instellingen van de eigenaar zoals wachtwoord en pincode wijzigingen.
/// </summary>
public interface IOwnerSettingsService
{
    /// <summary>
    /// Wijzigt het wachtwoord van de eigenaar.
    /// </summary>
    /// <param name="request">ChangePasswordRequestDto met huidig wachtwoord en nieuw wachtwoord.</param>
    /// <param name="ct">Annuleringstoken.</param>
    /// <exception cref="ArgumentException">Als het huidige wachtwoord incorrect is of validatie mislukt.</exception>
    Task ChangePasswordAsync(ChangePasswordRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Stelt een pincode voor de eigenaar in.
    /// </summary>
    /// <param name="request">ChangePinRequestDto met pincode.</param>
    /// <param name="ct">Annuleringstoken.</param>
    /// <exception cref="ArgumentException">Als de pincode ongeldig is.</exception>
    Task SetPinAsync(ChangePinRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Verwijdert de pincode van de eigenaar.
    /// </summary>
    /// <param name="ct">Annuleringstoken.</param>
    Task ClearPinAsync(CancellationToken ct = default);
}
