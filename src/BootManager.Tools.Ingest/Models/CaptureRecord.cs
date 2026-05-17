namespace BootManager.Tools.Ingest.Models;

/// <summary>
/// NDJSON-record voor raw capture logging in de ingest-service.
/// Eén instantie vertegenwoordigt één ontvangen regel.
/// API-resultaatvelden zijn optioneel, omdat raw capture vóór de API-post wordt weggeschreven.
/// </summary>
public class CaptureRecord
{
    /// <summary>
    /// Tijdstip van ontvangst in UTC.
    /// </summary>
    public DateTime ReceivedAtUtc { get; set; }

    /// <summary>
    /// Remote endpoint (IP:poort) van de afzender.
    /// </summary>
    public string RemoteEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gedetecteerd protocol: "NMEA0183" of "NMEA2000".
    /// </summary>
    public string DetectedProtocol { get; set; } = string.Empty;

    /// <summary>
    /// De originele ontvangen regelinhoud.
    /// </summary>
    public string RawLine { get; set; } = string.Empty;

    /// <summary>
    /// Bericht-ID indien aanwezig (NMEA 2000); <c>null</c> voor NMEA 0183.
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// Hex-payload indien aanwezig (NMEA 2000); <c>null</c> voor NMEA 0183.
    /// </summary>
    public string? PayloadHex { get; set; }

    /// <summary>
    /// Geeft aan of de API-post succesvol was, indien dit resultaat is vastgelegd.
    /// </summary>
    public bool? ApiPostSucceeded { get; set; }

    /// <summary>
    /// HTTP-statuscode van de API-post response, indien beschikbaar.
    /// </summary>
    public int? ApiStatusCode { get; set; }

    /// <summary>
    /// Foutmelding indien de API-post mislukt is; anders <c>null</c>.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
