namespace BootManager.Core.Entities;

/// <summary>
/// Domein-entiteit voor het opslaan van geïnterpreteerde watertemperatuur-metingen.
/// Gebaseerd op PGN 130312 (Temperature / Temperature, Water).
/// </summary>
public class WaterTemperatureMeasurement
{
    /// <summary>
    /// Unieke identificator van de meting.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Tijdstempel (UTC) waarop de meting is geregistreerd.
    /// </summary>
    public DateTime RecordedAtUtc { get; private set; }

    /// <summary>
    /// Oorsprong van de meting (bijv. IP-adres of apparaatnaam van de bron).
    /// </summary>
    public string Source { get; private set; } = default!;

    /// <summary>
    /// Referentie naar het oorspronkelijke netwerkbericht waaruit deze meting is afgeleid.
    /// </summary>
    public string MessageId { get; private set; } = default!;

    /// <summary>
    /// Temperature instance (0 = Sea/Water Temperature).
    /// </summary>
    public byte TemperatureInstance { get; private set; }

    /// <summary>
    /// Gemeten watertemperatuur in Kelvin.
    /// </summary>
    public decimal TemperatureKelvin { get; private set; }

    /// <summary>
    /// Gemeten watertemperatuur in graden Celsius.
    /// </summary>
    public decimal TemperatureCelsius { get; private set; }

    private WaterTemperatureMeasurement() { } // Voor EF

    /// <summary>
    /// Initialiseert een nieuwe watertemperatuur-meting met de verplichte velden.
    /// </summary>
    public WaterTemperatureMeasurement(
        DateTime recordedAtUtc,
        string source,
        string messageId,
        byte temperatureInstance,
        decimal temperatureKelvin,
        decimal temperatureCelsius)
    {
        RecordedAtUtc = recordedAtUtc;
        Source = source;
        MessageId = messageId;
        TemperatureInstance = temperatureInstance;
        TemperatureKelvin = temperatureKelvin;
        TemperatureCelsius = temperatureCelsius;
    }
}
