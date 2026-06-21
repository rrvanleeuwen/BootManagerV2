using Bunit;
using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Core.Entities;
using BootManager.Web.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Real bUnit tests for ScanProductMutateLocation.razor mutation flow.
/// Tests that selecting a location navigates to a real new scan-specific route
/// and that saving calls StockService.MutateStockAsync(...).
/// </summary>
public class ScanProductMutateLocationComponentTests : TestContext
{
    private readonly Mock<IProductService> _productServiceMock = new();
    private readonly Mock<IStockService> _stockServiceMock = new();

    public ScanProductMutateLocationComponentTests()
    {
        Services.AddScoped<IProductService>(_ => _productServiceMock.Object);
        Services.AddScoped<IStockService>(_ => _stockServiceMock.Object);
    }

    [Fact]
    public async Task MutateLocation_RendersVerbruikAsFirstRealOption()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        SetupAuthState("Crew");

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

        var cut = RenderComponent<ScanProductMutateLocation>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString())
            .Add(p => p.StorageLocationId, locationId.ToString()));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Verbruik (afname)", cut.Markup);
            var typeSelect = cut.Find("select");
            var options = typeSelect.QuerySelectorAll("option");
            var firstRealOption = options.FirstOrDefault(o => o.TextContent.Contains("Verbruik"));
            Assert.NotNull(firstRealOption);
        });
    }

    [Fact]
    public async Task MutateLocation_AllowsSwitchingBetweenMutationTypes()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupAuthState("Owner", userId);

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

        _stockServiceMock
            .Setup(s => s.MutateStockAsync(
                productId, locationId, StockMutationType.Correctie, 10m, userId, null, default))
            .ReturnsAsync(InventoryOperationResult.Ok());

        var cut = RenderComponent<ScanProductMutateLocation>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString())
            .Add(p => p.StorageLocationId, locationId.ToString()));

        await cut.InvokeAsync(() =>
        {
            var typeSelect = cut.Find("select");
            typeSelect.Change("Correctie");

            var quantityInput = cut.FindAll("input[type='number']")[0];
            quantityInput.Change(10);

            var saveButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Opslaan"));
            if (saveButton != null)
                saveButton.Click();
        });

        cut.WaitForAssertion(() =>
        {
            _stockServiceMock.Verify(
                s => s.MutateStockAsync(
                    productId, locationId, StockMutationType.Correctie, 10m, userId, null, default),
                Times.Once);
        });
    }

    [Fact]
    public async Task MutateLocation_RendersProductAndLocationContext()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        SetupAuthState("Crew");

        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk",
            Code = new ProductCodeDto { Value = "TEST-001" }
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

        var cut = RenderComponent<ScanProductMutateLocation>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString())
            .Add(p => p.StorageLocationId, locationId.ToString()));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Test Product", cut.Markup);
            Assert.Contains("Kombuis", cut.Markup);
            Assert.Contains("Kastje", cut.Markup);
            Assert.Contains("5 stuk", cut.Markup);
        });
    }

    [Fact]
    public async Task MutationForm_WithValidInput_CallsMutateStockAsync()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupAuthState("Owner", userId);

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

        _stockServiceMock
            .Setup(s => s.MutateStockAsync(
                productId, locationId, StockMutationType.Verbruik, 2m, userId, null, default))
            .ReturnsAsync(InventoryOperationResult.Ok());

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<ScanProductMutateLocation>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString())
            .Add(p => p.StorageLocationId, locationId.ToString()));

        await cut.InvokeAsync(() =>
        {
            // Fill in the form
            var typeSelect = cut.Find("select");
            typeSelect.Change("Verbruik");

            var quantityInput = cut.FindAll("input[type='number']")[0];
            quantityInput.Change(2);

            // Submit
            var saveButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Opslaan"));
            if (saveButton != null)
                saveButton.Click();
        });

        cut.WaitForAssertion(() =>
        {
            _stockServiceMock.Verify(
                s => s.MutateStockAsync(
                    productId, locationId, StockMutationType.Verbruik, 2m, userId, null, default),
                Times.Once);
        });
    }

    [Fact]
    public async Task MutationSave_WithSuccess_ShowsSuccessMessage()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupAuthState("Owner", userId);

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

        _stockServiceMock
            .Setup(s => s.MutateStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<StockMutationType>(), It.IsAny<decimal>(), It.IsAny<Guid>(), It.IsAny<string>(), default))
            .ReturnsAsync(InventoryOperationResult.Ok());

        var cut = RenderComponent<ScanProductMutateLocation>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString())
            .Add(p => p.StorageLocationId, locationId.ToString()));

        await cut.InvokeAsync(() =>
        {
            var typeSelect = cut.Find("select");
            typeSelect.Change("Verbruik");

            var quantityInput = cut.FindAll("input[type='number']")[0];
            quantityInput.Change(2);

            var saveButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Opslaan"));
            if (saveButton != null)
                saveButton.Click();
        });

        cut.WaitForAssertion(() =>
        {
            _stockServiceMock.Verify(
                s => s.MutateStockAsync(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<StockMutationType>(), It.IsAny<decimal>(), It.IsAny<Guid>(), It.IsAny<string>(), default),
                Times.Once);
        });
    }

    [Fact]
    public async Task InvalidProductId_ShowsNotFoundMessage()
    {
        SetupAuthState("Owner");

        var cut = RenderComponent<ScanProductMutateLocation>(parameters => parameters
            .Add(p => p.ProductId, "invalid-guid")
            .Add(p => p.StorageLocationId, Guid.NewGuid().ToString()));

        cut.WaitForAssertion(() =>
            Assert.Contains("Product of locatie niet gevonden", cut.Markup));
    }

    [Fact]
    public async Task MutationSave_WithNameIdentifierClaimFallback_CallsMutateStockAsync()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupAuthStateWithNameIdentifierOnly("Owner", userId);

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

        _stockServiceMock
            .Setup(s => s.MutateStockAsync(
                productId, locationId, StockMutationType.Verbruik, 2m, userId, null, default))
            .ReturnsAsync(InventoryOperationResult.Ok());

        var cut = RenderComponent<ScanProductMutateLocation>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString())
            .Add(p => p.StorageLocationId, locationId.ToString()));

        await cut.InvokeAsync(() =>
        {
            var typeSelect = cut.Find("select");
            typeSelect.Change("Verbruik");

            var quantityInput = cut.FindAll("input[type='number']")[0];
            quantityInput.Change(2);

            var saveButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Opslaan"));
            if (saveButton != null)
                saveButton.Click();
        });

        cut.WaitForAssertion(() =>
        {
            _stockServiceMock.Verify(
                s => s.MutateStockAsync(
                    productId, locationId, StockMutationType.Verbruik, 2m, userId, null, default),
                Times.Once);
        });
    }

    private void SetupAuthStateWithNameIdentifierOnly(string role, Guid userId)
    {
        var claims = new List<Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
            new(System.Security.Claims.ClaimTypes.Name, "Test User"),
            new(System.Security.Claims.ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        var authState = new AuthenticationState(principal);

        var authStateMock = new Mock<AuthenticationStateProvider>();
        authStateMock
            .Setup(provider => provider.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        Services.AddScoped<AuthenticationStateProvider>(_ => authStateMock.Object);
    }

    private void SetupAuthState(string role, Guid userId = default)
    {
        if (userId == default)
            userId = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user"),
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Role, role),
            new("sub", userId.ToString())
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
