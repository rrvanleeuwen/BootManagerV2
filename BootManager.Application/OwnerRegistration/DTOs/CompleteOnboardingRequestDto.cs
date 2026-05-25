namespace BootManager.Application.OwnerRegistration.DTOs;

/// <summary>
/// DTO voor het voltooien van de initiale onboarding met eigenaar-, boot- en wachtwoordgegevens.
/// </summary>
public sealed class CompleteOnboardingRequestDto
{
    /// <summary>
    /// Naam van de eigenaar (verplicht).
    /// </summary>
    public string OwnerName { get; set; } = string.Empty;

    /// <summary>
    /// E-mail van de eigenaar (optioneel).
    /// </summary>
    public string? OwnerEmail { get; set; }

    /// <summary>
    /// Naam van de boot (verplicht).
    /// </summary>
    public string VesselName { get; set; } = string.Empty;

    /// <summary>
    /// Thuishaven van de boot (optioneel).
    /// </summary>
    public string? HomePort { get; set; }

    /// <summary>
    /// Roepnaam van de boot (optioneel).
    /// </summary>
    public string? CallSign { get; set; }

    /// <summary>
    /// MMSI van de boot (optioneel).
    /// </summary>
    public string? Mmsi { get; set; }

    /// <summary>
    /// Huidig/bootstrap wachtwoord (verplicht voor verificatie).
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// Nieuw wachtwoord (verplicht).
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Bevestiging van het nieuwe wachtwoord (verplicht).
    /// </summary>
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
