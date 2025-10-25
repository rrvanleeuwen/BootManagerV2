using BootManager.Application.OwnerRegistration.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using System.Text.Json;

namespace BootManager.Application.OwnerRegistration.Services;

/// <summary>
/// Application service responsible for the first-owner registration flow and first-run detection.
/// Implements user story US0.2 (Register first owner).
/// </summary>
/// <remarks>
/// Responsibilities:
/// - Determine whether the application is in first-run state (no owner profile persisted).
/// - Create and persist a single <see cref="OwnerProfile"/> with secure password hashing
///   and encrypted PII payload (name, email).
/// - Optionally generate a one-time recovery code and persist only its hash.
/// 
/// Notes:
/// - This service does not manage sign-in/session; it focuses on initial registration.
/// - Throws exceptions to signal business rule violations (e.g., owner already exists).
/// - Uses abstractions (IRepository, IPasswordHasher, IEncryptionService, ISystemClock) to
///   preserve Clean Architecture boundaries and enable unit testing.
/// </remarks>
public class OwnerRegistrationService : IOwnerRegistrationService
{
    private readonly IRepository<OwnerProfile> _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IEncryptionService _encryption;
    private readonly ISystemClock _clock;

    /// <summary>
    /// Creates a new <see cref="OwnerRegistrationService"/>.
    /// </summary>
    /// <param name="repo">Generic repository for persisting and querying <see cref="OwnerProfile"/>.</param>
    /// <param name="hasher">Password hasher used to hash and verify secrets.</param>
    /// <param name="encryption">Encryption service for protecting PII within the profile payload.</param>
    /// <param name="clock">System clock abstraction for consistent, testable timestamps.</param>
    public OwnerRegistrationService(
        IRepository<OwnerProfile> repo,
        IPasswordHasher hasher,
        IEncryptionService encryption,
        ISystemClock clock)
    {
        _repo = repo;
        _hasher = hasher;
        _encryption = encryption;
        _clock = clock;
    }

    /// <summary>
    /// Returns whether this is the first run of the application (i.e., no owner profile exists).
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <see cref="IsFirstRunResultDto"/> with <c>IsFirstRun</c> set to <c>true</c> when no owner exists; otherwise <c>false</c>.
    /// </returns>
    public async Task<IsFirstRunResultDto> CheckFirstRunAsync(CancellationToken ct = default)
    {
        var exists = await _repo.AnyAsync(ct: ct);
        return new IsFirstRunResultDto { IsFirstRun = !exists };
    }

    /// <summary>
    /// Registers the first (and only) owner profile. Fails if a profile already exists.
    /// </summary>
    /// <param name="request">Owner registration request (name, email, password, options).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="RegisterOwnerResponseDto"/> containing the new owner identifier, normalized email,
    /// timestamps, and optionally the plaintext recovery code (shown once to the user).
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when an owner already exists.</exception>
    /// <exception cref="ArgumentException">Thrown when input validation fails (name, email, password).</exception>
    /// <remarks>
    /// Security:
    /// - Password and recovery code are hashed (non-reversible).
    /// - Name and email are stored inside an encrypted payload (symmetric encryption).
    /// 
    /// Validation:
    /// - Minimal checks here (presence, email shape, password length and match).
    ///   Consider central validators if stricter policies are desired.
    /// </remarks>
    public async Task<RegisterOwnerResponseDto> RegisterFirstOwnerAsync(RegisterOwnerRequestDto request, CancellationToken ct = default)
    {
        // Basic validation (can be refactored later)
        if (await _repo.AnyAsync(ct: ct))
            throw new InvalidOperationException("Owner already exists.");
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name required");
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            throw new ArgumentException("Valid email required");
        if (request.Password != request.ConfirmPassword)
            throw new ArgumentException("Passwords do not match");
        if (request.Password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters");

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var hash = _hasher.Hash(request.Password);

        // Encrypt PII (Name, Email) as a compact JSON payload
        var payloadObj = new { request.Name, Email = normalizedEmail };
        var json = JsonSerializer.Serialize(payloadObj);
        var encrypted = _encryption.Encrypt(json);

        var owner = OwnerProfile.Create(
            passwordHash: hash.Hash,
            passwordSalt: hash.Salt,
            hashAlgorithm: hash.Algorithm,
            encryptedProfilePayload: encrypted,
            encryptionVersion: 1,
            createdUtc: _clock.UtcNow
        );

        // Optional: generate one-time recovery code (only plaintext returned to caller once)
        string? recoveryPlain = null;
        if (request.GenerateRecoveryCode)
        {
            recoveryPlain = GenerateRecoveryCode();
            var rcHash = _hasher.Hash(recoveryPlain);
            owner.SetRecoveryCode(rcHash.Hash, rcHash.Salt, _clock.UtcNow);
        }

        await _repo.AddAsync(owner, ct);

        return new RegisterOwnerResponseDto
        {
            OwnerId = owner.Id,
            Name = request.Name,
            Email = normalizedEmail,
            CreatedUtc = owner.CreatedUtc,
            RecoveryCodePlain = recoveryPlain
        };
    }

    /// <summary>
    /// Generates a random, human-friendly recovery code intended to be shown once and stored by the user.
    /// The plaintext value is not persisted; only its hash is stored.
    /// </summary>
    /// <returns>A 24-character recovery code (A–Z excluding ambiguous characters, digits 2–9).</returns>
    private static string GenerateRecoveryCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rnd = Random.Shared;
        return new string(Enumerable.Range(0, 24).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
    }
}