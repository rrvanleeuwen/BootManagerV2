namespace BootManager.Tools.Ingest.Options;

/// <summary>
/// Opties/configuratie voor de ingest-service.
/// Er is één gecombineerde UDP-listener die zowel NMEA 0183 als NMEA 2000/raw-like regels verwerkt.
/// Protocoldetectie vindt plaats op basis van de regelinhoud: regels die beginnen met '$' of '!' zijn NMEA 0183.
/// </summary>
public class IngestOptions
{
    /// <summary>
    /// IP-adres waarop de gecombineerde UDP-listener luistert.
    /// Gebruik "0.0.0.0" om op alle interfaces te luisteren.
    /// Standaard: "0.0.0.0" (aanbevolen).
    /// </summary>
    public string ListenAddress { get; set; } = "0.0.0.0";

    /// <summary>
    /// Poort waarop de gecombineerde UDP-listener luistert.
    /// Standaard: 10110 (aanbevolen NMEA 0183 UDP-poort).
    /// Alternatief: 2000 als de YDEN op die poort is geconfigureerd.
    /// Luister niet tegelijk op 2000 én 10110 om dubbele YDEN-verwerking te voorkomen.
    /// </summary>
    public int ListenPort { get; set; } = 10110;

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

    /// <summary>
    /// Configuratie voor optionele raw capture logging.
    /// Standaard uitgeschakeld. Zet <see cref="CaptureLoggingOptions.Enabled"/> op <c>true</c> voor een boot-test.
    /// </summary>
    public CaptureLoggingOptions CaptureLogging { get; set; } = new();
}
