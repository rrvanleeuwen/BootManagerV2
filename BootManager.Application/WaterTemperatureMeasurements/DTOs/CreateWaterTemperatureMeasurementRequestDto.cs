namespace BootManager.Application.WaterTemperatureMeasurements.DTOs;

/// <summary>
/// DTO voor het aanmaken van een nieuw WaterTemperatureMeasurement-record.
/// </summary>
public class CreateWaterTemperatureMeasurementRequestDto
{
    /// <summary>
    /// Tijdstempel (UTC) waarop de meting is geregistreerd.
    /// </summary>
    public DateTime RecordedAtUtc { get; init; }

    /// <summary>
    /// Oorsprong van de meting (bijv. IP-adres of apparaatnaam van de bron).
    /// </summary>
    public string Source { get; init; } = default!;

    /// <summary>
    /// Referentie naar het oorspronkelijke netwerkbericht waaruit deze meting is afgeleid.
    /// </summary>
    public string MessageId { get; init; } = default!;

    /// <summary>
    /// Temperature instance (0 = Sea/Water Temperature).
    /// </summary>
    public byte TemperatureInstance { get; init; }

    /// <summary>
    /// Gemeten watertemperatuur in Kelvin.
    /// </summary>
    public decimal TemperatureKelvin { get; init; }

    /// <summary>
    /// Gemeten watertemperatuur in graden Celsius.
    /// </summary>
    public decimal TemperatureCelsius { get; init; }
}
