namespace BootManager.Core.Entities;

/// <summary>
/// Domein-entiteit voor het opslaan van geïnterpreteerde dieptemetingen.
/// Bevat de eerste betekenisvolle meetwaarden die uit netwerkberichten zijn geëxtraheerd.
/// </summary>
public class DepthMeasurement
{
    /// <summary>
    /// Unieke identificator van de dieptemeting.
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
    /// Gemeten diepte in meters (bijv. 3.50).
    /// </summary>
    public decimal DepthMeters { get; private set; }

    private DepthMeasurement() { } // Voor EF

    /// <summary>
    /// Initialiseert een nieuwe dieptemeting met de verplichte velden.
    /// </summary>
    public DepthMeasurement(DateTime recordedAtUtc, string source, string messageId, decimal depthMeters)
    {
        RecordedAtUtc = recordedAtUtc;
        Source = source;
        MessageId = messageId;
        DepthMeters = depthMeters;
    }
}