using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Storage.Services;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Core.Interfaces;
using Moq;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Unit tests for StorageService TagStatus operations.
/// Tests manual status updates and persistence.
/// </summary>
public class StorageServiceTagStatusTests
{
    private readonly Mock<IRepository<StorageArea>> _areaRepoMock = new();
    private readonly Mock<IRepository<StorageLocation>> _locationRepoMock = new();
    private readonly Mock<IStockService> _stockServiceMock = new();
    private StorageService _service = null!;

    public StorageServiceTagStatusTests()
    {
        ResetService();
    }

    private void ResetService()
    {
        _service = new StorageService(_areaRepoMock.Object, _locationRepoMock.Object, _stockServiceMock.Object);
    }

    [Fact]
    public async Task UpdateTagStatus_SucceedsForValidStatus()
    {
        var locationId = Guid.NewGuid();
        var location = StorageLocation.Create(Guid.NewGuid(), "TestLocation");
        Assert.Equal(TagStatus.NotPrinted, location.TagStatus);

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        var result = await _service.UpdateTagStatusAsync(locationId, TagStatus.Printed);

        Assert.True(result.Success);
        Assert.Equal(TagStatus.Printed, location.TagStatus);
        _locationRepoMock.Verify(r => r.UpdateAsync(location, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTagStatus_CanSetToApplied()
    {
        var locationId = Guid.NewGuid();
        var location = StorageLocation.Create(Guid.NewGuid(), "TestLocation");

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        await _service.UpdateTagStatusAsync(locationId, TagStatus.Applied);

        Assert.Equal(TagStatus.Applied, location.TagStatus);
    }

    [Fact]
    public async Task UpdateTagStatus_CanSetToReplaced()
    {
        var locationId = Guid.NewGuid();
        var location = StorageLocation.Create(Guid.NewGuid(), "TestLocation");

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        await _service.UpdateTagStatusAsync(locationId, TagStatus.Replaced);

        Assert.Equal(TagStatus.Replaced, location.TagStatus);
    }

    [Fact]
    public async Task UpdateTagStatus_ReturnsErrorForMissingLocation()
    {
        var locationId = Guid.NewGuid();
        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StorageLocation?)null);

        var result = await _service.UpdateTagStatusAsync(locationId, TagStatus.Printed);

        Assert.False(result.Success);
        Assert.Contains("niet gevonden", result.ErrorMessage ?? "");
    }

    [Fact]
    public async Task GetAllLocationsOverview_IncludesTagStatus()
    {
        var areaId = Guid.NewGuid();
        var area = StorageArea.Create("TestArea");
        var location = StorageLocation.Create(areaId, "TestLocation");
        location.UpdateTagStatus(TagStatus.Applied);

        _areaRepoMock.Setup(r => r.ListAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([area]);
        _locationRepoMock.Setup(r => r.ListAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync([location]);

        var result = await _service.GetAllLocationsOverviewAsync();

        Assert.Single(result);
        Assert.Equal(TagStatus.Applied, result[0].TagStatus);
    }

    [Fact]
    public async Task GetLocationDetail_IncludesTagStatus()
    {
        var locationId = Guid.NewGuid();
        var areaId = Guid.NewGuid();
        var area = StorageArea.Create("TestArea");
        var location = StorageLocation.Create(areaId, "TestLocation");
        location.UpdateTagStatus(TagStatus.Printed);

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);
        _areaRepoMock.Setup(r => r.GetByIdAsync(areaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(area);

        var result = await _service.GetLocationDetailAsync(locationId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(TagStatus.Printed, result.Data.TagStatus);
    }
}
