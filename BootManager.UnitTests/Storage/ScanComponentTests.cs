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
using Moq;
using System.Security.Claims;

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
    public async Task KnownProductCode_StartsInventoryFlow()
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

        _storageMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new List<StorageAreaDto>().AsReadOnly());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(productCode, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetMostRecentStockForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<StockDto>.NotFound());

        _stockServiceMock
            .Setup(s => s.GetAlternativeLocationsForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(productCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Product inruimen", cut.Markup, StringComparison.OrdinalIgnoreCase));
        Assert.Contains("TestProduct", cut.Markup);
    }

    [Fact]
    public async Task KnownProductCode_WithSuggestedLocation_ShowsLocation()
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

        var suggestedLocation = new StockDto
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
            .Setup(s => s.GetMostRecentStockForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(suggestedLocation));

        _stockServiceMock
            .Setup(s => s.GetAlternativeLocationsForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(productCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
            Assert.Contains("Kombuis - Kast", cut.Markup));
        Assert.Contains("Voorgestelde locatie", cut.Markup);
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

    private void SetupAuthState(string role)
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, role) }, "test");
        var principal = new ClaimsPrincipal(identity);
        var authStateMock = new Mock<AuthenticationStateProvider>();
        authStateMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(principal));
        Services.AddScoped(_ => authStateMock.Object);
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
    public async Task KnownProductCode_WithSuggestedLocation_ShowsManualSelectionOption()
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

        var suggestedLocation = new StockDto
        {
            StorageLocationId = locationId,
            StorageAreaName = "Kombuis",
            StorageLocationName = "Kast",
            Quantity = 5
        };

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(productCode, default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _storageMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new List<StorageAreaDto>().AsReadOnly());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(productCode, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetMostRecentStockForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(suggestedLocation));

        _stockServiceMock
            .Setup(s => s.GetAlternativeLocationsForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(productCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Voorgestelde locatie", cut.Markup);
            var buttons = cut.FindAll("button");
            Assert.NotNull(buttons.FirstOrDefault(b => b.TextContent.Contains("Handmatig selecteren")));
            Assert.NotNull(buttons.FirstOrDefault(b => b.TextContent.Contains("Of scan locatie-QR")));
        });
    }

    [Fact]
    public async Task KnownProductCode_WithAlternativeLocations_ShowsManualSelectionOption()
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

        var alternativeLocation = new StockDto
        {
            StorageLocationId = Guid.NewGuid(),
            StorageAreaName = "Pantry",
            StorageLocationName = "Plank",
            Quantity = 3
        };

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(productCode, default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _storageMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new List<StorageAreaDto>().AsReadOnly());

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(productCode, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetMostRecentStockForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<StockDto>.NotFound());

        _stockServiceMock
            .Setup(s => s.GetAlternativeLocationsForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { alternativeLocation }.AsReadOnly()));

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(productCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Alternatieve locaties", cut.Markup);
            var buttons = cut.FindAll("button");
            Assert.NotNull(buttons.FirstOrDefault(b => b.TextContent.Contains("Handmatig selecteren")));
        });
    }

    [Fact]
    public async Task ManualSelectionWithSuggestedLocation_LoadsFullLocationList()
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

        var suggestedLocation = new StockDto
        {
            StorageLocationId = locationId,
            StorageAreaName = "Kombuis",
            StorageLocationName = "Kast",
            Quantity = 5
        };

        var area1 = new StorageAreaDto { Id = Guid.NewGuid(), Name = "Kombuis" };
        var area2 = new StorageAreaDto { Id = Guid.NewGuid(), Name = "Pantry" };

        var loc1 = new StorageLocationDto { Id = Guid.NewGuid(), Name = "Kast", StorageAreaName = "Kombuis" };
        var loc2 = new StorageLocationDto { Id = Guid.NewGuid(), Name = "Plank", StorageAreaName = "Pantry" };

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(productCode, default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _storageMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new List<StorageAreaDto> { area1, area2 }.AsReadOnly());

        _storageMock
            .Setup(s => s.GetLocationsByAreaAsync(area1.Id, default))
            .ReturnsAsync(new List<StorageLocationDto> { loc1 });

        _storageMock
            .Setup(s => s.GetLocationsByAreaAsync(area2.Id, default))
            .ReturnsAsync(new List<StorageLocationDto> { loc2 });

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(productCode, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetMostRecentStockForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<StockDto>.Ok(suggestedLocation));

        _stockServiceMock
            .Setup(s => s.GetAlternativeLocationsForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(productCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() => Assert.Contains("Voorgestelde locatie", cut.Markup));

        await cut.InvokeAsync(() =>
            cut.FindAll("button").Single(b => b.TextContent.Contains("Handmatig selecteren")).Click());

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Kast", cut.Markup);
            Assert.Contains("Plank", cut.Markup);
        });
    }

    [Fact]
    public async Task ManualSelectionWithAlternativeLocations_LoadsFullLocationList()
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

        var alternativeLocation = new StockDto
        {
            StorageLocationId = Guid.NewGuid(),
            StorageAreaName = "Pantry",
            StorageLocationName = "Plank",
            Quantity = 3
        };

        var area1 = new StorageAreaDto { Id = Guid.NewGuid(), Name = "Kombuis" };
        var area2 = new StorageAreaDto { Id = Guid.NewGuid(), Name = "Pantry" };

        var loc1 = new StorageLocationDto { Id = Guid.NewGuid(), Name = "Kast", StorageAreaName = "Kombuis" };
        var loc2 = new StorageLocationDto { Id = Guid.NewGuid(), Name = "Plank", StorageAreaName = "Pantry" };

        _storageMock
            .Setup(s => s.ResolveQrValueAsync(productCode, default))
            .ReturnsAsync(QrResolutionResult.Invalid());

        _storageMock
            .Setup(s => s.GetAllAreasAsync(default))
            .ReturnsAsync(new List<StorageAreaDto> { area1, area2 }.AsReadOnly());

        _storageMock
            .Setup(s => s.GetLocationsByAreaAsync(area1.Id, default))
            .ReturnsAsync(new List<StorageLocationDto> { loc1 });

        _storageMock
            .Setup(s => s.GetLocationsByAreaAsync(area2.Id, default))
            .ReturnsAsync(new List<StorageLocationDto> { loc2 });

        _productServiceMock
            .Setup(s => s.GetByCodeValueAsync(productCode, default))
            .ReturnsAsync(InventoryOperationResult<ProductDto>.Ok(product));

        _stockServiceMock
            .Setup(s => s.GetMostRecentStockForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<StockDto>.NotFound());

        _stockServiceMock
            .Setup(s => s.GetAlternativeLocationsForProductAsync(productId, default))
            .ReturnsAsync(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(
                new List<StockDto> { alternativeLocation }.AsReadOnly()));

        var cut = RenderComponent<Scan>();

        await cut.InvokeAsync(() =>
        {
            cut.Find("input[placeholder='Voer barcode of QR-waarde in…']").Input(productCode);
            cut.FindAll("button").Single(b => b.TextContent.Contains("Toepassen")).Click();
        });

        cut.WaitForAssertion(() => Assert.Contains("Alternatieve locaties", cut.Markup));

        await cut.InvokeAsync(() =>
            cut.FindAll("button").Single(b => b.TextContent.Contains("Handmatig selecteren")).Click());

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Kast", cut.Markup);
            Assert.Contains("Plank", cut.Markup);
        });
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

    private void SetupScannerJs()
    {
        var module = JSInterop.SetupModule("./js/barcodeScanner.js");
        module.Setup<bool>("checkSecureContext").SetResult(true);
        module.SetupVoid("stopScan");
        module.SetupVoid("dispose");
        module.SetupVoid("startScan", _ => true);
    }
}
