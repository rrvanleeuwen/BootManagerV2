using Bunit;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Results;
using BootManager.Application.Storage.Services;
using BootManager.Web.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Real bUnit tests for LinkLocationQr.razor.
/// Covers QR revalidation and both submit flows.
/// </summary>
public class LinkLocationQrComponentTests : TestContext
{
    private readonly Mock<IStorageService> _storageMock = new();

    public LinkLocationQrComponentTests()
    {
        Services.AddScoped<IStorageService>(_ => _storageMock.Object);
    }

    [Fact]
    public void InvalidQrValue_ShowsInvalidState()
    {
        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/storage/link-location-qr?qrValue=malformed-value");

        var cut = RenderComponent<LinkLocationQr>();

        Assert.Contains("QR-token is ongeldig of ontbreekt", cut.Markup);
    }

    [Fact]
    public async Task ExistingLocationFlow_UsesParsedTokenAndNavigatesToDetail()
    {
        var token = "a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5";
        var qrValue = $"bootmanager:location:{token}";
        var areaId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var areas = new[] { new StorageAreaDto { Id = areaId, Name = "Kombuis" } };
        var locations = new[] { new StorageLocationDto { Id = locationId, StorageAreaId = areaId, Name = "Kast 1" } };

        _storageMock.Setup(s => s.GetAllAreasAsync(default)).ReturnsAsync(areas);
        _storageMock.Setup(s => s.GetLocationsByAreaAsync(areaId, default)).ReturnsAsync(locations);
        _storageMock.Setup(s => s.LinkQrToExistingLocationAsync(token, locationId, default))
            .ReturnsAsync(StorageOperationResult.Ok());

        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo($"/storage/link-location-qr?qrValue={Uri.EscapeDataString(qrValue)}");
        var cut = RenderComponent<LinkLocationQr>();

        cut.WaitForAssertion(() => Assert.Contains(qrValue, cut.Markup));

        await cut.InvokeAsync(() =>
        {
            cut.Find("select").Change(areaId.ToString());
        });

        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll("select").Count));

        await cut.InvokeAsync(() =>
        {
            cut.FindAll("select")[1].Change(locationId.ToString());
            cut.FindAll("button").Single(b => b.TextContent.Contains("Koppelen")).Click();
        });

        _storageMock.Verify(s => s.LinkQrToExistingLocationAsync(token, locationId, default), Times.Once);
        Assert.EndsWith($"/storage/locations/{locationId}", navigation.Uri);
    }

    [Fact]
    public async Task NewLocationFlow_UsesParsedTokenAndNavigatesToNewDetail()
    {
        var token = "a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5";
        var qrValue = $"bootmanager:location:{token}";
        var areaId = Guid.NewGuid();
        var newLocationId = Guid.NewGuid();
        var areas = new[] { new StorageAreaDto { Id = areaId, Name = "Kombuis" } };

        _storageMock.Setup(s => s.GetAllAreasAsync(default)).ReturnsAsync(areas);
        _storageMock.Setup(s => s.GetLocationsByAreaAsync(areaId, default)).ReturnsAsync(Array.Empty<StorageLocationDto>());
        _storageMock.Setup(s => s.CreateLocationWithQrTokenAsync(areaId, "Kast 1", "Beschrijving", token, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(new StorageLocationDetailDto
            {
                Id = newLocationId,
                AreaName = "Kombuis",
                LocationName = "Kast 1",
                Description = "Beschrijving",
                QrValue = qrValue
            }));

        var navigation = Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo($"/storage/link-location-qr?qrValue={Uri.EscapeDataString(qrValue)}");
        var cut = RenderComponent<LinkLocationQr>();

        cut.WaitForAssertion(() => Assert.Contains(qrValue, cut.Markup));

        await cut.InvokeAsync(() =>
        {
            cut.FindAll("button").Single(b => b.TextContent.Contains("Nieuwe locatie aanmaken")).Click();
        });

        await cut.InvokeAsync(() =>
        {
            cut.Find("select").Change(areaId.ToString());
            cut.Find("input[placeholder='Bijv. Kast 1']").Change("Kast 1");
            cut.Find("textarea").Change("Beschrijving");
            cut.FindAll("button").Single(b => b.TextContent.Contains("Locatie aanmaken en koppelen")).Click();
        });

        _storageMock.Verify(s => s.CreateLocationWithQrTokenAsync(areaId, "Kast 1", "Beschrijving", token, default), Times.Once);
        Assert.EndsWith($"/storage/locations/{newLocationId}", navigation.Uri);
    }
}
