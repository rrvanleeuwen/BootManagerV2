using System;

namespace BootManager.Application.PositionMeasurements.DTOs;

/// <summary>
/// DTO voor het aanmaken van een nieuw PositionMeasurement-record.
/// </summary>
public class CreatePositionMeasurementRequestDto
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
    /// Breedtegraad in decimale graden (bijvoorbeeld 52.1234).
    /// </summary>
    public decimal Latitude { get; init; }

    /// <summary>
    /// Lengtegraad in decimale graden (bijvoorbeeld 5.5678).
    /// </summary>
    public decimal Longitude { get; init; }
}
