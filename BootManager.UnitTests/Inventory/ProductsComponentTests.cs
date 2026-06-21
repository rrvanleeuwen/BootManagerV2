using Bunit;
using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Services;
using BootManager.Core.Entities;
using BootManager.Web.Components.Pages.Inventory;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;
using System.Reflection;

namespace BootManager.UnitTests.Inventory;

/// <summary>
/// Real bUnit tests for Products.razor manual search and product finding flow.
/// Covers search functionality, direct navigation with one location, and multi-location display.
/// </summary>
public class ProductsComponentTests : TestContext
{
    private readonly Mock<IProductService> _productServiceMock = new();
    private readonly Mock<IProductCategoryService> _categoryServiceMock = new();
    private readonly Mock<IUnitService> _unitServiceMock = new();
    private readonly Mock<IStockService> _stockServiceMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();

    public ProductsComponentTests()
    {
        Services.AddScoped<IProductService>(_ => _productServiceMock.Object);
        Services.AddScoped<IProductCategoryService>(_ => _categoryServiceMock.Object);
        Services.AddScoped<IUnitService>(_ => _unitServiceMock.Object);
        Services.AddScoped<IStockService>(_ => _stockServiceMock.Object);
        Services.AddScoped<IStorageService>(_ => _storageServiceMock.Object);
        Services.AddLogging();
        SetupAuthState("Owner");
    }

    [Fact]
    public async Task ManualSearch_FindsProductByName_CaseInsensitive()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Appel",
            Description = "Rode appels",
            DefaultUnitName = "stuk"
        };

        _productServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>().AsReadOnly());
        _categoryServiceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductCategoryDto>().AsReadOnly());
        _unitServiceMock
            .Setup(s => s.InitializeDefaultUnitsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitServiceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UnitDto>().AsReadOnly());
        _categoryServiceMock
            .Setup(s => s.GetValidIconKeys())
            .Returns(new List<string>());

        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync("appel", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { product }.AsReadOnly());

        var cut = RenderComponent<Products>();

        // Act: Open search and search for product
        await cut.InvokeAsync(() =>
        {
            var searchButton = cut.FindAll("button").First(b => b.TextContent.Contains("Zoeken"));
            searchButton.Click();
        });

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.Find("input[placeholder='Zoeken op naam of omschrijving…']")));

        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input[placeholder='Zoeken op naam of omschrijving…']");
            searchInput.Input("appel");
            var searchBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Zoeken") && b != cut.FindAll("button").Last());
            searchBtn.Click();
        });

        // Assert
        cut.WaitForAssertion(() =>
            Assert.Contains("Appel", cut.Markup));
    }

    [Fact]
    public async Task ManualSearch_WithOneActiveLocation_NavigatesDirectlyToLocation()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Appel",
            Description = "Rode appels",
            DefaultUnitName = "stuk"
        };

        var activeLocation = new StockDto
        {
            StorageLocationId = locationId,
            StorageAreaName = "Kombuis",
            StorageLocationName = "Kast",
            Quantity = 5,
            DefaultUnitName = "stuk"
        };

        _productServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>().AsReadOnly());
        _categoryServiceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductCategoryDto>().AsReadOnly());
        _unitServiceMock
            .Setup(s => s.InitializeDefaultUnitsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitServiceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UnitDto>().AsReadOnly());
        _categoryServiceMock
            .Setup(s => s.GetValidIconKeys())
            .Returns(new List<string>());

        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync("appel", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { product }.AsReadOnly());

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { activeLocation }.AsReadOnly()));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<Products>();

        // Act: Open search, search for product, and click on result
        await cut.InvokeAsync(() =>
        {
            var searchButton = cut.FindAll("button").First(b => b.TextContent.Contains("Zoeken"));
            searchButton.Click();
        });

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.Find("input[placeholder='Zoeken op naam of omschrijving…']")));

        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input[placeholder='Zoeken op naam of omschrijving…']");
            searchInput.Input("appel");
            var searchBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Zoeken"));
            searchBtn.Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Appel", cut.Markup));

        await cut.InvokeAsync(() =>
        {
            var productItem = cut.FindAll("button").First(b => b.TextContent.Contains("Appel"));
            productItem.Click();
        });

        // Assert: Navigation should go directly to location
        cut.WaitForAssertion(() =>
            Assert.EndsWith($"/storage/locations/{locationId}", navigation.Uri));
    }

    [Fact]
    public async Task ManualSearch_WithMultipleActiveLocations_ShowsLocationListWithoutNavigating()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Appel",
            Description = "Rode appels",
            DefaultUnitName = "stuk"
        };

        var location1 = new StockDto
        {
            StorageLocationId = Guid.NewGuid(),
            StorageAreaName = "Kombuis",
            StorageLocationName = "Kast",
            Quantity = 5,
            DefaultUnitName = "stuk"
        };

        var location2 = new StockDto
        {
            StorageLocationId = Guid.NewGuid(),
            StorageAreaName = "Pantry",
            StorageLocationName = "Plank",
            Quantity = 3,
            DefaultUnitName = "stuk"
        };

        _productServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>().AsReadOnly());
        _categoryServiceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductCategoryDto>().AsReadOnly());
        _unitServiceMock
            .Setup(s => s.InitializeDefaultUnitsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitServiceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UnitDto>().AsReadOnly());
        _categoryServiceMock
            .Setup(s => s.GetValidIconKeys())
            .Returns(new List<string>());

        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync("appel", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { product }.AsReadOnly());

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { location1, location2 }.AsReadOnly()));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var initialUri = navigation.Uri;
        var cut = RenderComponent<Products>();

        // Act: Search and click product
        await cut.InvokeAsync(() =>
        {
            var searchButton = cut.FindAll("button").First(b => b.TextContent.Contains("Zoeken"));
            searchButton.Click();
        });

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.Find("input[placeholder='Zoeken op naam of omschrijving…']")));

        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input[placeholder='Zoeken op naam of omschrijving…']");
            searchInput.Input("appel");
            var searchBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Zoeken"));
            searchBtn.Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Appel", cut.Markup));

        await cut.InvokeAsync(() =>
        {
            var productItem = cut.FindAll("button").First(b => b.TextContent.Contains("Appel"));
            productItem.Click();
        });

        // Assert: Should show location list, not navigate
        cut.WaitForAssertion(() =>
            Assert.Contains("Product gevonden op meerdere locaties", cut.Markup, StringComparison.OrdinalIgnoreCase));
        Assert.Contains("Kombuis - Kast", cut.Markup);
        Assert.Contains("Pantry - Plank", cut.Markup);
        Assert.Equal(initialUri, navigation.Uri);
    }

    [Fact]
    public async Task ManualSearch_WithNoActiveStock_ShowsNoActiveStockMessage()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var expectedLocationId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Appel",
            Description = "Rode appels",
            DefaultUnitName = "stuk"
        };

        var expectedLocation = new StockDto
        {
            StorageLocationId = expectedLocationId,
            StorageAreaName = "Kombuis",
            StorageLocationName = "Kast"
        };

        _productServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>().AsReadOnly());
        _categoryServiceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductCategoryDto>().AsReadOnly());
        _unitServiceMock
            .Setup(s => s.InitializeDefaultUnitsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitServiceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UnitDto>().AsReadOnly());
        _categoryServiceMock
            .Setup(s => s.GetValidIconKeys())
            .Returns(new List<string>());

        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync("appel", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { product }.AsReadOnly());

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto>().AsReadOnly()));

        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(expectedLocation));

        var cut = RenderComponent<Products>();

        // Act: Search and click product
        await cut.InvokeAsync(() =>
        {
            var searchButton = cut.FindAll("button").First(b => b.TextContent.Contains("Zoeken"));
            searchButton.Click();
        });

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.Find("input[placeholder='Zoeken op naam of omschrijving…']")));

        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input[placeholder='Zoeken op naam of omschrijving…']");
            searchInput.Input("appel");
            var searchBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Zoeken"));
            searchBtn.Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Appel", cut.Markup));

        await cut.InvokeAsync(() =>
        {
            var productItem = cut.FindAll("button").First(b => b.TextContent.Contains("Appel"));
            productItem.Click();
        });

        // Assert: Should show no active stock message with expected location
        cut.WaitForAssertion(() =>
            Assert.Contains("Geen actieve voorraad", cut.Markup, StringComparison.OrdinalIgnoreCase));
        Assert.Contains("Kombuis - Kast", cut.Markup);
        Assert.NotNull(cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Voorraad toevoegen")));
    }

    [Fact]
    public async Task ManualSearch_WithNoActiveStock_OpensAddStockModal()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Appel",
            Description = "Rode appels",
            DefaultUnitName = "stuk"
        };

        _productServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>().AsReadOnly());
        _categoryServiceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductCategoryDto>().AsReadOnly());
        _unitServiceMock
            .Setup(s => s.InitializeDefaultUnitsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitServiceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UnitDto>().AsReadOnly());
        _categoryServiceMock
            .Setup(s => s.GetValidIconKeys())
            .Returns(new List<string>());

        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync("appel", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { product }.AsReadOnly());

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto>().AsReadOnly()));

        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.NotFound());

        var mockLocations = new List<StorageLocationOverviewDto>
        {
            new() { Id = Guid.NewGuid(), AreaName = "Kombuis", LocationName = "Kast" }
        };

        _storageServiceMock
            .Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockLocations);

        var cut = RenderComponent<Products>();

        // Act: Search and click product
        await cut.InvokeAsync(() =>
        {
            var searchButton = cut.FindAll("button").First(b => b.TextContent.Contains("Zoeken"));
            searchButton.Click();
        });

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.Find("input[placeholder='Zoeken op naam of omschrijving…']")));

        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input[placeholder='Zoeken op naam of omschrijving…']");
            searchInput.Input("appel");
            var searchBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Zoeken"));
            searchBtn.Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Appel", cut.Markup));

        await cut.InvokeAsync(() =>
        {
            var productItem = cut.FindAll("button").First(b => b.TextContent.Contains("Appel"));
            productItem.Click();
        });

        // Assert: Should show no active stock message
        cut.WaitForAssertion(() =>
            Assert.Contains("Geen actieve voorraad", cut.Markup, StringComparison.OrdinalIgnoreCase));

        // Click "Voorraad toevoegen" button
        await cut.InvokeAsync(() =>
        {
            var addStockButton = cut.FindAll("button").First(b => b.TextContent.Contains("Voorraad toevoegen"));
            addStockButton.Click();
        });

        // Assert: Modal should appear
        cut.WaitForAssertion(() =>
        {
            var modalHeaders = cut.FindAll("h5");
            Assert.NotEmpty(modalHeaders.Where(h => h.TextContent.Contains("Voorraad toevoegen")));
        });

        // Verify the modal shows the product
        Assert.Contains("Appel", cut.Markup);

        // Verify location selection dropdown exists in modal
        var selects = cut.FindAll("select");
        Assert.NotEmpty(selects);
    }

    [Fact]
    public async Task AdministrativeMutationFallback_ModalCanBeOpened()
    {
        // Arrange
        SetupAuthState("Owner");

        _productServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>().AsReadOnly());
        _categoryServiceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductCategoryDto>().AsReadOnly());
        _unitServiceMock
            .Setup(s => s.InitializeDefaultUnitsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitServiceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UnitDto>().AsReadOnly());
        _categoryServiceMock
            .Setup(s => s.GetValidIconKeys())
            .Returns(new List<string>());

        var cut = RenderComponent<Products>();

        // Act: Click the fallback mutation button
        await cut.InvokeAsync(() =>
        {
            var mutationBtn = cut.FindAll("button").First(b => b.TextContent.Contains("Voorraadbijzonderheid"));
            mutationBtn.Click();
        });

        // Assert: Modal opens
        cut.WaitForAssertion(() =>
            Assert.Contains("Voorraadbijzonderheid vastleggen", cut.Markup));
    }

    [Fact]
    public async Task AdministrativeMutationFallback_CallsMutateStockAsync_WhenSaved()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Appel",
            Description = "Groene appel",
            DefaultUnitName = "stuk"
        };
        var stock = new StockDto
        {
            ProductId = productId,
            StorageLocationId = locationId,
            ProductName = "Appel",
            StorageAreaName = "Kombuis",
            StorageLocationName = "Voorraadbak",
            Quantity = 5,
            DefaultUnitName = "stuk"
        };

        SetupAuthState("Owner", userId);

        _productServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>().AsReadOnly());
        _categoryServiceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductCategoryDto>().AsReadOnly());
        _unitServiceMock
            .Setup(s => s.InitializeDefaultUnitsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitServiceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UnitDto>().AsReadOnly());
        _categoryServiceMock
            .Setup(s => s.GetValidIconKeys())
            .Returns(new List<string>());
        _productServiceMock
            .Setup(s => s.SearchByNameOrDescriptionAsync("Appel", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { product }.AsReadOnly());
        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto> { stock }.AsReadOnly()));

        _stockServiceMock
            .Setup(s => s.MutateStockAsync(
                productId,
                locationId,
                StockMutationType.Verbruik,
                2m,
                userId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult.Ok());

        var cut = RenderComponent<Products>();

        await cut.InvokeAsync(() =>
        {
            cut.FindAll("button")
                .First(b => b.TextContent.Contains("Voorraadbijzonderheid"))
                .Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Voorraadbijzonderheid vastleggen", cut.Markup));

        var componentType = typeof(Products);
        componentType
            .GetField("_searchTerm", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(cut.Instance, "Appel");

        var performFallbackSearch = componentType.GetMethod(
            "PerformMutationFallbackSearch",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var selectFallbackProduct = componentType.GetMethod(
            "SelectProductForMutationFallback",
            BindingFlags.Instance | BindingFlags.NonPublic);

        await cut.InvokeAsync(async () =>
        {
            await (Task)performFallbackSearch!.Invoke(cut.Instance, null)!;
            await (Task)selectFallbackProduct!.Invoke(cut.Instance, new object[] { product })!;
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Appel", cut.Markup));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Locatie:", cut.Markup);
            Assert.Contains("Voorraadbak", cut.Markup);
        });

        componentType
            .GetField("_fallbackMutationType", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(cut.Instance, "Verbruik");
        componentType
            .GetField("_fallbackQuantity", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(cut.Instance, 2m);

        var saveFallbackMutation = componentType.GetMethod(
            "SaveMutationFallback",
            BindingFlags.Instance | BindingFlags.NonPublic);

        await cut.InvokeAsync(async () =>
        {
            await (Task)saveFallbackMutation!.Invoke(cut.Instance, null)!;
        });

        _stockServiceMock.Verify(
            s => s.MutateStockAsync(
                productId,
                locationId,
                StockMutationType.Verbruik,
                2m,
                userId,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
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
