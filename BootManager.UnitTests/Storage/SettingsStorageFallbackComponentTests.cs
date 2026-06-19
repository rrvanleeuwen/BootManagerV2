using Bunit;
using BootManager.Application.Authentication.DTOs;
using BootManager.Application.Authentication.Services;
using BootManager.Application.OperationalSettings.DTOs;
using BootManager.Application.OperationalSettings.Services;
using BootManager.Application.VesselProfile.DTOs;
using BootManager.Application.VesselProfile.Services;
using BootManager.Web.Components.Pages;
using BootManager.Web.Controllers;
using BootManager.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Security.Claims;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Real bUnit tests for Settings page navigation after storage moved to main nav.
/// </summary>
public class SettingsStorageFallbackComponentTests : TestContext
{
    public SettingsStorageFallbackComponentTests()
    {
        Services.AddAuthorizationCore();
        Services.AddScoped<AuthenticationStateProvider>(_ => new TestAuthStateProvider(CreateOwnerState()));
        Services.AddScoped<IOwnerSettingsService>(_ => new TestOwnerSettingsService());
        Services.AddScoped<ILocalUserManagementService>(_ => new TestLocalUserManagementService());
        Services.AddScoped<IOperationalSettingsService>(_ => new TestOperationalSettingsService());
        Services.AddScoped<IOperationalSettingsWithReloadService>(_ => new TestOperationalSettingsWithReloadService());
        Services.AddScoped<IVesselProfileService>(_ => new TestVesselProfileService());
        Services.AddScoped(_ => new HttpClient());
    }

    [Fact]
    public void SettingsPage_DoesNotRenderStandaloneStorageAccordion()
    {
        var cut = RenderComponent<Settings>();

        Assert.DoesNotContain("aria-controls=\"storageCollapse\"", cut.Markup);
        Assert.DoesNotContain(">Opslag</button>", cut.Markup);
        Assert.DoesNotContain("Opslaggebieden", cut.Markup);
    }

    private static Task<AuthenticationState> CreateOwnerState()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "owner-user"),
            new Claim(ClaimTypes.Name, "Owner User"),
            new Claim(ClaimTypes.Role, "Owner")
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        return Task.FromResult(new AuthenticationState(principal));
    }

    private sealed class TestAuthStateProvider(Task<AuthenticationState> authStateTask)
        : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => authStateTask;
    }

    private sealed class TestOwnerSettingsService : IOwnerSettingsService
    {
        public Task ChangePasswordAsync(ChangePasswordRequestDto request, CancellationToken ct = default) => Task.CompletedTask;
        public Task SetPinAsync(ChangePinRequestDto request, CancellationToken ct = default) => Task.CompletedTask;
        public Task ClearPinAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task<GetOwnerProfileResponseDto> GetOwnerProfileAsync(CancellationToken ct = default) =>
            Task.FromResult(new GetOwnerProfileResponseDto { Name = "Owner", Email = "owner@example.test" });
        public Task UpdateOwnerProfileAsync(UpdateOwnerProfileRequestDto request, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class TestLocalUserManagementService : ILocalUserManagementService
    {
        public Task<List<ActiveUsersListDto>> GetActiveUsersAsync(CancellationToken ct = default) => Task.FromResult(new List<ActiveUsersListDto>());
        public Task<List<CrewManagementListDto>> GetAllCrewAsync(CancellationToken ct = default) => Task.FromResult(new List<CrewManagementListDto>());
        public Task<CreateCrewResultDto> CreateCrewAsync(string displayName, string temporaryPassword, CancellationToken ct = default) =>
            Task.FromResult(new CreateCrewResultDto { Success = true });
        public Task<ResetCrewPasswordResultDto> ResetCrewPasswordAsync(Guid crewId, string newTemporaryPassword, CancellationToken ct = default) =>
            Task.FromResult(new ResetCrewPasswordResultDto { Success = true });
        public Task<bool> DisableCrewAsync(Guid crewId, CancellationToken ct = default) => Task.FromResult(true);
        public Task<bool> ReactivateCrewAsync(Guid crewId, CancellationToken ct = default) => Task.FromResult(true);
        public Task<bool> UpdateOwnerDisplayNameAsync(Guid ownerId, string newName, CancellationToken ct = default) => Task.FromResult(true);
    }

    private sealed class TestOperationalSettingsService : IOperationalSettingsService
    {
        public Task<OperationalSettingsDto> GetAsync(CancellationToken ct = default) => Task.FromResult(new OperationalSettingsDto());
        public Task SaveAsync(OperationalSettingsDto dto, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class TestOperationalSettingsWithReloadService : IOperationalSettingsWithReloadService
    {
        public Task<OperationalSettingsDto> GetOperationalSettingsAsync(CancellationToken ct = default) =>
            Task.FromResult(new OperationalSettingsDto());

        public Task<SaveOperationalSettingsResponse> SaveAndReloadAsync(OperationalSettingsDto dto, CancellationToken ct = default) =>
            Task.FromResult(new SaveOperationalSettingsResponse { SettingsSaved = true, SaveMessage = "Instellingen opgeslagen." });
    }

    private sealed class TestVesselProfileService : IVesselProfileService
    {
        public Task<VesselProfileDto> GetOrCreateVesselProfileAsync(CancellationToken ct = default) =>
            Task.FromResult(new VesselProfileDto { Id = Guid.NewGuid(), VesselName = "Test Vessel" });

        public Task<VesselProfileDto> UpdateVesselProfileAsync(UpdateVesselProfileRequestDto request, CancellationToken ct = default) =>
            Task.FromResult(new VesselProfileDto { Id = Guid.NewGuid(), VesselName = request.VesselName });

        public Task<VesselProfileDto> AdvanceCurrentMetersAsync(decimal?[] engineHoursCandidates, decimal?[] logstandCandidates, CancellationToken ct = default) =>
            Task.FromResult(new VesselProfileDto { Id = Guid.NewGuid(), VesselName = "Test Vessel" });
    }
}
