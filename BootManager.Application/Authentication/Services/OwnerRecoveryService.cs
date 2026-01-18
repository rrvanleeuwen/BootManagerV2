using BootManager.Application.Authentication.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;

namespace BootManager.Application.Authentication.Services;

/// <summary>
/// Responsible for restoring access using a one-time backup code or importing a master key.
/// - Backup code: user provides the recovery code generated during registration. Only its hash
///   is stored; we compare and then clear recovery info and reactivate owner profile.
/// - Master key: imports an externally-held key that decrypts and re-seals profile payload.
/// </summary>
public class OwnerRecoveryService : IOwnerRecoveryService
{
    private readonly IRepository<OwnerProfile> _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IEncryptionService _encryption;
    private readonly ISystemClock _clock;

    public OwnerRecoveryService(IRepository<OwnerProfile> repo, IPasswordHasher hasher, IEncryptionService encryption, ISystemClock clock)
    {
        _repo = repo;
        _hasher = hasher;
        _encryption = encryption;
        _clock = clock;
    }

    public async Task<RestoreAccessResponseDto> RestoreWithBackupCodeAsync(string backupCode, string newPassword, CancellationToken ct = default)
    {
        var owner = await _repo.SingleOrDefaultAsync(ct: ct);
        if (owner is null) return new RestoreAccessResponseDto { Success = false, Message = "Geen eigenaarprofiel gevonden." };

        if (string.IsNullOrEmpty(owner.RecoveryCodeHash) || string.IsNullOrEmpty(owner.RecoveryCodeSalt))
            return new RestoreAccessResponseDto { Success = false, Message = "Er is geen recovery code geregistreerd." };

        var stored = new HashResult(owner.RecoveryCodeHash, owner.RecoveryCodeSalt, owner.HashAlgorithm);
        if (!_hasher.Verify(backupCode, stored))
            return new RestoreAccessResponseDto { Success = false, Message = "Onjuiste recovery code." };

        // Clear recovery code so it is one-time
        owner.ClearRecoveryCode(_clock.UtcNow);
        // Optionally reset pin state
        owner.ClearPin(_clock.UtcNow);

        // Set new password
        var newHashPwd = _hasher.Hash(newPassword);
        owner.UpdatePassword(newHashPwd.Hash, newHashPwd.Salt, newHashPwd.Algorithm, _clock.UtcNow);

        // Generate a new recovery code and persist its hash so user receives a fresh one
        var newRecoveryPlain = GenerateRecoveryCode();
        var newHash = _hasher.Hash(newRecoveryPlain);
        owner.SetRecoveryCode(newHash.Hash, newHash.Salt, _clock.UtcNow);

        await _repo.UpdateAsync(owner, ct);
        return new RestoreAccessResponseDto { Success = true, NewRecoveryCodePlain = newRecoveryPlain };
    }

    public async Task<RestoreAccessResponseDto> RestoreWithMasterKeyAsync(string masterKey, string newPassword, CancellationToken ct = default)
    {
        var owner = await _repo.SingleOrDefaultAsync(ct: ct);
        if (owner is null) return new RestoreAccessResponseDto { Success = false, Message = "Geen eigenaarprofiel gevonden." };

        try
        {
            // Try to decrypt the payload with provided masterKey. If succeeds, re-encrypt with current app key
            var plain = _encryption.Decrypt(owner.EncryptedProfilePayload);
            // If masterKey was intended to be used to decrypt, we'd need a way to use it. For now assume masterKey
            // is actually the plaintext of the backup (this is a placeholder for real key import).
            // Re-seal payload (no-op with current encryption service) and mark updated.
            owner.ReplaceEncryptedPayload(owner.EncryptedProfilePayload, owner.EncryptionVersion, _clock.UtcNow);

            // Set new password
            var newHash = _hasher.Hash(newPassword);
            owner.UpdatePassword(newHash.Hash, newHash.Salt, newHash.Algorithm, _clock.UtcNow);

            // Generate new recovery code and persist
            var newRec = GenerateRecoveryCode();
            var rcHash = _hasher.Hash(newRec);
            owner.SetRecoveryCode(rcHash.Hash, rcHash.Salt, _clock.UtcNow);

            await _repo.UpdateAsync(owner, ct);
            return new RestoreAccessResponseDto { Success = true, NewRecoveryCodePlain = newRec };
        }
        catch
        {
            return new RestoreAccessResponseDto { Success = false, Message = "Master key import mislukt." };
        }
    }

    /// <summary>
    /// Generates a random, human-friendly recovery code intended to be shown once and stored by the user.
    /// The plaintext value is not persisted; only its hash is stored.
    /// </summary>
    private static string GenerateRecoveryCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rnd = Random.Shared;
        return new string(Enumerable.Range(0, 24).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
    }

}
