namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using NetworkMessageParsing.DTOs;
using NetworkMessageParsing.Enums;

/// <summary>
/// Semantische interpreter voor batterij-statusberichten.
/// 
/// Deze service interpreteert technische parse-resultaten van type Battery
/// naar semantische domein-waarden (spanning, SOC, etc.).
/// 
/// NMEA 2000-achtige implementatie (gebaseerd op PGN 127508):
/// - Byte 0: Battery Instance (0x00 voor eerste batterij)
/// - Bytes 1-2: Voltage in 0,01V (uint16, little-endian)
/// - Byte 3: State of Charge in % (0-100, of 0xFF voor unknown)
/// 
/// Decodelogica:
/// - Spanning in V = (bytes[1] + bytes[2] * 256) / 100
/// - SOC in % = bytes[3] (direct, null als 0xFF)
/// </summary>
public class BatteryMessageInterpreterService : INetworkMessageInterpreter<BatteryMessageInterpretationDto>
{
    private const byte UnknownSocValue = 0xFF;

    /// <summary>
    /// Bepaalt of dit parse-resultaat door deze interpreter kan worden geïnterpreteerd.
    /// </summary>
    public bool CanInterpret(NetworkMessageParseResultDto parseResult)
    {
        return parseResult.IsSuccess
            && parseResult.MessageType == NetworkMessageType.Battery
            && parseResult.PayloadBytes.Length >= 4;
    }

    /// <summary>
    /// Voert semantische interpretatie uit.
    /// </summary>
    public BatteryMessageInterpretationDto Interpret(NetworkMessageParseResultDto parseResult)
    {
        if (!parseResult.IsSuccess)
        {
            return new BatteryMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Parse-resultaat is niet succesvol."
            };
        }

        if (parseResult.MessageType != NetworkMessageType.Battery)
        {
            return new BatteryMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Bericht-type is {parseResult.MessageType}, geen Battery."
            };
        }

        if (parseResult.PayloadBytes.Length < 4)
        {
            return new BatteryMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Onvoldoende bytes in payload. Minimaal 4 bytes vereist."
            };
        }

        try
        {
            // Decodeer spanning van bytes 1-2 (little-endian uint16, 0,01V per eenheid)
            ushort voltageCentivolts = (ushort)(parseResult.PayloadBytes[1] | (parseResult.PayloadBytes[2] << 8));
            decimal voltageVolts = voltageCentivolts / 100.0m;

            // Decodeer State of Charge van byte 3
            byte soc = parseResult.PayloadBytes[3];
            int? socValue = soc == UnknownSocValue ? null : (int?)soc;

            return new BatteryMessageInterpretationDto
            {
                IsSuccess = true,
                Voltage = voltageVolts,
                Unit = "V",
                StateOfCharge = socValue
            };
        }
        catch (Exception ex)
        {
            return new BatteryMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Onverwachte fout bij decoding: {ex.Message}"
            };
        }
    }
}