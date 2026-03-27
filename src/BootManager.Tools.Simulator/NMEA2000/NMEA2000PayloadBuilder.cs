namespace BootManager.Tools.Simulator.NMEA2000;

using System.Text;
using BootManager.Tools.Simulator.Models;

/// <summary>
/// Bouwt NMEA 2000-achtige payloads voor verschillende PGN's op basis van BoatState.
/// 
/// Dit is een implementatie-specifieke benadering voor simulatie, GEEN volledige gecertificeerde
/// NMEA 2000-encodering. De payload-layouts volgen de NMEA 2000-semantiek waar van toepassing.
/// </summary>
public class NMEA2000PayloadBuilder
{
    /// <summary>
    /// Bouwt een PGN 129025 (Position, Rapid Update) payload.
    /// 
    /// Payload-layout (8 bytes):
    /// - Bytes 0-3: Latitude in 1e-7 degrees (int32, little-endian)
    /// - Bytes 4-7: Longitude in 1e-7 degrees (int32, little-endian)
    /// </summary>
    public static byte[] BuildPositionPayload(BoatState state)
    {
        var buffer = new byte[8];
        var latInt = (int)Math.Round(state.Latitude * 1e7);
        var lonInt = (int)Math.Round(state.Longitude * 1e7);

        BitConverter.GetBytes(latInt).CopyTo(buffer, 0);
        BitConverter.GetBytes(lonInt).CopyTo(buffer, 4);

        return buffer;
    }

    /// <summary>
    /// Bouwt een PGN 129026 (Course Over Ground &amp; Speed Over Ground, Rapid Update) payload.
    /// 
    /// NMEA 2000-achtige simulatie (GEEN volledige gecertificeerde implementatie).
    /// 
    /// Payload-layout (4 bytes):
    /// - Bytes 0-1: Course Over Ground in 1/10000 radians (uint16, little-endian)
    ///             Bereik: 0 tot 62832 (= 0 tot 2π radialen = 0 tot 360 graden)
    ///             Conversie van graden: graden * 10000π / 360
    /// - Bytes 2-3: Speed Over Ground in 0,01 knots (uint16, little-endian)
    ///             Bereik: 0 tot 655.35 knots
    ///             Conversie van knoten: knoten * 100
    /// 
    /// Opmerking: Een volledige NMEA 2000 PGN 129026 kan extra velden bevatten (Magnetic COG, Mode, etc.).
    /// Deze simulatie bevat alleen de twee essentiële velden (COG en SOG).
    /// </summary>
    public static byte[] BuildCogSogPayload(BoatState state)
    {
        var buffer = new byte[4];

        // Bytes 0-1: COG als NMEA 2000-achtige uint16 (1e-4 radialen, little-endian)
        // DegreesToNMEA2000Radians() converteert graden naar 1e-4 rad eenheden als ushort
        ushort cogNMEA2000 = NMEA2000PgnSpecification.DegreesToNMEA2000Radians(state.CogDegrees);
        BitConverter.GetBytes(cogNMEA2000).CopyTo(buffer, 0);

        // Bytes 2-3: SOG in centiknoten (uint16, little-endian)
        ushort sogCentiknots = (ushort)Math.Round(Math.Clamp(state.SogKnots * 100, 0, 65535));
        BitConverter.GetBytes(sogCentiknots).CopyTo(buffer, 2);

        return buffer;
    }

    /// <summary>
    /// Bouwt een PGN 127250 (Vessel Heading) payload.
    /// 
    /// Payload-layout (2 bytes):
    /// - Bytes 0-1: Heading in 1/10000 radians (uint16, little-endian)
    /// </summary>
    public static byte[] BuildHeadingPayload(BoatState state)
    {
        var buffer = new byte[2];
        var headingRadians = NMEA2000PgnSpecification.DegreesToNMEA2000Radians(state.HeadingDegrees);

        BitConverter.GetBytes(headingRadians).CopyTo(buffer, 0);

        return buffer;
    }

    /// <summary>
    /// Bouwt een PGN 130306 (Wind Data, Rapid Update) payload.
    /// 
    /// Payload-layout (4 bytes):
    /// - Bytes 0-1: Wind Speed in 0,01 m/s (uint16, little-endian)
    /// - Bytes 2-3: Wind Angle in 1/10000 radians (uint16, little-endian)
    /// </summary>
    public static byte[] BuildWindPayload(BoatState state)
    {
        var buffer = new byte[4];
        var windSpeedCentiMps = (ushort)Math.Round(Math.Clamp(state.WindSpeedMps * 100, 0, 65535));
        var windAngleRadians = NMEA2000PgnSpecification.DegreesToNMEA2000Radians(state.WindAngleDeg);

        BitConverter.GetBytes(windSpeedCentiMps).CopyTo(buffer, 0);
        BitConverter.GetBytes(windAngleRadians).CopyTo(buffer, 2);

        return buffer;
    }

    /// <summary>
    /// Bouwt een PGN 128267 (Water Depth, Rapid Update) payload (3 bytes).
    /// 
    /// Payload-layout (3 bytes):
    /// - Bytes 0-2: Depth in 0,01 meter (uint24, little-endian)
    /// </summary>
    public static byte[] BuildDepthPayload(BoatState state)
    {
        var depthCentimeters = (uint)Math.Round(Math.Clamp(state.DepthMeters * 100, 0, 16777215)); // 3-byte max
        var bytes = BitConverter.GetBytes(depthCentimeters);
        return bytes.Take(3).ToArray(); // Zet op 3 bytes
    }

    /// <summary>
    /// Bouwt een PGN 127508 (Battery Status) payload.
    /// 
    /// Payload-layout (4 bytes):
    /// - Byte 0: Battery Instance (0x00 voor eerste batterij)
    /// - Bytes 1-2: Voltage in 0,01V (uint16, little-endian)
    /// - Byte 3: State of Charge in % (0-100, of 0xFF voor unknown)
    /// </summary>
    public static byte[] BuildBatteryPayload(BoatState state, byte instance = 0)
    {
        var buffer = new byte[4];

        buffer[0] = instance;

        var voltageCentivolts = (ushort)Math.Round(Math.Clamp(state.BatteryVoltage * 100, 0, 65535));
        BitConverter.GetBytes(voltageCentivolts).CopyTo(buffer, 1);

        buffer[3] = (byte)Math.Round(Math.Clamp(state.BatterySoc, 0, 100));

        return buffer;
    }

    /// <summary>
    /// Converteert bytes naar een hex-string geschikt voor transport.
    /// </summary>
    public static string BytesToHexString(byte[] data)
    {
        return string.Join(" ", data.Select(b => b.ToString("X2")));
    }
}