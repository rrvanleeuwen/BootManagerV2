using BootManager.Application.Storage.QrFormat;
using BootManager.Application.Storage.Results;
using BootManager.Application.Storage.Services;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Moq;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Unit tests for StorageService QR token operations.
/// Tests token generation, resolution, and linking workflows.
/// </summary>
public class StorageServiceQrTokenTests
{
    private readonly Mock<IRepository<StorageArea>> _areaRepoMock = new();
    private readonly Mock<IRepository<StorageLocation>> _locationRepoMock = new();
    private StorageService _service = null!;

    public StorageServiceQrTokenTests()
    {
        ResetService();
    }

    private void ResetService()
    {
        _service = new StorageService(_areaRepoMock.Object, _locationRepoMock.Object);
    }

    [Fact]
    public async Task GenerateOrGetQrToken_GeneratesTokenForLocationWithoutOne()
    {
        var locationId = Guid.NewGuid();
        var location = StorageLocation.Create(Guid.NewGuid(), "TestLocation");

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);

        var result = await _service.GenerateOrGetQrTokenAsync(locationId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(LocationQrValue.IsValidToken(LocationQrValue.TryParseQrValue(result.Data)));
        _locationRepoMock.Verify(r => r.UpdateAsync(location, default), Times.Once);
    }

    [Fact]
    public async Task GenerateOrGetQrToken_IsIdempotent_ReturnsSameToken()
    {
        var locationId = Guid.NewGuid();
        var location = StorageLocation.Create(Guid.NewGuid(), "TestLocation");

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);

        var result1 = await _service.GenerateOrGetQrTokenAsync(locationId);
        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);

        var result2 = await _service.GenerateOrGetQrTokenAsync(locationId);

        Assert.True(result1.Success);
        Assert.True(result2.Success);
        Assert.Equal(result1.Data, result2.Data);
    }

    [Fact]
    public async Task GenerateOrGetQrToken_ReturnsErrorForMissingLocation()
    {
        var locationId = Guid.NewGuid();
        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync((StorageLocation?)null);

        var result = await _service.GenerateOrGetQrTokenAsync(locationId);

        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task ResolveQrValue_ReturnsLinkedForKnownToken()
    {
        var token = LocationQrValue.GenerateToken();
        var qrValue = LocationQrValue.FormatQrValue(token);
        var location = StorageLocation.Create(Guid.NewGuid(), "TestLocation");
        location.SetQrToken(token);

        _locationRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StorageLocation, bool>>>(), default))
            .ReturnsAsync(location);

        var result = await _service.ResolveQrValueAsync(qrValue);

        Assert.Equal(QrStatus.Linked, result.Status);
        Assert.Equal(location.Id, result.LinkedLocationId);
    }

    [Fact]
    public async Task ResolveQrValue_ReturnsUnknownForValidButUnlinkedToken()
    {
        var token = LocationQrValue.GenerateToken();
        var qrValue = LocationQrValue.FormatQrValue(token);

        _locationRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StorageLocation, bool>>>(), default))
            .ReturnsAsync((StorageLocation?)null);

        var result = await _service.ResolveQrValueAsync(qrValue);

        Assert.Equal(QrStatus.Unknown, result.Status);
        Assert.Equal(token, result.Token);
    }

    [Fact]
    public async Task ResolveQrValue_ReturnsInvalidForNonBootManagerQr()
    {
        var qrValue = "random:qr:code";

        var result = await _service.ResolveQrValueAsync(qrValue);

        Assert.Equal(QrStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task LinkQrToExistingLocation_SucceedsForUnlinkedToken()
    {
        var token = LocationQrValue.GenerateToken();
        var locationId = Guid.NewGuid();
        var location = StorageLocation.Create(Guid.NewGuid(), "TestLocation");

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);
        _locationRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StorageLocation, bool>>>(), default))
            .ReturnsAsync((StorageLocation?)null);

        var result = await _service.LinkQrToExistingLocationAsync(token, locationId);

        Assert.True(result.Success);
        Assert.Equal(token, location.QrToken);
        _locationRepoMock.Verify(r => r.UpdateAsync(location, default), Times.Once);
    }

    [Fact]
    public async Task LinkQrToExistingLocation_FailsForAlreadyLinkedToken()
    {
        var token = LocationQrValue.GenerateToken();
        var locationId = Guid.NewGuid();
        var location = StorageLocation.Create(Guid.NewGuid(), "TestLocation");
        var otherLocation = StorageLocation.Create(Guid.NewGuid(), "OtherLocation");
        otherLocation.SetQrToken(token);

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);
        _locationRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StorageLocation, bool>>>(), default))
            .ReturnsAsync(otherLocation);

        var result = await _service.LinkQrToExistingLocationAsync(token, locationId);

        Assert.False(result.Success);
        Assert.Null(location.QrToken);
    }

    [Fact]
    public async Task LinkQrToExistingLocation_FailsForInvalidToken()
    {
        var token = "invalid";
        var locationId = Guid.NewGuid();

        var result = await _service.LinkQrToExistingLocationAsync(token, locationId);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task CreateLocationWithQrToken_SucceedsAndSetsToken()
    {
        var areaId = Guid.NewGuid();
        var token = LocationQrValue.GenerateToken();
        var area = StorageArea.Create("TestArea");

        _areaRepoMock.Setup(r => r.GetByIdAsync(areaId, default))
            .ReturnsAsync(area);
        _locationRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StorageLocation, bool>>>(), default))
            .ReturnsAsync((StorageLocation?)null);

        var result = await _service.CreateLocationWithQrTokenAsync(areaId, "NewLocation", "Description", token);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(token, LocationQrValue.TryParseQrValue(result.Data.QrValue));
        _locationRepoMock.Verify(r => r.AddAsync(It.Is<StorageLocation>(l => l.QrToken == token), default), Times.Once);
    }

    [Fact]
    public async Task CreateLocationWithQrToken_FailsForAlreadyLinkedToken()
    {
        var areaId = Guid.NewGuid();
        var token = LocationQrValue.GenerateToken();
        var area = StorageArea.Create("TestArea");
        var existingLocation = StorageLocation.Create(areaId, "ExistingLocation");
        existingLocation.SetQrToken(token);

        _areaRepoMock.Setup(r => r.GetByIdAsync(areaId, default))
            .ReturnsAsync(area);
        _locationRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StorageLocation, bool>>>(), default))
            .ReturnsAsync(existingLocation);

        var result = await _service.CreateLocationWithQrTokenAsync(areaId, "NewLocation", "Description", token);

        Assert.False(result.Success);
        _locationRepoMock.Verify(r => r.AddAsync(It.IsAny<StorageLocation>(), default), Times.Never);
    }

    [Fact]
    public async Task LinkQrToExistingLocation_RefusesWhenLocationHasExistingToken()
    {
        var token = LocationQrValue.GenerateToken();
        var otherToken = LocationQrValue.GenerateToken();
        var locationId = Guid.NewGuid();
        var location = StorageLocation.Create(Guid.NewGuid(), "TestLocation");
        location.SetQrToken(otherToken);

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);

        var result = await _service.LinkQrToExistingLocationAsync(token, locationId);

        Assert.False(result.Success);
        Assert.Contains("al een QR-token", result.ErrorMessage ?? "");
        _locationRepoMock.Verify(r => r.UpdateAsync(It.IsAny<StorageLocation>(), default), Times.Never);
    }

    [Fact]
    public async Task LinkQrToExistingLocation_ReturnsErrorForMissingLocation()
    {
        var token = LocationQrValue.GenerateToken();
        var locationId = Guid.NewGuid();

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync((StorageLocation?)null);

        var result = await _service.LinkQrToExistingLocationAsync(token, locationId);

        Assert.False(result.Success);
        Assert.Contains("niet gevonden", result.ErrorMessage ?? "");
    }

    [Fact]
    public async Task LinkQrToExistingLocation_TranslatesUniqueConstraintFailureToFunctionalError()
    {
        var token = LocationQrValue.GenerateToken();
        var locationId = Guid.NewGuid();
        var location = StorageLocation.Create(Guid.NewGuid(), "TestLocation");

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);
        _locationRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StorageLocation, bool>>>(), default))
            .ReturnsAsync((StorageLocation?)null);
        _locationRepoMock.Setup(r => r.UpdateAsync(location, default))
            .ThrowsAsync(new Exception("outer", new Exception("UNIQUE constraint failed: StorageLocations.QrToken")));

        var result = await _service.LinkQrToExistingLocationAsync(token, locationId);

        Assert.False(result.Success);
        Assert.Contains("gekoppeld", result.ErrorMessage ?? "");
    }
}
