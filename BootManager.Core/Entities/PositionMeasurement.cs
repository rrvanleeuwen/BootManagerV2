namespace BootManager.Core.Entities;

/// <summary>
/// Domein-entiteit voor het opslaan van geïnterpreteerde positiemetingen.
/// Bevat de eerste betekenisvolle meetwaarden die uit netwerkberichten zijn geëxtraheerd.
/// </summary>
public class PositionMeasurement
{
    /// <summary>
    /// Unieke identificator van de positiemeting.
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
    /// Breedtegraad in decimale graden (bijvoorbeeld 52.1234).
    /// </summary>
    public decimal Latitude { get; private set; }

    /// <summary>
    /// Lengtegraad in decimale graden (bijvoorbeeld 5.5678).
    /// </summary>
    public decimal Longitude { get; private set; }

    private PositionMeasurement() { } // Voor EF

    /// <summary>
    /// Initialiseert een nieuwe positiemeting met de verplichte velden.
    /// </summary>
    public PositionMeasurement(DateTime recordedAtUtc, string source, string messageId, decimal latitude, decimal longitude)
    {
        RecordedAtUtc = recordedAtUtc;
        Source = source;
        MessageId = messageId;
        Latitude = latitude;
        Longitude = longitude;
    }
}
