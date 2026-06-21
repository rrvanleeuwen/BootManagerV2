using Bunit;
using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Results;
using BootManager.Application.Storage.Services;
using BootManager.Core.Entities;
using BootManager.Web.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;
using System.Reflection;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Real bUnit tests for Scan.razor QR behavior and inventory flow.
/// Covers navigation, product code detection, and scan-driven inventory flow.
/// </summary>
public class ScanComponentTests : TestContext
{
    private readonly Mock<IStorageService> _storageMock = new();
    private readonly Mock<IProductService> _productServiceMock = new();
    private readonly Mock<IStockService> _stockServiceMock = new();
    private readonly Mock<IUnitService> _unitServiceMock = new();

    public ScanComponentTests()
    {
        Services.AddScoped<IStorageService>(_ => _storageMock.Object);
        Services.AddScoped<IProductService>(_ => _productServiceMock.Object);
        Services.AddScoped<IStockService>(_ => _stockServiceMock.Object);
        Services.AddScoped<IUnitService>(_ => _unitServiceMock.Object);
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

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(It.IsAny<string>(), default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.NotFound());

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

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(It.IsAny<string>(), default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.NotFound());

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

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(value, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.NotFound());

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

    [Fact]
    public async Task KnownProductCode_WithNoActiveStock_ShowsNoActiveStockMessage()
    {
        var productId = Guid.NewGuid();
        var productCode = "ABC123";
        SetupAuthState("Owner");

        var product = new ProductDto
        {
            Id = productId,
            Name = "TestProduct",
            DefaultUnitName = "stuk",
            Code = new ProductCodeDto { Value = productCode }
        };

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(productCode, default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(productCode, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<StockDto>.NotFound());

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(productCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Geen actieve voorraad", cut.Markup, StringComparison.OrdinalIgnoreCase));
        Assert.Contains("TestProduct", cut.Markup);
    }

    [Fact]
    public async Task KnownProductCode_WithOneActiveLocation_NavigatesDirectlyToLocation()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var productCode = "ABC123";
        SetupAuthState("Owner");

        var product = new ProductDto
        {
            Id = productId,
            Name = "TestProduct",
            DefaultUnitName = "stuk"
        };

        var activeLocation = new StockDto
        {
            StorageLocationId = locationId,
            StorageAreaName = "Kombuis",
            StorageLocationName = "Kast",
            Quantity = 5
        };

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(productCode, default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(productCode, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { activeLocation }.AsReadOnly()));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(productCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.EndsWith($"/storage/locations/{locationId}", navigation.Uri));
    }

    [Fact]
    public async Task LocationQr_DoesNotStartInventoryFlow_StaysNormal()
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

        cut.WaitForAssertion(() =>
            Assert.EndsWith($"/storage/locations/{locationId}", navigation.Uri));

        Assert.DoesNotContain("Product inruimen", cut.Markup);
    }

    [Fact]
    public async Task UnknownProductCode_ShowsThreeChoices()
    {
        var unknownCode = "XYZ999";
        SetupAuthState("Owner");

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(unknownCode, default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(unknownCode, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.NotFound());

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(unknownCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Onbekende productcode", cut.Markup);
            var buttons = cut.FindAll("button");
            Assert.NotNull(buttons.FirstOrDefault(b => b.TextContent.Contains("Nieuw product")));
            Assert.NotNull(buttons.FirstOrDefault(b => b.TextContent.Contains("Code koppelen")));
            Assert.NotNull(buttons.FirstOrDefault(b => b.TextContent.Contains("Annuleren")));
        });
    }

    [Fact]
    public async Task UnknownProductCode_NewProductFlow_StartsWithCodePrefilled()
    {
        var unknownCode = "ABC789";
        SetupAuthState("Owner");

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(unknownCode, default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(unknownCode, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.NotFound());

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(unknownCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Nieuw product"))));

        await cut.InvokeAsync(() =>
            cut.FindAll("button").Single(b => b.TextContent.Contains("Nieuw product")).Click());

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Nieuw product aanmaken", cut.Markup);
            Assert.Contains(unknownCode, cut.Markup);
        });
    }

    [Fact]
    public async Task UnknownProductCode_NewProductFlow_CodeIsEditableInputField()
    {
        var scannedCode = "ABC789";
        var editedCode = "XYZ999";
        var productName = "TestProduct";
        var productId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        SetupAuthState("Owner");

        var createdProduct = new ProductDto
        {
            Id = productId,
            Name = productName,
            DefaultUnitName = "stuk",
            Code = new ProductCodeDto { Value = editedCode }
        };

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(scannedCode, default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _storageMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new List<StorageAreaDto>().AsReadOnly());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(scannedCode, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.NotFound());

        _productServiceMock
            .Setup(s => s.GetAllAsync(default))
            .ReturnsAsync(new List<ProductDto>().AsReadOnly());

        _productServiceMock
            .Setup(s => s.CreateAsync(productName, null, unitId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(createdProduct));

        var addCodeCalls = new List<(Guid productId, string code, string type)>();
        _productServiceMock
            .Setup(s => s.AddCodeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, string, string, CancellationToken>((pid, code, type, ct) =>
                addCodeCalls.Add((pid, code, type)))
            .ReturnsAsync(InventoryOperationResult<ProductCodeDto>.Ok(new ProductCodeDto { Value = editedCode }));

        _stockServiceMock
            .Setup(s => s.GetMostRecentStockForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<StockDto>.NotFound());

        _stockServiceMock
            .Setup(s => s.GetAlternativeLocationsForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        _unitServiceMock
            .Setup(s => s.GetAllAsync(default))
            .ReturnsAsync(new List<UnitDto> { new UnitDto { Id = unitId, Name = "stuk" } }.AsReadOnly());

        var cut = RenderComponent<Scan>();

        // Scan unknown code to trigger flow
        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(scannedCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Nieuw product"))));

        // Click "Nieuw product"
        await cut.InvokeAsync(() =>
            cut.FindAll("button").Single(b => b.TextContent.Contains("Nieuw product")).Click());

        cut.WaitForAssertion(() =>
            Assert.Contains("Nieuw product aanmaken", cut.Markup));

        // Fill in product name
        await cut.InvokeAsync(() =>
        {
            var inputs = cut.FindAll("input[type='text']");
            var productNameInput = inputs.FirstOrDefault(i => i.GetAttribute("placeholder") == "Voer productnaam in");
            Assert.NotNull(productNameInput);
            productNameInput.Change(productName);
        });

        // Verify code input field exists and is prefilled with scanned code
        var codeInputs = cut.FindAll("input[placeholder='Productcode']");
        Assert.Single(codeInputs);
        var codeInput = codeInputs[0];
        Assert.Equal(scannedCode, codeInput.GetAttribute("value"));

        // Edit the code field to a different value using Change event
        await cut.InvokeAsync(() =>
        {
            codeInput.Change(editedCode);
        });

        // Verify the edited value is in the field
        codeInput = cut.FindAll("input[placeholder='Productcode']")[0];
        Assert.Equal(editedCode, codeInput.GetAttribute("value"));

        // Select a unit
        await cut.InvokeAsync(() =>
        {
            var select = cut.Find("select");
            select.Change(unitId.ToString());
        });

        // Click "Aanmaken en doorgaan"
        await cut.InvokeAsync(() =>
            cut.FindAll("button").Single(b => b.TextContent.Contains("Aanmaken en doorgaan")).Click());

        // Verify flow continues to location choosing, which proves the edited code was accepted
        cut.WaitForAssertion(() =>
            Assert.Contains("Product inruimen", cut.Markup, StringComparison.OrdinalIgnoreCase));

        // Verify AddCodeAsync was called with the edited code, not the scanned code
        Assert.NotEmpty(addCodeCalls);
        var lastCall = addCodeCalls.Last();
        Assert.Equal(editedCode, lastCall.code);
    }

    [Fact]
    public async Task KnownProductCode_WithMultipleActiveLocations_ShowsLocationListWithoutNavigating()
    {
        var productId = Guid.NewGuid();
        var productCode = "ABC123";
        SetupAuthState("Owner");

        var product = new ProductDto
        {
            Id = productId,
            Name = "TestProduct",
            DefaultUnitName = "stuk"
        };

        var activeLocation1 = new StockDto
        {
            StorageLocationId = Guid.NewGuid(),
            StorageAreaName = "Kombuis",
            StorageLocationName = "Kast",
            Quantity = 5,
            DefaultUnitName = "stuk"
        };

        var activeLocation2 = new StockDto
        {
            StorageLocationId = Guid.NewGuid(),
            StorageAreaName = "Pantry",
            StorageLocationName = "Plank",
            Quantity = 3,
            DefaultUnitName = "stuk"
        };

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(productCode, default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(productCode, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { activeLocation1, activeLocation2 }.AsReadOnly()));

        var navigation = Services.GetRequiredService<NavigationManager>();
        var initialUri = navigation.Uri;
        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(productCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Product gevonden op meerdere locaties", cut.Markup, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Kombuis - Kast", cut.Markup);
            Assert.Contains("Pantry - Plank", cut.Markup);
        });

        Assert.Equal(initialUri, navigation.Uri);
    }

    [Fact]
    public async Task KnownLocationQr_NavigatesDirectly_StillWorks()
    {
        var token = "a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5";
        var qrValue = $"bootmanager:location:{token}";
        var locationId = Guid.NewGuid();
        SetupAuthState("Owner");

        var navigation = Services.GetRequiredService<NavigationManager>();

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(qrValue, default))
            .ReturnsAsync(QrResolutionResult.Linked(locationId));

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(qrValue);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() => Assert.EndsWith($"/storage/locations/{locationId}", navigation.Uri));
    }

    [Fact]
    public async Task UnknownBootManagerQr_Owner_StaysOnPageBeforeClickingLinkButton()
    {
        var token = "a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5";
        var qrValue = $"bootmanager:location:{token}";
        SetupAuthState("Owner");

        var navigation = Services.GetRequiredService<NavigationManager>();
        var initialUri = navigation.Uri;

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(qrValue, default))
            .ReturnsAsync(QrResolutionResult.Unknown(token));

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(It.IsAny<string>(), default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.NotFound());

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(qrValue);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Onbekende BootManager locatie-QR", cut.Markup));

        var afterScanUri = navigation.Uri;
        Assert.Equal(initialUri, afterScanUri);
    }

    [Fact]
    public async Task UnknownBootManagerQr_Crew_StaysOnPageWithoutButton()
    {
        var token = "a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5";
        var qrValue = $"bootmanager:location:{token}";
        SetupAuthState("Crew");

        var navigation = Services.GetRequiredService<NavigationManager>();
        var initialUri = navigation.Uri;

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(qrValue, default))
            .ReturnsAsync(QrResolutionResult.Unknown(token));

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(It.IsAny<string>(), default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.NotFound());

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(qrValue);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("beheerder", cut.Markup, StringComparison.OrdinalIgnoreCase));

        var afterScanUri = navigation.Uri;
        Assert.Equal(initialUri, afterScanUri);
        Assert.Empty(cut.FindAll("button").Where(b => b.TextContent.Contains("Koppelen")));
    }

    [Fact]
    public async Task UnknownProductCode_NewProductFlow_UnitSelectionRequired()
    {
        var unknownCode = "ABC789";
        SetupAuthState("Owner");

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(unknownCode, default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(unknownCode, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.NotFound());

        var unit1 = new UnitDto { Id = Guid.NewGuid(), Name = "stuk" };
        var unit2 = new UnitDto { Id = Guid.NewGuid(), Name = "kg" };

        _unitServiceMock
            .Setup(s => s.GetAllAsync(default))
            .ReturnsAsync(new List<UnitDto> { unit1, unit2 }.AsReadOnly());

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(unknownCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Nieuw product"))));

        await cut.InvokeAsync(() =>
            cut.FindAll("button").Single(b => b.TextContent.Contains("Nieuw product")).Click());

        cut.WaitForAssertion(() =>
        {
            var selects = cut.FindAll("select");
            Assert.NotEmpty(selects);
            var unitSelect = selects.FirstOrDefault(s => s.GetAttribute("value") == "");
            Assert.NotNull(unitSelect);
        });

        var options = cut.FindAll("option");
        Assert.Contains(options, o => o.TextContent.Contains("stuk"));
        Assert.Contains(options, o => o.TextContent.Contains("kg"));
    }

    [Fact]
    public async Task UnknownProductCode_NewProductFlow_ButtonDisabledWithoutUnit()
    {
        var unknownCode = "ABC789";
        var productName = "TestProduct";
        SetupAuthState("Owner");

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(unknownCode, default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(unknownCode, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.NotFound());

        var unit1 = new UnitDto { Id = Guid.NewGuid(), Name = "stuk" };

        _unitServiceMock
            .Setup(s => s.GetAllAsync(default))
            .ReturnsAsync(new List<UnitDto> { unit1 }.AsReadOnly());

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(unknownCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Nieuw product"))));

        await cut.InvokeAsync(() =>
            cut.FindAll("button").Single(b => b.TextContent.Contains("Nieuw product")).Click());

        cut.WaitForAssertion(() =>
            Assert.Contains("Nieuw product aanmaken", cut.Markup));

        // Fill in product name and code but not unit
        await cut.InvokeAsync(() =>
        {
            var inputs = cut.FindAll("input[type='text']");
            var productNameInput = inputs.FirstOrDefault(i => i.GetAttribute("placeholder") == "Voer productnaam in");
            Assert.NotNull(productNameInput);
            productNameInput.Change(productName);

            var codeInputs = cut.FindAll("input[placeholder='Productcode']");
            codeInputs[0].Change(unknownCode);
        });

        // Verify button is disabled when unit is not selected
        var createButton = cut.FindAll("button").Single(b => b.TextContent.Contains("Aanmaken en doorgaan"));
        Assert.True(createButton.HasAttribute("disabled"));
    }

    [Fact]
    public async Task UnknownProductCode_NewProductFlow_PassesSelectedUnitToCreateAsync()
    {
        var scannedCode = "ABC789";
        var productName = "TestProduct";
        var productId = Guid.NewGuid();
        var selectedUnitId = Guid.NewGuid();
        SetupAuthState("Owner");

        var createdProduct = new ProductDto
        {
            Id = productId,
            Name = productName,
            DefaultUnitName = "stuk",
            Code = new ProductCodeDto { Value = scannedCode }
        };

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(scannedCode, default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _storageMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new List<StorageAreaDto>().AsReadOnly());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(scannedCode, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.NotFound());

        _productServiceMock
            .Setup(s => s.GetAllAsync(default))
            .ReturnsAsync(new List<ProductDto>().AsReadOnly());

        var createCalls = new List<(string name, string? desc, Guid unitId)>();
        _productServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), null, It.IsAny<CancellationToken>()))
            .Callback<string, string?, Guid, object?, CancellationToken>((name, desc, unitId, _, ct) =>
                createCalls.Add((name, desc, unitId)))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(createdProduct));

        _productServiceMock
            .Setup(s => s.AddCodeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<ProductCodeDto>.Ok(new ProductCodeDto { Value = scannedCode }));

        _stockServiceMock
            .Setup(s => s.GetMostRecentStockForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<StockDto>.NotFound());

        _stockServiceMock
            .Setup(s => s.GetAlternativeLocationsForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        var unit1 = new UnitDto { Id = selectedUnitId, Name = "stuk" };
        var unit2 = new UnitDto { Id = Guid.NewGuid(), Name = "kg" };

        _unitServiceMock
            .Setup(s => s.GetAllAsync(default))
            .ReturnsAsync(new List<UnitDto> { unit1, unit2 }.AsReadOnly());

        var cut = RenderComponent<Scan>();

        // Scan unknown code
        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(scannedCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.NotNull(cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Nieuw product"))));

        // Click "Nieuw product"
        await cut.InvokeAsync(() =>
            cut.FindAll("button").Single(b => b.TextContent.Contains("Nieuw product")).Click());

        cut.WaitForAssertion(() =>
            Assert.Contains("Nieuw product aanmaken", cut.Markup));

        // Fill in all fields including unit selection
        await cut.InvokeAsync(() =>
        {
            var inputs = cut.FindAll("input[type='text']");
            var productNameInput = inputs.FirstOrDefault(i => i.GetAttribute("placeholder") == "Voer productnaam in");
            Assert.NotNull(productNameInput);
            productNameInput.Change(productName);

            var codeInputs = cut.FindAll("input[placeholder='Productcode']");
            codeInputs[0].Change(scannedCode);

            var select = cut.Find("select");
            select.Change(selectedUnitId.ToString());
        });

        // Click "Aanmaken en doorgaan"
        await cut.InvokeAsync(() =>
            cut.FindAll("button").Single(b => b.TextContent.Contains("Aanmaken en doorgaan")).Click());

        // Verify flow continues to location choosing
        cut.WaitForAssertion(() =>
            Assert.Contains("Product inruimen", cut.Markup, StringComparison.OrdinalIgnoreCase));

        // Verify CreateAsync was called with the selected unit
        Assert.NotEmpty(createCalls);
        var lastCall = createCalls.Last();
        Assert.Equal(selectedUnitId, lastCall.unitId);
    }

    [Fact]
    public async Task KnownProductCode_WithNoActiveStock_OpensAddStockModal()
    {
        var productId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var productCode = "ABC123";
        SetupAuthState("Owner");

        var product = new ProductDto
        {
            Id = productId,
            Name = "TestProduct",
            DefaultUnitName = "stuk",
            Code = new ProductCodeDto { Value = productCode }
        };

        var mockLocations = new List<StorageLocationOverviewDto>
        {
            new() { Id = locationId, AreaName = "Kombuis", LocationName = "Kast" }
        };

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(productCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _storageMock
            .Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockLocations);

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(productCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.NotFound());

        var navigation = Services.GetRequiredService<NavigationManager>();
        var initialUri = navigation.Uri;
        var cut = RenderComponent<Scan>();

        // Act: Scan product code
        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(productCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        // Assert: Modal should appear - the key test is that instead of navigating,
        // we show the no-stock state first.
        cut.WaitForAssertion(() =>
            Assert.Contains("Geen actieve voorraad", cut.Markup, StringComparison.OrdinalIgnoreCase));

        Assert.Equal(initialUri, navigation.Uri);

        // Click "Voorraad toevoegen" from the no-stock state.
        await cut.InvokeAsync(() =>
        {
            var addStockButton = cut.FindAll("button")
                .First(b => b.TextContent.Trim().Equals("Voorraad toevoegen", StringComparison.Ordinal));
            addStockButton.Click();
        });

        // Assert: the new modal opens instead of navigating away.
        cut.WaitForAssertion(() =>
        {
            Assert.Contains("TestProduct", cut.Markup);
            Assert.Contains("Opslaglocatie *", cut.Markup);
            Assert.Contains("Hoeveelheid *", cut.Markup);
            Assert.Contains("Kombuis - Kast", cut.Markup);
        });

        Assert.Equal(initialUri, navigation.Uri);
    }

    [Fact]
    public async Task PhysicalMutationRoute_PreScannedProduct_StoresMutationAndReturnsToStart()
    {
        var initialProductId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const string initialCode = "INIT-001";

        SetupAuthState("Crew", userId);

        var initialProduct = new ProductDto
        {
            Id = initialProductId,
            Name = "Lege Productkaart",
            DefaultUnitName = "stuk",
            Code = new ProductCodeDto { Value = initialCode, Format = "EAN13" }
        };

        var suggestedLocation = new StockDto
        {
            ProductId = initialProductId,
            StorageLocationId = locationId,
            StorageAreaName = "Kombuis",
            StorageLocationName = "Bakboordkast",
            Quantity = 0,
            DefaultUnitName = "stuk"
        };

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(initialCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(initialProduct));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(initialProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));
        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(initialProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.NotFound());
        _stockServiceMock
            .Setup(s => s.GetMostRecentStockForProductAsync(initialProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(suggestedLocation));
        _stockServiceMock
            .Setup(s => s.GetAlternativeLocationsForProductAsync(initialProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));
        _stockServiceMock
            .Setup(s => s.GetStocksByLocationAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));
        _stockServiceMock
            .Setup(s => s.MutateStockAsync(
                initialProductId,
                locationId,
                StockMutationType.Correctie,
                2m,
                userId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult.Ok());

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(initialCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Geen actieve voorraad", cut.Markup, StringComparison.OrdinalIgnoreCase));

        await cut.InvokeAsync(() =>
        {
            cut.FindAll("button")
                .First(b => b.TextContent.Trim().Equals("Voorraadbijzonderheid", StringComparison.Ordinal))
                .Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Voorraadbijzonderheid: locatie selecteren", cut.Markup));

        await cut.InvokeAsync(() =>
        {
            cut.FindAll("button")
                .First(b => b.TextContent.Trim().Equals("Selecteren", StringComparison.Ordinal))
                .Click();
        });

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Voorraadbijzonderheid opslaan", cut.Markup);
            Assert.Contains("Lege Productkaart", cut.Markup);
        });

        await cut.InvokeAsync(() =>
        {
            cut.Find("select").Change("Correctie");
            cut.Find("input[type='number']").Change("2");
            cut.FindAll("button").First(b => b.TextContent.Trim().Equals("Opslaan", StringComparison.Ordinal)).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Voorraadbijzonderheid opgeslagen!", cut.Markup));

        _stockServiceMock.Verify(
            s => s.MutateStockAsync(
                initialProductId,
                locationId,
                StockMutationType.Correctie,
                2m,
                userId,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);

        await cut.InvokeAsync(() =>
        {
            cut.FindAll("button")
                .First(b => b.TextContent.Contains("Ja, nog een", StringComparison.Ordinal))
                .Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Voorraadbijzonderheid: locatie selecteren", cut.Markup));
    }

    [Fact]
    public async Task MutationFlow_WhenScannedProductExistsAtLocation_SkipsScanningAndGoesDirectlyToQuantity()
    {
        var scannedProductId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const string scannedCode = "PROD-001";

        SetupAuthState("Crew", userId);

        var scannedProduct = new ProductDto
        {
            Id = scannedProductId,
            Name = "TestProduct",
            DefaultUnitName = "stuk",
            Code = new ProductCodeDto { Value = scannedCode, Format = "EAN13" }
        };

        var location = new StockDto
        {
            StorageLocationId = locationId,
            StorageAreaName = "Area1",
            StorageLocationName = "Location1"
        };

        var stockAtLocation = new StockDto
        {
            ProductId = scannedProductId,
            StorageLocationId = locationId,
            ProductName = "TestProduct",
            StorageAreaName = "Area1",
            StorageLocationName = "Location1",
            Quantity = 10,
            DefaultUnitName = "stuk"
        };

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(scannedCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(scannedProduct));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(scannedProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(scannedProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.NotFound());

        _stockServiceMock
            .Setup(s => s.GetMostRecentStockForProductAsync(scannedProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(location));

        _stockServiceMock
            .Setup(s => s.GetAlternativeLocationsForProductAsync(scannedProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        // Key setup: the scanned product DOES exist at the suggested location
        _stockServiceMock
            .Setup(s => s.GetStocksByLocationAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { stockAtLocation }.AsReadOnly()));

        var cut = RenderComponent<Scan>();

        // Scan the product code
        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(scannedCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        // Assert: no active stock found, showing mutation flow option
        cut.WaitForAssertion(() =>
            Assert.Contains("Geen actieve voorraad", cut.Markup, StringComparison.OrdinalIgnoreCase));

        // Click "Voorraadbijzonderheid"
        await cut.InvokeAsync(() =>
        {
            cut.FindAll("button")
                .First(b => b.TextContent.Trim().Equals("Voorraadbijzonderheid", StringComparison.Ordinal))
                .Click();
        });

        // Assert: should show location selection
        cut.WaitForAssertion(() =>
            Assert.Contains("Voorraadbijzonderheid: locatie selecteren", cut.Markup));

        // Click the suggested location (which contains the scanned product)
        await cut.InvokeAsync(() =>
        {
            cut.FindAll("button")
                .First(b => b.TextContent.Trim().Equals("Selecteren", StringComparison.Ordinal))
                .Click();
        });

        // KEY ASSERTION: should skip product scanning and go directly to quantity entry
        // This verifies the fix: the scanned product is automatically reused without asking to scan again
        cut.WaitForAssertion(() =>
            Assert.Contains("Voorraadbijzonderheid opslaan", cut.Markup));

        // Verify the product info is shown (proving the product was found without rescanning)
        Assert.Contains("TestProduct", cut.Markup);
        Assert.Contains("Area1 - Location1", cut.Markup);

        // Should NOT show the "product scannen" state
        Assert.DoesNotContain("Voorraadbijzonderheid: product scannen", cut.Markup);
    }

    [Fact]
    public async Task MutationFlow_WhenScannedProductHasNoStockAtScannedLocation_GoesDirectlyToQuantityWithZeroStock()
    {
        var scannedProductId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const string scannedCode = "PROD-QR-001";
        const string locationQr = "bootmanager:location:abc123";

        SetupAuthState("Crew", userId);

        var scannedProduct = new ProductDto
        {
            Id = scannedProductId,
            Name = "TestProduct",
            DefaultUnitName = "stuk",
            Code = new ProductCodeDto { Value = scannedCode, Format = "EAN13" }
        };

        var suggestedLocation = new StockDto
        {
            StorageLocationId = locationId,
            StorageAreaName = "Area1",
            StorageLocationName = "Location1"
        };

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(scannedCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(QrResolutionResult.Invalid());
        _storageMock
            .Setup(s => s.ResolveQrValueAsync(locationQr, It.IsAny<CancellationToken>()))
            .ReturnsAsync(QrResolutionResult.Linked(locationId));
        _storageMock
            .Setup(s => s.GetLocationDetailAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(new StorageLocationDetailDto
            {
                Id = locationId,
                AreaName = "Area1",
                LocationName = "Location1"
            }));

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(scannedCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(scannedProduct));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(scannedProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));
        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(scannedProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.NotFound());
        _stockServiceMock
            .Setup(s => s.GetMostRecentStockForProductAsync(scannedProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(suggestedLocation));
        _stockServiceMock
            .Setup(s => s.GetAlternativeLocationsForProductAsync(scannedProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));
        _stockServiceMock
            .Setup(s => s.GetStocksByLocationAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(scannedCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Geen actieve voorraad", cut.Markup, StringComparison.OrdinalIgnoreCase));

        await cut.InvokeAsync(() =>
        {
            cut.FindAll("button")
                .First(b => b.TextContent.Trim().Equals("Voorraadbijzonderheid", StringComparison.Ordinal))
                .Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Voorraadbijzonderheid: locatie selecteren", cut.Markup));

        await cut.InvokeAsync(() =>
        {
            cut.FindAll("button")
                .First(b => b.TextContent.Contains("Of scan locatie-QR", StringComparison.Ordinal))
                .Click();
        });

        var requestId = (int)typeof(Scan)
            .GetField("_requestId", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(cut.Instance)!;

        await cut.InvokeAsync(() => cut.Instance.OnScanResult(requestId, locationQr, "QR_CODE"));

        cut.WaitForAssertion(() =>
            Assert.Contains("Voorraadbijzonderheid opslaan", cut.Markup));

        Assert.Contains("TestProduct", cut.Markup);
        Assert.Contains("Area1 - Location1", cut.Markup);
        Assert.Contains("Huidige hoeveelheid:", cut.Markup);
        Assert.Contains("0 stuk", cut.Markup);
        Assert.DoesNotContain("Voorraadbijzonderheid: product scannen", cut.Markup);
    }

    [Fact]
    public async Task MutationFlow_ContinueNewMutation_ResetsPreviousProductContext()
    {
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const string code1 = "PROD-001";
        const string code2 = "PROD-002";

        SetupAuthState("Crew", userId);

        var product1 = new ProductDto
        {
            Id = product1Id,
            Name = "Product1",
            DefaultUnitName = "stuk",
            Code = new ProductCodeDto { Value = code1, Format = "EAN13" }
        };

        var product2 = new ProductDto
        {
            Id = product2Id,
            Name = "Product2",
            DefaultUnitName = "stuk",
            Code = new ProductCodeDto { Value = code2, Format = "EAN13" }
        };

        var location = new StockDto
        {
            StorageLocationId = locationId,
            StorageAreaName = "Area1",
            StorageLocationName = "Location1"
        };

        var stock1 = new StockDto
        {
            ProductId = product1Id,
            StorageLocationId = locationId,
            ProductName = "Product1",
            StorageAreaName = "Area1",
            StorageLocationName = "Location1",
            Quantity = 10,
            DefaultUnitName = "stuk"
        };

        var stock2 = new StockDto
        {
            ProductId = product2Id,
            StorageLocationId = locationId,
            ProductName = "Product2",
            StorageAreaName = "Area1",
            StorageLocationName = "Location1",
            Quantity = 5,
            DefaultUnitName = "stuk"
        };

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(code1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product1));
        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(code2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product2));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(product1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        _stockServiceMock
            .Setup(s => s.GetActiveStocksByProductAsync(product2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        _stockServiceMock
            .Setup(s => s.GetExpectedLocationForProductAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.NotFound());

        _stockServiceMock
            .Setup(s => s.GetMostRecentStockForProductAsync(product1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(location));

        _stockServiceMock
            .Setup(s => s.GetMostRecentStockForProductAsync(product2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(location));

        _stockServiceMock
            .Setup(s => s.GetAlternativeLocationsForProductAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        // First mutation: product1 at location
        _stockServiceMock
            .Setup(s => s.GetStocksByLocationAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { stock1 }.AsReadOnly()));

        _stockServiceMock
            .Setup(s => s.MutateStockAsync(
                product1Id,
                locationId,
                StockMutationType.Verbruik,
                1m,
                userId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult.Ok());

        var cut = RenderComponent<Scan>();

        // Scan product1
        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(code1);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Geen actieve voorraad", cut.Markup, StringComparison.OrdinalIgnoreCase));

        // Start mutation flow
        await cut.InvokeAsync(() =>
        {
            cut.FindAll("button")
                .First(b => b.TextContent.Trim().Equals("Voorraadbijzonderheid", StringComparison.Ordinal))
                .Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Voorraadbijzonderheid: locatie selecteren", cut.Markup));

        // Select location
        await cut.InvokeAsync(() =>
        {
            cut.FindAll("button")
                .First(b => b.TextContent.Trim().Equals("Selecteren", StringComparison.Ordinal))
                .Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Voorraadbijzonderheid opslaan", cut.Markup));

        // Fill and save mutation
        await cut.InvokeAsync(() =>
        {
            cut.Find("select").Change("Verbruik");
            cut.Find("input[type='number']").Change("1");
            cut.FindAll("button").First(b => b.TextContent.Trim().Equals("Opslaan", StringComparison.Ordinal)).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Voorraadbijzonderheid opgeslagen!", cut.Markup));

        // Click "Ja, nog een"
        await cut.InvokeAsync(() =>
        {
            cut.FindAll("button")
                .First(b => b.TextContent.Contains("Ja, nog een", StringComparison.Ordinal))
                .Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Voorraadbijzonderheid: locatie selecteren", cut.Markup));

        // KEY TEST: Now setup mocks for product2 at location, which should NOT auto-match
        // because the scanned product context should have been reset
        _stockServiceMock
            .Setup(s => s.GetStocksByLocationAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { stock2 }.AsReadOnly()));

        // Select same location again
        await cut.InvokeAsync(() =>
        {
            cut.FindAll("button")
                .First(b => b.TextContent.Trim().Equals("Selecteren", StringComparison.Ordinal))
                .Click();
        });

        // Should now show product scanning (NOT direct entry), because old product context was cleared
        cut.WaitForAssertion(() =>
            Assert.Contains("Voorraadbijzonderheid: product scannen", cut.Markup, StringComparison.OrdinalIgnoreCase));
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

    private void SetupScannerJs()
    {
        var module = JSInterop.SetupModule("./js/barcodeScanner.js");
        module.Setup<bool>("checkSecureContext").SetResult(true);
        module.SetupVoid("stopScan");
        module.SetupVoid("dispose");
        module.SetupVoid("startScan", _ => true);
    }
}
