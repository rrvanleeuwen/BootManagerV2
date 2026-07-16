using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Storage.QrFormat;
using BootManager.Core.Entities;
using BootManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BootManager.IntegrationTests.Inventory;

/// <summary>
/// Integratietests voor de CSV-startimport op echte SQLite: bewijst destructieve reset met
/// eenheidsbehoud, opbouw van gebieden/locaties/tokens/producten/voorraad uit mappings en
/// dat een onbekende gescande code daarna aan een geimporteerd product gekoppeld kan worden.
/// Gebruikt tijdelijke SQLite-databases; raakt geen productie- of Raspberry Pi-database.
/// </summary>
public class InventoryImportIntegrationTests
{
    private const string Csv =
        "Aantal;Eenheid;Product;Locatie\n" +
        "4;liter;Rivella;Salonbank, rugleuning\n" +
        "1,5;pak;koffiebonen;Salonbank, rugleuning\n" +
        "1;pak;kaasvlinders;Salon Snackla\n";

    [Fact]
    public async Task ExecuteImport_WipesOldInventory_KeepsUnits_AndBuildsMappedData()
    {
        await using var factory = new TestFactory();

        Guid seededUnitId;
        Guid seededCategoryId;

        // --- Seed: bestaande eenheid + categorie + te wissen voorraadbeheerdata ---
        using (var scope = factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
            await ctx.Database.MigrateAsync();

            var unit = Unit.Create("liter");
            ctx.Units.Add(unit);
            var category = ProductCategory.Create("Bestaande categorie", null, "food");
            ctx.ProductCategories.Add(category);

            var oldArea = StorageArea.Create("Oud gebied");
            ctx.StorageAreas.Add(oldArea);
            var oldLocation = StorageLocation.Create(oldArea.Id, "Oude locatie");
            oldLocation.SetQrToken(LocationQrValue.GenerateToken());
            ctx.StorageLocations.Add(oldLocation);

            var oldProduct = Product.Create("Oud product", null, unit.Id);
            ctx.Products.Add(oldProduct);
            ctx.ProductCodes.Add(ProductCode.Create(oldProduct.Id, "0000000000000", "barcode"));
            ctx.Stocks.Add(Stock.Create(oldProduct.Id, oldLocation.Id, 5m));
            ctx.StockExpectedLocations.Add(StockExpectedLocation.Create(oldProduct.Id, oldLocation.Id));

            await ctx.SaveChangesAsync();

            seededUnitId = unit.Id;
            seededCategoryId = category.Id;
        }

        // --- Act: parse + import in een verse scope ---
        using (var scope = factory.Services.CreateScope())
        {
            var import = scope.ServiceProvider.GetRequiredService<IInventoryImportService>();

            var parse = import.ParseCsv(Csv);
            Assert.True(parse.Success);
            Assert.Equal(3, parse.Rows.Count);
            Assert.Equal(2, parse.DistinctSourceLocations.Count);

            var mappings = new List<InventoryLocationMappingDto>
            {
                new() { SourceLocation = "Salonbank, rugleuning", AreaName = "Salon", LocationName = "Rugleuning" },
                new() { SourceLocation = "Salon Snackla", AreaName = "Salon", LocationName = "Snackla" }
            };

            var result = await import.ExecuteImportAsync(parse.Rows, mappings);

            Assert.True(result.Success, result.ErrorMessage);
            Assert.Equal(1, result.AreasCreated);
            Assert.Equal(2, result.LocationsCreated);
            Assert.Equal(2, result.TokensGenerated);
            Assert.Equal(3, result.ProductsCreated);
            Assert.Equal(3, result.StockRowsCreated);
            Assert.Equal(1, result.UnitsCreated); // 'pak' nieuw; 'liter' hergebruikt
        }

        // --- Assert: eindtoestand op echte database ---
        using (var scope = factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();

            // Oude data verdwenen.
            Assert.False(await ctx.StorageAreas.AnyAsync(a => a.Name == "Oud gebied"));
            Assert.False(await ctx.StorageLocations.AnyAsync(l => l.Name == "Oude locatie"));
            Assert.False(await ctx.Products.AnyAsync(p => p.Name == "Oud product"));
            Assert.False(await ctx.ProductCodes.AnyAsync(c => c.Value == "0000000000000"));

            // Eenheden behouden (zelfde Id) en aangevuld.
            Assert.True(await ctx.Units.AnyAsync(u => u.Id == seededUnitId && u.Name == "liter"));
            Assert.True(await ctx.Units.AnyAsync(u => u.Name == "pak"));

            // Categorie is geen onderdeel van de reset en blijft bestaan.
            Assert.True(await ctx.ProductCategories.AnyAsync(c => c.Id == seededCategoryId));

            // Geimporteerde gebieden/locaties met QR-token.
            var areas = await ctx.StorageAreas.ToListAsync();
            Assert.Single(areas);
            Assert.Equal("Salon", areas[0].Name);

            var locations = await ctx.StorageLocations.ToListAsync();
            Assert.Equal(2, locations.Count);
            Assert.All(locations, l => Assert.False(string.IsNullOrEmpty(l.QrToken)));
            Assert.Contains(locations, l => l.Name == "Rugleuning");
            Assert.Contains(locations, l => l.Name == "Snackla");

            // Producten zonder categorie en zonder code na import.
            var products = await ctx.Products.ToListAsync();
            Assert.Equal(3, products.Count);
            Assert.Empty(await ctx.ProductCodes.ToListAsync());
            Assert.Empty(await ctx.ProductCategoryMappings.ToListAsync());

            // Voorraadhoeveelheden (incl. decimale komma) komen overeen met het CSV.
            var koffie = products.Single(p => p.Name == "koffiebonen");
            var koffieStock = await ctx.Stocks.SingleAsync(s => s.ProductId == koffie.Id);
            Assert.Equal(1.5m, koffieStock.Quantity);

            var rivella = products.Single(p => p.Name == "Rivella");
            var rivellaStock = await ctx.Stocks.SingleAsync(s => s.ProductId == rivella.Id);
            Assert.Equal(4m, rivellaStock.Quantity);
        }

        // --- Assert: onbekende code kan aan geimporteerd product gekoppeld worden ---
        using (var scope = factory.Services.CreateScope())
        {
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
            var ctx = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();

            var imported = await ctx.Products.FirstAsync(p => p.Name == "kaasvlinders");

            var addResult = await productService.AddCodeAsync(imported.Id, "8710398520019", "EAN13");
            Assert.True(addResult.Success, addResult.ErrorMessage);

            var byCode = await productService.GetByCodeValueAsync("8710398520019");
            Assert.True(byCode.Success);
            Assert.Equal(imported.Id, byCode.Data!.Id);
        }
    }

    /// <summary>
    /// WebApplicationFactory met tijdelijke SQLite-database voor geïsoleerde integratietests.
    /// </summary>
    public sealed class TestFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"bm_import_test_{Guid.NewGuid():N}.db");

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
