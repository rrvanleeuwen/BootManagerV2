using BootManager.Application.Authentication.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;
using System.Text.Json;

namespace BootManager.Application.Authentication.Services;

/// <summary>
/// Service für Owner-Profileinstellungen: Verschlüsselte Nutzlast (Name, Email) und Legacy PIN-Verwaltung.
/// Wird von der Settings-UI aufgerufen, um das Profil des aktuellen Owner zu verwalten.
/// </summary>
public class OwnerSettingsService : IOwnerSettingsService
{
    private readonly IRepository<LocalUser> _repo;
    private readonly IPasswordHasher _hasher;
    private readonly ISystemClock _clock;
    private readonly IEncryptionService _encryption;

    public OwnerSettingsService(
        IRepository<LocalUser> repo,
        IPasswordHasher hasher,
        ISystemClock clock,
        IEncryptionService encryption)
    {
        _repo = repo;
        _hasher = hasher;
        _clock = clock;
        _encryption = encryption;
    }

    /// <summary>
    /// Ändert das Passwort des aktuellen Benutzers.
    /// </summary>
    public async Task ChangePasswordAsync(ChangePasswordRequestDto request, CancellationToken ct = default)
    {
        // Dies ist Legacy - verwenden Sie stattdessen AccountService für neue Passwortänderungen.
        // Halten Sie dies nur aus Gründen der Kompatibilität.
        if (request.NewPassword != request.ConfirmNewPassword)
            throw new ArgumentException("New passwords do not match");
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters");

        // Für Legacy: Hole den ersten Owner
        var owner = await _repo.SingleOrDefaultAsync(u => u.Role == Core.Enums.LocalUserRole.Owner, ct);
        if (owner is null) throw new InvalidOperationException("No owner profile found");

        var storedPwd = new HashResult(owner.PasswordHash, owner.PasswordSalt, owner.HashAlgorithm);
        if (!_hasher.Verify(request.CurrentPassword, storedPwd))
            throw new UnauthorizedAccessException("Current credential invalid");

        var newHash = _hasher.Hash(request.NewPassword);
        owner.UpdatePassword(newHash.Hash, newHash.Salt, newHash.Algorithm, _clock.UtcNow);
        await _repo.UpdateAsync(owner, ct);
    }

    /// <summary>
    /// Setzt eine Legacy-PIN. Nicht mehr im Einsatz, aber für Migrationskompatibilität vorhanden.
    /// </summary>
    public async Task SetPinAsync(ChangePinRequestDto request, CancellationToken ct = default)
    {
        if (request.NewPin != request.ConfirmNewPin)
            throw new ArgumentException("Pins do not match");
        if (request.NewPin.Length < 4)
            throw new ArgumentException("Pin must be at least 4 digits");

        var owner = await _repo.SingleOrDefaultAsync(u => u.Role == Core.Enums.LocalUserRole.Owner, ct);
        if (owner is null) throw new InvalidOperationException("No owner profile found");

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

    /// <summary>
    /// Löscht die Legacy-PIN. Nicht mehr im Einsatz, aber für Migrationskompatibilität vorhanden.
    /// </summary>
    public async Task ClearPinAsync(CancellationToken ct = default)
    {
        var owner = await _repo.SingleOrDefaultAsync(u => u.Role == Core.Enums.LocalUserRole.Owner, ct);
        if (owner is null) throw new InvalidOperationException("No owner profile found");
        owner.ClearPin(_clock.UtcNow);
        await _repo.UpdateAsync(owner, ct);
    }

    /// <summary>
    /// Ruft das verschlüsselte Owner-Profil (Name, E-Mail) ab.
    /// </summary>
    public async Task<GetOwnerProfileResponseDto> GetOwnerProfileAsync(CancellationToken ct = default)
    {
        var owner = await _repo.SingleOrDefaultAsync(u => u.Role == Core.Enums.LocalUserRole.Owner, ct);
        if (owner is null) throw new InvalidOperationException("Geen eigenaarprofiel gevonden.");

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

    /// <summary>
    /// Aktualisiert das Owner-Profil (Name, E-Mail) und synchronisiert den Namen mit DisplayName.
    /// </summary>
    public async Task UpdateOwnerProfileAsync(UpdateOwnerProfileRequestDto request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Naam is verplicht.");
        if (request.Name.Length > 100)
            throw new ArgumentException("Naam mag niet langer dan 100 tekens zijn.");
        if (!string.IsNullOrEmpty(request.Email) && request.Email.Length > 200)
            throw new ArgumentException("E-mail mag niet langer dan 200 tekens zijn.");

        if (!string.IsNullOrEmpty(request.Email))
        {
            if (!request.Email.Contains("@") || request.Email.Count(c => c == '@') != 1)
                throw new ArgumentException("Ongeldig e-mailadres.");
        }

        var owner = await _repo.SingleOrDefaultAsync(u => u.Role == Core.Enums.LocalUserRole.Owner, ct);
        if (owner is null) throw new InvalidOperationException("Geen eigenaarprofiel gevonden.");

        // Validate uniqueness of new DisplayName (excluding current user)
        var trimmedName = request.Name.Trim();
        var normalized = trimmedName.ToLowerInvariant();
        var existing = await _repo.ListAsync(u => u.NormalizedName == normalized && u.Id != owner.Id, ct);
        if (existing.Any())
            throw new ArgumentException("Deze accountnaam bestaat al.");

        // Update encrypted profile payload
        var updatedPayload = new
        {
            Name = trimmedName,
            Email = string.IsNullOrEmpty(request.Email) ? string.Empty : request.Email.Trim().ToLowerInvariant()
        };

        var newJson = JsonSerializer.Serialize(updatedPayload);
        var encryptedPayload = _encryption.Encrypt(newJson);

        owner.ReplaceEncryptedPayload(encryptedPayload, owner.EncryptionVersion, _clock.UtcNow);

        // Synchronize DisplayName with name from encrypted payload
        owner.UpdateDisplayName(trimmedName, _clock.UtcNow);

        await _repo.UpdateAsync(owner, ct);
    }
}
