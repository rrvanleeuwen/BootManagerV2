namespace BootManager.Application.Logbook.DTOs;

/// <summary>
/// Bevat automatisch voorgestelde meetwaarden voor een nieuwe logboekregel.
/// Punt-in-tijd velden zijn gebaseerd op de laatste meting vóór of op EntryTimeUtc.
/// Periode-aggregaties lopen van de vorige logboekregel (of reisvertrek) tot EntryTimeUtc.
/// Null-waarden geven aan dat er geen meetdata beschikbaar was.
/// </summary>
public class LogbookMeasurementSuggestionDto
{
    /// <summary>
    /// Voorgestelde koers in graden (0-359). Afkomstig van HeadingMeasurement, met fallback naar MotionMeasurement.
    /// </summary>
    public int? Course { get; init; }

    /// <summary>
    /// Voorgestelde windomschrijving, compact geformatteerd (bijv. "45° 5.4 kn").
    /// </summary>
    public string? WindDescription { get; init; }

    /// <summary>
    /// GPS-statusindicator ("OK" als positiemeting beschikbaar is).
    /// </summary>
    public string? GpsStatus { get; init; }

    /// <summary>
    /// Breedtegraad (WGS84, decimaal) afkomstig van de meest recente PositionMeasurement.
    /// </summary>
    public double? Latitude { get; init; }

    /// <summary>
    /// Lengtegraad (WGS84, decimaal) afkomstig van de meest recente PositionMeasurement.
    /// </summary>
    public double? Longitude { get; init; }

    /// <summary>
    /// Gemiddelde SOG in knopen over de logperiode (vorige logregel tot EntryTimeUtc).
    /// </summary>
    public decimal? AverageSogKnots { get; init; }
}
