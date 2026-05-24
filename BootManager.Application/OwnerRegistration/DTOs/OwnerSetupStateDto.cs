namespace BootManager.Application.OwnerRegistration.DTOs;

/// <summary>
/// DTO dat de huidige setup-status van de eigenaar beschrijft.
/// </summary>
public sealed class OwnerSetupStateDto
{
    /// <summary>
    /// Geeft aan of een eigenaar in de database bestaat.
    /// </summary>
    public bool HasOwner { get; init; }

    /// <summary>
    /// Geeft aan dat het wachtwoord moet worden gewijzigd (bv. bij bootstrap).
    /// Alleen relevant als HasOwner=true.
    /// </summary>
    public bool PasswordChangeRequired { get; init; }

    /// <summary>
    /// Geeft aan dat de onboarding-flow is voltooid.
    /// Alleen relevant als HasOwner=true.
    /// </summary>
    public bool OnboardingCompleted { get; init; }

    /// <summary>
    /// Samenvattende vlag: true als setup verplicht is.
    /// Setup is verplicht als PasswordChangeRequired=true of OnboardingCompleted=false.
    /// </summary>
    public bool SetupRequired => !HasOwner || PasswordChangeRequired || !OnboardingCompleted;
}
