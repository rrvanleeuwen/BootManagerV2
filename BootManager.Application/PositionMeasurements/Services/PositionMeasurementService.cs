using BootManager.Application.PositionMeasurements.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BootManager.Application.PositionMeasurements.Services;

/// <summary>
/// Implementation van <see cref="IPositionMeasurementService"/> met behulp van de generieke <see cref="IRepository{T}"/>.
/// Voert defensieve validatie uit en persisteert positiemetingen.
/// </summary>
public class PositionMeasurementService : IPositionMeasurementService
{
    private readonly IRepository<PositionMeasurement> _repo;
    private readonly ILogger<PositionMeasurementService> _logger;

    /// <summary>
    /// Creëert een nieuwe <see cref="PositionMeasurementService"/>.
    /// </summary>
    public PositionMeasurementService(IRepository<PositionMeasurement> repo, ILogger<PositionMeasurementService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<int> SaveAsync(CreatePositionMeasurementRequestDto request, CancellationToken cancellationToken = default)
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

        // Valideer coördinaten (basisvalidatie: breedtegraad -90 tot 90, lengtegraad -180 tot 180)
        if (request.Latitude < -90 || request.Latitude > 90)
        {
            throw new ArgumentException("Latitude moet tussen -90 en 90 liggen.", nameof(request.Latitude));
        }

        if (request.Longitude < -180 || request.Longitude > 180)
        {
            throw new ArgumentException("Longitude moet tussen -180 en 180 liggen.", nameof(request.Longitude));
        }

        // Map DTO -> entity
        var entity = new PositionMeasurement(
            recordedAtUtc: request.RecordedAtUtc,
            source: request.Source,
            messageId: request.MessageId,
            latitude: request.Latitude,
            longitude: request.Longitude
        );

        // Persist via generieke repository
        await _repo.AddAsync(entity, cancellationToken);

        _logger.LogInformation(
            "Positiemeting opgeslagen: Source={Source}, MessageId={MessageId}, Latitude={Latitude}, Longitude={Longitude}",
            entity.Source,
            entity.MessageId,
            entity.Latitude,
            entity.Longitude);

        return entity.Id;
    }
}
