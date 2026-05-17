namespace BootManager.Application.NetworkMessageInterpretation.Services;

using Contracts;
using DTOs;
using NetworkMessageParsing.DTOs;
using System;
using System.Globalization;

/// <summary>
/// Semantische interpreter voor NMEA 0183 GGA sentences (Global Positioning System Fix Data).
///
/// GGA-veldindeling (0-gebaseerd):
/// [0] UTC-tijd (hhmmss.ss)
/// [1] Breedtegraad in ddmm.mmmm
/// [2] N of S (hemisfeerteken breedtegraad)
/// [3] Lengtegraad in dddmm.mmmm
/// [4] E of W (hemisfeerteken lengtegraad)
/// [5] GPS-fixkwaliteit: 0 = ongeldig, 1 = GPS, 2 = DGPS, enz.
/// [6] Aantal satellieten in gebruik
/// [7] HDOP (horizontal dilution of precision)
/// [8] Hoogte boven gemiddeld zeeniveau (meter)
/// [9] M (eenheid hoogte)
/// [10] Hoogte geoid boven WGS84 ellipsoïde (meter)
/// [11] M (eenheid geoid hoogte)
///
/// Checksumbeleid: als ChecksumValid == false, wordt geen interpretatie uitgevoerd.
/// Alleen bij fixkwaliteit > 0 worden metingen opgeslagen.
/// Hoogte, satellieten en fix-details worden in fase 3c niet opgeslagen.
/// </summary>
public class Nmea0183GgaInterpreterService : INmea0183MessageInterpreter<PositionMessageInterpretationDto>
{
    /// <inheritdoc />
    public bool CanInterpret(Nmea0183ParseResultDto parseResult)
    {
        return parseResult.IsSuccess
            && string.Equals(parseResult.SentenceType, "GGA", StringComparison.OrdinalIgnoreCase)
            && parseResult.Fields.Count >= 6
            && parseResult.ChecksumValid != false;
    }

    /// <inheritdoc />
    public PositionMessageInterpretationDto Interpret(Nmea0183ParseResultDto parseResult)
    {
        if (!CanInterpret(parseResult))
        {
            return new PositionMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = "GGA-sentence kan niet worden geïnterpreteerd (ongeldig, onvolledig of checksum mislukt)."
            };
        }

        try
        {
            // Veld [5]: fixkwaliteit – verplicht aanwezig, numeriek parsebaar en > 0
            if (string.IsNullOrWhiteSpace(parseResult.Fields[5])
                || !int.TryParse(parseResult.Fields[5], out var fixQuality)
                || fixQuality <= 0)
            {
                return new PositionMessageInterpretationDto
                {
                    IsSuccess = false,
                    ErrorMessage = $"GGA-fixkwaliteit ontbreekt of ongeldig ('{parseResult.Fields[5]}'). Alleen fixkwaliteit > 0 wordt opgeslagen."
                };
            }

            // Positie: velden [1] (lat), [2] (N/S), [3] (lon), [4] (E/W)
            if (!TryParseLatitude(parseResult.Fields[1], parseResult.Fields[2], out var latitude))
            {
                return new PositionMessageInterpretationDto
                {
                    IsSuccess = false,
                    ErrorMessage = "GGA: breedtegraad kon niet worden geparseerd."
                };
            }

            if (!TryParseLongitude(parseResult.Fields[3], parseResult.Fields[4], out var longitude))
            {
                return new PositionMessageInterpretationDto
                {
                    IsSuccess = false,
                    ErrorMessage = "GGA: lengtegraad kon niet worden geparseerd."
                };
            }

            return new PositionMessageInterpretationDto
            {
                IsSuccess = true,
                Latitude = latitude,
                Longitude = longitude
            };
        }
        catch (Exception ex)
        {
            return new PositionMessageInterpretationDto
            {
                IsSuccess = false,
                ErrorMessage = $"GGA-interpretatie mislukt: {ex.Message}"
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
