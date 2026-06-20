using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;

namespace BootManager.Application.Inventory.Services;

/// <summary>
/// Service voor product beheer.
/// Handelt validatie, trimmen, uniqueness, categorie-koppeling en codes af.
/// </summary>
public class ProductService : IProductService
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;
    private const int MaxCodeLength = 255;

    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<ProductCode> _codeRepo;
    private readonly IRepository<ProductCategoryMapping> _mappingRepo;
    private readonly IRepository<ProductCategory> _categoryRepo;
    private readonly IRepository<Unit> _unitRepo;

    public ProductService(
        IRepository<Product> productRepo,
        IRepository<ProductCode> codeRepo,
        IRepository<ProductCategoryMapping> mappingRepo,
        IRepository<ProductCategory> categoryRepo,
        IRepository<Unit> unitRepo)
    {
        _productRepo = productRepo;
        _codeRepo = codeRepo;
        _mappingRepo = mappingRepo;
        _categoryRepo = categoryRepo;
        _unitRepo = unitRepo;
    }

    public async Task<IReadOnlyList<ProductDto>> GetActiveAsync(CancellationToken ct = default)
    {
        var products = await _productRepo.ListAsync(p => p.ArchivedAt == null, ct);
        return await MapToDtoListAsync(products, ct);
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct = default)
    {
        var products = await _productRepo.ListAsync(ct: ct);
        return await MapToDtoListAsync(products, ct);
    }

    public async Task<InventoryOperationResult<ProductDto>> GetByIdAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product == null)
            return InventoryOperationResult<ProductDto>.NotFound();

        var dto = await MapToDtoAsync(product, ct);
        return InventoryOperationResult<ProductDto>.Ok(dto);
    }

    public async Task<InventoryOperationResult<ProductDto>> CreateAsync(
        string name, string? description, Guid defaultUnitId, Guid? categoryId = null, CancellationToken ct = default)
    {
        var validationResult = await ValidateProductInput(name, description, defaultUnitId, ct);
        if (!validationResult.Success)
            return InventoryOperationResult<ProductDto>.Error(validationResult.ErrorMessage!);

        if (categoryId.HasValue)
        {
            var categoryCheckResult = await ValidateCategoryExists(categoryId.Value, ct);
            if (!categoryCheckResult.Success)
                return InventoryOperationResult<ProductDto>.Error(categoryCheckResult.ErrorMessage!);
        }

        var newProduct = Product.Create(name.Trim(), description, defaultUnitId);
        await _productRepo.AddAsync(newProduct, ct);

        if (categoryId.HasValue)
        {
            var mapping = ProductCategoryMapping.Create(newProduct.Id, categoryId.Value);
            await _mappingRepo.AddAsync(mapping, ct);
        }

        var dto = await MapToDtoAsync(newProduct, ct);
        return InventoryOperationResult<ProductDto>.Ok(dto);
    }

    public async Task<InventoryOperationResult<ProductDto>> UpdateAsync(
        Guid productId, string newName, string? newDescription, Guid newDefaultUnitId, Guid? newCategoryId = null, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product == null)
            return InventoryOperationResult<ProductDto>.NotFound();

        var validationResult = await ValidateProductInput(newName, newDescription, newDefaultUnitId, ct);
        if (!validationResult.Success)
            return InventoryOperationResult<ProductDto>.Error(validationResult.ErrorMessage!);

        if (newCategoryId.HasValue)
        {
            var categoryCheckResult = await ValidateCategoryExists(newCategoryId.Value, ct);
            if (!categoryCheckResult.Success)
                return InventoryOperationResult<ProductDto>.Error(categoryCheckResult.ErrorMessage!);
        }

        product.UpdateNameAndDescription(newName, newDescription);
        product.SetDefaultUnit(newDefaultUnitId);

        // Handle category mapping
        if (newCategoryId.HasValue)
        {
            var existingMapping = await _mappingRepo.SingleOrDefaultAsync(
                m => m.ProductId == productId && m.IsActive, ct);

            if (existingMapping == null)
            {
                var newMapping = ProductCategoryMapping.Create(productId, newCategoryId.Value);
                await _mappingRepo.AddAsync(newMapping, ct);
            }
            else if (existingMapping.ProductCategoryId != newCategoryId.Value)
            {
                existingMapping.Deactivate();
                await _mappingRepo.UpdateAsync(existingMapping, ct);

                var newMapping = ProductCategoryMapping.Create(productId, newCategoryId.Value);
                await _mappingRepo.AddAsync(newMapping, ct);
            }
        }
        else
        {
            var existingMapping = await _mappingRepo.SingleOrDefaultAsync(
                m => m.ProductId == productId && m.IsActive, ct);

            if (existingMapping != null)
            {
                existingMapping.Deactivate();
                await _mappingRepo.UpdateAsync(existingMapping, ct);
            }
        }

        await _productRepo.UpdateAsync(product, ct);

        var dto = await MapToDtoAsync(product, ct);
        return InventoryOperationResult<ProductDto>.Ok(dto);
    }

    public async Task<InventoryOperationResult> SetCategoryAsync(Guid productId, Guid categoryId, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product == null)
            return InventoryOperationResult.Error("Product niet gevonden.");

        var categoryCheckResult = await ValidateCategoryExists(categoryId, ct);
        if (!categoryCheckResult.Success)
            return InventoryOperationResult.Error(categoryCheckResult.ErrorMessage!);

        var existingMapping = await _mappingRepo.SingleOrDefaultAsync(
            m => m.ProductId == productId && m.IsActive, ct);

        if (existingMapping == null)
        {
            var newMapping = ProductCategoryMapping.Create(productId, categoryId);
            await _mappingRepo.AddAsync(newMapping, ct);
        }
        else if (existingMapping.ProductCategoryId != categoryId)
        {
            existingMapping.Deactivate();
            await _mappingRepo.UpdateAsync(existingMapping, ct);

            var newMapping = ProductCategoryMapping.Create(productId, categoryId);
            await _mappingRepo.AddAsync(newMapping, ct);
        }

        return InventoryOperationResult.Ok();
    }

    public async Task<InventoryOperationResult> RemoveCategoryAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product == null)
            return InventoryOperationResult.Error("Product niet gevonden.");

        var existingMapping = await _mappingRepo.SingleOrDefaultAsync(
            m => m.ProductId == productId && m.IsActive, ct);

        if (existingMapping != null)
        {
            existingMapping.Deactivate();
            await _mappingRepo.UpdateAsync(existingMapping, ct);
        }

        return InventoryOperationResult.Ok();
    }

    public async Task<InventoryOperationResult> ArchiveAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product == null)
            return InventoryOperationResult.Error("Product niet gevonden.");

        product.Archive();
        await _productRepo.UpdateAsync(product, ct);

        return InventoryOperationResult.Ok();
    }

    public async Task<InventoryOperationResult> ReactivateAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product == null)
            return InventoryOperationResult.Error("Product niet gevonden.");

        if (!product.IsArchived)
            return InventoryOperationResult.Error("Product is niet gearchiveerd.");

        product.Reactivate();
        await _productRepo.UpdateAsync(product, ct);

        return InventoryOperationResult.Ok();
    }

    public async Task<InventoryOperationResult<ProductCodeDto>> AddCodeAsync(
        Guid productId, string codeValue, string codeFormat, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product == null)
            return InventoryOperationResult<ProductCodeDto>.Error("Product niet gevonden.");

        var codeValidation = ValidateCodeInput(codeValue);
        if (!codeValidation.Success)
            return InventoryOperationResult<ProductCodeDto>.Error(codeValidation.ErrorMessage!);

        var existingCode = await GetCodeByProductIdAsync(productId, ct);
        if (existingCode != null)
            return InventoryOperationResult<ProductCodeDto>.Error("Product heeft al een gekoppelde code.");

        var isUnique = await IsCodeValueUniqueAsync(codeValue, null, ct);
        if (!isUnique)
            return InventoryOperationResult<ProductCodeDto>.Error("Code-waarde bestaat al in de catalogus.");

        var newCode = ProductCode.Create(productId, codeValue, codeFormat);
        await _codeRepo.AddAsync(newCode, ct);

        var dto = MapCodeToDto(newCode);
        return InventoryOperationResult<ProductCodeDto>.Ok(dto);
    }

    public async Task<InventoryOperationResult<ProductCodeDto>> ReplaceCodeAsync(
        Guid productId, string newCodeValue, string newCodeFormat, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product == null)
            return InventoryOperationResult<ProductCodeDto>.Error("Product niet gevonden.");

        var codeValidation = ValidateCodeInput(newCodeValue);
        if (!codeValidation.Success)
            return InventoryOperationResult<ProductCodeDto>.Error(codeValidation.ErrorMessage!);

        var existingCode = await GetCodeByProductIdAsync(productId, ct);
        if (existingCode == null)
            return InventoryOperationResult<ProductCodeDto>.Error("Product heeft geen gekoppelde code om te vervangen.");

        var isUnique = await IsCodeValueUniqueAsync(newCodeValue, productId, ct);
        if (!isUnique)
            return InventoryOperationResult<ProductCodeDto>.Error("Code-waarde bestaat al in de catalogus.");

        existingCode.UpdateValue(newCodeValue, newCodeFormat);
        await _codeRepo.UpdateAsync(existingCode, ct);

        var dto = MapCodeToDto(existingCode);
        return InventoryOperationResult<ProductCodeDto>.Ok(dto);
    }

    public async Task<InventoryOperationResult> RemoveCodeAsync(Guid productId, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product == null)
            return InventoryOperationResult.Error("Product niet gevonden.");

        var code = await GetCodeByProductIdAsync(productId, ct);
        if (code == null)
            return InventoryOperationResult.Error("Product heeft geen gekoppelde code.");

        await _codeRepo.DeleteAsync(code, ct);

        return InventoryOperationResult.Ok();
    }

    public async Task<bool> IsCodeValueUniqueAsync(string codeValue, Guid? excludeProductId = null, CancellationToken ct = default)
    {
        var trimmedValue = codeValue?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedValue))
            return false;

        var normalizedValue = trimmedValue.ToLowerInvariant();
        var exists = excludeProductId.HasValue
            ? await _codeRepo.AnyAsync(c => c.NormalizedValue == normalizedValue && c.ProductId != excludeProductId, ct)
            : await _codeRepo.AnyAsync(c => c.NormalizedValue == normalizedValue, ct);

        return !exists;
    }

    public async Task<IReadOnlyList<ProductDto>> SearchByNameOrDescriptionAsync(string searchTerm, CancellationToken ct = default)
    {
        var trimmedSearch = searchTerm?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedSearch))
            return new List<ProductDto>().AsReadOnly();

        var normalizedSearch = trimmedSearch.ToLowerInvariant();

        var allActiveProducts = await _productRepo.ListAsync(p => p.ArchivedAt == null, ct);

        var matchedProducts = new List<ProductDto>();
        foreach (var product in allActiveProducts)
        {
            var nameLower = product.Name.ToLowerInvariant();
            var descriptionLower = (product.Description ?? string.Empty).ToLowerInvariant();

            if (nameLower.Contains(normalizedSearch) || descriptionLower.Contains(normalizedSearch))
            {
                var dto = await MapToDtoAsync(product, ct);
                matchedProducts.Add(dto);
            }
        }

        return matchedProducts.AsReadOnly();
    }

    public async Task<InventoryOperationResult<ProductDto>> GetByCodeValueAsync(string codeValue, CancellationToken ct = default)
    {
        var trimmedValue = codeValue?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedValue))
            return InventoryOperationResult<ProductDto>.NotFound();

        var normalizedValue = trimmedValue.ToLowerInvariant();
        var code = await _codeRepo.SingleOrDefaultAsync(c => c.NormalizedValue == normalizedValue, ct);

        if (code == null)
            return InventoryOperationResult<ProductDto>.NotFound();

        var product = await _productRepo.GetByIdAsync(code.ProductId, ct);
        if (product == null)
            return InventoryOperationResult<ProductDto>.NotFound();

        var dto = await MapToDtoAsync(product, ct);
        return InventoryOperationResult<ProductDto>.Ok(dto);
    }

    private async Task<IReadOnlyList<ProductDto>> MapToDtoListAsync(IReadOnlyList<Product> products, CancellationToken ct)
    {
        var dtos = new List<ProductDto>();
        foreach (var product in products)
        {
            dtos.Add(await MapToDtoAsync(product, ct));
        }
        return dtos;
    }

    private async Task<ProductDto> MapToDtoAsync(Product product, CancellationToken ct)
    {
        var unit = await _unitRepo.GetByIdAsync(product.DefaultUnitId, ct);
        var activeMapping = await _mappingRepo.SingleOrDefaultAsync(
            m => m.ProductId == product.Id && m.IsActive, ct);
        var code = await GetCodeByProductIdAsync(product.Id, ct);

        ProductCategoryDto? category = null;
        if (activeMapping != null)
        {
            var cat = await _categoryRepo.GetByIdAsync(activeMapping.ProductCategoryId, ct);
            if (cat != null)
            {
                category = new ProductCategoryDto
                {
                    Id = cat.Id,
                    Name = cat.Name,
                    Description = cat.Description,
                    IconKey = cat.IconKey,
                    IsArchived = cat.IsArchived
                };
            }
        }

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            DefaultUnitId = product.DefaultUnitId,
            DefaultUnitName = unit?.Name,
            ActiveCategoryId = category?.Id,
            ActiveCategoryName = category?.Name,
            ActiveCategoryIconKey = category?.IconKey,
            Code = code != null ? MapCodeToDto(code) : null,
            IsArchived = product.IsArchived
        };
    }

    private Task<ProductCode?> GetCodeByProductIdAsync(Guid productId, CancellationToken ct)
        => _codeRepo.SingleOrDefaultAsync(c => c.ProductId == productId, ct);

    private static ProductCodeDto MapCodeToDto(ProductCode code)
        => new()
        {
            Id = code.Id,
            Value = code.Value,
            Format = code.Format
        };

    private async Task<InventoryOperationResult> ValidateProductInput(
        string name, string? description, Guid defaultUnitId, CancellationToken ct)
    {
        var trimmedName = name?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedName))
            return InventoryOperationResult.Error("Productnaam mag niet leeg zijn.");

        if (trimmedName.Length > MaxNameLength)
            return InventoryOperationResult.Error($"Productnaam mag maximaal {MaxNameLength} tekens lang zijn.");

        if (!string.IsNullOrEmpty(description) && description.Trim().Length > MaxDescriptionLength)
            return InventoryOperationResult.Error($"Omschrijving mag maximaal {MaxDescriptionLength} tekens lang zijn.");

        var unit = await _unitRepo.GetByIdAsync(defaultUnitId, ct);
        if (unit == null)
            return InventoryOperationResult.Error("Standaardeenheid niet gevonden.");

        return InventoryOperationResult.Ok();
    }

    private async Task<InventoryOperationResult> ValidateCategoryExists(Guid categoryId, CancellationToken ct)
    {
        var category = await _categoryRepo.GetByIdAsync(categoryId, ct);
        if (category == null)
            return InventoryOperationResult.Error("Categorie niet gevonden.");

        return InventoryOperationResult.Ok();
    }

    private static InventoryOperationResult ValidateCodeInput(string codeValue)
    {
        var trimmedValue = codeValue?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedValue))
            return InventoryOperationResult.Error("Code-waarde mag niet leeg zijn.");

        if (trimmedValue.Length > MaxCodeLength)
            return InventoryOperationResult.Error($"Code-waarde mag maximaal {MaxCodeLength} tekens lang zijn.");

        return InventoryOperationResult.Ok();
    }
}
