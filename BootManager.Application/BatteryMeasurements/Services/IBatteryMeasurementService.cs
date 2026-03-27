using BootManager.Application.BatteryMeasurements.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace BootManager.Application.BatteryMeasurements.Services;

/// <summary>
/// Interface voor application-service die BatteryMeasurement use-cases aanbiedt.
/// </summary>
public interface IBatteryMeasurementService
{
    /// <summary>
    /// Slaat een batterijmeting op en retourneert het Id van het aangemaakte record.
    /// </summary>
    /// <param name="request">De DTO met meting-gegevens.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Het Id van het aangemaakte BatteryMeasurement-record.</returns>
    Task<int> SaveAsync(CreateBatteryMeasurementRequestDto request, CancellationToken cancellationToken = default);
}