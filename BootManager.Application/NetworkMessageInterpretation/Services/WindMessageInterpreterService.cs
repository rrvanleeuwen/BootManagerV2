namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using NetworkMessageParsing.DTOs;
using NetworkMessageParsing.Enums;

/// <summary>
/// Semantische interpreter voor windberichten.
/// 
/// Deze service interpreteert technische parse-resultaten van type Wind
/// naar semantische domein-waarden (windhoek in graden, windsnelheid in m/s).
/// 
/// NMEA 2000-achtige implementatie (gebaseerd op PGN 130306 Wind Data):
/// - Bytes 0-1: Wind Speed in 0,01 m/s (uint16, little-endian)
/// - Bytes 2-3: Wind Angle in 1/10000 radians (uint16, little-endian)
/// 
/// Decodelogica:
/// - Wind Speed m/s = (bytes[0] + bytes[1] * 256) / 100
/// - Wind Angle graden = NMEA2000 radialen (1e-4 rad) * 180 / π
/// </summary>
public class WindMessageInterpreterService : INetworkMessageInterpreter<WindMessageInterpretationDto>
{
    /// <summary>
    /// Bepaalt of dit parse-resultaat door deze interpreter kan worden geïnterpreteerd.
    /// </summary>
    public bool CanInterpret(NetworkMessageParseResultDto parseResult)
    {
        return parseResult.IsSuccess
            && parseResult.MessageType == NetworkMessageType.Wind
            && parseResult.PayloadBytes.Length >= 4;
    }

    /// <summary>
    /// Voert semantische interpretatie uit.
    /// </summary>
    public WindMessageInterpretationDto Interpret(NetworkMessageParseResultDto parseResult)
    {
        if (!parseResult.IsSuccess)
        {
            return new WindMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Parse-resultaat is niet succesvol."
            };
        }

        if (parseResult.MessageType != NetworkMessageType.Wind)
        {
            return new WindMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Bericht-type is {parseResult.MessageType}, geen Wind."
            };
        }

        if (parseResult.PayloadBytes.Length < 4)
        {
            return new WindMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Onvoldoende bytes in payload. Minimaal 4 bytes vereist."
            };
        }

        try
        {
            // Decodeer windsnelheid van bytes 0-1 (little-endian uint16, 0,01 m/s per eenheid)
            ushort windSpeedCentiMps = (ushort)(parseResult.PayloadBytes[0] 
                | (parseResult.PayloadBytes[1] << 8));
            decimal windSpeedMps = windSpeedCentiMps / 100.0m;

            // Decodeer windhoek van bytes 2-3 (little-endian uint16, 1e-4 radialen)
            ushort windAngleRadiansScaled = (ushort)(parseResult.PayloadBytes[2] 
                | (parseResult.PayloadBytes[3] << 8));
            
            // Converteer van 1e-4 radialen naar radialen, dan naar graden
            double windAngleRadians = windAngleRadiansScaled / 10000.0;
            double windAngleDegrees = windAngleRadians * (180.0 / Math.PI);
            
            // Zorg dat de hoek in het bereik 0-360 ligt
            windAngleDegrees = windAngleDegrees % 360.0;
            if (windAngleDegrees < 0)
            {
                windAngleDegrees += 360.0;
            }

            return new WindMessageInterpretationDto
            {
                IsSuccess = true,
                WindAngleDegrees = (decimal)windAngleDegrees,
                AngleUnit = "°",
                WindSpeedMps = windSpeedMps,
                SpeedUnit = "m/s"
            };
        }
        catch (Exception ex)
        {
            return new WindMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Onverwachte fout bij decoding: {ex.Message}"
            };
        }
    }
}