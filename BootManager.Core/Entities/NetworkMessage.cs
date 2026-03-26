using System;

namespace BootManager.Core.Entities;

/// <summary>
/// Domein-entiteit voor het opslaan van een ruwe ontvangen netwerkregel voor de eerste verticale slice.
/// Houdt alleen noodzakelijke velden voor persistente opslag.
/// </summary>
public class NetworkMessage
{
    /// <summary>
    /// Unieke identificator van het bericht.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Tijdstempel (UTC) waarop de regel is ontvangen.
    /// </summary>
    public DateTime ReceivedAtUtc { get; private set; }

    /// <summary>
    /// Oorsprong van het bericht (bijv. IP-adres, apparaatnaam).
    /// </summary>
    public string Source { get; private set; } = default!;

    /// <summary>
    /// Protocol of bronformaat (bijv. "TCP", "UDP", "NMEA").
    /// </summary>
    public string Protocol { get; private set; } = default!;

    /// <summary>
    /// De ruwe ontvangen regel zoals binnengekomen (tekst).
    /// </summary>
    public string RawLine { get; private set; } = default!;

    /// <summary>
    /// Optionele bericht-id voor correlatie met bronsystemen (kan null zijn).
    /// </summary>
    public string? MessageId { get; private set; }

    /// <summary>
    /// Optionele hex-gecodeerde payload voor binaire gegevens (kan null zijn).
    /// </summary>
    public string? PayloadHex { get; private set; }

    private NetworkMessage() { } // Voor EF

    private NetworkMessage(DateTime receivedAtUtc, string source, string protocol, string rawLine, string? messageId, string? payloadHex)
    {
        ReceivedAtUtc = receivedAtUtc;
        Source = source;
        Protocol = protocol;
        RawLine = rawLine;
        MessageId = messageId;
        PayloadHex = payloadHex;
    }

    /// <summary>
    /// Factory voor het creëren van een nieuw NetworkMessage-instance.
    /// </summary>
    public static NetworkMessage Create(
        DateTime receivedAtUtc,
        string source,
        string protocol,
        string rawLine,
        string? messageId = null,
        string? payloadHex = null)
        => new NetworkMessage(receivedAtUtc, source, protocol, rawLine, messageId, payloadHex);
}