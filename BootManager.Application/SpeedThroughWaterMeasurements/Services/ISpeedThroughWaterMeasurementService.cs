namespace BootManager.Application.SpeedThroughWaterMeasurements.Services;

using DTOs;

/// <summary>
/// Contract voor de service die snelheid-door-water-metingen persisteert.
/// </summary>
public interface ISpeedThroughWaterMeasurementService
{
    /// <summary>
    /// Slaat een nieuwe snelheid-door-water-meting op in de database.
    /// </summary>
    /// <param name="request">De meting die opgeslagen moet worden.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Het gegenereerde Id van de opgeslagen meting.</returns>
    Task<int> SaveAsync(CreateSpeedThroughWaterMeasurementRequestDto request, CancellationToken cancellationToken = default);
}
