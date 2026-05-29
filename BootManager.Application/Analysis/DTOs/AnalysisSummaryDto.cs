namespace BootManager.Application.Analysis.DTOs;

/// <summary>
/// DTO voor een analyse-samenvatting over een tijdsvenster.
/// </summary>
public class AnalysisSummaryDto
{
    /// <summary>
    /// Begintijd van het analyse-venster (UTC).
    /// </summary>
    public DateTime StartUtc { get; set; }

    /// <summary>
    /// Eindtijd van het analyse-venster (UTC).
    /// </summary>
    public DateTime EndUtc { get; set; }

    /// <summary>
    /// Totaal aantal ruwe netwerkberichten in het venster.
    /// </summary>
    public int TotalNetworkMessages { get; set; }

    /// <summary>
    /// Overzicht van aantallen per meettype.
    /// </summary>
    public List<MeasurementCountDto> MeasurementCounts { get; set; } = new();

    /// <summary>
    /// Opmerking over warning/error-beschikbaarheid (momenteel niet persistent gelogd).
    /// </summary>
    public string WarningErrorsStatus { get; set; } = "Waarschuwingen en fouten worden momenteel niet permanent opgeslagen in de database.";
}
