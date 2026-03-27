using BootManager.Application.MotionMeasurements.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace BootManager.Application.MotionMeasurements.Services;

/// <summary>
/// Interface voor application-service die MotionMeasurement use-cases aanbiedt.
/// </summary>
public interface IMotionMeasurementService
{
    /// <summary>
    /// Slaat een bewegingsmeting op en retourneert het Id van het aangemaakte record.
    /// </summary>
    /// <param name="request">De DTO met meting-gegevens.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Het Id van het aangemaakte MotionMeasurement-record.</returns>
    Task<int> SaveAsync(CreateMotionMeasurementRequestDto request, CancellationToken cancellationToken = default);
}
