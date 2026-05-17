namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using NetworkMessageParsing.DTOs;

/// <summary>
/// Semantische interpreter voor NMEA 0183 MWV sentences (Wind Speed and Angle).
///
/// MWV-veldindeling (0-gebaseerd):
/// [0] Windhoek in graden (0-359,9)
/// [1] Reference: R = Relatief (apparent), T = True
/// [2] Windsnelheid (numerieke waarde)
/// [3] Eenheid: K = km/h, M = m/s, N = knoten
/// [4] Status: A = geldig, V = ongeldig
///
/// Alleen sentences met status A worden geïnterpreteerd als geldige meting.
/// Snelheid wordt altijd omgerekend naar m/s als interne eenheid.
/// </summary>
public class Nmea0183MwvInterpreterService : INmea0183MessageInterpreter<WindMessageInterpretationDto>
{
    private const decimal KnotsToMetersPerSecond = 0.514444m;
    private const decimal KmhToMetersPerSecond = 1m / 3.6m;

    /// <inheritdoc />
    public bool CanInterpret(Nmea0183ParseResultDto parseResult)
    {
        return parseResult.IsSuccess
            && parseResult.ChecksumValid != false
            && string.Equals(parseResult.SentenceType, "MWV", StringComparison.OrdinalIgnoreCase)
            && parseResult.Fields.Count >= 5;
    }

    /// <inheritdoc />
    public WindMessageInterpretationDto Interpret(Nmea0183ParseResultDto parseResult)
    {
        if (!CanInterpret(parseResult))
        {
            return new WindMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "MWV-sentence kan niet worden geïnterpreteerd (ongeldig of onvolledig)."
            };
        }

        try
        {
            // Veld [4] = status: A = geldig, V = ongeldig
            var status = parseResult.Fields[4].Trim().ToUpperInvariant();
            if (status != "A")
            {
                return new WindMessageInterpretationDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"MWV: status '{status}' is niet geldig (verwacht A)."
                };
            }

            // Veld [0] = windhoek in graden
            if (!decimal.TryParse(parseResult.Fields[0], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var angleDeg)
                || angleDeg < 0 || angleDeg > 360)
            {
                return new WindMessageInterpretationDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"MWV: veld [0] bevat geen geldige windhoek: '{parseResult.Fields[0]}'."
                };
            }

            // Veld [2] = windsnelheid (numeriek)
            if (!decimal.TryParse(parseResult.Fields[2], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var speedRaw)
                || speedRaw < 0)
            {
                return new WindMessageInterpretationDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"MWV: veld [2] bevat geen geldige windsnelheid: '{parseResult.Fields[2]}'."
                };
            }

            // Veld [3] = eenheid
            var unit = parseResult.Fields[3].Trim().ToUpperInvariant();
            decimal speedMps = unit switch
            {
                "M" => speedRaw,
                "N" => speedRaw * KnotsToMetersPerSecond,
                "K" => speedRaw * KmhToMetersPerSecond,
                _ => -1m
            };

            if (speedMps < 0)
            {
                return new WindMessageInterpretationDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"MWV: onbekende snelheidseenheid '{unit}' in veld [3]."
                };
            }

            return new WindMessageInterpretationDto
            {
                IsSuccess = true,
                WindAngleDegrees = Math.Round(angleDeg, 2),
                WindSpeedMps = Math.Round(speedMps, 4),
                SpeedUnit = "m/s"
            };
        }
        catch (Exception ex)
        {
            return new WindMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"MWV-interpretatie mislukt: {ex.Message}"
            };
        }
    }
}
