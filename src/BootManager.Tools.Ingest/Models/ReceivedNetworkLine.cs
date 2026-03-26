namespace BootManager.Tools.Ingest.Models;

/// <summary>
/// Intern ingest-model voor een ontvangen netwerkregel.
/// Dit model representeert een gestructureerde versie van ontvangen UDP-data.
/// </summary>
public class ReceivedNetworkLine
{
    /// <summary>
    /// Moment waarop de regel is ontvangen (UTC).
    /// </summary>
    public DateTime ReceivedAtUtc { get; set; }

    /// <summary>
    /// De originele ontvangen regelinhoud.
    /// </summary>
    public string RawLine { get; set; } = string.Empty;

    /// <summary>
    /// Bericht-ID uit de regel (bijvoorbeeld hex-waarde van device ID).
    /// Kan leeg zijn als deze niet uit de regel kon worden geparsed.
    /// </summary>
    public string MessageId { get; set; } = string.Empty;

    /// <summary>
    /// Hex-payload uit de regel.
    /// Kan leeg zijn als deze niet uit de regel kon worden geparsed.
    /// </summary>
    public string PayloadHex { get; set; } = string.Empty;

    /// <summary>
    /// Bron van de regel. Vast ingesteld op "Simulator" voor nu.
    /// </summary>
    public string Source { get; set; } = "Simulator";

    /// <summary>
    /// Protocol-type. Vast ingesteld op "YdenRawLike" voor nu.
    /// </summary>
    public string Protocol { get; set; } = "YdenRawLike";
}