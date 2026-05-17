namespace BootManager.Tools.Ingest.Options;

/// <summary>
/// Opties/configuratie voor de ingest-service (UDP-listeners en API-instellingen).
/// </summary>
public class IngestOptions
{
    /// <summary>
    /// IP-adres waarop de NMEA2000/raw-like UDP-listener luistert.
    /// </summary>
    public string ListenAddress { get; set; } = "127.0.0.1";

    /// <summary>
    /// Poort waarop de NMEA2000/raw-like UDP-listener luistert.
    /// </summary>
    public int ListenPort { get; set; } = 2000;

    /// <summary>
    /// Instellingen voor de NMEA 0183 UDP-listener.
    /// </summary>
    public Nmea0183ListenerOptions Nmea0183 { get; set; } = new();

    /// <summary>
    /// Maximale grootte van de interne berichtenwachtrij.
    /// </summary>
    public int MaxQueueSize { get; set; } = 1000;

    /// <summary>
    /// Aantal berichten per batch voor verwerking.
    /// </summary>
    public int BatchSize { get; set; } = 10;

    /// <summary>
    /// Base URL van de BootManager.Web API (bijv. http://localhost:5046).
    /// </summary>
    public string ApiBaseUrl { get; set; } = "http://localhost:5046";

    /// <summary>
    /// Relatief endpoint voor NetworkMessages API (bijv. /api/networkmessages).
    /// </summary>
    public string NetworkMessagesEndpoint { get; set; } = "/api/networkmessages";
}

/// <summary>
/// Opties voor de NMEA 0183 UDP-listener.
/// </summary>
public class Nmea0183ListenerOptions
{
    /// <summary>
    /// IP-adres waarop de NMEA 0183 UDP-listener luistert.
    /// Gebruik "0.0.0.0" om op alle interfaces te luisteren.
    /// </summary>
    public string ListenAddress { get; set; } = "0.0.0.0";

    /// <summary>
    /// Poort waarop de NMEA 0183 UDP-listener luistert.
    /// Standaard NMEA 0183 UDP-poort is 10110.
    /// </summary>
    public int ListenPort { get; set; } = 10110;

    /// <summary>
    /// Geeft aan of de NMEA 0183 listener actief is.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
