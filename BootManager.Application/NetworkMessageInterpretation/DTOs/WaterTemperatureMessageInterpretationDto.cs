namespace BootManager.Application.NetworkMessageInterpretation.DTOs;

/// <summary>
/// Semantisch interpretatieresultaat voor watertemperatuur-berichten.
/// 
/// Gebaseerd op PGN 130312 (Temperature / Temperature, Water).
/// Payload-velden:
/// - Byte 0:   SID
/// - Byte 1:   Temperature Instance (0 = Sea/Water Temperature)
/// - Bytes 2-3: Temperatuur in 0,01 Kelvin (uint16, little-endian)
/// </summary>
public class WaterTemperatureMessageInterpretationDto
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
    /// Temperature instance uit byte 1 (0 = Sea/Water Temperature).
    /// </summary>
    public byte TemperatureInstance { get; set; }

    /// <summary>
    /// Gemeten watertemperatuur in Kelvin.
    /// Null als IsSuccess == false.
    /// </summary>
    public decimal? TemperatureKelvin { get; set; }

    /// <summary>
    /// Gemeten watertemperatuur in graden Celsius.
    /// Null als IsSuccess == false.
    /// </summary>
    public decimal? TemperatureCelsius { get; set; }

    /// <summary>
    /// Foutbericht als IsSuccess == false.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
