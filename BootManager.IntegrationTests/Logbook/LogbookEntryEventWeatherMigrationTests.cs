using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace BootManager.IntegrationTests.Logbook;

/// <summary>
/// Migratietests voor <c>AddLogbookEntryEventAndWeather</c>: bewijzen dat het upgradepad vanaf
/// <c>20260621074251_AddStockExpectedLocations</c> bestaande logboekregels behoudt, dat de nieuwe
/// kolommen ontstaan en dat oude regels null-veilig blijven voor gebeurtenis en weerconditie.
/// </summary>
public class LogbookEntryEventWeatherMigrationTests
{
    private const string PreviousMigration = "20260621074251_AddStockExpectedLocations";
    private const string TargetMigration = "20260717092947_AddLogbookEntryEventAndWeather";

    [Fact]
    public async Task Upgrade_PreservesExistingEntry_AddsColumns_AndKeepsEventWeatherNullForOldRows()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<BootManagerDbContext>()
            .UseSqlite(connection)
            .Options;

        int tripId;

        // 1) Migreer expliciet naar de vorige migratie (vóór de nieuwe kolommen bestaan).
        using (var context = new BootManagerDbContext(options))
        {
            var migrator = ((IInfrastructure<IServiceProvider>)context).Instance.GetRequiredService<IMigrator>();
            await migrator.MigrateAsync(PreviousMigration);

            var appliedBefore = await context.Database.GetAppliedMigrationsAsync();
            Assert.Contains(PreviousMigration, appliedBefore);
            Assert.DoesNotContain(TargetMigration, appliedBefore);

            // De nieuwe kolommen bestaan nog niet in deze staat.
            var columnsBefore = await GetLogbookEntryColumnsAsync(connection);
            Assert.DoesNotContain("EventType", columnsBefore);
            Assert.DoesNotContain("WeatherCondition", columnsBefore);

            // Voeg vooraf bestaande data toe: een reis (schema ongewijzigd) en een bestaande regel.
            var trip = new LogbookTrip(name: "Bestaande reis", departureUtc: new DateTime(2026, 7, 16, 8, 0, 0, DateTimeKind.Utc));
            context.LogbookTrips.Add(trip);
            await context.SaveChangesAsync();
            tripId = trip.Id;

            // De bestaande logboekregel wordt via raw SQL toegevoegd met alleen de kolommen die
            // in de vorige migratie bestaan (de nieuwe kolommen zijn er nog niet).
            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO LogbookEntries (LogbookTripId, EntryTimeUtc, Status, CreatedAtUtc, UpdatedAtUtc, Remarks) " +
                "VALUES ({0}, {1}, {2}, {3}, {4}, {5})",
                tripId,
                new DateTime(2026, 7, 16, 10, 0, 0, DateTimeKind.Utc),
                (int)LogbookEntryStatus.Confirmed,
                new DateTime(2026, 7, 16, 9, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 7, 16, 9, 0, 0, DateTimeKind.Utc),
                "Bestaande regel");
        }

        // 2) Migreer naar de laatste migratie (past de nieuwe kolommen toe).
        using (var context = new BootManagerDbContext(options))
        {
            var migrator = ((IInfrastructure<IServiceProvider>)context).Instance.GetRequiredService<IMigrator>();
            await migrator.MigrateAsync();

            var appliedAfter = await context.Database.GetAppliedMigrationsAsync();
            Assert.Contains(PreviousMigration, appliedAfter);
            Assert.Contains(TargetMigration, appliedAfter);

            // De nieuwe kolommen bestaan nu.
            var columnsAfter = await GetLogbookEntryColumnsAsync(connection);
            Assert.Contains("EventType", columnsAfter);
            Assert.Contains("WeatherCondition", columnsAfter);

            // De vooraf toegevoegde regel is behouden en null-veilig voor de nieuwe waarden.
            var entries = await context.LogbookEntries.ToListAsync();
            Assert.Single(entries);
            var preserved = entries[0];
            Assert.Equal("Bestaande regel", preserved.Remarks);
            Assert.Equal(tripId, preserved.LogbookTripId);
            Assert.Equal(LogbookEntryStatus.Confirmed, preserved.Status);
            Assert.Null(preserved.EventType);
            Assert.Null(preserved.WeatherCondition);
        }
    }

    [Fact]
    public async Task Upgrade_AllowsPersistingStableEventAndWeatherValues_AfterMigration()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<BootManagerDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new BootManagerDbContext(options);
        await context.Database.MigrateAsync();

        var trip = new LogbookTrip(name: "Reis", departureUtc: new DateTime(2026, 7, 16, 8, 0, 0, DateTimeKind.Utc));
        context.LogbookTrips.Add(trip);
        await context.SaveChangesAsync();

        var entry = new LogbookEntry(
            logbookTripId: trip.Id,
            entryTimeUtc: new DateTime(2026, 7, 16, 10, 0, 0, DateTimeKind.Utc),
            remarks: "Overstag bij de ton",
            eventType: LogbookEventType.Overstag,
            weatherCondition: LogbookWeatherCondition.HalfBewolkt);
        context.LogbookEntries.Add(entry);
        await context.SaveChangesAsync();

        // Herlaad uit een verse context om de daadwerkelijk opgeslagen kolomwaarden te bewijzen.
        using var verify = new BootManagerDbContext(options);
        var stored = await verify.LogbookEntries.SingleAsync();
        Assert.Equal(LogbookEventType.Overstag, stored.EventType);
        Assert.Equal(LogbookWeatherCondition.HalfBewolkt, stored.WeatherCondition);
        Assert.Equal("Overstag bij de ton", stored.Remarks);

        // De weerwaarde is als stabiele integer-domeinwaarde opgeslagen, niet als icoon/label.
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT WeatherCondition FROM LogbookEntries WHERE Id = " + stored.Id;
        var raw = await cmd.ExecuteScalarAsync();
        Assert.Equal((long)LogbookWeatherCondition.HalfBewolkt, Convert.ToInt64(raw));
    }

    private static async Task<List<string>> GetLogbookEntryColumnsAsync(SqliteConnection connection)
    {
        var columns = new List<string>();
        await using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA table_info('LogbookEntries');";
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            // Kolom 1 van PRAGMA table_info is de kolomnaam.
            columns.Add(reader.GetString(1));
        }
        return columns;
    }
}
