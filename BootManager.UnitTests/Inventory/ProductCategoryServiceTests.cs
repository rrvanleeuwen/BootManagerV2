using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.Services;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Moq;
using Xunit;

namespace BootManager.UnitTests.Inventory;

public class ProductCategoryServiceTests
{
    private readonly Mock<IRepository<ProductCategory>> _categoryRepoMock;
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly IProductCategoryService _service;

    public ProductCategoryServiceTests()
    {
        _categoryRepoMock = new Mock<IRepository<ProductCategory>>();
        _productRepoMock = new Mock<IRepository<Product>>();
        _service = new ProductCategoryService(_categoryRepoMock.Object, _productRepoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_CreatesCategory()
    {
        // Arrange
        var name = "Drinken";
        var description = "Drank producten";
        var iconKey = "beverage";

        _categoryRepoMock
            .Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCategory, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductCategory?)null);
        _categoryRepoMock.Setup(r => r.AddAsync(It.IsAny<ProductCategory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(name, description, iconKey);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(name, result.Data.Name);
        Assert.Equal(description, result.Data.Description);
        Assert.Equal(iconKey, result.Data.IconKey);
        _categoryRepoMock.Verify(r => r.AddAsync(It.IsAny<ProductCategory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsError()
    {
        // Act
        var result = await _service.CreateAsync("", null, "beverage");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("leeg", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        _categoryRepoMock.Verify(r => r.AddAsync(It.IsAny<ProductCategory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ReturnsError()
    {
        // Arrange
        var name = "Drinken";
        var existing = ProductCategory.Create(name, null, "beverage");

        _categoryRepoMock
            .Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<ProductCategory, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await _service.CreateAsync(name, null, "beverage");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("bestaat", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        _categoryRepoMock.Verify(r => r.AddAsync(It.IsAny<ProductCategory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidIconKey_ReturnsError()
    {
        // Act
        var result = await _service.CreateAsync("Drinken", null, "invalid_icon");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("icoon", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ArchiveAsync_WithoutActiveProducts_Archives()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = ProductCategory.Create("Drinken", null, "beverage");

        _categoryRepoMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _productRepoMock
            .Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _categoryRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ProductCategory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ArchiveAsync(categoryId);

        // Assert
        Assert.True(result.Success);
        Assert.True(category.IsArchived);
        _categoryRepoMock.Verify(r => r.UpdateAsync(category, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ArchiveAsync_WithActiveProducts_ReturnsError()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = ProductCategory.Create("Drinken", null, "beverage");

        _categoryRepoMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _productRepoMock
            .Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.ArchiveAsync(categoryId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("actieve", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        _categoryRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ProductCategory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void IsValidIconKey_WithValidKey_ReturnsTrue()
    {
        // Assert
        Assert.True(_service.IsValidIconKey("beverage"));
        Assert.True(_service.IsValidIconKey("part"));
        Assert.True(_service.IsValidIconKey("tool"));
    }

    [Fact]
    public void IsValidIconKey_WithInvalidKey_ReturnsFalse()
    {
        // Assert
        Assert.False(_service.IsValidIconKey("invalid"));
        Assert.False(_service.IsValidIconKey(""));
    }
}
