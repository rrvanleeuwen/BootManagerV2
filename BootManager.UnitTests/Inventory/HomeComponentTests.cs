using Bunit;
using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Application.Storage.Services;
using BootManager.Core.Entities;
using BootManager.Web.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace BootManager.UnitTests.Inventory;

/// <summary>
/// Real bUnit tests for Home.razor component.
/// Covers primary navigation tiles, product search, search results display,
/// and result interaction flows (direct navigation, location choice, add stock).
/// </summary>
public class HomeComponentTests : TestContext
{
    private readonly Mock<IProductService> _productServiceMock = new();
    private readonly Mock<IStockService> _stockServiceMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();

    public HomeComponentTests()
    {
        Services.AddScoped<IProductService>(_ => _productServiceMock.Object);
        Services.AddScoped<IStockService>(_ => _stockServiceMock.Object);
        Services.AddScoped<IStorageService>(_ => _storageServiceMock.Object);
        Services.AddLogging();
        SetupAuthState("Owner");
    }

    [Fact]
    public void Home_RendersPrimaryTiles()
    {
        // Arrange: Setup mocks with empty data
        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>().AsReadOnly());

        // Act: Render the Home component
        var cut = RenderComponent<Home>();

        // Assert: Verify all three primary tiles are rendered
        var markup = cut.Markup;
        Assert.Contains("Scannen", markup);
        Assert.Contains("Dashboard", markup);
        Assert.Contains("Logboek", markup);

        // Verify tile links point to correct routes
        var links = cut.FindAll("a");
        Assert.Contains(links, l => l.GetAttribute("href")?.Contains("/scan") ?? false);
        Assert.Contains(links, l => l.GetAttribute("href")?.Contains("/dashboard") ?? false);
        Assert.Contains(links, l => l.GetAttribute("href")?.Contains("/logbook") ?? false);
    }

    [Fact]
    public async Task Home_SearchInput_IsPresent()
    {
        // Arrange
        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>().AsReadOnly());

        // Act
        var cut = RenderComponent<Home>();

        // Assert: Search input exists with correct placeholder
        cut.WaitForAssertion(() =>
        {
            var searchInput = cut.Find("input.search-input");
            Assert.NotNull(searchInput);
            Assert.Equal("Zoeken op product naam of omschrijving…", searchInput.GetAttribute("placeholder"));
        });
    }

    [Fact]
    public async Task Home_Search_DisplaysResultsWithProductDetails()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Reddingsvest Pro 150N",
            Description = "Professional life jacket",
            DefaultUnitName = "stuk"
        };

        var stock = new StockDto
        {
            StorageLocationId = locationId,
            StorageAreaName = "Magazijn A",
            StorageLocationName = "Schap A-24",
            Quantity = 12,
            DefaultUnitName = "stuk"
        };

        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync("reddingsvest", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { product }.AsReadOnly());

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { stock }.AsReadOnly()));

        var cut = RenderComponent<Home>();

        // Act: Enter search term via input binding and trigger search via Enter key
        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.Input("reddingsvest");
        });

        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.KeyDown("Enter");
        });

        // Assert: Result displays product name, quantity, unit, and locations
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Reddingsvest Pro 150N", cut.Markup);
            Assert.Contains("12", cut.Markup);
            Assert.Contains("stuk", cut.Markup);
            Assert.Contains("Magazijn A - Schap A-24", cut.Markup);
        });
    }

    [Fact]
    public async Task Home_ClickProduct_OpensProductDetails()
    {
        // Arrange: Product with one active location
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Ankerketting 8mm",
            DefaultUnitName = "m"
        };

        var stock = new StockDto
        {
            StorageLocationId = locationId,
            StorageAreaName = "Magazijn B",
            StorageLocationName = "Plank B-1",
            Quantity = 45,
            DefaultUnitName = "m"
        };

        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync("ankerketting", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { product }.AsReadOnly());

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { stock }.AsReadOnly()));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<Home>();

        // Act: Search and click product
        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.Input("ankerketting");
        });

        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.KeyDown("Enter");
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Ankerketting 8mm", cut.Markup));

        // Click on the result
        await cut.InvokeAsync(() =>
        {
            var resultItem = cut.Find(".result-item");
            resultItem.Click();
        });

        // Assert: Product details modal opens with product name and location info
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Ankerketting 8mm", cut.Markup);
            Assert.Contains("Magazijn B - Plank B-1", cut.Markup);
            Assert.Contains("45", cut.Markup);
            Assert.Contains("Verbruik registreren", cut.Markup);
        });
    }

    [Fact]
    public async Task Home_ClickProductWithMultipleLocations_ShowsProductDetailsWithAllLocations()
    {
        // Arrange: Product at multiple locations
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "VHF Radio HX300",
            DefaultUnitName = "stuk"
        };

        var location1 = new StockDto
        {
            StorageLocationId = Guid.NewGuid(),
            StorageAreaName = "Kantoor",
            StorageLocationName = "Voorraad",
            Quantity = 2,
            DefaultUnitName = "stuk"
        };

        var location2 = new StockDto
        {
            StorageLocationId = Guid.NewGuid(),
            StorageAreaName = "Magazijn C",
            StorageLocationName = "Schap C-5",
            Quantity = 3,
            DefaultUnitName = "stuk"
        };

        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync("vhf radio", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { product }.AsReadOnly());

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { location1, location2 }.AsReadOnly()));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var initialUri = navigation.Uri;
        var cut = RenderComponent<Home>();

        // Act: Search and click product
        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.Input("vhf radio");
        });

        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.KeyDown("Enter");
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("VHF Radio HX300", cut.Markup));

        await cut.InvokeAsync(() =>
        {
            var resultItem = cut.Find(".result-item");
            resultItem.Click();
        });

        // Assert: Product details modal shows all locations and total quantity, no navigation
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("VHF Radio HX300", cut.Markup);
            Assert.Contains("Kantoor - Voorraad", cut.Markup);
            Assert.Contains("Magazijn C - Schap C-5", cut.Markup);
            Assert.Contains("5", cut.Markup); // Total quantity: 2+3
            Assert.Equal(initialUri, navigation.Uri);
        });
    }

    [Fact]
    public async Task Home_ClickProductWithNoActiveStock_ShowsProductDetailsWithNoActiveStockMessage()
    {
        // Arrange: Product with no active stock
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Hydraulische Olie VG46",
            DefaultUnitName = "L"
        };

        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync("hydraulische", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { product }.AsReadOnly());

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto>().AsReadOnly()));

        var cut = RenderComponent<Home>();

        // Act: Search and click product
        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.Input("hydraulische");
        });

        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.KeyDown("Enter");
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Hydraulische Olie VG46", cut.Markup));

        await cut.InvokeAsync(() =>
        {
            var resultItem = cut.Find(".result-item");
            resultItem.Click();
        });

        // Assert: Product details modal shows with no active stock message
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Hydraulische Olie VG46", cut.Markup);
            Assert.Contains("Geen actieve voorraad", cut.Markup, StringComparison.OrdinalIgnoreCase);
            var consumeBtn = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Verbruik registreren"));
            Assert.NotNull(consumeBtn);
        });
    }

    [Fact]
    public async Task Home_ClickProductAndNavigateToConsumption_ShowsConsumptionAction()
    {
        // Arrange: Product that user can navigate to consumption registration
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Scheepsverf Wit",
            DefaultUnitName = "L"
        };

        var stock = new StockDto
        {
            StorageLocationId = Guid.NewGuid(),
            StorageAreaName = "Magazijn",
            StorageLocationName = "Opslagruimte",
            Quantity = 50,
            DefaultUnitName = "L"
        };

        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync("verf", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { product }.AsReadOnly());

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { stock }.AsReadOnly()));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<Home>();

        // Act: Search and click product
        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.Input("verf");
        });

        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.KeyDown("Enter");
        });

        cut.WaitForAssertion(() => Assert.Contains("Scheepsverf Wit", cut.Markup));

        await cut.InvokeAsync(() =>
        {
            var resultItem = cut.Find(".result-item");
            resultItem.Click();
        });

        // Assert: Product details modal shows with "Verbruik registreren" button
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Scheepsverf Wit", cut.Markup);
            Assert.Contains("50", cut.Markup);
            var consumeBtn = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Verbruik registreren"));
            Assert.NotNull(consumeBtn);
        });
    }

    [Fact]
    public async Task Home_ClickConsumptionAction_NavigatesToMutationsWithProductIdParameter()
    {
        // Arrange: Product with active stock and consumption action
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Scheepsverf Blauw",
            DefaultUnitName = "L"
        };

        var stock = new StockDto
        {
            StorageLocationId = Guid.NewGuid(),
            StorageAreaName = "Magazijn",
            StorageLocationName = "Opslagruimte",
            Quantity = 30,
            DefaultUnitName = "L"
        };

        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync("blauw", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { product }.AsReadOnly());

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { stock }.AsReadOnly()));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<Home>();

        // Act: Search and click product
        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.Input("blauw");
        });

        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.KeyDown("Enter");
        });

        cut.WaitForAssertion(() => Assert.Contains("Scheepsverf Blauw", cut.Markup));

        await cut.InvokeAsync(() =>
        {
            var resultItem = cut.Find(".result-item");
            resultItem.Click();
        });

        // Assert: Product details modal opens
        cut.WaitForAssertion(() => Assert.Contains("Scheepsverf Blauw", cut.Markup));

        // Act: Click "Verbruik registreren" button
        await cut.InvokeAsync(() =>
        {
            var consumeBtn = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Verbruik registreren"));
            consumeBtn?.Click();
        });

        // Assert: Navigation should include productId query parameter
        cut.WaitForAssertion(() =>
            Assert.Contains($"productId={productId}", navigation.Uri));
    }

    [Fact]
    public async Task Home_ClickConsumptionAction_NavigatesDirectlyToCotnsumptionFlow()
    {
        // Arrange: Product with active stock - verify that clicking consumption doesn't show product selection
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Scheepsverf Rood",
            DefaultUnitName = "L"
        };

        var stock = new StockDto
        {
            StorageLocationId = Guid.NewGuid(),
            StorageAreaName = "Magazijn",
            StorageLocationName = "Opslagruimte",
            Quantity = 20,
            DefaultUnitName = "L"
        };

        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync("rood", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { product }.AsReadOnly());

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { stock }.AsReadOnly()));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<Home>();

        // Act: Search and click product
        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.Input("rood");
        });

        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.KeyDown("Enter");
        });

        cut.WaitForAssertion(() => Assert.Contains("Scheepsverf Rood", cut.Markup));

        await cut.InvokeAsync(() =>
        {
            var resultItem = cut.Find(".result-item");
            resultItem.Click();
        });

        cut.WaitForAssertion(() => Assert.Contains("Scheepsverf Rood", cut.Markup));

        // Act: Click consumption button - should not redirect to product list but to preselected mutation flow
        var initialUri = navigation.Uri;
        await cut.InvokeAsync(() =>
        {
            var consumeBtn = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Verbruik registreren"));
            consumeBtn?.Click();
        });

        // Assert: Navigation goes to mutations with product parameter (not to /inventory/products)
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("/inventory/mutations-admin", navigation.Uri);
            Assert.Contains($"productId={productId}", navigation.Uri);
        });
    }

    [Fact]
    public async Task Home_Pagination_LimitsResultsTo10PerPage()
    {
        // Arrange: Create 25 products to test pagination
        var productIds = Enumerable.Range(1, 25).Select(_ => Guid.NewGuid()).ToList();
        var products = Enumerable.Range(1, 25)
            .Select(i => new ProductDto
            {
                Id = productIds[i - 1],
                Name = $"Product {i:D2}",
                DefaultUnitName = "stuk"
            })
            .ToList();

        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.AsReadOnly());

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto>().AsReadOnly()));

        var cut = RenderComponent<Home>();

        // Act: Search via UI
        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.Input("product");
        });

        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.KeyDown("Enter");
        });

        // Assert: First page shows max 10 results
        cut.WaitForAssertion(() =>
        {
            var resultItems = cut.FindAll(".result-item");
            Assert.Equal(10, resultItems.Count);
            Assert.Contains("Product 01", cut.Markup);
            Assert.Contains("Product 10", cut.Markup);
            Assert.DoesNotContain("Product 11", cut.Markup);
        }, TimeSpan.FromSeconds(5));

        // Act: Go to next page
        await cut.InvokeAsync(() =>
        {
            var nextBtn = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Volgende"));
            nextBtn?.Click();
        });

        // Assert: Second page shows products 11-20
        cut.WaitForAssertion(() =>
        {
            var resultItems = cut.FindAll(".result-item");
            Assert.Equal(10, resultItems.Count);
            Assert.Contains("Product 11", cut.Markup);
            Assert.Contains("Product 20", cut.Markup);
            Assert.DoesNotContain("Product 01", cut.Markup);
        }, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Home_SearchResult_DisplaysProductWithMultipleLocations()
    {
        // Arrange: Product at multiple locations
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Reddingsvest Type II",
            DefaultUnitName = "stuk"
        };

        var locations = new List<StockDto>
        {
            new() { StorageAreaName = "Magazijn A", StorageLocationName = "Skap 1", Quantity = 5, DefaultUnitName = "stuk" },
            new() { StorageAreaName = "Magazijn B", StorageLocationName = "Skap 2", Quantity = 3, DefaultUnitName = "stuk" },
            new() { StorageAreaName = "Magazijn C", StorageLocationName = "Skap 3", Quantity = 2, DefaultUnitName = "stuk" }
        };

        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync("reddingsvest", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { product }.AsReadOnly());

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(locations.AsReadOnly()));

        var cut = RenderComponent<Home>();

        // Act: Search via UI
        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.Input("reddingsvest");
        });

        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input.search-input");
            searchInput.KeyDown("Enter");
        });

        // Assert: All locations visible in result, total quantity calculated
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Reddingsvest Type II", cut.Markup);
            Assert.Contains("10", cut.Markup); // Total quantity: 5+3+2
            Assert.Contains("Magazijn A - Skap 1", cut.Markup);
            Assert.Contains("Magazijn B - Skap 2", cut.Markup);
            Assert.Contains("Magazijn C - Skap 3", cut.Markup);
        });
    }

    private void SetupAuthState(string role, Guid? userId = null)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.Role, role) };
        if (userId.HasValue)
        {
            claims.Add(new Claim("sub", userId.Value.ToString()));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        }
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        var authStateMock = new Mock<AuthenticationStateProvider>();
        authStateMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(principal));
        Services.AddScoped(_ => authStateMock.Object);
    }
}
