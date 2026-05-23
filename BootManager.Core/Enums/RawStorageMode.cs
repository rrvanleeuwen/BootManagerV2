namespace BootManager.Core.Enums;

/// <summary>
/// Bepaalt hoe ruwe NMEA-berichten worden opgeslagen.
/// </summary>
public enum RawStorageMode
{
    /// <summary>
    /// Alle ontvangen berichten worden opgeslagen.
    /// </summary>
    All = 0,

    /// <summary>
    /// Berichten worden opgeslagen op basis van een sample-interval.
    /// </summary>
    Sampled = 1,

    /// <summary>
    /// Opslag stopt nadat een bericht succesvol is geparsed.
    /// </summary>
    OffAfterSuccessfulParse = 2
}
