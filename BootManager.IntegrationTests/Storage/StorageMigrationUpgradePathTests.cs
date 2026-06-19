using BootManager.Application.Storage.QrFormat;
using BootManager.Application.Storage.Services;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Infrastructure.Persistence;
using BootManager.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BootManager.IntegrationTests.Storage;

/// <summary>
/// Integration test proving the upgrade path from AddStorageLocationQrToken to AddStorageLocationTagStatus.
/// Demonstrates that existing data is preserved during migration and new features work post-upgrade.
/// </summary>
public class StorageMigrationUpgradePathTests
{
    [Fact]
    public async Task MigrationUpgradePath_PreservesDataAndAllowsReplacement()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"bootmanager_upgrade_test_{Guid.NewGuid()}.db");
        try
        {
            var connectionString = $"DataSource={dbPath}";

            // Step 1: Migrate to AddStorageLocationQrToken (previous migration)
            // Then insert data, then migrate to latest
            var options = new DbContextOptionsBuilder<BootManagerDbContext>()
                .UseSqlite(connectionString)
                .Options;

            using (var context = new BootManagerDbContext(options))
            {
                var migrator = context.GetService<IMigrator>();
                await migrator!.MigrateAsync("20260618192723_AddStorageLocationQrToken");
            }

            // Step 2: Insert data while at AddStorageLocationQrToken
            var area = StorageArea.Create("TestArea");
            var locationWithToken = StorageLocation.Create(area.Id, "LocationWithToken");
            var token1 = LocationQrValue.GenerateToken();
            locationWithToken.SetQrToken(token1);
            var locationWithoutToken = StorageLocation.Create(area.Id, "LocationWithoutToken");

            using (var context = new BootManagerDbContext(options))
            {
                context.StorageAreas.Add(area);
                context.StorageLocations.AddRange(locationWithToken, locationWithoutToken);
                await context.SaveChangesAsync();
            }

            // Step 3: Migrate from old state to latest
            using (var context = new BootManagerDbContext(options))
            {
                var migrator = context.GetService<IMigrator>();
                await migrator!.MigrateAsync();
            }

            // Step 4: Verify data preserved and replacement works on upgraded database
            using (var context = new BootManagerDbContext(options))
            {
                var areaRepo = new EfRepository<StorageArea>(context);
                var locationRepo = new EfRepository<StorageLocation>(context);
                var service = new StorageService(areaRepo, locationRepo);

                var allAfter = await context.StorageLocations.ToListAsync();
                Assert.Equal(2, allAfter.Count);
                Assert.NotNull(allAfter.First(l => l.Name == "LocationWithToken").QrToken);
                Assert.Null(allAfter.First(l => l.Name == "LocationWithoutToken").QrToken);
                Assert.All(allAfter, l => Assert.Equal(TagStatus.NotPrinted, l.TagStatus));

                var toReplace = allAfter.First(l => l.Name == "LocationWithToken");
                var oldToken = toReplace.QrToken;

                var replaceResult = await service.ReplaceQrTokenAsync(toReplace.Id);
                Assert.True(replaceResult.Success);
                var newToken = LocationQrValue.TryParseQrValue(replaceResult.Data);
                Assert.NotEqual(oldToken, newToken);

                var resolveOld = await service.ResolveQrValueAsync(LocationQrValue.FormatQrValue(oldToken!));
                Assert.Null(resolveOld.LinkedLocationId);

                var resolveNew = await service.ResolveQrValueAsync(LocationQrValue.FormatQrValue(newToken!));
                Assert.Equal(toReplace.Id, resolveNew.LinkedLocationId);
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
    public async Task ReplaceQrToken_RefusesLocationWithoutToken_AfterUpgrade()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"bootmanager_upgrade_test2_{Guid.NewGuid()}.db");
        try
        {
            var connectionString = $"DataSource={dbPath}";

            // Migrate to AddStorageLocationQrToken
            var options1 = new DbContextOptionsBuilder<BootManagerDbContext>()
                .UseSqlite(connectionString)
                .Options;

            using (var context = new BootManagerDbContext(options1))
            {
                var migrator = context.GetService<IMigrator>();
                await migrator!.MigrateAsync("20260618192723_AddStorageLocationQrToken");
            }

            // Insert location without token
            using (var context = new BootManagerDbContext(options1))
            {
                var area = StorageArea.Create("TestArea");
                context.StorageAreas.Add(area);
                await context.SaveChangesAsync();

                var location = StorageLocation.Create(area.Id, "NoTokenLocation");
                context.StorageLocations.Add(location);
                await context.SaveChangesAsync();
            }

            // Migrate to latest
            var options2 = new DbContextOptionsBuilder<BootManagerDbContext>()
                .UseSqlite(connectionString)
                .Options;

            using (var context = new BootManagerDbContext(options2))
            {
                var migrator = context.GetService<IMigrator>();
                await migrator!.MigrateAsync();
            }

            // Verify replacement is guarded on upgraded data
            using (var context = new BootManagerDbContext(options2))
            {
                var areaRepo = new EfRepository<StorageArea>(context);
                var locationRepo = new EfRepository<StorageLocation>(context);
                var service = new StorageService(areaRepo, locationRepo);

                var location = await context.StorageLocations.FirstAsync();
                Assert.Null(location.QrToken);

                var replaceResult = await service.ReplaceQrTokenAsync(location.Id);

                Assert.False(replaceResult.Success);
                Assert.Contains("nog geen QR-token", replaceResult.ErrorMessage ?? "");
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
