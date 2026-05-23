using System;
using System.Globalization;
using System.Text;
using BootManager.Tools.Simulator.Models;

namespace BootManager.Tools.Simulator.NMEA0183;

/// <summary>
/// Bouwt geldige NMEA 0183 sentences met correcte XOR-checksum voor gebruik in de simulator.
/// Alle sentences gebruiken talker-prefix "II" (Integrated Instrumentation), passend bij multi-sensor systemen.
/// </summary>
public static class Nmea0183SentenceBuilder
{
    private const string Talker = "II";

    /// <summary>
    /// Berekent de XOR-checksum over de sentence-inhoud (tussen '$' en '*').
    /// </summary>
    /// <param name="sentenceBody">De inhoud van de sentence zonder '$' en zonder '*checksum'.</param>
    /// <returns>Twee hexadecimale tekens van de checksum.</returns>
    public static string CalculateChecksum(string sentenceBody)
    {
        byte cs = 0;
        foreach (var c in sentenceBody)
            cs ^= (byte)c;
        return cs.ToString("X2");
    }

    /// <summary>
    /// Assembleert een volledige NMEA 0183 sentence inclusief '$', body en '*checksum'.
    /// </summary>
    private static string Build(string body)
    {
        var cs = CalculateChecksum(body);
        return $"${body}*{cs}";
    }

    /// <summary>
    /// Converteert decimale graden naar NMEA ddmm.mmmm-formaat voor breedtegraad.
    /// </summary>
    private static (string value, string hemisphere) LatToNmea(double lat)
    {
        var hemi = lat >= 0 ? "N" : "S";
        var absLat = Math.Abs(lat);
        var degrees = (int)absLat;
        var minutes = (absLat - degrees) * 60.0;
        return ($"{degrees:D2}{minutes.ToString("F4", CultureInfo.InvariantCulture)}", hemi);
    }

    /// <summary>
    /// Converteert decimale graden naar NMEA dddmm.mmmm-formaat voor lengtegraad.
    /// </summary>
    private static (string value, string hemisphere) LonToNmea(double lon)
    {
        var hemi = lon >= 0 ? "E" : "W";
        var absLon = Math.Abs(lon);
        var degrees = (int)absLon;
        var minutes = (absLon - degrees) * 60.0;
        return ($"{degrees:D3}{minutes.ToString("F4", CultureInfo.InvariantCulture)}", hemi);
    }

    // -------------------------------------------------------------------------
    // Fase 3a sentences
    // -------------------------------------------------------------------------

    /// <summary>
    /// Bouwt een VHW sentence (Water Speed and Heading).
    /// Veld: heading true, heading magnetic, speed knots, speed km/h.
    /// Levert SpeedThroughWaterMeasurement op.
    /// Voorbeeld: $IIVHW,83.0,T,83.0,M,5.3,N,9.8,K*xx
    /// </summary>
    public static string BuildVhw(BoatState s)
    {
        var speedKmh = s.SpeedThroughWaterKnots * 1.852;
        var body = string.Format(CultureInfo.InvariantCulture,
            "{0}VHW,{1:F1},T,{1:F1},M,{2:F1},N,{3:F1},K",
            Talker, s.HeadingDegrees, s.SpeedThroughWaterKnots, speedKmh);
        return Build(body);
    }

    /// <summary>
    /// Bouwt een MTW sentence (Water Temperature).
    /// Levert WaterTemperatureMeasurement op.
    /// Voorbeeld: $IIMTW,17.5,C*xx
    /// </summary>
    public static string BuildMtw(BoatState s)
    {
        var body = string.Format(CultureInfo.InvariantCulture,
            "{0}MTW,{1:F1},C",
            Talker, s.WaterTemperatureCelsius);
        return Build(body);
    }

    /// <summary>
    /// Bouwt een DBT sentence (Depth Below Transducer).
    /// Levert DepthMeasurement op.
    /// Voorbeeld: $IIDBT,11.5,f,3.5,M,1.9,F*xx
    /// </summary>
    public static string BuildDbt(BoatState s)
    {
        var feet = s.DepthMeters * 3.28084;
        var fathoms = s.DepthMeters * 0.546807;
        var body = string.Format(CultureInfo.InvariantCulture,
            "{0}DBT,{1:F1},f,{2:F1},M,{3:F1},F",
            Talker, feet, s.DepthMeters, fathoms);
        return Build(body);
    }

    // -------------------------------------------------------------------------
    // Fase 3b sentences
    // -------------------------------------------------------------------------

    /// <summary>
    /// Bouwt een MWV sentence (Wind Speed and Angle) met status A (geldig).
    /// Levert WindMeasurement op.
    /// Voorbeeld: $IIMWV,45.0,R,9.7,N,A*xx
    /// </summary>
    public static string BuildMwv(BoatState s)
    {
        var speedKnots = s.WindSpeedMps * 1.94384;
        var body = string.Format(CultureInfo.InvariantCulture,
            "{0}MWV,{1:F1},R,{2:F1},N,A",
            Talker, NormalizeAngle360(s.WindAngleDeg), speedKnots);
        return Build(body);
    }

    /// <summary>
    /// Bouwt een HDT sentence (Heading True).
    /// Levert HeadingMeasurement op.
    /// Voorbeeld: $IIHDT,83.0,T*xx
    /// </summary>
    public static string BuildHdt(BoatState s)
    {
        var body = string.Format(CultureInfo.InvariantCulture,
            "{0}HDT,{1:F1},T",
            Talker, s.HeadingDegrees);
        return Build(body);
    }

    // -------------------------------------------------------------------------
    // Fase 3c sentences
    // -------------------------------------------------------------------------

    /// <summary>
    /// Bouwt een RMC sentence (Recommended Minimum Navigation) met status A (geldig).
    /// Levert PositionMeasurement en MotionMeasurement op.
    /// Voorbeeld: $IIRMC,120000.00,A,5236.0000,N,00518.0000,E,5.5,85.0,010101,,,A*xx
    /// </summary>
    public static string BuildRmc(BoatState s)
    {
        var t = s.TimestampUtc;
        var timeStr = t.ToString("HHmmss.ff");
        var dateStr = t.ToString("ddMMyy");
        var (latVal, latHemi) = LatToNmea(s.Latitude);
        var (lonVal, lonHemi) = LonToNmea(s.Longitude);
        var body = string.Format(CultureInfo.InvariantCulture,
            "{0}RMC,{1},A,{2},{3},{4},{5},{6:F1},{7:F1},{8},,,A",
            Talker, timeStr, latVal, latHemi, lonVal, lonHemi,
            s.SogKnots, s.CogDegrees, dateStr);
        return Build(body);
    }

    /// <summary>
    /// Bouwt een GGA sentence (Global Positioning System Fix) met fixkwaliteit 1.
    /// Levert PositionMeasurement op.
    /// Voorbeeld: $IIGGA,120000.00,5236.0000,N,00518.0000,E,1,08,1.2,0.0,M,0.0,M,,*xx
    /// </summary>
    public static string BuildGga(BoatState s)
    {
        var t = s.TimestampUtc;
        var timeStr = t.ToString("HHmmss.ff");
        var (latVal, latHemi) = LatToNmea(s.Latitude);
        var (lonVal, lonHemi) = LonToNmea(s.Longitude);
        var body = string.Format(CultureInfo.InvariantCulture,
            "{0}GGA,{1},{2},{3},{4},{5},1,08,1.2,0.0,M,0.0,M,,",
            Talker, timeStr, latVal, latHemi, lonVal, lonHemi);
        return Build(body);
    }

    // -------------------------------------------------------------------------
    // Negatieve testvarianten
    // -------------------------------------------------------------------------

    /// <summary>
    /// Bouwt een MWV sentence met status V (ongeldig).
    /// Triggert raw opslag maar geen WindMeasurement.
    /// </summary>
    public static string BuildMwvStatusV(BoatState s)
    {
        var speedKnots = s.WindSpeedMps * 1.94384;
        var body = string.Format(CultureInfo.InvariantCulture,
            "{0}MWV,{1:F1},R,{2:F1},N,V",
            Talker, NormalizeAngle360(s.WindAngleDeg), speedKnots);
        return Build(body);
    }

    /// <summary>
    /// Bouwt een RMC sentence met status V (ongeldig).
    /// Triggert raw opslag maar geen Position- of MotionMeasurement.
    /// </summary>
    public static string BuildRmcStatusV(BoatState s)
    {
        var t = s.TimestampUtc;
        var timeStr = t.ToString("HHmmss.ff");
        var dateStr = t.ToString("ddMMyy");
        var (latVal, latHemi) = LatToNmea(s.Latitude);
        var (lonVal, lonHemi) = LonToNmea(s.Longitude);
        var body = string.Format(CultureInfo.InvariantCulture,
            "{0}RMC,{1},V,{2},{3},{4},{5},{6:F1},{7:F1},{8},,,N",
            Talker, timeStr, latVal, latHemi, lonVal, lonHemi,
            s.SogKnots, s.CogDegrees, dateStr);
        return Build(body);
    }

    /// <summary>
    /// Bouwt een GGA sentence met fixkwaliteit 0 (geen fix).
    /// Triggert raw opslag maar geen PositionMeasurement.
    /// </summary>
    public static string BuildGgaNoFix(BoatState s)
    {
        var t = s.TimestampUtc;
        var timeStr = t.ToString("HHmmss.ff");
        var (latVal, latHemi) = LatToNmea(s.Latitude);
        var (lonVal, lonHemi) = LonToNmea(s.Longitude);
        var body = string.Format(CultureInfo.InvariantCulture,
            "{0}GGA,{1},{2},{3},{4},{5},0,00,99.9,0.0,M,0.0,M,,",
            Talker, timeStr, latVal, latHemi, lonVal, lonHemi);
        return Build(body);
    }

    /// <summary>
    /// Bouwt een VHW sentence met een opzettelijk foutieve checksum (laatste byte verhoogd met 1).
    /// Triggert raw opslag maar geen SpeedThroughWaterMeasurement.
    /// </summary>
    public static string BuildVhwBadChecksum(BoatState s)
    {
        var speedKmh = s.SpeedThroughWaterKnots * 1.852;
        var body = string.Format(CultureInfo.InvariantCulture,
            "{0}VHW,{1:F1},T,{1:F1},M,{2:F1},N,{3:F1},K",
            Talker, s.HeadingDegrees, s.SpeedThroughWaterKnots, speedKmh);
        var cs = CalculateChecksum(body);
        // Corrupte checksum: verhoog laatste hex-teken
        var corrupted = cs[0].ToString() + ((char)(cs[1] + 1)).ToString();
        return $"${body}*{corrupted}";
    }

    // -------------------------------------------------------------------------
    // Hulpfuncties
    // -------------------------------------------------------------------------

    private static double NormalizeAngle360(double angle)
    {
        var a = angle % 360.0;
        if (a < 0) a += 360.0;
        return a;
    }
}
