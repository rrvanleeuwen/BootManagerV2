namespace BootManager.Application.NetworkMessageParsing.Services;

using DTOs;

/// <summary>
/// Biedt functionaliteit voor het technisch parseren van NMEA 0183 sentences.
///
/// De parser:
/// - herkent de sentence-structuur ($talker+type,veld1,veld2,...*checksum)
/// - negeert de talker-prefix bij type-herkenning (bijv. "IIVHW" → type "VHW")
/// - valideert basisstructuur en optionele checksum
/// - extraheert velden als string-array
/// - voert GEEN semantische interpretatie uit
/// </summary>
public interface INmea0183ParserService
{
    /// <summary>
    /// Parseert een NMEA 0183 sentence technisch.
    ///
    /// Voert GEEN semantische interpretatie uit. Het resultaat bevat:
    /// - talker-prefix en sentence-type
    /// - geëxtraheerde velden
    /// - checksum-validatie (indien aanwezig)
    /// </summary>
    /// <param name="rawSentence">De onbewerkte NMEA 0183 sentence (bijv. "$IIVHW,,,,,5.3,N,,K*53").</param>
    /// <returns>Het technische parse-resultaat.</returns>
    Nmea0183ParseResultDto Parse(string rawSentence);
}
