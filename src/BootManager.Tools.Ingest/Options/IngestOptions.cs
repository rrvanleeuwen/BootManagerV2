namespace BootManager.Tools.Ingest.Options;

/// <summary>
/// Opties/configuratie voor de ingest-service (UDP-listener instellingen en queue-opties).
/// </summary>
public class IngestOptions
{
    /// <summary>
    /// IP-adres waarop de UDP-listener luistert.
    /// </summary>
    public string ListenAddress { get; set; } = "127.0.0.1";

    /// <summary>
    /// Poort waarop de UDP-listener luistert.
    /// </summary>
    public int ListenPort { get; set; } = 2000;

    /// <summary>
    /// Maximale grootte van de interne berichtenwachtrij.
    /// </summary>
    public int MaxQueueSize { get; set; } = 1000;

    /// <summary>
    /// Aantal berichten per batch voor verwerking.
    /// </summary>
    public int BatchSize { get; set; } = 10;
}