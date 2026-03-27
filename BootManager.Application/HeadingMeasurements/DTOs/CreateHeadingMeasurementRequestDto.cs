namespace BootManager.Application.HeadingMeasurements.DTOs;

/// <summary>
/// DTO voor het aanmaken van een nieuw HeadingMeasurement-record.
/// </summary>
public class CreateHeadingMeasurementRequestDto
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
    /// Gemeten koers in graden (0-360).
    /// </summary>
    public decimal HeadingDegrees { get; init; }
}
