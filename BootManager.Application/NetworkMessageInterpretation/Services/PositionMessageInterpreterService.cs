namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using NetworkMessageParsing.DTOs;
using NetworkMessageParsing.Enums;

/// <summary>
/// Semantische interpreter voor positieberichten.
/// 
/// Deze service interpreteert technische parse-resultaten van type Position
/// naar semantische domein-waarden (breedtegraad, lengtegraad, etc.).
/// 
/// NMEA 2000-achtige implementatie (gebaseerd op PGN 129025):
/// - Bytes 0-3: Latitude in 1e-7 degrees (int32, little-endian)
/// - Bytes 4-7: Longitude in 1e-7 degrees (int32, little-endian)
/// 
/// Decodelogica:
/// - Breedtegraad = bytes[0-3] / 1e7
/// - Lengtegraad = bytes[4-7] / 1e7
/// </summary>
public class PositionMessageInterpreterService : INetworkMessageInterpreter<PositionMessageInterpretationDto>
{
    /// <summary>
    /// Bepaalt of dit parse-resultaat door deze interpreter kan worden geïnterpreteerd.
    /// </summary>
    public bool CanInterpret(NetworkMessageParseResultDto parseResult)
    {
        return parseResult.IsSuccess
            && parseResult.MessageType == NetworkMessageType.Position
            && parseResult.PayloadBytes.Length >= 8;
    }

    /// <summary>
    /// Voert semantische interpretatie uit.
    /// </summary>
    public PositionMessageInterpretationDto Interpret(NetworkMessageParseResultDto parseResult)
    {
        if (!parseResult.IsSuccess)
        {
            return new PositionMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Parse-resultaat is niet succesvol."
            };
        }

        if (parseResult.MessageType != NetworkMessageType.Position)
        {
            return new PositionMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Bericht-type is {parseResult.MessageType}, geen Position."
            };
        }

        if (parseResult.PayloadBytes.Length < 8)
        {
            return new PositionMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Onvoldoende bytes in payload. Minimaal 8 bytes vereist."
            };
        }

        try
        {
            // Decodeer latitude van bytes 0-3 (little-endian int32, 1e-7 graden per eenheid)
            int latitudeInt = BitConverter.ToInt32(parseResult.PayloadBytes, 0);
            decimal latitudeDegrees = latitudeInt / 1e7m;

            // Decodeer longitude van bytes 4-7 (little-endian int32, 1e-7 graden per eenheid)
            int longitudeInt = BitConverter.ToInt32(parseResult.PayloadBytes, 4);
            decimal longitudeDegrees = longitudeInt / 1e7m;

            return new PositionMessageInterpretationDto
            {
                IsSuccess = true,
                Latitude = latitudeDegrees,
                Longitude = longitudeDegrees,
                Unit = "degrees"
            };
        }
        catch (Exception ex)
        {
            return new PositionMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Decodering van positie mislukt: {ex.Message}"
            };
        }
    }
}
