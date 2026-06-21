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
    private readonly IRepository<StockMutation> _mutationRepo;
    private readonly IRepository<LocalUser> _userRepo;
    private readonly IRepository<StockExpectedLocation> _expectedLocationRepo;

    public StockService(
        IRepository<Stock> stockRepo,
        IRepository<Product> productRepo,
        IRepository<StorageLocation> locationRepo,
        IRepository<Unit> unitRepo,
        IRepository<StorageArea> areaRepo,
        IRepository<ProductCode> codeRepo,
        IRepository<StockMutation> mutationRepo,
        IRepository<LocalUser> userRepo,
        IRepository<StockExpectedLocation> expectedLocationRepo)
    {
        _stockRepo = stockRepo;
        _productRepo = productRepo;
        _locationRepo = locationRepo;
        _unitRepo = unitRepo;
        _areaRepo = areaRepo;
        _codeRepo = codeRepo;
        _mutationRepo = mutationRepo;
        _userRepo = userRepo;
        _expectedLocationRepo = expectedLocationRepo;
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

            // Zorg dat verwachte locatie bijgewerkt is
            await UpdateExpectedLocationAsync(productId, locationId, ct);

            return InventoryOperationResult<StockDto>.Ok(MapToDto(existingStock, product, location, area, unit));
        }

        // Nieuwe voorraadregel
        var newStock = Stock.Create(productId, locationId, quantity);
        await _stockRepo.AddAsync(newStock, ct);

        // Zorg dat verwachte locatie ingesteld is
        await UpdateExpectedLocationAsync(productId, locationId, ct);

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

    public async Task<InventoryOperationResult<StockDto>> GetMostRecentStockForProductAsync(
        Guid productId, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product == null)
            return InventoryOperationResult<StockDto>.Error("Product niet gevonden.");

        var unit = await _unitRepo.GetByIdAsync(product.DefaultUnitId, ct);
        if (unit == null)
            return InventoryOperationResult<StockDto>.Error("Standaardeenheid niet gevonden.");

        var stocks = await _stockRepo.ListAsync(
            s => s.ProductId == productId, ct);

        if (stocks.Count == 0)
            return InventoryOperationResult<StockDto>.NotFound();

        var mostRecent = stocks.OrderByDescending(s => s.UpdatedAt).FirstOrDefault();
        if (mostRecent == null)
            return InventoryOperationResult<StockDto>.NotFound();

        var location = await _locationRepo.GetByIdAsync(mostRecent.StorageLocationId, ct);
        if (location == null)
            return InventoryOperationResult<StockDto>.Error("Opslaglocatie niet gevonden.");

        var area = await _areaRepo.GetByIdAsync(location.StorageAreaId, ct);
        if (area == null)
            return InventoryOperationResult<StockDto>.Error("Opslaggebied niet gevonden.");

        return InventoryOperationResult<StockDto>.Ok(
            MapToDto(mostRecent, product, location, area, unit));
    }

    public async Task<InventoryOperationResult<IReadOnlyList<StockDto>>> GetAlternativeLocationsForProductAsync(
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

        if (stocks.Count <= 1)
            return InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto>().AsReadOnly());

        var mostRecent = stocks.OrderByDescending(s => s.UpdatedAt).First();
        var alternatives = stocks.Where(s => s.Id != mostRecent.Id)
            .OrderByDescending(s => s.UpdatedAt)
            .ToList();

        var dtos = new List<StockDto>();
        foreach (var stock in alternatives)
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

    public async Task<InventoryOperationResult<IReadOnlyList<StockDto>>> GetActiveStocksByProductAsync(
        Guid productId, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product == null)
            return InventoryOperationResult<IReadOnlyList<StockDto>>.Error("Product niet gevonden.");

        var unit = await _unitRepo.GetByIdAsync(product.DefaultUnitId, ct);
        if (unit == null)
            return InventoryOperationResult<IReadOnlyList<StockDto>>.Error("Standaardeenheid niet gevonden.");

        var stocks = await _stockRepo.ListAsync(
            s => s.ProductId == productId && s.Quantity > 0, ct);

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

    public async Task<InventoryOperationResult<StockDto>> GetExpectedLocationForProductAsync(
        Guid productId, CancellationToken ct = default)
    {
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product == null)
            return InventoryOperationResult<StockDto>.Error("Product niet gevonden.");

        var unit = await _unitRepo.GetByIdAsync(product.DefaultUnitId, ct);
        if (unit == null)
            return InventoryOperationResult<StockDto>.Error("Standaardeenheid niet gevonden.");

        var expectedLocation = await _expectedLocationRepo.SingleOrDefaultAsync(
            el => el.ProductId == productId, ct);

        if (expectedLocation == null)
            return InventoryOperationResult<StockDto>.NotFound();

        var location = await _locationRepo.GetByIdAsync(expectedLocation.StorageLocationId, ct);
        if (location == null)
            return InventoryOperationResult<StockDto>.Error("Opslaglocatie niet gevonden.");

        var area = await _areaRepo.GetByIdAsync(location.StorageAreaId, ct);
        if (area == null)
            return InventoryOperationResult<StockDto>.Error("Opslaggebied niet gevonden.");

        return InventoryOperationResult<StockDto>.Ok(
            new StockDto
            {
                Id = Guid.Empty,
                ProductId = product.Id,
                StorageLocationId = expectedLocation.StorageLocationId,
                ProductName = product.Name,
                StorageAreaName = area.Name,
                StorageLocationName = location.Name,
                Quantity = 0,
                DefaultUnitName = unit.Name
            });
    }

    public async Task<InventoryOperationResult> MutateStockAsync(
        Guid productId, Guid locationId, StockMutationType mutationType,
        decimal quantityOrAmount, Guid userId, string? note = null, CancellationToken ct = default)
    {
        // Valideer hoeveelheid
        if (quantityOrAmount <= 0)
            return InventoryOperationResult.Error("Hoeveelheid moet groter dan 0 zijn.");

        // Controleer product
        var product = await _productRepo.GetByIdAsync(productId, ct);
        if (product == null)
            return InventoryOperationResult.Error("Product niet gevonden.");

        // Controleer locatie
        var location = await _locationRepo.GetByIdAsync(locationId, ct);
        if (location == null)
            return InventoryOperationResult.Error("Opslaglocatie niet gevonden.");

        // Controleer gebruiker
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user == null)
            return InventoryOperationResult.Error("Gebruiker niet gevonden.");

        // Zoek bestaande voorraadregel
        var stock = await _stockRepo.SingleOrDefaultAsync(
            s => s.ProductId == productId && s.StorageLocationId == locationId, ct);

        decimal oldQuantity = stock?.Quantity ?? 0;
        decimal newQuantity;

        if (mutationType == StockMutationType.Verbruik)
        {
            // Verbruik: trek hoeveelheid af
            if (stock == null || stock.Quantity < quantityOrAmount)
                return InventoryOperationResult.Error("Onvoldoende voorraad om deze hoeveelheid te verbruiken.");

            newQuantity = stock.Quantity - quantityOrAmount;
        }
        else if (mutationType == StockMutationType.Telling || mutationType == StockMutationType.Correctie)
        {
            // Telling/Correctie: stel nieuw aantal in
            newQuantity = quantityOrAmount;
        }
        else
        {
            return InventoryOperationResult.Error("Onbekend mutatielogtype.");
        }

        // Log mutatie
        var mutation = StockMutation.Create(productId, locationId, mutationType, oldQuantity, newQuantity, userId, note);
        await _mutationRepo.AddAsync(mutation, ct);

        // Update of verwijder voorraadregel
        if (newQuantity == 0)
        {
            // Verwijder voorraadregel, behoud verwachte locatie
            if (stock != null)
            {
                await _stockRepo.DeleteAsync(stock, ct);
                // Zorg dat verwachte locatie blijft bestaan
                await UpdateExpectedLocationAsync(productId, locationId, ct);
            }
        }
        else if (stock != null)
        {
            // Update bestaande regel
            stock.SetQuantity(newQuantity);
            await _stockRepo.UpdateAsync(stock, ct);
            // Zorg dat verwachte locatie bijgewerkt is
            await UpdateExpectedLocationAsync(productId, locationId, ct);
        }
        else
        {
            // Maak nieuwe regel aan (zelden, maar mogelijk bij Telling/Correctie op niet-bestaande regel)
            var newStock = Stock.Create(productId, locationId, newQuantity);
            await _stockRepo.AddAsync(newStock, ct);
            // Zorg dat verwachte locatie ingesteld is
            await UpdateExpectedLocationAsync(productId, locationId, ct);
        }

        return InventoryOperationResult.Ok();
    }

    public async Task<InventoryOperationResult<IReadOnlyList<StockMutationDto>>> GetStockMutationsAsync(
        CancellationToken ct = default)
    {
        var mutations = await _mutationRepo.ListAsync(ct: ct);
        var sortedMutations = mutations.OrderByDescending(m => m.MutatedAt).ToList();

        var dtos = new List<StockMutationDto>();
        foreach (var mutation in sortedMutations)
        {
            var product = await _productRepo.GetByIdAsync(mutation.ProductId, ct);
            if (product == null) continue;

            var location = await _locationRepo.GetByIdAsync(mutation.StorageLocationId, ct);
            if (location == null) continue;

            var area = await _areaRepo.GetByIdAsync(location.StorageAreaId, ct);
            if (area == null) continue;

            var unit = await _unitRepo.GetByIdAsync(product.DefaultUnitId, ct);
            if (unit == null) continue;

            var user = await _userRepo.GetByIdAsync(mutation.UserId, ct);
            if (user == null) continue;

            dtos.Add(new StockMutationDto
            {
                Id = mutation.Id,
                ProductId = mutation.ProductId,
                StorageLocationId = mutation.StorageLocationId,
                MutationType = mutation.MutationType,
                OldQuantity = mutation.OldQuantity,
                NewQuantity = mutation.NewQuantity,
                MutatedAt = mutation.MutatedAt,
                UserId = mutation.UserId,
                Note = mutation.Note,
                ProductName = product.Name,
                StorageAreaName = area.Name,
                StorageLocationName = location.Name,
                DefaultUnitName = unit.Name,
                UserDisplayName = user.DisplayName
            });
        }

        return InventoryOperationResult<IReadOnlyList<StockMutationDto>>.Ok(dtos.AsReadOnly());
    }

    private async Task UpdateExpectedLocationAsync(Guid productId, Guid locationId, CancellationToken ct)
    {
        var existing = await _expectedLocationRepo.SingleOrDefaultAsync(
            el => el.ProductId == productId, ct);

        if (existing != null)
        {
            existing.UpdateLocation(locationId);
            await _expectedLocationRepo.UpdateAsync(existing, ct);
        }
        else
        {
            var newExpectedLocation = StockExpectedLocation.Create(productId, locationId);
            await _expectedLocationRepo.AddAsync(newExpectedLocation, ct);
        }
    }
}
