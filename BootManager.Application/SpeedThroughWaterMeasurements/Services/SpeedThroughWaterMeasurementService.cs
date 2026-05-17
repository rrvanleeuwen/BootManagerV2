namespace BootManager.Application.SpeedThroughWaterMeasurements.Services;

using DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementatie van <see cref="ISpeedThroughWaterMeasurementService"/> met behulp van de generieke <see cref="IRepository{T}"/>.
/// Voert defensieve validatie uit en persisteert snelheid-door-water-metingen.
/// </summary>
public class SpeedThroughWaterMeasurementService : ISpeedThroughWaterMeasurementService
{
    private readonly IRepository<SpeedThroughWaterMeasurement> _repo;
    private readonly ILogger<SpeedThroughWaterMeasurementService> _logger;

    /// <summary>
    /// Creëert een nieuwe <see cref="SpeedThroughWaterMeasurementService"/>.
    /// </summary>
    public SpeedThroughWaterMeasurementService(
        IRepository<SpeedThroughWaterMeasurement> repo,
        ILogger<SpeedThroughWaterMeasurementService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<int> SaveAsync(CreateSpeedThroughWaterMeasurementRequestDto request, CancellationToken cancellationToken = default)
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

        if (request.SpeedMetersPerSecond < 0)
        {
            throw new ArgumentException("SpeedMetersPerSecond mag niet negatief zijn.", nameof(request.SpeedMetersPerSecond));
        }

        if (request.SpeedKnots < 0)
        {
            throw new ArgumentException("SpeedKnots mag niet negatief zijn.", nameof(request.SpeedKnots));
        }

        var entity = new SpeedThroughWaterMeasurement(
            recordedAtUtc: request.RecordedAtUtc,
            source: request.Source,
            messageId: request.MessageId,
            speedMetersPerSecond: request.SpeedMetersPerSecond,
            speedKnots: request.SpeedKnots,
            speedWaterReferenceType: request.SpeedWaterReferenceType
        );

        await _repo.AddAsync(entity, cancellationToken);

        _logger.LogInformation(
            "Snelheid-door-water-meting opgeslagen: Source={Source}, MessageId={MessageId}, SpeedMps={Mps} m/s, SpeedKnots={Knots} kn",
            entity.Source,
            entity.MessageId,
            entity.SpeedMetersPerSecond,
            entity.SpeedKnots);

        return entity.Id;
    }
}
