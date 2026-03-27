using System;

namespace BootManager.Application.MotionMeasurements.DTOs;

/// <summary>
/// DTO voor het aanmaken van een nieuw MotionMeasurement-record.
/// </summary>
public class CreateMotionMeasurementRequestDto
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
    /// Koers over grond in graden (0-359,99).
    /// </summary>
    public decimal CourseOverGroundDegrees { get; init; }

    /// <summary>
    /// Snelheid over grond in de gespecificeerde eenheid.
    /// </summary>
    public decimal SpeedOverGround { get; init; }

    /// <summary>
    /// Eenheid van snelheid (bijv. "kn" voor knopen).
    /// </summary>
    public string SpeedUnit { get; init; } = default!;
}
