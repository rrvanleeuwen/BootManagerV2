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
///
/// Voor handmatig ingevoerde regels (onlyPeriodData=false, default):
/// - Punt-in-tijd velden (Course, Wind, Position) zijn gebaseerd op de meest recente meting vóór of op EntryTimeUtc.
///
/// Voor automatisch gemaakte Draft-regels (onlyPeriodData=true):
/// - Punt-in-tijd velden gebruiken alleen metingen BINNEN het logtijdvak.
/// - Als geen metingen in het logtijdvak beschikbaar zijn, blijven die velden leeg.
///
/// Periode-aggregaties (AverageSogKnots) lopen altijd van vorige logboekregel (of reisvertrek) tot EntryTimeUtc.
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
    public async Task<LogbookMeasurementSuggestionDto> GetSuggestionsAsync(
        int tripId,
        DateTime entryTimeUtc,
        bool onlyPeriodData = false,
        CancellationToken cancellationToken = default)
    {
        // Bepaal de start van de periode: vorige logregel of reisvertrek
        DateTime? periodStart = await BepaalPeriodStartAsync(tripId, entryTimeUtc, cancellationToken);

        // === SEMANTIEK PUNT-IN-TIJD VELDEN (Course, Wind, GPS) ===
        // Voor Draft-regels (onlyPeriodData=true):
        //   Gebruik alleen meetdata BINNEN het logtijdvak (periodStart tot entryTimeUtc).
        //   Als geen data in logtijdvak: veld blijft leeg.
        //   Dit voorkomt misleidende oude waarden.
        //
        // Voor handmatige regels (onlyPeriodData=false):
        //   Gebruik laatst bekende waarde vóór of op het logmoment.
        //   Dit geeft de gebruiker de momentane staat op het moment van loggen.

        int? course = null;
        if (onlyPeriodData && periodStart.HasValue)
        {
            // Draft-regel: alleen metingen BINNEN logtijdvak
            var headingsInPeriod = await _headingRepo.ListAsync(
                h => h.RecordedAtUtc >= periodStart.Value && h.RecordedAtUtc <= entryTimeUtc, cancellationToken);
            var latestHeadingInPeriod = headingsInPeriod.OrderByDescending(h => h.RecordedAtUtc).FirstOrDefault();
            if (latestHeadingInPeriod != null)
            {
                course = (int)Math.Round((double)latestHeadingInPeriod.HeadingDegrees);
            }
            else
            {
                // Fallback: Motion/COG in logtijdvak
                var motionsInPeriod = await _motionRepo.ListAsync(
                    m => m.RecordedAtUtc >= periodStart.Value && m.RecordedAtUtc <= entryTimeUtc, cancellationToken);
                var latestMotionInPeriod = motionsInPeriod.OrderByDescending(m => m.RecordedAtUtc).FirstOrDefault();
                if (latestMotionInPeriod != null)
                    course = (int)Math.Round((double)latestMotionInPeriod.CourseOverGroundDegrees);
            }
        }
        else if (!onlyPeriodData)
        {
            // Handmatige regel: laatst bekende waarde vóór of op entryTimeUtc
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
        }

        // Wind
        string? windDescription = null;
        if (onlyPeriodData && periodStart.HasValue)
        {
            // Draft-regel: alleen metingen BINNEN logtijdvak
            var windsInPeriod = await _windRepo.ListAsync(
                w => w.RecordedAtUtc >= periodStart.Value && w.RecordedAtUtc <= entryTimeUtc, cancellationToken);
            var latestWindInPeriod = windsInPeriod.OrderByDescending(w => w.RecordedAtUtc).FirstOrDefault();
            if (latestWindInPeriod != null)
            {
                var windKnoten = latestWindInPeriod.WindSpeed / 0.514444m;
                windDescription = $"{latestWindInPeriod.WindAngleDegrees:F0}° {windKnoten:F1} kn";
            }
        }
        else if (!onlyPeriodData)
        {
            // Handmatige regel: laatst bekende waarde vóór of op entryTimeUtc
            var winds = await _windRepo.ListAsync(
                w => w.RecordedAtUtc <= entryTimeUtc, cancellationToken);
            var latestWind = winds.OrderByDescending(w => w.RecordedAtUtc).FirstOrDefault();
            if (latestWind != null)
            {
                var windKnoten = latestWind.WindSpeed / 0.514444m;
                windDescription = $"{latestWind.WindAngleDegrees:F0}° {windKnoten:F1} kn";
            }
        }

        // Positie
        string? gpsStatus = null;
        double? latitude = null;
        double? longitude = null;
        if (onlyPeriodData && periodStart.HasValue)
        {
            // Draft-regel: alleen metingen BINNEN logtijdvak
            var positionsInPeriod = await _positionRepo.ListAsync(
                p => p.RecordedAtUtc >= periodStart.Value && p.RecordedAtUtc <= entryTimeUtc, cancellationToken);
            var latestPositionInPeriod = positionsInPeriod.OrderByDescending(p => p.RecordedAtUtc).FirstOrDefault();
            if (latestPositionInPeriod != null)
            {
                gpsStatus = "OK";
                latitude = (double)latestPositionInPeriod.Latitude;
                longitude = (double)latestPositionInPeriod.Longitude;
            }
        }
        else if (!onlyPeriodData)
        {
            // Handmatige regel: laatst bekende waarde vóór of op entryTimeUtc
            var positions = await _positionRepo.ListAsync(
                p => p.RecordedAtUtc <= entryTimeUtc, cancellationToken);
            var latestPosition = positions.OrderByDescending(p => p.RecordedAtUtc).FirstOrDefault();
            if (latestPosition != null)
            {
                gpsStatus = "OK";
                latitude = (double)latestPosition.Latitude;
                longitude = (double)latestPosition.Longitude;
            }
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
    /// Bepaalt de startgrens van het logtijdvak voor periode-aggregaties (bijv. gemiddelde SOG).
    /// Het logtijdvak loopt van de EntryTimeUtc van de vorige logregel (of reisvertrekdatum als geen vorige regel)
    /// tot aan het huidige logmoment (EntryTimeUtc).
    /// Dit zorgt ervoor dat elke logperiode semantisch aansluit bij de logboekregels-hiërarchie.
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
