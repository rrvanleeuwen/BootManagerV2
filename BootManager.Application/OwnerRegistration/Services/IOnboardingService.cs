using BootManager.Application.OwnerRegistration.DTOs;

namespace BootManager.Application.OwnerRegistration.Services;

/// <summary>
/// Service voor het voltooien van de initiale onboarding (eigenaar-, boot- en wachtwoordgegevens).
/// </summary>
public interface IOnboardingService
{
    /// <summary>
    /// Voltooit de initiale onboarding met eigenaar-, boot- en wachtwoordgegevens.
    /// Valideert alle input, werkt het eigenaarprofiel en bootprofiel bij, wijzigt het wachtwoord,
    /// en zet de setup-vlaggen op voltooid.
    /// </summary>
    /// <param name="request">De onboarding-gegevens.</param>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>Response DTO met success-status en eventuele foutmeldingen.</returns>
    /// <exception cref="ArgumentException">Als validatie faalt.</exception>
    /// <exception cref="UnauthorizedAccessException">Als het huidige wachtwoord onjuist is.</exception>
    /// <exception cref="InvalidOperationException">Als er geen eigenaar gevonden wordt.</exception>
    Task<CompleteOnboardingResponseDto> CompleteInitialOnboardingAsync(
        CompleteOnboardingRequestDto request,
        CancellationToken ct = default);
}
