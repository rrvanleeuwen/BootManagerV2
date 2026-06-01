namespace BootManager.Application.FluidLevelMeasurements.Services;

using DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Interface voor het beheren van tankniveau-metingen.
/// </summary>
public interface IFluidLevelMeasurementService
{
    /// <summary>
    /// Slaat een nieuwe tankniveau-meting op.
    /// </summary>
    Task SaveAsync(CreateFluidLevelMeasurementRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Haalt alle tankniveau-metingen op.
    /// </summary>
    Task<List<FluidLevelDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Haalt tankniveau-metingen voor een specifieke fluid type en instance op.
    /// </summary>
    Task<List<FluidLevelDto>> GetByFluidTypeAndInstanceAsync(
        BootManager.Core.Entities.FluidType fluidType,
        byte fluidInstance,
        CancellationToken ct = default);

    /// <summary>
    /// Haalt de meest recente tankniveau-meting op voor een specifieke fluid type en instance.
    /// </summary>
    Task<FluidLevelDto?> GetLatestByFluidTypeAndInstanceAsync(
        BootManager.Core.Entities.FluidType fluidType,
        byte fluidInstance,
        CancellationToken ct = default);
}
