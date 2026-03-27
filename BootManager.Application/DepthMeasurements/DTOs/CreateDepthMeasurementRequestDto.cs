namespace BootManager.Application.DepthMeasurements.DTOs;

/// <summary>
/// DTO voor het aanmaken van een nieuw DepthMeasurement-record.
/// </summary>
public class CreateDepthMeasurementRequestDto
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
    /// Gemeten diepte in meters (bijv. 3.50).
    /// </summary>
    public decimal DepthMeters { get; init; }
}