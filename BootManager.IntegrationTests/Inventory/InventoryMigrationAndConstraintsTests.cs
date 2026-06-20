using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using BootManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BootManager.IntegrationTests.Inventory;

public class InventoryMigrationAndConstraintsTests
{
    [Fact]
    public async Task Migration_CreatesAllInventoryTables()
    {
        await using var factory = new TestFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();

        await context.Database.MigrateAsync();

        var tables = context.Model.GetEntityTypes().Select(t => t.GetTableName()).ToList();
        Assert.Contains("ProductCategories", tables);
        Assert.Contains("Units", tables);
        Assert.Contains("Products", tables);
        Assert.Contains("ProductCategoryMappings", tables);
        Assert.Contains("ProductCodes", tables);
    }

    [Fact]
    public async Task CategoryNameMustBeUnique()
    {
        await using var factory = new TestFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<ProductCategory>>();

        await context.Database.MigrateAsync();

        var category1 = ProductCategory.Create("Drinken", null, "beverage");
        var category2 = ProductCategory.Create("Drinken", null, "food");

        await repo.AddAsync(category1);

        repo = scope.ServiceProvider.GetRequiredService<IRepository<ProductCategory>>();
        await Assert.ThrowsAsync<DbUpdateException>(async () => await repo.AddAsync(category2));
    }

    [Fact]
    public async Task UnitNameMustBeUnique()
    {
        await using var factory = new TestFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Unit>>();

        await context.Database.MigrateAsync();

        var unit1 = Unit.Create("liter");
        var unit2 = Unit.Create("liter");

        await repo.AddAsync(unit1);

        repo = scope.ServiceProvider.GetRequiredService<IRepository<Unit>>();
        await Assert.ThrowsAsync<DbUpdateException>(async () => await repo.AddAsync(unit2));
    }

    [Fact]
    public async Task ProductCodeValueMustBeUnique()
    {
        await using var factory = new TestFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
        var unitRepo = scope.ServiceProvider.GetRequiredService<IRepository<Unit>>();
        var productRepo = scope.ServiceProvider.GetRequiredService<IRepository<Product>>();
        var codeRepo = scope.ServiceProvider.GetRequiredService<IRepository<ProductCode>>();

        await context.Database.MigrateAsync();

        var unit = Unit.Create("stuk");
        await unitRepo.AddAsync(unit);

        var product1 = Product.Create("Appel", null, unit.Id);
        var product2 = Product.Create("Peer", null, unit.Id);
        await productRepo.AddAsync(product1);
        await productRepo.AddAsync(product2);

        var code1 = ProductCode.Create(product1.Id, "123456789", "barcode");
        var code2 = ProductCode.Create(product2.Id, "123456789", "barcode");

        await codeRepo.AddAsync(code1);

        codeRepo = scope.ServiceProvider.GetRequiredService<IRepository<ProductCode>>();
        await Assert.ThrowsAsync<DbUpdateException>(async () => await codeRepo.AddAsync(code2));
    }

    [Fact]
    public async Task SoftDeleteWorks_ArchiveHidesFromActiveLists()
    {
        await using var factory = new TestFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<ProductCategory>>();

        await context.Database.MigrateAsync();

        var category = ProductCategory.Create("Drinken", null, "beverage");
        await repo.AddAsync(category);

        category.Archive();
        await repo.UpdateAsync(category);

        var activeCategories = await context.ProductCategories.Where(c => c.ArchivedAt == null).ToListAsync();
        Assert.Empty(activeCategories);

        var allCategories = await context.ProductCategories.ToListAsync();
        Assert.Single(allCategories);
    }

    [Fact]
    public async Task CascadeDeleteWorks_DeleteProductCascadesToCode()
    {
        await using var factory = new TestFactory();
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
        var unitRepo = scope.ServiceProvider.GetRequiredService<IRepository<Unit>>();
        var productRepo = scope.ServiceProvider.GetRequiredService<IRepository<Product>>();
        var codeRepo = scope.ServiceProvider.GetRequiredService<IRepository<ProductCode>>();

        await context.Database.MigrateAsync();

        var unit = Unit.Create("stuk");
        await unitRepo.AddAsync(unit);

        var product = Product.Create("Appel", null, unit.Id);
        await productRepo.AddAsync(product);

        var code = ProductCode.Create(product.Id, "123456789", "barcode");
        await codeRepo.AddAsync(code);

        await productRepo.DeleteAsync(product);

        var codes = await context.ProductCodes.ToListAsync();
        Assert.Empty(codes);
    }

    public sealed class TestFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"bm_inventory_test_{Guid.NewGuid():N}.db");

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
