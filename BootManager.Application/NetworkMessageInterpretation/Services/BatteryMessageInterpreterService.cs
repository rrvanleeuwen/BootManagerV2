namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using NetworkMessageParsing.DTOs;
using NetworkMessageParsing.Enums;

/// <summary>
/// Semantische interpreter voor batterij-statusberichten.
/// 
/// Deze service interpreteert technische parse-resultaten van type Battery
/// naar semantische domein-waarden (spanning, eenheid, etc.).
/// 
/// BELANGRIJK: Deze implementatie is specifiek voor het huidige simulatorformaat.
/// De decodelogica werkt als volgt:
/// - Vereist minimaal 2 bytes in de payload
/// - Eerste byte (MSB) en tweede byte (LSB) worden gecombineerd tot een uint16
/// - Deze waarde wordt geïnterpreteerd als spanning in decivolt (0,1V per eenheid)
/// - Daarom: spanning in V = (uint16) / 10
/// 
/// Dit is GEEN NMEA2000-standaard-decoder. Als volledige NMEA2000 wordt geïmplementeerd,
/// zal deze logica moeten worden vervangen of herzien.
/// </summary>
public class BatteryMessageInterpreterService : INetworkMessageInterpreter<BatteryMessageInterpretationDto>
{
    /// <inheritdoc />
    public bool CanInterpret(NetworkMessageParseResultDto parseResult)
    {
        // Interpreter kan alleen technisch succesvolle Battery-berichten verwerken
        return parseResult.IsSuccess
            && parseResult.MessageType == NetworkMessageType.Battery
            && parseResult.PayloadBytes.Length >= 2;
    }

    /// <inheritdoc />
    public BatteryMessageInterpretationDto Interpret(NetworkMessageParseResultDto parseResult)
    {
        // Minimale validatie: als CanInterpret false zou zijn, dit is defensief
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

        if (parseResult.PayloadBytes.Length < 2)
        {
            return new BatteryMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Onvoldoende bytes in payload. Minimaal 2 bytes vereist."
            };
        }

        try
        {
            // Decodeer spanning van eerste twee bytes (big-endian uint16)
            // Formule: spanning in V = (first_byte * 256 + second_byte) / 10
            // Dit geeft spanning in decivolt-eenheden
            ushort voltageDecivolts = (ushort)((parseResult.PayloadBytes[0] << 8) | parseResult.PayloadBytes[1]);
            decimal voltageVolts = voltageDecivolts / 10.0m;

            return new BatteryMessageInterpretationDto
            {
                IsSuccess = true,
                Voltage = voltageVolts,
                Unit = "V"
            };
        }
        catch (Exception ex)
        {
            // Dit moet normaal niet gebeuren omdat we al hebben gevalideerd, maar defensief
            return new BatteryMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Onverwachte fout bij decoding: {ex.Message}"
            };
        }
    }
}