namespace BootManager.Application.WindMeasurements.Services;

using DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementation van <see cref="IWindMeasurementService"/> met behulp van de generieke <see cref="IRepository{T}"/>.
/// Voert defensieve validatie uit en persisteert windmetingen.
/// </summary>
public class WindMeasurementService : IWindMeasurementService
{
    private readonly IRepository<WindMeasurement> _repo;
    private readonly ILogger<WindMeasurementService> _logger;

    /// <summary>
    /// Creëert een nieuwe <see cref="WindMeasurementService"/>.
    /// </summary>
    public WindMeasurementService(IRepository<WindMeasurement> repo, ILogger<WindMeasurementService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<int> SaveAsync(CreateWindMeasurementRequestDto request, CancellationToken cancellationToken = default)
    {
        // Defensieve validatie van verplichte velden
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

        if (string.IsNullOrWhiteSpace(request.SpeedUnit))
        {
            throw new ArgumentException("SpeedUnit mag niet leeg zijn.", nameof(request.SpeedUnit));
        }

        if (request.WindSpeed < 0)
        {
            throw new ArgumentException("WindSpeed mag niet negatief zijn.", nameof(request.WindSpeed));
        }

        if (request.WindAngleDegrees < 0 || request.WindAngleDegrees > 360)
        {
            throw new ArgumentException("WindAngleDegrees moet tussen 0 en 360 graden liggen.", nameof(request.WindAngleDegrees));
        }

        // Map DTO -> entity
        var entity = new WindMeasurement(
            recordedAtUtc: request.RecordedAtUtc,
            source: request.Source,
            messageId: request.MessageId,
            windAngleDegrees: request.WindAngleDegrees,
            windSpeed: request.WindSpeed,
            speedUnit: request.SpeedUnit
        );

        // Persist via generieke repository
        await _repo.AddAsync(entity, cancellationToken);

        _logger.LogInformation(
            "Windmeting opgeslagen: Source={Source}, MessageId={MessageId}, Angle={Angle}°, Speed={Speed}{Unit}",
            entity.Source,
            entity.MessageId,
            entity.WindAngleDegrees,
            entity.WindSpeed,
            entity.SpeedUnit);

        return entity.Id;
    }
}