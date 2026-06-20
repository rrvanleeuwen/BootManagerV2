using Bunit;
using Bunit.TestDoubles;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Results;
using BootManager.Application.Storage.Services;
using BootManager.Web.Components.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Real bUnit component tests for StorageManagement Blazor component.
/// Tests the isolated location creation form tied to a single selected area.
/// </summary>
public class StorageManagementComponentTests : TestContext
{
    private readonly TestStorageService _testService = new();

    public StorageManagementComponentTests()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("owner");
        authContext.SetRoles("Owner");

        Services.AddScoped<IStorageService>(_ => _testService);
    }

    [Fact]
    public void InitialRender_NoLocationCreationModal_WhenNoAreaSelected()
    {
        // Arrange: Two areas, no locations
        _testService.Areas.Add(new() { Id = Guid.NewGuid(), Name = "Kombuis" });
        _testService.Areas.Add(new() { Id = Guid.NewGuid(), Name = "Salon" });

        // Act: Render component
        var cut = RenderComponent<StorageManagement>();

        // Assert: No location creation modal should be visible initially
        var createModal = cut.FindAll(".modal.show").FirstOrDefault(el => el.TextContent.Contains("Locatie toevoegen in"));
        Assert.Null(createModal);
    }

    [Fact]
    public void ClickAddLocationButton_ShowsModalForSelectedArea()
    {
        // Arrange: Kombuis area
        var kombuisId = Guid.NewGuid();
        _testService.Areas.Add(new() { Id = kombuisId, Name = "Kombuis" });

        // Act: Render and click "Locatie toevoegen" button
        var cut = RenderComponent<StorageManagement>();
        var addButton = cut.FindAll("button").First(b => b.TextContent == "Locatie toevoegen");
        addButton.Click();

        // Assert: Modal title shows "Kombuis"
        var modalTitle = cut.Find(".modal.show .modal-title");
        Assert.Contains("Kombuis", modalTitle.TextContent);

        // The former create card below the area list must not return.
        var creationCards = cut.FindAll(".card")
            .Where(card => card.TextContent.Contains("Locatie toevoegen in"));
        Assert.Empty(creationCards);
    }

    [Fact]
    public async Task FillAndSaveLocation_CallsServiceOnce_WithSelectedAreaId()
    {
        // Arrange
        var kombuisId = Guid.NewGuid();
        var salonId = Guid.NewGuid();
        _testService.Areas.Add(new() { Id = kombuisId, Name = "Kombuis" });
        _testService.Areas.Add(new() { Id = salonId, Name = "Salon" });
        _testService.CreateLocationHandler = (areaId, name, desc) =>
            StorageOperationResult<StorageLocationDto>.Ok(new()
            {
                Id = Guid.NewGuid(),
                StorageAreaId = areaId,
                Name = name,
                Description = desc
            });

        var cut = RenderComponent<StorageManagement>();

        // Act: Click button to show modal
        var addButton = cut.FindAll("button").First(b => b.TextContent == "Locatie toevoegen");
        await cut.InvokeAsync(() => addButton.Click());

        // Act: Fill modal inputs - find fresh inputs each time
        var inputs = cut.FindAll(".modal-body input[type='text']");
        await cut.InvokeAsync(() => inputs[0].Change("Kast 1"));

        inputs = cut.FindAll(".modal-body input[type='text']");
        await cut.InvokeAsync(() => inputs[1].Change("Beschrijving test"));

        // Act: Click save
        var saveButton = cut.FindAll("button").First(b => b.TextContent.Contains("Opslaan") && b.GetAttribute("class")?.Contains("btn-primary") == true);
        await cut.InvokeAsync(async () => saveButton.Click());

        // Assert: Service called once with Kombuis ID, not Salon
        Assert.Single(_testService.CreateLocationCalls);
        var call = _testService.CreateLocationCalls[0];
        Assert.Equal(kombuisId, call.AreaId);
        Assert.Equal("Kast 1", call.Name);
        Assert.Equal("Beschrijving test", call.Description);

        // A successful create closes the modal.
        var createModals = cut.FindAll(".modal.show")
            .Where(modal => modal.TextContent.Contains("Locatie toevoegen in"));
        Assert.Empty(createModals);
    }

    [Fact]
    public async Task SwitchAreas_ClearsModalInputAndError()
    {
        // Arrange: Two areas, Kombuis will error on create
        var kombuisId = Guid.NewGuid();
        var salonId = Guid.NewGuid();
        _testService.Areas.Add(new() { Id = kombuisId, Name = "Kombuis" });
        _testService.Areas.Add(new() { Id = salonId, Name = "Salon" });
        _testService.CreateLocationHandler = (_, _, _) =>
            StorageOperationResult<StorageLocationDto>.Error("Duplicaat in Kombuis");

        var cut = RenderComponent<StorageManagement>();

        // Act: Open Kombuis modal and fill it
        await cut.InvokeAsync(() =>
        {
            var addButtons = cut.FindAll("button").Where(b => b.TextContent == "Locatie toevoegen").ToList();
            addButtons[0].Click();
        });

        await cut.InvokeAsync(() =>
        {
            var inputs = cut.FindAll(".modal-body input[type='text']");
            inputs.FirstOrDefault(i => i.GetAttribute("placeholder")?.Contains("Locatienaam") == true)?.Change("Test Input");
        });

        // Act: Try to save - will error
        await cut.InvokeAsync(async () =>
        {
            var saveButton = cut.FindAll("button").First(b => b.TextContent.Contains("Opslaan") && b.GetAttribute("class")?.Contains("btn-primary") == true);
            saveButton.Click();
        });

        // Assert: Error is displayed exactly once
        var errors = cut.FindAll(".alert.alert-danger");
        Assert.Single(errors);
        Assert.Contains("Duplicaat in Kombuis", errors[0].TextContent);

        // Act: Switch to Salon modal
        await cut.InvokeAsync(() =>
        {
            var addButtons = cut.FindAll("button").Where(b => b.TextContent == "Locatie toevoegen").ToList();
            addButtons[1].Click();
        });

        // Assert: Modal shows Salon, inputs and error are cleared
        var modalTitle = cut.Find(".modal.show .modal-title");
        Assert.Contains("Salon", modalTitle.TextContent);

        var newInputs = cut.FindAll(".modal-body input[type='text']");
        Assert.All(newInputs, input => Assert.Empty(input.GetAttribute("value") ?? ""));

        // Assert: Error is gone
        var newErrors = cut.FindAll(".alert.alert-danger");
        Assert.Empty(newErrors);
    }

    [Fact]
    public void CancelButton_ClosesModalWithoutServiceCall()
    {
        // Arrange
        var kombuisId = Guid.NewGuid();
        _testService.Areas.Add(new() { Id = kombuisId, Name = "Kombuis" });

        var cut = RenderComponent<StorageManagement>();

        // Act: Open, fill, cancel
        var addButton = cut.FindAll("button").First(b => b.TextContent == "Locatie toevoegen");
        addButton.Click();

        var inputs = cut.FindAll(".modal-body input[type='text']");
        inputs[0]?.Change("Test");

        var cancelButton = cut.FindAll("button").First(b => b.TextContent == "Annuleren" && b.GetAttribute("class")?.Contains("btn-secondary") == true);
        cancelButton.Click();

        // Assert: No service call, modal closed
        Assert.Empty(_testService.CreateLocationCalls);
        var modal = cut.FindAll(".modal.show").FirstOrDefault();
        Assert.Null(modal);
    }

    [Fact]
    public async Task ServiceError_ShowsMessageInModal()
    {
        // Arrange
        var kombuisId = Guid.NewGuid();
        _testService.Areas.Add(new() { Id = kombuisId, Name = "Kombuis" });
        _testService.CreateLocationHandler = (_, _, _) =>
            StorageOperationResult<StorageLocationDto>.Error("Duplicaat locatienaam");

        var cut = RenderComponent<StorageManagement>();

        // Act: Click button to show modal
        await cut.InvokeAsync(() =>
        {
            var addButton = cut.FindAll("button").First(b => b.TextContent == "Locatie toevoegen");
            addButton.Click();
        });

        // Act: Fill and save
        await cut.InvokeAsync(() =>
        {
            var inputs = cut.FindAll(".modal-body input[type='text']");
            inputs[0]?.Change("Kast 1");
        });

        await cut.InvokeAsync(async () =>
        {
            var saveButton = cut.FindAll("button").First(b => b.TextContent.Contains("Opslaan") && b.GetAttribute("class")?.Contains("btn-primary") == true);
            saveButton.Click();
        });

        // Assert: Error displayed exactly once in modal
        var errorAlerts = cut.FindAll(".modal-body .alert.alert-danger");
        Assert.Single(errorAlerts);
        Assert.Contains("Duplicaat", errorAlerts[0].TextContent);
    }

    [Fact]
    public void CloseButton_ClosesModalWithoutServiceCall()
    {
        // Arrange
        var kombuisId = Guid.NewGuid();
        _testService.Areas.Add(new() { Id = kombuisId, Name = "Kombuis" });

        var cut = RenderComponent<StorageManagement>();

        // Act: Open modal
        var addButton = cut.FindAll("button").First(b => b.TextContent == "Locatie toevoegen");
        addButton.Click();

        // Act: Fill and click close button
        var inputs = cut.FindAll(".modal-body input[type='text']");
        inputs[0]?.Change("Test");

        var closeButton = cut.FindAll("button").First(b => b.GetAttribute("class")?.Contains("btn-close") == true);
        closeButton.Click();

        // Assert: No service call, modal closed
        Assert.Empty(_testService.CreateLocationCalls);
        var modal = cut.FindAll(".modal.show").FirstOrDefault();
        Assert.Null(modal);
    }

    [Fact]
    public void LocationLink_HasCorrectUrlAndNoTargetBlank()
    {
        // Arrange
        var kombuisId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        _testService.Areas.Add(new() { Id = kombuisId, Name = "Kombuis" });
        _testService.Locations.Add(new() { Id = locationId, StorageAreaId = kombuisId, Name = "Kast 1", Description = null });

        // Act: Render component
        var cut = RenderComponent<StorageManagement>();

        // Assert: Link has correct href and no target="_blank"
        var link = cut.Find("a[href*='/storage/locations/']");
        Assert.NotNull(link);
        Assert.Equal($"/storage/locations/{locationId}", link.GetAttribute("href"));
        Assert.Null(link.GetAttribute("target"));
    }

    [Fact]
    public async Task FailedUpdate_ShowsOperationError()
    {
        // Arrange
        var kombuisId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        _testService.Areas.Add(new() { Id = kombuisId, Name = "Kombuis" });
        _testService.Locations.Add(new() { Id = locationId, StorageAreaId = kombuisId, Name = "Kast", Description = null });
        _testService.UpdateLocationHandler = (_, _, _) =>
            StorageOperationResult<StorageLocationDto>.Error("Update error");

        var cut = RenderComponent<StorageManagement>();

        // Act: Click edit button
        await cut.InvokeAsync(() =>
        {
            var editButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent == "Bewerk");
            editButton?.Click();
        });

        // Act: Change location name
        await cut.InvokeAsync(() =>
        {
            var inputs = cut.FindAll("input[type='text']");
            inputs.LastOrDefault()?.Change("New Name");
        });

        // Act: Click save and wait for async operation
        await cut.InvokeAsync(async () =>
        {
            var saveButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Opslaan") && b.GetAttribute("class")?.Contains("btn-success") == true);
            saveButton?.Click();
        });

        // Assert: Operation error shown
        var errorAlert = cut.FindAll(".alert.alert-danger").FirstOrDefault(el => el.TextContent.Contains("Update error"));
        Assert.NotNull(errorAlert);

        // Assert: Edit mode stays open - edit input field is visible
        var editInputs = cut.FindAll("input[type='text'].form-control-sm");
        Assert.NotEmpty(editInputs);

        // Assert: Save and Cancel buttons are still visible in edit mode
        var saveButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Opslaan") && b.GetAttribute("class")?.Contains("btn-success") == true);
        var cancelButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent == "Annulieren" && b.GetAttribute("class")?.Contains("btn-secondary") == true);
        Assert.NotNull(saveButton);
        Assert.NotNull(cancelButton);
    }

    [Fact]
    public async Task FailedMove_KeepsMoveDialogOpen()
    {
        // Arrange
        var kombuisId = Guid.NewGuid();
        var salonId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        _testService.Areas.Add(new() { Id = kombuisId, Name = "Kombuis" });
        _testService.Areas.Add(new() { Id = salonId, Name = "Salon" });
        _testService.Locations.Add(new() { Id = locationId, StorageAreaId = kombuisId, Name = "Kast", Description = null });
        _testService.MoveLocationHandler = (_, _) =>
            StorageOperationResult<StorageLocationDto>.Error("Move failed");

        var cut = RenderComponent<StorageManagement>();

        // Act: Click move button to open dialog
        await cut.InvokeAsync(() =>
        {
            var moveButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent == "Verplaats");
            moveButton?.Click();
        });

        // Act: Select target area
        await cut.InvokeAsync(() =>
        {
            var select = cut.Find("select");
            select?.Change(salonId.ToString());
        });

        // Act: Click move confirm and wait for async operation
        await cut.InvokeAsync(async () =>
        {
            var moveConfirmButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent == "Verplaats" && b.GetAttribute("class")?.Contains("btn-primary") == true);
            moveConfirmButton?.Click();
        });

        // Assert: Dialog still open, error shown
        var modal = cut.FindAll(".modal.show").FirstOrDefault();
        Assert.NotNull(modal);
        var errorAlert = cut.FindAll(".alert.alert-danger").FirstOrDefault(el => el.TextContent.Contains("Move failed"));
        Assert.NotNull(errorAlert);
    }

    [Fact]
    public void Crew_Render_IsReadOnly()
    {
        var authContext = this.AddTestAuthorization();
        authContext.SetAuthorized("crew");
        authContext.SetRoles("Crew");

        var kombuisId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        _testService.Areas.Add(new() { Id = kombuisId, Name = "Kombuis" });
        _testService.Locations.Add(new() { Id = locationId, StorageAreaId = kombuisId, Name = "Kast 1", Description = null });

        var cut = RenderComponent<StorageManagement>();

        Assert.Contains("Kombuis", cut.Markup);
        Assert.Contains("Kast 1", cut.Markup);
        Assert.Empty(cut.FindAll("button").Where(b => b.TextContent.Contains("Locatie toevoegen")));
        Assert.Empty(cut.FindAll("button").Where(b => b.TextContent.Contains("Hernoemen")));
        Assert.Empty(cut.FindAll("button").Where(b => b.TextContent.Contains("Bewerk")));
        Assert.Empty(cut.FindAll("button").Where(b => b.TextContent.Contains("Verwijder")));
    }

    private class TestStorageService : IStorageService
    {
        public List<StorageAreaDto> Areas { get; } = new();
        public List<StorageLocationDto> Locations { get; } = new();
        public List<(Guid AreaId, string Name, string? Description)> CreateLocationCalls { get; } = new();

        public Func<Guid, string, string?, StorageOperationResult<StorageLocationDto>>? CreateLocationHandler { get; set; }
        public Func<Guid, string, string?, StorageOperationResult<StorageLocationDto>>? UpdateLocationHandler { get; set; }
        public Func<Guid, Guid, StorageOperationResult<StorageLocationDto>>? MoveLocationHandler { get; set; }

        public Task<IReadOnlyList<StorageAreaDto>> GetAllAreasAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<StorageAreaDto>>(Areas);

        public Task<IReadOnlyList<StorageLocationDto>> GetLocationsByAreaAsync(Guid areaId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<StorageLocationDto>>(Locations.Where(l => l.StorageAreaId == areaId).ToList());

        public Task<StorageOperationResult<StorageLocationDto>> CreateLocationAsync(Guid areaId, string name, string? description, CancellationToken ct = default)
        {
            CreateLocationCalls.Add((areaId, name, description));
            return Task.FromResult(CreateLocationHandler?.Invoke(areaId, name, description) ??
                StorageOperationResult<StorageLocationDto>.Ok(new() { Id = Guid.NewGuid(), StorageAreaId = areaId, Name = name, Description = description }));
        }

        public Task<StorageOperationResult<StorageLocationDto>> UpdateLocationAsync(Guid locationId, string newName, string? newDescription, CancellationToken ct = default) =>
            Task.FromResult(UpdateLocationHandler?.Invoke(locationId, newName, newDescription) ??
                StorageOperationResult<StorageLocationDto>.Ok(new() { Id = locationId, StorageAreaId = Guid.NewGuid(), Name = newName, Description = newDescription }));

        public Task<StorageOperationResult<StorageLocationDto>> MoveLocationAsync(Guid locationId, Guid newAreaId, CancellationToken ct = default) =>
            Task.FromResult(MoveLocationHandler?.Invoke(locationId, newAreaId) ??
                StorageOperationResult<StorageLocationDto>.Ok(new() { Id = locationId, StorageAreaId = newAreaId }));

        public Task<StorageOperationResult<StorageAreaDto>> CreateAreaAsync(string name, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<StorageOperationResult> RenameAreaAsync(Guid areaId, string newName, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<StorageOperationResult> DeleteAreaAsync(Guid areaId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<StorageOperationResult> DeleteLocationAsync(Guid locationId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<StorageOperationResult<BootManager.Application.Storage.DTOs.StorageLocationDetailDto>> GetLocationDetailAsync(Guid locationId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<StorageOperationResult<string>> GenerateOrGetQrTokenAsync(Guid locationId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<QrResolutionResult> ResolveQrValueAsync(string? qrValue, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<StorageOperationResult> LinkQrToExistingLocationAsync(string token, Guid locationId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<StorageOperationResult<BootManager.Application.Storage.DTOs.StorageLocationDetailDto>> CreateLocationWithQrTokenAsync(Guid areaId, string name, string? description, string token, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<BootManager.Application.Storage.DTOs.StorageLocationOverviewDto>> GetAllLocationsOverviewAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<StorageOperationResult<string>> ReplaceQrTokenAsync(Guid locationId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<StorageOperationResult> UpdateTagStatusAsync(Guid locationId, BootManager.Core.Enums.TagStatus newStatus, CancellationToken ct = default) => throw new NotImplementedException();
    }
}
