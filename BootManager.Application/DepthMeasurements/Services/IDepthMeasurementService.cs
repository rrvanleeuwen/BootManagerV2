namespace BootManager.Application.DepthMeasurements.Services;

using DTOs;

/// <summary>
/// Interface voor application-service die DepthMeasurement use-cases aanbiedt.
/// </summary>
public interface IDepthMeasurementService
{
    /// <summary>
    /// Slaat een dieptemeting op en retourneert het Id van het aangemaakte record.
    /// </summary>
    /// <param name="request">De DTO met meting-gegevens.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Het Id van het aangemaakte DepthMeasurement-record.</returns>
    Task<int> SaveAsync(CreateDepthMeasurementRequestDto request, CancellationToken cancellationToken = default);
}