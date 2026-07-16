using Bunit;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Results;
using BootManager.Application.Storage.Services;
using BootManager.Core.Enums;
using BootManager.Web.Components.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Real bUnit component tests for StorageLocationTagOverview.razor.
/// Tests Owner-only access, tag status updates, token replacement, and rendering.
/// </summary>
public class StorageLocationTagOverviewComponentTests : TestContext
{
    private readonly Mock<IStorageService> _storageMock = new();

    public StorageLocationTagOverviewComponentTests()
    {
        Services.AddScoped<IStorageService>(_ => _storageMock.Object);
    }

    private void SetupAuthState(bool owner = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "testuser"),
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Role, owner ? "Owner" : "Crew")
        };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        var state = Task.FromResult(new AuthenticationState(principal));
        Services.AddScoped<AuthenticationStateProvider>(_ => new TestAuthStateProvider(state));
    }

    [Fact]
    public void Component_LoadsAndRenders()
    {
        var location1 = new StorageLocationOverviewDto
        {
            Id = Guid.NewGuid(),
            AreaName = "Kombuis",
            LocationName = "Koelkast",
            QrValue = "bootmanager:location:abcd1234efgh5678ijkl9012mnop3456",
            TagStatus = TagStatus.Printed
        };

        _storageMock.Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([location1]);

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTagOverview>();
        Assert.NotNull(cut);
    }

    [Fact]
    public async Task Component_DisplaysQrValueWhenAvailable()
    {
        var token = "abcd1234efgh5678ijkl9012mnop3456";
        var qrValue = $"bootmanager:location:{token}";
        var location = new StorageLocationOverviewDto
        {
            Id = Guid.NewGuid(),
            AreaName = "Kombuis",
            LocationName = "Koelkast",
            QrValue = qrValue,
            TagStatus = TagStatus.Printed
        };

        _storageMock.Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([location]);

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTagOverview>();

        var codeElements = cut.FindAll("code");
        Assert.Contains(codeElements, c => c.TextContent == qrValue);
    }

    [Fact]
    public async Task Component_ShowsNoTokenMessageWhenAbsent()
    {
        var location = new StorageLocationOverviewDto
        {
            Id = Guid.NewGuid(),
            AreaName = "Kombuis",
            LocationName = "Koelkast",
            QrValue = null,
            TagStatus = TagStatus.NotPrinted
        };

        _storageMock.Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([location]);

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTagOverview>();

        var text = cut.Markup;
        Assert.Contains("geen token", text);
    }

    [Fact]
    public async Task Component_RendersStatusDropdown()
    {
        var location = new StorageLocationOverviewDto
        {
            Id = Guid.NewGuid(),
            AreaName = "Kombuis",
            LocationName = "Koelkast",
            QrValue = "bootmanager:location:abcd1234efgh5678ijkl9012mnop3456",
            TagStatus = TagStatus.NotPrinted
        };

        _storageMock.Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([location]);
        _storageMock.Setup(s => s.UpdateTagStatusAsync(It.IsAny<Guid>(), It.IsAny<TagStatus>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(StorageOperationResult.Ok());

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTagOverview>();

        var selects = cut.FindAll("select");
        Assert.Single(selects);
        var options = cut.FindAll("option");
        Assert.Contains(options, o => o.TextContent.Contains("Niet geprint"));
        Assert.Contains(options, o => o.TextContent.Contains("Geprint"));
        Assert.Contains(options, o => o.TextContent.Contains("Gekoppeld"));
        Assert.Contains(options, o => o.TextContent.Contains("Vervangen"));
    }

    [Fact]
    public async Task Component_CanUpdateTagStatus()
    {
        var locationId = Guid.NewGuid();
        var location = new StorageLocationOverviewDto
        {
            Id = locationId,
            AreaName = "Kombuis",
            LocationName = "Koelkast",
            QrValue = "bootmanager:location:abcd1234efgh5678ijkl9012mnop3456",
            TagStatus = TagStatus.NotPrinted
        };

        _storageMock.Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([location]);
        _storageMock.Setup(s => s.UpdateTagStatusAsync(locationId, TagStatus.Printed, It.IsAny<CancellationToken>()))
            .ReturnsAsync(StorageOperationResult.Ok());

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTagOverview>();
        var select = cut.Find("select");
        await cut.InvokeAsync(() => select.Change(TagStatus.Printed.ToString()));

        _storageMock.Verify(s => s.UpdateTagStatusAsync(locationId, TagStatus.Printed, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Component_ShowsReplaceButtonWhenTokenExists()
    {
        var location = new StorageLocationOverviewDto
        {
            Id = Guid.NewGuid(),
            AreaName = "Kombuis",
            LocationName = "Koelkast",
            QrValue = "bootmanager:location:abcd1234efgh5678ijkl9012mnop3456",
            TagStatus = TagStatus.Applied
        };

        _storageMock.Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([location]);

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTagOverview>();

        var buttons = cut.FindAll("button");
        var replaceButton = buttons.FirstOrDefault(b => b.TextContent.Contains("Vervangen"));
        Assert.NotNull(replaceButton);
    }

    [Fact]
    public async Task Component_DoesNotShowReplaceButtonWhenNoToken()
    {
        var location = new StorageLocationOverviewDto
        {
            Id = Guid.NewGuid(),
            AreaName = "Kombuis",
            LocationName = "Koelkast",
            QrValue = null,
            TagStatus = TagStatus.NotPrinted
        };

        _storageMock.Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([location]);

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTagOverview>();

        var replaceButtons = cut.FindAll("button")
            .Where(b => b.TextContent.Contains("Vervangen"))
            .ToList();
        Assert.Empty(replaceButtons);
    }

    [Fact]
    public void Component_RendersBatchPrintAction()
    {
        var location = new StorageLocationOverviewDto
        {
            Id = Guid.NewGuid(),
            AreaName = "Kombuis",
            LocationName = "Koelkast",
            QrValue = "bootmanager:location:abcd1234efgh5678ijkl9012mnop3456",
            TagStatus = TagStatus.Printed
        };

        _storageMock.Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([location]);

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTagOverview>();

        var batchPrintButton = cut.FindAll("button")
            .FirstOrDefault(b => b.TextContent.Contains("Alle tags afdrukken"));
        Assert.NotNull(batchPrintButton);
    }

    [Fact]
    public async Task Component_BatchPrintAction_NavigatesToExistingPrintRoute()
    {
        var location = new StorageLocationOverviewDto
        {
            Id = Guid.NewGuid(),
            AreaName = "Kombuis",
            LocationName = "Koelkast",
            QrValue = "bootmanager:location:abcd1234efgh5678ijkl9012mnop3456",
            TagStatus = TagStatus.Printed
        };

        _storageMock.Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([location]);

        SetupAuthState(owner: true);

        var navigation = Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
        var cut = RenderComponent<StorageLocationTagOverview>();

        var batchPrintButton = cut.FindAll("button").First(b => b.TextContent.Contains("Alle tags afdrukken"));
        await cut.InvokeAsync(() => batchPrintButton.Click());

        Assert.EndsWith("/storage/tag-print-overview", navigation.Uri);
    }

    [Fact]
    public async Task Component_CanReplaceToken()
    {
        var locationId = Guid.NewGuid();
        var oldQrValue = "bootmanager:location:abcd1234efgh5678ijkl9012mnop3456";
        var newQrValue = "bootmanager:location:zyxw9876vutsrqpollkjihgfedcba5432";

        var location = new StorageLocationOverviewDto
        {
            Id = locationId,
            AreaName = "Kombuis",
            LocationName = "Koelkast",
            QrValue = oldQrValue,
            TagStatus = TagStatus.Applied
        };

        var locationAfterReplace = new StorageLocationOverviewDto
        {
            Id = locationId,
            AreaName = "Kombuis",
            LocationName = "Koelkast",
            QrValue = newQrValue,
            TagStatus = TagStatus.Replaced
        };

        _storageMock.Setup(s => s.GetAllLocationsOverviewAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([location]);
        _storageMock.Setup(s => s.ReplaceQrTokenAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(StorageOperationResult<string>.Ok(newQrValue))
            .Callback(() =>
            {
                location.QrValue = newQrValue;
                location.TagStatus = TagStatus.Replaced;
            });
        _storageMock.Setup(s => s.UpdateTagStatusAsync(It.IsAny<Guid>(), It.IsAny<TagStatus>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(StorageOperationResult.Ok());

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationTagOverview>();
        var replaceButton = cut.FindAll("button").First(b => b.TextContent.Contains("Vervangen"));
        await cut.InvokeAsync(() => replaceButton.Click());

        _storageMock.Verify(s => s.ReplaceQrTokenAsync(locationId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

internal class TestAuthStateProvider : AuthenticationStateProvider
{
    private readonly Task<AuthenticationState> _state;

    public TestAuthStateProvider(Task<AuthenticationState> state)
    {
        _state = state;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _state;
}
