namespace BootManager.Application.NetworkMessageInterpretation.DTOs;

/// <summary>
/// Semantisch interpretatieresultaat voor positieberichten.
/// 
/// Dit DTO vertegenwoordigt de domein-waarden die zijn afgeleid van een technisch parse-resultaat.
/// 
/// NMEA 2000-achtige implementatie (gebaseerd op PGN 129025 Position, Rapid Update).
/// Zodra volledige NMEA 2000-ondersteuning wordt toegevoegd, zal dit vervangen of uitgebreid moeten worden.
/// </summary>
public class PositionMessageInterpretationDto
{
    /// <summary>
    /// Geeft aan of de interpretatie succesvol is voltooid.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// De geïnterpreteerde breedtegraad in decimale graden.
    /// Null als IsSuccess == false of als de breedtegraad niet kon worden afgeleid.
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// De geïnterpreteerde lengtegraad in decimale graden.
    /// Null als IsSuccess == false of als de lengtegraad niet kon worden afgeleid.
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// De eenheid van de coördinaten. Altijd "degrees" voor decimale graden.
    /// </summary>
    public string Unit { get; set; } = "degrees";

    /// <summary>
    /// Foutbericht als IsSuccess == false.
    /// Beschrijft waarom de interpretatie niet kon worden voltooid.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
