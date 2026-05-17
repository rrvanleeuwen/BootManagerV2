namespace BootManager.Application.WaterTemperatureMeasurements.Services;

using DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementatie van <see cref="IWaterTemperatureMeasurementService"/> met behulp van de generieke <see cref="IRepository{T}"/>.
/// Voert defensieve validatie uit en persisteert watertemperatuur-metingen.
/// </summary>
public class WaterTemperatureMeasurementService : IWaterTemperatureMeasurementService
{
    private readonly IRepository<WaterTemperatureMeasurement> _repo;
    private readonly ILogger<WaterTemperatureMeasurementService> _logger;

    /// <summary>
    /// Creëert een nieuwe <see cref="WaterTemperatureMeasurementService"/>.
    /// </summary>
    public WaterTemperatureMeasurementService(
        IRepository<WaterTemperatureMeasurement> repo,
        ILogger<WaterTemperatureMeasurementService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<int> SaveAsync(CreateWaterTemperatureMeasurementRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Source))
        {
            throw new ArgumentException("Source mag niet leeg zijn.", nameof(request.Source));
        }

        if (string.IsNullOrWhiteSpace(request.MessageId))
        {
            throw new ArgumentException("MessageId mag niet leeg zijn.", nameof(request.MessageId));
        }

        if (request.TemperatureKelvin < 0)
        {
            throw new ArgumentException("TemperatureKelvin mag niet negatief zijn.", nameof(request.TemperatureKelvin));
        }

        var entity = new WaterTemperatureMeasurement(
            recordedAtUtc: request.RecordedAtUtc,
            source: request.Source,
            messageId: request.MessageId,
            temperatureInstance: request.TemperatureInstance,
            temperatureKelvin: request.TemperatureKelvin,
            temperatureCelsius: request.TemperatureCelsius);

        await _repo.AddAsync(entity, cancellationToken);

        _logger.LogInformation(
            "WaterTemperatureMeasurement opgeslagen: Id={Id}, TemperatureKelvin={K} K, TemperatureCelsius={C} °C",
            entity.Id,
            entity.TemperatureKelvin,
            entity.TemperatureCelsius);

        return entity.Id;
    }
}
