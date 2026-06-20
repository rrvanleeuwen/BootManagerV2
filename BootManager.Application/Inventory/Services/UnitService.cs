using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;

namespace BootManager.Application.Inventory.Services;

/// <summary>
/// Service voor eenheid beheer.
/// Handelt validatie, trimmen, uniqueness en defaultset af.
/// </summary>
public class UnitService : IUnitService
{
    private const int MaxNameLength = 100;

    private static readonly string[] DefaultUnitNames = new[]
    {
        "stuk",
        "liter",
        "kilogram",
        "meter",
        "set"
    };

    private readonly IRepository<Unit> _unitRepo;
    private readonly IRepository<Product> _productRepo;

    public UnitService(
        IRepository<Unit> unitRepo,
        IRepository<Product> productRepo)
    {
        _unitRepo = unitRepo;
        _productRepo = productRepo;
    }

    public async Task<IReadOnlyList<UnitDto>> GetActiveAsync(CancellationToken ct = default)
    {
        var units = await _unitRepo.ListAsync(u => u.ArchivedAt == null, ct);
        return units.Select(u => MapToDto(u)).ToList();
    }

    public async Task<IReadOnlyList<UnitDto>> GetAllAsync(CancellationToken ct = default)
    {
        var units = await _unitRepo.ListAsync(ct: ct);
        return units.Select(u => MapToDto(u)).ToList();
    }

    public async Task<InventoryOperationResult<UnitDto>> GetByIdAsync(Guid unitId, CancellationToken ct = default)
    {
        var unit = await _unitRepo.GetByIdAsync(unitId, ct);
        if (unit == null)
            return InventoryOperationResult<UnitDto>.NotFound();

        return InventoryOperationResult<UnitDto>.Ok(MapToDto(unit));
    }

    public async Task<InventoryOperationResult<UnitDto>> CreateAsync(string name, CancellationToken ct = default)
    {
        var trimmedName = name?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedName))
            return InventoryOperationResult<UnitDto>.Error("Eenheidsnaam mag niet leeg zijn.");

        if (trimmedName.Length > MaxNameLength)
            return InventoryOperationResult<UnitDto>.Error($"Eenheidsnaam mag maximaal {MaxNameLength} tekens lang zijn.");

        var normalizedName = trimmedName.ToLowerInvariant();
        var existing = await _unitRepo.SingleOrDefaultAsync(
            u => u.NormalizedName == normalizedName, ct);

        if (existing != null)
            return InventoryOperationResult<UnitDto>.Error("Eenheidsnaam bestaat al.");

        var newUnit = Unit.Create(trimmedName);
        await _unitRepo.AddAsync(newUnit, ct);

        return InventoryOperationResult<UnitDto>.Ok(MapToDto(newUnit));
    }

    public async Task<InventoryOperationResult<UnitDto>> UpdateAsync(Guid unitId, string newName, CancellationToken ct = default)
    {
        var unit = await _unitRepo.GetByIdAsync(unitId, ct);
        if (unit == null)
            return InventoryOperationResult<UnitDto>.NotFound();

        var trimmedName = newName?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedName))
            return InventoryOperationResult<UnitDto>.Error("Eenheidsnaam mag niet leeg zijn.");

        if (trimmedName.Length > MaxNameLength)
            return InventoryOperationResult<UnitDto>.Error($"Eenheidsnaam mag maximaal {MaxNameLength} tekens lang zijn.");

        var normalizedName = trimmedName.ToLowerInvariant();
        if (unit.NormalizedName != normalizedName)
        {
            var duplicate = await _unitRepo.SingleOrDefaultAsync(
                u => u.NormalizedName == normalizedName && u.Id != unitId, ct);
            if (duplicate != null)
                return InventoryOperationResult<UnitDto>.Error("Eenheidsnaam bestaat al.");
        }

        unit.UpdateName(trimmedName);
        await _unitRepo.UpdateAsync(unit, ct);

        return InventoryOperationResult<UnitDto>.Ok(MapToDto(unit));
    }

    public async Task<InventoryOperationResult> ArchiveAsync(Guid unitId, CancellationToken ct = default)
    {
        var unit = await _unitRepo.GetByIdAsync(unitId, ct);
        if (unit == null)
            return InventoryOperationResult.Error("Eenheid niet gevonden.");

        var activeProductCount = await _productRepo.CountAsync(
            p => p.DefaultUnitId == unitId && p.ArchivedAt == null, ct);

        if (activeProductCount > 0)
            return InventoryOperationResult.Error("Eenheid heeft actieve producten en kan niet worden gearchiveerd.");

        unit.Archive();
        await _unitRepo.UpdateAsync(unit, ct);

        return InventoryOperationResult.Ok();
    }

    public async Task<InventoryOperationResult> ReactivateAsync(Guid unitId, CancellationToken ct = default)
    {
        var unit = await _unitRepo.GetByIdAsync(unitId, ct);
        if (unit == null)
            return InventoryOperationResult.Error("Eenheid niet gevonden.");

        if (!unit.IsArchived)
            return InventoryOperationResult.Error("Eenheid is niet gearchiveerd.");

        unit.Reactivate();
        await _unitRepo.UpdateAsync(unit, ct);

        return InventoryOperationResult.Ok();
    }

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedName))
            return false;

        var normalizedName = trimmedName.ToLowerInvariant();
        var exists = excludeId.HasValue
            ? await _unitRepo.AnyAsync(u => u.NormalizedName == normalizedName && u.Id != excludeId, ct)
            : await _unitRepo.AnyAsync(u => u.NormalizedName == normalizedName, ct);

        return !exists;
    }

    public async Task InitializeDefaultUnitsAsync(CancellationToken ct = default)
    {
        var existingCount = await _unitRepo.CountAsync(ct: ct);
        if (existingCount > 0)
            return;

        foreach (var name in DefaultUnitNames)
        {
            var unit = Unit.Create(name);
            await _unitRepo.AddAsync(unit, ct);
        }
    }

    private static UnitDto MapToDto(Unit unit)
        => new()
        {
            Id = unit.Id,
            Name = unit.Name,
            IsArchived = unit.IsArchived
        };
}
