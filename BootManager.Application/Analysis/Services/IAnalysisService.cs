using BootManager.Application.Analysis.DTOs;

namespace BootManager.Application.Analysis.Services;

/// <summary>
/// Interface voor analyse-service die diagnostische informatie over een tijdsvenster biedt.
/// </summary>
public interface IAnalysisService
{
    /// <summary>
    /// Haalt een analyse-samenvatting op voor een gegeven tijdsvenster.
    /// </summary>
    /// <param name="timeWindow">Het analyse-venster (van/tot UTC).</param>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>Een samenvatting met aantallen NetworkMessages en Measurements per type.</returns>
    Task<AnalysisSummaryDto> GetAnalysisSummaryAsync(AnalysisTimeWindowDto timeWindow, CancellationToken ct = default);

    /// <summary>
    /// Exporteert de analyse-samenvatting als CSV-tekst.
    /// </summary>
    /// <param name="summary">De analyse-samenvatting.</param>
    /// <returns>CSV-inhoud als string.</returns>
    string ExportAsCSV(AnalysisSummaryDto summary);

    /// <summary>
    /// Exporteert de analyse-samenvatting als JSON-tekst.
    /// </summary>
    /// <param name="summary">De analyse-samenvatting.</param>
    /// <returns>JSON-inhoud als string.</returns>
    string ExportAsJSON(AnalysisSummaryDto summary);
}
