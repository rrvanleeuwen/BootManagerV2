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
    private readonly ILogger<LogbookService> _logger;

    /// <summary>
    /// Maakt een nieuwe <see cref="LogbookService"/> aan.
    /// </summary>
    public LogbookService(
        IRepository<LogbookTrip> tripRepo,
        IRepository<LogbookEntry> entryRepo,
        ILogger<LogbookService> logger)
    {
        _tripRepo = tripRepo;
        _entryRepo = entryRepo;
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
            totalSailingHours: dto.TotalSailingHours);

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
            totalSailingHours: dto.TotalSailingHours);

        await _tripRepo.UpdateAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LogbookEntryDto>> GetEntriesAsync(int tripId, CancellationToken cancellationToken = default)
    {
        var list = await _entryRepo.ListAsync(e => e.LogbookTripId == tripId, cancellationToken);
        return list.OrderBy(e => e.EntryTimeUtc).Select(MapEntry).ToList();
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
        return MapEntry(entity);
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
        CreatedAtUtc = t.CreatedAtUtc,
        UpdatedAtUtc = t.UpdatedAtUtc
    };

    private static LogbookEntryDto MapEntry(LogbookEntry e) => new()
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
        UpdatedAtUtc = e.UpdatedAtUtc
    };
}
