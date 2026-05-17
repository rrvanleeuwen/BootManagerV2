namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using NetworkMessageParsing.DTOs;

/// <summary>
/// Semantische interpreter voor NMEA 0183 DBT en DPT sentences (Depth).
///
/// DBT-veldindeling (0-gebaseerd):
/// [0] Diepte in voet       [1] f
/// [2] Diepte in meters     [3] M
/// [4] Diepte in vadem      [5] F
///
/// DPT-veldindeling (0-gebaseerd):
/// [0] Diepte in meters (t.o.v. transducer)
/// [1] Offset transducer (positief = boven kiel, negatief = onder kiel)
///
/// Voorkeur: meters (veld [2] voor DBT, veld [0] voor DPT).
/// Fallback voor DBT: voet omrekenen naar meters als meters-veld leeg is.
/// </summary>
public class Nmea0183DbtDptInterpreterService : INmea0183MessageInterpreter<DepthMessageInterpretationDto>
{
    private const decimal FeetToMeters = 0.3048m;

    /// <inheritdoc />
    public bool CanInterpret(Nmea0183ParseResultDto parseResult)
    {
        if (!parseResult.IsSuccess)
            return false;

        var type = parseResult.SentenceType;
        return (string.Equals(type, "DBT", StringComparison.OrdinalIgnoreCase) && parseResult.Fields.Count >= 1)
            || (string.Equals(type, "DPT", StringComparison.OrdinalIgnoreCase) && parseResult.Fields.Count >= 1);
    }

    /// <inheritdoc />
    public DepthMessageInterpretationDto Interpret(Nmea0183ParseResultDto parseResult)
    {
        if (!CanInterpret(parseResult))
        {
            return new DepthMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "DBT/DPT-sentence kan niet worden geïnterpreteerd (ongeldig of onvolledig)."
            };
        }

        try
        {
            decimal? depthMeters = null;
            var type = parseResult.SentenceType.ToUpperInvariant();

            if (type == "DPT")
            {
                // Veld [0] = diepte in meters
                if (!string.IsNullOrWhiteSpace(parseResult.Fields[0])
                    && decimal.TryParse(parseResult.Fields[0], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var dpt)
                    && dpt >= 0)
                {
                    depthMeters = Math.Round(dpt, 2);
                }
            }
            else // DBT
            {
                // Veld [2] = diepte in meters (prefereer)
                if (parseResult.Fields.Count > 2
                    && !string.IsNullOrWhiteSpace(parseResult.Fields[2])
                    && decimal.TryParse(parseResult.Fields[2], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var dbtM)
                    && dbtM >= 0)
                {
                    depthMeters = Math.Round(dbtM, 2);
                }
                // Fallback: veld [0] = voet → meters
                else if (!string.IsNullOrWhiteSpace(parseResult.Fields[0])
                    && decimal.TryParse(parseResult.Fields[0], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var dbtFt)
                    && dbtFt >= 0)
                {
                    depthMeters = Math.Round(dbtFt * FeetToMeters, 2);
                }
            }

            if (!depthMeters.HasValue)
            {
                return new DepthMessageInterpretationDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"{type}: geen geldige dieptewaarde gevonden in de sentence."
                };
            }

            return new DepthMessageInterpretationDto
            {
                IsSuccess = true,
                DepthMeters = depthMeters,
                Unit = "m"
            };
        }
        catch (Exception ex)
        {
            return new DepthMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"DBT/DPT-interpretatie mislukt: {ex.Message}"
            };
        }
    }
}
