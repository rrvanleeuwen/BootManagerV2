using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BootManager.Application.Logbook.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;

namespace BootManager.Application.Logbook.Services;

/// <summary>
/// Haalt automatische meetdata-suggesties op voor een logboekregel.
/// Punt-in-tijd velden zijn gebaseerd op de meest recente meting vóór of op EntryTimeUtc.
/// Periode-aggregaties lopen van de vorige logboekregel (of reisvertrek) tot EntryTimeUtc.
/// </summary>
public class LogbookMeasurementSuggestionService : ILogbookMeasurementSuggestionService
{
    private readonly IRepository<HeadingMeasurement> _headingRepo;
    private readonly IRepository<MotionMeasurement> _motionRepo;
    private readonly IRepository<WindMeasurement> _windRepo;
    private readonly IRepository<PositionMeasurement> _positionRepo;
    private readonly IRepository<LogbookEntry> _entryRepo;
    private readonly IRepository<LogbookTrip> _tripRepo;

    /// <summary>
    /// Initialiseert de service met de benodigde repositories.
    /// </summary>
    public LogbookMeasurementSuggestionService(
        IRepository<HeadingMeasurement> headingRepo,
        IRepository<MotionMeasurement> motionRepo,
        IRepository<WindMeasurement> windRepo,
        IRepository<PositionMeasurement> positionRepo,
        IRepository<LogbookEntry> entryRepo,
        IRepository<LogbookTrip> tripRepo)
    {
        _headingRepo = headingRepo;
        _motionRepo = motionRepo;
        _windRepo = windRepo;
        _positionRepo = positionRepo;
        _entryRepo = entryRepo;
        _tripRepo = tripRepo;
    }

    /// <inheritdoc />
    public async Task<LogbookMeasurementSuggestionDto> GetSuggestionsAsync(int tripId, DateTime entryTimeUtc, CancellationToken cancellationToken = default)
    {
        // Bepaal de start van de periode: vorige logregel of reisvertrek
        DateTime? periodStart = await BepaalPeriodStartAsync(tripId, entryTimeUtc, cancellationToken);

        // Koers: voorkeur HeadingMeasurement, fallback MotionMeasurement (punt-in-tijd)
        int? course = null;
        var headings = await _headingRepo.ListAsync(
            h => h.RecordedAtUtc <= entryTimeUtc, cancellationToken);
        var latestHeading = headings.OrderByDescending(h => h.RecordedAtUtc).FirstOrDefault();
        if (latestHeading != null)
        {
            course = (int)Math.Round((double)latestHeading.HeadingDegrees);
        }
        else
        {
            var motions = await _motionRepo.ListAsync(
                m => m.RecordedAtUtc <= entryTimeUtc, cancellationToken);
            var latestMotion = motions.OrderByDescending(m => m.RecordedAtUtc).FirstOrDefault();
            if (latestMotion != null)
                course = (int)Math.Round((double)latestMotion.CourseOverGroundDegrees);
        }

        // Wind (punt-in-tijd)
        string? windDescription = null;
        var winds = await _windRepo.ListAsync(
            w => w.RecordedAtUtc <= entryTimeUtc, cancellationToken);
        var latestWind = winds.OrderByDescending(w => w.RecordedAtUtc).FirstOrDefault();
        if (latestWind != null)
        {
            var windKnoten = latestWind.WindSpeed / 0.514444m;
            windDescription = $"{latestWind.WindAngleDegrees:F0}° {windKnoten:F1} kn";
        }

        // Positie (punt-in-tijd)
        string? gpsStatus = null;
        double? latitude = null;
        double? longitude = null;
        var positions = await _positionRepo.ListAsync(
            p => p.RecordedAtUtc <= entryTimeUtc, cancellationToken);
        var latestPosition = positions.OrderByDescending(p => p.RecordedAtUtc).FirstOrDefault();
        if (latestPosition != null)
        {
            gpsStatus = "OK";
            latitude = (double)latestPosition.Latitude;
            longitude = (double)latestPosition.Longitude;
        }

        // Gemiddelde SOG over de periode (alleen als periodStart bekend is)
        decimal? averageSogKnots = null;
        if (periodStart.HasValue)
        {
            var sogMetingen = await _motionRepo.ListAsync(
                m => m.RecordedAtUtc >= periodStart.Value && m.RecordedAtUtc <= entryTimeUtc, cancellationToken);
            if (sogMetingen.Count > 0)
            {
                // SpeedOverGround is opgeslagen in knopen (zie MotionMeasurement)
                averageSogKnots = Math.Round(sogMetingen.Average(m => m.SpeedOverGround), 1);
            }
        }

        return new LogbookMeasurementSuggestionDto
        {
            Course = course,
            WindDescription = windDescription,
            GpsStatus = gpsStatus,
            Latitude = latitude,
            Longitude = longitude,
            AverageSogKnots = averageSogKnots
        };
    }

    /// <summary>
    /// Bepaalt de startgrens van de logperiode:
    /// de EntryTimeUtc van de vorige logregel, of anders de reisvertrekdatum.
    /// </summary>
    private async Task<DateTime?> BepaalPeriodStartAsync(int tripId, DateTime entryTimeUtc, CancellationToken cancellationToken)
    {
        var vorigeRegels = await _entryRepo.ListAsync(
            e => e.LogbookTripId == tripId && e.EntryTimeUtc < entryTimeUtc, cancellationToken);
        var vorigeRegel = vorigeRegels.OrderByDescending(e => e.EntryTimeUtc).FirstOrDefault();
        if (vorigeRegel != null)
            return vorigeRegel.EntryTimeUtc;

        // Fallback: reisvertrekdatum
        var trip = await _tripRepo.SingleOrDefaultAsync(t => t.Id == tripId, cancellationToken);
        if (trip != null && trip.DepartureUtc <= entryTimeUtc)
            return trip.DepartureUtc;

        return null;
    }
}
