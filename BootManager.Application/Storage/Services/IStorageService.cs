using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Results;

namespace BootManager.Application.Storage.Services;

/// <summary>
/// Service voor het beheer van opslaggebieden en opslaglocaties.
/// </summary>
public interface IStorageService
{
    // --- StorageArea CRUD ---

    Task<IReadOnlyList<StorageAreaDto>> GetAllAreasAsync(CancellationToken ct = default);
    Task<StorageOperationResult<StorageAreaDto>> CreateAreaAsync(string name, CancellationToken ct = default);
    Task<StorageOperationResult> RenameAreaAsync(Guid areaId, string newName, CancellationToken ct = default);
    Task<StorageOperationResult> DeleteAreaAsync(Guid areaId, CancellationToken ct = default);

    // --- StorageLocation CRUD ---

    Task<IReadOnlyList<StorageLocationDto>> GetLocationsByAreaAsync(Guid areaId, CancellationToken ct = default);
    Task<StorageOperationResult<StorageLocationDto>> CreateLocationAsync(Guid areaId, string name, string? description, CancellationToken ct = default);
    Task<StorageOperationResult<StorageLocationDto>> UpdateLocationAsync(Guid locationId, string newName, string? newDescription, CancellationToken ct = default);
    Task<StorageOperationResult<StorageLocationDto>> MoveLocationAsync(Guid locationId, Guid newAreaId, CancellationToken ct = default);
    Task<StorageOperationResult> DeleteLocationAsync(Guid locationId, CancellationToken ct = default);

    // --- Detail view ---

    Task<StorageOperationResult<StorageLocationDetailDto>> GetLocationDetailAsync(Guid locationId, CancellationToken ct = default);

    // --- QR Token operations ---

    Task<StorageOperationResult<string>> GenerateOrGetQrTokenAsync(Guid locationId, CancellationToken ct = default);
    Task<QrResolutionResult> ResolveQrValueAsync(string? qrValue, CancellationToken ct = default);
    Task<StorageOperationResult> LinkQrToExistingLocationAsync(string token, Guid locationId, CancellationToken ct = default);
    Task<StorageOperationResult<StorageLocationDetailDto>> CreateLocationWithQrTokenAsync(
        Guid areaId, string name, string? description, string token, CancellationToken ct = default);
}
