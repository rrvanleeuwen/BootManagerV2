using Bunit;
using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Results;
using BootManager.Application.Storage.Services;
using BootManager.Web.Components.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using System.Security.Claims;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Tests for StorageLocationDetails.razor with stock functionality.
/// </summary>
public class StorageLocationDetailsWithStockComponentTests : TestContext
{
    private readonly Mock<IStorageService> _storageMock = new();
    private readonly Mock<IStockService> _stockMock = new();
    private readonly Mock<IProductService> _productMock = new();
    private readonly Mock<IUnitService> _unitMock = new();

    public StorageLocationDetailsWithStockComponentTests()
    {
        Services.AddScoped<IStorageService>(_ => _storageMock.Object);
        Services.AddScoped<IStockService>(_ => _stockMock.Object);
        Services.AddScoped<IProductService>(_ => _productMock.Object);
        Services.AddScoped<IUnitService>(_ => _unitMock.Object);
        Services.AddScoped<IJSRuntime>(_ => new Mock<IJSRuntime>().Object);
        Services.AddScoped<AuthenticationStateProvider>(_ =>
        {
            var authProviderMock = new Mock<AuthenticationStateProvider>();
            authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
                .ReturnsAsync(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal()));
            return authProviderMock.Object;
        });
    }

    [Fact]
    public async Task Component_DisplaysStockInformation_WhenLocationHasStock()
    {
        var locationId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var areaName = "TestArea";
        var locationName = "TestLocation";

        var stocks = new List<StockDto>
        {
            new StockDto
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                StorageLocationId = locationId,
                ProductName = "TestProduct",
                StorageAreaName = areaName,
                StorageLocationName = locationName,
                Quantity = 25,
                DefaultUnitName = "Stuk"
            }
        };

        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = areaName,
            LocationName = locationName,
            Description = null,
            QrValue = null,
            Stocks = stocks
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationDetails>(
            p => p.Add(c => c.LocationId, locationId));

        var productNameCell = cut.FindAll("td").FirstOrDefault(t => t.TextContent.Contains("TestProduct"));
        Assert.NotNull(productNameCell);
        Assert.Contains("25", cut.Markup);
        Assert.Contains("Stuk", cut.Markup);
    }

    [Fact]
    public async Task Component_DisplaysEmptyMessage_WhenLocationHasNoStock()
    {
        var locationId = Guid.NewGuid();

        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "TestArea",
            LocationName = "TestLocation",
            Description = null,
            QrValue = null,
            Stocks = new List<StockDto>()
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        SetupAuthState(owner: false);

        var cut = RenderComponent<StorageLocationDetails>(
            p => p.Add(c => c.LocationId, locationId));

        Assert.Contains("Geen voorraad op deze locatie", cut.Markup);
    }

    [Fact]
    public async Task Component_ShowsAddStockButton()
    {
        var locationId = Guid.NewGuid();

        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "TestArea",
            LocationName = "TestLocation",
            Stocks = new List<StockDto>()
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        SetupAuthState(owner: false);

        var cut = RenderComponent<StorageLocationDetails>(
            p => p.Add(c => c.LocationId, locationId));

        var addButton = cut.FindAll("button")
            .FirstOrDefault(b => b.TextContent.Contains("Voorraad toevoegen"));

        Assert.NotNull(addButton);
    }

    [Fact]
    public async Task Component_ShowsDeleteButton_ForEachStock()
    {
        var locationId = Guid.NewGuid();
        var stocks = new List<StockDto>
        {
            new StockDto
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                StorageLocationId = locationId,
                ProductName = "Product1",
                StorageAreaName = "Area1",
                StorageLocationName = "Location1",
                Quantity = 10,
                DefaultUnitName = "Stuk"
            }
        };

        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "Area1",
            LocationName = "Location1",
            Stocks = stocks
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        SetupAuthState(owner: false);

        var cut = RenderComponent<StorageLocationDetails>(
            p => p.Add(c => c.LocationId, locationId));

        var deleteButtons = cut.FindAll("button")
            .Where(b => b.ClassList.Contains("btn-outline-danger"));

        Assert.NotEmpty(deleteButtons);
    }

    [Fact]
    public async Task Component_DeletesStock_AfterConfirmation()
    {
        var locationId = Guid.NewGuid();
        var stockId = Guid.NewGuid();
        var stocks = new List<StockDto>
        {
            new StockDto
            {
                Id = stockId,
                ProductId = Guid.NewGuid(),
                StorageLocationId = locationId,
                ProductName = "Product1",
                StorageAreaName = "Area1",
                StorageLocationName = "Location1",
                Quantity = 10,
                DefaultUnitName = "Stuk"
            }
        };

        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "Area1",
            LocationName = "Location1",
            Stocks = stocks
        };

        var detailDtoAfterDelete = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "Area1",
            LocationName = "Location1",
            Stocks = new List<StockDto>()
        };

        _storageMock.SetupSequence(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDtoAfterDelete));

        _stockMock.Setup(s => s.DeleteStockAsync(stockId, default))
            .ReturnsAsync(InventoryOperationResult.Ok());

        SetupAuthState(owner: false);

        var cut = RenderComponent<StorageLocationDetails>(
            p => p.Add(c => c.LocationId, locationId));

        var deleteButton = cut.FindAll("button")
            .First(b => b.ClassList.Contains("btn-outline-danger"));

        await cut.InvokeAsync(() => deleteButton.Click());

        var confirmButton = cut.FindAll("button")
            .FirstOrDefault(b => b.TextContent.Contains("Verwijderen") && b.ClassList.Contains("btn-danger"));

        Assert.NotNull(confirmButton);

        await cut.InvokeAsync(() => confirmButton.Click());

        _stockMock.Verify(s => s.DeleteStockAsync(stockId, default), Times.Once);
    }

    [Fact]
    public async Task AddStockDialog_CreateProductFlow_ReturnsToLocationContextWithSelectedProduct()
    {
        var locationId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var createdProductId = Guid.NewGuid();

        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "Area1",
            LocationName = "Location1",
            Stocks = new List<StockDto>()
        };

        var units = new List<UnitDto>
        {
            new() { Id = unitId, Name = "Stuk", IsArchived = false }
        };

        var createdProduct = new ProductDto
        {
            Id = createdProductId,
            Name = "Nieuwe rijst",
            DefaultUnitId = unitId,
            DefaultUnitName = "Stuk",
            IsArchived = false
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));
        _unitMock.Setup(s => s.GetActiveAsync(default))
            .ReturnsAsync(units);
        _stockMock.Setup(s => s.SearchProductsInLocationAsync(locationId, "Nieuwe rijst", default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<ProductDto>>.Ok(Array.Empty<ProductDto>()));
        _productMock.Setup(s => s.CreateAsync("Nieuwe rijst", null, unitId, null, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(createdProduct));

        SetupAuthState(owner: false);

        var cut = RenderComponent<StorageLocationDetails>(
            p => p.Add(c => c.LocationId, locationId));

        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Voorraad toevoegen"));
        await cut.InvokeAsync(() => addButton.Click());

        var searchInput = cut.Find("input[placeholder='Zoeken op naam of code...']");
        await cut.InvokeAsync(() => searchInput.Input("Nieuwe rijst"));

        var createButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Nieuw product aanmaken"));
        await cut.InvokeAsync(() => createButton.Click());

        var nameInput = cut.Find("input[placeholder='Productnaam']");
        await cut.InvokeAsync(() => nameInput.Change("Nieuwe rijst"));

        var unitSelect = cut.Find("select.form-select");
        await cut.InvokeAsync(() => unitSelect.Change(unitId.ToString()));

        var saveButton = cut.FindAll("button")
            .First(b => b.TextContent.Equals("Opslaan", StringComparison.OrdinalIgnoreCase));
        await cut.InvokeAsync(() => saveButton.Click());

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Geselecteerd product:", cut.Markup);
            Assert.Contains("Nieuwe rijst", cut.Markup);
            Assert.Contains("Stuk", cut.Markup);
        });

        _productMock.Verify(s => s.CreateAsync("Nieuwe rijst", null, unitId, null, default), Times.Once);
    }

    private void SetupAuthState(bool owner)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, owner ? "Owner" : "Crew")
        };

        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        var authState = new AuthenticationState(principal);

        var authProviderMock = new Mock<AuthenticationStateProvider>();
        authProviderMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        Services.AddScoped<AuthenticationStateProvider>(_ => authProviderMock.Object);
    }
}
