using System;

namespace BootManager.Application.BatteryMeasurements.DTOs;

/// <summary>
/// DTO voor het aanmaken van een nieuw BatteryMeasurement-record.
/// </summary>
public class CreateBatteryMeasurementRequestDto
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
    /// Gemeten spanning in volts (bijv. 12.60).
    /// </summary>
    public decimal Voltage { get; init; }

    /// <summary>
    /// Optionele laadtoestand in procenten (0-100). Kan null zijn als niet beschikbaar.
    /// </summary>
    public int? StateOfCharge { get; init; }
}