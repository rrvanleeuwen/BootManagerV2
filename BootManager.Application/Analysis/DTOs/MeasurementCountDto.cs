namespace BootManager.Application.Analysis.DTOs;

/// <summary>
/// DTO voor een telresultaat van een specifiek meettype.
/// </summary>
public class MeasurementCountDto
{
    /// <summary>
    /// Naam van het meettype (bijv. "Battery", "Heading", "Position").
    /// </summary>
    public string MeasurementType { get; set; } = string.Empty;

    /// <summary>
    /// Aantal metingen van dit type in het analyse-venster.
    /// </summary>
    public int Count { get; set; }
}
