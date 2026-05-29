namespace BootManager.Application.OperationalSettings.DTOs;

/// <summary>
/// Data transfer object met de operationele instellingen die BootManager.Tools.Ingest
/// bij startup ophaalt via de Web API.
/// </summary>
public class IngestSettingsDto
{
    /// <summary>IP-adres of hostname waarop de ingest-service luistert.</summary>
    public string ListenAddress { get; set; } = "0.0.0.0";

    /// <summary>Primaire poort waarop de ingest-service luistert.</summary>
    public int ListenPort { get; set; } = 10110;

    /// <summary>Basis-URL van de BootManager Web API.</summary>
    public string ApiBaseUrl { get; set; } = "http://localhost:5046";

    /// <summary>Schakel capture-logging in of uit.</summary>
    public bool CaptureLoggingEnabled { get; set; } = false;

    /// <summary>Schakel ingest-verwerking in of uit. Als false, accepteert Ingest UDP-verkeer maar post niets naar de API.</summary>
    public bool IngestProcessingEnabled { get; set; } = true;

    /// <summary>
    /// Hoe ruwe NMEA-berichten worden opgeslagen als string.
    /// Mogelijke waarden: "All", "Sampled", "OffAfterSuccessfulParse".
    /// Wordt meegezonden voor toekomstig gebruik, maar nog niet toegepast in deze slice.
    /// </summary>
    public string RawStorageMode { get; set; } = "All";

    /// <summary>
    /// Standaard sample-interval in seconden.
    /// Wordt meegezonden voor toekomstig gebruik, maar nog niet toegepast in deze slice.
    /// </summary>
    public int DefaultSampleIntervalSeconds { get; set; } = 10;
}
