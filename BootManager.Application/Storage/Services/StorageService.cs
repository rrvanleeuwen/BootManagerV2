using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Results;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;

namespace BootManager.Application.Storage.Services;

/// <summary>
/// Service voor het beheer van opslaggebieden en opslaglocaties.
/// Handelt validatie, trimmen, en uniqueness af.
/// </summary>
public class StorageService : IStorageService
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;

    private readonly IRepository<StorageArea> _areaRepo;
    private readonly IRepository<StorageLocation> _locationRepo;

    public StorageService(
        IRepository<StorageArea> areaRepo,
        IRepository<StorageLocation> locationRepo)
    {
        _areaRepo = areaRepo;
        _locationRepo = locationRepo;
    }

    // --- StorageArea CRUD ---

    public async Task<IReadOnlyList<StorageAreaDto>> GetAllAreasAsync(CancellationToken ct = default)
    {
        var areas = await _areaRepo.ListAsync(ct: ct);
        return areas.Select(a => new StorageAreaDto { Id = a.Id, Name = a.Name }).ToList();
    }

    public async Task<StorageOperationResult<StorageAreaDto>> CreateAreaAsync(string name, CancellationToken ct = default)
    {
        var trimmedName = name?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedName))
            return StorageOperationResult<StorageAreaDto>.Error("Gebiedsnaam mag niet leeg zijn.");

        if (trimmedName.Length > MaxNameLength)
            return StorageOperationResult<StorageAreaDto>.Error($"Gebiedsnaam mag maximaal {MaxNameLength} tekens lang zijn.");

        var normalizedName = trimmedName.ToLowerInvariant();
        var existing = await _areaRepo.SingleOrDefaultAsync(
            a => a.NormalizedName == normalizedName, ct);

        if (existing != null)
            return StorageOperationResult<StorageAreaDto>.Error("Gebiedsnaam bestaat al.");

        var newArea = StorageArea.Create(trimmedName);
        await _areaRepo.AddAsync(newArea, ct);

        return StorageOperationResult<StorageAreaDto>.Ok(new StorageAreaDto
        {
            Id = newArea.Id,
            Name = newArea.Name
        });
    }

    public async Task<StorageOperationResult> RenameAreaAsync(Guid areaId, string newName, CancellationToken ct = default)
    {
        var area = await _areaRepo.GetByIdAsync(areaId, ct);
        if (area == null)
            return StorageOperationResult.Error("Gebied niet gevonden.");

        var trimmedName = newName?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedName))
            return StorageOperationResult.Error("Gebiedsnaam mag niet leeg zijn.");

        if (trimmedName.Length > MaxNameLength)
            return StorageOperationResult.Error($"Gebiedsnaam mag maximaal {MaxNameLength} tekens lang zijn.");

        var normalizedName = trimmedName.ToLowerInvariant();
        if (area.NormalizedName != normalizedName)
        {
            var duplicate = await _areaRepo.SingleOrDefaultAsync(
                a => a.NormalizedName == normalizedName && a.Id != areaId, ct);
            if (duplicate != null)
                return StorageOperationResult.Error("Gebiedsnaam bestaat al.");
        }

        area.UpdateName(trimmedName);
        await _areaRepo.UpdateAsync(area, ct);
        return StorageOperationResult.Ok();
    }

    public async Task<StorageOperationResult> DeleteAreaAsync(Guid areaId, CancellationToken ct = default)
    {
        var area = await _areaRepo.GetByIdAsync(areaId, ct);
        if (area == null)
            return StorageOperationResult.Error("Gebied niet gevonden.");

        var locationCount = await _locationRepo.CountAsync(l => l.StorageAreaId == areaId, ct);
        if (locationCount > 0)
            return StorageOperationResult.Error("Gebied bevat locaties en kan niet worden verwijderd.");

        await _areaRepo.DeleteAsync(area, ct);
        return StorageOperationResult.Ok();
    }

    // --- StorageLocation CRUD ---

    public async Task<IReadOnlyList<StorageLocationDto>> GetLocationsByAreaAsync(Guid areaId, CancellationToken ct = default)
    {
        var locations = await _locationRepo.ListAsync(l => l.StorageAreaId == areaId, ct);
        return locations.Select(l => new StorageLocationDto
        {
            Id = l.Id,
            StorageAreaId = l.StorageAreaId,
            Name = l.Name,
            Description = l.Description
        }).ToList();
    }

    public async Task<StorageOperationResult<StorageLocationDto>> CreateLocationAsync(
        Guid areaId, string name, string? description, CancellationToken ct = default)
    {
        var area = await _areaRepo.GetByIdAsync(areaId, ct);
        if (area == null)
            return StorageOperationResult<StorageLocationDto>.Error("Gebied niet gevonden.");

        var trimmedName = name?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedName))
            return StorageOperationResult<StorageLocationDto>.Error("Locatienaam mag niet leeg zijn.");

        if (trimmedName.Length > MaxNameLength)
            return StorageOperationResult<StorageLocationDto>.Error($"Locatienaam mag maximaal {MaxNameLength} tekens lang zijn.");

        if (!string.IsNullOrEmpty(description) && description.Trim().Length > MaxDescriptionLength)
            return StorageOperationResult<StorageLocationDto>.Error($"Beschrijving mag maximaal {MaxDescriptionLength} tekens lang zijn.");

        var normalizedName = trimmedName.ToLowerInvariant();
        var duplicate = await _locationRepo.SingleOrDefaultAsync(
            l => l.StorageAreaId == areaId && l.NormalizedName == normalizedName, ct);

        if (duplicate != null)
            return StorageOperationResult<StorageLocationDto>.Error("Locatienaam bestaat al in dit gebied.");

        var newLocation = StorageLocation.Create(areaId, trimmedName, description);
        await _locationRepo.AddAsync(newLocation, ct);

        return StorageOperationResult<StorageLocationDto>.Ok(new StorageLocationDto
        {
            Id = newLocation.Id,
            StorageAreaId = newLocation.StorageAreaId,
            Name = newLocation.Name,
            Description = newLocation.Description
        });
    }

    public async Task<StorageOperationResult<StorageLocationDto>> UpdateLocationAsync(
        Guid locationId, string newName, string? newDescription, CancellationToken ct = default)
    {
        var location = await _locationRepo.GetByIdAsync(locationId, ct);
        if (location == null)
            return StorageOperationResult<StorageLocationDto>.Error("Locatie niet gevonden.");

        var trimmedName = newName?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedName))
            return StorageOperationResult<StorageLocationDto>.Error("Locatienaam mag niet leeg zijn.");

        if (trimmedName.Length > MaxNameLength)
            return StorageOperationResult<StorageLocationDto>.Error($"Locatienaam mag maximaal {MaxNameLength} tekens lang zijn.");

        if (!string.IsNullOrEmpty(newDescription) && newDescription.Trim().Length > MaxDescriptionLength)
            return StorageOperationResult<StorageLocationDto>.Error($"Beschrijving mag maximaal {MaxDescriptionLength} tekens lang zijn.");

        var normalizedName = trimmedName.ToLowerInvariant();
        if (location.NormalizedName != normalizedName)
        {
            var duplicate = await _locationRepo.SingleOrDefaultAsync(
                l => l.StorageAreaId == location.StorageAreaId &&
                     l.NormalizedName == normalizedName &&
                     l.Id != locationId, ct);
            if (duplicate != null)
                return StorageOperationResult<StorageLocationDto>.Error("Locatienaam bestaat al in dit gebied.");
        }

        location.UpdateNameAndDescription(trimmedName, newDescription);
        await _locationRepo.UpdateAsync(location, ct);

        return StorageOperationResult<StorageLocationDto>.Ok(new StorageLocationDto
        {
            Id = location.Id,
            StorageAreaId = location.StorageAreaId,
            Name = location.Name,
            Description = location.Description
        });
    }

    public async Task<StorageOperationResult<StorageLocationDto>> MoveLocationAsync(
        Guid locationId, Guid newAreaId, CancellationToken ct = default)
    {
        var location = await _locationRepo.GetByIdAsync(locationId, ct);
        if (location == null)
            return StorageOperationResult<StorageLocationDto>.Error("Locatie niet gevonden.");

        var newArea = await _areaRepo.GetByIdAsync(newAreaId, ct);
        if (newArea == null)
            return StorageOperationResult<StorageLocationDto>.Error("Doelgebied niet gevonden.");

        // Controleer op duplicate in het nieuwe gebied
        var duplicate = await _locationRepo.SingleOrDefaultAsync(
            l => l.StorageAreaId == newAreaId && l.NormalizedName == location.NormalizedName, ct);
        if (duplicate != null)
            return StorageOperationResult<StorageLocationDto>.Error("Locatienaam bestaat al in het doelgebied.");

        location.MoveToArea(newAreaId);
        await _locationRepo.UpdateAsync(location, ct);

        return StorageOperationResult<StorageLocationDto>.Ok(new StorageLocationDto
        {
            Id = location.Id,
            StorageAreaId = location.StorageAreaId,
            Name = location.Name,
            Description = location.Description
        });
    }

    public async Task<StorageOperationResult> DeleteLocationAsync(Guid locationId, CancellationToken ct = default)
    {
        var location = await _locationRepo.GetByIdAsync(locationId, ct);
        if (location == null)
            return StorageOperationResult.Error("Locatie niet gevonden.");

        await _locationRepo.DeleteAsync(location, ct);
        return StorageOperationResult.Ok();
    }

    // --- Detail view ---

    public async Task<StorageOperationResult<StorageLocationDetailDto>> GetLocationDetailAsync(
        Guid locationId, CancellationToken ct = default)
    {
        var location = await _locationRepo.GetByIdAsync(locationId, ct);
        if (location == null)
            return StorageOperationResult<StorageLocationDetailDto>.NotFound();

        var area = await _areaRepo.GetByIdAsync(location.StorageAreaId, ct);
        if (area == null)
            return StorageOperationResult<StorageLocationDetailDto>.NotFound();

        return StorageOperationResult<StorageLocationDetailDto>.Ok(new StorageLocationDetailDto
        {
            Id = location.Id,
            AreaName = area.Name,
            LocationName = location.Name,
            Description = location.Description
        });
    }
}
