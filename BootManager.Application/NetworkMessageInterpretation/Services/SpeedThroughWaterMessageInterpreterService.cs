namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using NetworkMessageParsing.DTOs;
using NetworkMessageParsing.Enums;

/// <summary>
/// Semantische interpreter voor snelheid-door-water-berichten.
/// 
/// Gebaseerd op PGN 128259 (Speed Through Water / Speed, Water Referenced):
/// - Byte 0:   SID
/// - Bytes 1-2: Snelheid in 0,01 m/s (uint16, little-endian)
/// - Byte 3:   Speed Water Reference Type
/// </summary>
public class SpeedThroughWaterMessageInterpreterService : INetworkMessageInterpreter<SpeedThroughWaterMessageInterpretationDto>
{
    private const decimal CentiMetersPerSecondToMetersPerSecond = 0.01m;
    private const decimal MetersPerSecondToKnots = 1.94384m;

    /// <summary>
    /// Bepaalt of dit parse-resultaat door deze interpreter kan worden geïnterpreteerd.
    /// </summary>
    public bool CanInterpret(NetworkMessageParseResultDto parseResult)
    {
        return parseResult.IsSuccess
            && parseResult.MessageType == NetworkMessageType.SpeedThroughWater
            && parseResult.PayloadBytes.Length >= 4;
    }

    /// <summary>
    /// Voert semantische interpretatie uit.
    /// </summary>
    public SpeedThroughWaterMessageInterpretationDto Interpret(NetworkMessageParseResultDto parseResult)
    {
        if (!parseResult.IsSuccess)
        {
            return new SpeedThroughWaterMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Parse-resultaat is niet succesvol."
            };
        }

        if (parseResult.MessageType != NetworkMessageType.SpeedThroughWater)
        {
            return new SpeedThroughWaterMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Bericht-type is {parseResult.MessageType}, geen SpeedThroughWater."
            };
        }

        if (parseResult.PayloadBytes.Length < 4)
        {
            return new SpeedThroughWaterMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Onvoldoende bytes in payload. Minimaal 4 bytes vereist."
            };
        }

        try
        {
            byte sid = parseResult.PayloadBytes[0];

            // Bytes 1-2: snelheid in 0,01 m/s (uint16, little-endian)
            ushort rawSpeed = (ushort)(parseResult.PayloadBytes[1] | (parseResult.PayloadBytes[2] << 8));
            decimal speedMps = rawSpeed * CentiMetersPerSecondToMetersPerSecond;
            decimal speedKnots = Math.Round(speedMps * MetersPerSecondToKnots, 2);

            byte referenceType = parseResult.PayloadBytes[3];

            return new SpeedThroughWaterMessageInterpretationDto
            {
                IsSuccess = true,
                Sid = sid,
                SpeedMetersPerSecond = speedMps,
                SpeedKnots = speedKnots,
                SpeedWaterReferenceType = referenceType
            };
        }
        catch (Exception ex)
        {
            return new SpeedThroughWaterMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Onverwachte fout bij decoding: {ex.Message}"
            };
        }
    }
}
