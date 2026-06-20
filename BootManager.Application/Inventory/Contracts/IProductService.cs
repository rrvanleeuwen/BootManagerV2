using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;

namespace BootManager.Application.Inventory.Contracts;

/// <summary>
/// Contract voor product beheer.
/// </summary>
public interface IProductService
{
    /// <summary>Haalt alle actieve producten op.</summary>
    Task<IReadOnlyList<ProductDto>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>Haalt alle producten (inclusief gearchiveerde) op.</summary>
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Haalt een product op bij ID.</summary>
    Task<InventoryOperationResult<ProductDto>> GetByIdAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Maakt een nieuw product aan met optionele categorie.</summary>
    Task<InventoryOperationResult<ProductDto>> CreateAsync(
        string name, string? description, Guid defaultUnitId, Guid? categoryId = null, CancellationToken ct = default);

    /// <summary>Werkt een product bij.</summary>
    Task<InventoryOperationResult<ProductDto>> UpdateAsync(
        Guid productId, string newName, string? newDescription, Guid newDefaultUnitId, Guid? newCategoryId = null, CancellationToken ct = default);

    /// <summary>Stelt de categorie van een product in (max één actieve).</summary>
    Task<InventoryOperationResult> SetCategoryAsync(Guid productId, Guid categoryId, CancellationToken ct = default);

    /// <summary>Verwijdert de categorie van een product.</summary>
    Task<InventoryOperationResult> RemoveCategoryAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Archiveert een product.</summary>
    Task<InventoryOperationResult> ArchiveAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Reactiveert een gearchiveerd product.</summary>
    Task<InventoryOperationResult> ReactivateAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Voegt een gekoppelde code toe aan het product.</summary>
    Task<InventoryOperationResult<ProductCodeDto>> AddCodeAsync(
        Guid productId, string codeValue, string codeFormat, CancellationToken ct = default);

    /// <summary>Vervangt de gekoppelde code van het product.</summary>
    Task<InventoryOperationResult<ProductCodeDto>> ReplaceCodeAsync(
        Guid productId, string newCodeValue, string newCodeFormat, CancellationToken ct = default);

    /// <summary>Verwijdert de gekoppelde code van het product.</summary>
    Task<InventoryOperationResult> RemoveCodeAsync(Guid productId, CancellationToken ct = default);

    /// <summary>Controleert of een code-waarde uniek is catalog-breed (case-insensitive).</summary>
    Task<bool> IsCodeValueUniqueAsync(string codeValue, Guid? excludeProductId = null, CancellationToken ct = default);

    /// <summary>Haalt product op via code-waarde.</summary>
    Task<InventoryOperationResult<ProductDto>> GetByCodeValueAsync(string codeValue, CancellationToken ct = default);

    /// <summary>Zoekt producten op catalogusniveau op naam en omschrijving, hoofdletterongevoelig.</summary>
    Task<IReadOnlyList<ProductDto>> SearchByNameOrDescriptionAsync(string searchTerm, CancellationToken ct = default);
}
