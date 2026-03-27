using BootManager.Application.MotionMeasurements.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BootManager.Application.MotionMeasurements.Services;

/// <summary>
/// Implementation van <see cref="IMotionMeasurementService"/> met behulp van de generieke <see cref="IRepository{T}"/>.
/// Voert defensieve validatie uit en persisteert bewegingsmetingen.
/// </summary>
public class MotionMeasurementService : IMotionMeasurementService
{
    private readonly IRepository<MotionMeasurement> _repo;
    private readonly ILogger<MotionMeasurementService> _logger;

    /// <summary>
    /// Creëert een nieuwe <see cref="MotionMeasurementService"/>.
    /// </summary>
    public MotionMeasurementService(IRepository<MotionMeasurement> repo, ILogger<MotionMeasurementService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<int> SaveAsync(CreateMotionMeasurementRequestDto request, CancellationToken cancellationToken = default)
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

        if (request.CourseOverGroundDegrees < 0 || request.CourseOverGroundDegrees >= 360)
        {
            throw new ArgumentException("CourseOverGroundDegrees moet tussen 0 en 360 graden liggen.", nameof(request.CourseOverGroundDegrees));
        }

        if (request.SpeedOverGround < 0)
        {
            throw new ArgumentException("SpeedOverGround mag niet negatief zijn.", nameof(request.SpeedOverGround));
        }

        if (string.IsNullOrWhiteSpace(request.SpeedUnit))
        {
            throw new ArgumentException("SpeedUnit mag niet leeg zijn.", nameof(request.SpeedUnit));
        }

        // Map DTO -> entity
        var entity = new MotionMeasurement(
            recordedAtUtc: request.RecordedAtUtc,
            source: request.Source,
            messageId: request.MessageId,
            courseOverGroundDegrees: request.CourseOverGroundDegrees,
            speedOverGround: request.SpeedOverGround,
            speedUnit: request.SpeedUnit
        );

        // Persist via generieke repository
        await _repo.AddAsync(entity, cancellationToken);

        _logger.LogInformation(
            "Bewegingsmeting opgeslagen: Source={Source}, MessageId={MessageId}, COG={COG}°, SOG={SOG}{Unit}",
            entity.Source,
            entity.MessageId,
            entity.CourseOverGroundDegrees,
            entity.SpeedOverGround,
            entity.SpeedUnit);

        return entity.Id;
    }
}
