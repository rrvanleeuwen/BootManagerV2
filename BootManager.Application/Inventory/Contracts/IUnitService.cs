using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;

namespace BootManager.Application.Inventory.Contracts;

/// <summary>
/// Contract voor eenheid beheer.
/// </summary>
public interface IUnitService
{
    /// <summary>Haalt alle actieve eenheden op.</summary>
    Task<IReadOnlyList<UnitDto>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>Haalt alle eenheden (inclusief gearchiveerde) op.</summary>
    Task<IReadOnlyList<UnitDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Haalt een eenheid op bij ID.</summary>
    Task<InventoryOperationResult<UnitDto>> GetByIdAsync(Guid unitId, CancellationToken ct = default);

    /// <summary>Maakt een nieuwe eenheid aan.</summary>
    Task<InventoryOperationResult<UnitDto>> CreateAsync(string name, CancellationToken ct = default);

    /// <summary>Werkt een eenheid bij.</summary>
    Task<InventoryOperationResult<UnitDto>> UpdateAsync(Guid unitId, string newName, CancellationToken ct = default);

    /// <summary>Archiveert een eenheid; weigert als actieve producten ervan afhangen.</summary>
    Task<InventoryOperationResult> ArchiveAsync(Guid unitId, CancellationToken ct = default);

    /// <summary>Reactiveert een gearchiveerde eenheid.</summary>
    Task<InventoryOperationResult> ReactivateAsync(Guid unitId, CancellationToken ct = default);

    /// <summary>Controleert of een naam uniek is (case-insensitive).</summary>
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>Initialiseert de defaultset van eenheden indien lege database.</summary>
    Task InitializeDefaultUnitsAsync(CancellationToken ct = default);
}
