namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using NetworkMessageParsing.DTOs;
using NetworkMessageParsing.Enums;

/// <summary>
/// Semantische interpreter voor watertemperatuur-berichten.
/// 
/// Gebaseerd op PGN 130312 (Temperature / Temperature, Water):
/// - Byte 0:   SID
/// - Byte 1:   Temperature Instance (0 = Sea/Water Temperature)
/// - Bytes 2-3: Temperatuur in 0,01 Kelvin (uint16, little-endian)
/// </summary>
public class WaterTemperatureMessageInterpreterService : INetworkMessageInterpreter<WaterTemperatureMessageInterpretationDto>
{
    private const decimal CentiKelvinToKelvin = 0.01m;
    private const decimal KelvinToCelsiusOffset = 273.15m;

    /// <summary>
    /// Bepaalt of dit parse-resultaat door deze interpreter kan worden geïnterpreteerd.
    /// </summary>
    public bool CanInterpret(NetworkMessageParseResultDto parseResult)
    {
        return parseResult.IsSuccess
            && parseResult.MessageType == NetworkMessageType.WaterTemperature
            && parseResult.PayloadBytes.Length >= 4;
    }

    /// <summary>
    /// Voert semantische interpretatie uit.
    /// </summary>
    public WaterTemperatureMessageInterpretationDto Interpret(NetworkMessageParseResultDto parseResult)
    {
        if (!parseResult.IsSuccess)
        {
            return new WaterTemperatureMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Parse-resultaat is niet succesvol."
            };
        }

        if (parseResult.MessageType != NetworkMessageType.WaterTemperature)
        {
            return new WaterTemperatureMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"Bericht-type is {parseResult.MessageType}, geen WaterTemperature."
            };
        }

        if (parseResult.PayloadBytes.Length < 4)
        {
            return new WaterTemperatureMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "Onvoldoende bytes in payload. Minimaal 4 bytes vereist."
            };
        }

        var bytes = parseResult.PayloadBytes;

        // Byte 0: SID
        byte sid = bytes[0];

        // Byte 1: Temperature Instance
        byte temperatureInstance = bytes[1];

        // Bytes 2-3: Temperatuur in 0,01 Kelvin (uint16 LE)
        ushort rawTemperature = (ushort)(bytes[2] | (bytes[3] << 8));
        decimal temperatureKelvin = rawTemperature * CentiKelvinToKelvin;
        decimal temperatureCelsius = temperatureKelvin - KelvinToCelsiusOffset;

        return new WaterTemperatureMessageInterpretationDto
        {
            IsSuccess = true,
            Sid = sid,
            TemperatureInstance = temperatureInstance,
            TemperatureKelvin = temperatureKelvin,
            TemperatureCelsius = temperatureCelsius
        };
    }
}
