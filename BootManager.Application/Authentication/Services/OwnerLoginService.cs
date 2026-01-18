using BootManager.Application.Authentication.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;

namespace BootManager.Application.Authentication.Services;

/// <summary>
/// Implementeert credentialvalidatie voor de eigenaar op basis van opgeslagen hashes.
/// </summary>
public sealed class OwnerLoginService : IOwnerLoginService
{
    private readonly IRepository<OwnerProfile> _repo;
    private readonly IPasswordHasher _hasher;

    public OwnerLoginService(IRepository<OwnerProfile> repo, IPasswordHasher hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public async Task<LoginResultDto> ValidateAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        var owner = await _repo.SingleOrDefaultAsync(ct: ct);
        if (owner is null)
        {
            return new LoginResultDto { Success = false, Message = "Geen eigenaarprofiel gevonden." };
        }

        // Eerst wachtwoord, anders pincode
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var stored = new HashResult(owner.PasswordHash, owner.PasswordSalt, owner.HashAlgorithm);
            var ok = _hasher.Verify(request.Password, stored);
            return ok
                ? new LoginResultDto { Success = true, OwnerId = owner.Id }
                : new LoginResultDto { Success = false, Message = "Ongeldig wachtwoord." };
        }

        if (!string.IsNullOrWhiteSpace(request.Pin))
        {
            if (string.IsNullOrEmpty(owner.PinHash) || string.IsNullOrEmpty(owner.PinSalt))
            {
                return new LoginResultDto { Success = false, Message = "Er is geen pincode ingesteld." };
            }

            var stored = new HashResult(owner.PinHash, owner.PinSalt, owner.HashAlgorithm);
            var ok = _hasher.Verify(request.Pin, stored);
            return ok
                ? new LoginResultDto { Success = true, OwnerId = owner.Id }
                : new LoginResultDto { Success = false, Message = "Ongeldige pincode." };
        }

        return new LoginResultDto { Success = false, Message = "Geen wachtwoord of pincode opgegeven." };
    }
}