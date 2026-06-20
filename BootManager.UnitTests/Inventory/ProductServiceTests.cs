using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.Services;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Moq;
using Xunit;

namespace BootManager.UnitTests.Inventory;

public class ProductServiceTests
{
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly Mock<IRepository<ProductCode>> _codeRepoMock;
    private readonly Mock<IRepository<ProductCategoryMapping>> _mappingRepoMock;
    private readonly Mock<IRepository<ProductCategory>> _categoryRepoMock;
    private readonly Mock<IRepository<Unit>> _unitRepoMock;
    private readonly IProductService _service;

    public ProductServiceTests()
    {
        _productRepoMock = new Mock<IRepository<Product>>();
        _codeRepoMock = new Mock<IRepository<ProductCode>>();
        _mappingRepoMock = new Mock<IRepository<ProductCategoryMapping>>();
        _categoryRepoMock = new Mock<IRepository<ProductCategory>>();
        _unitRepoMock = new Mock<IRepository<Unit>>();
        _service = new ProductService(
            _productRepoMock.Object,
            _codeRepoMock.Object,
            _mappingRepoMock.Object,
            _categoryRepoMock.Object,
            _unitRepoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_CreatesProduct()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        var unit = Unit.Create("liter");

        _unitRepoMock.Setup(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unit);
        _productRepoMock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mappingRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCategoryMapping, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategoryMapping?)null);

        // Act
        var result = await _service.CreateAsync("Appel", "Rode appels", unitId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Appel", result.Data.Name);
        Assert.Equal(unitId, result.Data.DefaultUnitId);
        _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsError()
    {
        // Arrange
        var unitId = Guid.NewGuid();

        // Act
        var result = await _service.CreateAsync("", null, unitId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("leeg", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidUnit_ReturnsError()
    {
        // Arrange
        var unitId = Guid.NewGuid();

        _unitRepoMock.Setup(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Unit?)null);

        // Act
        var result = await _service.CreateAsync("Appel", null, unitId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("eenheid", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AddCodeAsync_WithValidInput_AddsCode()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = Product.Create("Appel", null, Guid.NewGuid());

        _productRepoMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _codeRepoMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCode, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _codeRepoMock
            .Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCode, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCode?)null);
        _codeRepoMock.Setup(r => r.AddAsync(It.IsAny<ProductCode>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddCodeAsync(productId, "123456789", "barcode");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("123456789", result.Data.Value);
        Assert.Equal("barcode", result.Data.Format);
    }

    [Fact]
    public async Task AddCodeAsync_WithDuplicateCode_ReturnsError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = Product.Create("Appel", null, Guid.NewGuid());

        _productRepoMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _codeRepoMock
            .Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCode, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCode?)null);
        _codeRepoMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCode, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.AddCodeAsync(productId, "123456789", "barcode");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("bestaat", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task IsCodeValueUniqueAsync_WithUniqueCode_ReturnsTrue()
    {
        // Arrange
        _codeRepoMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCode, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.IsCodeValueUniqueAsync("123456789");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsCodeValueUniqueAsync_WithDuplicateCode_ReturnsFalse()
    {
        // Arrange
        _codeRepoMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCode, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.IsCodeValueUniqueAsync("123456789");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RemoveCodeAsync_WithExistingCode_RemovesIt()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var code = ProductCode.Create(productId, "123456789", "barcode");
        var product = Product.Create("Appel", null, Guid.NewGuid());

        _productRepoMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _codeRepoMock
            .Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCode, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(code);
        _codeRepoMock.Setup(r => r.DeleteAsync(code, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RemoveCodeAsync(productId);

        // Assert
        Assert.True(result.Success);
        _codeRepoMock.Verify(r => r.DeleteAsync(code, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithStoredCode_MapsCodeIntoDto()
    {
        var productId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var product = Product.Create("Appel", null, unitId);
        var code = ProductCode.Create(productId, "4335619174771", "barcode");
        var unit = Unit.Create("stuks");

        typeof(Product).GetProperty(nameof(Product.Id))?.SetValue(product, productId);

        _productRepoMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _unitRepoMock.Setup(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unit);
        _mappingRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCategoryMapping, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategoryMapping?)null);
        _codeRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCode, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(code);

        var result = await _service.GetByIdAsync(productId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Code);
        Assert.Equal("4335619174771", result.Data.Code.Value);
        Assert.Equal("barcode", result.Data.Code.Format);
    }

    [Fact]
    public async Task SearchByNameOrDescriptionAsync_FindsProductByName_CaseInsensitive()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        var product1 = Product.Create("Appel", "Rode appels", unitId);
        var product2 = Product.Create("Banaan", "Gele fruit", unitId);
        var unit = Unit.Create("stuks");

        _productRepoMock.Setup(r => r.ListAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product1, product2 });
        _unitRepoMock.Setup(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unit);
        _mappingRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCategoryMapping, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategoryMapping?)null);
        _codeRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCode, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCode?)null);

        // Act
        var result = await _service.SearchByNameOrDescriptionAsync("appel");

        // Assert
        Assert.Single(result);
        Assert.Equal("Appel", result.First().Name);
    }

    [Fact]
    public async Task SearchByNameOrDescriptionAsync_FindsProductByDescription_CaseInsensitive()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        var product1 = Product.Create("Appel", "Rode appels", unitId);
        var product2 = Product.Create("Banaan", "Gele fruit", unitId);
        var unit = Unit.Create("stuks");

        _productRepoMock.Setup(r => r.ListAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product1, product2 });
        _unitRepoMock.Setup(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unit);
        _mappingRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCategoryMapping, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategoryMapping?)null);
        _codeRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCode, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCode?)null);

        // Act
        var result = await _service.SearchByNameOrDescriptionAsync("RODE");

        // Assert
        Assert.Single(result);
        Assert.Equal("Appel", result.First().Name);
    }

    [Fact]
    public async Task SearchByNameOrDescriptionAsync_ReturnMultipleMatches_WhenMultipleProductsMatch()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        var product1 = Product.Create("Appel", "Rode appels", unitId);
        var product2 = Product.Create("Appelsin", "Oranje fruit", unitId);
        var product3 = Product.Create("Banaan", "Gele fruit", unitId);
        var unit = Unit.Create("stuks");

        _productRepoMock.Setup(r => r.ListAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product1, product2, product3 });
        _unitRepoMock.Setup(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unit);
        _mappingRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCategoryMapping, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategoryMapping?)null);
        _codeRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCode, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCode?)null);

        // Act
        var result = await _service.SearchByNameOrDescriptionAsync("appel");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "Appel");
        Assert.Contains(result, p => p.Name == "Appelsin");
    }

    [Fact]
    public async Task SearchByNameOrDescriptionAsync_ReturnsEmpty_WhenNoMatches()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        var product1 = Product.Create("Appel", "Rode appels", unitId);
        var unit = Unit.Create("stuks");

        _productRepoMock.Setup(r => r.ListAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product1 });
        _unitRepoMock.Setup(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unit);
        _mappingRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCategoryMapping, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategoryMapping?)null);
        _codeRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCode, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCode?)null);

        // Act
        var result = await _service.SearchByNameOrDescriptionAsync("banaan");

        // Assert
        Assert.Empty(result);
    }
}
