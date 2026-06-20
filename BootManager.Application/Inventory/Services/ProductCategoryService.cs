using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;

namespace BootManager.Application.Inventory.Services;

/// <summary>
/// Service voor productcategorie beheer.
/// Handelt validatie, trimmen en uniqueness af.
/// </summary>
public class ProductCategoryService : IProductCategoryService
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;

    private static readonly IReadOnlyList<string> ValidIconKeys = new[]
    {
        "drank",
        "onderdeel",
        "gereedschap",
        "voeding",
        "elektronics",
        "veiligheid"
    };

    private readonly IRepository<ProductCategory> _categoryRepo;
    private readonly IRepository<Product> _productRepo;

    public ProductCategoryService(
        IRepository<ProductCategory> categoryRepo,
        IRepository<Product> productRepo)
    {
        _categoryRepo = categoryRepo;
        _productRepo = productRepo;
    }

    public async Task<IReadOnlyList<ProductCategoryDto>> GetActiveAsync(CancellationToken ct = default)
    {
        var categories = await _categoryRepo.ListAsync(c => c.ArchivedAt == null, ct);
        return categories.Select(c => MapToDto(c)).ToList();
    }

    public async Task<IReadOnlyList<ProductCategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var categories = await _categoryRepo.ListAsync(ct: ct);
        return categories.Select(c => MapToDto(c)).ToList();
    }

    public async Task<InventoryOperationResult<ProductCategoryDto>> GetByIdAsync(Guid categoryId, CancellationToken ct = default)
    {
        var category = await _categoryRepo.GetByIdAsync(categoryId, ct);
        if (category == null)
            return InventoryOperationResult<ProductCategoryDto>.NotFound();

        return InventoryOperationResult<ProductCategoryDto>.Ok(MapToDto(category));
    }

    public async Task<InventoryOperationResult<ProductCategoryDto>> CreateAsync(
        string name, string? description, string iconKey, CancellationToken ct = default)
    {
        var trimmedName = name?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedName))
            return InventoryOperationResult<ProductCategoryDto>.Error("Categorienaam mag niet leeg zijn.");

        if (trimmedName.Length > MaxNameLength)
            return InventoryOperationResult<ProductCategoryDto>.Error($"Categorienaam mag maximaal {MaxNameLength} tekens lang zijn.");

        if (!string.IsNullOrEmpty(description) && description.Trim().Length > MaxDescriptionLength)
            return InventoryOperationResult<ProductCategoryDto>.Error($"Omschrijving mag maximaal {MaxDescriptionLength} tekens lang zijn.");

        if (!IsValidIconKey(iconKey))
            return InventoryOperationResult<ProductCategoryDto>.Error("Ongeldig icoonsleutel.");

        var normalizedName = trimmedName.ToLowerInvariant();
        var existing = await _categoryRepo.SingleOrDefaultAsync(
            c => c.NormalizedName == normalizedName, ct);

        if (existing != null)
            return InventoryOperationResult<ProductCategoryDto>.Error("Categorienaam bestaat al.");

        var newCategory = ProductCategory.Create(trimmedName, description, iconKey);
        await _categoryRepo.AddAsync(newCategory, ct);

        return InventoryOperationResult<ProductCategoryDto>.Ok(MapToDto(newCategory));
    }

    public async Task<InventoryOperationResult<ProductCategoryDto>> UpdateAsync(
        Guid categoryId, string newName, string? newDescription, string newIconKey, CancellationToken ct = default)
    {
        var category = await _categoryRepo.GetByIdAsync(categoryId, ct);
        if (category == null)
            return InventoryOperationResult<ProductCategoryDto>.NotFound();

        var trimmedName = newName?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedName))
            return InventoryOperationResult<ProductCategoryDto>.Error("Categorienaam mag niet leeg zijn.");

        if (trimmedName.Length > MaxNameLength)
            return InventoryOperationResult<ProductCategoryDto>.Error($"Categorienaam mag maximaal {MaxNameLength} tekens lang zijn.");

        if (!string.IsNullOrEmpty(newDescription) && newDescription.Trim().Length > MaxDescriptionLength)
            return InventoryOperationResult<ProductCategoryDto>.Error($"Omschrijving mag maximaal {MaxDescriptionLength} tekens lang zijn.");

        if (!IsValidIconKey(newIconKey))
            return InventoryOperationResult<ProductCategoryDto>.Error("Ongeldig icoonsleutel.");

        var normalizedName = trimmedName.ToLowerInvariant();
        if (category.NormalizedName != normalizedName)
        {
            var duplicate = await _categoryRepo.SingleOrDefaultAsync(
                c => c.NormalizedName == normalizedName && c.Id != categoryId, ct);
            if (duplicate != null)
                return InventoryOperationResult<ProductCategoryDto>.Error("Categorienaam bestaat al.");
        }

        category.UpdateNameAndDescription(trimmedName, newDescription);
        category.UpdateIconKey(newIconKey);
        await _categoryRepo.UpdateAsync(category, ct);

        return InventoryOperationResult<ProductCategoryDto>.Ok(MapToDto(category));
    }

    public async Task<InventoryOperationResult> ArchiveAsync(Guid categoryId, CancellationToken ct = default)
    {
        var category = await _categoryRepo.GetByIdAsync(categoryId, ct);
        if (category == null)
            return InventoryOperationResult.Error("Categorie niet gevonden.");

        var activeProductCount = await _productRepo.CountAsync(
            p => p.CategoryMappings.Any(m => m.ProductCategoryId == categoryId && m.IsActive) && p.ArchivedAt == null, ct);

        if (activeProductCount > 0)
            return InventoryOperationResult.Error("Categorie heeft actieve producten en kan niet worden gearchiveerd.");

        category.Archive();
        await _categoryRepo.UpdateAsync(category, ct);

        return InventoryOperationResult.Ok();
    }

    public async Task<InventoryOperationResult> ReactivateAsync(Guid categoryId, CancellationToken ct = default)
    {
        var category = await _categoryRepo.GetByIdAsync(categoryId, ct);
        if (category == null)
            return InventoryOperationResult.Error("Categorie niet gevonden.");

        if (!category.IsArchived)
            return InventoryOperationResult.Error("Categorie is niet gearchiveerd.");

        category.Reactivate();
        await _categoryRepo.UpdateAsync(category, ct);

        return InventoryOperationResult.Ok();
    }

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedName))
            return false;

        var normalizedName = trimmedName.ToLowerInvariant();
        var exists = excludeId.HasValue
            ? await _categoryRepo.AnyAsync(c => c.NormalizedName == normalizedName && c.Id != excludeId, ct)
            : await _categoryRepo.AnyAsync(c => c.NormalizedName == normalizedName, ct);

        return !exists;
    }

    public bool IsValidIconKey(string iconKey)
        => ValidIconKeys.Contains(iconKey);

    public IReadOnlyList<string> GetValidIconKeys()
        => ValidIconKeys;

    private static ProductCategoryDto MapToDto(ProductCategory category)
        => new()
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IconKey = category.IconKey,
            IsArchived = category.IsArchived
        };
}
