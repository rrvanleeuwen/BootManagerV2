namespace BootManager.Core.Entities;

/// <summary>
/// Domein-entiteit voor het opslaan van geïnterpreteerde snelheid-door-water-metingen.
/// Gebaseerd op PGN 128259 (Speed Through Water / Speed, Water Referenced).
/// </summary>
public class SpeedThroughWaterMeasurement
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
    /// Gemeten snelheid door water in meters per seconde.
    /// </summary>
    public decimal SpeedMetersPerSecond { get; private set; }

    /// <summary>
    /// Gemeten snelheid door water in knopen.
    /// </summary>
    public decimal SpeedKnots { get; private set; }

    /// <summary>
    /// Type snelheidsreferentie (0 = Paddle wheel, 1 = Pitot tube, 2 = Doppler, etc.).
    /// </summary>
    public byte SpeedWaterReferenceType { get; private set; }

    private SpeedThroughWaterMeasurement() { } // Voor EF

    /// <summary>
    /// Initialiseert een nieuwe snelheid-door-water-meting met de verplichte velden.
    /// </summary>
    public SpeedThroughWaterMeasurement(
        DateTime recordedAtUtc,
        string source,
        string messageId,
        decimal speedMetersPerSecond,
        decimal speedKnots,
        byte speedWaterReferenceType)
    {
        RecordedAtUtc = recordedAtUtc;
        Source = source;
        MessageId = messageId;
        SpeedMetersPerSecond = speedMetersPerSecond;
        SpeedKnots = speedKnots;
        SpeedWaterReferenceType = speedWaterReferenceType;
    }
}
