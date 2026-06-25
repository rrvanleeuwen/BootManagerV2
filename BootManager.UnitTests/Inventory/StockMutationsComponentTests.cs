using Bunit;
using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Core.Entities;
using BootManager.Web.Components.Pages.Inventory;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace BootManager.UnitTests.Inventory;

/// <summary>
/// Real bUnit tests for StockMutations.razor component.
/// Covers basic product selection workflow and location handling.
/// Query parameter handling is tested through HomeComponentTests integration.
/// </summary>
public class StockMutationsComponentTests : TestContext
{
    private readonly Mock<IProductService> _productServiceMock = new();
    private readonly Mock<IStockService> _stockServiceMock = new();

    public StockMutationsComponentTests()
    {
        Services.AddScoped<IProductService>(_ => _productServiceMock.Object);
        Services.AddScoped<IStockService>(_ => _stockServiceMock.Object);
        Services.AddLogging();
        SetupAuthState("Owner");
    }

    [Fact]
    public void StockMutations_RenderWithoutQueryParam_StartAtProductSelection()
    {
        // Arrange
        _productServiceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>().AsReadOnly());

        // Act
        var cut = RenderComponent<StockMutations>();

        // Assert: Should show product selection step
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("1. Product selecteren", cut.Markup);
            Assert.Contains("Zoeken op productnaam", cut.Markup);
        });
    }

    [Fact]
    public async Task StockMutations_SearchAndSelectProduct_MovesToLocationSelection()
    {
        // Arrange: Product with one active location
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Reddingsvest Pro 150N",
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
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { product }.AsReadOnly());

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { stock }.AsReadOnly()));

        var cut = RenderComponent<StockMutations>();

        // Act: Search for product
        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input[placeholder='Zoeken op productnaam…']");
            searchInput.Input("Reddingsvest");
            var searchBtn = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Zoeken"));
            searchBtn?.Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Reddingsvest Pro 150N", cut.Markup));

        // Act: Click product to select it
        await cut.InvokeAsync(() =>
        {
            var productBtn = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Reddingsvest Pro 150N"));
            productBtn?.Click();
        });

        // Assert: Should move to location selection
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("2. Locatie selecteren", cut.Markup);
            Assert.Contains("Reddingsvest Pro 150N", cut.Markup);
            Assert.Contains("Magazijn A - Schap A-24", cut.Markup);
        });
    }

    [Fact]
    public async Task StockMutations_WithProductWithMultipleLocations_RequiresLocationSelection()
    {
        // Arrange: Product at multiple locations
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "VHF Radio",
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
            StorageAreaName = "Magazijn",
            StorageLocationName = "Schap",
            Quantity = 3,
            DefaultUnitName = "stuk"
        };

        _productServiceMock
            .Setup(s => s.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { product }.AsReadOnly());

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { location1, location2 }.AsReadOnly()));

        var cut = RenderComponent<StockMutations>();

        // Act: Search and select product
        await cut.InvokeAsync(() =>
        {
            var searchInput = cut.Find("input[placeholder='Zoeken op productnaam…']");
            searchInput.Input("VHF");
        });

        await cut.InvokeAsync(() =>
        {
            var searchBtn = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Zoeken"));
            searchBtn?.Click();
        });

        cut.WaitForAssertion(() => Assert.Contains("VHF Radio", cut.Markup));

        await cut.InvokeAsync(() =>
        {
            var productBtn = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("VHF Radio"));
            productBtn?.Click();
        });

        // Assert: Should show both locations without auto-selection
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Kantoor - Voorraad", cut.Markup);
            Assert.Contains("Magazijn - Schap", cut.Markup);
            Assert.DoesNotContain("Automatisch geselecteerd", cut.Markup);
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
