using Bunit;
using BootManager.Application.Storage.Contracts;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Results;
using BootManager.Application.Storage.Services;
using BootManager.Web.Components.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Real bUnit component tests for StorageLocationTag.razor.
/// Tests that the component uses the QR renderer abstraction (not JS-based generation),
/// and verifies authorization, rendering, and PNG download behavior.
/// </summary>
public class StorageLocationTagComponentTests : TestContext
{
    private readonly Mock<IStorageService> _storageMock = new();
    private readonly Mock<IStorageLocationQrTagRenderer> _qrRendererMock = new();

    public StorageLocationTagComponentTests()
    {
        Services.AddScoped<IStorageService>(_ => _storageMock.Object);
        Services.AddScoped<IStorageLocationQrTagRenderer>(_ => _qrRendererMock.Object);
    }

    [Fact]
    public void Page_LoadsLocationDetail_WhenInitialized()
    {
        var locationId = Guid.NewGuid();
        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "TestArea",
            LocationName = "TestLocation",
            QrValue = "bootmanager:location:a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5"
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        _qrRendererMock.Setup(r => r.RenderQrTagAsync(detailDto.QrValue, default))
            .ReturnsAsync(new StorageLocationQrTagRenderResult { SvgContent = "<svg></svg>", PngBytes = [1, 2, 3] });

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTag>(
            p => p.Add(c => c.LocationId, locationId));

        _storageMock.Verify(s => s.GetLocationDetailAsync(locationId, default), Times.Once);
    }

    [Fact]
    public void Page_DisplaysAreaAndLocationName_WhenDetailLoaded()
    {
        var locationId = Guid.NewGuid();
        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "Kombuis",
            LocationName = "Bovenkast",
            QrValue = "bootmanager:location:a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5"
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        _qrRendererMock.Setup(r => r.RenderQrTagAsync(detailDto.QrValue, default))
            .ReturnsAsync(new StorageLocationQrTagRenderResult { SvgContent = "<svg></svg>", PngBytes = [1, 2, 3] });

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTag>(
            p => p.Add(c => c.LocationId, locationId));

        var markup = cut.Markup;
        Assert.Contains("Kombuis", markup);
        Assert.Contains("Bovenkast", markup);
    }

    [Fact]
    public void Page_DisplaysQrValue_WhenPresent()
    {
        var locationId = Guid.NewGuid();
        var qrValue = "bootmanager:location:a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5";
        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "Kombuis",
            LocationName = "Bovenkast",
            QrValue = qrValue
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        _qrRendererMock.Setup(r => r.RenderQrTagAsync(qrValue, default))
            .ReturnsAsync(new StorageLocationQrTagRenderResult { SvgContent = "<svg></svg>", PngBytes = [1, 2, 3] });

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTag>(
            p => p.Add(c => c.LocationId, locationId));

        var markup = cut.Markup;
        Assert.Contains(qrValue, markup);
    }

    [Fact]
    public void Page_ShowsWarning_WhenNoQrValue()
    {
        var locationId = Guid.NewGuid();
        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "Kombuis",
            LocationName = "Bovenkast",
            QrValue = null
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTag>(
            p => p.Add(c => c.LocationId, locationId));

        var markup = cut.Markup;
        Assert.Contains("nog geen QR-code", markup);
    }

    [Fact]
    public async Task PrintButton_CallsWindowPrint()
    {
        var locationId = Guid.NewGuid();
        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "Kombuis",
            LocationName = "Bovenkast",
            QrValue = "bootmanager:location:a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5"
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        _qrRendererMock.Setup(r => r.RenderQrTagAsync(detailDto.QrValue, default))
            .ReturnsAsync(new StorageLocationQrTagRenderResult { SvgContent = "<svg></svg>", PngBytes = [1, 2, 3] });

        SetupAuthState(owner: true);
        JSInterop.SetupVoid("window.print");

        var cut = RenderComponent<StorageLocationTag>(
            p => p.Add(c => c.LocationId, locationId));

        var printButton = cut.FindAll("button")
            .FirstOrDefault(b => b.TextContent.Contains("Afdrukken"));

        Assert.NotNull(printButton);
        await cut.InvokeAsync(() => printButton.Click());

        Assert.Single(JSInterop.Invocations.Where(i => i.Identifier == "window.print"));
    }

    [Fact]
    public async Task BackButton_CallsHistoryBack()
    {
        var locationId = Guid.NewGuid();
        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "Kombuis",
            LocationName = "Bovenkast",
            QrValue = "bootmanager:location:a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5"
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        _qrRendererMock.Setup(r => r.RenderQrTagAsync(detailDto.QrValue, default))
            .ReturnsAsync(new StorageLocationQrTagRenderResult { SvgContent = "<svg></svg>", PngBytes = [1, 2, 3] });

        SetupAuthState(owner: true);
        JSInterop.SetupVoid("history.back");

        var cut = RenderComponent<StorageLocationTag>(
            p => p.Add(c => c.LocationId, locationId));

        var backButton = cut.FindAll("button")
            .FirstOrDefault(b => b.TextContent.Contains("Terug"));

        Assert.NotNull(backButton);
        await cut.InvokeAsync(() => backButton.Click());

        Assert.Single(JSInterop.Invocations.Where(i => i.Identifier == "history.back"));
    }

    [Fact]
    public void PngDownloadButton_IsDisabled_WhenQrRenderingFails()
    {
        var locationId = Guid.NewGuid();
        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "Kombuis",
            LocationName = "Bovenkast",
            QrValue = "bootmanager:location:a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5"
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        // QR rendering fails
        _qrRendererMock.Setup(r => r.RenderQrTagAsync(detailDto.QrValue, default))
            .ThrowsAsync(new InvalidOperationException("QR rendering failed"));

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTag>(
            p => p.Add(c => c.LocationId, locationId));

        var pngButton = cut.FindAll("button")
            .FirstOrDefault(b => b.TextContent.Contains("PNG"));

        Assert.NotNull(pngButton);
        // Button must remain disabled when QR rendering fails
        var isDisabled = pngButton.GetAttribute("disabled");
        Assert.NotNull(isDisabled);
    }

    [Fact]
    public void PngDownloadButton_IsEnabled_WhenQrRenderingSucceeds()
    {
        var locationId = Guid.NewGuid();
        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "Kombuis",
            LocationName = "Bovenkast",
            QrValue = "bootmanager:location:a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5"
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        // QR rendering succeeds
        _qrRendererMock.Setup(r => r.RenderQrTagAsync(detailDto.QrValue, default))
            .ReturnsAsync(new StorageLocationQrTagRenderResult { SvgContent = "<svg></svg>", PngBytes = [1, 2, 3] });

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTag>(
            p => p.Add(c => c.LocationId, locationId));

        cut.WaitForAssertion(() =>
        {
            var pngButton = cut.FindAll("button")
                .FirstOrDefault(b => b.TextContent.Contains("PNG"));

            Assert.NotNull(pngButton);
            // Button must be enabled when QR rendering succeeds
            var isDisabled = pngButton.GetAttribute("disabled");
            Assert.Null(isDisabled);
        });
    }

    [Fact]
    public async Task PngDownloadButton_CallsDownloadFileFromStream_WithFilenameAndStream()
    {
        var locationId = Guid.NewGuid();
        var locationName = "TestLocation";
        var svgMarkup = "<svg xmlns='http://www.w3.org/2000/svg' width='200' height='200'><rect width='200' height='200' fill='white'/></svg>";
        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "TestArea",
            LocationName = locationName,
            QrValue = "bootmanager:location:a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5"
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        _qrRendererMock.Setup(r => r.RenderQrTagAsync(detailDto.QrValue, default))
            .ReturnsAsync(new StorageLocationQrTagRenderResult { SvgContent = svgMarkup, PngBytes = [1, 2, 3] });

        SetupAuthState(owner: true);
        JSInterop.SetupVoid("downloadFileFromStream");

        var cut = RenderComponent<StorageLocationTag>(
            p => p.Add(c => c.LocationId, locationId));

        cut.WaitForAssertion(() =>
        {
            var pngButton = cut.FindAll("button")
                .FirstOrDefault(b => b.TextContent.Contains("PNG"));
            Assert.NotNull(pngButton);
            Assert.Null(pngButton.GetAttribute("disabled"));
        });

        var pngBtn = cut.FindAll("button")
            .FirstOrDefault(b => b.TextContent.Contains("PNG"));
        Assert.NotNull(pngBtn);
        await cut.InvokeAsync(() => pngBtn.Click());

        // Verify downloadFileFromStream was called with filename and stream reference
        var invocations = JSInterop.Invocations
            .Where(i => i.Identifier == "downloadFileFromStream")
            .ToList();

        Assert.NotEmpty(invocations);
        var call = invocations.First();
        Assert.NotNull(call.Arguments);
        Assert.Equal(2, call.Arguments.Count);
        var filename = call.Arguments[0]?.ToString();
        var streamReference = call.Arguments[1];

        Assert.Equal($"{locationName}.png", filename);
        Assert.NotNull(streamReference);
    }

    [Fact]
    public void PngDownloadHelper_NotCalled_WhenQrRenderingFails()
    {
        var locationId = Guid.NewGuid();
        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "TestArea",
            LocationName = "TestLocation",
            QrValue = "bootmanager:location:a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5"
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        // QR rendering fails
        _qrRendererMock.Setup(r => r.RenderQrTagAsync(detailDto.QrValue, default))
            .ThrowsAsync(new InvalidOperationException("QR rendering failed"));

        SetupAuthState(owner: true);
        JSInterop.SetupVoid("downloadFileFromStream");

        var cut = RenderComponent<StorageLocationTag>(
            p => p.Add(c => c.LocationId, locationId));

        // Verify PNG button is disabled and helper never gets called
        var pngButton = cut.FindAll("button")
            .FirstOrDefault(b => b.TextContent.Contains("PNG"));

        Assert.NotNull(pngButton);
        var isDisabled = pngButton.GetAttribute("disabled");
        Assert.NotNull(isDisabled);

        // Verify the download helper was NEVER invoked
        var invocations = JSInterop.Invocations
            .Where(i => i.Identifier == "downloadFileFromStream")
            .ToList();

        Assert.Empty(invocations);
    }

    [Fact]
    public void Page_UsesCompactQrLayout_WithDedicatedContainer()
    {
        var locationId = Guid.NewGuid();
        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "Kombuis",
            LocationName = "Bovenkast",
            QrValue = "bootmanager:location:a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5"
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        _qrRendererMock.Setup(r => r.RenderQrTagAsync(detailDto.QrValue, default))
            .ReturnsAsync(new StorageLocationQrTagRenderResult { SvgContent = "<svg></svg>", PngBytes = [1, 2, 3] });

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTag>(
            p => p.Add(c => c.LocationId, locationId));

        cut.WaitForAssertion(() =>
        {
            var markup = cut.Markup;
            // Verify the compact layout container exists
            Assert.Contains("tag-qr-wrapper", markup);
            // Verify the compact content wrapper exists
            Assert.Contains("tag-qr-content", markup);
            // Verify compact heading classes
            Assert.Contains("tag-qr-area", markup);
            Assert.Contains("tag-qr-location", markup);
            // Verify compact QR value display
            Assert.Contains("tag-qr-value", markup);
        });
    }

    [Fact]
    public void ComponentUsesQrRendererAbstraction_NotJsBasedGeneration()
    {
        var locationId = Guid.NewGuid();
        var qrValue = "bootmanager:location:a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5";
        var svgContent = "<svg><circle cx='10' cy='10' r='10'/></svg>";
        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "Kombuis",
            LocationName = "Bovenkast",
            QrValue = qrValue
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        _qrRendererMock.Setup(r => r.RenderQrTagAsync(qrValue, default))
            .ReturnsAsync(new StorageLocationQrTagRenderResult { SvgContent = svgContent, PngBytes = [1, 2, 3] });

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTag>(
            p => p.Add(c => c.LocationId, locationId));

        // Verify renderer was called with the correct QR value
        _qrRendererMock.Verify(r => r.RenderQrTagAsync(qrValue, default), Times.Once);

        // Verify SVG content appears in the markup (not JS canvas)
        cut.WaitForAssertion(() =>
        {
            Assert.Contains(svgContent, cut.Markup);
        });
    }

    [Fact]
    public void Page_ShowsNotFound_WhenLocationDoesNotExist()
    {
        var locationId = Guid.NewGuid();

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.NotFound());

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTag>(
            p => p.Add(c => c.LocationId, locationId));

        var markup = cut.Markup;
        Assert.Contains("niet gevonden", markup);
    }

    private void SetupAuthState(bool owner)
    {
        var role = owner ? "Owner" : "Crew";
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role) }, "test");
        var principal = new ClaimsPrincipal(identity);
        var authStateMock = new Mock<AuthenticationStateProvider>();
        authStateMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(principal));
        Services.AddScoped(_ => authStateMock.Object);
    }
}
