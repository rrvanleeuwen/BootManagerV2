namespace BootManager.Application.NetworkMessageInterpretation.DTOs;

/// <summary>
/// Semantisch interpretatieresultaat voor koersberichten.
/// 
/// Dit DTO vertegenwoordigt de domein-waarden die zijn afgeleid van een technisch parse-resultaat.
/// 
/// NMEA 2000-achtige implementatie (gebaseerd op PGN 127250 Vessel Heading).
/// 
/// Payload-velden die voor deze eerste interpretatie worden gebruikt:
/// - Byte 0: SID (Sequence ID)
/// - Bytes 1-2: Heading in 1/10000 radialen (uint16, little-endian)
/// - Bytes 3-4: Deviation in 1/10000 radialen (niet opgeslagen in deze versie)
/// - Bytes 5-6: Variation in 1/10000 radialen (niet opgeslagen in deze versie)
/// - Byte 7: Reference (directional reference type)
/// </summary>
public class HeadingMessageInterpretationDto
{
    /// <summary>
    /// Geeft aan of de interpretatie succesvol is voltooid.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// De geïnterpreteerde koers in graden (0-360).
    /// Null als IsSuccess == false of als de koers niet kon worden afgeleid.
    /// </summary>
    public decimal? HeadingDegrees { get; set; }

    /// <summary>
    /// De eenheid van de koers. Altijd "°" voor graden.
    /// </summary>
    public string Unit { get; set; } = "°";

    /// <summary>
    /// Foutbericht als de interpretatie mislukt is.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
