namespace BootManager.Application.DepthMeasurements.Services;

using DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Implementation van <see cref="IDepthMeasurementService"/> met behulp van de generieke <see cref="IRepository{T}"/>.
/// Voert defensieve validatie uit en persisteert dieptemetingen.
/// </summary>
public class DepthMeasurementService : IDepthMeasurementService
{
    private readonly IRepository<DepthMeasurement> _repo;
    private readonly ILogger<DepthMeasurementService> _logger;

    /// <summary>
    /// Creëert een nieuwe <see cref="DepthMeasurementService"/>.
    /// </summary>
    public DepthMeasurementService(IRepository<DepthMeasurement> repo, ILogger<DepthMeasurementService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<int> SaveAsync(CreateDepthMeasurementRequestDto request, CancellationToken cancellationToken = default)
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

        if (request.DepthMeters < 0)
        {
            throw new ArgumentException("DepthMeters mag niet negatief zijn.", nameof(request.DepthMeters));
        }

        // Map DTO -> entity
        var entity = new DepthMeasurement(
            recordedAtUtc: request.RecordedAtUtc,
            source: request.Source,
            messageId: request.MessageId,
            depthMeters: request.DepthMeters
        );

        // Persist via generieke repository
        // AddAsync retourneert Task (geen waarde); EF Core werkt de entity in-place bij
        await _repo.AddAsync(entity, cancellationToken);

        _logger.LogInformation(
            "Dieptemeting opgeslagen: Source={Source}, MessageId={MessageId}, DepthMeters={Depth}m",
            entity.Source,
            entity.MessageId,
            entity.DepthMeters);

        return entity.Id;
    }
}