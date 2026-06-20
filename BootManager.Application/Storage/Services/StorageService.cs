using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.QrFormat;
using BootManager.Application.Storage.Results;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
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
    private readonly IStockService _stockService;

    public StorageService(
        IRepository<StorageArea> areaRepo,
        IRepository<StorageLocation> locationRepo,
        IStockService stockService)
    {
        _areaRepo = areaRepo;
        _locationRepo = locationRepo;
        _stockService = stockService;
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

        var qrValue = location.QrToken != null ? LocationQrValue.FormatQrValue(location.QrToken) : null;

        var stocksResult = await _stockService.GetStocksByLocationAsync(locationId, ct);
        var stocks = stocksResult.Success ? stocksResult.Data ?? new List<BootManager.Application.Inventory.DTOs.StockDto>()
                                          : new List<BootManager.Application.Inventory.DTOs.StockDto>();

        return StorageOperationResult<StorageLocationDetailDto>.Ok(new StorageLocationDetailDto
        {
            Id = location.Id,
            AreaName = area.Name,
            LocationName = location.Name,
            Description = location.Description,
            QrValue = qrValue,
            TagStatus = location.TagStatus,
            Stocks = stocks
        });
    }

    // --- QR Token operations ---

    public async Task<StorageOperationResult<string>> GenerateOrGetQrTokenAsync(Guid locationId, CancellationToken ct = default)
    {
        var location = await _locationRepo.GetByIdAsync(locationId, ct);
        if (location == null)
            return StorageOperationResult<string>.Error("Locatie niet gevonden.");

        if (location.QrToken != null)
            return StorageOperationResult<string>.Ok(LocationQrValue.FormatQrValue(location.QrToken));

        var newToken = LocationQrValue.GenerateToken();
        location.SetQrToken(newToken);
        await _locationRepo.UpdateAsync(location, ct);

        return StorageOperationResult<string>.Ok(LocationQrValue.FormatQrValue(newToken));
    }

    public async Task<QrResolutionResult> ResolveQrValueAsync(string? qrValue, CancellationToken ct = default)
    {
        var token = LocationQrValue.TryParseQrValue(qrValue);
        if (token == null)
            return QrResolutionResult.Invalid();

        var location = await _locationRepo.SingleOrDefaultAsync(l => l.QrToken == token, ct);
        if (location == null)
            return QrResolutionResult.Unknown(token);

        return QrResolutionResult.Linked(location.Id);
    }

    public async Task<StorageOperationResult> LinkQrToExistingLocationAsync(string token, Guid locationId, CancellationToken ct = default)
    {
        if (!LocationQrValue.IsValidToken(token))
            return StorageOperationResult.Error("Ongeldig token-formaat.");

        var location = await _locationRepo.GetByIdAsync(locationId, ct);
        if (location == null)
            return StorageOperationResult.Error("Locatie niet gevonden.");

        if (location.QrToken != null)
            return StorageOperationResult.Error("Deze locatie heeft al een QR-token gekoppeld.");

        var alreadyLinked = await _locationRepo.SingleOrDefaultAsync(l => l.QrToken == token, ct);
        if (alreadyLinked != null)
            return StorageOperationResult.Error("Deze QR-code is al gekoppeld aan een locatie.");

        try
        {
            location.SetQrToken(token);
            await _locationRepo.UpdateAsync(location, ct);
            return StorageOperationResult.Ok();
        }
        catch (Exception ex)
            when (ex.InnerException?.Message?.Contains("UNIQUE constraint failed", StringComparison.Ordinal) ?? false)
        {
            return StorageOperationResult.Error("Deze QR-code is inmiddels al gekoppeld aan een andere locatie.");
        }
    }

    public async Task<StorageOperationResult<StorageLocationDetailDto>> CreateLocationWithQrTokenAsync(
        Guid areaId, string name, string? description, string token, CancellationToken ct = default)
    {
        var area = await _areaRepo.GetByIdAsync(areaId, ct);
        if (area == null)
            return StorageOperationResult<StorageLocationDetailDto>.Error("Gebied niet gevonden.");

        if (!LocationQrValue.IsValidToken(token))
            return StorageOperationResult<StorageLocationDetailDto>.Error("Ongeldig token-formaat.");

        var trimmedName = name?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedName))
            return StorageOperationResult<StorageLocationDetailDto>.Error("Locatienaam mag niet leeg zijn.");

        if (trimmedName.Length > MaxNameLength)
            return StorageOperationResult<StorageLocationDetailDto>.Error($"Locatienaam mag maximaal {MaxNameLength} tekens lang zijn.");

        if (!string.IsNullOrEmpty(description) && description.Trim().Length > MaxDescriptionLength)
            return StorageOperationResult<StorageLocationDetailDto>.Error($"Beschrijving mag maximaal {MaxDescriptionLength} tekens lang zijn.");

        var normalizedName = trimmedName.ToLowerInvariant();
        var duplicate = await _locationRepo.SingleOrDefaultAsync(
            l => l.StorageAreaId == areaId && l.NormalizedName == normalizedName, ct);

        if (duplicate != null)
            return StorageOperationResult<StorageLocationDetailDto>.Error("Locatienaam bestaat al in dit gebied.");

        var alreadyLinked = await _locationRepo.SingleOrDefaultAsync(l => l.QrToken == token, ct);
        if (alreadyLinked != null)
            return StorageOperationResult<StorageLocationDetailDto>.Error("Deze QR-code is al gekoppeld aan een andere locatie.");

        var newLocation = StorageLocation.Create(areaId, trimmedName, description);
        newLocation.SetQrToken(token);

        try
        {
            await _locationRepo.AddAsync(newLocation, ct);
        }
        catch (Exception ex)
            when (ex.InnerException?.Message?.Contains("UNIQUE constraint failed", StringComparison.Ordinal) ?? false)
        {
            return StorageOperationResult<StorageLocationDetailDto>.Error("Deze QR-code is inmiddels al gekoppeld aan een andere locatie.");
        }

        var qrValue = LocationQrValue.FormatQrValue(token);
        return StorageOperationResult<StorageLocationDetailDto>.Ok(new StorageLocationDetailDto
        {
            Id = newLocation.Id,
            AreaName = area.Name,
            LocationName = newLocation.Name,
            Description = newLocation.Description,
            QrValue = qrValue,
            TagStatus = newLocation.TagStatus
        });
    }

    // --- Tag Overview ---

    public async Task<IReadOnlyList<StorageLocationOverviewDto>> GetAllLocationsOverviewAsync(CancellationToken ct = default)
    {
        var locations = await _locationRepo.ListAsync(ct: ct);
        var areas = await _areaRepo.ListAsync(ct: ct);
        var areaDict = areas.ToDictionary(a => a.Id);

        return locations
            .Select(l =>
            {
                var areaName = areaDict.TryGetValue(l.StorageAreaId, out var area) ? area.Name : "Onbekend";
                var qrValue = l.QrToken != null ? LocationQrValue.FormatQrValue(l.QrToken) : null;
                return new StorageLocationOverviewDto
                {
                    Id = l.Id,
                    AreaName = areaName,
                    LocationName = l.Name,
                    QrValue = qrValue,
                    TagStatus = l.TagStatus
                };
            })
            .ToList();
    }

    // --- Token Replacement & Tag Status ---

    public async Task<StorageOperationResult<string>> ReplaceQrTokenAsync(Guid locationId, CancellationToken ct = default)
    {
        var location = await _locationRepo.GetByIdAsync(locationId, ct);
        if (location == null)
            return StorageOperationResult<string>.Error("Locatie niet gevonden.");

        if (location.QrToken == null)
            return StorageOperationResult<string>.Error("Deze locatie heeft nog geen QR-token en kan niet vervangen worden. Genereer eerst een token.");

        var newToken = LocationQrValue.GenerateToken();

        try
        {
            location.ReplaceQrToken(newToken);
            await _locationRepo.UpdateAsync(location, ct);
            return StorageOperationResult<string>.Ok(LocationQrValue.FormatQrValue(newToken));
        }
        catch (Exception ex)
            when (ex.InnerException?.Message?.Contains("UNIQUE constraint failed", StringComparison.Ordinal) ?? false)
        {
            return StorageOperationResult<string>.Error("Dit token is inmiddels al gekoppeld aan een andere locatie.");
        }
    }

    public async Task<StorageOperationResult> UpdateTagStatusAsync(Guid locationId, TagStatus newStatus, CancellationToken ct = default)
    {
        var location = await _locationRepo.GetByIdAsync(locationId, ct);
        if (location == null)
            return StorageOperationResult.Error("Locatie niet gevonden.");

        location.UpdateTagStatus(newStatus);
        await _locationRepo.UpdateAsync(location, ct);
        return StorageOperationResult.Ok();
    }
}
