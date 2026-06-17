using BootManager.Application.OwnerRegistration.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Core.Interfaces;

namespace BootManager.Application.OwnerRegistration.Services;

/// <summary>
/// Service dat de huidge setup-status van een gebruiker bepaalt.
/// </summary>
public class OwnerSetupStateService : IOwnerSetupStateService
{
    private readonly IRepository<LocalUser> _userRepository;

    public OwnerSetupStateService(IRepository<LocalUser> userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Haalt de huidge setup-status op (parameterloze variant).
    /// </summary>
    public async Task<OwnerSetupStateDto> GetSetupStateAsync(CancellationToken ct = default)
    {
        // Default: no user
        return await GetSetupStateAsync(null, ct);
    }

    /// <summary>
    /// Haalt de huidge setup-status op voor een specifieke gebruiker ID.
    /// </summary>
    public async Task<OwnerSetupStateDto> GetSetupStateAsync(Guid? userId, CancellationToken ct = default)
    {
        if (!userId.HasValue)
        {
            return new OwnerSetupStateDto
            {
                HasOwner = false,
                PasswordChangeRequired = false,
                OnboardingCompleted = false
            };
        }

        var user = await _userRepository.SingleOrDefaultAsync(u => u.Id == userId.Value, ct);
        if (user is null)
        {
            return new OwnerSetupStateDto
            {
                HasOwner = false,
                PasswordChangeRequired = false,
                OnboardingCompleted = false
            };
        }

        // Owner moet onboarding voltooien; Crew mag direct aan de slag
        var passwordChangeRequired = user.PasswordChangeRequired;
        var onboardingCompleted = user.OnboardingCompleted;

        return new OwnerSetupStateDto
        {
            HasOwner = true,
            PasswordChangeRequired = passwordChangeRequired,
            OnboardingCompleted = onboardingCompleted
        };
    }
}
