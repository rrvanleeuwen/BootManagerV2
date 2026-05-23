using BootManager.Core.Enums;
using Microsoft.Extensions.Logging;

namespace BootManager.Tools.Ingest.Policies;

/// <summary>
/// Implementatie van sampling-beleid voor ruwe netwerkberichten.
/// 
/// Gedrag per RawStorageMode:
/// - <see cref="RawStorageMode.All"/>: Alle berichten worden doorgelaten (geen sampling).
/// - <see cref="RawStorageMode.Sampled"/>: Per stream key (Protocol + MessageId) 
///   maximaal één bericht per DefaultSampleIntervalSeconds.
/// - <see cref="RawStorageMode.OffAfterSuccessfulParse"/>: Voorlopig hetzelfde als Sampled;
///   echte post-parse raw-retentie volgt in volgende slice.
/// 
/// Stream key wordt bepaald als "Protocol:MessageId" of "Protocol:Unknown" als MessageId ontbreekt.
/// </summary>
public class IngestSamplingPolicy : IIngestSamplingPolicy
{
    private RawStorageMode _rawStorageMode;
    private int _sampleIntervalSeconds;
    private readonly ILogger<IngestSamplingPolicy> _logger;

    /// <summary>
    /// Dictionary die per stream key bijhoudt wanneer het laatste bericht is doorgelaten.
    /// Sleutel: "Protocol:MessageId" of "Protocol:Unknown".
    /// Waarde: UTC DateTime van het laatst doorgelaten bericht.
    /// </summary>
    private readonly Dictionary<string, DateTime> _lastMessageTimes = new();
    private readonly object _lockObject = new();

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="IngestSamplingPolicy"/>.
    /// </summary>
    /// <param name="rawStorageMode">Het actieve RawStorageMode.</param>
    /// <param name="sampleIntervalSeconds">Sample-interval in seconden.
    /// Als &lt;= 0, wordt fallback naar 10 seconden en een waarschuwing gelogd.</param>
    /// <param name="logger">Logger-instantie.</param>
    public IngestSamplingPolicy(RawStorageMode rawStorageMode, int sampleIntervalSeconds, ILogger<IngestSamplingPolicy> logger)
    {
        _logger = logger;

        // Valideer interval defensief
        if (sampleIntervalSeconds <= 0)
        {
            _sampleIntervalSeconds = 10;
            _logger.LogWarning("DefaultSampleIntervalSeconds is {Value}, using fallback of 10 seconds", sampleIntervalSeconds);
        }
        else
        {
            _sampleIntervalSeconds = sampleIntervalSeconds;
        }

        _rawStorageMode = rawStorageMode;

        // Log welke mode actief is
        if (rawStorageMode == RawStorageMode.OffAfterSuccessfulParse)
        {
            _logger.LogInformation(
                "RawStorageMode set to OffAfterSuccessfulParse; treating as Sampled for now. " +
                "True post-parse raw-retention not yet supported (will be implemented in future slice).");
        }
        else
        {
            _logger.LogInformation(
                "RawStorageMode set to {Mode}; sample interval is {Interval} seconds.",
                rawStorageMode, _sampleIntervalSeconds);
        }
    }

    /// <summary>
    /// Bepaalt of een ontvangen bericht mag worden doorgelaten naar de API.
    /// </summary>
    public bool ShouldProcessMessage(string protocol, string? messageId)
    {
        // Mode All: laat alles door
        if (_rawStorageMode == RawStorageMode.All)
        {
            return true;
        }

        // Modes Sampled en OffAfterSuccessfulParse: pas sampling toe
        if (_rawStorageMode == RawStorageMode.Sampled || _rawStorageMode == RawStorageMode.OffAfterSuccessfulParse)
        {
            return EvaluateSampledPolicy(protocol, messageId);
        }

        // Voor onbekende modes: conservatief, laat door
        _logger.LogWarning("Unknown RawStorageMode {Mode}; allowing message through", _rawStorageMode);
        return true;
    }

    /// <summary>
    /// Implementeert sampling-logica voor Sampled en OffAfterSuccessfulParse modes.
    /// </summary>
    private bool EvaluateSampledPolicy(string protocol, string? messageId)
    {
        var streamKey = BuildStreamKey(protocol, messageId);
        var now = DateTime.UtcNow;

        lock (_lockObject)
        {
            // Als we nog nooit dit stream key hebben gezien, laat door en registreer
            if (!_lastMessageTimes.TryGetValue(streamKey, out var lastTime))
            {
                _lastMessageTimes[streamKey] = now;
                return true;
            }

            // Controleer of interval is verstreken
            var elapsedSeconds = (now - lastTime).TotalSeconds;
            if (elapsedSeconds >= _sampleIntervalSeconds)
            {
                _lastMessageTimes[streamKey] = now;
                return true;
            }

            // Interval nog niet verstreken; skip dit bericht
            return false;
        }
    }

    /// <summary>
    /// Bouwt een stabiele stream key op basis van protocol en messageId.
    /// Zorgt ervoor dat verschillende berichttypen niet elkaar verdringen.
    /// </summary>
    private static string BuildStreamKey(string protocol, string? messageId)
    {
        if (string.IsNullOrWhiteSpace(messageId))
        {
            return $"{protocol}:Unknown";
        }

        // Normaliseer messageId voor consistentie
        return $"{protocol}:{messageId.Trim().ToUpperInvariant()}";
    }

    /// <summary>
    /// Reset alle interne timing-state.
    /// </summary>
    public void Reset()
    {
        lock (_lockObject)
        {
            _lastMessageTimes.Clear();
        }
    }

    /// <summary>
    /// Update de sampling policy met nieuwe RawStorageMode en interval.
    /// Dit kan veilig worden aangeroepen terwijl berichten worden verwerkt (thread-safe).
    /// </summary>
    /// <param name="newMode">De nieuwe RawStorageMode.</param>
    /// <param name="newIntervalSeconds">Het nieuwe sample-interval in seconden. Fallback naar 10 als &lt;= 0.</param>
    public void Update(RawStorageMode newMode, int newIntervalSeconds)
    {
        lock (_lockObject)
        {
            var oldMode = _rawStorageMode;
            var oldInterval = _sampleIntervalSeconds;

            _rawStorageMode = newMode;
            _sampleIntervalSeconds = newIntervalSeconds <= 0 ? 10 : newIntervalSeconds;

            // Wis timing-state bij mode-wijziging voor schone start
            if (_rawStorageMode != oldMode)
            {
                _lastMessageTimes.Clear();
                _logger.LogInformation(
                    "RawStorageMode updated from {OldMode} to {NewMode}. Timing state reset.",
                    oldMode, newMode);
            }

            // Log interval-wijziging
            if (_sampleIntervalSeconds != oldInterval)
            {
                _logger.LogInformation(
                    "DefaultSampleIntervalSeconds updated from {OldInterval} to {NewInterval}.",
                    oldInterval, _sampleIntervalSeconds);
            }
        }
    }
}
