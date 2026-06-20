using BootManager.Core.Entities;
using BootManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Data.Sqlite;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace BootManager.IntegrationTests.Inventory;

/// <summary>
/// Integration tests for Stock migration: verifies schema, unique constraint on product+location,
/// and that existing data is preserved during upgrade.
/// </summary>
public class StockMigrationTests
{
    [Fact]
    public async Task Migration_CreatesStockTable_WithCorrectSchema()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<BootManagerDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new BootManagerDbContext(options))
        {
            context.Database.Migrate();

            var stockTable = context.Model.FindEntityType(typeof(Stock));
            Assert.NotNull(stockTable);

            var idProperty = stockTable.FindProperty("Id");
            Assert.NotNull(idProperty);

            var productIdProperty = stockTable.FindProperty("ProductId");
            Assert.NotNull(productIdProperty);

            var locationIdProperty = stockTable.FindProperty("StorageLocationId");
            Assert.NotNull(locationIdProperty);

            var quantityProperty = stockTable.FindProperty("Quantity");
            Assert.NotNull(quantityProperty);
        }
    }

    [Fact]
    public async Task Migration_EnforcesUniqueConstraint_OnProductAndLocationCombination()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<BootManagerDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new BootManagerDbContext(options))
        {
            context.Database.Migrate();

            // Create unit
            var unit = Unit.Create("Stuk");
            context.Units.Add(unit);
            await context.SaveChangesAsync();

            // Create product
            var product = Product.Create("TestProduct", null, unit.Id);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Create storage area
            var area = StorageArea.Create("TestArea");
            context.StorageAreas.Add(area);
            await context.SaveChangesAsync();

            // Create storage location
            var location = StorageLocation.Create(area.Id, "TestLocation");
            context.StorageLocations.Add(location);
            await context.SaveChangesAsync();

            // Create first stock entry
            var stock1 = Stock.Create(product.Id, location.Id, 10);
            context.Stocks.Add(stock1);
            await context.SaveChangesAsync();

            // Try to create second stock entry with same product and location
            var stock2 = Stock.Create(product.Id, location.Id, 5);
            context.Stocks.Add(stock2);

            // Should throw due to unique constraint
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
                await context.SaveChangesAsync());
        }
    }

    [Fact]
    public async Task Migration_PreservesExistingData_WhenUpgradingFromPreviousMigration()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<BootManagerDbContext>()
            .UseSqlite(connection)
            .Options;

        // Create initial database at the previous migration state (20260620120948_AddInventoryEntities)
        // by migrating to just before the stock migration
        using (var context = new BootManagerDbContext(options))
        {
            var serviceProvider = ((IInfrastructure<IServiceProvider>)context).Instance;
            var migrator = serviceProvider.GetRequiredService<IMigrator>();
            await migrator.MigrateAsync("20260620120948_AddInventoryEntities");

            // Verify initial migrations were applied
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            Assert.Contains("20260620120948_AddInventoryEntities", appliedMigrations);
            Assert.DoesNotContain("20260620152203_AddStockEntities", appliedMigrations);

            // Add initial data before stock migration
            var unit = Unit.Create("Stuk");
            context.Units.Add(unit);

            var product = Product.Create("Product1", "Description", unit.Id);
            context.Products.Add(product);

            var area = StorageArea.Create("Area1");
            context.StorageAreas.Add(area);

            var location = StorageLocation.Create(area.Id, "Location1");
            context.StorageLocations.Add(location);

            await context.SaveChangesAsync();
        }

        // Now apply the stock migration upgrade
        using (var context = new BootManagerDbContext(options))
        {
            var serviceProvider = ((IInfrastructure<IServiceProvider>)context).Instance;
            var migrator = serviceProvider.GetRequiredService<IMigrator>();
            await migrator.MigrateAsync();

            // Verify the stock migration was applied
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            Assert.Contains("20260620120948_AddInventoryEntities", appliedMigrations);
            Assert.Contains("20260620152203_AddStockEntities", appliedMigrations);

            // Verify existing data is still there after upgrade
            var unitsCount = await context.Units.CountAsync();
            var productsCount = await context.Products.CountAsync();
            var areasCount = await context.StorageAreas.CountAsync();
            var locationsCount = await context.StorageLocations.CountAsync();

            Assert.Equal(1, unitsCount);
            Assert.Equal(1, productsCount);
            Assert.Equal(1, areasCount);
            Assert.Equal(1, locationsCount);

            // Verify Stocks table exists and is empty (as expected after upgrade)
            var stocksCount = await context.Stocks.CountAsync();
            Assert.Equal(0, stocksCount);
        }
    }

    [Fact]
    public async Task Stock_CanBeCreatedAndRetrieved()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<BootManagerDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new BootManagerDbContext(options))
        {
            context.Database.Migrate();

            var unit = Unit.Create("Stuk");
            context.Units.Add(unit);

            var product = Product.Create("TestProduct", null, unit.Id);
            context.Products.Add(product);

            var area = StorageArea.Create("TestArea");
            context.StorageAreas.Add(area);

            var location = StorageLocation.Create(area.Id, "TestLocation");
            context.StorageLocations.Add(location);

            await context.SaveChangesAsync();

            var stock = Stock.Create(product.Id, location.Id, 25);
            context.Stocks.Add(stock);
            await context.SaveChangesAsync();

            var retrievedStock = await context.Stocks
                .FirstOrDefaultAsync(s => s.ProductId == product.Id && s.StorageLocationId == location.Id);

            Assert.NotNull(retrievedStock);
            Assert.Equal(25, retrievedStock.Quantity);
        }
    }

    [Fact]
    public async Task Stock_DeleteCascade_DeletesStockWhenProductDeleted()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<BootManagerDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new BootManagerDbContext(options))
        {
            context.Database.Migrate();

            var unit = Unit.Create("Stuk");
            context.Units.Add(unit);

            var product = Product.Create("TestProduct", null, unit.Id);
            context.Products.Add(product);

            var area = StorageArea.Create("TestArea");
            context.StorageAreas.Add(area);

            var location = StorageLocation.Create(area.Id, "TestLocation");
            context.StorageLocations.Add(location);

            await context.SaveChangesAsync();

            var stock = Stock.Create(product.Id, location.Id, 10);
            context.Stocks.Add(stock);
            await context.SaveChangesAsync();

            var stockId = stock.Id;

            // Delete product
            context.Products.Remove(product);
            await context.SaveChangesAsync();

            // Verify stock is also deleted
            var deletedStock = await context.Stocks
                .FirstOrDefaultAsync(s => s.Id == stockId);

            Assert.Null(deletedStock);
        }
    }
}
