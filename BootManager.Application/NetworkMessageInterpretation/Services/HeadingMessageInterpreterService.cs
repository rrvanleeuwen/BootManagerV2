namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using NetworkMessageParsing.DTOs;
using NetworkMessageParsing.Enums;

/// <summary>
/// Semantische interpreter voor koersberichten.
/// 
/// Deze service interpreteert technische parse-resultaten van type Heading
/// naar semantische domein-waarden (koers in graden).
/// 
/// NMEA 2000-achtige implementatie (gebaseerd op PGN 127250 Vessel Heading).
/// Dit is GEEN volledige gecertificeerde NMEA 2000-implementatie.
/// 
/// Payload-layout (8 bytes totaal):
/// - Byte 0: SID (Sequence ID) voor bericht-volgordenummering
/// - Bytes 1-2: Heading in 1/10000 radialen (uint16, little-endian)
///             Bereik: 0 tot 62832 (= 0 tot 2π radialen = 0 tot 360 graden)
///             Conversie naar graden: (bytes_value / 10000) * 180 / π
/// - Bytes 3-4: Deviation in 1/10000 radialen (uint16, little-endian)
///             Voor nu niet opgeslagen; kan later uitgebreid worden
/// - Bytes 5-6: Variation in 1/10000 radialen (uint16, little-endian)
///             Voor nu niet opgeslagen; kan later uitgebreid worden
/// - Byte 7: Reference (bits 0-1: 00=True, 01=Magnetic)
///             Voor nu niet opgeslagen; kan later uitgebreid worden
/// 
/// Decodelogica:
/// - Heading: Lees bytes 1-2 als uint16 (little-endian), converteer naar radialen, dan naar graden
/// </summary>
public class HeadingMessageInterpreterService : INetworkMessageInterpreter<HeadingMessageInterpretationDto>
{
    /// <summary>
    /// Bepaalt of dit parse-resultaat door deze interpreter kan worden geïnterpreteerd.
    /// Controleert of:
    /// - Parse-resultaat succesvol is
    /// - Bericht-type Heading is
    /// - Payload minimaal 3 bytes bevat (SID + Heading)
    /// </summary>
    public bool CanInterpret(NetworkMessageParseResultDto parseResult)
    {
        return parseResult.IsSuccess
            && parseResult.MessageType == NetworkMessageType.Heading
            && parseResult.PayloadBytes.Length >= 3;
    }

    /// <summary>
    /// Voert semantische interpretatie uit op een NMEA 2000-achtige PGN 127250 payload.
    /// </summary>
    public HeadingMessageInterpretationDto Interpret(NetworkMessageParseResultDto parseResult)
    {
        if (!parseResult.IsSuccess)
        {
            return new HeadingMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Parse-resultaat is niet succesvol."
            };
        }

        if (parseResult.MessageType != NetworkMessageType.Heading)
        {
            return new HeadingMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Bericht-type is {parseResult.MessageType}, geen Heading."
            };
        }

        if (parseResult.PayloadBytes.Length < 3)
        {
            return new HeadingMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Onvoldoende bytes in payload. Minimaal 3 bytes vereist (SID + Heading)."
            };
        }

        try
        {
            // Byte 0 is SID; niet gebruikt voor deze interpretatie maar aanwezig in payload

            // Decodeer Bytes 1-2: Heading als uint16 in NMEA 2000-achtige radialen (1e-4 rad eenheden)
            // little-endian: byte 1 is LSB, byte 2 is MSB
            ushort headingNMEA2000 = (ushort)(parseResult.PayloadBytes[1] | (parseResult.PayloadBytes[2] << 8));

            // Converteert van NMEA 2000-radialen (1e-4 rad) naar graden
            // 1 NMEA 2000 radian unit = 1e-4 radialen
            // radialen in echte eenheden = headingNMEA2000 / 10000
            // graden = radialen * 180 / π
            double headingRadians = headingNMEA2000 / 10000.0;
            decimal headingDegrees = (decimal)(headingRadians * 180.0 / Math.PI);

            // Zet naar bereik [0, 360)
            if (headingDegrees < 0)
                headingDegrees += 360;
            if (headingDegrees >= 360)
                headingDegrees = headingDegrees % 360;

            return new HeadingMessageInterpretationDto
            {
                IsSuccess = true,
                HeadingDegrees = headingDegrees,
                Unit = "°"
            };
        }
        catch (Exception ex)
        {
            return new HeadingMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Fout bij interpretatie van koersbericht: {ex.Message}"
            };
        }
    }
}
