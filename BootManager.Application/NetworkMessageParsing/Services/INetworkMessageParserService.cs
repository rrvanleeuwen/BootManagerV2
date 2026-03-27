namespace BootManager.Application.NetworkMessageParsing.Services;

using DTOs;

/// <summary>
/// Biedt functionaliteit voor het herkennen en technisch parseren van onbewerkte netwerkberichten.
/// 
/// Dit is een LOW-LEVEL parseer-service die:
/// - bericht-IDs herkent en classificeert naar <see cref="Enums.NetworkMessageType"/>
/// - hexadecimale payloads converteert naar bytes
/// - GEEN domein-gerelateerde decoding of interpretatie uitvoert
/// 
/// Dit is een tussenstap. Verdere interpretatie, validatie en decoding vindt plaats via <see cref="INetworkMessageInterpreter{TInterpretationResult}"/>.
/// </summary>
public interface INetworkMessageParserService
{
    /// <summary>
    /// Parseert een netwerkbericht op basis van de gegeven aanvraag.
    /// 
    /// Voert GEEN semantische interpretatie uit. Dit resultaat is pure technische parse-output:
    /// - bericht-ID-herkenning en classificatie naar <see cref="Enums.NetworkMessageType"/>
    /// - hex-to-byte conversie
    /// 
    /// Waarden zoals windsnelheid, windrichting, diepte of snelheid worden hier niet afgeleid.
    /// </summary>
    /// <param name="request">De parseer-aanvraag met onbewerkte netwerkgegevens.</param>
    /// <returns>Het technische parse-resultaat met gestructureerde bericht-informatie of foutdetails. Dit is nog geen domeinobject.</returns>
    NetworkMessageParseResultDto Parse(NetworkMessageParseRequestDto request);
}