namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using NetworkMessageParsing.DTOs;

/// <summary>
/// Semantische interpreter voor NMEA 0183 MTW sentences (Water Temperature).
///
/// MTW-veldindeling (0-gebaseerd):
/// [0] Temperatuur in graden Celsius
/// [1] C (eenheidsindicator, altijd Celsius)
/// </summary>
public class Nmea0183MtwInterpreterService : INmea0183MessageInterpreter<WaterTemperatureMessageInterpretationDto>
{
    private const decimal CelsiusToKelvinOffset = 273.15m;

    /// <inheritdoc />
    public bool CanInterpret(Nmea0183ParseResultDto parseResult)
    {
        return parseResult.IsSuccess
            && string.Equals(parseResult.SentenceType, "MTW", StringComparison.OrdinalIgnoreCase)
            && parseResult.Fields.Count >= 1;
    }

    /// <inheritdoc />
    public WaterTemperatureMessageInterpretationDto Interpret(Nmea0183ParseResultDto parseResult)
    {
        if (!CanInterpret(parseResult))
        {
            return new WaterTemperatureMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "MTW-sentence kan niet worden geïnterpreteerd (ongeldig of onvolledig)."
            };
        }

        try
        {
            if (!decimal.TryParse(parseResult.Fields[0], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var celsius))
            {
                return new WaterTemperatureMessageInterpretationDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"MTW: veld [0] bevat geen geldige temperatuurwaarde: '{parseResult.Fields[0]}'."
                };
            }

            var kelvin = Math.Round(celsius + CelsiusToKelvinOffset, 2);
            var celsiusRounded = Math.Round(celsius, 2);

            return new WaterTemperatureMessageInterpretationDto
            {
                IsSuccess = true,
                TemperatureInstance = 0,
                TemperatureCelsius = celsiusRounded,
                TemperatureKelvin = kelvin
            };
        }
        catch (Exception ex)
        {
            return new WaterTemperatureMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"MTW-interpretatie mislukt: {ex.Message}"
            };
        }
    }
}
