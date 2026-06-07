using BootManager.Core.Entities;
using BootManager.Infrastructure.Dashboard;
using BootManager.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BootManager.UnitTests.Dashboard;

public sealed class DashboardMeasurementServiceTests : IAsyncLifetime
{
    private readonly string _databasePath =
        Path.Combine(Path.GetTempPath(), $"bootmanager-dashboard-{Guid.NewGuid():N}.db");

    private DbContextOptions<BootManagerDbContext> _options = default!;

    public async Task InitializeAsync()
    {
        _options = new DbContextOptionsBuilder<BootManagerDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;

        await using var db = new BootManagerDbContext(_options);
        await db.Database.EnsureCreatedAsync();

        var older = new WindMeasurement(
            new DateTime(2026, 6, 7, 8, 0, 0, DateTimeKind.Utc),
            "test",
            "older",
            90m,
            5m,
            "m/s");
        var latest = new WindMeasurement(
            new DateTime(2026, 6, 7, 9, 0, 0, DateTimeKind.Utc),
            "test",
            "latest",
            120m,
            7m,
            "m/s");

        db.WindMeasurements.AddRange(older, latest);
        await db.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        SqliteConnection.ClearAllPools();

        if (File.Exists(_databasePath))
            File.Delete(_databasePath);

        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetCurrentMeasurementsAsync_ConcurrentCallsUseSeparateContexts()
    {
        var factory = new CountingDbContextFactory(_options);
        var sut = new DashboardMeasurementService(factory);

        var results = await Task.WhenAll(
            Enumerable.Range(0, 8)
                .Select(_ => sut.GetCurrentMeasurementsAsync()));

        Assert.Equal(8, factory.CreatedContextCount);
        Assert.All(results, result =>
        {
            Assert.Equal(120m, result.Wind.AngleDegrees);
            Assert.Equal(7m, result.Wind.SpeedMetersPerSecond);
            Assert.Equal(
                new DateTime(2026, 6, 7, 9, 0, 0, DateTimeKind.Utc),
                result.Wind.RecordedAtUtc);
        });
    }

    private sealed class CountingDbContextFactory
        : IDbContextFactory<BootManagerDbContext>
    {
        private readonly DbContextOptions<BootManagerDbContext> _options;
        private int _createdContextCount;

        public CountingDbContextFactory(
            DbContextOptions<BootManagerDbContext> options)
        {
            _options = options;
        }

        public int CreatedContextCount => Volatile.Read(ref _createdContextCount);

        public BootManagerDbContext CreateDbContext()
        {
            Interlocked.Increment(ref _createdContextCount);
            return new BootManagerDbContext(_options);
        }
    }
}
