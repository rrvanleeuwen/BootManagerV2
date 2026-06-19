using BootManager.Application.Storage.QrFormat;
using BootManager.Application.Storage.Services;
using BootManager.Core.Entities;
using BootManager.Infrastructure.Persistence;
using BootManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BootManager.IntegrationTests.Storage;

/// <summary>
/// Integration tests for QR token functionality on real SQLite database.
/// Tests migration, unique constraint, and complete QR workflows.
/// Each test class instance gets its own temporary database file.
/// </summary>
public class StorageQrTokenIntegrationTests : IAsyncLifetime
{
    private string _dbPath = null!;
    private BootManagerDbContext _context = null!;
    private IStorageService _service = null!;

    public async Task InitializeAsync()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"bootmanager_test_{Guid.NewGuid()}.db");
        var connectionString = $"DataSource={_dbPath}";

        var options = new DbContextOptionsBuilder<BootManagerDbContext>()
            .UseSqlite(connectionString)
            .Options;

        _context = new BootManagerDbContext(options);
        await _context.Database.MigrateAsync();

        var areaRepo = new EfRepository<StorageArea>(_context);
        var locationRepo = new EfRepository<StorageLocation>(_context);
        _service = new StorageService(areaRepo, locationRepo);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        try
        {
            if (File.Exists(_dbPath))
                File.Delete(_dbPath);
        }
        catch { }
    }

    [Fact]
    public async Task Migration_CreatesQrTokenColumn()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location = StorageLocation.Create(area.Id, "TestLocation");
        _context.StorageLocations.Add(location);
        await _context.SaveChangesAsync();

        var retrieved = await _context.StorageLocations.FindAsync(location.Id);
        Assert.NotNull(retrieved);
        Assert.Null(retrieved.QrToken);
    }

    [Fact]
    public async Task UniqueConstraint_AllowsMultipleNullTokens()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location1 = StorageLocation.Create(area.Id, "Location1");
        var location2 = StorageLocation.Create(area.Id, "Location2");
        _context.StorageLocations.AddRange(location1, location2);

        var ex = await Record.ExceptionAsync(() => _context.SaveChangesAsync());
        Assert.Null(ex);
    }

    [Fact]
    public async Task UniqueConstraint_RejectsNonUniqueToken()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var token = LocationQrValue.GenerateToken();
        var location1 = StorageLocation.Create(area.Id, "Location1");
        location1.SetQrToken(token);
        var location2 = StorageLocation.Create(area.Id, "Location2");
        location2.SetQrToken(token);

        _context.StorageLocations.AddRange(location1, location2);

        var ex = await Record.ExceptionAsync(() => _context.SaveChangesAsync());
        Assert.NotNull(ex);
    }

    [Fact]
    public async Task GenerateQrToken_Idempotent_SameTokenAfterSecondCall()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location = StorageLocation.Create(area.Id, "TestLocation");
        _context.StorageLocations.Add(location);
        await _context.SaveChangesAsync();

        var result1 = await _service.GenerateOrGetQrTokenAsync(location.Id);
        var result2 = await _service.GenerateOrGetQrTokenAsync(location.Id);

        Assert.True(result1.Success);
        Assert.True(result2.Success);
        Assert.Equal(result1.Data, result2.Data);
    }

    [Fact]
    public async Task LinkQrAndNavigate_KnownTokenReturnsLocationId()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location = StorageLocation.Create(area.Id, "TestLocation");
        var token = LocationQrValue.GenerateToken();
        location.SetQrToken(token);
        _context.StorageLocations.Add(location);
        await _context.SaveChangesAsync();

        var qrValue = LocationQrValue.FormatQrValue(token);
        var resolution = await _service.ResolveQrValueAsync(qrValue);

        Assert.Equal(location.Id, resolution.LinkedLocationId);
    }

    [Fact]
    public async Task CreateLocationWithQrToken_AtomicInsertWithToken()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var token = LocationQrValue.GenerateToken();
        var result = await _service.CreateLocationWithQrTokenAsync(area.Id, "NewLocation", "Description", token);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        var retrieved = await _context.StorageLocations.FindAsync(result.Data.Id);
        Assert.Equal(token, retrieved?.QrToken);
    }

    [Fact]
    public async Task TokenRename_TokenUnchangedAfterLocationRename()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location = StorageLocation.Create(area.Id, "OriginalName");
        var token = LocationQrValue.GenerateToken();
        location.SetQrToken(token);
        _context.StorageLocations.Add(location);
        await _context.SaveChangesAsync();

        location.UpdateNameAndDescription("NewName", "Description");
        _context.StorageLocations.Update(location);
        await _context.SaveChangesAsync();

        var retrieved = await _context.StorageLocations.FindAsync(location.Id);
        Assert.Equal(token, retrieved?.QrToken);
        Assert.Equal("NewName", retrieved?.Name);
    }

    [Fact]
    public async Task TokenAndMove_TokenUnchangedAfterAreaMove()
    {
        var area1 = StorageArea.Create("Area1");
        var area2 = StorageArea.Create("Area2");
        _context.StorageAreas.AddRange(area1, area2);
        await _context.SaveChangesAsync();

        var location = StorageLocation.Create(area1.Id, "Location");
        var token = LocationQrValue.GenerateToken();
        location.SetQrToken(token);
        _context.StorageLocations.Add(location);
        await _context.SaveChangesAsync();

        location.MoveToArea(area2.Id);
        _context.StorageLocations.Update(location);
        await _context.SaveChangesAsync();

        var retrieved = await _context.StorageLocations.FindAsync(location.Id);
        Assert.Equal(token, retrieved?.QrToken);
        Assert.Equal(area2.Id, retrieved?.StorageAreaId);
    }

    [Fact]
    public async Task PreexistingDataMigration_LocationsWithoutTokens()
    {
        var area = StorageArea.Create("PreexistingArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location = StorageLocation.Create(area.Id, "PreexistingLocation");
        _context.StorageLocations.Add(location);
        await _context.SaveChangesAsync();

        var retrieved = await _context.StorageLocations.FindAsync(location.Id);
        Assert.Null(retrieved?.QrToken);
    }

    [Fact]
    public async Task LinkQrToExistingLocation_RefusesExistingToken()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location = StorageLocation.Create(area.Id, "TestLocation");
        _context.StorageLocations.Add(location);
        await _context.SaveChangesAsync();

        var token = LocationQrValue.GenerateToken();
        var result = await _service.LinkQrToExistingLocationAsync(token, location.Id);
        Assert.True(result.Success);

        var otherToken = LocationQrValue.GenerateToken();
        var result2 = await _service.LinkQrToExistingLocationAsync(otherToken, location.Id);
        Assert.False(result2.Success);
        Assert.Contains("al een QR-token", result2.ErrorMessage ?? "");
    }

    [Fact]
    public async Task DuplicateTokenRace_TranslatesToFunctionalError()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location1 = StorageLocation.Create(area.Id, "Location1");
        var location2 = StorageLocation.Create(area.Id, "Location2");
        _context.StorageLocations.AddRange(location1, location2);
        await _context.SaveChangesAsync();

        var token = LocationQrValue.GenerateToken();

        // First link succeeds
        var result1 = await _service.LinkQrToExistingLocationAsync(token, location1.Id);
        Assert.True(result1.Success);

        // Manually bypass the pre-check by directly setting the token on location2
        // to simulate a race condition where another thread linked the same token
        location2.SetQrToken(token);
        _context.StorageLocations.Update(location2);

        var dbEx = await Record.ExceptionAsync(() => _context.SaveChangesAsync());
        Assert.NotNull(dbEx); // The constraint violation happens

        // Reload location2 to clear the context state
        _context.ChangeTracker.Clear();
        location2 = await _context.StorageLocations.FindAsync(location2.Id);

        // Now verify that the service properly translates the race when trying to link
        // a token that was already linked between the pre-check and the update
        var newLocation = StorageLocation.Create(area.Id, "Location3");
        _context.StorageLocations.Add(newLocation);
        await _context.SaveChangesAsync();

        // Attempt to link the already-linked token; should return functional error not exception
        var result2 = await _service.LinkQrToExistingLocationAsync(token, newLocation.Id);

        // The service should return failure, not throw an exception
        Assert.False(result2.Success);
        Assert.NotNull(result2.ErrorMessage);
        Assert.True(
            result2.ErrorMessage.Contains("al een") ||
            result2.ErrorMessage.Contains("gekoppeld") ||
            result2.ErrorMessage.Contains("reeds"),
            $"Expected functional error message about existing token, got: {result2.ErrorMessage}");
    }

    [Fact]
    public async Task AcceptancePath_LinkUnknownQrToExistingLocation_ThenFreshReload()
    {
        // Setup: Create area and location without token
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location = StorageLocation.Create(area.Id, "TestLocation");
        _context.StorageLocations.Add(location);
        await _context.SaveChangesAsync();

        Assert.Null(location.QrToken);

        // Link an unknown valid BootManager QR to this existing location
        var unknownToken = LocationQrValue.GenerateToken();
        var linkResult = await _service.LinkQrToExistingLocationAsync(unknownToken, location.Id);
        Assert.True(linkResult.Success, $"Link failed: {linkResult.ErrorMessage}");

        // Fresh reload of location detail from database
        var detailResult = await _service.GetLocationDetailAsync(location.Id);
        Assert.True(detailResult.Success);
        Assert.NotNull(detailResult.Data);

        // The location should now have the QrValue in the detail
        var expectedQrValue = LocationQrValue.FormatQrValue(unknownToken);
        Assert.Equal(expectedQrValue, detailResult.Data.QrValue);
        Assert.False(string.IsNullOrEmpty(detailResult.Data.QrValue),
            "QrValue should not be empty after linking and reloading");

        // Verify scanning the same token now resolves to this location
        var scanResolution = await _service.ResolveQrValueAsync(expectedQrValue);
        Assert.Equal(location.Id, scanResolution.LinkedLocationId);
    }

    [Fact]
    public async Task AcceptancePath_TwoSeparateScopes_LinkThenReload()
    {
        // This test simulates the real web flow: link in one scope, reload in another scope
        var dbPath = Path.Combine(Path.GetTempPath(), $"bootmanager_scope_test_{Guid.NewGuid()}.db");
        var connectionString = $"DataSource={dbPath}";

        try
        {
            var areaId = Guid.NewGuid();
            var locationId = Guid.NewGuid();

            // Scope 1: Setup
            {
                var options1 = new DbContextOptionsBuilder<BootManagerDbContext>()
                    .UseSqlite(connectionString)
                    .Options;
                await using (var ctx1 = new BootManagerDbContext(options1))
                {
                    await ctx1.Database.MigrateAsync();
                    var area = StorageArea.Create("TestArea");
                    area.GetType().GetProperty("Id")!.SetValue(area, areaId);
                    var location = StorageLocation.Create(areaId, "TestLocation");
                    location.GetType().GetProperty("Id")!.SetValue(location, locationId);
                    ctx1.StorageAreas.Add(area);
                    ctx1.StorageLocations.Add(location);
                    await ctx1.SaveChangesAsync();
                }
            }

            // Scope 2: Link the QR
            var linkedToken = LocationQrValue.GenerateToken();
            {
                var options2 = new DbContextOptionsBuilder<BootManagerDbContext>()
                    .UseSqlite(connectionString)
                    .Options;
                await using (var ctx2 = new BootManagerDbContext(options2))
                {
                    var service2 = new StorageService(
                        new EfRepository<StorageArea>(ctx2),
                        new EfRepository<StorageLocation>(ctx2));

                    var linkResult = await service2.LinkQrToExistingLocationAsync(linkedToken, locationId);
                    Assert.True(linkResult.Success, $"Link failed: {linkResult.ErrorMessage}");
                }
            }

            // Scope 3: Reload and verify in a completely fresh context
            {
                var options3 = new DbContextOptionsBuilder<BootManagerDbContext>()
                    .UseSqlite(connectionString)
                    .Options;
                await using (var ctx3 = new BootManagerDbContext(options3))
                {
                    var service3 = new StorageService(
                        new EfRepository<StorageArea>(ctx3),
                        new EfRepository<StorageLocation>(ctx3));

                    var detailResult = await service3.GetLocationDetailAsync(locationId);
                    Assert.True(detailResult.Success);
                    Assert.NotNull(detailResult.Data);

                    var expectedQrValue = LocationQrValue.FormatQrValue(linkedToken);
                    Assert.Equal(expectedQrValue, detailResult.Data.QrValue);
                    Assert.False(string.IsNullOrEmpty(detailResult.Data.QrValue),
                        "QrValue should be persisted and readable in a fresh context");
                }
            }
        }
        finally
        {
            try
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
            catch { }
        }
    }

    [Fact]
    public async Task MigrationUpgradePath_FromAddStorageAreasAndLocations_PreservesExistingData()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"bootmanager_upgrade_test_{Guid.NewGuid()}.db");
        var connectionString = $"DataSource={dbPath}";
        var areaIdBeforeUpgrade = Guid.NewGuid();
        var locationIdBeforeUpgrade = Guid.NewGuid();
        const string areaNameBefore = "PreexistingArea";
        const string locationNameBefore = "PreexistingLocation";
        const string descriptionBefore = "Preexisting description";

        try
        {
            var optionsA = new DbContextOptionsBuilder<BootManagerDbContext>()
                .UseSqlite(connectionString)
                .Options;

            await using (var contextA = new BootManagerDbContext(optionsA))
            {
                var migrator = contextA.GetService<IMigrator>();
                await migrator.MigrateAsync("20260618175732_AddStorageAreasAndLocations");

                var appliedBefore = (await contextA.Database.GetAppliedMigrationsAsync()).ToList();
                Assert.Contains("20260618175732_AddStorageAreasAndLocations", appliedBefore);
                Assert.DoesNotContain("20260618192723_AddStorageLocationQrToken", appliedBefore);

                await contextA.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO StorageAreas (Id, Name, NormalizedName)
VALUES ({areaIdBeforeUpgrade}, {areaNameBefore}, {"preexistingarea"});");

                await contextA.Database.ExecuteSqlInterpolatedAsync($@"
INSERT INTO StorageLocations (Id, StorageAreaId, Name, NormalizedName, Description)
VALUES ({locationIdBeforeUpgrade}, {areaIdBeforeUpgrade}, {locationNameBefore}, {"preexistinglocation"}, {descriptionBefore});");
            }

            var optionsB = new DbContextOptionsBuilder<BootManagerDbContext>()
                .UseSqlite(connectionString)
                .Options;

            await using (var contextB = new BootManagerDbContext(optionsB))
            {
                await contextB.Database.MigrateAsync();

                var appliedAfter = (await contextB.Database.GetAppliedMigrationsAsync()).ToList();
                Assert.Contains("20260618192723_AddStorageLocationQrToken", appliedAfter);

                var retrievedArea = await contextB.StorageAreas.FindAsync(areaIdBeforeUpgrade);
                var retrievedLocation = await contextB.StorageLocations.FindAsync(locationIdBeforeUpgrade);

                Assert.NotNull(retrievedArea);
                Assert.NotNull(retrievedLocation);
                Assert.Equal(areaNameBefore, retrievedArea!.Name);
                Assert.Equal(locationNameBefore, retrievedLocation!.Name);
                Assert.Equal(descriptionBefore, retrievedLocation.Description);
                Assert.Equal(areaIdBeforeUpgrade, retrievedLocation.StorageAreaId);
                Assert.Null(retrievedLocation.QrToken);

                var service = new StorageService(
                    new EfRepository<StorageArea>(contextB),
                    new EfRepository<StorageLocation>(contextB));

                var generated = await service.GenerateOrGetQrTokenAsync(locationIdBeforeUpgrade);
                Assert.True(generated.Success);

                var reloaded = await contextB.StorageLocations.FindAsync(locationIdBeforeUpgrade);
                Assert.NotNull(reloaded?.QrToken);
                Assert.Equal(generated.Data, LocationQrValue.FormatQrValue(reloaded!.QrToken!));
            }
        }
        finally
        {
            try
            {
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
            catch { }
        }
    }
}
