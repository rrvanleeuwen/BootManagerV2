using BootManager.Application.BatteryMeasurements.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BootManager.Application.BatteryMeasurements.Services;

/// <summary>
/// Implementation van <see cref="IBatteryMeasurementService"/> met behulp van de generieke <see cref="IRepository{T}"/>.
/// Voert defensieve validatie uit en persisteert batterijmetingen.
/// </summary>
public class BatteryMeasurementService : IBatteryMeasurementService
{
    private readonly IRepository<BatteryMeasurement> _repo;
    private readonly ILogger<BatteryMeasurementService> _logger;

    /// <summary>
    /// Creëert een nieuwe <see cref="BatteryMeasurementService"/>.
    /// </summary>
    public BatteryMeasurementService(IRepository<BatteryMeasurement> repo, ILogger<BatteryMeasurementService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<int> SaveAsync(CreateBatteryMeasurementRequestDto request, CancellationToken cancellationToken = default)
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

        if (request.Voltage <= 0)
        {
            throw new ArgumentException("Voltage moet groter dan 0 zijn.", nameof(request.Voltage));
        }

        // Map DTO -> entity
        var entity = new BatteryMeasurement(
            recordedAtUtc: request.RecordedAtUtc,
            source: request.Source,
            messageId: request.MessageId,
            voltage: request.Voltage,
            stateOfCharge: request.StateOfCharge
        );

        // Persist via generieke repository
        // AddAsync retourneert Task (geen waarde); EF Core werkt de entity in-place bij
        await _repo.AddAsync(entity, cancellationToken);

        _logger.LogInformation(
            "Batterijmeting opgeslagen: Source={Source}, MessageId={MessageId}, Voltage={Voltage}V, SOC={SOC}%",
            entity.Source,
            entity.MessageId,
            entity.Voltage,
            entity.StateOfCharge ?? 0);

        return entity.Id;
    }
}