namespace BootManager.Tools.Ingest.Options;

using BootManager.Core.Enums;

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

    /// <summary>
    /// Bepaalt hoe ruwe NMEA-berichten naar de API/database worden opgeslagen.
    /// - <see cref="RawStorageMode.All"/>: Alle ontvangen berichten worden gepost (huidig gedrag).
    /// - <see cref="RawStorageMode.Sampled"/>: Maximaal één bericht per stream key per <see cref="DefaultSampleIntervalSeconds"/>.
    /// - <see cref="RawStorageMode.OffAfterSuccessfulParse"/>: Tijdelijk als Sampled; echte post-parse raw-retentie in volgende slice.
    /// Standaard: <see cref="RawStorageMode.All"/>.
    /// </summary>
    public RawStorageMode RawStorageMode { get; set; } = RawStorageMode.All;

    /// <summary>
    /// Standaard sample-interval in seconden voor <see cref="RawStorageMode.Sampled"/> en <see cref="RawStorageMode.OffAfterSuccessfulParse"/>.
    /// Bepaalt minimale tijd tussen opeenvolgende berichten per stream key die naar de API worden gepost.
    /// Als waarde &lt;= 0, wordt fallback naar 10 seconden en een waarschuwing gelogd.
    /// Standaard: 10 seconden.
    /// </summary>
    public int DefaultSampleIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Configuratie voor de lokale control API.
    /// Dit endpoint stelt Web in staat om Ingest settings opnieuw in te laden zonder procesrestart.
    /// </summary>
    public ControlApiOptions ControlApi { get; set; } = new();
}

/// <summary>
/// Opties voor de lokale control API in Ingest.
/// De control API luistert standaard alleen op localhost (127.0.0.1) voor veiligheid.
/// </summary>
public class ControlApiOptions
{
    /// <summary>
    /// Schakel de control API in of uit.
    /// Standaard: true (aanbevolen).
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// IP-adres waarop de control API luistert.
    /// BELANGRIJK: Bindt standaard alleen op 127.0.0.1 voor veiligheid.
    /// Wijzig niet naar 0.0.0.0 tenzij je network-beveiliging hebt geconfigureerd.
    /// </summary>
    public string ListenAddress { get; set; } = "127.0.0.1";

    /// <summary>
    /// Poort waarop de control API luistert.
    /// Standaard: 5010 (configureerbaar via appsettings).
    /// </summary>
    public int ListenPort { get; set; } = 5010;
}
