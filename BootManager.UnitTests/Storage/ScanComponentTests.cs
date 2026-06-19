using Bunit;
using BootManager.Application.Storage.Results;
using BootManager.Application.Storage.Services;
using BootManager.Web.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Real bUnit tests for Scan.razor QR behavior.
/// Covers navigation and role-based Owner/Crew rendering for manual QR input.
/// </summary>
public class ScanComponentTests : TestContext
{
    private readonly Mock<IStorageService> _storageMock = new();

    public ScanComponentTests()
    {
        Services.AddScoped<IStorageService>(_ => _storageMock.Object);
        SetupScannerJs();
    }

    [Fact]
    public async Task KnownQr_ManualInput_NavigatesDirectlyToLocationDetail()
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
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(qrValue);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() => Assert.EndsWith($"/storage/locations/{locationId}", navigation.Uri));
    }

    [Fact]
    public async Task UnknownValidQr_Owner_SeesLinkActionWithEncodedFullQrValue()
    {
        var token = "a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5";
        var qrValue = $"bootmanager:location:{token}";
        SetupAuthState("Owner");

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(qrValue, default))
            .ReturnsAsync(QrResolutionResult.Unknown(token));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(qrValue);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Koppelen"))));

        await cut.InvokeAsync(() =>
            cut.FindAll("button").Single(b => b.TextContent.Contains("Koppelen")).Click());

        var expected = Uri.EscapeDataString(qrValue);
        Assert.EndsWith($"/storage/link-location-qr?qrValue={expected}", navigation.Uri);
    }

    [Fact]
    public async Task UnknownValidQr_Crew_SeesNoLinkAction()
    {
        var token = "a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5";
        var qrValue = $"bootmanager:location:{token}";
        SetupAuthState("Crew");

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(qrValue, default))
            .ReturnsAsync(QrResolutionResult.Unknown(token));

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(qrValue);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("beheerder", cut.Markup, StringComparison.OrdinalIgnoreCase));

        Assert.Empty(cut.FindAll("button").Where(b => b.TextContent.Contains("Koppelen")));
    }

    [Fact]
    public async Task NonBootManagerValue_RemainsGenericWithoutLinkAction()
    {
        var value = "random:barcode:value";
        SetupAuthState("Owner");

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(value, default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(value);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() => Assert.Contains(value, cut.Markup));

        Assert.Empty(cut.FindAll("button").Where(b => b.TextContent.Contains("Koppelen")));
        Assert.DoesNotContain("BootManager locatie-QR", cut.Markup, StringComparison.Ordinal);
    }

    private void SetupAuthState(string role)
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role) }, "test");
        var principal = new ClaimsPrincipal(identity);
        var authStateMock = new Mock<AuthenticationStateProvider>();
        authStateMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(principal));
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
