namespace BootManager.Core.Entities;

/// <summary>
/// Domein-entiteit voor het opslaan van geïnterpreteerde koersmetingen.
/// Bevat de eerste betekenisvolle meetwaarden die uit netwerkberichten zijn geëxtraheerd.
/// </summary>
public class HeadingMeasurement
{
    /// <summary>
    /// Unieke identificator van de koersmeting.
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
    /// Gemeten koers in graden (0-360).
    /// </summary>
    public decimal HeadingDegrees { get; private set; }

    private HeadingMeasurement() { } // Voor EF

    /// <summary>
    /// Initialiseert een nieuwe koersmeting met de verplichte velden.
    /// </summary>
    public HeadingMeasurement(DateTime recordedAtUtc, string source, string messageId, decimal headingDegrees)
    {
        RecordedAtUtc = recordedAtUtc;
        Source = source;
        MessageId = messageId;
        HeadingDegrees = headingDegrees;
    }
}
