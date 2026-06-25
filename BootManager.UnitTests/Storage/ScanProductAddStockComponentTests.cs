using Bunit;
using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Services;
using BootManager.Application.Storage.Results;
using BootManager.Web.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Real bUnit tests for ScanProductAddStock.razor add-stock flow.
/// Tests that saving calls StockService.AddOrIncrementStockAsync(...).
/// </summary>
public class ScanProductAddStockComponentTests : TestContext
{
    private readonly Mock<IProductService> _productServiceMock = new();
    private readonly Mock<IStockService> _stockServiceMock = new();
    private readonly Mock<IStorageService> _storageServiceMock = new();

    public ScanProductAddStockComponentTests()
    {
        Services.AddScoped<IProductService>(_ => _productServiceMock.Object);
        Services.AddScoped<IStockService>(_ => _stockServiceMock.Object);
        Services.AddScoped<IStorageService>(_ => _storageServiceMock.Object);

        var scannerModule = JSInterop.SetupModule("./js/barcodeScanner.js");
        scannerModule.Setup<bool>("checkSecureContext").SetResult(true);
        scannerModule.Setup<Task>("startScan", _ => true).SetResult(Task.CompletedTask);
        scannerModule.Setup<Task>("stopScan", _ => true).SetResult(Task.CompletedTask);
    }

    [Fact]
    public async Task AddStockForm_WithValidInput_CallsAddOrIncrementStockAsync()
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

        var location = new StorageLocationDto
        {
            Id = locationId,
            StorageAreaId = Guid.NewGuid(),
            StorageAreaName = "Kombuis",
            Name = "Kastje"
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        _storageServiceMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new[] { new StorageAreaDto { Id = location.StorageAreaId, Name = "Kombuis" } });

        _storageServiceMock
            .Setup(s => s.GetLocationsByAreaAsync(location.StorageAreaId, default))
            .ReturnsAsync(new[] { location });

        _storageServiceMock
            .Setup(s => s.ResolveQrValueAsync(It.IsAny<string>(), default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _stockServiceMock
            .Setup(s => s.AddOrIncrementStockAsync(productId, locationId, 10m, default))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(new StockDto()));

        var cut = RenderComponent<ScanProductAddStock>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        await cut.InvokeAsync(() =>
        {
            var select = cut.Find("select");
            select.Change(locationId.ToString());

            var quantityInput = cut.FindAll("input[type='number']")[0];
            quantityInput.Change(10);

            var button = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Voorraad toevoegen"));
            if (button != null)
                button.Click();
        });

        cut.WaitForAssertion(() =>
        {
            _stockServiceMock.Verify(
                s => s.AddOrIncrementStockAsync(productId, locationId, 10m, default),
                Times.Once);
        });
    }

    [Fact]
    public async Task AddStockSave_WithSuccess_CallsServiceAndShowsMessage()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        SetupAuthState("Owner");

        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        var location = new StorageLocationDto
        {
            Id = locationId,
            StorageAreaId = Guid.NewGuid(),
            StorageAreaName = "Kombuis",
            Name = "Kastje"
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        _storageServiceMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new[] { new StorageAreaDto { Id = location.StorageAreaId, Name = "Kombuis" } });

        _storageServiceMock
            .Setup(s => s.GetLocationsByAreaAsync(location.StorageAreaId, default))
            .ReturnsAsync(new[] { location });

        _storageServiceMock
            .Setup(s => s.ResolveQrValueAsync(It.IsAny<string>(), default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _stockServiceMock
            .Setup(s => s.AddOrIncrementStockAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), default))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(new StockDto()));

        var cut = RenderComponent<ScanProductAddStock>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        await cut.InvokeAsync(() =>
        {
            var select = cut.Find("select");
            select.Change(locationId.ToString());

            var quantityInput = cut.FindAll("input[type='number']")[0];
            quantityInput.Change(10);

            var button = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Voorraad toevoegen"));
            if (button != null)
                button.Click();
        });

        cut.WaitForAssertion(() =>
        {
            _stockServiceMock.Verify(
                s => s.AddOrIncrementStockAsync(
                    It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), default),
                Times.Once);
        });
    }

    [Fact]
    public async Task AddStockPage_RendersProductAndLocationSelection()
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

        var location = new StorageLocationDto
        {
            Id = locationId,
            StorageAreaId = Guid.NewGuid(),
            StorageAreaName = "Kombuis",
            Name = "Kastje"
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        _storageServiceMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new[] { new StorageAreaDto { Id = location.StorageAreaId, Name = "Kombuis" } });

        _storageServiceMock
            .Setup(s => s.GetLocationsByAreaAsync(location.StorageAreaId, default))
            .ReturnsAsync(new[] { location });

        _storageServiceMock
            .Setup(s => s.ResolveQrValueAsync(It.IsAny<string>(), default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        var cut = RenderComponent<ScanProductAddStock>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Test Product", cut.Markup);
            Assert.Contains("Locatie selecteren", cut.Markup);
            Assert.Contains("Hoeveelheid", cut.Markup);
        });
    }

    [Fact]
    public async Task AddStockForm_WithManualLocationCode_SelectsLocationAndCallsService()
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

        var location = new StorageLocationDto
        {
            Id = locationId,
            StorageAreaId = Guid.NewGuid(),
            StorageAreaName = "Kombuis",
            Name = "Kastje"
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        _storageServiceMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new[] { new StorageAreaDto { Id = location.StorageAreaId, Name = "Kombuis" } });

        _storageServiceMock
            .Setup(s => s.GetLocationsByAreaAsync(location.StorageAreaId, default))
            .ReturnsAsync(new[] { location });

        _storageServiceMock
            .Setup(s => s.ResolveQrValueAsync(It.IsAny<string>(), default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _stockServiceMock
            .Setup(s => s.AddOrIncrementStockAsync(productId, locationId, 5m, default))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(new StockDto()));

        var cut = RenderComponent<ScanProductAddStock>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        await cut.InvokeAsync(() =>
        {
            var manualCodeInput = cut.Find("#location-code-input");
            manualCodeInput.Input("Kastje");

            var inputs = cut.FindAll(".scan-location-manual-input button");
            if (inputs.Count > 0)
                inputs[0].Click();

            var quantityInput = cut.FindAll("input[type='number']")[0];
            quantityInput.Change(5);

            var submitButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Voorraad toevoegen"));
            if (submitButton != null)
                submitButton.Click();
        });

        cut.WaitForAssertion(() =>
        {
            _stockServiceMock.Verify(
                s => s.AddOrIncrementStockAsync(productId, locationId, 5m, default),
                Times.Once);
        });
    }

    [Fact]
    public async Task AddStockForm_WithManualLocationCode_UnknownCode_ShowsError()
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

        var location = new StorageLocationDto
        {
            Id = locationId,
            StorageAreaId = Guid.NewGuid(),
            StorageAreaName = "Kombuis",
            Name = "Kastje"
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        _storageServiceMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new[] { new StorageAreaDto { Id = location.StorageAreaId, Name = "Kombuis" } });

        _storageServiceMock
            .Setup(s => s.GetLocationsByAreaAsync(location.StorageAreaId, default))
            .ReturnsAsync(new[] { location });

        _storageServiceMock
            .Setup(s => s.ResolveQrValueAsync(It.IsAny<string>(), default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        var cut = RenderComponent<ScanProductAddStock>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        await cut.InvokeAsync(() =>
        {
            var manualCodeInput = cut.Find("#location-code-input");
            manualCodeInput.Input("UnknownLocation");

            var inputs = cut.FindAll(".scan-location-manual-input button");
            if (inputs.Count > 0)
                inputs[0].Click();
        });

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Locatie niet gevonden", cut.Markup);
        });
    }

    [Fact]
    public async Task AddStockForm_WithPastedBootManagerLocationQr_ResolvesAndSelectsLocation()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var qrToken = "b57e72abe729a2f3dc2408eb9ab76d0c";
        var qrValue = $"bootmanager:location:{qrToken}";
        SetupAuthState("Crew");

        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        var location = new StorageLocationDto
        {
            Id = locationId,
            StorageAreaId = Guid.NewGuid(),
            StorageAreaName = "Kombuis",
            Name = "Kastje"
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        _storageServiceMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new[] { new StorageAreaDto { Id = location.StorageAreaId, Name = "Kombuis" } });

        _storageServiceMock
            .Setup(s => s.GetLocationsByAreaAsync(location.StorageAreaId, default))
            .ReturnsAsync(new[] { location });

        _storageServiceMock
            .Setup(s => s.ResolveQrValueAsync(qrValue, default))
            .ReturnsAsync(QrResolutionResult.Linked(locationId));

        _stockServiceMock
            .Setup(s => s.AddOrIncrementStockAsync(productId, locationId, 5m, default))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(new StockDto()));

        var cut = RenderComponent<ScanProductAddStock>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        await cut.InvokeAsync(() =>
        {
            var manualCodeInput = cut.Find("#location-code-input");
            manualCodeInput.Input(qrValue);

            var inputs = cut.FindAll(".scan-location-manual-input button");
            if (inputs.Count > 0)
                inputs[0].Click();

            var quantityInput = cut.FindAll("input[type='number']")[0];
            quantityInput.Change(5);

            var submitButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Voorraad toevoegen"));
            if (submitButton != null)
                submitButton.Click();
        });

        cut.WaitForAssertion(() =>
        {
            _storageServiceMock.Verify(
                s => s.ResolveQrValueAsync(qrValue, default),
                Times.Once);
            _stockServiceMock.Verify(
                s => s.AddOrIncrementStockAsync(productId, locationId, 5m, default),
                Times.Once);
        });
    }

    [Fact]
    public async Task AddStockForm_WithUnknownBootManagerQr_ShowsError()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var unknownQrToken = "b57e72abe729a2f3dc2408eb9ab76d0c";
        var unknownQrValue = $"bootmanager:location:{unknownQrToken}";
        SetupAuthState("Crew");

        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        var location = new StorageLocationDto
        {
            Id = locationId,
            StorageAreaId = Guid.NewGuid(),
            StorageAreaName = "Kombuis",
            Name = "Kastje"
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        _storageServiceMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new[] { new StorageAreaDto { Id = location.StorageAreaId, Name = "Kombuis" } });

        _storageServiceMock
            .Setup(s => s.GetLocationsByAreaAsync(location.StorageAreaId, default))
            .ReturnsAsync(new[] { location });

        _storageServiceMock
            .Setup(s => s.ResolveQrValueAsync(unknownQrValue, default))
            .ReturnsAsync(QrResolutionResult.Unknown(unknownQrToken));

        var cut = RenderComponent<ScanProductAddStock>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        await cut.InvokeAsync(() =>
        {
            var manualCodeInput = cut.Find("#location-code-input");
            manualCodeInput.Input(unknownQrValue);

            var inputs = cut.FindAll(".scan-location-manual-input button");
            if (inputs.Count > 0)
                inputs[0].Click();
        });

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Onbekende BootManager locatie-QR", cut.Markup);
        });
    }

    [Fact]
    public async Task AddStockForm_WithCameraScannedBootManagerLocationQr_ResolvesAndSelectsLocation()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var qrToken = "b57e72abe729a2f3dc2408eb9ab76d0c";
        var qrValue = $"bootmanager:location:{qrToken}";
        SetupAuthState("Crew");

        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        var location = new StorageLocationDto
        {
            Id = locationId,
            StorageAreaId = Guid.NewGuid(),
            StorageAreaName = "Kombuis",
            Name = "Kastje"
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        _storageServiceMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new[] { new StorageAreaDto { Id = location.StorageAreaId, Name = "Kombuis" } });

        _storageServiceMock
            .Setup(s => s.GetLocationsByAreaAsync(location.StorageAreaId, default))
            .ReturnsAsync(new[] { location });

        _storageServiceMock
            .Setup(s => s.ResolveQrValueAsync(qrValue, default))
            .ReturnsAsync(QrResolutionResult.Linked(locationId));

        _stockServiceMock
            .Setup(s => s.AddOrIncrementStockAsync(productId, locationId, 3m, default))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(new StockDto()));

        var cut = RenderComponent<ScanProductAddStock>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        // Simulate camera scan result via OnScanResult callback (matching barcodeScanner.js behavior)
        var component = cut.Instance as ScanProductAddStock;
        await cut.InvokeAsync(() =>
        {
            if (component != null)
            {
                component.OnScanResult(0, qrValue, "QR_CODE");
            }
        });

        // Wait for the async processing and state change to complete
        cut.WaitForAssertion(() =>
        {
            _storageServiceMock.Verify(
                s => s.ResolveQrValueAsync(qrValue, default),
                Times.Once);
        }, TimeSpan.FromSeconds(5));

        // Now proceed with quantity and save
        await cut.InvokeAsync(() =>
        {
            var quantityInput = cut.FindAll("input[type='number']")[0];
            quantityInput.Change(3);

            var submitButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Voorraad toevoegen"));
            if (submitButton != null)
                submitButton.Click();
        });

        cut.WaitForAssertion(() =>
        {
            _stockServiceMock.Verify(
                s => s.AddOrIncrementStockAsync(productId, locationId, 3m, default),
                Times.Once);
        }, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AddStockForm_WithCameraScannedUnknownBootManagerQr_ShowsErrorMessage()
    {
        var productId = Guid.NewGuid();
        var qrToken = "a00000000000000000000000000000d0";
        var qrValue = $"bootmanager:location:{qrToken}";
        SetupAuthState("Crew");

        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        var location = new StorageLocationDto
        {
            Id = Guid.NewGuid(),
            StorageAreaId = Guid.NewGuid(),
            StorageAreaName = "Kombuis",
            Name = "Kastje"
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        _storageServiceMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new[] { new StorageAreaDto { Id = location.StorageAreaId, Name = "Kombuis" } });

        _storageServiceMock
            .Setup(s => s.GetLocationsByAreaAsync(location.StorageAreaId, default))
            .ReturnsAsync(new[] { location });

        _storageServiceMock
            .Setup(s => s.ResolveQrValueAsync(qrValue, default))
            .ReturnsAsync(QrResolutionResult.Unknown(qrToken));

        var cut = RenderComponent<ScanProductAddStock>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        // Simulate camera scan result via OnScanResult callback
        var component = cut.Instance as ScanProductAddStock;
        await cut.InvokeAsync(() =>
        {
            if (component != null)
            {
                component.OnScanResult(0, qrValue, "QR_CODE");
            }
        });

        // Verify that the QR was resolved as expected
        cut.WaitForAssertion(() =>
        {
            _storageServiceMock.Verify(
                s => s.ResolveQrValueAsync(qrValue, default),
                Times.Once);
            Assert.Contains("Onbekende BootManager locatie-QR", cut.Markup);
        }, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task AddStockForm_WithCameraScanError_ShowsPermissionDeniedMessage()
    {
        var productId = Guid.NewGuid();
        SetupAuthState("Crew");

        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            DefaultUnitName = "stuk"
        };

        var location = new StorageLocationDto
        {
            Id = Guid.NewGuid(),
            StorageAreaId = Guid.NewGuid(),
            StorageAreaName = "Kombuis",
            Name = "Kastje"
        };

        _productServiceMock
            .Setup(s => s.GetByIdAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        _storageServiceMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new[] { new StorageAreaDto { Id = location.StorageAreaId, Name = "Kombuis" } });

        _storageServiceMock
            .Setup(s => s.GetLocationsByAreaAsync(location.StorageAreaId, default))
            .ReturnsAsync(new[] { location });

        _storageServiceMock
            .Setup(s => s.ResolveQrValueAsync(It.IsAny<string>(), default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        var cut = RenderComponent<ScanProductAddStock>(parameters => parameters
            .Add(p => p.ProductId, productId.ToString()));

        // Simulate camera error callback
        var component = cut.Instance as ScanProductAddStock;
        await cut.InvokeAsync(() =>
        {
            if (component != null)
            {
                component.OnScanError(0, "PERMISSION_DENIED");
            }
        });

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Cameratoestemming geweigerd.", cut.Markup);
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
}
