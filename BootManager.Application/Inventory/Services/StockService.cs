using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;

namespace BootManager.Application.Inventory.Services;

/// <summary>
/// Service voor voorraadbeheer.
/// Handelt validatie, aanvulling en uniqueness van product-locatie-combinaties af.
/// </summary>
public class StockService : IStockService
{
    private readonly IRepository<Stock> _stockRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<StorageLocation> _locationRepo;
    private readonly IRepository<Unit> _unitRepo;
    private readonly IRepository<StorageArea> _areaRepo;
    private readonly IRepository<ProductCode> _codeRepo;

    public StockService(
        IRepository<Stock> stockRepo,
        IRepository<Product> productRepo,
        IRepository<StorageLocation> locationRepo,
        IRepository<Unit> unitRepo,
        IRepository<StorageArea> areaRepo,
        IRepository<ProductCode> codeRepo)
    {
        _stockRepo = stockRepo;
        _productRepo = productRepo;
        _locationRepo = locationRepo;
        _unitRepo = unitRepo;
        _areaRepo = areaRepo;
        _codeRepo = codeRepo;
    }

    public async Task<InventoryOperationResult<StockDto>> AddOrIncrementStockAsync(
        Guid productId, Guid locationId, decimal quantity, CancellationToken ct = default)
    {
        // Valideer hoeveelheid
        if (quantity <= 0)
            return InventoryOperationResult<StockDto>.Error("Hoeveelheid moet groter dan 0 zijn.");

        // Controleer product
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product == null)
            return InventoryOperationResult<StockDto>.Error("Product niet gevonden.");

        // Controleer locatie
        var location = await _locationRepo.GetByIdAsync(locationId, ct);
        if (location == null)
            return InventoryOperationResult<StockDto>.Error("Opslaglocatie niet gevonden.");

        // Laad gerelateerde entiteiten
        var unit = await _unitRepo.GetByIdAsync(product.DefaultUnitId, ct);
        if (unit == null)
            return InventoryOperationResult<StockDto>.Error("Standaardeenheid niet gevonden.");

        var area = await _areaRepo.GetByIdAsync(location.StorageAreaId, ct);
        if (area == null)
            return InventoryOperationResult<StockDto>.Error("Opslaggebied niet gevonden.");

        // Zoek bestaande voorraadregel
        var existingStock = await _stockRepo.SingleOrDefaultAsync(
            s => s.ProductId == productId && s.StorageLocationId == locationId, ct);

        if (existingStock != null)
        {
            // Aanvullen van bestaande regel
            existingStock.AddQuantity(quantity);
            await _stockRepo.UpdateAsync(existingStock, ct);
            return InventoryOperationResult<StockDto>.Ok(MapToDto(existingStock, product, location, area, unit));
        }

        // Nieuwe voorraadregel
        var newStock = Stock.Create(productId, locationId, quantity);
        await _stockRepo.AddAsync(newStock, ct);

        return InventoryOperationResult<StockDto>.Ok(MapToDto(newStock, product, location, area, unit));
    }

    public async Task<InventoryOperationResult<IReadOnlyList<StockDto>>> GetStocksByLocationAsync(
        Guid locationId, CancellationToken ct = default)
    {
        var location = await _locationRepo.GetByIdAsync(locationId, ct);
        if (location == null)
            return InventoryOperationResult<IReadOnlyList<StockDto>>.Error("Opslaglocatie niet gevonden.");

        var area = await _areaRepo.GetByIdAsync(location.StorageAreaId, ct);
        if (area == null)
            return InventoryOperationResult<IReadOnlyList<StockDto>>.Error("Opslaggebied niet gevonden.");

        var stocks = await _stockRepo.ListAsync(
            s => s.StorageLocationId == locationId, ct);

        if (stocks.Count == 0)
            return InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto>().AsReadOnly());

        var dtos = new List<StockDto>();
        foreach (var stock in stocks)
        {
            var product = await _productRepo.GetByIdAsync(stock.ProductId, ct);
            if (product != null)
            {
                var unit = await _unitRepo.GetByIdAsync(product.DefaultUnitId, ct);
                if (unit != null)
                {
                    dtos.Add(MapToDto(stock, product, location, area, unit));
                }
            }
        }

        return InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(dtos.AsReadOnly());
    }

    public async Task<InventoryOperationResult<IReadOnlyList<StockDto>>> GetStocksByProductAsync(
        Guid productId, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product == null)
            return InventoryOperationResult<IReadOnlyList<StockDto>>.Error("Product niet gevonden.");

        var unit = await _unitRepo.GetByIdAsync(product.DefaultUnitId, ct);
        if (unit == null)
            return InventoryOperationResult<IReadOnlyList<StockDto>>.Error("Standaardeenheid niet gevonden.");

        var stocks = await _stockRepo.ListAsync(
            s => s.ProductId == productId, ct);

        if (stocks.Count == 0)
            return InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto>().AsReadOnly());

        var dtos = new List<StockDto>();
        foreach (var stock in stocks)
        {
            var location = await _locationRepo.GetByIdAsync(stock.StorageLocationId, ct);
            if (location != null)
            {
                var area = await _areaRepo.GetByIdAsync(location.StorageAreaId, ct);
                if (area != null)
                {
                    dtos.Add(MapToDto(stock, product, location, area, unit));
                }
            }
        }

        return InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(dtos.AsReadOnly());
    }

    public async Task<InventoryOperationResult<IReadOnlyList<ProductDto>>> SearchProductsInLocationAsync(
        Guid locationId, string searchTerm, CancellationToken ct = default)
    {
        var location = await _locationRepo.GetByIdAsync(locationId, ct);
        if (location == null)
            return InventoryOperationResult<IReadOnlyList<ProductDto>>.Error("Opslaglocatie niet gevonden.");

        var trimmedSearch = searchTerm?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedSearch))
            return InventoryOperationResult<IReadOnlyList<ProductDto>>.Ok(
                new List<ProductDto>().AsReadOnly());

        var normalizedSearch = trimmedSearch.ToLowerInvariant();

        // Alle actieve producten zoeken
        var allProducts = await _productRepo.ListAsync(p => p.ArchivedAt == null, ct);

        // Alle codes fetchen om ze apart te matchen
        var allCodes = await _codeRepo.ListAsync(ct: ct);
        var codesByProductId = allCodes.GroupBy(c => c.ProductId).ToDictionary(g => g.Key, g => g.FirstOrDefault());

        var matchedProducts = new List<ProductDto>();
        var matchedProductIds = new HashSet<Guid>();

        foreach (var product in allProducts)
        {
            var nameLower = product.Name.ToLowerInvariant();
            if (nameLower.Contains(normalizedSearch))
            {
                if (matchedProductIds.Add(product.Id))
                {
                    matchedProducts.Add(await MapProductToDtoWithCode(product, codesByProductId, ct));
                }
                continue;
            }

            // Controleer gekoppelde codes
            if (codesByProductId.TryGetValue(product.Id, out var code) && code != null)
            {
                var codeLower = code.Value.ToLowerInvariant();
                if (codeLower.Contains(normalizedSearch))
                {
                    if (matchedProductIds.Add(product.Id))
                    {
                        matchedProducts.Add(await MapProductToDtoWithCode(product, codesByProductId, ct));
                    }
                }
            }
        }

        return InventoryOperationResult<IReadOnlyList<ProductDto>>.Ok(matchedProducts.AsReadOnly());
    }

    public async Task<InventoryOperationResult> DeleteStockAsync(Guid stockId, CancellationToken ct = default)
    {
        var stock = await _stockRepo.GetByIdAsync(stockId, ct);
        if (stock == null)
            return InventoryOperationResult.Error("Voorraadregel niet gevonden.");

        await _stockRepo.DeleteAsync(stock, ct);
        return InventoryOperationResult.Ok();
    }

    private StockDto MapToDto(Stock stock, Product product, StorageLocation location, StorageArea area, Unit unit)
        => new()
        {
            Id = stock.Id,
            ProductId = stock.ProductId,
            StorageLocationId = stock.StorageLocationId,
            ProductName = product.Name,
            StorageAreaName = area.Name,
            StorageLocationName = location.Name,
            Quantity = stock.Quantity,
            DefaultUnitName = unit.Name
        };

    private async Task<ProductDto> MapProductToDto(Product product, CancellationToken ct)
    {
        var unit = await _unitRepo.GetByIdAsync(product.DefaultUnitId, ct);
        var activeCategory = product.CategoryMappings.FirstOrDefault(m => m.IsActive);

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            DefaultUnitId = product.DefaultUnitId,
            DefaultUnitName = unit?.Name,
            ActiveCategoryId = activeCategory?.ProductCategoryId,
            ActiveCategoryName = activeCategory?.ProductCategory?.Name,
            ActiveCategoryIconKey = activeCategory?.ProductCategory?.IconKey,
            Code = product.Code != null ? new ProductCodeDto { Id = product.Code.Id, Value = product.Code.Value } : null,
            IsArchived = product.IsArchived
        };
    }

    private async Task<ProductDto> MapProductToDtoWithCode(
        Product product,
        Dictionary<Guid, ProductCode?> codesByProductId,
        CancellationToken ct)
    {
        var unit = await _unitRepo.GetByIdAsync(product.DefaultUnitId, ct);
        var activeCategory = product.CategoryMappings.FirstOrDefault(m => m.IsActive);

        ProductCodeDto? codeDto = null;
        if (codesByProductId.TryGetValue(product.Id, out var code) && code != null)
        {
            codeDto = new ProductCodeDto { Id = code.Id, Value = code.Value };
        }

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            DefaultUnitId = product.DefaultUnitId,
            DefaultUnitName = unit?.Name,
            ActiveCategoryId = activeCategory?.ProductCategoryId,
            ActiveCategoryName = activeCategory?.ProductCategory?.Name,
            ActiveCategoryIconKey = activeCategory?.ProductCategory?.IconKey,
            Code = codeDto,
            IsArchived = product.IsArchived
        };
    }
}
