using BootManager.Application.OwnerRegistration.DTOs;

namespace BootManager.Application.OwnerRegistration.Services;

/// <summary>
/// Beheerst de eerste registratie van de applicatie-eigenaar.
/// </summary>
public interface IOwnerRegistrationService
{
    /// <summary>
    /// Controleert of de applicatie voor het eerst wordt opgestart (geen eigenaar geregistreerd).
    /// </summary>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>IsFirstRunResultDto met firstRun=true als geen eigenaar bestaat, false anders.</returns>
    Task<IsFirstRunResultDto> CheckFirstRunAsync(CancellationToken ct = default);

    /// <summary>
    /// Registreert de eerste eigenaar van de applicatie.
    /// 
    /// Deze methode mag alleen eenmaal worden aangeroepen bij de eerste opstart.
    /// </summary>
    /// <param name="request">Registratiegegevens met naam, wachtwoord en optioneel pincode.</param>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>RegisterOwnerResponseDto met successtatus, backup-code en master-key.</returns>
    Task<RegisterOwnerResponseDto> RegisterFirstOwnerAsync(RegisterOwnerRequestDto request, CancellationToken ct = default);
}