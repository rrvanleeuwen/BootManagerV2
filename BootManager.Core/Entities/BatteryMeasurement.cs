using System;

namespace BootManager.Core.Entities;

/// <summary>
/// Domein-entiteit voor het opslaan van geïnterpreteerde batterijmetingen.
/// Bevat de eerste betekenisvolle meetwaarden die uit netwerkberichten zijn geëxtraheerd.
/// </summary>
public class BatteryMeasurement
{
    /// <summary>
    /// Unieke identificator van de batterijmeting.
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
    /// Gemeten spanning in volts (bijv. 12.60).
    /// </summary>
    public decimal Voltage { get; private set; }

    /// <summary>
    /// Optionele laadtoestand in procenten (0-100). Kan null zijn als niet beschikbaar.
    /// </summary>
    public int? StateOfCharge { get; private set; }

    private BatteryMeasurement() { } // Voor EF

    /// <summary>
    /// Initialiseert een nieuwe batterijmeting met de verplichte velden.
    /// </summary>
    public BatteryMeasurement(DateTime recordedAtUtc, string source, string messageId, decimal voltage, int? stateOfCharge = null)
    {
        RecordedAtUtc = recordedAtUtc;
        Source = source;
        MessageId = messageId;
        Voltage = voltage;
        StateOfCharge = stateOfCharge;
    }
}