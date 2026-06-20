using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.Services;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Moq;
using Xunit;

namespace BootManager.UnitTests.Inventory;

public class UnitServiceTests
{
    private readonly Mock<IRepository<Unit>> _unitRepoMock;
    private readonly Mock<IRepository<Product>> _productRepoMock;
    private readonly IUnitService _service;

    public UnitServiceTests()
    {
        _unitRepoMock = new Mock<IRepository<Unit>>();
        _productRepoMock = new Mock<IRepository<Product>>();
        _service = new UnitService(_unitRepoMock.Object, _productRepoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidInput_CreatesUnit()
    {
        // Arrange
        var name = "liter";

        _unitRepoMock
            .Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Unit, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Unit?)null);
        _unitRepoMock.Setup(r => r.AddAsync(It.IsAny<Unit>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(name);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(name, result.Data.Name);
        _unitRepoMock.Verify(r => r.AddAsync(It.IsAny<Unit>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsError()
    {
        // Act
        var result = await _service.CreateAsync("");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("leeg", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        _unitRepoMock.Verify(r => r.AddAsync(It.IsAny<Unit>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ReturnsError()
    {
        // Arrange
        var name = "liter";
        var existing = Unit.Create(name);

        _unitRepoMock
            .Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Unit, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await _service.CreateAsync(name);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("bestaat", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        _unitRepoMock.Verify(r => r.AddAsync(It.IsAny<Unit>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ArchiveAsync_WithoutActiveProducts_Archives()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        var unit = Unit.Create("liter");

        _unitRepoMock.Setup(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unit);
        _productRepoMock
            .Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _unitRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Unit>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ArchiveAsync(unitId);

        // Assert
        Assert.True(result.Success);
        Assert.True(unit.IsArchived);
        _unitRepoMock.Verify(r => r.UpdateAsync(unit, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ArchiveAsync_WithActiveProducts_ReturnsError()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        var unit = Unit.Create("liter");

        _unitRepoMock.Setup(r => r.GetByIdAsync(unitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unit);
        _productRepoMock
            .Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.ArchiveAsync(unitId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("actieve", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        _unitRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Unit>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InitializeDefaultUnitsAsync_WithEmptyDatabase_CreatesDefaults()
    {
        // Arrange
        _unitRepoMock.Setup(r => r.CountAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _unitRepoMock.Setup(r => r.AddAsync(It.IsAny<Unit>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.InitializeDefaultUnitsAsync();

        // Assert
        _unitRepoMock.Verify(r => r.AddAsync(It.IsAny<Unit>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task InitializeDefaultUnitsAsync_WithExistingData_DoesNotAdd()
    {
        // Arrange
        _unitRepoMock.Setup(r => r.CountAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.InitializeDefaultUnitsAsync();

        // Assert
        _unitRepoMock.Verify(r => r.AddAsync(It.IsAny<Unit>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
