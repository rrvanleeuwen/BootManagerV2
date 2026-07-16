using Bunit;
using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Services;
using BootManager.Web.Components.Pages.Inventory;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BootManager.UnitTests.Inventory;

/// <summary>
/// Real bUnit component tests voor InventoryImport.razor: CSV-upload toont de mappingstap,
/// en het bevestigen voert de import uit met de ingevulde mappings.
/// </summary>
public class InventoryImportComponentTests : TestContext
{
    private readonly Mock<IInventoryImportService> _importMock = new();
    private readonly Mock<IStorageService> _storageMock = new();

    public InventoryImportComponentTests()
    {
        Services.AddScoped<IInventoryImportService>(_ => _importMock.Object);
        Services.AddScoped<IStorageService>(_ => _storageMock.Object);
        _storageMock.Setup(s => s.GetAllAreasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StorageAreaDto>());
        _storageMock.Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StorageLocationOverviewDto>());
    }

    private void SetupExistingLocations(params (string Area, string Location)[] locations)
    {
        var overview = locations
            .Select(l => new StorageLocationOverviewDto
            {
                Id = Guid.NewGuid(),
                AreaName = l.Area,
                LocationName = l.Location
            })
            .ToList();
        _storageMock.Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(overview);
    }

    private void SetupParse(params string[] distinctLocations)
    {
        var parseResult = new InventoryImportParseResult
        {
            Success = true,
            Rows = distinctLocations.Select(loc => new InventoryImportRowDto
            {
                Quantity = 1m,
                Unit = "stuk",
                ProductName = "Testproduct",
                SourceLocation = loc
            }).ToList(),
            DistinctSourceLocations = distinctLocations.ToList()
        };
        _importMock.Setup(s => s.ParseCsv(It.IsAny<string>())).Returns(parseResult);
    }

    [Fact]
    public void Upload_ShowsMappingStepWithDistinctLocations()
    {
        SetupParse("Salonbank, rugleuning");

        var cut = RenderComponent<InventoryImport>();

        cut.FindComponent<InputFile>().UploadFiles(
            InputFileContent.CreateFromText("csv-inhoud", "voorraad.csv"));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Locaties koppelen", cut.Markup);
            Assert.Contains("Salonbank, rugleuning", cut.Markup);
        });

        // Destructieve waarschuwing zichtbaar in de mappingstap.
        Assert.Contains("worden", cut.Markup);
        Assert.Contains("verwijderd", cut.Markup);
    }

    [Fact]
    public void Confirm_ExecutesImportWithFilledMappings()
    {
        SetupParse("Salonbank, rugleuning");
        _importMock.Setup(s => s.ExecuteImportAsync(
                It.IsAny<IReadOnlyList<InventoryImportRowDto>>(),
                It.IsAny<IReadOnlyList<InventoryLocationMappingDto>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryImportExecutionResult.Ok());

        var cut = RenderComponent<InventoryImport>();

        cut.FindComponent<InputFile>().UploadFiles(
            InputFileContent.CreateFromText("csv-inhoud", "voorraad.csv"));

        cut.WaitForAssertion(() => Assert.Contains("Locaties koppelen", cut.Markup));

        // Vul het gebied in (locatie is voorgevuld met de bronlocatie).
        cut.Find("input[placeholder='Gebiedsnaam']").Input("Salon");

        // Bevestig de destructieve actie.
        cut.Find("#confirm-destructive").Change(true);

        cut.FindAll("button").First(b => b.TextContent.Contains("Import uitvoeren")).Click();

        cut.WaitForAssertion(() => Assert.Contains("Import geslaagd", cut.Markup));

        _importMock.Verify(s => s.ExecuteImportAsync(
            It.IsAny<IReadOnlyList<InventoryImportRowDto>>(),
            It.Is<IReadOnlyList<InventoryLocationMappingDto>>(m =>
                m.Count == 1
                && m[0].SourceLocation == "Salonbank, rugleuning"
                && m[0].AreaName == "Salon"
                && m[0].LocationName == "Salonbank, rugleuning"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Mapping_ExistingLocationsSelectable_AndReactToChosenArea()
    {
        SetupParse("Salonbank, rugleuning");
        SetupExistingLocations(
            ("Kombuis", "Kruidenkast"),
            ("Salon", "Rugleuning"));

        var cut = RenderComponent<InventoryImport>();

        cut.FindComponent<InputFile>().UploadFiles(
            InputFileContent.CreateFromText("csv-inhoud", "voorraad.csv"));

        cut.WaitForAssertion(() => Assert.Contains("Locaties koppelen", cut.Markup));

        // Kies gebied Kombuis: alleen locaties uit Kombuis zijn selecteerbaar.
        cut.Find("input[placeholder='Gebiedsnaam']").Input("Kombuis");

        var kombuisOptions = cut.FindAll("#existing-locations-0 option")
            .Select(o => o.GetAttribute("value"))
            .ToList();
        Assert.Contains("Kruidenkast", kombuisOptions);
        Assert.DoesNotContain("Rugleuning", kombuisOptions);

        // Wissel naar gebied Salon: de keuzelijst reageert op het gekozen gebied.
        cut.Find("input[placeholder='Gebiedsnaam']").Input("Salon");

        var salonOptions = cut.FindAll("#existing-locations-0 option")
            .Select(o => o.GetAttribute("value"))
            .ToList();
        Assert.Contains("Rugleuning", salonOptions);
        Assert.DoesNotContain("Kruidenkast", salonOptions);
    }

    [Fact]
    public void Mapping_AllowsFreeTextNewLocation_NotInExistingList()
    {
        SetupParse("Salonbank, rugleuning");
        SetupExistingLocations(("Kombuis", "Kruidenkast"));
        _importMock.Setup(s => s.ExecuteImportAsync(
                It.IsAny<IReadOnlyList<InventoryImportRowDto>>(),
                It.IsAny<IReadOnlyList<InventoryLocationMappingDto>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(InventoryImportExecutionResult.Ok());

        var cut = RenderComponent<InventoryImport>();

        cut.FindComponent<InputFile>().UploadFiles(
            InputFileContent.CreateFromText("csv-inhoud", "voorraad.csv"));

        cut.WaitForAssertion(() => Assert.Contains("Locaties koppelen", cut.Markup));

        cut.Find("input[placeholder='Gebiedsnaam']").Input("Kombuis");
        // Typ een nieuwe locatienaam die niet in de bestaande keuzelijst staat.
        cut.Find("input[placeholder='Bestaande locatie kiezen of nieuwe naam typen']").Input("Nieuwe plank");
        cut.Find("#confirm-destructive").Change(true);

        cut.FindAll("button").First(b => b.TextContent.Contains("Import uitvoeren")).Click();

        cut.WaitForAssertion(() => Assert.Contains("Import geslaagd", cut.Markup));

        _importMock.Verify(s => s.ExecuteImportAsync(
            It.IsAny<IReadOnlyList<InventoryImportRowDto>>(),
            It.Is<IReadOnlyList<InventoryLocationMappingDto>>(m =>
                m.Count == 1
                && m[0].AreaName == "Kombuis"
                && m[0].LocationName == "Nieuwe plank"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void ImportButton_DisabledUntilConfirmedAndMapped()
    {
        SetupParse("Salonbank, rugleuning");

        var cut = RenderComponent<InventoryImport>();

        cut.FindComponent<InputFile>().UploadFiles(
            InputFileContent.CreateFromText("csv-inhoud", "voorraad.csv"));

        cut.WaitForAssertion(() => Assert.Contains("Locaties koppelen", cut.Markup));

        var importButton = cut.FindAll("button").First(b => b.TextContent.Contains("Import uitvoeren"));
        Assert.True(importButton.HasAttribute("disabled"));

        // Alleen gebied invullen is onvoldoende zonder bevestiging.
        cut.Find("input[placeholder='Gebiedsnaam']").Input("Salon");
        importButton = cut.FindAll("button").First(b => b.TextContent.Contains("Import uitvoeren"));
        Assert.True(importButton.HasAttribute("disabled"));

        // Na bevestiging wordt de knop actief.
        cut.Find("#confirm-destructive").Change(true);
        importButton = cut.FindAll("button").First(b => b.TextContent.Contains("Import uitvoeren"));
        Assert.False(importButton.HasAttribute("disabled"));
    }
}
