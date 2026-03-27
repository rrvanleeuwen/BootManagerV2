namespace BootManager.Application.NetworkMessageInterpretation.DTOs;

/// <summary>
/// Semantisch interpretatieresultaat voor windberichten.
/// 
/// Dit DTO vertegenwoordigt de domein-waarden die zijn afgeleid van een technisch parse-resultaat.
/// 
/// NMEA 2000-achtige implementatie (gebaseerd op PGN 130306 Wind Data).
/// De huidige implementatie is gebaseerd op de simulatie-aanpak en zal uitgebreid moeten worden
/// zodra volledige NMEA 2000-ondersteuning wordt toegevoegd.
/// 
/// Payload-velden die voor deze eerste interpretatie worden gebruikt:
/// - Bytes 0-1: Wind Speed in 0,01 m/s (uint16, little-endian)
/// - Bytes 2-3: Wind Angle in 1/10000 radians (uint16, little-endian)
/// </summary>
public class WindMessageInterpretationDto
{
    /// <summary>
    /// Geeft aan of de interpretatie succesvol is voltooid.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// De geïnterpreteerde windhoek in graden (0-360).
    /// Null als IsSuccess == false of als de hoek niet kon worden afgeleid.
    /// </summary>
    public decimal? WindAngleDegrees { get; set; }

    /// <summary>
    /// De eenheid van de windhoek. Altijd "°" voor graden.
    /// </summary>
    public string AngleUnit { get; set; } = "°";

    /// <summary>
    /// De geïnterpreteerde windsnelheid in m/s.
    /// Null als IsSuccess == false of als de snelheid niet kon worden afgeleid.
    /// </summary>
    public decimal? WindSpeedMps { get; set; }

    /// <summary>
    /// De eenheid van de windsnelheid. Altijd "m/s" voor meter per seconde.
    /// </summary>
    public string SpeedUnit { get; set; } = "m/s";

    /// <summary>
    /// Foutbericht als IsSuccess == false.
    /// Beschrijft waarom de interpretatie niet kon worden voltooid.
    /// </summary>
    public string? ErrorMessage { get; set; }
}