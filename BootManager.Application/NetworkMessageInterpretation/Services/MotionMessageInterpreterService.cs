namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using NetworkMessageParsing.DTOs;
using NetworkMessageParsing.Enums;

/// <summary>
/// Semantische interpreter voor bewegingsberichten (koers en snelheid over grond).
/// 
/// Deze service interpreteert technische parse-resultaten van type Motion
/// naar semantische domein-waarden (koers in graden, snelheid in knopen, etc.).
/// 
/// NMEA 2000-achtige implementatie (gebaseerd op PGN 129026 COG/SOG Rapid Update).
/// Dit is GEEN volledige gecertificeerde NMEA 2000-implementatie.
/// 
/// Payload-layout (4 bytes totaal):
/// - Bytes 0-1: Course Over Ground in 1/10000 radians (uint16, little-endian)
///             Bereik: 0 tot 62832 (= 0 tot 2π radialen = 0 tot 360 graden)
///             Conversie naar graden: (bytes_value / 10000) * 180 / π
/// - Bytes 2-3: Speed Over Ground in 0,01 knots (uint16, little-endian)
///             Bereik: 0 tot 655.35 knots
///             Conversie naar knoten: bytes_value / 100
/// 
/// Decodelogica:
/// - COG: Lees bytes 0-1 als uint16 (little-endian), converteert naar radians, dan naar graden
/// - SOG: Lees bytes 2-3 als uint16 (little-endian), deel door 100 voor knopen
/// 
/// Opmerking: Een volledige PGN 129026 kan extra velden bevatten (Magnetic COG, Mode, etc.).
/// Deze interpreter verwerkt alleen de essentiële COG- en SOG-velden.
/// </summary>
public class MotionMessageInterpreterService : INetworkMessageInterpreter<MotionMessageInterpretationDto>
{
    /// <summary>
    /// Bepaalt of dit parse-resultaat door deze interpreter kan worden geïnterpreteerd.
    /// Controleert of:
    /// - Parse-resultaat succesvol is
    /// - Bericht-type Motion is
    /// - Payload minimaal 4 bytes bevat (COG + SOG)
    /// </summary>
    public bool CanInterpret(NetworkMessageParseResultDto parseResult)
    {
        return parseResult.IsSuccess
            && parseResult.MessageType == NetworkMessageType.Motion
            && parseResult.PayloadBytes.Length >= 4;
    }

    /// <summary>
    /// Voert semantische interpretatie uit op een NMEA 2000-achtige PGN 129026 payload.
    /// </summary>
    public MotionMessageInterpretationDto Interpret(NetworkMessageParseResultDto parseResult)
    {
        if (!parseResult.IsSuccess)
        {
            return new MotionMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Parse-resultaat is niet succesvol."
            };
        }

        if (parseResult.MessageType != NetworkMessageType.Motion)
        {
            return new MotionMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Bericht-type is {parseResult.MessageType}, geen Motion."
            };
        }

        if (parseResult.PayloadBytes.Length < 4)
        {
            return new MotionMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Onvoldoende bytes in payload. Minimaal 4 bytes vereist (COG + SOG)."
            };
        }

        try
        {
            // Decodeer Bytes 0-1: COG als uint16 in NMEA 2000-achtige radialen (1e-4 rad eenheden)
            // little-endian: byte 0 is LSB, byte 1 is MSB
            ushort cogNMEA2000 = (ushort)(parseResult.PayloadBytes[0] | (parseResult.PayloadBytes[1] << 8));

            // Converteert van NMEA 2000-radialen (1e-4 rad) naar graden
            // 1 NMEA 2000 radian unit = 1e-4 radialen
            // radialen in echte eenheden = cogNMEA2000 / 10000
            // graden = radialen * 180 / π
            double cogRadians = cogNMEA2000 / 10000.0;
            decimal courseDegrees = (decimal)(cogRadians * 180.0 / Math.PI);

            // Zet naar bereik [0, 360)
            if (courseDegrees < 0)
                courseDegrees += 360;
            if (courseDegrees >= 360)
                courseDegrees = courseDegrees % 360;

            // Decodeer Bytes 2-3: SOG als uint16 in centiknoten
            // little-endian: byte 2 is LSB, byte 3 is MSB
            ushort sogCentiknots = (ushort)(parseResult.PayloadBytes[2] | (parseResult.PayloadBytes[3] << 8));
            decimal speedKnots = sogCentiknots / 100.0m;

            return new MotionMessageInterpretationDto
            {
                IsSuccess = true,
                CourseOverGroundDegrees = courseDegrees,
                SpeedOverGround = speedKnots,
                SpeedUnit = "kn"
            };
        }
        catch (Exception ex)
        {
            return new MotionMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Onverwachte fout bij decoding van Motion-payload: {ex.Message}"
            };
        }
    }
}
