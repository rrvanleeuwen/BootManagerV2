using BootManager.Application.Authentication.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;

namespace BootManager.Application.Authentication.Services;

/// <summary>
/// Service voor accountbewerking: wachtwoordwijziging.
/// </summary>
public sealed class AccountService : IAccountService
{
    private readonly IRepository<LocalUser> _repo;
    private readonly IPasswordHasher _hasher;

    public AccountService(IRepository<LocalUser> repo, IPasswordHasher hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public async Task<ChangePasswordResultDto> ChangePasswordAsync(Guid userId, ChangePasswordDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            return new ChangePasswordResultDto { Success = false, Message = "Wachtwoord moet minimaal 8 tekens zijn." };
        }

        if (!string.Equals(request.NewPassword, request.ConfirmNewPassword, StringComparison.Ordinal))
        {
            return new ChangePasswordResultDto { Success = false, Message = "Wachtwoorden komen niet overeen." };
        }

        var user = await _repo.SingleOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null)
        {
            return new ChangePasswordResultDto { Success = false, Message = "Gebruiker niet gevonden." };
        }

        // Verify current password
        var currentStored = new HashResult(user.PasswordHash, user.PasswordSalt, user.HashAlgorithm);
        if (!_hasher.Verify(request.CurrentPassword, currentStored))
        {
            return new ChangePasswordResultDto { Success = false, Message = "Huidig wachtwoord is onjuist." };
        }

        // New password moet verschillen van huidig
        if (string.Equals(request.NewPassword, request.CurrentPassword, StringComparison.Ordinal))
        {
            return new ChangePasswordResultDto { Success = false, Message = "Nieuw wachtwoord moet verschillen van het huidige wachtwoord." };
        }

        // Hash new password
        var (hash, salt, algo) = _hasher.Hash(request.NewPassword);
        var now = DateTime.UtcNow;

        user.UpdatePassword(hash, salt, algo, now);
        if (user.PasswordChangeRequired)
        {
            user.SetPasswordChangeRequired(false, now);
        }

        await _repo.UpdateAsync(user, ct);

        return new ChangePasswordResultDto
        {
            Success = true,
            Message = "Wachtwoord succesvol gewijzigd.",
            NewCredentialVersion = user.CredentialVersion,
            DisplayName = user.DisplayName,
            Role = user.Role
        };
    }
}
