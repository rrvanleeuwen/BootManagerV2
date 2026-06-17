using BootManager.Core.Enums;

namespace BootManager.Core.Entities;

/// <summary>
/// Lokale gebruiker-entiteit voor uniforme Owner- en Crew-authenticatie.
/// Bevat hash voor wachtwoord, rol, actieve status en credentialversie voor sessievalidatie.
/// </summary>
public class LocalUser
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>Leesbare, unieke accountnaam (trimmed, case-insensitive uniek, max 100 chars).</summary>
    public string DisplayName { get; private set; } = default!;

    /// <summary>Genormaliseerde naam voor uniqueness-checking (lowercase).</summary>
    public string NormalizedName { get; private set; } = default!;

    /// <summary>Lokale rol: Owner of Crew.</summary>
    public LocalUserRole Role { get; private set; }

    /// <summary>Actieve status; false betekent account is uitgeschakeld.</summary>
    public bool IsActive { get; private set; } = true;

    // Wachtwoord (vereist)
    public string PasswordHash { get; private set; } = default!;
    public string PasswordSalt { get; private set; } = default!;
    public string HashAlgorithm { get; private set; } = default!;

    /// <summary>Oplopend versienummer voor sessie-/token-intrekking bij reset/uitschakeling.</summary>
    public int CredentialVersion { get; private set; } = 1;

    /// <summary>Vlag: wachtwoord moet worden gewijzigd (bootstrap of reset).</summary>
    public bool PasswordChangeRequired { get; private set; }

    /// <summary>Vlag: Owner-onboarding voltooid (alleen voor Owner).</summary>
    public bool OnboardingCompleted { get; private set; }

    // Versleutelde payload met JSON { Name, Email } (behouden van OwnerProfile)
    public byte[] EncryptedProfilePayload { get; private set; } = Array.Empty<byte>();
    public int EncryptionVersion { get; private set; } = 1;

    // Optiónele legacy velden (voor compatibiliteit bij migratie; niet gebruikt in normale flow)
    public string? PinHash { get; private set; }
    public string? PinSalt { get; private set; }
    public string? RecoveryCodeHash { get; private set; }
    public string? RecoveryCodeSalt { get; private set; }

    public DateTime CreatedUtc { get; private set; }
    public DateTime? UpdatedUtc { get; private set; }

    private LocalUser() { } // Voor EF

    private LocalUser(
        string displayName,
        LocalUserRole role,
        string passwordHash,
        string passwordSalt,
        string hashAlgorithm,
        byte[] encryptedProfilePayload,
        int encryptionVersion,
        DateTime createdUtc,
        bool passwordChangeRequired = false,
        bool onboardingCompleted = false)
    {
        DisplayName = displayName.Trim();
        NormalizedName = displayName.Trim().ToLowerInvariant();
        Role = role;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        HashAlgorithm = hashAlgorithm;
        EncryptedProfilePayload = encryptedProfilePayload;
        EncryptionVersion = encryptionVersion;
        CreatedUtc = createdUtc;
        PasswordChangeRequired = passwordChangeRequired;
        OnboardingCompleted = onboardingCompleted;
    }

    public static LocalUser Create(
        string displayName,
        LocalUserRole role,
        string passwordHash,
        string passwordSalt,
        string hashAlgorithm,
        byte[] encryptedProfilePayload,
        int encryptionVersion,
        DateTime createdUtc,
        bool passwordChangeRequired = false,
        bool onboardingCompleted = false)
        => new(displayName, role, passwordHash, passwordSalt, hashAlgorithm,
            encryptedProfilePayload, encryptionVersion, createdUtc,
            passwordChangeRequired, onboardingCompleted);

    public void UpdatePassword(string newHash, string newSalt, string algorithm, DateTime nowUtc)
    {
        PasswordHash = newHash;
        PasswordSalt = newSalt;
        HashAlgorithm = algorithm;
        CredentialVersion++;
        UpdatedUtc = nowUtc;
    }

    public void SetPasswordChangeRequired(bool required, DateTime nowUtc)
    {
        PasswordChangeRequired = required;
        UpdatedUtc = nowUtc;
    }

    public void SetOnboardingCompleted(bool completed, DateTime nowUtc)
    {
        OnboardingCompleted = completed;
        UpdatedUtc = nowUtc;
    }

    public void SetActive(bool active, DateTime nowUtc)
    {
        IsActive = active;
        if (!active)
        {
            CredentialVersion++;
        }
        UpdatedUtc = nowUtc;
    }

    public void UpdateDisplayName(string newName, DateTime nowUtc)
    {
        DisplayName = newName.Trim();
        NormalizedName = newName.Trim().ToLowerInvariant();
        UpdatedUtc = nowUtc;
    }

    public void ReplaceEncryptedPayload(byte[] newPayload, int encryptionVersion, DateTime nowUtc)
    {
        EncryptedProfilePayload = newPayload;
        EncryptionVersion = encryptionVersion;
        UpdatedUtc = nowUtc;
    }

    /// <summary>Legacy: Setzt die PIN-Hash (für Migrationkompatibilität).</summary>
    public void SetPin(string pinHash, string pinSalt, DateTime nowUtc)
    {
        PinHash = pinHash;
        PinSalt = pinSalt;
        UpdatedUtc = nowUtc;
    }

    /// <summary>Legacy: Löscht die PIN (für Migrationkompatibilität).</summary>
    public void ClearPin(DateTime nowUtc)
    {
        PinHash = null;
        PinSalt = null;
        UpdatedUtc = nowUtc;
    }
}
