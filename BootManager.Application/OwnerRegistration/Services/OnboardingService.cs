using BootManager.Application.OwnerRegistration.DTOs;
using BootManager.Application.VesselProfile.DTOs;
using BootManager.Application.VesselProfile.Services;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BootManager.Application.OwnerRegistration.Services;

/// <summary>
/// Service voor het voltooien van de initiale onboarding met eigenaar-, boot- en wachtwoordgegevens.
/// </summary>
public class OnboardingService : IOnboardingService
{
    private const int MinPasswordLength = 8;

    private readonly IRepository<OwnerProfile> _ownerRepository;
    private readonly IPasswordHasher _hasher;
    private readonly IEncryptionService _encryption;
    private readonly ISystemClock _clock;
    private readonly IVesselProfileService _vesselProfileService;
    private readonly ILogger<OnboardingService> _logger;

    public OnboardingService(
        IRepository<OwnerProfile> ownerRepository,
        IPasswordHasher hasher,
        IEncryptionService encryption,
        ISystemClock clock,
        IVesselProfileService vesselProfileService,
        ILogger<OnboardingService> logger)
    {
        _ownerRepository = ownerRepository;
        _hasher = hasher;
        _encryption = encryption;
        _clock = clock;
        _vesselProfileService = vesselProfileService;
        _logger = logger;
    }

    public async Task<CompleteOnboardingResponseDto> CompleteInitialOnboardingAsync(
        CompleteOnboardingRequestDto request,
        CancellationToken ct = default)
    {
        try
        {
            // Valideer invoer
            ValidateRequest(request);

            // Haal de huidige eigenaar op
            var owner = await _ownerRepository.SingleOrDefaultAsync(ct: ct);
            if (owner is null)
            {
                throw new InvalidOperationException("No owner profile found.");
            }

            // Verificeer het huidige/bootstrap wachtwoord
            var storedPwd = new HashResult(owner.PasswordHash, owner.PasswordSalt, owner.HashAlgorithm);
            var passwordValid = _hasher.Verify(request.CurrentPassword, storedPwd);
            if (!passwordValid)
            {
                throw new UnauthorizedAccessException("Huidig wachtwoord is onjuist.");
            }

            // Controleer dat het nieuwe wachtwoord verschilt van het huidige
            if (_hasher.Verify(request.NewPassword, storedPwd))
            {
                throw new ArgumentException("Nieuw wachtwoord mag niet hetzelfde zijn als het huidige wachtwoord.");
            }

            // Zorg dat het singleton bootprofiel bestaat voordat we het bijwerken.
            await _vesselProfileService.GetOrCreateVesselProfileAsync(ct);

            // Update het bootprofiel
            var updateVesselRequest = new UpdateVesselProfileRequestDto
            {
                VesselName = request.VesselName,
                HomePort = request.HomePort,
                CallSign = request.CallSign,
                Mmsi = request.Mmsi
            };
            var updatedVessel = await _vesselProfileService.UpdateVesselProfileAsync(updateVesselRequest, ct);

            // Update de versleutelde eigenaar-payload met naam en e-mail
            var payloadObj = new { Name = request.OwnerName, Email = request.OwnerEmail ?? string.Empty };
            var json = JsonSerializer.Serialize(payloadObj);
            var encrypted = _encryption.Encrypt(json);
            owner.ReplaceEncryptedPayload(encrypted, 1, _clock.UtcNow);

            // Wijzig het wachtwoord
            var newHash = _hasher.Hash(request.NewPassword);
            owner.UpdatePassword(newHash.Hash, newHash.Salt, newHash.Algorithm, _clock.UtcNow);

            // Zet de setup-vlaggen op voltooid
            owner.SetPasswordChangeRequired(false, _clock.UtcNow);
            owner.SetOnboardingCompleted(true, _clock.UtcNow);

            // Sla alle wijzigingen op
            await _ownerRepository.UpdateAsync(owner, ct);

            _logger.LogInformation(
                "Onboarding completed successfully for owner {OwnerId}: name={OwnerName}, vessel={VesselName}",
                owner.Id,
                request.OwnerName,
                request.VesselName);

            return new CompleteOnboardingResponseDto
            {
                Success = true,
                UpdatedVesselProfile = updatedVessel,
                UpdatedOwnerName = request.OwnerName,
                UpdatedOwnerEmail = request.OwnerEmail
            };
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Onboarding validation failed: {Message}", ex.Message);
            return new CompleteOnboardingResponseDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Onboarding authorization failed: {Message}", ex.Message);
            return new CompleteOnboardingResponseDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Onboarding failed with invalid operation: {Message}", ex.Message);
            return new CompleteOnboardingResponseDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during onboarding");
            return new CompleteOnboardingResponseDto
            {
                Success = false,
                ErrorMessage = "Er is een onverwachte fout opgetreden. Probeer het later opnieuw."
            };
        }
    }

    /// <summary>
    /// Valideert de onboarding-aanvraag.
    /// </summary>
    private static void ValidateRequest(CompleteOnboardingRequestDto request)
    {
        // Valideer eigenaar naam
        if (string.IsNullOrWhiteSpace(request.OwnerName))
        {
            throw new ArgumentException("Eigenaarsnaam is verplicht.");
        }

        // Valideer boot naam
        if (string.IsNullOrWhiteSpace(request.VesselName))
        {
            throw new ArgumentException("Bootnaam is verplicht.");
        }

        // Valideer wachtwoord
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            throw new ArgumentException("Huidig wachtwoord is verplicht.");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            throw new ArgumentException("Nieuw wachtwoord is verplicht.");
        }

        if (request.NewPassword.Length < MinPasswordLength)
        {
            throw new ArgumentException($"Nieuw wachtwoord moet minimaal {MinPasswordLength} tekens lang zijn.");
        }

        if (string.IsNullOrWhiteSpace(request.ConfirmNewPassword))
        {
            throw new ArgumentException("Bevestiging van het nieuwe wachtwoord is verplicht.");
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            throw new ArgumentException("Nieuw wachtwoord en bevestiging komen niet overeen.");
        }
    }
}
