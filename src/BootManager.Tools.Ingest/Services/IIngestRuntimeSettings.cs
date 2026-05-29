using BootManager.Core.Enums;

namespace BootManager.Tools.Ingest.Services;

/// <summary>
/// Interface voor thread-safe runtime instellingen van de ingest-service.
/// Deze instellingen kunnen live worden geupdate zonder procesrestart.
/// </summary>
public interface IIngestRuntimeSettings
{
    /// <summary>
    /// Haalt of stelt de huidige API base URL in.
    /// </summary>
    string ApiBaseUrl { get; set; }

    /// <summary>
    /// Haalt of stelt de huidige RawStorageMode in.
    /// </summary>
    RawStorageMode RawStorageMode { get; set; }

    /// <summary>
    /// Haalt of stelt het huidige DefaultSampleIntervalSeconds in.
    /// </summary>
    int DefaultSampleIntervalSeconds { get; set; }

    /// <summary>
    /// Haalt of stelt of capture logging is ingeschakeld.
    /// </summary>
    bool CaptureLoggingEnabled { get; set; }

    /// <summary>
    /// Haalt of stelt in of ingest-verwerking is ingeschakeld.
    /// Als false, accepteert Ingest UDP-verkeer maar post niets naar de API.
    /// </summary>
    bool IngestProcessingEnabled { get; set; }

    /// <summary>
    /// Haalt op of de ListenAddress is gewijzigd sinds vorige reload (vereist herstart).
    /// </summary>
    string ListenAddress { get; }

    /// <summary>
    /// Haalt op of de ListenPort is gewijzigd sinds vorige reload (vereist herstart).
    /// </summary>
    int ListenPort { get; }
}
