namespace BootManager.Tools.Ingest.Options;

/// <summary>
/// Configuratie voor optionele raw capture logging in de ingest-service.
/// Als <see cref="Enabled"/> op <c>true</c> staat, wordt elke ontvangen en verwerkte regel
/// als NDJSON-record weggeschreven naar een timestamped logbestand.
/// </summary>
public class CaptureLoggingOptions
{
    /// <summary>
    /// Geeft aan of capture logging ingeschakeld is.
    /// Standaard: <c>false</c>. Zet op <c>true</c> voor een boot-test.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Directory voor de capture logbestanden, relatief aan de werkdirectory van het Ingest-proces.
    /// Standaard: <c>logs/ingest-capture</c>.
    /// De directory wordt automatisch aangemaakt als die nog niet bestaat.
    /// </summary>
    public string Directory { get; set; } = "logs/ingest-capture";

    /// <summary>
    /// Voorvoegsel voor de bestandsnaam van het capture logbestand.
    /// Standaard: <c>ingest-capture</c>.
    /// De volledige bestandsnaam wordt: <c>{FilePrefix}-yyyyMMdd-HHmmss.ndjson</c>.
    /// </summary>
    public string FilePrefix { get; set; } = "ingest-capture";
}
