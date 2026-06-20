using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;

namespace BootManager.Application.Inventory.Contracts;

/// <summary>
/// Contract voor productcategorie beheer.
/// </summary>
public interface IProductCategoryService
{
    /// <summary>Haalt alle actieve categorieën op.</summary>
    Task<IReadOnlyList<ProductCategoryDto>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>Haalt alle categorieën (inclusief gearchiveerde) op.</summary>
    Task<IReadOnlyList<ProductCategoryDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Haalt een categorie op bij ID.</summary>
    Task<InventoryOperationResult<ProductCategoryDto>> GetByIdAsync(Guid categoryId, CancellationToken ct = default);

    /// <summary>Maakt een nieuwe categorie aan.</summary>
    Task<InventoryOperationResult<ProductCategoryDto>> CreateAsync(
        string name, string? description, string iconKey, CancellationToken ct = default);

    /// <summary>Werkt een categorie bij.</summary>
    Task<InventoryOperationResult<ProductCategoryDto>> UpdateAsync(
        Guid categoryId, string newName, string? newDescription, string newIconKey, CancellationToken ct = default);

    /// <summary>Archiveert een categorie; weigert als actieve producten ervan afhangen.</summary>
    Task<InventoryOperationResult> ArchiveAsync(Guid categoryId, CancellationToken ct = default);

    /// <summary>Reactiveert een gearchiveerde categorie.</summary>
    Task<InventoryOperationResult> ReactivateAsync(Guid categoryId, CancellationToken ct = default);

    /// <summary>Controleert of een naam uniek is (case-insensitive).</summary>
    Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>Controleert of een icoonsleutel geldig is.</summary>
    bool IsValidIconKey(string iconKey);

    /// <summary>Retourneert de lijst geldige icoonsleutels.</summary>
    IReadOnlyList<string> GetValidIconKeys();
}
