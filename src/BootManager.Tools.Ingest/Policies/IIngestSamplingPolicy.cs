namespace BootManager.Tools.Ingest.Policies;

using BootManager.Core.Enums;

/// <summary>
/// Interface voor het sampling-beleid van ruwe netwerkberichten.
/// Bepaalt of een ontvangen bericht moet worden doorgelaten naar de API
/// op basis van RawStorageMode en sample-interval.
/// </summary>
public interface IIngestSamplingPolicy
{
    /// <summary>
    /// Bepaalt of een ontvangen bericht mag worden doorgelaten naar de API
    /// op basis van protocol, messageId, en de geconfigureerde sampling-instellingen.
    /// </summary>
    /// <param name="protocol">Het gedetecteerde protocol (bijv. "NMEA0183", "NMEA2000").</param>
    /// <param name="messageId">De bericht-ID (bijv. NMEA0183 sentence type of NMEA2000 PGN), 
    /// of null/empty als niet bepaald.</param>
    /// <returns>true als het bericht mag worden gepost naar de API, false als het moet worden overgeslagen.</returns>
    bool ShouldProcessMessage(string protocol, string? messageId);

    /// <summary>
    /// Reset alle interne timing-state.
    /// Bruikbaar voor testing of als de policy moet worden vernieuwd.
    /// </summary>
    void Reset();

    /// <summary>
    /// Update de sampling policy met nieuwe RawStorageMode en interval.
    /// Dit kan veilig worden aangeroepen terwijl berichten worden verwerkt (thread-safe).
    /// </summary>
    /// <param name="newMode">De nieuwe RawStorageMode.</param>
    /// <param name="newIntervalSeconds">Het nieuwe sample-interval in seconden. Fallback naar 10 als &lt;= 0.</param>
    void Update(RawStorageMode newMode, int newIntervalSeconds);
}

