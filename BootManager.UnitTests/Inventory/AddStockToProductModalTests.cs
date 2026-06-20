using Bunit;
using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Services;
using BootManager.Web.Components.Inventory;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BootManager.UnitTests.Inventory;

/// <summary>
/// Real bUnit tests for AddStockToProductModal component.
/// Tests location selection, quantity input, successful save, error handling, and cancellation.
/// </summary>
public class AddStockToProductModalTests : TestContext
{
    private readonly Mock<IStockService> _stockServiceMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();

    public AddStockToProductModalTests()
    {
        Services.AddScoped<IStockService>(_ => _stockServiceMock.Object);
        Services.AddScoped<IStorageService>(_ => _storageServiceMock.Object);
    }

    [Fact]
    public async Task Modal_ShowsProductName_WhenInitialized()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk",
            Code = new ProductCodeDto { Value = "TEST123" }
        };

        var locations = new List<StorageLocationOverviewDto>
        {
            new() { Id = Guid.NewGuid(), AreaName = "Kombuis", LocationName = "Kast" }
        };

        _storageServiceMock
            .Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Error("No expected location"));

        var cut = RenderComponent<AddStockToProductModal>(
            ComponentParameter.CreateParameter(nameof(AddStockToProductModal.Product), product));

        // Assert
        cut.WaitForAssertion(() =>
            Assert.Contains("Test Product", cut.Markup));
        Assert.Contains("TEST123", cut.Markup);
    }

    [Fact]
    public async Task Modal_ShowsAvailableLocations_InDropdown()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        var locations = new List<StorageLocationOverviewDto>
        {
            new() { Id = locationId, AreaName = "Kombuis", LocationName = "Kast" },
            new() { Id = Guid.NewGuid(), AreaName = "Garage", LocationName = "Rek" }
        };

        _storageServiceMock
            .Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Error("No expected location"));

        var cut = RenderComponent<AddStockToProductModal>(
            ComponentParameter.CreateParameter(nameof(AddStockToProductModal.Product), product));

        // Assert
        cut.WaitForAssertion(() =>
        {
            var options = cut.FindAll("option");
            Assert.Contains(options, o => o.TextContent.Contains("Kombuis - Kast"));
            Assert.Contains(options, o => o.TextContent.Contains("Garage - Rek"));
        });
    }

    [Fact]
    public async Task Modal_ShowsQuantityInput_WithProductUnit()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "kilogram"
        };

        var locations = new List<StorageLocationOverviewDto>
        {
            new() { Id = Guid.NewGuid(), AreaName = "Kombuis", LocationName = "Kast" }
        };

        _storageServiceMock
            .Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Error("No expected location"));

        var cut = RenderComponent<AddStockToProductModal>(
            ComponentParameter.CreateParameter(nameof(AddStockToProductModal.Product), product));

        // Assert
        cut.WaitForAssertion(() =>
            Assert.Contains("kilogram", cut.Markup));
    }

    [Fact]
    public async Task Modal_SaveButton_DisabledWhenNoLocationOrQuantity()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        var locations = new List<StorageLocationOverviewDto>
        {
            new() { Id = Guid.NewGuid(), AreaName = "Kombuis", LocationName = "Kast" }
        };

        _storageServiceMock
            .Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Error("No expected location"));

        var cut = RenderComponent<AddStockToProductModal>(
            ComponentParameter.CreateParameter(nameof(AddStockToProductModal.Product), product));

        // Assert
        cut.WaitForAssertion(() =>
        {
            var saveButton = cut.FindAll("button").First(b => b.TextContent.Contains("Voorraad toevoegen"));
            Assert.True(saveButton.HasAttribute("disabled"));
        });
    }

    [Fact]
    public async Task Modal_SaveButton_EnabledWhenLocationAndQuantityProvided()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        var locations = new List<StorageLocationOverviewDto>
        {
            new() { Id = locationId, AreaName = "Kombuis", LocationName = "Kast" }
        };

        _storageServiceMock
            .Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Error("No expected location"));

        var cut = RenderComponent<AddStockToProductModal>(
            ComponentParameter.CreateParameter(nameof(AddStockToProductModal.Product), product));

        // Act: Select location and enter quantity
        await cut.InvokeAsync(() =>
        {
            var select = cut.Find("select");
            select.Change(locationId.ToString());
            var input = cut.Find("input[type='number']");
            input.Change(5);
        });

        // Assert
        cut.WaitForAssertion(() =>
        {
            var saveButton = cut.FindAll("button").First(b => b.TextContent.Contains("Voorraad toevoegen"));
            Assert.False(saveButton.HasAttribute("disabled"));
        });
    }

    [Fact]
    public async Task Modal_OnClosingButton_InvokesOnCloseCallback()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        var locations = new List<StorageLocationOverviewDto>
        {
            new() { Id = Guid.NewGuid(), AreaName = "Kombuis", LocationName = "Kast" }
        };

        _storageServiceMock
            .Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Error("No expected location"));

        var onCloseInvoked = false;
        var cut = RenderComponent<AddStockToProductModal>(
            ComponentParameter.CreateParameter(nameof(AddStockToProductModal.Product), product),
            ComponentParameter.CreateParameter(nameof(AddStockToProductModal.OnClose),
                EventCallback.Factory.Create(this, () => { onCloseInvoked = true; })));

        // Act
        await cut.InvokeAsync(() =>
        {
            var closeButton = cut.FindAll("button").First(b => b.TextContent.Contains("Sluiten"));
            closeButton.Click();
        });

        // Assert
        Assert.True(onCloseInvoked);
    }

    [Fact]
    public async Task Modal_SuccessfulSave_CallsAddOrIncrementStockAsync()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        var locations = new List<StorageLocationOverviewDto>
        {
            new() { Id = locationId, AreaName = "Kombuis", LocationName = "Kast" }
        };

        var savedStock = new StockDto
        {
            StorageLocationId = locationId,
            StorageAreaName = "Kombuis",
            StorageLocationName = "Kast",
            Quantity = 5,
            DefaultUnitName = "stuk"
        };

        _storageServiceMock
            .Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Error("No expected location"));

        _stockServiceMock
            .Setup(s => s.AddOrIncrementStockAsync(productId, locationId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(savedStock));

        var cut = RenderComponent<AddStockToProductModal>(
            ComponentParameter.CreateParameter(nameof(AddStockToProductModal.Product), product));

        // Act
        await cut.InvokeAsync(() =>
        {
            var select = cut.Find("select");
            select.Change(locationId.ToString());
            var input = cut.Find("input[type='number']");
            input.Change(5);
            var saveButton = cut.FindAll("button").First(b => b.TextContent.Contains("Voorraad toevoegen"));
            saveButton.Click();
        });

        // Assert
        cut.WaitForAssertion(() =>
        {
            _stockServiceMock.Verify(
                s => s.AddOrIncrementStockAsync(productId, locationId, 5, It.IsAny<CancellationToken>()),
                Times.Once);
        });
    }

    [Fact]
    public async Task Modal_SuccessfulSave_InvokesOnStockAddedCallback()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        var locations = new List<StorageLocationOverviewDto>
        {
            new() { Id = locationId, AreaName = "Kombuis", LocationName = "Kast" }
        };

        var savedStock = new StockDto
        {
            StorageLocationId = locationId,
            StorageAreaName = "Kombuis",
            StorageLocationName = "Kast",
            Quantity = 5,
            DefaultUnitName = "stuk"
        };

        _storageServiceMock
            .Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Error("No expected location"));

        _stockServiceMock
            .Setup(s => s.AddOrIncrementStockAsync(productId, locationId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(savedStock));

        var onStockAddedLocationId = Guid.Empty;
        var cut = RenderComponent<AddStockToProductModal>(
            ComponentParameter.CreateParameter(nameof(AddStockToProductModal.Product), product),
            ComponentParameter.CreateParameter(nameof(AddStockToProductModal.OnStockAdded),
                EventCallback.Factory.Create<Guid>(this, lid => { onStockAddedLocationId = lid; })));

        // Act
        await cut.InvokeAsync(() =>
        {
            var select = cut.Find("select");
            select.Change(locationId.ToString());
            var input = cut.Find("input[type='number']");
            input.Change(5);
            var saveButton = cut.FindAll("button").First(b => b.TextContent.Contains("Voorraad toevoegen"));
            saveButton.Click();
        });

        // Assert
        cut.WaitForAssertion(() =>
            Assert.Equal(locationId, onStockAddedLocationId));
    }

    [Fact]
    public async Task Modal_SaveError_ShowsErrorMessage()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        var locations = new List<StorageLocationOverviewDto>
        {
            new() { Id = locationId, AreaName = "Kombuis", LocationName = "Kast" }
        };

        _storageServiceMock
            .Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Error("No expected location"));

        _stockServiceMock
            .Setup(s => s.AddOrIncrementStockAsync(productId, locationId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Error("Save failed"));

        var cut = RenderComponent<AddStockToProductModal>(
            ComponentParameter.CreateParameter(nameof(AddStockToProductModal.Product), product));

        // Act
        await cut.InvokeAsync(() =>
        {
            var select = cut.Find("select");
            select.Change(locationId.ToString());
            var input = cut.Find("input[type='number']");
            input.Change(5);
            var saveButton = cut.FindAll("button").First(b => b.TextContent.Contains("Voorraad toevoegen"));
            saveButton.Click();
        });

        // Assert
        cut.WaitForAssertion(() =>
            Assert.Contains("Save failed", cut.Markup));
    }
}
