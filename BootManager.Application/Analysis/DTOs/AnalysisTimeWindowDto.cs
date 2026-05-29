namespace BootManager.Application.Analysis.DTOs;

/// <summary>
/// DTO voor een analysetijdsvenster.
/// </summary>
public class AnalysisTimeWindowDto
{
    /// <summary>
    /// Begintijd van het analyse-venster (UTC).
    /// </summary>
    public DateTime StartUtc { get; set; }

    /// <summary>
    /// Eindtijd van het analyse-venster (UTC).
    /// </summary>
    public DateTime EndUtc { get; set; }
}
