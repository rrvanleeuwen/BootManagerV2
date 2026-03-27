namespace BootManager.Application.NetworkMessageInterpretation.Contracts;

using NetworkMessageParsing.DTOs;

/// <summary>
/// Generieke interface voor semantische interpretatie van technische parse-resultaten.
/// 
/// BELANGRIJK: Dit contract werkt BOVENOP de technische parser.
/// De parser levert een technische tussenlaag (<see cref="NetworkMessageParseResultDto"/>) met:
/// - geclassificeerde bericht-typen
/// - geconverteerde payloads in bytes
/// 
/// Deze interface beschrijft hoe een concrete interpreter daarvan een semantische interpretatie maakt.
/// Bijvoorbeeld: Battery-data uit bytes naar spanning en eenheid.
/// 
/// Interpreters worden opgesteld als afzonderlijke implementaties die dezelfde manier werken,
/// zodat meerdere typen (Wind, Depth, Motion, Position) later identiek kunnen worden toegevoegd.
/// </summary>
/// <typeparam name="TInterpretationResult">Het type van het interpretatieresultaat (bijv. BatteryMessageInterpretationDto).</typeparam>
public interface INetworkMessageInterpreter<TInterpretationResult> where TInterpretationResult : class
{
    /// <summary>
    /// Bepaalt of dit parse-resultaat door deze interpreter kan worden geïnterpreteerd.
    /// 
    /// Deze methode moet snel controleren of:
    /// - Het parse-resultaat technisch geslaagd is (IsSuccess == true)
    /// - Het bericht-type geschikt is voor deze interpreter
    /// - De payload minimaal voldoende bytes bevat
    /// 
    /// Als deze methode true retourneert, mag <see cref="Interpret"/> worden aangeroepen.
    /// </summary>
    /// <param name="parseResult">Het technische parse-resultaat van de laag eronder.</param>
    /// <returns>True als dit resultaat door deze interpreter kan worden geïnterpreteerd; anders false.</returns>
    bool CanInterpret(NetworkMessageParseResultDto parseResult);

    /// <summary>
    /// Voert semantische interpretatie uit op een technisch parse-resultaat.
    /// 
    /// Deze methode mag alleen worden aangeroepen nadat <see cref="CanInterpret"/> true heeft geretourneerd.
    /// Echter, de implementatie moet robuust zijn en gecontroleerd fouten retourneren
    /// zonder uitzonderingen voor normale invoerfouten (bijv. onverwacht payload-formaat).
    /// 
    /// Dit converteerd bijvoorbeeld:
    /// - Raw payload-bytes naar domein-waarden (bijv. spanning)
    /// - Voegt contextuele informatie toe (bijv. eenheden, berichtkwaliteit)
    /// - Retourneert een semantisch betekenisvolle interpretatie-DTO
    /// </summary>
    /// <param name="parseResult">Het technische parse-resultaat dat door <see cref="CanInterpret"/> is gevalideerd.</param>
    /// <returns>Het semantische interpretatieresultaat met domein-waarden en/of foutdetails.</returns>
    TInterpretationResult Interpret(NetworkMessageParseResultDto parseResult);
}