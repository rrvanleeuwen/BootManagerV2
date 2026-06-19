using Bunit;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Results;
using BootManager.Application.Storage.Services;
using BootManager.Web.Components.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Real bUnit component tests for StorageLocations page.
/// Tests that the page renders the StorageManagement component.
/// </summary>
public class StorageLocationsComponentTests : TestContext
{
    private readonly TestStorageService _testService = new();

    public StorageLocationsComponentTests()
    {
        Services.AddScoped<IStorageService>(_ => _testService);
        Services.AddAuthorizationCore();

        // Provide Owner authorization context
        var authStateProvider = new TestOwnerAuthorizationStateProvider();
        Services.AddScoped<AuthenticationStateProvider>(_ => authStateProvider);
    }

    [Fact]
    public void StorageLocationsPage_RendersStorageManagementComponent()
    {
        // Arrange: One area with test data
        var kombuisId = Guid.NewGuid();
        _testService.Areas.Add(new() { Id = kombuisId, Name = "Kombuis" });
        _testService.Locations.Add(new()
        {
            Id = Guid.NewGuid(),
            StorageAreaId = kombuisId,
            Name = "Oven",
            Description = "Galley oven"
        });

        // Act: Render StorageLocations page
        var cut = RenderComponent<StorageLocations>();

        // Assert: StorageManagement component should be rendered
        var heading = cut.Find("h5");
        Assert.NotNull(heading);
        Assert.Contains("Opslag", heading.TextContent);

        // Check that area table is rendered
        var areaNameElements = cut.FindAll("span").Where(el => el.TextContent == "Kombuis");
        Assert.NotEmpty(areaNameElements);
    }

    /// <summary>
    /// Test authorization state provider that simulates Owner role.
    /// </summary>
    private class TestOwnerAuthorizationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var claims = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Owner")
            }, "TestScheme");

            var user = new System.Security.Claims.ClaimsPrincipal(claims);
            var state = new AuthenticationState(user);
            return Task.FromResult(state);
        }
    }

    /// <summary>
    /// Test double for IStorageService, mimics behavior of StorageManagementComponentTests.
    /// </summary>
    private class TestStorageService : IStorageService
    {
        public List<StorageAreaDto> Areas { get; } = new();
        public List<StorageLocationDto> Locations { get; } = new();

        public Task<IReadOnlyList<StorageAreaDto>> GetAllAreasAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<StorageAreaDto>>(Areas);

        public Task<IReadOnlyList<StorageLocationDto>> GetLocationsByAreaAsync(Guid areaId, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<StorageLocationDto>>(Locations.Where(l => l.StorageAreaId == areaId).ToList());

        public Task<StorageOperationResult<StorageLocationDto>> CreateLocationAsync(Guid areaId, string name, string? description, CancellationToken ct = default) =>
            Task.FromResult(StorageOperationResult<StorageLocationDto>.Ok(new() { Id = Guid.NewGuid(), StorageAreaId = areaId, Name = name, Description = description }));

        public Task<StorageOperationResult<StorageLocationDto>> UpdateLocationAsync(Guid locationId, string newName, string? newDescription, CancellationToken ct = default) =>
            Task.FromResult(StorageOperationResult<StorageLocationDto>.Ok(new() { Id = locationId, StorageAreaId = Guid.NewGuid(), Name = newName, Description = newDescription }));

        public Task<StorageOperationResult<StorageLocationDto>> MoveLocationAsync(Guid locationId, Guid newAreaId, CancellationToken ct = default) =>
            Task.FromResult(StorageOperationResult<StorageLocationDto>.Ok(new() { Id = locationId, StorageAreaId = newAreaId }));

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
