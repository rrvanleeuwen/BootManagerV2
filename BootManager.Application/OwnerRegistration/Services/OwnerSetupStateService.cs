using BootManager.Application.OwnerRegistration.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;

namespace BootManager.Application.OwnerRegistration.Services;

/// <summary>
/// Service dat de huidige setup-status van de eigenaar bepaalt.
/// </summary>
public class OwnerSetupStateService : IOwnerSetupStateService
{
    private readonly IRepository<OwnerProfile> _ownerRepository;

    public OwnerSetupStateService(IRepository<OwnerProfile> ownerRepository)
    {
        _ownerRepository = ownerRepository;
    }

    /// <summary>
    /// Haalt de huidige setup-status op.
    /// </summary>
    public async Task<OwnerSetupStateDto> GetSetupStateAsync(CancellationToken ct = default)
    {
        var owner = await _ownerRepository.SingleOrDefaultAsync(ct: ct);

        if (owner is null)
        {
            return new OwnerSetupStateDto
            {
                HasOwner = false,
                PasswordChangeRequired = false,
                OnboardingCompleted = false
            };
        }

        return new OwnerSetupStateDto
        {
            HasOwner = true,
            PasswordChangeRequired = owner.PasswordChangeRequired,
            OnboardingCompleted = owner.OnboardingCompleted
        };
    }
}
