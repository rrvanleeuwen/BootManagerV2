namespace BootManager.Application.WindMeasurements.Services;

using DTOs;

/// <summary>
/// Interface voor application-service die WindMeasurement use-cases aanbiedt.
/// </summary>
public interface IWindMeasurementService
{
    /// <summary>
    /// Slaat een windmeting op en retourneert het Id van het aangemaakte record.
    /// </summary>
    /// <param name="request">De DTO met meting-gegevens.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Het Id van het aangemaakte WindMeasurement-record.</returns>
    Task<int> SaveAsync(CreateWindMeasurementRequestDto request, CancellationToken cancellationToken = default);
}