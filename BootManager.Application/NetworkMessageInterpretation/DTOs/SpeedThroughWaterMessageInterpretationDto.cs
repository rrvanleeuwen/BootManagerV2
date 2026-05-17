namespace BootManager.Application.NetworkMessageInterpretation.DTOs;

/// <summary>
/// Semantisch interpretatieresultaat voor snelheid-door-water-berichten.
/// 
/// Gebaseerd op PGN 128259 (Speed Through Water / Speed, Water Referenced).
/// Payload-velden:
/// - Byte 0:   SID
/// - Bytes 1-2: Snelheid in 0,01 m/s (uint16, little-endian)
/// - Byte 3:   Speed Water Reference Type
/// </summary>
public class SpeedThroughWaterMessageInterpretationDto
{
    /// <summary>
    /// Geeft aan of de interpretatie succesvol is voltooid.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Sequence ID uit byte 0.
    /// </summary>
    public byte Sid { get; set; }

    /// <summary>
    /// Gemeten snelheid door water in meters per seconde.
    /// Null als IsSuccess == false.
    /// </summary>
    public decimal? SpeedMetersPerSecond { get; set; }

    /// <summary>
    /// Gemeten snelheid door water in knopen.
    /// Null als IsSuccess == false.
    /// </summary>
    public decimal? SpeedKnots { get; set; }

    /// <summary>
    /// Type snelheidsreferentie (0 = Paddle wheel, 1 = Pitot tube, 2 = Doppler, etc.).
    /// </summary>
    public byte SpeedWaterReferenceType { get; set; }

    /// <summary>
    /// Foutbericht als IsSuccess == false.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
