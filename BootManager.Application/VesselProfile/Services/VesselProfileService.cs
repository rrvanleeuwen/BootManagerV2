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

        var emptyVesselName = "Naamloze boot"; // Standaard bootnaam voor nieuw profiel
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
            throw new InvalidOperationException("Bootprofiel niet gevonden. Zorg dat u eerst GetOrCreateVesselProfileAsync aanroept.");
        }

        // Update het profiel
        profile.Update(
            vesselName: request.VesselName,
            homePort: request.HomePort,
            callSign: request.CallSign,
            mmsi: request.Mmsi,
            currentEngineHours: request.CurrentEngineHours,
            currentLogstand: request.CurrentLogstand,
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
            throw new ArgumentException("Bootnaam is verplicht.", nameof(request.VesselName));
        }

        if (request.VesselName.Length > MaxVesselNameLength)
        {
            throw new ArgumentException(
                $"Bootnaam mag niet langer zijn dan {MaxVesselNameLength} tekens.",
                nameof(request.VesselName));
        }

        if (!string.IsNullOrEmpty(request.HomePort) && request.HomePort.Length > MaxHomePortLength)
        {
            throw new ArgumentException(
                $"Thuishaven mag niet langer zijn dan {MaxHomePortLength} tekens.",
                nameof(request.HomePort));
        }

        if (!string.IsNullOrEmpty(request.CallSign) && request.CallSign.Length > MaxCallSignLength)
        {
            throw new ArgumentException(
                $"Roepnaam mag niet langer zijn dan {MaxCallSignLength} tekens.",
                nameof(request.CallSign));
        }

        if (!string.IsNullOrEmpty(request.Mmsi) && request.Mmsi.Length > MaxMmsiLength)
        {
            throw new ArgumentException(
                $"MMSI mag niet langer zijn dan {MaxMmsiLength} tekens.",
                nameof(request.Mmsi));
        }

        // Valideer actuele tellerstandwaarden (niet-negatief als aanwezig)
        if (request.CurrentEngineHours.HasValue && request.CurrentEngineHours < 0)
        {
            throw new ArgumentException(
                "Actuele motorurenstand mag niet negatief zijn.",
                nameof(request.CurrentEngineHours));
        }

        if (request.CurrentLogstand.HasValue && request.CurrentLogstand < 0)
        {
            throw new ArgumentException(
                "Actuele logstandwaarde mag niet negatief zijn.",
                nameof(request.CurrentLogstand));
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
            CurrentEngineHours = profile.CurrentEngineHours,
            CurrentLogstand = profile.CurrentLogstand,
            CreatedUtc = profile.CreatedUtc,
            UpdatedUtc = profile.UpdatedUtc
        };
    }

    /// <summary>
    /// Voortschrijving van actuele motorurenstand en logstandwaarde op basis van reis-tellerwaarden.
    /// Verhoogt alleen als de reis-kandidaat hoger is dan de huidige profiel-waarde.
    /// </summary>
    public async Task<VesselProfileDto> AdvanceCurrentMetersAsync(decimal?[] engineHoursCandidates, decimal?[] logstandCandidates, CancellationToken ct = default)
    {
        // Haal bestaand profiel op
        var profiles = await _repo.ListAsync(ct: ct);
        var profile = profiles.FirstOrDefault();

        if (profile == null)
        {
            throw new InvalidOperationException("Bootprofiel niet gevonden. Zorg dat u eerst GetOrCreateVesselProfileAsync aanroept.");
        }

        // Bepaal hoogste geldige motorurenstand
        var maxEngineHours = GetMaxValidValue(engineHoursCandidates);
        if (maxEngineHours.HasValue && maxEngineHours.Value > (profile.CurrentEngineHours ?? 0))
        {
            profile.UpdateCurrentEngineHours(maxEngineHours, _clock.UtcNow);
            _logger.LogInformation("Vessel profile: engine hours advanced to {EngineHours}", maxEngineHours);
        }

        // Bepaal hoogste geldige logstandwaarde
        var maxLogstand = GetMaxValidValue(logstandCandidates);
        if (maxLogstand.HasValue && maxLogstand.Value > (profile.CurrentLogstand ?? 0))
        {
            profile.UpdateCurrentLogstand(maxLogstand, _clock.UtcNow);
            _logger.LogInformation("Vessel profile: logstand advanced to {Logstand}", maxLogstand);
        }

        await _repo.UpdateAsync(profile, ct);

        return MapToDto(profile);
    }

    /// <summary>
    /// Bepaalt de hoogste geldige waarde uit een array van kandidaatwaarden.
    /// Geldige waarden zijn niet-null en niet-negatief.
    /// Retourneert null als geen geldige waarden aanwezig zijn.
    /// </summary>
    private static decimal? GetMaxValidValue(decimal?[] candidates)
    {
        if (candidates == null || candidates.Length == 0)
        {
            return null;
        }

        decimal? maxValid = null;
        foreach (var candidate in candidates)
        {
            // Alleen overwegen als niet null, niet negatief en positief
            if (candidate.HasValue && candidate.Value > 0)
            {
                if (maxValid == null || candidate.Value > maxValid.Value)
                {
                    maxValid = candidate.Value;
                }
            }
        }

        return maxValid;
    }
}
