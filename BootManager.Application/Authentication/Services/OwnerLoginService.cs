using BootManager.Application.Authentication.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;

namespace BootManager.Application.Authentication.Services;

/// <summary>
/// Valideert lokale gebruiker-credentials (wachtwoord of pincode) tegen actieve LocalUser-records.
/// </summary>
public sealed class OwnerLoginService : IOwnerLoginService
{
    private readonly IRepository<LocalUser> _repo;
    private readonly IPasswordHasher _hasher;

    public OwnerLoginService(IRepository<LocalUser> repo, IPasswordHasher hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public async Task<LoginResultDto> ValidateAsync(LoginRequestDto request, CancellationToken ct = default)
    {
        if (!request.UserId.HasValue)
        {
            return new LoginResultDto { Success = false, Message = "Geen gebruiker geselecteerd." };
        }

        var user = await _repo.SingleOrDefaultAsync(u => u.Id == request.UserId.Value, ct);
        if (user is null)
        {
            return new LoginResultDto { Success = false, Message = "Gebruiker niet gevonden." };
        }

        if (!user.IsActive)
        {
            return new LoginResultDto { Success = false, Message = "Dit account is uitgeschakeld." };
        }

        // Eerst wachtwoord, anders pincode
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var stored = new HashResult(user.PasswordHash, user.PasswordSalt, user.HashAlgorithm);
            var ok = _hasher.Verify(request.Password, stored);
            return ok
                ? new LoginResultDto
                {
                    Success = true,
                    UserId = user.Id,
                    DisplayName = user.DisplayName,
                    Role = user.Role,
                    CredentialVersion = user.CredentialVersion,
                    PasswordChangeRequired = user.PasswordChangeRequired
                }
                : new LoginResultDto { Success = false, Message = "Ongeldig wachtwoord." };
        }

        if (!string.IsNullOrWhiteSpace(request.Pin))
        {
            if (string.IsNullOrEmpty(user.PinHash) || string.IsNullOrEmpty(user.PinSalt))
            {
                return new LoginResultDto { Success = false, Message = "Er is geen pincode ingesteld." };
            }

            var stored = new HashResult(user.PinHash, user.PinSalt, user.HashAlgorithm);
            var ok = _hasher.Verify(request.Pin, stored);
            return ok
                ? new LoginResultDto
                {
                    Success = true,
                    UserId = user.Id,
                    DisplayName = user.DisplayName,
                    Role = user.Role,
                    CredentialVersion = user.CredentialVersion,
                    PasswordChangeRequired = user.PasswordChangeRequired
                }
                : new LoginResultDto { Success = false, Message = "Ongeldige pincode." };
        }

        return new LoginResultDto { Success = false, Message = "Geen wachtwoord of pincode opgegeven." };
    }
}