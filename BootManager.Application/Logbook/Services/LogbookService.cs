using BootManager.Application.Logbook.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BootManager.Application.Logbook.Services;

/// <summary>
/// Implementatie van <see cref="ILogbookService"/> via de generieke repository.
/// </summary>
public class LogbookService : ILogbookService
{
    private readonly IRepository<LogbookTrip> _tripRepo;
    private readonly IRepository<LogbookEntry> _entryRepo;
    private readonly ILogbookMeasurementSuggestionService _suggestionService;
    private readonly ILogbookAttachmentService _attachmentService;
    private readonly ILogger<LogbookService> _logger;

    /// <summary>
    /// Maakt een nieuwe <see cref="LogbookService"/> aan.
    /// </summary>
    public LogbookService(
        IRepository<LogbookTrip> tripRepo,
        IRepository<LogbookEntry> entryRepo,
        ILogbookMeasurementSuggestionService suggestionService,
        ILogbookAttachmentService attachmentService,
        ILogger<LogbookService> logger)
    {
        _tripRepo = tripRepo;
        _entryRepo = entryRepo;
        _suggestionService = suggestionService;
        _attachmentService = attachmentService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<LogbookTripDto> CreateTripAsync(CreateLogbookTripDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Reisnaam mag niet leeg zijn.", nameof(dto.Name));

        var entity = new LogbookTrip(
            name: dto.Name,
            departureUtc: dto.DepartureUtc,
            arrivalUtc: dto.ArrivalUtc,
            departurePort: dto.DeparturePort,
            destinationPort: dto.DestinationPort,
            vesselName: dto.VesselName,
            crew: dto.Crew,
            notes: dto.Notes,
            logstandStart: dto.LogstandStart,
            loggedMiles: dto.LoggedMiles,
            engineHoursStart: dto.EngineHoursStart,
            engineHoursEnd: dto.EngineHoursEnd,
            fuel: dto.Fuel,
            totalSailingHours: dto.TotalSailingHours,
            logIntervalMinutes: dto.LogIntervalMinutes);

        await _tripRepo.AddAsync(entity, cancellationToken);
        _logger.LogInformation("Logboek reis aangemaakt met id {TripId}.", entity.Id);
        return MapTrip(entity);
    }

    /// <inheritdoc />
    public async Task<LogbookTripDto?> GetTripAsync(int tripId, CancellationToken cancellationToken = default)
    {
        var entity = await _tripRepo.SingleOrDefaultAsync(t => t.Id == tripId, cancellationToken);
        return entity is null ? null : MapTrip(entity);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LogbookTripDto>> GetAllTripsAsync(CancellationToken cancellationToken = default)
    {
        var list = await _tripRepo.ListAsync(null, cancellationToken);
        return list.OrderByDescending(t => t.DepartureUtc).Select(MapTrip).ToList();
    }

    /// <inheritdoc />
    public async Task UpdateTripAsync(int tripId, CreateLogbookTripDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Reisnaam mag niet leeg zijn.", nameof(dto.Name));

        var entity = await _tripRepo.SingleOrDefaultAsync(t => t.Id == tripId, cancellationToken)
            ?? throw new InvalidOperationException($"Reis met id {tripId} niet gevonden.");

        entity.Update(
            name: dto.Name,
            departureUtc: dto.DepartureUtc,
            arrivalUtc: dto.ArrivalUtc,
            departurePort: dto.DeparturePort,
            destinationPort: dto.DestinationPort,
            vesselName: dto.VesselName,
            crew: dto.Crew,
            notes: dto.Notes,
            logstandStart: dto.LogstandStart,
            loggedMiles: dto.LoggedMiles,
            engineHoursStart: dto.EngineHoursStart,
            engineHoursEnd: dto.EngineHoursEnd,
            fuel: dto.Fuel,
            totalSailingHours: dto.TotalSailingHours,
            logIntervalMinutes: dto.LogIntervalMinutes);

        await _tripRepo.UpdateAsync(entity, cancellationToken);
    }

     /// <inheritdoc />
    public async Task<IReadOnlyList<LogbookEntryDto>> GetEntriesAsync(int tripId, CancellationToken cancellationToken = default)
    {
        var list = await _entryRepo.ListAsync(e => e.LogbookTripId == tripId, cancellationToken);
        var dtos = new List<LogbookEntryDto>();
        foreach (var entry in list.OrderBy(e => e.EntryTimeUtc))
        {
            var dto = await MapEntryAsync(entry, cancellationToken);
            dtos.Add(dto);
        }
        return dtos;
    }

    /// <inheritdoc />
    public async Task<LogbookEntryDto> CreateEntryAsync(int tripId, SaveLogbookEntryDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var entity = new LogbookEntry(
            logbookTripId: tripId,
            entryTimeUtc: dto.EntryTimeUtc,
            baroPressure: dto.BaroPressure,
            logValue: dto.LogValue,
            course: dto.Course,
            remarks: dto.Remarks,
            windDescription: dto.WindDescription,
            gpsStatus: dto.GpsStatus,
            latitude: dto.Latitude,
            longitude: dto.Longitude,
            averageSogKnots: dto.AverageSogKnots);

             await _entryRepo.AddAsync(entity, cancellationToken);
            _logger.LogInformation("Logboekregel aangemaakt met id {EntryId} voor reis {TripId}.", entity.Id, tripId);
            return await MapEntryAsync(entity, cancellationToken);
        }

    /// <inheritdoc />
    public async Task UpdateEntryAsync(int entryId, SaveLogbookEntryDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var entity = await _entryRepo.SingleOrDefaultAsync(e => e.Id == entryId, cancellationToken)
            ?? throw new InvalidOperationException($"Logboekregel met id {entryId} niet gevonden.");

        entity.Update(
            entryTimeUtc: dto.EntryTimeUtc,
            baroPressure: dto.BaroPressure,
            logValue: dto.LogValue,
            course: dto.Course,
            remarks: dto.Remarks,
            windDescription: dto.WindDescription,
            gpsStatus: dto.GpsStatus,
            latitude: dto.Latitude,
            longitude: dto.Longitude,
            averageSogKnots: dto.AverageSogKnots);

        await _entryRepo.UpdateAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task ConfirmEntryAsync(int entryId, CancellationToken cancellationToken = default)
    {
        var entity = await _entryRepo.SingleOrDefaultAsync(e => e.Id == entryId, cancellationToken)
            ?? throw new InvalidOperationException($"Logboekregel met id {entryId} niet gevonden.");

        entity.Confirm();
        await _entryRepo.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Logboekregel {EntryId} geaccordeerd.", entryId);
    }

    /// <inheritdoc />
    public async Task<DateTime> GetNextExpectedLogMomentAsync(int tripId, CancellationToken cancellationToken = default)
    {
        var trip = await _tripRepo.SingleOrDefaultAsync(t => t.Id == tripId, cancellationToken)
            ?? throw new InvalidOperationException($"Reis met id {tripId} niet gevonden.");

        var entries = await _entryRepo.ListAsync(e => e.LogbookTripId == tripId, cancellationToken);

        // Bepaal basis: laatste logboekregel of vertrek
        DateTime baseTime = entries.Count > 0
            ? entries.OrderByDescending(e => e.EntryTimeUtc).First().EntryTimeUtc
            : trip.DepartureUtc;

        // Loginterval: fallback naar 60 als ongeldig
        int interval = trip.LogIntervalMinutes > 0 ? trip.LogIntervalMinutes : 60;

        return baseTime.AddMinutes(interval);
    }

    /// <inheritdoc />
    public async Task<LogbookEntryDto> CreateDraftEntryAsync(int tripId, DateTime entryTimeUtc, CancellationToken cancellationToken = default)
    {
        var trip = await _tripRepo.SingleOrDefaultAsync(t => t.Id == tripId, cancellationToken)
            ?? throw new InvalidOperationException($"Reis met id {tripId} niet gevonden.");

        // Haal meetdatasuggesties op, exclusief oude "laatst bekende" waarden (alleen logtijdvak-data)
        var suggestions = await _suggestionService.GetSuggestionsAsync(tripId, entryTimeUtc, onlyPeriodData: true, cancellationToken);

        // Maak Draft-entry aan met suggesties (handmatige velden: BaroPressure en LogValue blijven null)
        var entity = new LogbookEntry(
            logbookTripId: tripId,
            entryTimeUtc: entryTimeUtc,
            baroPressure: null,
            logValue: null,
            course: suggestions.Course,
            remarks: null,
            windDescription: suggestions.WindDescription,
            gpsStatus: suggestions.GpsStatus,
            latitude: suggestions.Latitude,
            longitude: suggestions.Longitude,
            averageSogKnots: suggestions.AverageSogKnots);

        // Zet expliciet op Draft
        entity.SetDraft();

        await _entryRepo.AddAsync(entity, cancellationToken);
        _logger.LogInformation("Draft logboekregel aangemaakt met id {EntryId} voor reis {TripId}.", entity.Id, tripId);
        return await MapEntryAsync(entity, cancellationToken);
    }

    private static LogbookTripDto MapTrip(LogbookTrip t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        DepartureUtc = t.DepartureUtc,
        ArrivalUtc = t.ArrivalUtc,
        DeparturePort = t.DeparturePort,
        DestinationPort = t.DestinationPort,
        VesselName = t.VesselName,
        Crew = t.Crew,
        Notes = t.Notes,
        LogstandStart = t.LogstandStart,
        LoggedMiles = t.LoggedMiles,
        EngineHoursStart = t.EngineHoursStart,
        EngineHoursEnd = t.EngineHoursEnd,
        Fuel = t.Fuel,
        TotalSailingHours = t.TotalSailingHours,
        LogIntervalMinutes = t.LogIntervalMinutes,
        CreatedAtUtc = t.CreatedAtUtc,
        UpdatedAtUtc = t.UpdatedAtUtc
    };

    /// <inheritdoc />
    public async Task<MissedLogMomentsDto> GetMissedLogMomentsAsync(int tripId, CancellationToken cancellationToken = default)
    {
        var trip = await _tripRepo.SingleOrDefaultAsync(t => t.Id == tripId, cancellationToken)
            ?? throw new InvalidOperationException($"Reis met id {tripId} niet gevonden.");

        var entries = await _entryRepo.ListAsync(e => e.LogbookTripId == tripId, cancellationToken);

        // Bepaal basis: laatste logboekregel of vertrek
        DateTime baseTime = entries.Count > 0
            ? entries.OrderByDescending(e => e.EntryTimeUtc).First().EntryTimeUtc
            : trip.DepartureUtc;

        // Loginterval: fallback naar 60 als ongeldig
        int interval = trip.LogIntervalMinutes > 0 ? trip.LogIntervalMinutes : 60;

        // Bereken alle gemiste logmomenten
        var missedMoments = new List<DateTime>();
        var currentMoment = baseTime.AddMinutes(interval);
        var now = DateTime.UtcNow;

        while (currentMoment < now)
        {
            missedMoments.Add(currentMoment);
            currentMoment = currentMoment.AddMinutes(interval);
        }

        return new MissedLogMomentsDto
        {
            TotalCount = missedMoments.Count,
            MissedMoments = missedMoments.Select(m => new MissedMomentDto { EntryTimeUtc = m }).ToList()
        };
    }

    /// <inheritdoc />
    public async Task<int> CreateMultipleDraftEntriesAsync(int tripId, int maxCount = 24, CancellationToken cancellationToken = default)
    {
        var trip = await _tripRepo.SingleOrDefaultAsync(t => t.Id == tripId, cancellationToken)
            ?? throw new InvalidOperationException($"Reis met id {tripId} niet gevonden.");

        // Haal gemiste logmomenten op
        var missedData = await GetMissedLogMomentsAsync(tripId, cancellationToken);

        if (missedData.TotalCount == 0)
        {
            _logger.LogInformation("Geen gemiste logmomenten voor reis {TripId}.", tripId);
            return 0;
        }

        // Defensief begrenzen tot maxCount
        var momentsToCreate = missedData.MissedMoments
            .Take(Math.Min(maxCount, missedData.TotalCount))
            .ToList();

        int createdCount = 0;

        foreach (var moment in momentsToCreate)
        {
            try
            {
                // Controleer of er al een regel voor dit moment bestaat (defensieve check)
                var existingEntry = await _entryRepo.SingleOrDefaultAsync(
                    e => e.LogbookTripId == tripId && e.EntryTimeUtc == moment.EntryTimeUtc,
                    cancellationToken);

                if (existingEntry != null)
                {
                    _logger.LogWarning("Regel voor {EntryTimeUtc} bestaat al voor reis {TripId}, overgeslagen.", moment.EntryTimeUtc, tripId);
                    continue;
                }

                // Maak Draft-entry aan
                await CreateDraftEntryAsync(tripId, moment.EntryTimeUtc, cancellationToken);
                createdCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij aanmaken Draft-regel voor {EntryTimeUtc} in reis {TripId}.", moment.EntryTimeUtc, tripId);
                // Ga door met volgende moment
            }
        }

        _logger.LogInformation("{CreatedCount} Draft-regels aangemaakt voor reis {TripId}.", createdCount, tripId);
        return createdCount;
    }

    /// <inheritdoc />
    public async Task DeleteEntryAsync(int entryId, CancellationToken cancellationToken = default)
    {
        var entry = await _entryRepo.SingleOrDefaultAsync(e => e.Id == entryId, cancellationToken)
            ?? throw new InvalidOperationException($"Logboekregel met id {entryId} niet gevonden.");

        await _entryRepo.DeleteAsync(entry, cancellationToken);
        _logger.LogInformation("Logboekregel {EntryId} verwijderd.", entryId);
    }

    private async Task<LogbookEntryDto> MapEntryAsync(LogbookEntry e, CancellationToken cancellationToken)
    {
        var attachmentCount = await _attachmentService.GetAttachmentCountAsync(e.Id, cancellationToken);
        return new LogbookEntryDto
        {
            Id = e.Id,
            LogbookTripId = e.LogbookTripId,
            EntryTimeUtc = e.EntryTimeUtc,
            BaroPressure = e.BaroPressure,
            LogValue = e.LogValue,
            Course = e.Course,
            Remarks = e.Remarks,
            WindDescription = e.WindDescription,
            GpsStatus = e.GpsStatus,
            Latitude = e.Latitude,
            Longitude = e.Longitude,
            AverageSogKnots = e.AverageSogKnots,
            Status = e.Status,
            UpdatedAtUtc = e.UpdatedAtUtc,
            AttachmentCount = attachmentCount
        };
    }
}
