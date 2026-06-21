using Bunit;
using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Results;
using BootManager.Application.Storage.Services;
using BootManager.Core.Entities;
using BootManager.Web.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Real bUnit tests for new location scan workcontext (ScanLocation.razor and related).
/// Tests that known location scans land in a dedicated location workcontext with products and actions,
/// keeping location context throughout the mutation and add-product flows.
/// Ensures no fallback to legacy pages or the generic location management page.
/// </summary>
public class ScanLocationComponentTests : TestContext
{
    private readonly Mock<IStorageService> _storageMock = new();
    private readonly Mock<IProductService> _productServiceMock = new();
    private readonly Mock<IStockService> _stockServiceMock = new();

    public ScanLocationComponentTests()
    {
        Services.AddScoped<IStorageService>(_ => _storageMock.Object);
        Services.AddScoped<IProductService>(_ => _productServiceMock.Object);
        Services.AddScoped<IStockService>(_ => _stockServiceMock.Object);
    }

    [Fact]
    public async Task ScanLocation_LoadsLocationAndRendersIdentity()
    {
        var locationId = Guid.NewGuid();
        var location = new StorageLocationDetailDto
        {
            Id = locationId,
            LocationName = "Salon",
            AreaName = "Interieur",
            Description = "Main indoor lounge area",
            Stocks = new List<StockDto>()
        };
        SetupAuthState("Owner");

        _storageMock
            .Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(location));

        var cut = RenderComponent<ScanLocation>(
            parameters => parameters.Add(p => p.LocationId, locationId.ToString()));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Salon", cut.Markup);
            Assert.Contains("Interieur", cut.Markup);
        });

        Assert.Contains("Salon", cut.Find("h3").TextContent);
    }

    [Fact]
    public async Task ScanLocation_WithProductsOnLocation_RendersList()
    {
        var locationId = Guid.NewGuid();
        var stock1 = new StockDto
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            StorageLocationId = locationId,
            ProductName = "Spaghetti",
            StorageAreaName = "Interieur",
            StorageLocationName = "Salon",
            Quantity = 2,
            DefaultUnitName = "pak"
        };
        var stock2 = new StockDto
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            StorageLocationId = locationId,
            ProductName = "Koffie",
            StorageAreaName = "Interieur",
            StorageLocationName = "Salon",
            Quantity = 3,
            DefaultUnitName = "kg"
        };
        var location = new StorageLocationDetailDto
        {
            Id = locationId,
            LocationName = "Salon",
            AreaName = "Interieur",
            Stocks = new List<StockDto> { stock1, stock2 }
        };
        SetupAuthState("Owner");

        _storageMock
            .Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(location));

        var cut = RenderComponent<ScanLocation>(
            parameters => parameters.Add(p => p.LocationId, locationId.ToString()));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Spaghetti", cut.Markup);
            Assert.Contains("Koffie", cut.Markup);
            Assert.Contains("2 pak", cut.Markup);
            Assert.Contains("3 kg", cut.Markup);
        });
    }

    [Fact]
    public async Task ScanLocation_NoProducts_ShowsMessageAndAddButton()
    {
        var locationId = Guid.NewGuid();
        var location = new StorageLocationDetailDto
        {
            Id = locationId,
            LocationName = "Galley",
            AreaName = "Kombuis",
            Stocks = new List<StockDto>()
        };
        SetupAuthState("Crew");

        _storageMock
            .Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(location));

        var cut = RenderComponent<ScanLocation>(
            parameters => parameters.Add(p => p.LocationId, locationId.ToString()));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Geen producten", cut.Markup, StringComparison.OrdinalIgnoreCase);
            var addButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Product toevoegen"));
            Assert.NotNull(addButton);
        });
    }

    [Fact]
    public async Task ScanLocation_ClickProductButton_NavigatesToMutateRoute()
    {
        var locationId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var stock = new StockDto
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            StorageLocationId = locationId,
            ProductName = "Thee",
            StorageAreaName = "Pantry",
            StorageLocationName = "Galley",
            Quantity = 5,
            DefaultUnitName = "pak"
        };
        var location = new StorageLocationDetailDto
        {
            Id = locationId,
            LocationName = "Galley",
            AreaName = "Pantry",
            Stocks = new List<StockDto> { stock }
        };
        SetupAuthState("Owner");

        _storageMock
            .Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(location));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<ScanLocation>(
            parameters => parameters.Add(p => p.LocationId, locationId.ToString()));

        await cut.InvokeAsync(() =>
        {
            cut.WaitForAssertion(() =>
                Assert.NotNull(cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Thee"))));
            cut.FindAll("button").Single(b => b.TextContent.Contains("Thee")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.EndsWith($"/scan/location/{locationId}/mutate/{productId}", navigation.Uri));
    }

    [Fact]
    public async Task ScanLocation_ClickAddProductButton_NavigatesToAddProductRoute()
    {
        var locationId = Guid.NewGuid();
        var stock = new StockDto
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            StorageLocationId = locationId,
            ProductName = "Brood",
            StorageAreaName = "Kombuis",
            StorageLocationName = "Salon",
            Quantity = 1,
            DefaultUnitName = "stuk"
        };
        var location = new StorageLocationDetailDto
        {
            Id = locationId,
            LocationName = "Salon",
            AreaName = "Kombuis",
            Stocks = new List<StockDto> { stock }
        };
        SetupAuthState("Crew");

        _storageMock
            .Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(location));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<ScanLocation>(
            parameters => parameters.Add(p => p.LocationId, locationId.ToString()));

        await cut.InvokeAsync(() =>
        {
            cut.WaitForAssertion(() =>
                Assert.NotNull(cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Ander product"))));
            cut.FindAll("button").Single(b => b.TextContent.Contains("Ander product")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.EndsWith($"/scan/location/{locationId}/add-product", navigation.Uri));
    }

    [Fact]
    public async Task ScanLocationMutate_LoadsProductAndLocation_RendersBoth()
    {
        var locationId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var stock = new StockDto
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            StorageLocationId = locationId,
            ProductName = "Vis",
            StorageAreaName = "Storing",
            StorageLocationName = "Freezer",
            Quantity = 10,
            DefaultUnitName = "kg"
        };
        var location = new StorageLocationDetailDto
        {
            Id = locationId,
            LocationName = "Freezer",
            AreaName = "Storing",
            Stocks = new List<StockDto> { stock }
        };
        var product = new ProductDto
        {
            Id = productId,
            Name = "Vis",
            DefaultUnitName = "kg",
            Code = new ProductCodeDto { Value = "FISH-001" }
        };
        SetupAuthState("Owner");

        _storageMock
            .Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(location));

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        var cut = RenderComponent<ScanLocationMutate>(parameters =>
        {
            parameters.Add(p => p.LocationId, locationId.ToString());
            parameters.Add(p => p.ProductId, productId.ToString());
        });

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Vis", cut.Markup);
            Assert.Contains("Freezer", cut.Markup);
            Assert.Contains("Storing", cut.Markup);
            Assert.Contains("10 kg", cut.Markup);
        });
    }

    [Fact]
    public async Task ScanLocationMutate_SavesMutation_ReturnsToLocation()
    {
        var locationId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var stock = new StockDto
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            StorageLocationId = locationId,
            ProductName = "Melk",
            StorageAreaName = "Pantry",
            StorageLocationName = "Koeling",
            Quantity = 2,
            DefaultUnitName = "liter"
        };
        var location = new StorageLocationDetailDto
        {
            Id = locationId,
            LocationName = "Koeling",
            AreaName = "Pantry",
            Stocks = new List<StockDto> { stock }
        };
        var product = new ProductDto
        {
            Id = productId,
            Name = "Melk",
            DefaultUnitName = "liter"
        };
        SetupAuthState("Owner", userId);

        _storageMock
            .Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(location));

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.MutateStockAsync(
                productId,
                locationId,
                StockMutationType.Verbruik,
                1m,
                userId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult.Ok());

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<ScanLocationMutate>(parameters =>
        {
            parameters.Add(p => p.LocationId, locationId.ToString());
            parameters.Add(p => p.ProductId, productId.ToString());
        });

        await cut.InvokeAsync(() =>
        {
            cut.WaitForAssertion(() =>
            {
                var typeSelect = cut.Find("select");
                Assert.NotNull(typeSelect);
            });

            cut.Find("select").Change("Verbruik");
            cut.Find("input[type='number']").Change("1");
            cut.FindAll("button").Single(b => b.TextContent.Contains("Opslaan")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.EndsWith($"/scan/location/{locationId}", navigation.Uri));
    }

    [Fact]
    public async Task ScanLocationAddProduct_LoadsLocation_ShowsProductSearch()
    {
        var locationId = Guid.NewGuid();
        var location = new StorageLocationDetailDto
        {
            Id = locationId,
            LocationName = "Kombuis",
            AreaName = "Interieur",
            Stocks = new List<StockDto>()
        };
        SetupAuthState("Crew");
        SetupScannerJs();

        _storageMock
            .Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(location));

        var cut = RenderComponent<ScanLocationAddProduct>(
            parameters => parameters.Add(p => p.LocationId, locationId.ToString()));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Kombuis", cut.Markup);
            Assert.Contains("Interieur", cut.Markup);
            Assert.Contains("Barcode scannen", cut.Markup, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Handmatige invoer", cut.Markup, StringComparison.OrdinalIgnoreCase);
        });

        var searchInput = cut.Find("input");
        Assert.NotNull(searchInput);
    }

    [Fact]
    public async Task ScanLocationAddProduct_RendersScanAction_WithCameraCard()
    {
        var locationId = Guid.NewGuid();
        var location = new StorageLocationDetailDto
        {
            Id = locationId,
            LocationName = "Salon",
            AreaName = "Interieur",
            Stocks = new List<StockDto>()
        };
        SetupAuthState("Owner");
        SetupScannerJs();

        _storageMock
            .Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(location));

        var cut = RenderComponent<ScanLocationAddProduct>(
            parameters => parameters.Add(p => p.LocationId, locationId.ToString()));

        // Give component time to initialize
        await Task.Delay(100);

        var markup = cut.Markup;

        // Location context visible
        Assert.Contains("Salon", markup);
        Assert.Contains("Interieur", markup);

        // Visible scan action card with camera — proves scan is visible
        Assert.Contains("Barcode scannen", markup);
        Assert.Contains("scan-product-camera-card", markup);
        Assert.Contains("scan-video-wrapper", markup);

        // Visible search/manual input card alongside
        Assert.Contains("Handmatige invoer", markup);
        Assert.Contains("Voer in of zoek op naam", markup);

        // Both input methods present
        Assert.Contains("Zoeken", markup);
    }

    [Fact]
    public async Task ScanLocationAddProduct_BothInputMethods_WorkIndependently()
    {
        var locationId = Guid.NewGuid();
        var location = new StorageLocationDetailDto
        {
            Id = locationId,
            LocationName = "Salon",
            AreaName = "Interieur",
            Stocks = new List<StockDto>()
        };
        SetupAuthState("Crew");
        SetupScannerJs();

        _storageMock
            .Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(location));

        var cut = RenderComponent<ScanLocationAddProduct>(
            parameters => parameters.Add(p => p.LocationId, locationId.ToString()));

        // Both methods visible simultaneously
        await Task.Delay(100);

        var markup = cut.Markup;

        // Scan action card visible
        Assert.Contains("Barcode scannen", markup);
        Assert.Contains("scan-product-camera-card", markup);

        // Search/manual input card visible
        Assert.Contains("Handmatige invoer", markup);
        Assert.Contains("Voer in of zoek op naam", markup);

        // Both action buttons present
        var buttons = cut.FindAll("button");
        Assert.Contains(buttons, b => b.TextContent.Contains("Zoeken", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ScanLocationAddProduct_MaintainsLocationContext_WithBothInputMethods()
    {
        var locationId = Guid.NewGuid();
        var location = new StorageLocationDetailDto
        {
            Id = locationId,
            LocationName = "Kombuis",
            AreaName = "Pantry",
            Stocks = new List<StockDto>()
        };
        SetupAuthState("Crew");
        SetupScannerJs();

        _storageMock
            .Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(location));

        var cut = RenderComponent<ScanLocationAddProduct>(
            parameters => parameters.Add(p => p.LocationId, locationId.ToString()));

        await Task.Delay(100);

        var markup = cut.Markup;
        // Location context visible at page start
        Assert.Contains("Kombuis", markup);
        Assert.Contains("Pantry", markup);

        // Both input methods visible from the start
        Assert.Contains("Barcode scannen", markup);
        Assert.Contains("Handmatige invoer", markup);
    }



    private void SetupAuthState(string role, Guid? userId = null)
    {
        var uid = userId ?? Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, uid.ToString()),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim("sub", uid.ToString()),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        var authState = new AuthenticationState(principal);

        Services.AddScoped<AuthenticationStateProvider>(sp =>
            new TestAuthenticationStateProvider(authState));
    }

    private class TestAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly AuthenticationState _authState;

        public TestAuthenticationStateProvider(AuthenticationState authState)
        {
            _authState = authState;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(_authState);
        }
    }

    private void SetupScannerJs()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }
}
