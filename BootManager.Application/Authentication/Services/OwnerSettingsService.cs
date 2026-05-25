using BootManager.Application.Authentication.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;
using System.Text.Json;

namespace BootManager.Application.Authentication.Services;

public class OwnerSettingsService : IOwnerSettingsService
{
    private readonly IRepository<OwnerProfile> _repo;
    private readonly IPasswordHasher _hasher;
    private readonly ISystemClock _clock;
    private readonly IEncryptionService _encryption;

    public OwnerSettingsService(
        IRepository<OwnerProfile> repo,
        IPasswordHasher hasher,
        ISystemClock clock,
        IEncryptionService encryption)
    {
        _repo = repo;
        _hasher = hasher;
        _clock = clock;
        _encryption = encryption;
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

    public async Task<GetOwnerProfileResponseDto> GetOwnerProfileAsync(CancellationToken ct = default)
    {
        var owner = await _repo.SingleOrDefaultAsync(ct: ct);
        if (owner is null) throw new InvalidOperationException("Geen eigenaarprofiel gevonden.");

        // Decrypt the encrypted payload
        var json = _encryption.Decrypt(owner.EncryptedProfilePayload);
        var payload = JsonSerializer.Deserialize<JsonElement>(json);

        var name = payload.TryGetProperty("Name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty;
        var email = payload.TryGetProperty("Email", out var emailProp) ? emailProp.GetString() ?? string.Empty : string.Empty;

        return new GetOwnerProfileResponseDto
        {
            Name = name,
            Email = email
        };
    }

    public async Task UpdateOwnerProfileAsync(UpdateOwnerProfileRequestDto request, CancellationToken ct = default)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Naam is verplicht.");
        if (request.Name.Length > 100)
            throw new ArgumentException("Naam mag niet langer dan 100 tekens zijn.");
        if (!string.IsNullOrEmpty(request.Email) && request.Email.Length > 200)
            throw new ArgumentException("E-mail mag niet langer dan 200 tekens zijn.");

        // Validate email format if provided
        if (!string.IsNullOrEmpty(request.Email))
        {
            if (!request.Email.Contains("@") || request.Email.Count(c => c == '@') != 1)
                throw new ArgumentException("Ongeldig e-mailadres.");
        }

        var owner = await _repo.SingleOrDefaultAsync(ct: ct);
        if (owner is null) throw new InvalidOperationException("Geen eigenaarprofiel gevonden.");

        // Decrypt current payload to preserve structure
        var json = _encryption.Decrypt(owner.EncryptedProfilePayload);
        var payload = JsonSerializer.Deserialize<JsonElement>(json);

        // Create new payload with updated name/email (preserving any other fields)
        var updatedPayload = new
        {
            Name = request.Name.Trim(),
            Email = string.IsNullOrEmpty(request.Email) ? string.Empty : request.Email.Trim().ToLowerInvariant()
        };

        var newJson = JsonSerializer.Serialize(updatedPayload);
        var encryptedPayload = _encryption.Encrypt(newJson);

        // Update the payload only, keep password/flags intact
        owner.ReplaceEncryptedPayload(encryptedPayload, owner.EncryptionVersion, _clock.UtcNow);
        await _repo.UpdateAsync(owner, ct);
    }
}
