namespace BootManager.Application.NetworkMessageInterpretation.Contracts;

using BootManager.Application.NetworkMessageParsing.DTOs;

/// <summary>
/// Generieke interface voor semantische interpretatie van NMEA 0183 parse-resultaten.
///
/// Een implementatie van deze interface interpreteert een specifiek sentence-type
/// (bijv. VHW, MTW, DBT/DPT) en leidt domeinwaarden af uit de ruwe velden.
/// </summary>
/// <typeparam name="TInterpretationResult">
/// Het type van het interpretatieresultaat (bijv. <see cref="DTOs.SpeedThroughWaterMessageInterpretationDto"/>).
/// </typeparam>
public interface INmea0183MessageInterpreter<TInterpretationResult>
    where TInterpretationResult : class
{
    /// <summary>
    /// Bepaalt of dit parse-resultaat door deze interpreter kan worden verwerkt.
    /// Controleert minimaal het sentence-type en het minimale aantal velden.
    /// </summary>
    /// <param name="parseResult">Het technische NMEA 0183 parse-resultaat.</param>
    /// <returns><c>true</c> als <see cref="Interpret"/> mag worden aangeroepen.</returns>
    bool CanInterpret(Nmea0183ParseResultDto parseResult);

    /// <summary>
    /// Leidt domeinwaarden af uit het opgegeven NMEA 0183 parse-resultaat.
    /// </summary>
    /// <param name="parseResult">Het technische NMEA 0183 parse-resultaat.</param>
    /// <returns>Een interpretatieresultaat met de afgeleid domeinwaarden.</returns>
    TInterpretationResult Interpret(Nmea0183ParseResultDto parseResult);
}
