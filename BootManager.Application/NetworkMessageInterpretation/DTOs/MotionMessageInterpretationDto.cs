namespace BootManager.Application.NetworkMessageInterpretation.DTOs;

/// <summary>
/// Semantisch interpretatieresultaat voor bewegingsberichten.
/// 
/// Dit DTO vertegenwoordigt de domein-waarden die zijn afgeleid van een technisch parse-resultaat.
/// 
/// NMEA 2000-achtige implementatie (gebaseerd op PGN 129026 COG/SOG).
/// Zodra volledige NMEA 2000-ondersteuning wordt toegevoegd, zal dit vervangen of uitgebreid moeten worden.
/// </summary>
public class MotionMessageInterpretationDto
{
    /// <summary>
    /// Geeft aan of de interpretatie succesvol is voltooid.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// De geïnterpreteerde koers over grond in graden (0-359,99).
    /// Null als IsSuccess == false of als de koers niet kon worden afgeleid.
    /// </summary>
    public decimal? CourseOverGroundDegrees { get; set; }

    /// <summary>
    /// De geïnterpreteerde snelheid over grond.
    /// Null als IsSuccess == false of als de snelheid niet kon worden afgeleid.
    /// </summary>
    public decimal? SpeedOverGround { get; set; }

    /// <summary>
    /// De eenheid van de snelheid (bijv. "kn" voor knopen).
    /// </summary>
    public string SpeedUnit { get; set; } = "kn";

    /// <summary>
    /// Foutbericht als IsSuccess == false.
    /// Beschrijft waarom de interpretatie niet kon worden voltooid.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
