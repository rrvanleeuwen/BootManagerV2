namespace BootManager.Application.WindMeasurements.DTOs;

/// <summary>
/// DTO voor het aanmaken van een nieuw WindMeasurement-record.
/// </summary>
public class CreateWindMeasurementRequestDto
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
    /// Gemeten windhoek in graden (0-360).
    /// </summary>
    public decimal WindAngleDegrees { get; init; }

    /// <summary>
    /// Gemeten windsnelheid in m/s.
    /// </summary>
    public decimal WindSpeed { get; init; }

    /// <summary>
    /// Eenheid van de windsnelheid (bijv. "m/s").
    /// </summary>
    public string SpeedUnit { get; init; } = default!;
}