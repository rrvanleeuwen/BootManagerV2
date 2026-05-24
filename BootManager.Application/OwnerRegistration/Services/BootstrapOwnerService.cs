using BootManager.Application.OwnerRegistration.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BootManager.Application.OwnerRegistration.Services;

/// <summary>
/// Bootstrap-service die automatisch een eigenaar aanmaakt bij een lege database.
/// Verwendet het bootstrap wachtwoord dat vanuit configuratie wordt doorgegeven.
/// </summary>
public class BootstrapOwnerService : IBootstrapOwnerService
{
    private const string BootstrapOwnerName = "BootManager Owner";
    private const string BootstrapOwnerEmail = "owner@bootmanager.local";
    private const string DevFallbackPassword = "BootManagerDev123!";

    private readonly IRepository<OwnerProfile> _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IEncryptionService _encryption;
    private readonly ISystemClock _clock;
    private readonly ILogger<BootstrapOwnerService> _logger;

    public BootstrapOwnerService(
        IRepository<OwnerProfile> repo,
        IPasswordHasher hasher,
        IEncryptionService encryption,
        ISystemClock clock,
        ILogger<BootstrapOwnerService> logger)
    {
        _repo = repo;
        _hasher = hasher;
        _encryption = encryption;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    /// Zet de bootstrap eigenaar op als de database leeg is.
    /// </summary>
    public async Task<bool> EnsureBootstrapOwnerAsync(string? bootstrapPassword, bool isProduction, CancellationToken ct = default)
    {
        // Controleer of er al een eigenaar bestaat
        var ownerExists = await _repo.AnyAsync(ct: ct);
        if (ownerExists)
        {
            _logger.LogInformation("Owner already exists; bootstrap owner creation skipped.");
            return false;
        }

        // In Production moet bootstrap wachtwoord aanwezig zijn
        if (isProduction && string.IsNullOrWhiteSpace(bootstrapPassword))
        {
            var message = "Production mode: Bootstrap:DefaultPassword is required but not configured. "
                + "Unable to create bootstrap owner. Please configure Bootstrap:DefaultPassword and restart.";
            _logger.LogError(message);
            throw new InvalidOperationException(message);
        }

        // In Development mag fallback gebruikt worden
        if (string.IsNullOrWhiteSpace(bootstrapPassword))
        {
            bootstrapPassword = DevFallbackPassword;
            _logger.LogWarning(
                "Bootstrap:DefaultPassword not configured; using development fallback password. "
                + "DO NOT USE THIS IN PRODUCTION.");
        }

        // Maak bootstrap eigenaar aan
        _logger.LogInformation("Creating bootstrap owner: {Name} ({Email})", BootstrapOwnerName, BootstrapOwnerEmail);

        var hash = _hasher.Hash(bootstrapPassword);
        var payloadObj = new { Name = BootstrapOwnerName, Email = BootstrapOwnerEmail };
        var json = JsonSerializer.Serialize(payloadObj);
        var encrypted = _encryption.Encrypt(json);

        var owner = OwnerProfile.Create(
            passwordHash: hash.Hash,
            passwordSalt: hash.Salt,
            hashAlgorithm: hash.Algorithm,
            encryptedProfilePayload: encrypted,
            encryptionVersion: 1,
            createdUtc: _clock.UtcNow,
            passwordChangeRequired: true,
            onboardingCompleted: false
        );

        await _repo.AddAsync(owner, ct);

        _logger.LogInformation("Bootstrap owner created successfully with ID {OwnerId}", owner.Id);
        return true;
    }
}
