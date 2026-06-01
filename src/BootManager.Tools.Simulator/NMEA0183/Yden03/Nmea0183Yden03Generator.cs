using System;
using System.Collections.Generic;
using BootManager.Tools.Simulator.Models;
using BootManager.Tools.Simulator.NMEA0183;

namespace BootManager.Tools.Simulator.NMEA0183.Yden03;

/// <summary>
/// Generator voor een compact YDEN03-achtig profiel: vervangt talker "II" door "YD" voor
/// bestaande sentences en voegt een beperkte set raw-only sentences plus AIS raw regels toe.
/// Geen semantische AIS-decodering, alleen syntactisch correcte regels met checksum.
/// </summary>
public static class Nmea0183Yden03Generator
{
    public static IEnumerable<string> BuildSentences(BoatState s, bool includeNegative)
    {
        // Hergebruik bestaande builder en vervang talker prefix waar nodig.
        var list = new List<string>
        {
            ReplaceTalker(Nmea0183SentenceBuilder.BuildVhw(s), "YD"),
            ReplaceTalker(Nmea0183SentenceBuilder.BuildMtw(s), "YD"),
            ReplaceTalker(Nmea0183SentenceBuilder.BuildDbt(s), "YD"),
            ReplaceTalker(Nmea0183SentenceBuilder.BuildMwv(s), "YD"),
            ReplaceTalker(Nmea0183SentenceBuilder.BuildHdt(s), "YD"),
            ReplaceTalker(Nmea0183SentenceBuilder.BuildRmc(s), "YD"),
            ReplaceTalker(Nmea0183SentenceBuilder.BuildGga(s), "YD"),
        };

        // Voeg enkele raw-only sentences (simpel, vaste velden of licht afgeleid)
        list.Add(BuildZda(s));
        list.Add(BuildMwd(s));
        list.Add(BuildXdr(s));
        list.Add(BuildMda(s));
        list.Add(BuildVtg(s));

        // FluidLevel (PGN 127505) via gateway sentences PCDIN en MXPGN
        // Representatieve tankmetingen: fuel (2 instances), water (2 instances), gray water, invalid level
        list.Add(BuildFluidLevelPcdinFuel(s));
        list.Add(BuildFluidLevelMxpgnFuel(s));
        list.Add(BuildFluidLevelPcdinFuelInstance1(s));
        list.Add(BuildFluidLevelMxpgnFuelInstance1(s));
        list.Add(BuildFluidLevelPcdinWaterInstance0(s));
        list.Add(BuildFluidLevelMxpgnWaterInstance0(s));
        list.Add(BuildFluidLevelPcdinWaterInstance1(s));
        list.Add(BuildFluidLevelMxpgnWaterInstance1(s));
        list.Add(BuildFluidLevelPcdinGrayWater(s));
        list.Add(BuildFluidLevelMxpgnGrayWater(s));
        list.Add(BuildFluidLevelPcdinInvalidLevel(s));
        list.Add(BuildFluidLevelMxpgnInvalidLevel(s));

        // AIS raw-like messages
        list.Add(BuildAivdmSample());
        list.Add(BuildAivdoSample());

        if (includeNegative)
        {
            list.Add(ReplaceTalker(Nmea0183SentenceBuilder.BuildMwvStatusV(s), "YD"));
            list.Add(ReplaceTalker(Nmea0183SentenceBuilder.BuildRmcStatusV(s), "YD"));
            list.Add(ReplaceTalker(Nmea0183SentenceBuilder.BuildGgaNoFix(s), "YD"));
            list.Add(ReplaceTalker(Nmea0183SentenceBuilder.BuildVhwBadChecksum(s), "YD"));
        }

        return list;
    }

    private static string ReplaceTalker(string sentence, string newTalker)
    {
        if (string.IsNullOrEmpty(sentence)) return sentence;
        // sentence: $IIVHW,...*CS
        if (sentence.Length > 3 && sentence[0] == '$')
        {
            var rest = sentence.Substring(3);
            // recalc checksum for new body
            var newBody = newTalker + rest.Substring(0, rest.IndexOf('*'));
            var cs = Nmea0183SentenceBuilder.CalculateChecksum(newBody);
            return "$" + newBody + "*" + cs;
        }
        return sentence;
    }

    // Simple ZDA: time/date
    private static string BuildZda(BoatState s)
    {
        var t = s.TimestampUtc;
        var body = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "YDZDA,{0:HHmmss.ff},{1:D2},{2:D2},{3:D4},,",
            t, t.Day, t.Month, t.Year);
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    // Simple MWD (wind direction and speed) lightweight
    private static string BuildMwd(BoatState s)
    {
        var speedKnots = s.WindSpeedMps * 1.94384;
        var body = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "YDMWD,{0:F1},R,,,{1:F1},N",
            NormalizeAngle360(s.WindAngleDeg), speedKnots);
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    // XDR transducer example: temperature
    private static string BuildXdr(BoatState s)
    {
        var body = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "YDXDR,T,{0:F1},C,WaterTemp",
            s.WaterTemperatureCelsius);
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    // MDA (meteorological composite) small set
    private static string BuildMda(BoatState s)
    {
        var body = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "YDMDA,,,,,{0:F1},C,,,,,,",
            s.WaterTemperatureCelsius);
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    // VTG example
    private static string BuildVtg(BoatState s)
    {
        var body = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "YDVTG,{0:F1},T,{0:F1},M,{1:F1},N,{2:F1},K",
            s.CogDegrees, s.SogKnots, s.SogKnots * 1.852);
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    // FluidLevel (PGN 127505) via gateway sentences PCDIN en MXPGN
    // Format: $PCDIN,PGN_HEX,timestamps,payload_hex*CS
    //         $MXPGN,PGN_HEX,pgn_decimal,payload_hex*CS
    // Payload: byte 0 = instance+type, bytes 1-2 = level (0.004% increments), bytes 3-6 = capacity (0.1L increments), byte 7 = reserved

    private static string BuildFluidLevelPcdinFuel(BoatState s)
    {
        // Fuel tank instance 0, type 0, 100% full, 150L capacity
        // Payload: 00 (inst0, type0) + A861 (0x61A8 = 25000 = 100%) + DC050000 (0x000005DC = 1500 = 150L) + FF
        const string payload = "00A861DC050000FF";
        var body = "PCDIN,01F211,000024F3,43," + payload;
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    private static string BuildFluidLevelMxpgnFuel(BoatState s)
    {
        // Same as PCDIN but MXPGN format with decimal PGN
        const string payload = "00A861DC050000FF";
        var body = "MXPGN,01F211,6843," + payload;
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    private static string BuildFluidLevelPcdinFuelInstance1(BoatState s)
    {
        // Fuel tank instance 1, type 0, 75% full, 150L capacity
        // Payload: 01 (inst1, type0) + 4A61 (0x614A = 18762 = 75.048%) + DC050000 (0x000005DC = 1500 = 150L) + FF
        const string payload = "014A61DC050000FF";
        var body = "PCDIN,01F211,000024F3,43," + payload;
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    private static string BuildFluidLevelMxpgnFuelInstance1(BoatState s)
    {
        // Same as PCDIN but MXPGN format
        const string payload = "014A61DC050000FF";
        var body = "MXPGN,01F211,6843," + payload;
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    private static string BuildFluidLevelPcdinWaterInstance0(BoatState s)
    {
        // Water instance 0, type 1 (fresh water), ~17.704% level, 200L capacity
        // Payload: 10 (inst0, type1) + 4A11 (0x114A = 4426 = 17.704%) + D0070000 (0x000007D0 = 2000 = 200L) + FF
        const string payload = "104A11D0070000FF";
        var body = "PCDIN,01F211,000024F3,43," + payload;
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    private static string BuildFluidLevelMxpgnWaterInstance0(BoatState s)
    {
        const string payload = "104A11D0070000FF";
        var body = "MXPGN,01F211,6843," + payload;
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    private static string BuildFluidLevelPcdinWaterInstance1(BoatState s)
    {
        // Water instance 1, type 1 (fresh water), ~17.704% level, 200L capacity
        // Payload: 11 (inst1, type1) + 4A11 (0x114A = 4426 = 17.704%) + D0070000 (0x000007D0 = 2000 = 200L) + FF
        const string payload = "114A11D0070000FF";
        var body = "PCDIN,01F211,000024F3,43," + payload;
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    private static string BuildFluidLevelMxpgnWaterInstance1(BoatState s)
    {
        const string payload = "114A11D0070000FF";
        var body = "MXPGN,01F211,6843," + payload;
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    private static string BuildFluidLevelPcdinGrayWater(BoatState s)
    {
        // Gray water instance 0, type 2 (gray water), 50% level, 100L capacity
        // Payload: 20 (inst0, type2) + D430 (0x30D4 = 12500 = 50%) + E8030000 (0x000003E8 = 1000 = 100L) + FF
        const string payload = "20D430E8030000FF";
        var body = "PCDIN,01F211,000024F3,43," + payload;
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    private static string BuildFluidLevelMxpgnGrayWater(BoatState s)
    {
        const string payload = "20D430E8030000FF";
        var body = "MXPGN,01F211,6843," + payload;
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    private static string BuildFluidLevelPcdinInvalidLevel(BoatState s)
    {
        // Oil tank instance 0, type 5 (oil), INVALID level (0x7FFF), 100L capacity
        // Payload: 50 (inst0, type5) + FF7F (0x7FFF = invalid) + E8030000 (0x000003E8 = 1000 = 100L) + FF
        const string payload = "50FF7FE8030000FF";
        var body = "PCDIN,01F211,000024F3,43," + payload;
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    private static string BuildFluidLevelMxpgnInvalidLevel(BoatState s)
    {
        // Same as PCDIN but MXPGN format
        const string payload = "50FF7FE8030000FF";
        var body = "MXPGN,01F211,6843," + payload;
        var cs = Nmea0183SentenceBuilder.CalculateChecksum(body);
        return "$" + body + "*" + cs;
    }

    // AIS sample payloads (not meaningful content) – include proper checksum
    private static string BuildAivdmSample()
    {
        // short sample: !AIVDM,1,1,,A,15MuqP0000PD;88MD5MTd?vN0@E,0
        var body = "AIVDM,1,1,,A,15MuqP0000PD;88MD5MTd?vN0@E,0";
        var cs = CalculateAisChecksum(body);
        return "!" + body + "*" + cs;
    }

    private static string BuildAivdoSample()
    {
        var body = "AIVDO,1,1,,B,55NBh>02>f;9Q@<`@0000000,0";
        var cs = CalculateAisChecksum(body);
        return "!" + body + "*" + cs;
    }

    private static string CalculateAisChecksum(string body)
    {
        byte cs = 0;
        foreach (var c in body)
            cs ^= (byte)c;
        return cs.ToString("X2");
    }

    private static double NormalizeAngle360(double angle)
    {
        var a = angle % 360.0;
        if (a < 0) a += 360.0;
        return a;
    }
}
