using BootManager.Application.Storage.QrFormat;
using BootManager.Application.Storage.Services;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Infrastructure.Persistence;
using BootManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BootManager.IntegrationTests.Storage;

/// <summary>
/// Integration tests for QR token replacement and TagStatus persistence on real SQLite database.
/// Tests token replacement lifecycle and status migration.
/// </summary>
public class StorageTokenReplacementIntegrationTests : IAsyncLifetime
{
    private string _dbPath = null!;
    private BootManagerDbContext _context = null!;
    private IStorageService _service = null!;

    public async Task InitializeAsync()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"bootmanager_replace_test_{Guid.NewGuid()}.db");
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
    public async Task ReplaceToken_OldTokenNoLongerResolves()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location = StorageLocation.Create(area.Id, "TestLocation");
        var oldToken = LocationQrValue.GenerateToken();
        location.SetQrToken(oldToken);
        _context.StorageLocations.Add(location);
        await _context.SaveChangesAsync();

        var oldQrValue = LocationQrValue.FormatQrValue(oldToken);
        var resolveBefore = await _service.ResolveQrValueAsync(oldQrValue);
        Assert.Equal(location.Id, resolveBefore.LinkedLocationId);

        var replaceResult = await _service.ReplaceQrTokenAsync(location.Id);
        Assert.True(replaceResult.Success);

        var resolveAfter = await _service.ResolveQrValueAsync(oldQrValue);
        Assert.Null(resolveAfter.LinkedLocationId);
    }

    [Fact]
    public async Task ReplaceToken_NewTokenResolves()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location = StorageLocation.Create(area.Id, "TestLocation");
        var oldToken = LocationQrValue.GenerateToken();
        location.SetQrToken(oldToken);
        _context.StorageLocations.Add(location);
        await _context.SaveChangesAsync();

        var replaceResult = await _service.ReplaceQrTokenAsync(location.Id);
        Assert.True(replaceResult.Success);
        var newToken = LocationQrValue.TryParseQrValue(replaceResult.Data);
        var newQrValue = LocationQrValue.FormatQrValue(newToken);

        var resolveNew = await _service.ResolveQrValueAsync(newQrValue);
        Assert.Equal(location.Id, resolveNew.LinkedLocationId);
    }

    [Fact]
    public async Task ReplaceToken_SetsStatusToReplaced()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location = StorageLocation.Create(area.Id, "TestLocation");
        var oldToken = LocationQrValue.GenerateToken();
        location.SetQrToken(oldToken);
        _context.StorageLocations.Add(location);
        await _context.SaveChangesAsync();

        await _service.ReplaceQrTokenAsync(location.Id);

        var retrieved = await _context.StorageLocations.FindAsync(location.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(TagStatus.Replaced, retrieved.TagStatus);
    }

    [Fact]
    public async Task TagStatus_PersistsAcrossContext()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location = StorageLocation.Create(area.Id, "TestLocation");
        _context.StorageLocations.Add(location);
        await _context.SaveChangesAsync();

        await _service.UpdateTagStatusAsync(location.Id, TagStatus.Printed);

        var retrieved = await _context.StorageLocations.FindAsync(location.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(TagStatus.Printed, retrieved.TagStatus);
    }

    [Fact]
    public async Task TagStatus_UpdatesFromNotPrintedToApplied()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location = StorageLocation.Create(area.Id, "TestLocation");
        Assert.Equal(TagStatus.NotPrinted, location.TagStatus);
        _context.StorageLocations.Add(location);
        await _context.SaveChangesAsync();

        await _service.UpdateTagStatusAsync(location.Id, TagStatus.Applied);

        var retrieved = await _context.StorageLocations.FindAsync(location.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(TagStatus.Applied, retrieved.TagStatus);
    }

    [Fact]
    public async Task Migration_DefaultsTagStatusToNotPrinted()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location = StorageLocation.Create(area.Id, "TestLocation");
        var token = LocationQrValue.GenerateToken();
        location.SetQrToken(token);
        _context.StorageLocations.Add(location);
        await _context.SaveChangesAsync();

        var retrieved = await _context.StorageLocations.FindAsync(location.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(TagStatus.NotPrinted, retrieved.TagStatus);
    }

    [Fact]
    public async Task Migration_PreservesExistingLocationsAndTokens()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location1 = StorageLocation.Create(area.Id, "Location1");
        var token1 = LocationQrValue.GenerateToken();
        location1.SetQrToken(token1);

        var location2 = StorageLocation.Create(area.Id, "Location2");
        var token2 = LocationQrValue.GenerateToken();
        location2.SetQrToken(token2);

        var location3 = StorageLocation.Create(area.Id, "Location3");

        _context.StorageLocations.AddRange(location1, location2, location3);
        await _context.SaveChangesAsync();

        var all = await _context.StorageLocations.Where(l => l.StorageAreaId == area.Id).ToListAsync();
        Assert.Equal(3, all.Count);
        Assert.Equal(token1, all.First(l => l.Id == location1.Id).QrToken);
        Assert.Equal(token2, all.First(l => l.Id == location2.Id).QrToken);
        Assert.Null(all.First(l => l.Id == location3.Id).QrToken);
    }

    [Fact]
    public async Task GetAllLocationsOverview_IncludesTagStatus()
    {
        var area = StorageArea.Create("TestArea");
        _context.StorageAreas.Add(area);
        await _context.SaveChangesAsync();

        var location = StorageLocation.Create(area.Id, "TestLocation");
        var token = LocationQrValue.GenerateToken();
        location.SetQrToken(token);
        location.UpdateTagStatus(TagStatus.Applied);
        _context.StorageLocations.Add(location);
        await _context.SaveChangesAsync();

        var result = await _service.GetAllLocationsOverviewAsync();

        var overview = result.First(r => r.Id == location.Id);
        Assert.Equal(TagStatus.Applied, overview.TagStatus);
        Assert.Equal(LocationQrValue.FormatQrValue(token), overview.QrValue);
    }
}
