using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using BootManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BootManager.IntegrationTests.Storage;

/// <summary>
/// Integratietests voor StorageArea en StorageLocation migrations en constraints.
/// Gebruikt tijdelijke SQLite-databases; raakt geen productie- of Raspberry Pi-database.
/// </summary>
public class StorageMigrationAndConstraintsTests
{
    [Fact]
    public async Task Migration_CreatesStorageAreasAndLocationsTablesSuccessfully()
    {
        await using var factory = new TestFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();

        // Migratie moet succesvol zijn uitgevoerd
        await context.Database.MigrateAsync();

        // Tabellen moeten bestaan en leeg zijn
        var areaCount = await context.StorageAreas.CountAsync();
        var locationCount = await context.StorageLocations.CountAsync();

        Assert.Equal(0, areaCount);
        Assert.Equal(0, locationCount);
    }

    [Fact]
    public async Task UniqueIndex_OnAreaNormalizedName_IsEnforced()
    {
        await using var factory = new TestFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<StorageArea>>();

        await context.Database.MigrateAsync();

        var area1 = StorageArea.Create("Kombuis");
        await repo.AddAsync(area1);

        var area2 = StorageArea.Create("kombuis"); // Case-insensitive duplicate

        // The repository auto-saves, so the error happens in AddAsync
        await Assert.ThrowsAsync<DbUpdateException>(async () => await repo.AddAsync(area2));
    }

    [Fact]
    public async Task CompositeIndex_OnLocationAreaAndNormalizedName_IsEnforced()
    {
        await using var factory = new TestFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
        var areaRepo = scope.ServiceProvider.GetRequiredService<IRepository<StorageArea>>();
        var locRepo = scope.ServiceProvider.GetRequiredService<IRepository<StorageLocation>>();

        await context.Database.MigrateAsync();

        var area = StorageArea.Create("Kombuis");
        await areaRepo.AddAsync(area);

        var location1 = StorageLocation.Create(area.Id, "Kast 1");
        await locRepo.AddAsync(location1);

        // Try to add duplicate: second insert must cause constraint violation
        var location2 = StorageLocation.Create(area.Id, "kast 1"); // Case-insensitive duplicate in same area

        // The repository auto-saves, so the error happens in AddAsync
        await Assert.ThrowsAsync<DbUpdateException>(async () => await locRepo.AddAsync(location2));
    }

    [Fact]
    public async Task CompositeIndex_AllowsSameLocationNameInDifferentAreas()
    {
        await using var factory = new TestFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
        var areaRepo = scope.ServiceProvider.GetRequiredService<IRepository<StorageArea>>();
        var locRepo = scope.ServiceProvider.GetRequiredService<IRepository<StorageLocation>>();

        await context.Database.MigrateAsync();

        var area1 = StorageArea.Create("Kombuis");
        var area2 = StorageArea.Create("Salon");
        await areaRepo.AddAsync(area1);
        await areaRepo.AddAsync(area2);

        var location1 = StorageLocation.Create(area1.Id, "Kast 1");
        var location2 = StorageLocation.Create(area2.Id, "Kast 1");
        await locRepo.AddAsync(location1);
        await locRepo.AddAsync(location2);

        // Beide inserts moeten succesvol zijn
        await context.SaveChangesAsync();

        var count = await context.StorageLocations.CountAsync();
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task ForeignKey_WithRestrictDelete_PreventsDeletionOfAreaWithLocations()
    {
        await using var factory = new TestFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
        var areaRepo = scope.ServiceProvider.GetRequiredService<IRepository<StorageArea>>();
        var locRepo = scope.ServiceProvider.GetRequiredService<IRepository<StorageLocation>>();

        await context.Database.MigrateAsync();

        var area = StorageArea.Create("Kombuis");
        await areaRepo.AddAsync(area);

        var location = StorageLocation.Create(area.Id, "Kast 1");
        await locRepo.AddAsync(location);

        // Probeer het gebied te verwijderen terwijl het locaties bevat
        // The repository auto-saves, so the error happens during DeleteAsync due to restrict policy
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await areaRepo.DeleteAsync(area));
    }

    [Fact]
    public async Task ForeignKey_AllowsDeleteOfAreaAfterRemovingLocations()
    {
        await using var factory = new TestFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
        var areaRepo = scope.ServiceProvider.GetRequiredService<IRepository<StorageArea>>();
        var locRepo = scope.ServiceProvider.GetRequiredService<IRepository<StorageLocation>>();

        await context.Database.MigrateAsync();

        var area = StorageArea.Create("Kombuis");
        await areaRepo.AddAsync(area);

        var location = StorageLocation.Create(area.Id, "Kast 1");
        await locRepo.AddAsync(location);
        await context.SaveChangesAsync();

        // Verwijder de locatie eerst
        await locRepo.DeleteAsync(location);
        await context.SaveChangesAsync();

        // Nu kan het gebied worden verwijderd
        await areaRepo.DeleteAsync(area);
        await context.SaveChangesAsync();

        var areaCount = await context.StorageAreas.CountAsync();
        Assert.Equal(0, areaCount);
    }

    [Fact]
    public async Task LocationId_RemainsStableAfterUpdate()
    {
        await using var factory = new TestFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
        var areaRepo = scope.ServiceProvider.GetRequiredService<IRepository<StorageArea>>();
        var locRepo = scope.ServiceProvider.GetRequiredService<IRepository<StorageLocation>>();

        await context.Database.MigrateAsync();

        var area = StorageArea.Create("Kombuis");
        await areaRepo.AddAsync(area);

        var location = StorageLocation.Create(area.Id, "Kast 1", "Beschrijving");
        var originalId = location.Id;
        await locRepo.AddAsync(location);
        await context.SaveChangesAsync();

        // Update location name
        location.UpdateNameAndDescription("Kast 2", "Nieuwe beschrijving");
        await locRepo.UpdateAsync(location);
        await context.SaveChangesAsync();

        // ID moet hetzelfde zijn
        Assert.Equal(originalId, location.Id);

        // Verify in database
        var updatedLocation = await locRepo.GetByIdAsync(originalId);
        Assert.NotNull(updatedLocation);
        Assert.Equal("Kast 2", updatedLocation.Name);
        Assert.Equal(originalId, updatedLocation.Id);
    }

    [Fact]
    public async Task LocationId_RemainsStableAfterMove()
    {
        await using var factory = new TestFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
        var areaRepo = scope.ServiceProvider.GetRequiredService<IRepository<StorageArea>>();
        var locRepo = scope.ServiceProvider.GetRequiredService<IRepository<StorageLocation>>();

        await context.Database.MigrateAsync();

        var area1 = StorageArea.Create("Kombuis");
        var area2 = StorageArea.Create("Salon");
        await areaRepo.AddAsync(area1);
        await areaRepo.AddAsync(area2);

        var location = StorageLocation.Create(area1.Id, "Kast 1");
        var originalId = location.Id;
        await locRepo.AddAsync(location);
        await context.SaveChangesAsync();

        // Move to another area
        location.MoveToArea(area2.Id);
        await locRepo.UpdateAsync(location);
        await context.SaveChangesAsync();

        // ID moet hetzelfde zijn
        Assert.Equal(originalId, location.Id);

        // Verify in database
        var movedLocation = await locRepo.GetByIdAsync(originalId);
        Assert.NotNull(movedLocation);
        Assert.Equal(area2.Id, movedLocation.StorageAreaId);
        Assert.Equal(originalId, movedLocation.Id);
    }

    [Fact]
    public async Task OptionalDescription_CanBeNull()
    {
        await using var factory = new TestFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
        var areaRepo = scope.ServiceProvider.GetRequiredService<IRepository<StorageArea>>();
        var locRepo = scope.ServiceProvider.GetRequiredService<IRepository<StorageLocation>>();

        await context.Database.MigrateAsync();

        var area = StorageArea.Create("Kombuis");
        await areaRepo.AddAsync(area);

        var location = StorageLocation.Create(area.Id, "Kast 1", null);
        await locRepo.AddAsync(location);
        await context.SaveChangesAsync();

        var retrieved = await locRepo.GetByIdAsync(location.Id);
        Assert.Null(retrieved!.Description);
    }

    [Fact]
    public async Task Migration_PreservesExistingDataFromPreviousMigration()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"bm_migrate_proof_{Guid.NewGuid():N}.db");
        try
        {
            // Step 1-3: Create temp DB, migrate to previous migration, verify
            var optionsFirst = new DbContextOptionsBuilder<BootManagerDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            Guid savedVesselId = Guid.Empty;

            using (var contextFirst = new BootManagerDbContext(optionsFirst))
            {
                // Step 3: Use IMigrator to migrate to the SPECIFIC previous migration
                var migrator = ((IInfrastructure<IServiceProvider>)contextFirst).Instance.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
                await migrator.MigrateAsync("20260609204357_MigrateOwnerProfileToLocalUser");

                // Step 5: Assert previous migration IS applied, new storage migration is NOT
                var historyRepo = ((IInfrastructure<IServiceProvider>)contextFirst).Instance.GetService<Microsoft.EntityFrameworkCore.Migrations.IHistoryRepository>();
                var appliedMigrations = (await historyRepo.GetAppliedMigrationsAsync(default)).Select(h => h.MigrationId).ToList();

                Assert.Contains("20260609204357_MigrateOwnerProfileToLocalUser", appliedMigrations);
                Assert.DoesNotContain("20260618175732_AddStorageAreasAndLocations", appliedMigrations);

                // Step 6: Add VesselProfile with recognizable values before storage tables exist
                var vessel = VesselProfile.Create(
                    vesselName: "Linde",
                    homePort: "Amsterdam",
                    callSign: "LINDE",
                    mmsi: "123456789",
                    createdUtc: DateTime.UtcNow,
                    currentEngineHours: 1000m,
                    currentLogstand: 5000m);
                contextFirst.VesselProfiles.Add(vessel);
                await contextFirst.SaveChangesAsync();

                savedVesselId = vessel.Id;
            }

            // Step 7: Open new context on same database path
            var optionsSecond = new DbContextOptionsBuilder<BootManagerDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            using (var contextSecond = new BootManagerDbContext(optionsSecond))
            {
                // Step 8: Migrate to latest (includes storage tables)
                var migrator = ((IInfrastructure<IServiceProvider>)contextSecond).Instance.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
                await migrator.MigrateAsync();

                // Step 9: Assert new storage migration IS now applied
                var historyRepo = ((IInfrastructure<IServiceProvider>)contextSecond).Instance.GetService<Microsoft.EntityFrameworkCore.Migrations.IHistoryRepository>();
                var appliedMigrations = (await historyRepo.GetAppliedMigrationsAsync(default)).Select(h => h.MigrationId).ToList();

                Assert.Contains("20260618175732_AddStorageAreasAndLocations", appliedMigrations);

                // Step 10: Verify existing VesselProfile is unchanged
                var retrievedVessel = await contextSecond.VesselProfiles.FirstOrDefaultAsync(v => v.Id == savedVesselId);
                Assert.NotNull(retrievedVessel);
                Assert.Equal("Linde", retrievedVessel.VesselName);
                Assert.Equal("Amsterdam", retrievedVessel.HomePort);
                Assert.Equal("LINDE", retrievedVessel.CallSign);
                Assert.Equal("123456789", retrievedVessel.Mmsi);
                Assert.Equal(1000m, retrievedVessel.CurrentEngineHours);
                Assert.Equal(5000m, retrievedVessel.CurrentLogstand);

                // Step 11: Verify StorageAreas and StorageLocations tables exist and work
                var testArea = StorageArea.Create("Test Area");
                contextSecond.StorageAreas.Add(testArea);
                await contextSecond.SaveChangesAsync();

                var location = StorageLocation.Create(testArea.Id, "Test Location", "Test Description");
                contextSecond.StorageLocations.Add(location);
                await contextSecond.SaveChangesAsync();

                // Verify they can be read back
                var areaCount = await contextSecond.StorageAreas.CountAsync();
                var locationCount = await contextSecond.StorageLocations.CountAsync();
                Assert.Equal(1, areaCount);
                Assert.Equal(1, locationCount);

                var readArea = await contextSecond.StorageAreas.FirstAsync();
                var readLocation = await contextSecond.StorageLocations.FirstAsync();
                Assert.Equal("Test Area", readArea.Name);
                Assert.Equal("Test Location", readLocation.Name);
                Assert.Equal("Test Description", readLocation.Description);
            }
        }
        finally
        {
            // Step 12: Cleanup
            try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { }
        }
    }

    /// <summary>
    /// WebApplicationFactory met tijdelijke SQLite-database voor geïsoleerde integratietests.
    /// </summary>
    public sealed class TestFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"bm_storage_{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Bootstrap:DefaultPassword"] = "IntegrationTest99!",
                    ["Jwt:Key"] = "integration_test_jwt_key_32chars!!!!",
                    ["Encryption:Key"] = "IntegrationTestEncryptionKey1234"
                });
            });
            builder.ConfigureTestServices(services =>
            {
                var toRemove = services
                    .Where(d => d.ServiceType == typeof(IDbContextFactory<BootManagerDbContext>) ||
                                d.ServiceType == typeof(BootManagerDbContext))
                    .ToList();
                foreach (var d in toRemove) services.Remove(d);
                services.AddDbContextFactory<BootManagerDbContext>(
                    o => o.UseSqlite($"Data Source={_dbPath}"));
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { }
        }
    }
}
