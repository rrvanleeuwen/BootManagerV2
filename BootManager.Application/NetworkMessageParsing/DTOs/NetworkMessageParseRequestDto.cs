namespace BootManager.Application.NetworkMessageParsing.DTOs;

/// <summary>
/// DTO voor het aanvragen van netwerkbericht-parsing.
/// Bevat de onverwerkte netwerkbericht-gegevens die moeten worden geanalyseerd.
/// </summary>
public class NetworkMessageParseRequestDto
{
    /// <summary>
    /// De bron van het netwerkbericht (bijvoorbeeld device-ID of IP-adres).
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Het moment waarop het bericht is ontvangen (UTC).
    /// </summary>
    public DateTime ReceivedAtUtc { get; set; }

    /// <summary>
    /// De onbewerkte tekstlijn van het netwerkbericht.
    /// </summary>
    public string RawLine { get; set; } = string.Empty;

    /// <summary>
    /// De bericht-ID in hexadecimale notatie.
    /// </summary>
    public string MessageIdHex { get; set; } = string.Empty;

    /// <summary>
    /// De berichtlading (payload) in hexadecimale notatie.
    /// </summary>
    public string PayloadHex { get; set; } = string.Empty;
}