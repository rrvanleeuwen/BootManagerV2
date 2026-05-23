using BootManager.Core.Enums;

namespace BootManager.Core.Entities;

/// <summary>
/// Operationele instellingen voor de BootManager. Er is maximaal 1 record.
/// Bevat netwerk/ingest-configuratie en opslag/sampling-configuratie.
/// </summary>
public class OperationalSettings
{
    /// <summary>Primaire sleutel.</summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    // --- Netwerk / Ingest ---

    /// <summary>IP-adres of hostname waarop de ingest-service luistert.</summary>
    public string ListenAddress { get; private set; } = "0.0.0.0";

    /// <summary>Primaire poort waarop de ingest-service luistert.</summary>
    public int ListenPort { get; private set; } = 10110;

    /// <summary>Optionele alternatieve luisterpoort.</summary>
    public int? AlternativeListenPort { get; private set; }

    /// <summary>Basis-URL van de BootManager Web API.</summary>
    public string ApiBaseUrl { get; private set; } = "http://localhost:5046";

    // --- Opslag / Sampling ---

    /// <summary>Hoe ruwe NMEA-berichten worden opgeslagen.</summary>
    public RawStorageMode RawStorageMode { get; private set; } = RawStorageMode.All;

    /// <summary>Standaard sample-interval in seconden (1–3600).</summary>
    public int DefaultSampleIntervalSeconds { get; private set; } = 10;

    /// <summary>Schakel capture-logging in of uit.</summary>
    public bool CaptureLoggingEnabled { get; private set; } = false;

    /// <summary>Tijdstip van aanmaak (UTC).</summary>
    public DateTime CreatedUtc { get; private set; }

    /// <summary>Tijdstip van laatste wijziging (UTC).</summary>
    public DateTime? UpdatedUtc { get; private set; }

    private OperationalSettings() { } // Voor EF

    /// <summary>
    /// Maakt een nieuw <see cref="OperationalSettings"/>-record aan met standaardwaarden.
    /// </summary>
    public static OperationalSettings CreateDefaults(DateTime createdUtc) =>
        new()
        {
            Id = Guid.NewGuid(),
            ListenAddress = "0.0.0.0",
            ListenPort = 10110,
            AlternativeListenPort = null,
            ApiBaseUrl = "http://localhost:5046",
            RawStorageMode = RawStorageMode.All,
            DefaultSampleIntervalSeconds = 10,
            CaptureLoggingEnabled = false,
            CreatedUtc = createdUtc
        };

    /// <summary>
    /// Werkt de instellingen bij met nieuwe waarden.
    /// </summary>
    public void Update(
        string listenAddress,
        int listenPort,
        int? alternativeListenPort,
        string apiBaseUrl,
        RawStorageMode rawStorageMode,
        int defaultSampleIntervalSeconds,
        bool captureLoggingEnabled,
        DateTime updatedUtc)
    {
        ListenAddress = listenAddress;
        ListenPort = listenPort;
        AlternativeListenPort = alternativeListenPort;
        ApiBaseUrl = apiBaseUrl;
        RawStorageMode = rawStorageMode;
        DefaultSampleIntervalSeconds = defaultSampleIntervalSeconds;
        CaptureLoggingEnabled = captureLoggingEnabled;
        UpdatedUtc = updatedUtc;
    }
}
