using BootManager.Core.Enums;

namespace BootManager.Tools.Ingest.Services;

/// <summary>
/// Thread-safe implementatie van runtime-instellingen voor de ingest-service.
/// Alle properties zijn met lock beschermd tegen gelijktijdige toegang.
/// </summary>
public class IngestRuntimeSettings : IIngestRuntimeSettings
{
    private readonly object _lockObject = new();

    private string _apiBaseUrl = "http://localhost:5046";
    private RawStorageMode _rawStorageMode = RawStorageMode.All;
    private int _defaultSampleIntervalSeconds = 10;
    private bool _captureLoggingEnabled = false;
    private bool _ingestProcessingEnabled = true;
    private string _listenAddress = "0.0.0.0";
    private int _listenPort = 10110;

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="IngestRuntimeSettings"/>.
    /// </summary>
    /// <param name="apiBaseUrl">Initiële API base URL.</param>
    /// <param name="rawStorageMode">Initiële RawStorageMode.</param>
    /// <param name="defaultSampleIntervalSeconds">Initieel sample interval in seconden.</param>
    /// <param name="captureLoggingEnabled">Of capture logging initieel is ingeschakeld.</param>
    /// <param name="ingestProcessingEnabled">Of ingest-verwerking initieel is ingeschakeld.</param>
    /// <param name="listenAddress">IP-adres van de UDP listener (niet live aanpasbaar).</param>
    /// <param name="listenPort">Poort van de UDP listener (niet live aanpasbaar).</param>
    public IngestRuntimeSettings(
        string apiBaseUrl,
        RawStorageMode rawStorageMode,
        int defaultSampleIntervalSeconds,
        bool captureLoggingEnabled,
        bool ingestProcessingEnabled,
        string listenAddress,
        int listenPort)
    {
        _apiBaseUrl = apiBaseUrl;
        _rawStorageMode = rawStorageMode;
        _defaultSampleIntervalSeconds = defaultSampleIntervalSeconds;
        _captureLoggingEnabled = captureLoggingEnabled;
        _ingestProcessingEnabled = ingestProcessingEnabled;
        _listenAddress = listenAddress;
        _listenPort = listenPort;
    }

    /// <inheritdoc />
    public string ApiBaseUrl
    {
        get
        {
            lock (_lockObject)
            {
                return _apiBaseUrl;
            }
        }
        set
        {
            lock (_lockObject)
            {
                _apiBaseUrl = value;
            }
        }
    }

    /// <inheritdoc />
    public RawStorageMode RawStorageMode
    {
        get
        {
            lock (_lockObject)
            {
                return _rawStorageMode;
            }
        }
        set
        {
            lock (_lockObject)
            {
                _rawStorageMode = value;
            }
        }
    }

    /// <inheritdoc />
    public int DefaultSampleIntervalSeconds
    {
        get
        {
            lock (_lockObject)
            {
                return _defaultSampleIntervalSeconds;
            }
        }
        set
        {
            lock (_lockObject)
            {
                _defaultSampleIntervalSeconds = value;
            }
        }
    }

    /// <inheritdoc />
    public bool CaptureLoggingEnabled
    {
        get
        {
            lock (_lockObject)
            {
                return _captureLoggingEnabled;
            }
        }
        set
        {
            lock (_lockObject)
            {
                _captureLoggingEnabled = value;
            }
        }
    }

    /// <inheritdoc />
    public bool IngestProcessingEnabled
    {
        get
        {
            lock (_lockObject)
            {
                return _ingestProcessingEnabled;
            }
        }
        set
        {
            lock (_lockObject)
            {
                _ingestProcessingEnabled = value;
            }
        }
    }

    /// <inheritdoc />
    public string ListenAddress
    {
        get
        {
            lock (_lockObject)
            {
                return _listenAddress;
            }
        }
    }

    /// <inheritdoc />
    public int ListenPort
    {
        get
        {
            lock (_lockObject)
            {
                return _listenPort;
            }
        }
    }
}
