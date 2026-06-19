using Bunit;
using BootManager.Application.Storage.DTOs;
using BootManager.Application.Storage.Results;
using BootManager.Application.Storage.Services;
using BootManager.Web.Components.Pages;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using System.Security.Claims;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Real bUnit component tests for StorageLocationDetails.razor.
/// Tests back navigation, QR generation, Owner/Crew visibility, and idempotency.
/// </summary>
public class StorageLocationDetailsComponentTests : TestContext
{
    private readonly Mock<IStorageService> _storageMock = new();

    public StorageLocationDetailsComponentTests()
    {
        Services.AddScoped<IStorageService>(_ => _storageMock.Object);
    }

    [Fact]
    public async Task BackButton_CallsHistoryBack()
    {
        var locationId = Guid.NewGuid();
        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "TestArea",
            LocationName = "TestLocation",
            Description = "Test Description",
            QrValue = null
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        SetupAuthState(owner: true);
        JSInterop.SetupVoid("history.back");

        var cut = RenderComponent<StorageLocationDetails>(
            p => p.Add(c => c.LocationId, locationId));

        var backButton = cut.FindAll("button").First(b => b.TextContent.Contains("Terug"));
        await cut.InvokeAsync(() => backButton.Click());

        Assert.Single(JSInterop.Invocations.Where(i => i.Identifier == "history.back"));
    }

    [Fact]
    public void OwnerWithoutToken_SeesGenerateButton()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "TestArea",
            LocationName = "TestLocation",
            QrValue = null // No token yet
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationDetails>(
            p => p.Add(c => c.LocationId, locationId));

        var generateButton = cut.FindAll("button")
            .FirstOrDefault(b => b.TextContent.Contains("QR-token genereren"));

        Assert.NotNull(generateButton);
    }

    [Fact]
    public async Task OwnerAfterGenerate_ShowsQrValue()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var token = "a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5";
        var qrValue = $"bootmanager:location:{token}";

        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "TestArea",
            LocationName = "TestLocation",
            QrValue = null
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        _storageMock.Setup(s => s.GenerateOrGetQrTokenAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<string>.Ok(qrValue));

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationDetails>(
            p => p.Add(c => c.LocationId, locationId));

        var generateButton = cut.FindAll("button")
            .FirstOrDefault(b => b.TextContent.Contains("QR-token genereren"));

        Assert.NotNull(generateButton);
        await cut.InvokeAsync(() => generateButton!.Click());

        cut.WaitForAssertion(() => Assert.Contains(qrValue, cut.Markup));

        _storageMock.Verify(s => s.GenerateOrGetQrTokenAsync(locationId, default), Times.Once);
    }

    [Fact]
    public void CrewWithoutToken_SeesNoTokenAndNoGenerateButton()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var detailDto = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "TestArea",
            LocationName = "TestLocation",
            QrValue = null
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDto));

        SetupAuthState(owner: false);

        var cut = RenderComponent<StorageLocationDetails>(
            p => p.Add(c => c.LocationId, locationId));

        var generateButton = cut.FindAll("button")
            .FirstOrDefault(b => b.TextContent.Contains("QR-token genereren"));

        Assert.Null(generateButton);

        var qrSection = cut.FindAll(".mb-3")
            .FirstOrDefault(s => s.TextContent.Contains("BootManager QR"));

        Assert.Null(qrSection);
    }

    [Fact]
    public void SecondRender_WithExistingToken_ShowsSameValueWithoutExtraGenerate()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var token = "a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5";
        var qrValue = $"bootmanager:location:{token}";

        var detailDtoWithToken = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "TestArea",
            LocationName = "TestLocation",
            QrValue = qrValue // Already has token
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDtoWithToken));

        SetupAuthState(owner: true);

        var cut = RenderComponent<StorageLocationDetails>(
            p => p.Add(c => c.LocationId, locationId));

        Assert.Contains(qrValue, cut.Markup);
        var generateButton = cut.FindAll("button")
            .FirstOrDefault(b => b.TextContent.Contains("QR-token genereren"));

        Assert.Null(generateButton);
        _storageMock.Verify(s => s.GenerateOrGetQrTokenAsync(locationId, default), Times.Never);
    }

    [Fact]
    public void AcceptancePath_AfterLinkQrToExisting_DetailShowsQrNotGenerateButton()
    {
        // Arrange: Simulate the acceptance path where a location gets a linked QR via LinkQrToExistingLocation
        var locationId = Guid.NewGuid();
        var linkedToken = "a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5";
        var linkedQrValue = $"bootmanager:location:{linkedToken}";

        // After LinkQrToExistingLocationAsync succeeds and the user navigates to the detail page,
        // the detail service call should return the location WITH the newly linked QrValue
        var detailDtoAfterLink = new StorageLocationDetailDto
        {
            Id = locationId,
            AreaName = "TestArea",
            LocationName = "TestLocation",
            QrValue = linkedQrValue  // The token that was just linked
        };

        _storageMock.Setup(s => s.GetLocationDetailAsync(locationId, default))
            .ReturnsAsync(StorageOperationResult<StorageLocationDetailDto>.Ok(detailDtoAfterLink));

        SetupAuthState(owner: true);

        // Act: Render the detail page after the link
        var cut = RenderComponent<StorageLocationDetails>(
            p => p.Add(c => c.LocationId, locationId));

        // Assert: Should show the linked QR value, NOT the generate button
        Assert.Contains(linkedQrValue, cut.Markup);
        var generateButton = cut.FindAll("button")
            .FirstOrDefault(b => b.TextContent.Contains("QR-token genereren"));
        Assert.Null(generateButton);

        // Verify the service was only called for reading, not generating
        _storageMock.Verify(s => s.GetLocationDetailAsync(locationId, default), Times.Once);
        _storageMock.Verify(s => s.GenerateOrGetQrTokenAsync(locationId, default), Times.Never);
    }

    private void SetupAuthState(bool owner)
    {
        var claims = owner
            ? new[] { new Claim(ClaimTypes.Role, "Owner") }
            : new[] { new Claim(ClaimTypes.Role, "Crew") };

        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);
        var authState = new AuthenticationState(principal);

        var authStateMock = new Mock<AuthenticationStateProvider>();
        authStateMock.Setup(p => p.GetAuthenticationStateAsync())
            .ReturnsAsync(authState);

        Services.AddScoped(_ => authStateMock.Object);
    }
}
