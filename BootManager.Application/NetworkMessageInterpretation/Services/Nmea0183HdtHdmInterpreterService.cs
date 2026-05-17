namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using NetworkMessageParsing.DTOs;

/// <summary>
/// Semantische interpreter voor NMEA 0183 HDT en HDM sentences (Heading).
///
/// HDT-veldindeling (0-gebaseerd):
/// [0] Koers recht vooruit (True Heading) in graden
/// [1] T (indicator voor True)
///
/// HDM-veldindeling (0-gebaseerd):
/// [0] Magnetische koers (Magnetic Heading) in graden
/// [1] M (indicator voor Magnetic)
///
/// Beide sentence-typen leveren de koers in graden op als enige vereiste waarde.
/// </summary>
public class Nmea0183HdtHdmInterpreterService : INmea0183MessageInterpreter<HeadingMessageInterpretationDto>
{
    /// <inheritdoc />
    public bool CanInterpret(Nmea0183ParseResultDto parseResult)
    {
        if (!parseResult.IsSuccess || parseResult.ChecksumValid == false || parseResult.Fields.Count < 1)
            return false;

        var type = parseResult.SentenceType;
        return string.Equals(type, "HDT", StringComparison.OrdinalIgnoreCase)
            || string.Equals(type, "HDM", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public HeadingMessageInterpretationDto Interpret(Nmea0183ParseResultDto parseResult)
    {
        if (!CanInterpret(parseResult))
        {
            return new HeadingMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "HDT/HDM-sentence kan niet worden geïnterpreteerd (ongeldig of onvolledig)."
            };
        }

        try
        {
            // Veld [0] = koers in graden
            if (!decimal.TryParse(parseResult.Fields[0], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var headingDeg)
                || headingDeg < 0 || headingDeg > 360)
            {
                return new HeadingMessageInterpretationDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"{parseResult.SentenceType}: veld [0] bevat geen geldige koerswaarde: '{parseResult.Fields[0]}'."
                };
            }

            return new HeadingMessageInterpretationDto
            {
                IsSuccess = true,
                HeadingDegrees = Math.Round(headingDeg, 2),
                Unit = "°"
            };
        }
        catch (Exception ex)
        {
            return new HeadingMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"{parseResult.SentenceType}-interpretatie mislukt: {ex.Message}"
            };
        }
    }
}
