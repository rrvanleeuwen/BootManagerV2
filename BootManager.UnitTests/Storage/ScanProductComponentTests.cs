using Bunit;
using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Web.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Real bUnit tests for ScanProduct.razor product-scan workcontext.
/// Tests that the known-product route lands in the new product workcontext,
/// not in the legacy /scan/old experience.
/// </summary>
public class ScanProductComponentTests : TestContext
{
    private readonly Mock<IProductService> _productServiceMock = new();
    private readonly Mock<IStockService> _stockServiceMock = new();

    public ScanProductComponentTests()
    {
        Services.AddScoped<IProductService>(_ => _productServiceMock.Object);
        Services.AddScoped<IStockService>(_ => _stockServiceMock.Object);
    }

    [Fact]
    public async Task KnownProduct_WithActiveStock_RendersProductIdentityAndStockLocations()
    {
        var productId = Guid.NewGuid();
        var locationId1 = Guid.NewGuid();
        var locationId2 = Guid.NewGuid();
        SetupAuthState("Crew");

        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk",
            Code = new ProductCodeDto { Value = "PROD-12345" }
        };

        var stock1 = new StockDto
        {
            StorageLocationId = locationId1,
            StorageAreaName = "Kombuis",
            StorageLocationName = "Kastje",
            Quantity = 5,
            DefaultUnitName = "stuk"
        };

        var stock2 = new StockDto
        {
            StorageLocationId = locationId2,
            StorageAreaName = "Salon",
            StorageLocationName = "Regal",
            Quantity = 3,
            DefaultUnitName = "stuk"
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { stock1, stock2 }.AsReadOnly()));

        var cut = RenderComponent<ScanProduct>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Test Product", cut.Markup);
            Assert.Contains("PROD-12345", cut.Markup);
            Assert.Contains("Kombuis", cut.Markup);
            Assert.Contains("Kastje", cut.Markup);
            Assert.Contains("5 stuk", cut.Markup);
            Assert.Contains("Salon", cut.Markup);
            Assert.Contains("Regal", cut.Markup);
            Assert.Contains("3 stuk", cut.Markup);
        });
    }

    [Fact]
    public async Task KnownProduct_WithActiveStock_ShowsClickableLocationItems()
    {
        var productId = Guid.NewGuid();
        SetupAuthState("Owner");

        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        var stock = new StockDto
        {
            StorageLocationId = Guid.NewGuid(),
            StorageAreaName = "Kombuis",
            StorageLocationName = "Kastje",
            Quantity = 5,
            DefaultUnitName = "stuk"
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { stock }.AsReadOnly()));

        var cut = RenderComponent<ScanProduct>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        cut.WaitForAssertion(() =>
            Assert.NotEmpty(cut.FindAll(".stock-item-button")));
    }

    [Fact]
    public async Task KnownProduct_WithoutActiveStock_ShowsAddStockAction()
    {
        var productId = Guid.NewGuid();
        SetupAuthState("Owner");

        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        var cut = RenderComponent<ScanProduct>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Geen actieve voorraad", cut.Markup);
            Assert.NotNull(cut.FindAll("button").FirstOrDefault(b =>
                b.TextContent.Contains("Voorraad toevoegen")));
        });
    }

    [Fact]
    public async Task UnknownProductId_ShowsNotFoundMessage()
    {
        var unknownId = Guid.NewGuid();
        SetupAuthState("Crew");

        _productServiceMock
            .Setup(s => s.GetByIdAsync(unknownId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.NotFound());

        var cut = RenderComponent<ScanProduct>(parameters => parameters
            .Add(p => p.ProductId, unknownId.ToString()));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Product niet gevonden", cut.Markup);
            Assert.NotNull(cut.FindAll("button").FirstOrDefault(b =>
                b.TextContent.Contains("Terug naar scannen")));
        });
    }

    [Fact]
    public async Task InvalidProductId_ShowsNotFoundMessage()
    {
        SetupAuthState("Owner");

        var cut = RenderComponent<ScanProduct>(parameters => parameters
            .Add(p => p.ProductId, "invalid-guid"));

        cut.WaitForAssertion(() =>
            Assert.Contains("Product niet gevonden", cut.Markup));
    }

    [Fact]
    public async Task ScanProductPage_RendersMobileResponsiveLayout()
    {
        var productId = Guid.NewGuid();
        SetupAuthState("Owner");

        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        var cut = RenderComponent<ScanProduct>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        cut.WaitForAssertion(() =>
        {
            // Verify responsive container class exists
            Assert.NotNull(cut.Find(".scan-product-container"));
            // Verify card-based layout
            Assert.NotEmpty(cut.FindAll(".scan-product-card"));
        });
    }

    [Fact]
    public async Task ProductScanRoute_DoesNotUse_ScanOldAsEndExperience()
    {
        var productId = Guid.NewGuid();
        SetupAuthState("Owner");

        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk",
            Code = new ProductCodeDto { Value = "TEST-001" }
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        var cut = RenderComponent<ScanProduct>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        cut.WaitForAssertion(() =>
        {
            // Verify this is NOT the old scan page
            Assert.DoesNotContain("Scannen (oud)", cut.Markup);
            Assert.DoesNotContain("scan-video-wrapper", cut.Markup);
            // Verify this is the new product workcontext
            Assert.Contains("Productwerkcontext", cut.Markup);
            Assert.Contains("Test Product", cut.Markup);
        });
    }

    [Fact]
    public async Task LocationClick_WithStock_NavigatesToDirectMutateLocation()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        SetupAuthState("Owner");

        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        var stock = new StockDto
        {
            StorageLocationId = locationId,
            StorageAreaName = "Kombuis",
            StorageLocationName = "Kastje",
            Quantity = 5,
            DefaultUnitName = "stuk",
            ProductId = productId
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { stock }.AsReadOnly()));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<ScanProduct>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        await cut.InvokeAsync(() =>
        {
            var locationButtons = cut.FindAll(".stock-item-button");
            if (locationButtons.Count > 0)
                locationButtons[0].Click();
        });

        cut.WaitForAssertion(() =>
            Assert.EndsWith($"/scan/product/{productId}/mutate/{locationId}", navigation.Uri));
    }

    [Fact]
    public async Task AddStockSecondaryAction_WithStock_NavigatesToAddStockFlow()
    {
        var productId = Guid.NewGuid();
        SetupAuthState("Owner");

        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        var stock = new StockDto
        {
            StorageLocationId = Guid.NewGuid(),
            StorageAreaName = "Kombuis",
            StorageLocationName = "Kastje",
            Quantity = 5,
            DefaultUnitName = "stuk"
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { stock }.AsReadOnly()));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<ScanProduct>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        await cut.InvokeAsync(() =>
        {
            var button = cut.FindAll("button")
                .FirstOrDefault(b => b.TextContent.Contains("Voorraad op andere locatie toevoegen"));
            if (button != null)
                button.Click();
        });

        cut.WaitForAssertion(() =>
            Assert.EndsWith($"/scan/product/{productId}/add-stock", navigation.Uri));
    }

    [Fact]
    public async Task NoStock_PrimaryAction_ShowsAddStock()
    {
        var productId = Guid.NewGuid();
        SetupAuthState("Owner");

        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<ScanProduct>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        await cut.InvokeAsync(() =>
        {
            var button = cut.FindAll("button")
                .FirstOrDefault(b => b.TextContent.Contains("Voorraad toevoegen"));
            if (button != null)
                button.Click();
        });

        cut.WaitForAssertion(() =>
            Assert.EndsWith($"/scan/product/{productId}/add-stock", navigation.Uri));
    }

    private void SetupAuthState(string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user"),
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        var authState = new AuthenticationState(principal);

        var authStateMock = new Mock<AuthenticationStateProvider>();
        authStateMock
            .Setup(a => a.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        Services.AddScoped(_ => authStateMock.Object);
    }
}
