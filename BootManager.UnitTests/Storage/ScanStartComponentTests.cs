using Bunit;
using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Results;
using BootManager.Application.Storage.Services;
using BootManager.Web.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using System.Security.Claims;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Real bUnit tests for new Scan.razor startscreen routing behavior.
/// Tests location code routing, product code handoff, unknown code handoff, and recent scans tracking.
/// </summary>
public class ScanStartComponentTests : TestContext
{
    private readonly Mock<IStorageService> _storageMock = new();
    private readonly Mock<IProductService> _productServiceMock = new();

    public ScanStartComponentTests()
    {
        Services.AddScoped<IStorageService>(_ => _storageMock.Object);
        Services.AddScoped<IProductService>(_ => _productServiceMock.Object);
        SetupScannerJs();
    }

    [Fact]
    public async Task KnownLocationCode_ManualInput_NavigatesToNewLocationScanWorkcontext()
    {
        var token = "a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5";
        var qrValue = $"bootmanager:location:{token}";
        var locationId = Guid.NewGuid();
        SetupAuthState("Owner");

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(qrValue, default))
            .ReturnsAsync(QrResolutionResult.Linked(locationId));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer code in…']").Input(qrValue);
            cut.FindAll("button").Single(b => b.TextContent.Contains("OK")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.EndsWith($"/scan/location/{locationId}", navigation.Uri));
    }

    [Fact]
    public async Task KnownProductCode_ManualInput_NavigatesToNewProductScanContext()
    {
        var productCode = "PROD-12345";
        var productId = Guid.NewGuid();
        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk",
            Code = new ProductCodeDto { Value = productCode }
        };
        SetupAuthState("Crew");

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(productCode, default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(productCode, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer code in…']").Input(productCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("OK")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.EndsWith($"/scan/product/{productId}", navigation.Uri));
    }

    [Fact]
    public async Task UnknownCode_ManualInput_HandsOffToNewUnknownCodeScreenWithCodePreserved()
    {
        // PILOT-SCAN-05: Unknown code from /scan routes to new /scan/unknown screen, not /scan/old
        var unknownCode = "UNKNOWN-99999";
        SetupAuthState("Owner");

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(unknownCode, default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(unknownCode, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.NotFound());

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer code in…']").Input(unknownCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("OK")).Click();
        });

        cut.WaitForAssertion(() =>
        {
            var escapedCode = Uri.EscapeDataString(unknownCode);
            Assert.EndsWith($"/scan/unknown?code={escapedCode}", navigation.Uri);
            Assert.DoesNotContain("/scan/old", navigation.Uri);
        });
    }

    [Fact]
    public async Task RecentScans_MultipleManualEntries_ShowsNewestFirst()
    {
        var firstCode = "UNKNOWN-11111";
        var secondCode = "UNKNOWN-22222";
        SetupAuthState("Owner");

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(It.IsAny<string>(), default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(It.IsAny<string>(), default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.NotFound());

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer code in…']").Input(firstCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("OK")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Recente scans", cut.Markup));

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer code in…']").Input(secondCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("OK")).Click();
        });

        cut.WaitForAssertion(() =>
        {
            var recentButtons = cut.FindAll(".list-group-item-action");
            Assert.Equal(2, recentButtons.Count);
            Assert.Contains(secondCode, recentButtons[0].TextContent);
            Assert.Contains(firstCode, recentButtons[1].TextContent);
        });
    }

    [Fact]
    public async Task ScanStartScreen_RendersAllRequiredElements()
    {
        SetupAuthState("Owner");

        var cut = RenderComponent<Scan>();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Scannen", cut.Markup);
            Assert.Contains("Camera starten", cut.Markup);
            Assert.Contains("Handmatige invoer", cut.Markup);
            Assert.NotNull(cut.Find("input[placeholder='Voer code in…']"));
        });
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

    private void SetupScannerJs()
    {
        var module = JSInterop.SetupModule("./js/barcodeScanner.js");
        module.Setup<bool>("checkSecureContext").SetResult(true);
        module.SetupVoid("stopScan");
        module.SetupVoid("dispose");
        module.SetupVoid("startScan", _ => true);
    }
}
