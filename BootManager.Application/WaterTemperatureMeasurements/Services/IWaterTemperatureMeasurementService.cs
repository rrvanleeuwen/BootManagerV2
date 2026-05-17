namespace BootManager.Application.WaterTemperatureMeasurements.Services;

using DTOs;

/// <summary>
/// Contract voor het opslaan van watertemperatuur-metingen.
/// </summary>
public interface IWaterTemperatureMeasurementService
{
    /// <summary>
    /// Persisteert een watertemperatuur-meting en retourneert het gegenereerde ID.
    /// </summary>
    /// <param name="request">De gegevens van de te persisteren meting.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Het ID van de opgeslagen meting.</returns>
    Task<int> SaveAsync(CreateWaterTemperatureMeasurementRequestDto request, CancellationToken cancellationToken = default);
}
