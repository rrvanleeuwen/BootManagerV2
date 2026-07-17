using System;
using System.Collections.Generic;
using BootManager.Core.Enums;

namespace BootManager.Application.Logbook.DTOs;

/// <summary>
/// DTO met contextinformatie en geaggregeerde meetdata voor de detailweergave van één logboekregel.
/// </summary>
public class LogbookEntryDetailDto
{
    /// <summary>Unieke identificator van de logboekregel.</summary>
    public int EntryId { get; set; }

    /// <summary>Naam van de bijbehorende reis.</summary>
    public string TripName { get; set; } = string.Empty;

    /// <summary>Tijdstempel (UTC) van de logboekregel.</summary>
    public DateTime EntryTimeUtc { get; set; }

    /// <summary>Status van de logboekregel: Draft ("Concept") of Confirmed ("Akkoord").</summary>
    public bool IsDraft { get; set; }

    /// <summary>Opgeslagen waarden van de logboekregel zelf (de gebruiker ziet wat er in de regel staat).</summary>
    public LogbookSavedEntryValuesDto? SavedValues { get; set; }

    /// <summary>Begintijd (UTC) van de detailperiode. Null als geen geldige start bepaald kon worden.</summary>
    public DateTime? PeriodStartUtc { get; set; }

    /// <summary>Eindtijd (UTC) van de detailperiode (gelijk aan EntryTimeUtc).</summary>
    public DateTime PeriodEndUtc { get; set; }

    /// <summary>Samenvatting van positie binnen de periode.</summary>
    public LogbookDetailSummaryDto<LogbookPositionSampleDto>? Positie { get; set; }

    /// <summary>Samenvatting van COG/SOG (beweging) binnen de periode.</summary>
    public LogbookDetailSummaryDto<LogbookMotionSampleDto>? Beweging { get; set; }

    /// <summary>Samenvatting van heading binnen de periode.</summary>
    public LogbookDetailSummaryDto<LogbookHeadingSampleDto>? Heading { get; set; }

    /// <summary>Samenvatting van wind binnen de periode.</summary>
    public LogbookDetailSummaryDto<LogbookWindSampleDto>? Wind { get; set; }

    /// <summary>Samenvatting van diepte binnen de periode.</summary>
    public LogbookDetailSummaryDto<LogbookDepthSampleDto>? Diepte { get; set; }

    /// <summary>Samenvatting van watertemperatuur binnen de periode.</summary>
    public LogbookDetailSummaryDto<LogbookWaterTempSampleDto>? WaterTemperatuur { get; set; }
}

/// <summary>
/// De opgeslagen waarden van een logboekregel (zoals ingevuld door de gebruiker of voorgesteld via Draft-aanmaak).
/// </summary>
public class LogbookSavedEntryValuesDto
{
    /// <summary>Barometer-stand in hPa.</summary>
    public decimal? BaroPressure { get; set; }

    /// <summary>Logwaarde (afstand door water) in nautische mijlen.</summary>
    public decimal? LogValue { get; set; }

    /// <summary>Koers in graden (0-359).</summary>
    public int? Course { get; set; }

    /// <summary>Opmerkingen (positie, zeilvoering, etc.).</summary>
    public string? Remarks { get; set; }

    /// <summary>Windrichting en -kracht (compact geformatteerd).</summary>
    public string? WindDescription { get; set; }

    /// <summary>GPS-statusindicator.</summary>
    public string? GpsStatus { get; set; }

    /// <summary>Breedtegraad (WGS84, decimaal).</summary>
    public double? Latitude { get; set; }

    /// <summary>Lengtegraad (WGS84, decimaal).</summary>
    public double? Longitude { get; set; }

    /// <summary>Gemiddelde snelheid over grond (SOG) in knopen.</summary>
    public decimal? AverageSogKnots { get; set; }

    /// <summary>Gekozen gebeurtenis (stabiele domeinwaarde). Null indien niet gekozen.</summary>
    public LogbookEventType? EventType { get; set; }

    /// <summary>Gekozen weerconditie (stabiele domeinwaarde). Null indien niet gekozen.</summary>
    public LogbookWeatherCondition? WeatherCondition { get; set; }
}

/// <summary>
/// Compacte samenvatting voor één meettype, inclusief eerste/laatste waarde, gemiddelde en samples.
/// Samples zijn gesorteerd op tijd en beperkt tot maximaal 50 records.
/// </summary>
/// <typeparam name="T">Het sample-type voor dit meettype.</typeparam>
public class LogbookDetailSummaryDto<T>
{
    /// <summary>Eerste sample in de periode.</summary>
    public T? Eerste { get; set; }

    /// <summary>Laatste sample in de periode.</summary>
    public T? Laatste { get; set; }

    /// <summary>Gemiddelde waarde (waar van toepassing). Null als niet berekend.</summary>
    public string? Gemiddelde { get; set; }

    /// <summary>Alle beschikbare samples binnen de periode (max 50, gesorteerd op tijd).</summary>
    public List<T> Samples { get; set; } = new();
}

/// <summary>Positie-sample.</summary>
public class LogbookPositionSampleDto
{
    /// <summary>Tijdstempel (UTC) van de meting.</summary>
    public DateTime TijdUtc { get; set; }

    /// <summary>Breedtegraad (WGS84, decimaal).</summary>
    public decimal Latitude { get; set; }

    /// <summary>Lengtegraad (WGS84, decimaal).</summary>
    public decimal Longitude { get; set; }
}

/// <summary>COG/SOG-sample.</summary>
public class LogbookMotionSampleDto
{
    /// <summary>Tijdstempel (UTC) van de meting.</summary>
    public DateTime TijdUtc { get; set; }

    /// <summary>Koers over grond in graden.</summary>
    public decimal CogDegrees { get; set; }

    /// <summary>Snelheid over grond in knopen.</summary>
    public decimal SogKnots { get; set; }
}

/// <summary>Heading-sample.</summary>
public class LogbookHeadingSampleDto
{
    /// <summary>Tijdstempel (UTC) van de meting.</summary>
    public DateTime TijdUtc { get; set; }

    /// <summary>Koers in graden.</summary>
    public decimal HeadingDegrees { get; set; }
}

/// <summary>Wind-sample.</summary>
public class LogbookWindSampleDto
{
    /// <summary>Tijdstempel (UTC) van de meting.</summary>
    public DateTime TijdUtc { get; set; }

    /// <summary>Windhoek in graden.</summary>
    public decimal WindAngleDegrees { get; set; }

    /// <summary>Windsnelheid in knopen.</summary>
    public decimal WindSpeedKnots { get; set; }
}

/// <summary>Diepte-sample.</summary>
public class LogbookDepthSampleDto
{
    /// <summary>Tijdstempel (UTC) van de meting.</summary>
    public DateTime TijdUtc { get; set; }

    /// <summary>Diepte in meters.</summary>
    public decimal DepthMeters { get; set; }
}

/// <summary>Watertemperatuur-sample.</summary>
public class LogbookWaterTempSampleDto
{
    /// <summary>Tijdstempel (UTC) van de meting.</summary>
    public DateTime TijdUtc { get; set; }

    /// <summary>Watertemperatuur in graden Celsius.</summary>
    public decimal TemperatuurCelsius { get; set; }
}
