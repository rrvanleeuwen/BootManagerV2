using System;

namespace BootManager.Application.NetworkMessages.DTOs;

/// <summary>
/// DTO voor het aanmaken van een nieuw NetworkMessage-record.
/// </summary>
public class CreateNetworkMessageDto
{
    /// <summary>
    /// Tijdstip (UTC) waarop de regel ontvangen werd.
    /// </summary>
    public DateTime ReceivedAtUtc { get; init; }

    /// <summary>
    /// Oorsprong van het bericht (bijv. IP-adres of apparaatnaam).
    /// </summary>
    public string Source { get; init; } = default!;

    /// <summary>
    /// Protocol of bronformaat (bijv. "TCP", "UDP", "NMEA").
    /// </summary>
    public string Protocol { get; init; } = default!;

    /// <summary>
    /// De ruwe ontvangen regel zoals binnengekomen (tekst).
    /// </summary>
    public string RawLine { get; init; } = default!;

    /// <summary>
    /// Optionele bericht-id voor correlatie (kan null zijn).
    /// </summary>
    public string? MessageId { get; init; }

    /// <summary>
    /// Optionele hex-gecodeerde payload voor binaire gegevens (kan null zijn).
    /// </summary>
    public string? PayloadHex { get; init; }
}