using BootManager.Application.VesselProfile.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BootManager.Application.VesselProfile.Services;

/// <summary>
/// Service voor het beheer van het bootprofiel (singleton per installatie).
/// Zorgt ervoor dat er maximaal 1 bootprofiel bestaat en handelt get/create/update-logica af.
/// </summary>
public class VesselProfileService : IVesselProfileService
{
    private const int MaxVesselNameLength = 128;
    private const int MaxHomePortLength = 128;
    private const int MaxCallSignLength = 64;
    private const int MaxMmsiLength = 32;

    private readonly IRepository<Core.Entities.VesselProfile> _repo;
    private readonly ISystemClock _clock;
    private readonly ILogger<VesselProfileService> _logger;

    /// <summary>
    /// Initialiseert een nieuwe instantie van VesselProfileService.
    /// </summary>
    public VesselProfileService(
        IRepository<Core.Entities.VesselProfile> repo,
        ISystemClock clock,
        ILogger<VesselProfileService> logger)
    {
        _repo = repo;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    /// Haalt het huidige bootprofiel op, of maakt een lege profiel aan als er nog geen bestaat.
    /// </summary>
    public async Task<VesselProfileDto> GetOrCreateVesselProfileAsync(CancellationToken ct = default)
    {
        // Haal bestaand profiel op
        var profiles = await _repo.ListAsync(ct: ct);
        var profile = profiles.FirstOrDefault();

        // Als er al een profiel bestaat, retourneer deze
        if (profile != null)
        {
            _logger.LogDebug("Vessel profile found with ID {VesselProfileId}", profile.Id);
            return MapToDto(profile);
        }

        // Anders maak een leeg profiel aan
        _logger.LogInformation("No vessel profile found; creating empty profile");

        var emptyVesselName = "Unnamed Vessel"; // Standaard bootnaam voor nieuw profiel
        var newProfile = Core.Entities.VesselProfile.Create(
            vesselName: emptyVesselName,
            homePort: null,
            callSign: null,
            mmsi: null,
            createdUtc: _clock.UtcNow
        );

        await _repo.AddAsync(newProfile, ct);
        _logger.LogInformation("Empty vessel profile created with ID {VesselProfileId}", newProfile.Id);

        return MapToDto(newProfile);
    }

    /// <summary>
    /// Werkt het bestaande bootprofiel bij met nieuwe gegevens.
    /// </summary>
    public async Task<VesselProfileDto> UpdateVesselProfileAsync(UpdateVesselProfileRequestDto request, CancellationToken ct = default)
    {
        // Valideer input
        ValidateRequest(request);

        // Haal bestaand profiel op
        var profiles = await _repo.ListAsync(ct: ct);
        var profile = profiles.FirstOrDefault();

        if (profile == null)
        {
            throw new InvalidOperationException("No vessel profile found. Please call GetOrCreateVesselProfileAsync first.");
        }

        // Update het profiel
        profile.Update(
            vesselName: request.VesselName,
            homePort: request.HomePort,
            callSign: request.CallSign,
            mmsi: request.Mmsi,
            updatedUtc: _clock.UtcNow
        );

        await _repo.UpdateAsync(profile, ct);
        _logger.LogInformation("Vessel profile updated with ID {VesselProfileId}", profile.Id);

        return MapToDto(profile);
    }

    /// <summary>
    /// Valideert de invoergegevens.
    /// </summary>
    private static void ValidateRequest(UpdateVesselProfileRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.VesselName))
        {
            throw new ArgumentException("VesselName is required and cannot be empty.", nameof(request.VesselName));
        }

        if (request.VesselName.Length > MaxVesselNameLength)
        {
            throw new ArgumentException(
                $"VesselName cannot exceed {MaxVesselNameLength} characters.", 
                nameof(request.VesselName));
        }

        if (!string.IsNullOrEmpty(request.HomePort) && request.HomePort.Length > MaxHomePortLength)
        {
            throw new ArgumentException(
                $"HomePort cannot exceed {MaxHomePortLength} characters.", 
                nameof(request.HomePort));
        }

        if (!string.IsNullOrEmpty(request.CallSign) && request.CallSign.Length > MaxCallSignLength)
        {
            throw new ArgumentException(
                $"CallSign cannot exceed {MaxCallSignLength} characters.", 
                nameof(request.CallSign));
        }

        if (!string.IsNullOrEmpty(request.Mmsi) && request.Mmsi.Length > MaxMmsiLength)
        {
            throw new ArgumentException(
                $"Mmsi cannot exceed {MaxMmsiLength} characters.", 
                nameof(request.Mmsi));
        }
    }

    /// <summary>
    /// Zet een VesselProfile-entiteit om naar een DTO.
    /// </summary>
    private static VesselProfileDto MapToDto(Core.Entities.VesselProfile profile)
    {
        return new VesselProfileDto
        {
            Id = profile.Id,
            VesselName = profile.VesselName,
            HomePort = profile.HomePort,
            CallSign = profile.CallSign,
            Mmsi = profile.Mmsi,
            CreatedUtc = profile.CreatedUtc,
            UpdatedUtc = profile.UpdatedUtc
        };
    }
}
