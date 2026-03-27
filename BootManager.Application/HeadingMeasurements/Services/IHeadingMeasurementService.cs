namespace BootManager.Application.HeadingMeasurements.Services;

using DTOs;

/// <summary>
/// Interface voor application-service die HeadingMeasurement use-cases aanbiedt.
/// </summary>
public interface IHeadingMeasurementService
{
    /// <summary>
    /// Slaat een koersmeting op en retourneert het Id van het aangemaakte record.
    /// </summary>
    /// <param name="request">De DTO met meting-gegevens.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Het Id van het aangemaakte HeadingMeasurement-record.</returns>
    Task<int> SaveAsync(CreateHeadingMeasurementRequestDto request, CancellationToken cancellationToken = default);
}
