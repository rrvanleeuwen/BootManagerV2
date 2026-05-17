namespace BootManager.Application.NetworkMessageInterpretation.DTOs;

/// <summary>
/// Gecombineerd interpretatieresultaat voor NMEA 0183 RMC sentences.
///
/// RMC levert zowel positie- als bewegingsgegevens. Dit DTO bevat beide,
/// zodat de interpreter in één stap positie én motion kan teruggeven.
/// </summary>
public class Nmea0183RmcInterpretationDto
{
    /// <summary>
    /// Geeft aan of de interpretatie succesvol is voltooid.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Geeft aan of de positiegegevens geldig zijn en opgeslagen mogen worden.
    /// </summary>
    public bool HasValidPosition { get; set; }

    /// <summary>
    /// Geeft aan of de bewegingsgegevens geldig zijn en opgeslagen mogen worden.
    /// </summary>
    public bool HasValidMotion { get; set; }

    /// <summary>
    /// De geïnterpreteerde breedtegraad in decimale graden.
    /// Null als de positie niet beschikbaar of ongeldig is.
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// De geïnterpreteerde lengtegraad in decimale graden.
    /// Null als de positie niet beschikbaar of ongeldig is.
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// De koers over grond in decimale graden (0–359.99).
    /// Null als de koers niet beschikbaar of ongeldig is.
    /// </summary>
    public decimal? CourseOverGroundDegrees { get; set; }

    /// <summary>
    /// De snelheid over grond in knopen.
    /// Null als de snelheid niet beschikbaar of ongeldig is.
    /// </summary>
    public decimal? SpeedOverGroundKnots { get; set; }

    /// <summary>
    /// Foutbericht als IsSuccess == false.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
