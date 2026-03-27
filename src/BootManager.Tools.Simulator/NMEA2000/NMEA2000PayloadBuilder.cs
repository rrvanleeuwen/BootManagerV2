namespace BootManager.Tools.Simulator.NMEA2000;

using System.Text;
using BootManager.Tools.Simulator.Models;

/// <summary>
/// Bouwt NMEA 2000-achtige payloads voor verschillende PGN's op basis van BoatState.
/// </summary>
public class NMEA2000PayloadBuilder
{
    /// <summary>
    /// Bouwt een PGN 129025 (Position) payload.
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
    /// Bouwt een PGN 129026 (COG/SOG) payload.
    /// </summary>
    public static byte[] BuildCogSogPayload(BoatState state)
    {
        var buffer = new byte[4];
        var cogRadians = NMEA2000PgnSpecification.DegreesToNMEA2000Radians(state.CogDegrees);
        var sogCentiknots = (ushort)Math.Round(Math.Clamp(state.SogKnots * 100, 0, 65535));

        BitConverter.GetBytes(cogRadians).CopyTo(buffer, 0);
        BitConverter.GetBytes(sogCentiknots).CopyTo(buffer, 2);

        return buffer;
    }

    /// <summary>
    /// Bouwt een PGN 127250 (Heading) payload.
    /// </summary>
    public static byte[] BuildHeadingPayload(BoatState state)
    {
        var buffer = new byte[2];
        var headingRadians = NMEA2000PgnSpecification.DegreesToNMEA2000Radians(state.HeadingDegrees);

        BitConverter.GetBytes(headingRadians).CopyTo(buffer, 0);

        return buffer;
    }

    /// <summary>
    /// Bouwt een PGN 130306 (Wind) payload.
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
    /// Bouwt een PGN 128267 (Depth) payload (3 bytes).
    /// </summary>
    public static byte[] BuildDepthPayload(BoatState state)
    {
        var depthCentimeters = (uint)Math.Round(Math.Clamp(state.DepthMeters * 100, 0, 16777215)); // 3-byte max
        var bytes = BitConverter.GetBytes(depthCentimeters);
        return bytes.Take(3).ToArray(); // Zet op 3 bytes
    }

    /// <summary>
    /// Bouwt een PGN 127508 (Battery) payload.
    /// 
    /// Layout:
    /// - Byte 0: Instance (0x00 voor eerste batterij)
    /// - Bytes 1-2: Voltage in 0,01V (little-endian uint16)
    /// - Byte 3: State of Charge in %
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