namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using NetworkMessageParsing.DTOs;

/// <summary>
/// Semantische interpreter voor NMEA 0183 VHW sentences (Speed Through Water).
///
/// VHW-veldindeling (0-gebaseerd):
/// [0] Heading True (graden, of leeg)
/// [1] T
/// [2] Heading Magnetic (graden, of leeg)
/// [3] M
/// [4] Speed Through Water in knoten (of leeg)
/// [5] N
/// [6] Speed Through Water in km/h (of leeg)
/// [7] K
///
/// Velden mogen leeg zijn; de interpreter gebruikt knoten als primaire bron
/// en valt terug op km/h als knoten niet beschikbaar zijn.
/// </summary>
public class Nmea0183VhwInterpreterService : INmea0183MessageInterpreter<SpeedThroughWaterMessageInterpretationDto>
{
    private const decimal KnotsToMetersPerSecond = 0.514444m;
    private const decimal KmhToMetersPerSecond = 1m / 3.6m;
    private const decimal MetersPerSecondToKnots = 1.94384m;

    /// <inheritdoc />
    public bool CanInterpret(Nmea0183ParseResultDto parseResult)
    {
        return parseResult.IsSuccess
            && string.Equals(parseResult.SentenceType, "VHW", StringComparison.OrdinalIgnoreCase)
            && parseResult.Fields.Count >= 5;
    }

    /// <inheritdoc />
    public SpeedThroughWaterMessageInterpretationDto Interpret(Nmea0183ParseResultDto parseResult)
    {
        if (!CanInterpret(parseResult))
        {
            return new SpeedThroughWaterMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "VHW-sentence kan niet worden geïnterpreteerd (ongeldig of onvolledig)."
            };
        }

        try
        {
            decimal? speedMps = null;
            decimal? speedKnots = null;

            // Veld [4] = snelheid in knoten
            if (parseResult.Fields.Count > 4
                && decimal.TryParse(parseResult.Fields[4], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var kn)
                && kn >= 0)
            {
                speedKnots = Math.Round(kn, 2);
                speedMps = Math.Round(kn * KnotsToMetersPerSecond, 4);
            }
            // Fallback: veld [6] = snelheid in km/h
            else if (parseResult.Fields.Count > 6
                && decimal.TryParse(parseResult.Fields[6], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var kmh)
                && kmh >= 0)
            {
                speedMps = Math.Round(kmh * KmhToMetersPerSecond, 4);
                speedKnots = Math.Round(speedMps.Value * MetersPerSecondToKnots, 2);
            }

            if (!speedMps.HasValue || !speedKnots.HasValue)
            {
                return new SpeedThroughWaterMessageInterpretationDto
                {
                    IsSuccess = false,
                    ErrorMessage = "VHW: geen geldige snelheidswaarde in veld [4] (knoten) of veld [6] (km/h)."
                };
            }

            return new SpeedThroughWaterMessageInterpretationDto
            {
                IsSuccess = true,
                SpeedMetersPerSecond = speedMps,
                SpeedKnots = speedKnots,
                SpeedWaterReferenceType = 0
            };
        }
        catch (Exception ex)
        {
            return new SpeedThroughWaterMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"VHW-interpretatie mislukt: {ex.Message}"
            };
        }
    }
}
