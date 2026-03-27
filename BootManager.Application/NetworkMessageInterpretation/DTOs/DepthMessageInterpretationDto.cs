namespace BootManager.Application.NetworkMessageInterpretation.DTOs;

/// <summary>
/// Semantisch interpretatieresultaat voor diepteberichten.
/// 
/// Dit DTO vertegenwoordigt de domein-waarden die zijn afgeleid van een technisch parse-resultaat.
/// 
/// NMEA 2000-achtige implementatie (gebaseerd op PGN 128267 Water Depth, Rapid Update).
/// De huidige implementatie is gebaseerd op de simulatie-aanpak en zal uitgebreid moeten worden
/// zodra volledige NMEA 2000-ondersteuning wordt toegevoegd.
/// 
/// Payload-velden die voor deze eerste interpretatie worden gebruikt:
/// - Bytes 0-2: Diepte in 0,01 meter (uint24, little-endian)
/// </summary>
public class DepthMessageInterpretationDto
{
    /// <summary>
    /// Geeft aan of de interpretatie succesvol is voltooid.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// De geïnterpreteerde diepte in meters.
    /// Null als IsSuccess == false of als de diepte niet kon worden afgeleid.
    /// </summary>
    public decimal? DepthMeters { get; set; }

    /// <summary>
    /// De eenheid van de diepte. Altijd "m" voor meters.
    /// </summary>
    public string Unit { get; set; } = "m";

    /// <summary>
    /// Foutbericht als IsSuccess == false.
    /// Beschrijft waarom de interpretatie niet kon worden voltooid.
    /// </summary>
    public string? ErrorMessage { get; set; }
}