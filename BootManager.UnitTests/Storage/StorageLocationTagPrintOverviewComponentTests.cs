using Bunit;
using BootManager.Application.Storage.Contracts;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Services;
using BootManager.Core.Enums;
using BootManager.Web.Components.Pages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Real bUnit component tests voor StorageLocationTagPrintOverview.razor: het A4-printoverzicht
/// rendert per locatie met QR-token een QR-afbeelding met gebied- en locatienaam.
/// </summary>
public class StorageLocationTagPrintOverviewComponentTests : TestContext
{
    private readonly Mock<IStorageService> _storageMock = new();
    private readonly Mock<IStorageLocationQrTagRenderer> _rendererMock = new();

    public StorageLocationTagPrintOverviewComponentTests()
    {
        Services.AddScoped<IStorageService>(_ => _storageMock.Object);
        Services.AddScoped<IStorageLocationQrTagRenderer>(_ => _rendererMock.Object);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void RendersQrTagsWithAreaAndLocation()
    {
        var tags = new List<StorageLocationOverviewDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AreaName = "Kombuis",
                LocationName = "Kruidenkast",
                QrValue = "bootmanager:location:abcd1234efgh5678ijkl9012mnop3456",
                TagStatus = TagStatus.Printed
            },
            new()
            {
                Id = Guid.NewGuid(),
                AreaName = "Salon",
                LocationName = "Rugleuning",
                QrValue = "bootmanager:location:0123456789abcdef0123456789abcdef",
                TagStatus = TagStatus.Printed
            }
        };

        _storageMock.Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);
        _rendererMock.Setup(r => r.RenderQrTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string qr, CancellationToken _) => new StorageLocationQrTagRenderResult
            {
                SvgContent = $"<svg data-qr=\"{qr}\"><rect/></svg>",
                PngBytes = Array.Empty<byte>()
            });

        var cut = RenderComponent<StorageLocationTagPrintOverview>();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Kombuis", cut.Markup);
            Assert.Contains("Kruidenkast", cut.Markup);
            Assert.Contains("Salon", cut.Markup);
            Assert.Contains("Rugleuning", cut.Markup);
        });

        // Beide locaties krijgen een gerenderde QR-afbeelding.
        var svgs = cut.FindAll("svg");
        Assert.Equal(2, svgs.Count);
        _rendererMock.Verify(r => r.RenderQrTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public void RendersAllTaggedLocationsForBatchPrint_AcrossManyLocations()
    {
        // Batchprint moet alle beschikbare QR-tags renderen; bij grotere aantallen
        // paginatie de browser dat automatisch. Dit bewijst dat de printroute niet
        // aftopt en elk getagd item een eigen kaart met QR-afbeelding krijgt.
        var tags = Enumerable.Range(0, 12)
            .Select(i => new StorageLocationOverviewDto
            {
                Id = Guid.NewGuid(),
                AreaName = $"Gebied {i}",
                LocationName = $"Locatie {i}",
                QrValue = $"bootmanager:location:{i:D2}cd1234efgh5678ijkl9012mnop3456",
                TagStatus = TagStatus.Printed
            })
            .ToList();

        _storageMock.Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);
        _rendererMock.Setup(r => r.RenderQrTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string qr, CancellationToken _) => new StorageLocationQrTagRenderResult
            {
                SvgContent = $"<svg data-qr=\"{qr}\"><rect/></svg>",
                PngBytes = Array.Empty<byte>()
            });

        var cut = RenderComponent<StorageLocationTagPrintOverview>();

        cut.WaitForAssertion(() =>
        {
            foreach (var tag in tags)
            {
                Assert.Contains(tag.AreaName, cut.Markup);
                Assert.Contains(tag.LocationName, cut.Markup);
            }
        });

        Assert.Equal(12, cut.FindAll("svg").Count);
        _rendererMock.Verify(
            r => r.RenderQrTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(12));
    }

    [Fact]
    public void ShowsInfoMessageWhenNoTaggedLocations()
    {
        _storageMock.Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StorageLocationOverviewDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    AreaName = "Kombuis",
                    LocationName = "Zonder tag",
                    QrValue = null,
                    TagStatus = TagStatus.NotPrinted
                }
            });

        var cut = RenderComponent<StorageLocationTagPrintOverview>();

        cut.WaitForAssertion(() =>
            Assert.Contains("Geen opslaglocaties met QR-tag", cut.Markup));
        Assert.Empty(cut.FindAll("svg"));
    }
}
