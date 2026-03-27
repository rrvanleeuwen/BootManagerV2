namespace BootManager.Application.NetworkMessageParsing.DTOs;

using Enums;

/// <summary>
/// DTO voor het resultaat van netwerkbericht-parsing.
/// 
/// BELANGRIJK: Dit resultaat is een TECHNISCHE parse-output, GEEN semantisch geïnterpreteerd domeinobject.
/// Dit is een tussenstap in de verwerkingspijplijn. De parser:
/// - herkent bericht-IDs en classified berichten
/// - converteert hexadecimale payloads naar bytes
/// - voert GEEN domein-gerelateerde decoding uit
/// 
/// Waarden zoals windsnelheid, windrichting, diepte of snelheid worden hier nog NIET afgeleid.
/// Verdere interpretatie, validatie en decoding van de payload vindt plaats in latere verwerkingsstappen.
/// </summary>
public class NetworkMessageParseResultDto
{
    /// <summary>
    /// Geeft aan of het parsen geslaagd is.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Het geclassificeerde type van het netwerkbericht op basis van bericht-ID.
    /// Dit is een technische classificatie, geen semantische interpretatie van payload-inhoud.
    /// </summary>
    public NetworkMessageType MessageType { get; set; }

    /// <summary>
    /// De bericht-ID in hexadecimale notatie.
    /// </summary>
    public string MessageIdHex { get; set; } = string.Empty;

    /// <summary>
    /// De berichtlading als byte-array (geconverteerd van hexadecimale notatie).
    /// Dit is onbewerkte payload-data, nog niet geïnterpreteerd voor domein-waarden.
    /// </summary>
    public byte[] PayloadBytes { get; set; } = [];

    /// <summary>
    /// Foutbericht wanneer parsing mislukt (null bij succes).
    /// </summary>
    public string? ErrorMessage { get; set; }
}