namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using NetworkMessageParsing.DTOs;
using System;
using System.Globalization;

/// <summary>
/// Semantische interpreter voor NMEA 0183 RMC sentences (Recommended Minimum Specific GNSS Data).
///
/// RMC-veldindeling (0-gebaseerd):
/// [0] UTC-tijd (hhmmss.ss)
/// [1] Status: A = actief/geldig, V = ongeldig
/// [2] Breedtegraad in ddmm.mmmm
/// [3] N of S (hemisfeerteken breedtegraad)
/// [4] Lengtegraad in dddmm.mmmm
/// [5] E of W (hemisfeerteken lengtegraad)
/// [6] SOG: snelheid over grond in knopen
/// [7] COG: koers over grond in graden
/// [8] Datum (ddmmyy)
/// [9] Magnetische variatie (graden, optioneel)
/// [10] E of W (richting magnetische variatie, optioneel)
///
/// Checksumbeleid: als ChecksumValid == false, wordt geen interpretatie uitgevoerd.
/// Alleen bij status 'A' worden metingen opgeslagen.
/// </summary>
public class Nmea0183RmcInterpreterService : INmea0183MessageInterpreter<Nmea0183RmcInterpretationDto>
{
    /// <inheritdoc />
    public bool CanInterpret(Nmea0183ParseResultDto parseResult)
    {
        return parseResult.IsSuccess
            && string.Equals(parseResult.SentenceType, "RMC", StringComparison.OrdinalIgnoreCase)
            && parseResult.Fields.Count >= 8
            && parseResult.ChecksumValid != false;
    }

    /// <inheritdoc />
    public Nmea0183RmcInterpretationDto Interpret(Nmea0183ParseResultDto parseResult)
    {
        if (!CanInterpret(parseResult))
        {
            return new Nmea0183RmcInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "RMC-sentence kan niet worden geïnterpreteerd (ongeldig, onvolledig of checksum mislukt)."
            };
        }

        try
        {
            // Veld [1]: status
            var status = parseResult.Fields[1].Trim();
            if (!string.Equals(status, "A", StringComparison.OrdinalIgnoreCase))
            {
                return new Nmea0183RmcInterpretationDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"RMC-status niet geldig ('{status}'). Alleen status 'A' wordt opgeslagen."
                };
            }

            var result = new Nmea0183RmcInterpretationDto { IsSuccess = true };

            // Positie: velden [2] (lat) en [3] (N/S), [4] (lon) en [5] (E/W)
            if (TryParseLatitude(parseResult.Fields[2], parseResult.Fields[3], out var latitude)
                && TryParseLongitude(parseResult.Fields[4], parseResult.Fields[5], out var longitude))
            {
                result.Latitude = latitude;
                result.Longitude = longitude;
                result.HasValidPosition = true;
            }

            // Motion: veld [6] (SOG in knoten) en [7] (COG in graden)
            bool hasSog = parseResult.Fields.Count > 6
                && decimal.TryParse(parseResult.Fields[6], NumberStyles.Float, CultureInfo.InvariantCulture, out var sog)
                && sog >= 0;
            bool hasCog = parseResult.Fields.Count > 7
                && decimal.TryParse(parseResult.Fields[7], NumberStyles.Float, CultureInfo.InvariantCulture, out var cog)
                && cog >= 0 && cog < 360;

            if (hasSog && hasCog)
            {
                decimal.TryParse(parseResult.Fields[6], NumberStyles.Float, CultureInfo.InvariantCulture, out var sogVal);
                decimal.TryParse(parseResult.Fields[7], NumberStyles.Float, CultureInfo.InvariantCulture, out var cogVal);
                result.SpeedOverGroundKnots = Math.Round(sogVal, 2);
                result.CourseOverGroundDegrees = Math.Round(cogVal, 2);
                result.HasValidMotion = true;
            }

            return result;
        }
        catch (Exception ex)
        {
            return new Nmea0183RmcInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"RMC-interpretatie mislukt: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Converteert NMEA breedtegraad (ddmm.mmmm) met hemisfeerteken naar decimale graden.
    /// Retourneert false bij ongeldig hemisfeerteken, minuten >= 60 of graden buiten 0..90.
    /// </summary>
    private static bool TryParseLatitude(string rawLat, string hemisphere, out decimal result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(rawLat) || rawLat.Length < 4)
            return false;

        var hem = hemisphere?.Trim().ToUpperInvariant();
        if (hem != "N" && hem != "S")
            return false;

        if (!decimal.TryParse(rawLat, NumberStyles.Float, CultureInfo.InvariantCulture, out var raw))
            return false;

        var degrees = Math.Floor(raw / 100m);
        var minutes = raw - degrees * 100m;

        if (minutes < 0 || minutes >= 60m)
            return false;
        if (degrees < 0 || degrees > 90m)
            return false;
        if (degrees == 90m && minutes != 0m)
            return false;

        result = Math.Round(degrees + minutes / 60m, 7);

        if (hem == "S")
            result = -result;

        return true;
    }

    /// <summary>
    /// Converteert NMEA lengtegraad (dddmm.mmmm) met hemisfeerteken naar decimale graden.
    /// Retourneert false bij ongeldig hemisfeerteken, minuten >= 60 of graden buiten 0..180.
    /// </summary>
    private static bool TryParseLongitude(string rawLon, string hemisphere, out decimal result)
    {
        result = 0;
        if (string.IsNullOrWhiteSpace(rawLon) || rawLon.Length < 5)
            return false;

        var hem = hemisphere?.Trim().ToUpperInvariant();
        if (hem != "E" && hem != "W")
            return false;

        if (!decimal.TryParse(rawLon, NumberStyles.Float, CultureInfo.InvariantCulture, out var raw))
            return false;

        var degrees = Math.Floor(raw / 100m);
        var minutes = raw - degrees * 100m;

        if (minutes < 0 || minutes >= 60m)
            return false;
        if (degrees < 0 || degrees > 180m)
            return false;
        if (degrees == 180m && minutes != 0m)
            return false;

        result = Math.Round(degrees + minutes / 60m, 7);

        if (hem == "W")
            result = -result;

        return true;
    }
}
