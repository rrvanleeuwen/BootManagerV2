namespace BootManager.Application.NetworkMessageParsing.Enums;

/// <summary>
/// Classificatie van netwerkbericht-typen op basis van bericht-ID.
/// Dit is een technische classification, geen semantische interpretatie van inhoud.
/// </summary>
public enum NetworkMessageType
{
    /// <summary>
    /// Onbekend of niet geclassificeerd berichttype.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Positiegegevens (Message ID: 0A1B2C3D).
    /// </summary>
    Position = 1,

    /// <summary>
    /// Bewegingsgegevens (Message ID: 0A1B2C3E).
    /// </summary>
    Motion = 2,

    /// <summary>
    /// Windgegevens (Message ID: 0A1B2C3F).
    /// </summary>
    Wind = 3,

    /// <summary>
    /// Dieptegegevens (Message ID: 0A1B2C40).
    /// </summary>
    Depth = 4,

    /// <summary>
    /// Batterijstatus (Message ID: 0A1B2C41).
    /// </summary>
    Battery = 5,

    /// <summary>
    /// Koersgegevens (PGN 127250 - Vessel Heading).
    /// </summary>
    Heading = 6
}