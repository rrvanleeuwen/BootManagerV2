namespace BootManager.Application.FluidLevelMeasurements.Services;

using DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Implementatie van tankniveau-meting service met behulp van generieke repository.
/// </summary>
public class FluidLevelMeasurementService : IFluidLevelMeasurementService
{
    private readonly IRepository<FluidLevelMeasurement> _repo;
    private readonly ILogger<FluidLevelMeasurementService> _logger;

    /// <summary>
    /// Creëert een nieuwe FluidLevelMeasurementService.
    /// </summary>
    public FluidLevelMeasurementService(
        IRepository<FluidLevelMeasurement> repo,
        ILogger<FluidLevelMeasurementService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    /// <summary>
    /// Slaat een nieuwe tankniveau-meting op, met pragmatische de-duplicatie voor parallel PCDIN/MXPGN data.
    /// Als beide PCDIN en MXPGN dezelfde tank/type binnen dezelfde minuut meten, sla alleen PCDIN op.
    /// </summary>
    public async Task SaveAsync(CreateFluidLevelMeasurementRequestDto request, CancellationToken ct = default)
    {
        try
        {
            // De-duplication: check if a similar recent measurement exists (same minute, same tank)
            // and if this one is MXPGN (lower priority), skip it
            if (request.GatewaySentence?.Equals("MXPGN", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Check for recent PCDIN measurements in same minute for same tank
                var recentMeasurements = await _repo.ListAsync(
                    m => m.RecordedAtUtc.Year == request.RecordedAtUtc.Year &&
                         m.RecordedAtUtc.Month == request.RecordedAtUtc.Month &&
                         m.RecordedAtUtc.Day == request.RecordedAtUtc.Day &&
                         m.RecordedAtUtc.Hour == request.RecordedAtUtc.Hour &&
                         m.RecordedAtUtc.Minute == request.RecordedAtUtc.Minute &&
                         m.FluidType == request.FluidType &&
                         m.FluidInstance == request.FluidInstance &&
                         m.GatewaySentence == "PCDIN",
                    ct);

                if (recentMeasurements.Any())
                {
                    _logger.LogDebug(
                        "MXPGN-meting overgeslagen (duplicate van PCDIN): FluidType={FluidType}, Instance={Instance}",
                        request.FluidType,
                        request.FluidInstance);
                    return; // Skip MXPGN, keep PCDIN
                }
            }

            var entity = new FluidLevelMeasurement(
                recordedAtUtc: request.RecordedAtUtc,
                source: request.Source,
                messageId: request.MessageId,
                pgn: request.Pgn,
                fluidInstance: request.FluidInstance,
                fluidType: request.FluidType,
                rawFluidType: request.RawFluidType,
                levelPercent: request.LevelPercent,
                capacityLiters: request.CapacityLiters,
                isLevelInvalid: request.IsLevelInvalid,
                gatewaySentence: request.GatewaySentence,
                sourceAddress: request.SourceAddress);

            await _repo.AddAsync(entity, ct);

            _logger.LogInformation(
                "Tankniveau-meting opgeslagen: FluidType={FluidType}, Instance={Instance}, Level={Level}%, GatewaySentence={Gateway}",
                request.FluidType,
                request.FluidInstance,
                request.LevelPercent ?? -1,
                request.GatewaySentence);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Fout bij opslaan tankniveau-meting: FluidType={FluidType}, Instance={Instance}",
                request.FluidType,
                request.FluidInstance);
            throw;
        }
    }

    /// <summary>
    /// Haalt alle tankniveau-metingen op.
    /// </summary>
    public async Task<List<FluidLevelDto>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var entities = await _repo.ListAsync(ct: ct);
            return entities
                .OrderByDescending(f => f.RecordedAtUtc)
                .Select(MapToDto)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij ophalen alle tankniveau-metingen");
            throw;
        }
    }

    /// <summary>
    /// Haalt tankniveau-metingen voor een specifieke fluid type en instance op.
    /// </summary>
    public async Task<List<FluidLevelDto>> GetByFluidTypeAndInstanceAsync(
        FluidType fluidType,
        byte fluidInstance,
        CancellationToken ct = default)
    {
        try
        {
            var entities = await _repo.ListAsync(f => f.FluidType == fluidType && f.FluidInstance == fluidInstance, ct);
            return entities
                .OrderByDescending(f => f.RecordedAtUtc)
                .Select(MapToDto)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Fout bij ophalen tankniveau-metingen voor FluidType={FluidType}, Instance={Instance}",
                fluidType,
                fluidInstance);
            throw;
        }
    }

    /// <summary>
    /// Haalt de meest recente tankniveau-meting op voor een specifieke fluid type en instance.
    /// </summary>
    public async Task<FluidLevelDto?> GetLatestByFluidTypeAndInstanceAsync(
        FluidType fluidType,
        byte fluidInstance,
        CancellationToken ct = default)
    {
        try
        {
            var entities = await _repo.ListAsync(f => f.FluidType == fluidType && f.FluidInstance == fluidInstance, ct);
            var latest = entities
                .OrderByDescending(f => f.RecordedAtUtc)
                .FirstOrDefault();

            return latest != null ? MapToDto(latest) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Fout bij ophalen meest recente tankniveau-meting voor FluidType={FluidType}, Instance={Instance}",
                fluidType,
                fluidInstance);
            throw;
        }
    }

    private static FluidLevelDto MapToDto(FluidLevelMeasurement entity)
    {
        return new FluidLevelDto
        {
            Id = entity.Id,
            RecordedAtUtc = entity.RecordedAtUtc,
            Source = entity.Source,
            MessageId = entity.MessageId,
            Pgn = entity.Pgn,
            GatewaySentence = entity.GatewaySentence,
            SourceAddress = entity.SourceAddress,
            FluidInstance = entity.FluidInstance,
            FluidType = entity.FluidType,
            RawFluidType = entity.RawFluidType,
            LevelPercent = entity.LevelPercent,
            CapacityLiters = entity.CapacityLiters,
            IsLevelInvalid = entity.IsLevelInvalid
        };
    }
}
