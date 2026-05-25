using BootManager.Application.VesselProfile.DTOs;

namespace BootManager.Application.OwnerRegistration.DTOs;

/// <summary>
/// DTO voor het resultaat van de voltooide onboarding.
/// </summary>
public sealed class CompleteOnboardingResponseDto
{
    /// <summary>
    /// Geeft aan of de onboarding succesvol is voltooid.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Eventuele foutbericht als onboarding niet succesvol was.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Het bijgewerkte bootprofiel na succesvol onboarding.
    /// </summary>
    public VesselProfileDto? UpdatedVesselProfile { get; set; }

    /// <summary>
    /// Naam van de eigenaar na succesvol opslaan.
    /// </summary>
    public string? UpdatedOwnerName { get; set; }

    /// <summary>
    /// E-mail van de eigenaar na succesvol opslaan.
    /// </summary>
    public string? UpdatedOwnerEmail { get; set; }
}
