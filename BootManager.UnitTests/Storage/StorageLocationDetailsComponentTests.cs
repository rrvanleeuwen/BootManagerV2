using Bunit;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Results;
using BootManager.Application.Storage.Services;
using BootManager.Web.Components.Pages;
using Microsoft.Extensions.DependencyInjection;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Real bUnit component tests for StorageLocationDetails Blazor component.
/// Tests the back button interaction with browser history.
/// </summary>
public class StorageLocationDetailsComponentTests : TestContext
{
    private readonly TestStorageService _testService = new();

    public StorageLocationDetailsComponentTests()
    {
        Services.AddScoped<IStorageService>(_ => _testService);
    }

    [Fact]
    public async Task BackButton_CallsHistoryBack()
    {
        // Arrange: Set up service to return valid location detail
        var locationId = Guid.NewGuid();
        _testService.DetailHandler = _ =>
            StorageOperationResult<StorageLocationDetailDto>.Ok(new()
            {
                Id = locationId,
                LocationName = "Kast 1",
                AreaName = "Kombuis",
                Description = "Test beschrijving"
            });

        // Arrange: Configure JSInterop to track history.back calls
        JSInterop.SetupVoid("history.back");

        // Arrange: Create component with LocationId parameter
        var cut = RenderComponent<StorageLocationDetails>(parameters =>
            parameters.Add(p => p.LocationId, locationId));

        // Act: Click the back button
        var backButton = cut.Find("button");
        await cut.InvokeAsync(() => backButton.Click());

        // Assert: history.back was called exactly once
        var invocations = JSInterop.Invocations;
        var historyBackCalls = invocations.Where(inv => inv.Identifier == "history.back").ToList();
        Assert.Single(historyBackCalls);
    }

    private class TestStorageService : IStorageService
    {
        public Func<Guid, StorageOperationResult<StorageLocationDetailDto>>? DetailHandler { get; set; }

        public Task<StorageOperationResult<StorageLocationDetailDto>> GetLocationDetailAsync(Guid locationId, CancellationToken ct = default) =>
            Task.FromResult(DetailHandler?.Invoke(locationId) ??
                StorageOperationResult<StorageLocationDetailDto>.Ok(new() { Id = locationId }));

        public Task<IReadOnlyList<StorageAreaDto>> GetAllAreasAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<StorageLocationDto>> GetLocationsByAreaAsync(Guid areaId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<StorageOperationResult<StorageLocationDto>> CreateLocationAsync(Guid areaId, string name, string? description, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<StorageOperationResult<StorageLocationDto>> UpdateLocationAsync(Guid locationId, string newName, string? newDescription, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<StorageOperationResult<StorageLocationDto>> MoveLocationAsync(Guid locationId, Guid newAreaId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<StorageOperationResult<StorageAreaDto>> CreateAreaAsync(string name, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<StorageOperationResult> RenameAreaAsync(Guid areaId, string newName, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<StorageOperationResult> DeleteAreaAsync(Guid areaId, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<StorageOperationResult> DeleteLocationAsync(Guid locationId, CancellationToken ct = default) => throw new NotImplementedException();
    }
}
