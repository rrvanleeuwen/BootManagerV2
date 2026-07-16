using System.Linq.Expressions;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Application.Inventory.Services;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Results;
using BootManager.Application.Storage.Services;
using BootManager.Application.Inventory.Contracts;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Moq;
using Xunit;

namespace BootManager.UnitTests.Inventory;

/// <summary>
/// Unit-tests voor <see cref="InventoryImportService"/>: CSV-parsing, destructieve wipe met
/// eenheidsbehoud, eenmalige mappingreuse en opbouw van gebieden, locaties, tokens, producten en voorraad.
/// </summary>
public class InventoryImportServiceTests
{
    private readonly Mock<IStorageService> _storage = new();
    private readonly Mock<IProductService> _products = new();
    private readonly Mock<IStockService> _stock = new();
    private readonly Mock<IUnitService> _units = new();

    private readonly Mock<IRepository<StockMutation>> _mutationRepo = new();
    private readonly Mock<IRepository<Stock>> _stockRepo = new();
    private readonly Mock<IRepository<StockExpectedLocation>> _expectedRepo = new();
    private readonly Mock<IRepository<ProductCode>> _codeRepo = new();
    private readonly Mock<IRepository<ProductCategoryMapping>> _mappingRepo = new();
    private readonly Mock<IRepository<Product>> _productRepo = new();
    private readonly Mock<IRepository<StorageLocation>> _locationRepo = new();
    private readonly Mock<IRepository<StorageArea>> _areaRepo = new();

    private InventoryImportService BuildService() => new(
        _storage.Object, _products.Object, _stock.Object, _units.Object,
        _mutationRepo.Object, _stockRepo.Object, _expectedRepo.Object, _codeRepo.Object,
        _mappingRepo.Object, _productRepo.Object, _locationRepo.Object, _areaRepo.Object);

    private void SetupCreationDefaults(IReadOnlyList<UnitDto>? existingUnits = null)
    {
        _storage.Setup(s => s.CreateAreaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string name, CancellationToken _) =>
                StorageOperationResult<StorageAreaDto>.Ok(new StorageAreaDto { Id = Guid.NewGuid(), Name = name }));

        _storage.Setup(s => s.CreateLocationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid areaId, string name, string? _, CancellationToken __) =>
                StorageOperationResult<StorageLocationDto>.Ok(new StorageLocationDto { Id = Guid.NewGuid(), StorageAreaId = areaId, Name = name }));

        _storage.Setup(s => s.GenerateOrGetQrTokenAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(StorageOperationResult<string>.Ok("bootmanager:location:0123456789abcdef0123456789abcdef"));

        _units.Setup(u => u.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUnits ?? new List<UnitDto>());
        _units.Setup(u => u.CreateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string name, CancellationToken _) =>
                InventoryOperationResult<UnitDto>.Ok(new UnitDto { Id = Guid.NewGuid(), Name = name }));

        _products.Setup(p => p.CreateAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string name, string? _, Guid unitId, Guid? __, CancellationToken ___) =>
                InventoryOperationResult<ProductDto>.Ok(new ProductDto { Id = Guid.NewGuid(), Name = name, DefaultUnitId = unitId }));

        _stock.Setup(s => s.AddOrIncrementStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(new StockDto()));

        SetupEmptyRepo(_mutationRepo);
        SetupEmptyRepo(_stockRepo);
        SetupEmptyRepo(_expectedRepo);
        SetupEmptyRepo(_codeRepo);
        SetupEmptyRepo(_mappingRepo);
        SetupEmptyRepo(_productRepo);
        SetupEmptyRepo(_locationRepo);
        SetupEmptyRepo(_areaRepo);
    }

    private static void SetupEmptyRepo<T>(Mock<IRepository<T>> repo) where T : class
    {
        repo.Setup(r => r.ListAsync(It.IsAny<Expression<Func<T, bool>>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<T>());
        repo.Setup(r => r.DeleteAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private static void SetupRepoWith<T>(Mock<IRepository<T>> repo, T entity) where T : class
    {
        repo.Setup(r => r.ListAsync(It.IsAny<Expression<Func<T, bool>>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<T> { entity });
        repo.Setup(r => r.DeleteAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    // ---------- Parsing ----------

    [Fact]
    public void ParseCsv_ParsesRows_AndDecimalCommaQuantity()
    {
        var csv = "Aantal;Eenheid;Product;Locatie\n" +
                  "4;liter;Rivella;Salonbank, rugleuning\n" +
                  "1,5;pak;koffiebonen;Salonbank, rugleuning\n";

        var result = BuildService().ParseCsv(csv);

        Assert.True(result.Success);
        Assert.Equal(2, result.Rows.Count);
        Assert.Equal(4m, result.Rows[0].Quantity);
        Assert.Equal(1.5m, result.Rows[1].Quantity);
        Assert.Equal("koffiebonen", result.Rows[1].ProductName);
        Assert.Single(result.DistinctSourceLocations);
        Assert.Equal("Salonbank, rugleuning", result.DistinctSourceLocations[0]);
    }

    [Fact]
    public void ParseCsv_RepeatedSourceLocation_IsListedOnce()
    {
        var csv = "Aantal;Eenheid;Product;Locatie\n" +
                  "4;liter;Rivella;Salonbank\n" +
                  "10;liter;melk;Salonbank\n" +
                  "1;pak;kaasvlinders;Salon Snackla\n";

        var result = BuildService().ParseCsv(csv);

        Assert.True(result.Success);
        Assert.Equal(3, result.Rows.Count);
        Assert.Equal(2, result.DistinctSourceLocations.Count);
        Assert.Contains("Salonbank", result.DistinctSourceLocations);
        Assert.Contains("Salon Snackla", result.DistinctSourceLocations);
    }

    [Fact]
    public void ParseCsv_InvalidHeader_Fails()
    {
        var csv = "Hoeveelheid;Eenheid;Product;Locatie\n4;liter;Rivella;Salon\n";

        var result = BuildService().ParseCsv(csv);

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("kolomkoppen", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ParseCsv_InvalidQuantity_RecordsErrorAndFails()
    {
        var csv = "Aantal;Eenheid;Product;Locatie\n" +
                  "abc;liter;Rivella;Salon\n";

        var result = BuildService().ParseCsv(csv);

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Contains("ongeldige hoeveelheid", StringComparison.OrdinalIgnoreCase));
    }

    // ---------- Execution ----------

    [Fact]
    public async Task ExecuteImportAsync_MissingMapping_DoesNotDeleteAndReturnsError()
    {
        SetupCreationDefaults();
        var rows = new List<InventoryImportRowDto>
        {
            new() { Quantity = 1m, Unit = "liter", ProductName = "Rivella", SourceLocation = "Salon" }
        };
        var mappings = new List<InventoryLocationMappingDto>(); // geen mapping

        var result = await BuildService().ExecuteImportAsync(rows, mappings);

        Assert.False(result.Success);
        Assert.Contains("Salon", result.ErrorMessage);
        _productRepo.Verify(r => r.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _areaRepo.Verify(r => r.DeleteAsync(It.IsAny<StorageArea>(), It.IsAny<CancellationToken>()), Times.Never);
        _storage.Verify(s => s.CreateAreaAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteImportAsync_WipesInventoryData_InDependencyOrder_AndKeepsExistingUnits()
    {
        var existingUnit = new UnitDto { Id = Guid.NewGuid(), Name = "liter" };
        SetupCreationDefaults(new List<UnitDto> { existingUnit });

        var deleteOrder = new List<string>();
        SetupRepoWithOrder(_mutationRepo, StockMutation.Create(Guid.NewGuid(), Guid.NewGuid(), StockMutationType.Verbruik, 1, 0, Guid.NewGuid()), "mutation", deleteOrder);
        SetupRepoWithOrder(_stockRepo, Stock.Create(Guid.NewGuid(), Guid.NewGuid(), 1), "stock", deleteOrder);
        SetupRepoWithOrder(_expectedRepo, StockExpectedLocation.Create(Guid.NewGuid(), Guid.NewGuid()), "expected", deleteOrder);
        SetupRepoWithOrder(_codeRepo, ProductCode.Create(Guid.NewGuid(), "123", "barcode"), "code", deleteOrder);
        SetupRepoWithOrder(_mappingRepo, ProductCategoryMapping.Create(Guid.NewGuid(), Guid.NewGuid()), "categoryMapping", deleteOrder);
        SetupRepoWithOrder(_productRepo, Product.Create("Oud", null, Guid.NewGuid()), "product", deleteOrder);
        SetupRepoWithOrder(_locationRepo, StorageLocation.Create(Guid.NewGuid(), "Oude locatie"), "location", deleteOrder);
        SetupRepoWithOrder(_areaRepo, StorageArea.Create("Oud gebied"), "area", deleteOrder);

        var rows = new List<InventoryImportRowDto>
        {
            new() { Quantity = 3m, Unit = "liter", ProductName = "Rivella", SourceLocation = "Salon" }
        };
        var mappings = new List<InventoryLocationMappingDto>
        {
            new() { SourceLocation = "Salon", AreaName = "Salon", LocationName = "Rugleuning" }
        };

        var result = await BuildService().ExecuteImportAsync(rows, mappings);

        Assert.True(result.Success, result.ErrorMessage);

        // Alle inventory-tabellen zijn gewist.
        _mutationRepo.Verify(r => r.DeleteAsync(It.IsAny<StockMutation>(), It.IsAny<CancellationToken>()), Times.Once);
        _stockRepo.Verify(r => r.DeleteAsync(It.IsAny<Stock>(), It.IsAny<CancellationToken>()), Times.Once);
        _expectedRepo.Verify(r => r.DeleteAsync(It.IsAny<StockExpectedLocation>(), It.IsAny<CancellationToken>()), Times.Once);
        _codeRepo.Verify(r => r.DeleteAsync(It.IsAny<ProductCode>(), It.IsAny<CancellationToken>()), Times.Once);
        _mappingRepo.Verify(r => r.DeleteAsync(It.IsAny<ProductCategoryMapping>(), It.IsAny<CancellationToken>()), Times.Once);
        _productRepo.Verify(r => r.DeleteAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _locationRepo.Verify(r => r.DeleteAsync(It.IsAny<StorageLocation>(), It.IsAny<CancellationToken>()), Times.Once);
        _areaRepo.Verify(r => r.DeleteAsync(It.IsAny<StorageArea>(), It.IsAny<CancellationToken>()), Times.Once);

        // Restrict-FK-volgorde: expected-locations vóór locaties, locaties vóór gebieden.
        Assert.True(deleteOrder.IndexOf("expected") < deleteOrder.IndexOf("location"));
        Assert.True(deleteOrder.IndexOf("location") < deleteOrder.IndexOf("area"));

        // Bestaande eenheid 'liter' blijft behouden: geen nieuwe eenheid aangemaakt.
        _units.Verify(u => u.CreateAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal(0, result.UnitsCreated);
    }

    [Fact]
    public async Task ExecuteImportAsync_CreatesMissingUnitAdditively_AndReusesExisting()
    {
        var existingUnit = new UnitDto { Id = Guid.NewGuid(), Name = "liter" };
        SetupCreationDefaults(new List<UnitDto> { existingUnit });

        var rows = new List<InventoryImportRowDto>
        {
            new() { Quantity = 4m, Unit = "liter", ProductName = "Rivella", SourceLocation = "Salon" },
            new() { Quantity = 2m, Unit = "pak", ProductName = "koffiebonen", SourceLocation = "Salon" }
        };
        var mappings = new List<InventoryLocationMappingDto>
        {
            new() { SourceLocation = "Salon", AreaName = "Salon", LocationName = "Rugleuning" }
        };

        var result = await BuildService().ExecuteImportAsync(rows, mappings);

        Assert.True(result.Success, result.ErrorMessage);
        _units.Verify(u => u.CreateAsync("pak", It.IsAny<CancellationToken>()), Times.Once);
        _units.Verify(u => u.CreateAsync("liter", It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal(1, result.UnitsCreated);
    }

    [Fact]
    public async Task ExecuteImportAsync_ReusesMappingForRepeatedSourceLocation()
    {
        SetupCreationDefaults();

        var rows = new List<InventoryImportRowDto>
        {
            new() { Quantity = 4m, Unit = "liter", ProductName = "Rivella", SourceLocation = "Salon" },
            new() { Quantity = 10m, Unit = "liter", ProductName = "melk", SourceLocation = "Salon" }
        };
        var mappings = new List<InventoryLocationMappingDto>
        {
            new() { SourceLocation = "Salon", AreaName = "Salon", LocationName = "Rugleuning" }
        };

        var result = await BuildService().ExecuteImportAsync(rows, mappings);

        Assert.True(result.Success, result.ErrorMessage);
        _storage.Verify(s => s.CreateAreaAsync("Salon", It.IsAny<CancellationToken>()), Times.Once);
        _storage.Verify(s => s.CreateLocationAsync(It.IsAny<Guid>(), "Rugleuning", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
        _storage.Verify(s => s.GenerateOrGetQrTokenAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _stock.Verify(s => s.AddOrIncrementStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        Assert.Equal(1, result.AreasCreated);
        Assert.Equal(1, result.LocationsCreated);
        Assert.Equal(1, result.TokensGenerated);
    }

    [Fact]
    public async Task ExecuteImportAsync_CreatesProductsWithoutCategory_AndPassesCsvQuantityToStock()
    {
        SetupCreationDefaults();

        var rows = new List<InventoryImportRowDto>
        {
            new() { Quantity = 1.5m, Unit = "pak", ProductName = "koffiebonen", SourceLocation = "Salon" }
        };
        var mappings = new List<InventoryLocationMappingDto>
        {
            new() { SourceLocation = "Salon", AreaName = "Salon", LocationName = "Rugleuning" }
        };

        var result = await BuildService().ExecuteImportAsync(rows, mappings);

        Assert.True(result.Success, result.ErrorMessage);
        _products.Verify(p => p.CreateAsync("koffiebonen", null, It.IsAny<Guid>(), (Guid?)null, It.IsAny<CancellationToken>()), Times.Once);
        _stock.Verify(s => s.AddOrIncrementStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), 1.5m, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(1, result.ProductsCreated);
        Assert.Equal(1, result.StockRowsCreated);
    }

    [Fact]
    public async Task ExecuteImportAsync_GeneratesTokenForEveryImportedLocation_AndDedupesArea()
    {
        SetupCreationDefaults();

        var rows = new List<InventoryImportRowDto>
        {
            new() { Quantity = 4m, Unit = "liter", ProductName = "Rivella", SourceLocation = "Salon rugleuning" },
            new() { Quantity = 2m, Unit = "kilo", ProductName = "bloem", SourceLocation = "Salon boven" }
        };
        var mappings = new List<InventoryLocationMappingDto>
        {
            new() { SourceLocation = "Salon rugleuning", AreaName = "Salon", LocationName = "Rugleuning" },
            new() { SourceLocation = "Salon boven", AreaName = "Salon", LocationName = "Boven" }
        };

        var result = await BuildService().ExecuteImportAsync(rows, mappings);

        Assert.True(result.Success, result.ErrorMessage);
        _storage.Verify(s => s.CreateAreaAsync("Salon", It.IsAny<CancellationToken>()), Times.Once); // gedeeld gebied één keer
        _storage.Verify(s => s.CreateLocationAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _storage.Verify(s => s.GenerateOrGetQrTokenAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        Assert.Equal(1, result.AreasCreated);
        Assert.Equal(2, result.LocationsCreated);
        Assert.Equal(2, result.TokensGenerated);
        Assert.Equal(2, result.ImportedLocationIds.Count);
    }

    private static void SetupRepoWithOrder<T>(Mock<IRepository<T>> repo, T entity, string label, List<string> order) where T : class
    {
        repo.Setup(r => r.ListAsync(It.IsAny<Expression<Func<T, bool>>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<T> { entity });
        repo.Setup(r => r.DeleteAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback(() => order.Add(label));
    }
}
