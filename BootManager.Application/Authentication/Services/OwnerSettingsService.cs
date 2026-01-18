using BootManager.Application.Authentication.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;

namespace BootManager.Application.Authentication.Services;

public class OwnerSettingsService : IOwnerSettingsService
{
    private readonly IRepository<OwnerProfile> _repo;
    private readonly IPasswordHasher _hasher;
    private readonly ISystemClock _clock;

    public OwnerSettingsService(IRepository<OwnerProfile> repo, IPasswordHasher hasher, ISystemClock clock)
    {
        _repo = repo;
        _hasher = hasher;
        _clock = clock;
    }

    public async Task ChangePasswordAsync(ChangePasswordRequestDto request, CancellationToken ct = default)
    {
        // ChangePassword called
        if (request.NewPassword != request.ConfirmNewPassword)
            throw new ArgumentException("New passwords do not match");
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters");

        var owner = await _repo.SingleOrDefaultAsync(ct: ct);
        if (owner is null) throw new InvalidOperationException("No owner profile found");

        // verify current credential: allow either password or pin to authenticate change
        var currentOk = false;
        if (!string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            var storedPwd = new HashResult(owner.PasswordHash, owner.PasswordSalt, owner.HashAlgorithm);
            currentOk = _hasher.Verify(request.CurrentPassword, storedPwd);
            if (!currentOk && !string.IsNullOrEmpty(owner.PinHash) && !string.IsNullOrEmpty(owner.PinSalt))
            {
                var storedPin = new HashResult(owner.PinHash!, owner.PinSalt!, owner.HashAlgorithm);
                currentOk = _hasher.Verify(request.CurrentPassword, storedPin);
            }
        }

        if (!currentOk)
        {
            throw new UnauthorizedAccessException("Current credential invalid");
        }

        var newHash = _hasher.Hash(request.NewPassword);
        owner.UpdatePassword(newHash.Hash, newHash.Salt, newHash.Algorithm, _clock.UtcNow);
        await _repo.UpdateAsync(owner, ct);
    }

    public async Task SetPinAsync(ChangePinRequestDto request, CancellationToken ct = default)
    {
        if (request.NewPin != request.ConfirmNewPin)
            throw new ArgumentException("Pins do not match");
        if (request.NewPin.Length < 4) throw new ArgumentException("Pin must be at least 4 digits");

        var owner = await _repo.SingleOrDefaultAsync(ct: ct);
        if (owner is null) throw new InvalidOperationException("No owner profile found");

        // authenticate with current password or existing pin
        var authOk = false;
        if (!string.IsNullOrWhiteSpace(request.CurrentPasswordOrPin))
        {
            var storedPwd = new HashResult(owner.PasswordHash, owner.PasswordSalt, owner.HashAlgorithm);
            authOk = _hasher.Verify(request.CurrentPasswordOrPin, storedPwd);
            if (!authOk && !string.IsNullOrEmpty(owner.PinHash) && !string.IsNullOrEmpty(owner.PinSalt))
            {
                var storedPin = new HashResult(owner.PinHash!, owner.PinSalt!, owner.HashAlgorithm);
                authOk = _hasher.Verify(request.CurrentPasswordOrPin, storedPin);
            }
        }

        if (!authOk) throw new UnauthorizedAccessException("Current credential invalid");

        var pinHash = _hasher.Hash(request.NewPin);
        owner.SetPin(pinHash.Hash, pinHash.Salt, _clock.UtcNow);
        await _repo.UpdateAsync(owner, ct);
    }

    public async Task ClearPinAsync(CancellationToken ct = default)
    {
        var owner = await _repo.SingleOrDefaultAsync(ct: ct);
        if (owner is null) throw new InvalidOperationException("No owner profile found");
        owner.ClearPin(_clock.UtcNow);
        await _repo.UpdateAsync(owner, ct);
    }
}
