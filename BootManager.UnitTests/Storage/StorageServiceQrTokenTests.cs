using BootManager.Application.Inventory.Contracts;
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
    private readonly Mock<IStockService> _stockServiceMock = new();
    private StorageService _service = null!;

    public StorageServiceQrTokenTests()
    {
        ResetService();
    }

    private void ResetService()
    {
        _service = new StorageService(_areaRepoMock.Object, _locationRepoMock.Object, _stockServiceMock.Object);
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

    [Fact]
    public async Task ReplaceQrToken_GeneratesNewTokenAndInvalidatesOld()
    {
        var oldToken = LocationQrValue.GenerateToken();
        var locationId = Guid.NewGuid();
        var location = StorageLocation.Create(Guid.NewGuid(), "TestLocation");
        location.SetQrToken(oldToken);
        var oldQrValue = LocationQrValue.FormatQrValue(oldToken);

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);

        var result = await _service.ReplaceQrTokenAsync(locationId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        var newToken = LocationQrValue.TryParseQrValue(result.Data);
        Assert.NotEqual(oldToken, newToken);
        Assert.Equal(newToken, location.QrToken);
        _locationRepoMock.Verify(r => r.UpdateAsync(location, default), Times.Once);
    }

    [Fact]
    public async Task ReplaceQrToken_OldTokenNoLongerResolves()
    {
        var oldToken = LocationQrValue.GenerateToken();
        var oldQrValue = LocationQrValue.FormatQrValue(oldToken);
        var locationId = Guid.NewGuid();
        var location = StorageLocation.Create(Guid.NewGuid(), "TestLocation");
        location.SetQrToken(oldToken);

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);

        var replaceResult = await _service.ReplaceQrTokenAsync(locationId);
        Assert.True(replaceResult.Success);

        _locationRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StorageLocation, bool>>>(), default))
            .ReturnsAsync((StorageLocation?)null);

        var resolveOldResult = await _service.ResolveQrValueAsync(oldQrValue);

        Assert.Equal(QrStatus.Unknown, resolveOldResult.Status);
        Assert.Equal(oldToken, resolveOldResult.Token);
    }

    [Fact]
    public async Task ReplaceQrToken_NewTokenResolves()
    {
        var oldToken = LocationQrValue.GenerateToken();
        var locationId = Guid.NewGuid();
        var location = StorageLocation.Create(Guid.NewGuid(), "TestLocation");
        location.SetQrToken(oldToken);

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync(location);

        var replaceResult = await _service.ReplaceQrTokenAsync(locationId);
        Assert.True(replaceResult.Success);
        var newToken = LocationQrValue.TryParseQrValue(replaceResult.Data);
        var newQrValue = LocationQrValue.FormatQrValue(newToken);

        _locationRepoMock.Setup(r => r.SingleOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<StorageLocation, bool>>>(), default))
            .ReturnsAsync(location);

        var resolveNewResult = await _service.ResolveQrValueAsync(newQrValue);

        Assert.Equal(QrStatus.Linked, resolveNewResult.Status);
        Assert.Equal(location.Id, resolveNewResult.LinkedLocationId);
    }

    [Fact]
    public async Task ReplaceQrToken_ReturnsErrorForMissingLocation()
    {
        var locationId = Guid.NewGuid();
        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, default))
            .ReturnsAsync((StorageLocation?)null);

        var result = await _service.ReplaceQrTokenAsync(locationId);

        Assert.False(result.Success);
        Assert.Contains("niet gevonden", result.ErrorMessage ?? "");
    }

    [Fact]
    public async Task ReplaceQrToken_SetsStatusToReplaced()
    {
        var oldToken = LocationQrValue.GenerateToken();
        var locationId = Guid.NewGuid();
        var location = StorageLocation.Create(Guid.NewGuid(), "TestLocation");
        location.SetQrToken(oldToken);
        var originalStatus = location.TagStatus;

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        var result = await _service.ReplaceQrTokenAsync(locationId);

        Assert.True(result.Success);
        Assert.Equal(BootManager.Core.Enums.TagStatus.Replaced, location.TagStatus);
    }

    [Fact]
    public async Task ReplaceQrToken_RefusesLocationWithoutToken()
    {
        var locationId = Guid.NewGuid();
        var location = StorageLocation.Create(Guid.NewGuid(), "TestLocation");
        Assert.Null(location.QrToken);

        _locationRepoMock.Setup(r => r.GetByIdAsync(locationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(location);

        var result = await _service.ReplaceQrTokenAsync(locationId);

        Assert.False(result.Success);
        Assert.Contains("nog geen QR-token", result.ErrorMessage ?? "");
        _locationRepoMock.Verify(r => r.UpdateAsync(It.IsAny<StorageLocation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void DomainModel_ReplaceQrToken_RequiresExistingToken()
    {
        var location = StorageLocation.Create(Guid.NewGuid(), "TestLocation");
        Assert.Null(location.QrToken);

        var newToken = LocationQrValue.GenerateToken();
        var ex = Assert.Throws<InvalidOperationException>(() => location.ReplaceQrToken(newToken));
        Assert.Contains("bestaand token", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
