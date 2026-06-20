using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.Services;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Moq;

namespace BootManager.UnitTests.Inventory;

/// <summary>
/// Unit tests for StockService: add/increment, delete, search, quantity validation.
/// </summary>
public class StockServiceTests
{
    private readonly Mock<IRepository<Stock>> _stockRepoMock;
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly Mock<IRepository<StorageLocation>> _locationRepoMock;
    private readonly Mock<IRepository<Unit>> _unitRepoMock;
    private readonly Mock<IRepository<StorageArea>> _areaRepoMock;
    private readonly Mock<IRepository<ProductCode>> _codeRepoMock;
    private readonly StockService _service;

    public StockServiceTests()
    {
        _stockRepoMock = new Mock<IRepository<Stock>>();
        _productRepoMock = new Mock<IRepository<Product>>();
        _locationRepoMock = new Mock<IRepository<StorageLocation>>();
        _unitRepoMock = new Mock<IRepository<Unit>>();
        _areaRepoMock = new Mock<IRepository<StorageArea>>();
        _codeRepoMock = new Mock<IRepository<ProductCode>>();
        _service = new StockService(
            _stockRepoMock.Object,
            _productRepoMock.Object,
            _locationRepoMock.Object,
            _unitRepoMock.Object,
            _areaRepoMock.Object,
            _codeRepoMock.Object);
    }

    [Fact]
    public async Task AddOrIncrementStockAsync_CreatesNewStock_WhenProductAndLocationExist()
    {
        var unitId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var areaId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var quantity = 10m;

        var unit = Unit.Create("Stuk");
        var product = Product.Create("TestProduct", null, unitId);
        var area = StorageArea.Create("TestArea");
        var location = StorageLocation.Create(areaId, "TestLocation", null);

        var stockCapture = new Stock[1];

        _productRepoMock.Setup(r => r.GetByIdAsync(productId, default))
            .ReturnsAsync(product);
        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);
        _unitRepoMock.Setup(r => r.GetByIdAsync(unitId, default))
            .ReturnsAsync(unit);
        _areaRepoMock.Setup(r => r.GetByIdAsync(areaId, default))
            .ReturnsAsync(area);
        _stockRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Stock, bool>>>(), default))
            .ReturnsAsync((Stock?)null);
        _stockRepoMock.Setup(r => r.AddAsync(It.IsAny<Stock>(), default))
            .Callback<Stock, CancellationToken>((s, _) => stockCapture[0] = s)
            .Returns(Task.CompletedTask);

        var result = await _service.AddOrIncrementStockAsync(productId, locationId, quantity);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(quantity, result.Data.Quantity);
        Assert.Equal("Stuk", result.Data.DefaultUnitName);
        _stockRepoMock.Verify(r => r.AddAsync(It.IsAny<Stock>(), default), Times.Once);
    }

    [Fact]
    public async Task AddOrIncrementStockAsync_IncrementsExistingStock_WhenProductExistsOnLocation()
    {
        var unitId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var areaId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var initialQuantity = 5m;
        var additionalQuantity = 10m;

        var unit = Unit.Create("Stuk");
        var product = Product.Create("TestProduct", null, unitId);
        var area = StorageArea.Create("TestArea");
        var location = StorageLocation.Create(areaId, "TestLocation", null);
        var existingStock = Stock.Create(productId, locationId, initialQuantity);

        _productRepoMock.Setup(r => r.GetByIdAsync(productId, default))
            .ReturnsAsync(product);
        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);
        _unitRepoMock.Setup(r => r.GetByIdAsync(unitId, default))
            .ReturnsAsync(unit);
        _areaRepoMock.Setup(r => r.GetByIdAsync(areaId, default))
            .ReturnsAsync(area);
        _stockRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Stock, bool>>>(), default))
            .ReturnsAsync(existingStock);
        _stockRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Stock>(), default))
            .Returns(Task.CompletedTask);

        var result = await _service.AddOrIncrementStockAsync(productId, locationId, additionalQuantity);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(initialQuantity + additionalQuantity, result.Data.Quantity);
        _stockRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Stock>(), default), Times.Once);
    }

    [Fact]
    public async Task AddOrIncrementStockAsync_RejectsZeroQuantity()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var result = await _service.AddOrIncrementStockAsync(productId, locationId, 0);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("groter dan 0", result.ErrorMessage);
    }

    [Fact]
    public async Task AddOrIncrementStockAsync_RejectsNegativeQuantity()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var result = await _service.AddOrIncrementStockAsync(productId, locationId, -5);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task AddOrIncrementStockAsync_ReturnsError_WhenProductNotFound()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        _productRepoMock.Setup(r => r.GetByIdAsync(productId, default))
            .ReturnsAsync((Product?)null);

        var result = await _service.AddOrIncrementStockAsync(productId, locationId, 10);

        Assert.False(result.Success);
        Assert.Contains("Product", result.ErrorMessage);
    }

    [Fact]
    public async Task AddOrIncrementStockAsync_ReturnsError_WhenLocationNotFound()
    {
        var unitId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();

        var unit = Unit.Create("Stuk");
        var product = Product.Create("TestProduct", null, unitId);

        _productRepoMock.Setup(r => r.GetByIdAsync(productId, default))
            .ReturnsAsync(product);
        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync((StorageLocation?)null);

        var result = await _service.AddOrIncrementStockAsync(productId, locationId, 10);

        Assert.False(result.Success);
        Assert.Contains("Opslaglocatie", result.ErrorMessage);
    }

    [Fact]
    public async Task GetStocksByLocationAsync_ReturnsStocks_WhenLocationHasStocks()
    {
        var unitId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var areaId = Guid.NewGuid();

        var unit = Unit.Create("Stuk");
        var product1 = Product.Create("Product1", null, unitId);
        var product2 = Product.Create("Product2", null, unitId);
        var area = StorageArea.Create("TestArea");
        var location = StorageLocation.Create(areaId, "TestLocation", null);
        var stock1 = Stock.Create(productId1, locationId, 5);
        var stock2 = Stock.Create(productId2, locationId, 10);

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);
        _areaRepoMock.Setup(r => r.GetByIdAsync(areaId, default))
            .ReturnsAsync(area);
        _stockRepoMock.Setup(r => r.ListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Stock, bool>>>(), default))
            .ReturnsAsync(new List<Stock> { stock1, stock2 });
        _productRepoMock.Setup(r => r.GetByIdAsync(productId1, default))
            .ReturnsAsync(product1);
        _productRepoMock.Setup(r => r.GetByIdAsync(productId2, default))
            .ReturnsAsync(product2);
        _unitRepoMock.Setup(r => r.GetByIdAsync(unitId, default))
            .ReturnsAsync(unit);

        var result = await _service.GetStocksByLocationAsync(locationId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task DeleteStockAsync_DeletesStock_WhenStockExists()
    {
        var stockId = Guid.NewGuid();
        var stock = Stock.Create(Guid.NewGuid(), Guid.NewGuid(), 5);

        _stockRepoMock.Setup(r => r.GetByIdAsync(stockId, default))
            .ReturnsAsync(stock);
        _stockRepoMock.Setup(r => r.DeleteAsync(stock, default))
            .Returns(Task.CompletedTask);

        var result = await _service.DeleteStockAsync(stockId);

        Assert.True(result.Success);
        _stockRepoMock.Verify(r => r.DeleteAsync(stock, default), Times.Once);
    }

    [Fact]
    public async Task DeleteStockAsync_ReturnsError_WhenStockNotFound()
    {
        var stockId = Guid.NewGuid();

        _stockRepoMock.Setup(r => r.GetByIdAsync(stockId, default))
            .ReturnsAsync((Stock?)null);

        var result = await _service.DeleteStockAsync(stockId);

        Assert.False(result.Success);
        Assert.Contains("Voorraadregel", result.ErrorMessage);
    }

    [Fact]
    public async Task SearchProductsInLocationAsync_FindsProductsByName()
    {
        var unitId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var areaId = Guid.NewGuid();
        var searchTerm = "test";

        var unit = Unit.Create("Stuk");
        var product = Product.Create("TestProduct", null, unitId);
        var area = StorageArea.Create("TestArea");
        var location = StorageLocation.Create(areaId, "TestLocation", null);

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);
        _productRepoMock.Setup(r => r.ListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), default))
            .ReturnsAsync(new List<Product> { product });
        _codeRepoMock.Setup(r => r.ListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProductCode, bool>>>(), default))
            .ReturnsAsync(new List<ProductCode>());
        _unitRepoMock.Setup(r => r.GetByIdAsync(unitId, default))
            .ReturnsAsync(unit);

        var result = await _service.SearchProductsInLocationAsync(locationId, searchTerm);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("TestProduct", result.Data[0].Name);
    }

    [Fact]
    public async Task SearchProductsInLocationAsync_FindsProductsByLinkedCode()
    {
        var unitId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var areaId = Guid.NewGuid();
        var searchTerm = "ABC123";

        var unit = Unit.Create("Stuk");
        var product = Product.Create("TestProduct", null, unitId);
        var location = StorageLocation.Create(areaId, "TestLocation", null);
        var code = ProductCode.Create(product.Id, "ABC123", "EAN");

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);
        _productRepoMock.Setup(r => r.ListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), default))
            .ReturnsAsync(new List<Product> { product });
        _codeRepoMock.Setup(r => r.ListAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<ProductCode, bool>>?>(),
            default))
            .ReturnsAsync(new List<ProductCode> { code });
        _unitRepoMock.Setup(r => r.GetByIdAsync(unitId, default))
            .ReturnsAsync(unit);

        var result = await _service.SearchProductsInLocationAsync(locationId, searchTerm);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("TestProduct", result.Data[0].Name);
        Assert.NotNull(result.Data[0].Code);
        Assert.Equal("ABC123", result.Data[0].Code.Value);
    }

    [Fact]
    public async Task AddOrIncrementStockAsync_WorksWithNewlyCreatedProduct_FromLocationContext()
    {
        var unitId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var areaId = Guid.NewGuid();
        var quantity = 15m;

        var unit = Unit.Create("Stuk");
        var newProduct = Product.Create("NewlyCreatedProduct", null, unitId);
        var area = StorageArea.Create("TestArea");
        var location = StorageLocation.Create(areaId, "TestLocation", null);

        _productRepoMock.Setup(r => r.GetByIdAsync(newProduct.Id, default))
            .ReturnsAsync(newProduct);
        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);
        _unitRepoMock.Setup(r => r.GetByIdAsync(unitId, default))
            .ReturnsAsync(unit);
        _areaRepoMock.Setup(r => r.GetByIdAsync(areaId, default))
            .ReturnsAsync(area);
        _stockRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Stock, bool>>>(), default))
            .ReturnsAsync((Stock?)null);
        _stockRepoMock.Setup(r => r.AddAsync(It.IsAny<Stock>(), default))
            .Returns(Task.CompletedTask);

        var result = await _service.AddOrIncrementStockAsync(newProduct.Id, locationId, quantity);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("NewlyCreatedProduct", result.Data.ProductName);
        Assert.Equal(quantity, result.Data.Quantity);
        Assert.Equal("Stuk", result.Data.DefaultUnitName);
        _stockRepoMock.Verify(r => r.AddAsync(It.IsAny<Stock>(), default), Times.Once);
    }
}
