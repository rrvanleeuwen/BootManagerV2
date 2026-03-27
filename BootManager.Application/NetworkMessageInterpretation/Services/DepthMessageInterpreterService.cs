namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using NetworkMessageParsing.DTOs;
using NetworkMessageParsing.Enums;

/// <summary>
/// Semantische interpreter voor diepteberichten.
/// 
/// Deze service interpreteert technische parse-resultaten van type Depth
/// naar semantische domein-waarden (diepte in meters).
/// 
/// NMEA 2000-achtige implementatie (gebaseerd op PGN 128267 Water Depth, Rapid Update):
/// - Bytes 0-2: Diepte in 0,01 meter (uint24, little-endian)
/// 
/// Decodelogica:
/// - Diepte in meter = (bytes[0] + bytes[1] * 256 + bytes[2] * 65536) / 100
/// </summary>
public class DepthMessageInterpreterService : INetworkMessageInterpreter<DepthMessageInterpretationDto>
{
    /// <summary>
    /// Bepaalt of dit parse-resultaat door deze interpreter kan worden geïnterpreteerd.
    /// </summary>
    public bool CanInterpret(NetworkMessageParseResultDto parseResult)
    {
        return parseResult.IsSuccess
            && parseResult.MessageType == NetworkMessageType.Depth
            && parseResult.PayloadBytes.Length >= 3;
    }

    /// <summary>
    /// Voert semantische interpretatie uit.
    /// </summary>
    public DepthMessageInterpretationDto Interpret(NetworkMessageParseResultDto parseResult)
    {
        if (!parseResult.IsSuccess)
        {
            return new DepthMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Parse-resultaat is niet succesvol."
            };
        }

        if (parseResult.MessageType != NetworkMessageType.Depth)
        {
            return new DepthMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Bericht-type is {parseResult.MessageType}, geen Depth."
            };
        }

        if (parseResult.PayloadBytes.Length < 3)
        {
            return new DepthMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Onvoldoende bytes in payload. Minimaal 3 bytes vereist."
            };
        }

        try
        {
            // Decodeer diepte van bytes 0-2 (little-endian uint24, 0,01m per eenheid)
            uint depthCentimeters = (uint)(parseResult.PayloadBytes[0] 
                | (parseResult.PayloadBytes[1] << 8) 
                | (parseResult.PayloadBytes[2] << 16));
            decimal depthMeters = depthCentimeters / 100.0m;

            return new DepthMessageInterpretationDto
            {
                IsSuccess = true,
                DepthMeters = depthMeters,
                Unit = "m"
            };
        }
        catch (Exception ex)
        {
            return new DepthMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Onverwachte fout bij decoding: {ex.Message}"
            };
        }
    }
}