namespace BootManager.Application.HeadingMeasurements.Services;

using DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementation van <see cref="IHeadingMeasurementService"/> met behulp van de generieke <see cref="IRepository{T}"/>.
/// Voert defensieve validatie uit en persisteert koersmetingen.
/// </summary>
public class HeadingMeasurementService : IHeadingMeasurementService
{
    private readonly IRepository<HeadingMeasurement> _repo;
    private readonly ILogger<HeadingMeasurementService> _logger;

    /// <summary>
    /// Creëert een nieuwe <see cref="HeadingMeasurementService"/>.
    /// </summary>
    public HeadingMeasurementService(IRepository<HeadingMeasurement> repo, ILogger<HeadingMeasurementService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<int> SaveAsync(CreateHeadingMeasurementRequestDto request, CancellationToken cancellationToken = default)
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

        if (request.HeadingDegrees < 0 || request.HeadingDegrees > 360)
        {
            throw new ArgumentException("HeadingDegrees moet tussen 0 en 360 graden liggen.", nameof(request.HeadingDegrees));
        }

        // Map DTO -> entity
        var entity = new HeadingMeasurement(
            recordedAtUtc: request.RecordedAtUtc,
            source: request.Source,
            messageId: request.MessageId,
            headingDegrees: request.HeadingDegrees
        );

        // Persist via generieke repository
        await _repo.AddAsync(entity, cancellationToken);

        _logger.LogInformation(
            "Koersmeting opgeslagen: Source={Source}, MessageId={MessageId}, Heading={Heading}°",
            entity.Source,
            entity.MessageId,
            entity.HeadingDegrees);

        return entity.Id;
    }
}
