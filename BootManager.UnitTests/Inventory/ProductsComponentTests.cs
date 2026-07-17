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
/// Real bUnit tests for the Products.razor overview (PILOT-PERF-01). The overview is built
/// from the dedicated <see cref="IProductOverviewReadQuery"/> paged reader: the component
/// requests the correct search/archive/page arguments, renders the returned page content
/// (name/total/unit/locations + no-stock), returns to page 1 on search and requests the
/// correct server-side page on previous/next. The preserved primary-click finding flow,
/// detail popup, edit deep-link navigation and product management still use IStockService
/// and IProductService.
/// </summary>
public class ProductsComponentTests : TestContext
{
    private readonly Mock<IProductService> _productServiceMock = new();
    private readonly Mock<IProductOverviewReadQuery> _overviewReadQueryMock = new();
    private readonly Mock<IProductCategoryService> _categoryServiceMock = new();
    private readonly Mock<IUnitService> _unitServiceMock = new();
    private readonly Mock<IStockService> _stockServiceMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();

    public ProductsComponentTests()
    {
        Services.AddScoped<IProductService>(_ => _productServiceMock.Object);
        Services.AddScoped<IProductOverviewReadQuery>(_ => _overviewReadQueryMock.Object);
        Services.AddScoped<IProductCategoryService>(_ => _categoryServiceMock.Object);
        Services.AddScoped<IUnitService>(_ => _unitServiceMock.Object);
        Services.AddScoped<IStockService>(_ => _stockServiceMock.Object);
        Services.AddScoped<IStorageService>(_ => _storageServiceMock.Object);
        Services.AddLogging();
        SetupAuthState("Owner");

        // Veilige standaardwaarden zodat de finding-flow nooit op een niet-geconfigureerde
        // mock crasht. Specifieke tests overschrijven deze.
        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));
        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.NotFound());
    }

    [Fact]
    public void Overview_InitialPageWith11Products_RendersOnly10_AndRequestsSecondServerSidePage()
    {
        // Arrange: de reader levert pagina 1 (10 producten) en pagina 2 (het elfde),
        // met een totaal van 11 matches.
        var products = Enumerable.Range(1, 11)
            .Select(i => new ProductDto { Id = Guid.NewGuid(), Name = $"Product {i:00}", DefaultUnitName = "stuk" })
            .ToList();
        var location = Location("Kombuis", "Kast", 5);

        SetupBaseMocks();
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync(null, false, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(11, products.Take(10).Select(p => Item(p, location)).ToArray()));
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync(null, false, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(11, Item(products[10], location)));

        // Act
        var cut = RenderComponent<Products>();

        // Assert: exact 10 resultaten op pagina 1, met naam/totaal/eenheid/locatie-inhoud.
        cut.WaitForAssertion(() => Assert.Equal(10, cut.FindAll(".product-result").Count));
        Assert.Contains("Product 01", cut.Markup);
        Assert.DoesNotContain("Product 11", cut.Markup);

        var firstCard = cut.FindAll(".product-result").First().TextContent;
        Assert.Contains("Product 01", firstCard);
        Assert.Contains("5", firstCard);
        Assert.Contains("stuk", firstCard);
        Assert.Contains("Kombuis - Kast", firstCard);

        Assert.Contains("Pagina 1 van 2", cut.Markup);

        // Act: naar de volgende pagina met het pagineringscontrol.
        cut.InvokeAsync(() =>
            cut.FindAll("button").First(b => b.TextContent.Contains("Volgende")).Click());

        // Assert: het elfde resultaat staat op pagina 2 en de reader is server-side voor
        // pagina 2 aangeroepen.
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Product 11", cut.Markup);
            Assert.Single(cut.FindAll(".product-result"));
            Assert.Contains("Pagina 2 van 2", cut.Markup);
        });
        _overviewReadQueryMock.Verify(
            q => q.GetPageAsync(null, false, 2, 10, It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Overview_Search_ReturnsToFirstPage_AndRequestsServerSideSearchPage()
    {
        // Arrange: catalogus met paginering (11) en een zoekopdracht die 12 producten geeft.
        var catalogue = Enumerable.Range(1, 11)
            .Select(i => new ProductDto { Id = Guid.NewGuid(), Name = $"Cat {i:00}", DefaultUnitName = "stuk" })
            .ToList();
        var searchResults = Enumerable.Range(1, 12)
            .Select(i => new ProductDto { Id = Guid.NewGuid(), Name = $"Zoek {i:00}", DefaultUnitName = "stuk" })
            .ToList();
        var location = Location("Kombuis", "Kast", 5);

        SetupBaseMocks();
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync(null, false, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(11, catalogue.Take(10).Select(p => Item(p, location)).ToArray()));
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync(null, false, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(11, Item(catalogue[10], location)));
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync("zoek", false, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(12, searchResults.Take(10).Select(p => Item(p, location)).ToArray()));

        var cut = RenderComponent<Products>();

        // Ga eerst naar pagina 2 van de catalogus.
        await cut.InvokeAsync(() =>
            cut.FindAll("button").First(b => b.TextContent.Contains("Volgende")).Click());
        cut.WaitForAssertion(() => Assert.Contains("Pagina 2 van 2", cut.Markup));

        // Act: zoek.
        await SearchAsync(cut, "zoek");

        // Assert: zoekresultaten beginnen op pagina 1, met de door de reader geleverde inhoud.
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Zoek 01", cut.Markup);
            Assert.Contains("Pagina 1 van 2", cut.Markup);
        });
        Assert.DoesNotContain("Zoek 12", cut.Markup);

        var firstCard = cut.FindAll(".product-result").First().TextContent;
        Assert.Contains("5", firstCard);
        Assert.Contains("stuk", firstCard);
        Assert.Contains("Kombuis - Kast", firstCard);

        // Assert: de reader is met zoekterm, actieve stand en pagina 1 aangeroepen.
        _overviewReadQueryMock.Verify(
            q => q.GetPageAsync("zoek", false, 1, 10, It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void Overview_ProductWithoutActiveStock_RendersNoStockState()
    {
        // Arrange: één actief product zonder actieve voorraad (reader levert lege locaties).
        var product = new ProductDto { Id = Guid.NewGuid(), Name = "Appel", DefaultUnitName = "stuk" };

        SetupBaseMocks();
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync(null, false, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, Item(product)));

        // Act
        var cut = RenderComponent<Products>();

        // Assert: ook zonder actieve voorraad toont het resultaat de totale hoeveelheid 0
        // en de standaardeenheid, met de bewuste no-stockstatus en zonder locatiechips.
        cut.WaitForAssertion(() =>
        {
            var stockValue = cut.Find(".product-result .stock-value").TextContent.Trim();
            Assert.Equal("0", stockValue);
        });
        var card = cut.Find(".product-result").TextContent;
        Assert.Contains("stuk", card);
        Assert.Contains("Geen actieve voorraad", card);
        Assert.Empty(cut.FindAll(".location-chip"));
    }

    [Fact]
    public void Overview_DetailAndEditActions_AreDistinctFromPrimaryResultClick()
    {
        // Arrange: één actief product in het initiële overzicht.
        var product = new ProductDto { Id = Guid.NewGuid(), Name = "Appel", DefaultUnitName = "stuk" };

        SetupBaseMocks();
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync(null, false, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, Item(product)));

        var cut = RenderComponent<Products>();
        cut.WaitForAssertion(() => Assert.Contains("Appel", cut.Markup));

        // Assert: de primaire klik (productnaam) is een ander element dan de losse
        // detail- en bewerken/code-acties.
        var primaryClick = cut.Find("button.product-main");
        var detailButton = cut.FindAll("button[title='Productdetails']").Single();
        var editButton = cut.FindAll("button[title='Bewerken/code']").Single();

        Assert.Contains("Appel", primaryClick.TextContent);
        Assert.NotSame(primaryClick, detailButton);
        Assert.NotSame(primaryClick, editButton);
        Assert.DoesNotContain("Appel", detailButton.TextContent);
        Assert.DoesNotContain("Appel", editButton.TextContent);
        Assert.Contains("Details", detailButton.TextContent);
        Assert.Contains("Bewerken", editButton.TextContent);
    }

    [Fact]
    public void DesktopOnlyControls_AreGroupedUnderDesktopOnlyContainer()
    {
        // Arrange: leeg overzicht (reader levert lege pagina via SetupBaseMocks).
        SetupBaseMocks();

        // Act
        var cut = RenderComponent<Products>();

        // Assert: de archieftoggle en Voorraadbijzonderheid staan onder .desktop-only
        // (responsieve zichtbaarheidscontract; feitelijke verberging vraagt handmatige
        // viewportcontrole).
        var desktopOnlyButtons = cut.FindAll(".desktop-only button");
        Assert.Contains(desktopOnlyButtons, b => b.TextContent.Contains("Voorraadbijzonderheid"));
        Assert.Contains(desktopOnlyButtons, b => b.TextContent.Contains("weergeven"));

        // En het "Nieuw product"-hoofdactie staat bewust niet onder .desktop-only.
        Assert.DoesNotContain(desktopOnlyButtons, b => b.TextContent.Contains("Nieuw product"));
    }

    [Fact]
    public async Task ArchiveToggle_RequestsArchivedFirstPageFromReader()
    {
        // Arrange: actieve pagina leeg, gearchiveerde pagina met één product.
        var archived = new ProductDto { Id = Guid.NewGuid(), Name = "Oud product", DefaultUnitName = "stuk", IsArchived = true };

        SetupBaseMocks();
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync(null, true, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, Item(archived)));

        var cut = RenderComponent<Products>();

        // Act: schakel naar gearchiveerde weergave.
        await cut.InvokeAsync(() =>
            cut.FindAll("button").First(b => b.TextContent.Contains("Gearchiveerde weergeven")).Click());

        // Assert: de reader wordt met showArchived=true, pagina 1 aangeroepen en het
        // gearchiveerde product wordt getoond met de reactiveer-actie.
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Oud product", cut.Markup);
            Assert.NotEmpty(cut.FindAll("button[title='Reactiveren']"));
        });
        _overviewReadQueryMock.Verify(
            q => q.GetPageAsync(null, true, 1, 10, It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ManualSearch_RequestsReaderWithSearchTerm()
    {
        // Arrange
        var product = new ProductDto
        {
            Id = Guid.NewGuid(),
            Name = "Appel",
            Description = "Rode appels",
            DefaultUnitName = "stuk"
        };

        SetupBaseMocks();
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync("appel", false, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, Item(product)));

        var cut = RenderComponent<Products>();

        // Act
        await SearchAsync(cut, "appel");

        // Assert
        cut.WaitForAssertion(() => Assert.Contains("Appel", cut.Markup));
        _overviewReadQueryMock.Verify(
            q => q.GetPageAsync("appel", false, 1, 10, It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ManualSearch_WithOneActiveLocation_NavigatesDirectlyToLocation()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var product = new ProductDto { Id = productId, Name = "Appel", DefaultUnitName = "stuk" };
        var activeLocation = new StockDto
        {
            StorageLocationId = locationId,
            StorageAreaName = "Kombuis",
            StorageLocationName = "Kast",
            Quantity = 5,
            DefaultUnitName = "stuk"
        };

        SetupBaseMocks();
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync("appel", false, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, Item(product, activeLocation)));
        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { activeLocation }.AsReadOnly()));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<Products>();

        await SearchAsync(cut, "appel");
        cut.WaitForAssertion(() => Assert.Contains("Appel", cut.Markup));

        // Act: klik het resultaat (primaire klik).
        await cut.InvokeAsync(() =>
            cut.FindAll("button").First(b => b.TextContent.Contains("Appel")).Click());

        // Assert: directe navigatie naar de enige locatie.
        cut.WaitForAssertion(() =>
            Assert.EndsWith($"/storage/locations/{locationId}", navigation.Uri));
    }

    [Fact]
    public async Task ManualSearch_WithMultipleActiveLocations_ShowsLocationListWithoutNavigating()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductDto { Id = productId, Name = "Appel", DefaultUnitName = "stuk" };
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

        SetupBaseMocks();
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync("appel", false, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, Item(product, location1, location2)));
        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { location1, location2 }.AsReadOnly()));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var initialUri = navigation.Uri;
        var cut = RenderComponent<Products>();

        await SearchAsync(cut, "appel");
        cut.WaitForAssertion(() => Assert.Contains("Appel", cut.Markup));

        // Act: primaire klik.
        await cut.InvokeAsync(() =>
            cut.FindAll("button").First(b => b.TextContent.Contains("Appel")).Click());

        // Assert: locatielijst getoond, geen navigatie.
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
        var product = new ProductDto { Id = productId, Name = "Appel", DefaultUnitName = "stuk" };
        var expectedLocation = new StockDto
        {
            StorageLocationId = expectedLocationId,
            StorageAreaName = "Kombuis",
            StorageLocationName = "Kast"
        };

        SetupBaseMocks();
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync("appel", false, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, Item(product)));
        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));
        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(expectedLocation));

        var cut = RenderComponent<Products>();

        await SearchAsync(cut, "appel");
        cut.WaitForAssertion(() => Assert.Contains("Appel", cut.Markup));

        // Act: primaire klik.
        await cut.InvokeAsync(() =>
            cut.FindAll("button").First(b => b.TextContent.Contains("Appel")).Click());

        // Assert: no-active-stock met verwachte locatie en voorraad-toevoegen-actie.
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
        var product = new ProductDto { Id = productId, Name = "Appel", DefaultUnitName = "stuk" };

        SetupBaseMocks();
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync("appel", false, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, Item(product)));
        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));
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

        await SearchAsync(cut, "appel");
        cut.WaitForAssertion(() => Assert.Contains("Appel", cut.Markup));

        // Act: primaire klik, daarna "Voorraad toevoegen".
        await cut.InvokeAsync(() =>
            cut.FindAll("button").First(b => b.TextContent.Contains("Appel")).Click());
        cut.WaitForAssertion(() =>
            Assert.Contains("Geen actieve voorraad", cut.Markup, StringComparison.OrdinalIgnoreCase));

        await cut.InvokeAsync(() =>
            cut.FindAll("button").First(b => b.TextContent.Contains("Voorraad toevoegen")).Click());

        // Assert: de modal verschijnt met het product en een locatiekeuze.
        cut.WaitForAssertion(() =>
        {
            var modalHeaders = cut.FindAll("h5");
            Assert.NotEmpty(modalHeaders.Where(h => h.TextContent.Contains("Voorraad toevoegen")));
        });
        Assert.Contains("Appel", cut.Markup);
        Assert.NotEmpty(cut.FindAll("select"));
    }

    [Fact]
    public async Task AdministrativeMutationFallback_ModalCanBeOpened()
    {
        // Arrange
        SetupBaseMocks();

        var cut = RenderComponent<Products>();

        // Act
        await cut.InvokeAsync(() =>
            cut.FindAll("button").First(b => b.TextContent.Contains("Voorraadbijzonderheid")).Click());

        // Assert
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
        SetupBaseMocks();
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
            cut.FindAll("button").First(b => b.TextContent.Contains("Voorraadbijzonderheid")).Click());
        cut.WaitForAssertion(() =>
            Assert.Contains("Voorraadbijzonderheid vastleggen", cut.Markup));

        var componentType = typeof(Products);
        componentType
            .GetField("_fallbackSearchTerm", BindingFlags.Instance | BindingFlags.NonPublic)!
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

    [Fact]
    public async Task ProductSearchResult_ExposesSeparateDetailAction_DistinctFromMainClick()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductDto { Id = productId, Name = "Appel", DefaultUnitName = "stuk" };

        SetupBaseMocks();
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync("appel", false, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, Item(product)));

        var cut = RenderComponent<Products>();
        await SearchAsync(cut, "appel");
        cut.WaitForAssertion(() => Assert.Contains("Appel", cut.Markup));

        // Assert: hoofdklikknop (met productnaam) verschilt van de detailactie.
        var mainClickButton = cut.Find("button.product-main");
        var detailButton = cut.FindAll("button[title='Productdetails']").Single();

        Assert.NotSame(mainClickButton, detailButton);
        Assert.DoesNotContain("Appel", detailButton.TextContent);
        Assert.Contains("Details", detailButton.TextContent);
    }

    [Fact]
    public async Task ProductDetailAction_OpensPopupWithoutNavigating_ShowsUnitCodeAndStock()
    {
        // Arrange: product met code en één actieve locatie.
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Appel",
            Description = "Rode appels",
            DefaultUnitName = "stuk",
            Code = new ProductCodeDto { Id = Guid.NewGuid(), Value = "8712345678904", Format = "barcode" }
        };
        var activeLocation = new StockDto
        {
            StorageLocationId = Guid.NewGuid(),
            StorageAreaName = "Kombuis",
            StorageLocationName = "Kast",
            Quantity = 5,
            DefaultUnitName = "stuk"
        };

        SetupBaseMocks();
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync("appel", false, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, Item(product, activeLocation)));
        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { activeLocation }.AsReadOnly()));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var initialUri = navigation.Uri;
        var cut = RenderComponent<Products>();

        await SearchAsync(cut, "appel");
        cut.WaitForAssertion(() => Assert.Contains("Appel", cut.Markup));

        // Act: klik de expliciete detailactie (niet de primaire klik).
        await cut.InvokeAsync(() =>
            cut.FindAll("button[title='Productdetails']").Single().Click());

        // Assert: popup-inhoud getoond en geen navigatie ondanks één actieve locatie.
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Gekoppelde code", cut.Markup);
            Assert.Contains("8712345678904", cut.Markup);
            Assert.Contains("Eenheid", cut.Markup);
            Assert.Contains("Kombuis - Kast", cut.Markup);
        });
        Assert.Equal(initialUri, navigation.Uri);
    }

    [Fact]
    public async Task ProductDetailAction_WithNoActiveStock_ShowsNoStockWithoutCrash()
    {
        // Arrange: product zonder code en zonder actieve voorraad.
        var productId = Guid.NewGuid();
        var product = new ProductDto { Id = productId, Name = "Appel", DefaultUnitName = "stuk" };

        SetupBaseMocks();
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync("appel", false, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, Item(product)));
        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        var cut = RenderComponent<Products>();
        await SearchAsync(cut, "appel");
        cut.WaitForAssertion(() => Assert.Contains("Appel", cut.Markup));

        // Act: open detail popup.
        await cut.InvokeAsync(() =>
            cut.FindAll("button[title='Productdetails']").Single().Click());

        // Assert: popup toont de no-active-stockstatus zonder codesectie.
        cut.WaitForAssertion(() =>
            Assert.Contains("Geen actieve voorraad", cut.Markup, StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain("Gekoppelde code", cut.Markup);
    }

    [Fact]
    public async Task ProductSearchResult_RendersDetailsAndEditActionsAsSeparateButtons()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductDto { Id = productId, Name = "Appel", DefaultUnitName = "stuk" };

        SetupBaseMocks();
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync("appel", false, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, Item(product)));

        var cut = RenderComponent<Products>();
        await SearchAsync(cut, "appel");
        cut.WaitForAssertion(() => Assert.Contains("Appel", cut.Markup));

        // Assert: zowel de Details- als de bewerken/code-actie bestaan als losse knoppen.
        var detailButton = cut.FindAll("button[title='Productdetails']").Single();
        var editButton = cut.FindAll("button[title='Bewerken/code']").Single();

        Assert.NotSame(detailButton, editButton);
        Assert.Contains("Details", detailButton.TextContent);
        Assert.Contains("Bewerken", editButton.TextContent);
    }

    [Fact]
    public async Task EditProductAction_NavigatesToDeepLinkWithProductId()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new ProductDto { Id = productId, Name = "Appel", DefaultUnitName = "stuk", DefaultUnitId = Guid.NewGuid() };

        SetupBaseMocks();
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync("appel", false, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, Item(product)));
        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<Products>();
        await SearchAsync(cut, "appel");
        cut.WaitForAssertion(() => Assert.Contains("Appel", cut.Markup));

        // Act: klik de bewerken/code-actie.
        await cut.InvokeAsync(() =>
            cut.FindAll("button[title='Bewerken/code']").Single().Click());

        // Assert: deep link met het product-id.
        Assert.EndsWith($"/inventory/products?editProductId={productId}", navigation.Uri);
    }

    [Fact]
    public void Products_WithEditProductIdQuery_FetchesTargetProductOnDemand_OpensEditFormWithCodeSection()
    {
        // Arrange: het doelproduct wordt op aanvraag via GetByIdAsync opgehaald, niet uit een
        // volledige catalogus.
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Appel",
            Description = "Rode appels",
            DefaultUnitName = "stuk",
            DefaultUnitId = Guid.NewGuid(),
            Code = new ProductCodeDto { Id = Guid.NewGuid(), Value = "8712345678904", Format = "barcode" }
        };

        SetupBaseMocks();
        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo($"/inventory/products?editProductId={productId}");

        // Act
        var cut = RenderComponent<Products>();

        // Assert: bestaande bewerkform staat open voor dit product met de codesectie, en het
        // doelproduct is op aanvraag opgehaald.
        cut.WaitForAssertion(() =>
        {
            var nameInput = cut.Find("input[placeholder='Productnaam']");
            Assert.Equal("Appel", nameInput.GetAttribute("value"));
            Assert.Contains("Gekoppelde code", cut.Markup);
            Assert.Contains("8712345678904", cut.Markup);
        });
        _productServiceMock.Verify(
            s => s.GetByIdAsync(productId, It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void Products_WithUnknownEditProductIdQuery_ShowsErrorWithoutCrash()
    {
        // Arrange: query verwijst naar een id dat niet bestaat.
        var missingId = Guid.NewGuid();

        SetupBaseMocks();
        _productServiceMock
            .Setup(s => s.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.NotFound());

        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo($"/inventory/products?editProductId={missingId}");

        // Act
        var cut = RenderComponent<Products>();

        // Assert: duidelijke fout, geen bewerkform, lijstweergave nog bruikbaar (zoekveld aanwezig).
        cut.WaitForAssertion(() =>
            Assert.Contains("Product niet gevonden voor bewerken.", cut.Markup));
        Assert.Empty(cut.FindAll("input[placeholder='Productnaam']"));
        Assert.NotEmpty(cut.FindAll("input.products-search-input"));
    }

    [Fact]
    public void DeepLinkEdit_AfterCancel_CanReopenEditForSameProduct()
    {
        // Arrange: één product dat zowel via het overzicht als via GetByIdAsync beschikbaar is.
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Appel",
            Description = "Rode appels",
            DefaultUnitName = "stuk",
            DefaultUnitId = Guid.NewGuid()
        };

        SetupBaseMocks();
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync(null, false, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Page(1, Item(product)));
        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo($"/inventory/products?editProductId={productId}");

        var cut = RenderComponent<Products>();

        // Assert: de deeplink opent de bewerkform voor dit product.
        cut.WaitForAssertion(() =>
            Assert.Equal("Appel", cut.Find("input[placeholder='Productnaam']").GetAttribute("value")));

        // Act 1: Annuleren binnen de form.
        cut.InvokeAsync(() =>
            cut.FindAll("form button").First(b => b.TextContent.Trim() == "Annuleren").Click());

        // Assert: terug naar het overzicht en de editProductId-query is verwijderd.
        cut.WaitForAssertion(() =>
        {
            Assert.DoesNotContain("editProductId", navigation.Uri);
            Assert.Empty(cut.FindAll("input[placeholder='Productnaam']"));
            Assert.NotEmpty(cut.FindAll("input.products-search-input"));
        });

        // Act 2: opnieuw Bewerken kiezen voor hetzelfde product.
        cut.InvokeAsync(() =>
            cut.FindAll("button[title='Bewerken/code']").Single().Click());

        // Assert: de deeplink is opnieuw actief en de bewerkform staat weer open.
        cut.WaitForAssertion(() =>
        {
            Assert.EndsWith($"/inventory/products?editProductId={productId}", navigation.Uri);
            Assert.Equal("Appel", cut.Find("input[placeholder='Productnaam']").GetAttribute("value"));
        });
    }

    private void SetupBaseMocks()
    {
        // Standaard: leeg overzicht voor iedere reader-aanroep; specifieke tests overschrijven.
        _overviewReadQueryMock
            .Setup(q => q.GetPageAsync(
                It.IsAny<string?>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProductOverviewPageDto());
        // Standaard: onbekend product bij deep-link, tenzij overschreven.
        _productServiceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.NotFound());
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
    }

    private static ProductOverviewPageDto Page(int totalCount, params ProductOverviewItemDto[] items)
        => new() { TotalCount = totalCount, Items = items.ToList() };

    private static ProductOverviewItemDto Item(ProductDto product, params StockDto[] locations)
        => new()
        {
            Product = product,
            ActiveLocations = locations.ToList(),
            TotalQuantity = locations.Sum(l => l.Quantity)
        };

    private static StockDto Location(string areaName, string locationName, decimal quantity)
        => new()
        {
            StorageLocationId = Guid.NewGuid(),
            StorageAreaName = areaName,
            StorageLocationName = locationName,
            Quantity = quantity,
            DefaultUnitName = "stuk"
        };

    /// <summary>
    /// Voert de directe zoekinvoer uit: typt de term en drukt Enter, zoals de
    /// gebruiker op de pagina zoekt.
    /// </summary>
    private static async Task SearchAsync(IRenderedComponent<Products> cut, string term)
    {
        await cut.InvokeAsync(() =>
        {
            cut.Find("input.products-search-input").Input(term);
            cut.Find("input.products-search-input").KeyDown(Key.Enter);
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
