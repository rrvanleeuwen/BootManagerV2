using BootManager.Application.Dashboard.DTOs;

namespace BootManager.Application.Dashboard.Services;

/// <summary>
/// Interface voor dashboard-service die actuele/recentste meetwaarden beschikbaar stelt.
/// </summary>
public interface IDashboardMeasurementService
{
    /// <summary>
    /// Haalt de recentste beschikbare meetwaarden op voor weergave op het dashboard.
    /// </summary>
    /// <param name="cancellationToken">Annuleringstoken.</param>
    /// <returns>DTO met alle beschikbare huidige meetwaarden; null-velden geven aan dat geen data beschikbaar is.</returns>
    Task<CurrentMeasurementsDto> GetCurrentMeasurementsAsync(CancellationToken cancellationToken = default);
}
