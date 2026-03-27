namespace BootManager.Core.Entities;

/// <summary>
/// Domein-entiteit voor het opslaan van geïnterpreteerde windmetingen.
/// Bevat de eerste betekenisvolle meetwaarden die uit netwerkberichten zijn geëxtraheerd.
/// </summary>
public class WindMeasurement
{
    /// <summary>
    /// Unieke identificator van de windmeting.
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
    /// Gemeten windhoek in graden (0-360).
    /// </summary>
    public decimal WindAngleDegrees { get; private set; }

    /// <summary>
    /// Gemeten windsnelheid in m/s.
    /// </summary>
    public decimal WindSpeed { get; private set; }

    /// <summary>
    /// Eenheid van de windsnelheid (bijv. "m/s").
    /// </summary>
    public string SpeedUnit { get; private set; } = default!;

    private WindMeasurement() { } // Voor EF

    /// <summary>
    /// Initialiseert een nieuwe windmeting met de verplichte velden.
    /// </summary>
    public WindMeasurement(DateTime recordedAtUtc, string source, string messageId, decimal windAngleDegrees, decimal windSpeed, string speedUnit)
    {
        RecordedAtUtc = recordedAtUtc;
        Source = source;
        MessageId = messageId;
        WindAngleDegrees = windAngleDegrees;
        WindSpeed = windSpeed;
        SpeedUnit = speedUnit;
    }
}