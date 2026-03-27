namespace BootManager.Tools.Simulator.NMEA2000;

/// <summary>
/// Bevat de NMEA 2000-achtige specificatie voor PGN's die door de simulator worden gegenereerd.
/// 
/// Dit is een implementatie-specifieke benadering, niet een volledige gecertificeerde NMEA 2000-decoder.
/// De schalen en payloadlayouts zijn gebaseerd op werkelijke NMEA 2000-semantiek waar relevant.
/// </summary>
public static class NMEA2000PgnSpecification
{
    /// <summary>
    /// PGN 129025: Position, Rapid Update (GNSS Position Data)
    /// 
    /// Simulatie-payload-layout (8 bytes):
    /// - Bytes 0-3: Latitude in 1e-7 degrees (int32, little-endian)
    /// - Bytes 4-7: Longitude in 1e-7 degrees (int32, little-endian)
    /// 
    /// Opmerking: Een volledige NMEA 2000 PGN 129025 bevat meer velden (CoG, SOG, etc.),
    /// maar we simuleren hier alleen positie. COG/SOG zitten in PGN 129026.
    /// </summary>
    public const uint PGN_POSITION = 129025;

    /// <summary>
    /// PGN 129026: Course Over Ground, Speed Over Ground, Rapid Update
    /// 
    /// Simulatie-payload-layout (4 bytes):
    /// - Bytes 0-1: COG in 1/10000 radians (uint16, little-endian)
    /// - Bytes 2-3: SOG in 1/100 knots (uint16, little-endian)
    /// 
    /// Conversie:
    /// - COG graden naar NMEA 2000: graden * 10000 / 360 * π = graden * (10000π/360) rad units
    /// - SOG: knots * 100 = waarde in 0,01 knots
    /// </summary>
    public const uint PGN_COG_SOG = 129026;

    /// <summary>
    /// PGN 127250: Vessel Heading
    /// 
    /// Simulatie-payload-layout (8 bytes):
    /// - Byte 0: SID (Sequence ID) voor bericht-volgordenummering
    ///           Geeft aan welke opeenvolgende heading-berichten verwant zijn (0-255)
    /// - Bytes 1-2: Heading in 1/10000 radians (uint16, little-endian)
    ///              Bereik: 0 tot 62832 (= 0 tot 2π radialen = 0 tot 360 graden)
    ///              Conversie van graden: graden * 10000π / 360
    /// - Bytes 3-4: Deviation in 1/10000 radians (uint16, little-endian)
    ///              Magnetische deviatiehoek (verschil tussen True en Magnetic north)
    ///              Voor nu: default 0 (kan later uitgebreid worden met realistischere waarden)
    /// - Bytes 5-6: Variation in 1/10000 radians (uint16, little-endian)
    ///              Magnetische variatiehoek (declination)
    ///              Voor nu: default 0 (kan later uitgebreid worden met positie-gebaseerde waarden)
    /// - Byte 7: Reference (directional reference type)
    ///           Bits 0-1: 00=True, 01=Magnetic (voorlopig altijd True)
    ///           Bits 2-7: reserved
    /// 
    /// Opmerking: NMEA 2000 PGN 127250 bevat nominaal meer velden, maar deze simulatie
    /// concentreert zich op de essentiële heading-data met ruimte voor uitbreiding.
    /// </summary>
    public const uint PGN_HEADING = 127250;

    /// <summary>
    /// PGN 130306: Wind Data
    /// 
    /// Simulatie-payload-layout (4 bytes):
    /// - Bytes 0-1: Wind Speed in 0,01 m/s (uint16, little-endian)
    /// - Bytes 2-3: Wind Angle in 1/10000 radians (uint16, little-endian)
    /// 
    /// Conversie:
    /// - Wind Speed m/s: waarde / 100
    /// - Wind Angle graden naar NMEA 2000: graden * 10000 / 360 * π
    /// </summary>
    public const uint PGN_WIND = 130306;

    /// <summary>
    /// PGN 128267: Water Depth, Rapid Update
    /// 
    /// Simulatie-payload-layout (3 bytes):
    /// - Bytes 0-2: Depth in 0,01 meter (uint24, little-endian)
    /// 
    /// Conversie: diepte in meter * 100 = waarde in centimeters
    /// </summary>
    public const uint PGN_DEPTH = 128267;

    /// <summary>
    /// PGN 127508: Battery Status
    /// 
    /// Simulatie-payload-layout (4 bytes):
    /// - Byte 0: Battery Instance (0-15 in upper nibble, reserved in lower nibble)
    /// - Bytes 1-2: Voltage in 0,01V (uint16, little-endian)
    /// - Byte 3: State of Charge in % (0-100, or 0xFF voor unknown)
    /// 
    /// Conversie:
    /// - Voltage in V: waarde / 100 (dus 1200 = 12,00V)
    /// - SOC: rechtstreeks in procenten
    /// </summary>
    public const uint PGN_BATTERY = 127508;

    /// <summary>
    /// Hulpfunctie om graden naar NMEA 2000-achtige radialen (1e-4 rad eenheden) te converteren.
    /// </summary>
    public static ushort DegreesToNMEA2000Radians(double degrees)
    {
        const double DegreesToRadians = Math.PI / 180.0;
        const double ScaleFactor = 10000.0; // NMEA 2000 gebruikt 1e-4 radialen

        var radians = degrees * DegreesToRadians;
        var scaled = radians * ScaleFactor;
        return (ushort)Math.Round(Math.Clamp(scaled, 0, 65535));
    }

    /// <summary>
    /// Hulpfunctie om NMEA 2000-achtige radialen (1e-4 rad eenheden) naar graden te converteren.
    /// </summary>
    public static double NMEA2000RadiansToDegrees(ushort nmeaRadians)
    {
        const double RadiansToDegrees = 180.0 / Math.PI;
        const double ScaleFactor = 10000.0;

        var radians = nmeaRadians / ScaleFactor;
        return radians * RadiansToDegrees;
    }
}