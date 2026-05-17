using BootManager.Tools.Ingest.Models;

namespace BootManager.Tools.Ingest.Services;

/// <summary>
/// Interface voor optionele raw capture logging van ingest-regels.
/// Implementaties schrijven elk ontvangen en verwerkt regelrecord asynchroon weg als NDJSON.
/// </summary>
public interface IIngestCaptureLogger : IAsyncDisposable
{
    /// <summary>
    /// Geeft aan of capture logging ingeschakeld is.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Initialiseert de capture logger: maakt de directory en het logbestand aan indien nodig.
    /// Moet eenmalig worden aangeroepen bij het starten van de ingest-service.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Schrijft een <see cref="CaptureRecord"/> asynchroon weg als NDJSON-regel.
    /// Fouten bij het schrijven worden gelogd maar blokkeren de aanroeper niet.
    /// </summary>
    /// <param name="record">Het te loggen capture-record.</param>
    Task WriteAsync(CaptureRecord record);
}
