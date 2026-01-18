namespace BootManager.Core.Entities;

/// <summary>
/// Domein-entiteit voor het eigenaarprofiel. Er is maximaal 1 record.
/// Bevat hash voor wachtwoord, optioneel pincode-hash en versleutelde PII (naam, e-mail).
/// </summary>
public class OwnerProfile
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    // Wachtwoord (vereist)
    public string PasswordHash { get; private set; } = default!;
    public string PasswordSalt { get; private set; } = default!;
    public string HashAlgorithm { get; private set; } = default!;

    // Optionele pincode (mag null zijn als niet ingesteld)
    public string? PinHash { get; private set; }
    public string? PinSalt { get; private set; }

    // Recovery (optioneel)
    public string? RecoveryCodeHash { get; private set; }
    public string? RecoveryCodeSalt { get; private set; }

    // Versleutelde payload met JSON { Name, Email }
    public byte[] EncryptedProfilePayload { get; private set; } = Array.Empty<byte>();
    public int EncryptionVersion { get; private set; } = 1;

    public DateTime CreatedUtc { get; private set; }
    public DateTime? UpdatedUtc { get; private set; }

    private OwnerProfile() { } // Voor EF

    private OwnerProfile(string passwordHash, string passwordSalt, string hashAlgorithm,
        byte[] encryptedProfilePayload, int encryptionVersion, DateTime createdUtc)
    {
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        HashAlgorithm = hashAlgorithm;
        EncryptedProfilePayload = encryptedProfilePayload;
        EncryptionVersion = encryptionVersion;
        CreatedUtc = createdUtc;
    }

    public static OwnerProfile Create(
        string passwordHash,
        string passwordSalt,
        string hashAlgorithm,
        byte[] encryptedProfilePayload,
        int encryptionVersion,
        DateTime createdUtc)
        => new(passwordHash, passwordSalt, hashAlgorithm, encryptedProfilePayload, encryptionVersion, createdUtc);

    public void SetRecoveryCode(string recoveryHash, string recoverySalt, DateTime nowUtc)
    {
        RecoveryCodeHash = recoveryHash;
        RecoveryCodeSalt = recoverySalt;
        UpdatedUtc = nowUtc;
    }

    public void ClearRecoveryCode(DateTime nowUtc)
    {
        RecoveryCodeHash = null;
        RecoveryCodeSalt = null;
        UpdatedUtc = nowUtc;
    }

    public void UpdatePassword(string newHash, string newSalt, string algorithm, DateTime nowUtc)
    {
        PasswordHash = newHash;
        PasswordSalt = newSalt;
        HashAlgorithm = algorithm;
        UpdatedUtc = nowUtc;
    }

    public void ReplaceEncryptedPayload(byte[] newPayload, int encryptionVersion, DateTime nowUtc)
    {
        EncryptedProfilePayload = newPayload;
        EncryptionVersion = encryptionVersion;
        UpdatedUtc = nowUtc;
    }

    /// <summary>Stelt/overschrijft de pincodehash (optioneel feature; mag null blijven in DB).</summary>
    public void SetPin(string pinHash, string pinSalt, DateTime nowUtc)
    {
        PinHash = pinHash;
        PinSalt = pinSalt;
        UpdatedUtc = nowUtc;
    }

    /// <summary>Verwijdert de pincode.</summary>
    public void ClearPin(DateTime nowUtc)
    {
        PinHash = null;
        PinSalt = null;
        UpdatedUtc = nowUtc;
    }
}