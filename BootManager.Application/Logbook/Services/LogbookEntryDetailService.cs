using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BootManager.Application.Logbook.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Core.Interfaces;

namespace BootManager.Application.Logbook.Services;

/// <summary>
/// Implementatie van <see cref="ILogbookEntryDetailService"/>.
/// Laadt meetdata binnen de detailperiode van een logboekregel.
///
/// Periode-afbakening:
/// - start = EntryTimeUtc van de vorige logboekregel in dezelfde reis
/// - als er geen vorige regel is: DepartureUtc van de reis
/// - einde = EntryTimeUtc van de gekozen logboekregel
///
/// Samplestrategie:
/// - Alle beschikbare records binnen het tijdvak worden opgehaald.
/// - Per meettype worden maximaal 50 samples teruggegeven, gesorteerd op tijd.
/// - Als er meer dan 50 records zijn, wordt uniform gesampleld (elke N-de record).
/// </summary>
public class LogbookEntryDetailService : ILogbookEntryDetailService
{
    private const int MaxSamples = 50;

    private readonly IRepository<LogbookEntry> _entryRepo;
    private readonly IRepository<LogbookTrip> _tripRepo;
    private readonly IRepository<PositionMeasurement> _positionRepo;
    private readonly IRepository<MotionMeasurement> _motionRepo;
    private readonly IRepository<HeadingMeasurement> _headingRepo;
    private readonly IRepository<WindMeasurement> _windRepo;
    private readonly IRepository<DepthMeasurement> _depthRepo;
    private readonly IRepository<WaterTemperatureMeasurement> _waterTempRepo;

    /// <summary>
    /// Initialiseert de service met de benodigde repositories.
    /// </summary>
    public LogbookEntryDetailService(
        IRepository<LogbookEntry> entryRepo,
        IRepository<LogbookTrip> tripRepo,
        IRepository<PositionMeasurement> positionRepo,
        IRepository<MotionMeasurement> motionRepo,
        IRepository<HeadingMeasurement> headingRepo,
        IRepository<WindMeasurement> windRepo,
        IRepository<DepthMeasurement> depthRepo,
        IRepository<WaterTemperatureMeasurement> waterTempRepo)
    {
        _entryRepo = entryRepo;
        _tripRepo = tripRepo;
        _positionRepo = positionRepo;
        _motionRepo = motionRepo;
        _headingRepo = headingRepo;
        _windRepo = windRepo;
        _depthRepo = depthRepo;
        _waterTempRepo = waterTempRepo;
    }

    /// <inheritdoc />
    public async Task<LogbookEntryDetailDto?> GetEntryDetailAsync(int entryId, CancellationToken cancellationToken = default)
    {
        var entry = await _entryRepo.SingleOrDefaultAsync(e => e.Id == entryId, cancellationToken);
        if (entry == null)
            return null;

        var trip = await _tripRepo.SingleOrDefaultAsync(t => t.Id == entry.LogbookTripId, cancellationToken);
        if (trip == null)
            return null;

        DateTime entryTime = entry.EntryTimeUtc;
        DateTime? periodStart = await BepaalPeriodStartAsync(entry.LogbookTripId, entryTime, trip, cancellationToken);

        var dto = new LogbookEntryDetailDto
        {
            EntryId = entry.Id,
            TripName = trip.Name,
            EntryTimeUtc = entryTime,
            IsDraft = entry.Status == LogbookEntryStatus.Draft,
            PeriodStartUtc = periodStart,
            PeriodEndUtc = entryTime,
            SavedValues = MapToSavedEntryValuesDto(entry)
        };

        // Laad periode-samples
            if (periodStart.HasValue)
            {
                var start = periodStart.Value;
                var end = entryTime;

                dto.Positie = await BouwPositieSamenvattingAsync(start, end, cancellationToken);
                dto.Beweging = await BouwBewegingSamenvattingAsync(start, end, cancellationToken);
                dto.Heading = await BouwHeadingSamenvattingAsync(start, end, cancellationToken);
                dto.Wind = await BouwWindSamenvattingAsync(start, end, cancellationToken);
                dto.Diepte = await BouwDiepteSamenvattingAsync(start, end, cancellationToken);
                dto.WaterTemperatuur = await BouwWaterTempSamenvattingAsync(start, end, cancellationToken);
            }

        return dto;
    }

    /// <summary>
    /// Mapt de opgeslagen waarden van een LogbookEntry naar een SavedEntryValuesDto.
    /// </summary>
    private LogbookSavedEntryValuesDto MapToSavedEntryValuesDto(LogbookEntry entry)
    {
        return new LogbookSavedEntryValuesDto
        {
            BaroPressure = entry.BaroPressure,
            LogValue = entry.LogValue,
            Course = entry.Course,
            Remarks = entry.Remarks,
            WindDescription = entry.WindDescription,
            GpsStatus = entry.GpsStatus,
            Latitude = entry.Latitude,
            Longitude = entry.Longitude,
            AverageSogKnots = entry.AverageSogKnots,
            EventType = entry.EventType,
            WeatherCondition = entry.WeatherCondition
        };
    }

    /// <summary>
    /// Bepaalt de startgrens van de periode: vorige logboekregel of reisvertrek.
    /// </summary>
    private async Task<DateTime?> BepaalPeriodStartAsync(
        int tripId, DateTime entryTimeUtc, LogbookTrip trip, CancellationToken cancellationToken)
    {
        var vorigeRegels = await _entryRepo.ListAsync(
            e => e.LogbookTripId == tripId && e.EntryTimeUtc < entryTimeUtc, cancellationToken);
        var vorigeRegel = vorigeRegels.OrderByDescending(e => e.EntryTimeUtc).FirstOrDefault();
        if (vorigeRegel != null)
            return vorigeRegel.EntryTimeUtc;

        if (trip.DepartureUtc <= entryTimeUtc)
            return trip.DepartureUtc;

        return null;
    }

    private async Task<LogbookDetailSummaryDto<LogbookPositionSampleDto>?> BouwPositieSamenvattingAsync(
        DateTime start, DateTime end, CancellationToken ct)
    {
        var records = await _positionRepo.ListAsync(
            p => p.RecordedAtUtc >= start && p.RecordedAtUtc <= end, ct);
        if (records.Count == 0) return null;

        var gesorteerd = records.OrderBy(p => p.RecordedAtUtc).ToList();
        var samples = Sampel(gesorteerd, p => new LogbookPositionSampleDto
        {
            TijdUtc = p.RecordedAtUtc,
            Latitude = p.Latitude,
            Longitude = p.Longitude
        });

        return new LogbookDetailSummaryDto<LogbookPositionSampleDto>
        {
            Eerste = samples.First(),
            Laatste = samples.Last(),
            Samples = samples
        };
    }

    private async Task<LogbookDetailSummaryDto<LogbookMotionSampleDto>?> BouwBewegingSamenvattingAsync(
        DateTime start, DateTime end, CancellationToken ct)
    {
        var records = await _motionRepo.ListAsync(
            m => m.RecordedAtUtc >= start && m.RecordedAtUtc <= end, ct);
        if (records.Count == 0) return null;

        var gesorteerd = records.OrderBy(m => m.RecordedAtUtc).ToList();
        var samples = Sampel(gesorteerd, m => new LogbookMotionSampleDto
        {
            TijdUtc = m.RecordedAtUtc,
            CogDegrees = m.CourseOverGroundDegrees,
            SogKnots = m.SpeedOverGround // opgeslagen in knopen
        });

        var gemSog = Math.Round(gesorteerd.Average(m => m.SpeedOverGround), 1);

        return new LogbookDetailSummaryDto<LogbookMotionSampleDto>
        {
            Eerste = samples.First(),
            Laatste = samples.Last(),
            Gemiddelde = $"{gemSog:F1} kn",
            Samples = samples
        };
    }

    private async Task<LogbookDetailSummaryDto<LogbookHeadingSampleDto>?> BouwHeadingSamenvattingAsync(
        DateTime start, DateTime end, CancellationToken ct)
    {
        var records = await _headingRepo.ListAsync(
            h => h.RecordedAtUtc >= start && h.RecordedAtUtc <= end, ct);
        if (records.Count == 0) return null;

        var gesorteerd = records.OrderBy(h => h.RecordedAtUtc).ToList();
        var samples = Sampel(gesorteerd, h => new LogbookHeadingSampleDto
        {
            TijdUtc = h.RecordedAtUtc,
            HeadingDegrees = h.HeadingDegrees
        });

        return new LogbookDetailSummaryDto<LogbookHeadingSampleDto>
        {
            Eerste = samples.First(),
            Laatste = samples.Last(),
            Samples = samples
        };
    }

    private async Task<LogbookDetailSummaryDto<LogbookWindSampleDto>?> BouwWindSamenvattingAsync(
        DateTime start, DateTime end, CancellationToken ct)
    {
        var records = await _windRepo.ListAsync(
            w => w.RecordedAtUtc >= start && w.RecordedAtUtc <= end, ct);
        if (records.Count == 0) return null;

        var gesorteerd = records.OrderBy(w => w.RecordedAtUtc).ToList();
        var samples = Sampel(gesorteerd, w => new LogbookWindSampleDto
        {
            TijdUtc = w.RecordedAtUtc,
            WindAngleDegrees = w.WindAngleDegrees,
            // WindSpeed is opgeslagen in m/s, converteren naar knopen
            WindSpeedKnots = Math.Round(w.WindSpeed / 0.514444m, 1)
        });

        var gemKnoten = Math.Round(gesorteerd.Average(w => w.WindSpeed) / 0.514444m, 1);

        return new LogbookDetailSummaryDto<LogbookWindSampleDto>
        {
            Eerste = samples.First(),
            Laatste = samples.Last(),
            Gemiddelde = $"{gemKnoten:F1} kn",
            Samples = samples
        };
    }

    private async Task<LogbookDetailSummaryDto<LogbookDepthSampleDto>?> BouwDiepteSamenvattingAsync(
        DateTime start, DateTime end, CancellationToken ct)
    {
        var records = await _depthRepo.ListAsync(
            d => d.RecordedAtUtc >= start && d.RecordedAtUtc <= end, ct);
        if (records.Count == 0) return null;

        var gesorteerd = records.OrderBy(d => d.RecordedAtUtc).ToList();
        var samples = Sampel(gesorteerd, d => new LogbookDepthSampleDto
        {
            TijdUtc = d.RecordedAtUtc,
            DepthMeters = d.DepthMeters
        });

        var gemDiepte = Math.Round(gesorteerd.Average(d => d.DepthMeters), 1);

        return new LogbookDetailSummaryDto<LogbookDepthSampleDto>
        {
            Eerste = samples.First(),
            Laatste = samples.Last(),
            Gemiddelde = $"{gemDiepte:F1} m",
            Samples = samples
        };
    }

    private async Task<LogbookDetailSummaryDto<LogbookWaterTempSampleDto>?> BouwWaterTempSamenvattingAsync(
        DateTime start, DateTime end, CancellationToken ct)
    {
        var records = await _waterTempRepo.ListAsync(
            w => w.RecordedAtUtc >= start && w.RecordedAtUtc <= end, ct);
        if (records.Count == 0) return null;

        var gesorteerd = records.OrderBy(w => w.RecordedAtUtc).ToList();
        var samples = Sampel(gesorteerd, w => new LogbookWaterTempSampleDto
        {
            TijdUtc = w.RecordedAtUtc,
            TemperatuurCelsius = w.TemperatureCelsius
        });

        var gemTemp = Math.Round(gesorteerd.Average(w => w.TemperatureCelsius), 1);

        return new LogbookDetailSummaryDto<LogbookWaterTempSampleDto>
        {
            Eerste = samples.First(),
            Laatste = samples.Last(),
            Gemiddelde = $"{gemTemp:F1} °C",
            Samples = samples
        };
    }

    /// <summary>
    /// Sampelt een gesorteerde lijst naar maximaal <see cref="MaxSamples"/> items.
    /// Bij meer dan MaxSamples records wordt uniform gesampleld (elke N-de record).
    /// </summary>
    private static List<TOut> Sampel<TIn, TOut>(List<TIn> gesorteerd, Func<TIn, TOut> map)
    {
        if (gesorteerd.Count <= MaxSamples)
            return gesorteerd.Select(map).ToList();

        var result = new List<TOut>(MaxSamples);
        double stap = (double)(gesorteerd.Count - 1) / (MaxSamples - 1);
        for (int i = 0; i < MaxSamples; i++)
        {
            int index = (int)Math.Round(i * stap);
            result.Add(map(gesorteerd[index]));
        }
        return result;
    }
}
