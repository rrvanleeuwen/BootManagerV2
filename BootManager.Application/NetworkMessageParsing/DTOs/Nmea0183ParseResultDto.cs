namespace BootManager.Application.NetworkMessageParsing.DTOs;

/// <summary>
/// DTO voor het resultaat van het technisch parseren van een NMEA 0183 sentence.
///
/// Dit is een TECHNISCHE parse-output, GEEN semantisch geïnterpreteerd domeinobject.
/// Waarden zoals windsnelheid of koers worden hier niet afgeleid.
/// Verdere interpretatie vindt plaats in Fase 3 interpreter-services.
/// </summary>
public class Nmea0183ParseResultDto
{
    /// <summary>
    /// Geeft aan of het parseren geslaagd is.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// De volledige onbewerkte NMEA 0183 sentence zoals ontvangen.
    /// </summary>
    public string RawSentence { get; set; } = string.Empty;

    /// <summary>
    /// De talker-prefix van de sentence (bijv. "II", "GP", "HC", "WI").
    /// Leeg als de sentence niet herkend kon worden.
    /// </summary>
    public string TalkerPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Het sentence-type zonder talker-prefix (bijv. "VHW", "GGA", "RMC", "MWV").
    /// Leeg als de sentence niet herkend kon worden.
    /// </summary>
    public string SentenceType { get; set; } = string.Empty;

    /// <summary>
    /// De geëxtraheerde velden uit de sentence (kommagescheiden inhoud na het sentence-id, voor de checksum).
    /// Lege lijst als het parseren mislukt is.
    /// </summary>
    public IReadOnlyList<string> Fields { get; set; } = [];

    /// <summary>
    /// Geeft aan of de checksum aanwezig en correct was.
    /// Null als de sentence geen checksum bevat.
    /// </summary>
    public bool? ChecksumValid { get; set; }

    /// <summary>
    /// Foutmelding bij mislukt parseren. Leeg bij succes.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
