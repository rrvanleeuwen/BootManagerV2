namespace BootManager.Application.SpeedThroughWaterMeasurements.DTOs;

/// <summary>
/// DTO voor het aanmaken van een nieuw SpeedThroughWaterMeasurement-record.
/// </summary>
public class CreateSpeedThroughWaterMeasurementRequestDto
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
    /// Gemeten snelheid door water in meters per seconde.
    /// </summary>
    public decimal SpeedMetersPerSecond { get; init; }

    /// <summary>
    /// Gemeten snelheid door water in knopen.
    /// </summary>
    public decimal SpeedKnots { get; init; }

    /// <summary>
    /// Type snelheidsreferentie (0 = Paddle wheel, 1 = Pitot tube, 2 = Doppler, etc.).
    /// </summary>
    public byte SpeedWaterReferenceType { get; init; }
}
