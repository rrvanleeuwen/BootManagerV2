using System.Data.Common;
using BootManager.Application.Inventory.Contracts;
using BootManager.Core.Entities;
using BootManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BootManager.IntegrationTests.Inventory;

/// <summary>
/// Integratietests voor de geregistreerde <see cref="IProductOverviewReadQuery"/> op echte
/// SQLite. Bewijzen dat de production reader database-gestuurd filtert op archiefstand en op
/// hoofdletterongevoelige deelmatches in naam én omschrijving, stabiel in pagina's van tien
/// sorteert, product/eenheid/optionele code/actieve categorie/actieve locaties/totaal correct
/// projecteert, nulvoorraad uitsluit, en voor dezelfde pagina hoogstens vijf databasecommando's
/// uitvoert zonder groei wanneer extra niet-zichtbare producten worden toegevoegd. Een
/// <see cref="DbCommandInterceptor"/> op de test-DbContextFactory telt de uitgevoerde commando's.
/// Gebruikt tijdelijke SQLite-databases; raakt geen productie- of Raspberry Pi-database.
/// </summary>
public class ProductOverviewReadQueryIntegrationTests
{
    [Fact]
    public async Task GetPage_FiltersArchiveAndSearch_ProjectsData_AndExcludesZeroStock()
    {
        await using var factory = new TestFactory();

        using (var scope = factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
            await ctx.Database.MigrateAsync();

            var unit = Unit.Create("stuk");
            ctx.Units.Add(unit);
            var category = ProductCategory.Create("Dranken", "Alle dranken", "drank");
            ctx.ProductCategories.Add(category);

            var area = StorageArea.Create("Kombuis");
            ctx.StorageAreas.Add(area);
            var kast = StorageLocation.Create(area.Id, "Kast");
            var plank = StorageLocation.Create(area.Id, "Plank");
            ctx.StorageLocations.Add(kast);
            ctx.StorageLocations.Add(plank);

            // Match op naam ("sap"), met code, actieve categorie en één actieve locatie
            // (Kast qty 5) plus een nulvoorraadregel (Plank qty 0) die uitgesloten moet worden.
            var appelsap = Product.Create("Appelsap", "Fris en zoet", unit.Id);
            ctx.Products.Add(appelsap);
            ctx.ProductCodes.Add(ProductCode.Create(appelsap.Id, "1111", "barcode"));
            ctx.ProductCategoryMappings.Add(ProductCategoryMapping.Create(appelsap.Id, category.Id));
            ctx.Stocks.Add(Stock.Create(appelsap.Id, kast.Id, 5m));
            ctx.Stocks.Add(Stock.Create(appelsap.Id, plank.Id, 0m));

            // Match op omschrijving ("SAPPEN", hoofdletters), zonder code, zonder categorie en
            // zonder actieve voorraad (totaal 0).
            var bronwater = Product.Create("Bronwater", "Verse SAPPEN van de bron", unit.Id);
            ctx.Products.Add(bronwater);

            // Match op naam ("sap") maar met een gedeactiveerde categoriekoppeling: categorie
            // mag niet getoond worden.
            var sapOud = Product.Create("Sap-oud", "Restpartij", unit.Id);
            ctx.Products.Add(sapOud);
            var inactiveMapping = ProductCategoryMapping.Create(sapOud.Id, category.Id);
            inactiveMapping.Deactivate();
            ctx.ProductCategoryMappings.Add(inactiveMapping);

            // Geen match: bevat "sap" niet in naam of omschrijving.
            var cola = Product.Create("Cola", "Priklimonade", unit.Id);
            ctx.Products.Add(cola);

            // Match op naam ("sap") maar gearchiveerd: uitgesloten bij actieve stand.
            var sapArchief = Product.Create("Sap-archief", "Oud product", unit.Id);
            sapArchief.Archive();
            ctx.Products.Add(sapArchief);

            await ctx.SaveChangesAsync();
        }

        using (var scope = factory.Services.CreateScope())
        {
            var reader = scope.ServiceProvider.GetRequiredService<IProductOverviewReadQuery>();

            // Actieve stand, hoofdletterongevoelige zoekterm.
            var page = await reader.GetPageAsync("SaP", showArchived: false, pageNumber: 1, pageSize: 10);

            // Drie actieve matches (naam + omschrijving), gearchiveerde match uitgesloten.
            Assert.Equal(3, page.TotalCount);
            Assert.Equal(3, page.Items.Count);

            // Stabiel gesorteerd op naam: Appelsap, Bronwater, Sap-oud.
            Assert.Equal(new[] { "Appelsap", "Bronwater", "Sap-oud" }, page.Items.Select(i => i.Product.Name).ToArray());

            var appelsap = page.Items[0];
            Assert.Equal("stuk", appelsap.Product.DefaultUnitName);
            Assert.NotNull(appelsap.Product.Code);
            Assert.Equal("1111", appelsap.Product.Code!.Value);
            Assert.Equal("barcode", appelsap.Product.Code!.Format);
            Assert.Equal("Dranken", appelsap.Product.ActiveCategoryName);
            Assert.Equal("drank", appelsap.Product.ActiveCategoryIconKey);
            // Nulvoorraad (Plank) uitgesloten; alleen Kast qty 5 actief.
            Assert.Single(appelsap.ActiveLocations);
            Assert.Equal("Kombuis", appelsap.ActiveLocations[0].StorageAreaName);
            Assert.Equal("Kast", appelsap.ActiveLocations[0].StorageLocationName);
            Assert.Equal(5m, appelsap.ActiveLocations[0].Quantity);
            Assert.Equal(5m, appelsap.TotalQuantity);

            var bronwater = page.Items[1];
            Assert.Null(bronwater.Product.Code);
            Assert.Null(bronwater.Product.ActiveCategoryId);
            Assert.Null(bronwater.Product.ActiveCategoryName);
            Assert.Empty(bronwater.ActiveLocations);
            Assert.Equal(0m, bronwater.TotalQuantity);

            var sapOud = page.Items[2];
            // Gedeactiveerde categoriekoppeling: geen actieve categorie getoond.
            Assert.Null(sapOud.Product.ActiveCategoryId);
            Assert.Null(sapOud.Product.ActiveCategoryName);

            // Gearchiveerde stand met dezelfde zoekterm levert uitsluitend het archiefproduct.
            var archivedPage = await reader.GetPageAsync("SaP", showArchived: true, pageNumber: 1, pageSize: 10);
            Assert.Equal(1, archivedPage.TotalCount);
            Assert.Single(archivedPage.Items);
            Assert.Equal("Sap-archief", archivedPage.Items[0].Product.Name);
            Assert.True(archivedPage.Items[0].Product.IsArchived);
        }
    }

    [Fact]
    public async Task GetPage_OrdersByNameThenId_AndPagesInTensStably()
    {
        await using var factory = new TestFactory();

        using (var scope = factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
            await ctx.Database.MigrateAsync();

            var unit = Unit.Create("stuk");
            ctx.Units.Add(unit);

            // Twaalf producten, bewust in willekeurige volgorde toegevoegd.
            foreach (var i in new[] { 7, 2, 11, 4, 9, 1, 12, 5, 8, 3, 10, 6 })
            {
                ctx.Products.Add(Product.Create($"Prod {i:00}", null, unit.Id));
            }

            await ctx.SaveChangesAsync();
        }

        using (var scope = factory.Services.CreateScope())
        {
            var reader = scope.ServiceProvider.GetRequiredService<IProductOverviewReadQuery>();

            var page1 = await reader.GetPageAsync(null, showArchived: false, pageNumber: 1, pageSize: 10);
            Assert.Equal(12, page1.TotalCount);
            Assert.Equal(10, page1.Items.Count);
            Assert.Equal(
                Enumerable.Range(1, 10).Select(i => $"Prod {i:00}").ToArray(),
                page1.Items.Select(i => i.Product.Name).ToArray());

            var page2 = await reader.GetPageAsync(null, showArchived: false, pageNumber: 2, pageSize: 10);
            Assert.Equal(12, page2.TotalCount);
            Assert.Equal(2, page2.Items.Count);
            Assert.Equal(
                new[] { "Prod 11", "Prod 12" },
                page2.Items.Select(i => i.Product.Name).ToArray());
        }
    }

    [Fact]
    public async Task GetPage_UsesFixedQueryBudget_AndDoesNotGrowWithMoreProducts()
    {
        await using var factory = new TestFactory();

        Guid unitId;
        Guid locationId;

        using (var scope = factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
            await ctx.Database.MigrateAsync();

            var unit = Unit.Create("stuk");
            ctx.Units.Add(unit);
            var area = StorageArea.Create("Kombuis");
            ctx.StorageAreas.Add(area);
            var location = StorageLocation.Create(area.Id, "Kast");
            ctx.StorageLocations.Add(location);

            // Twaalf producten met actieve voorraad: pagina 1 toont er tien.
            for (var i = 1; i <= 12; i++)
            {
                var product = Product.Create($"Prod {i:000}", $"Omschrijving {i}", unit.Id);
                ctx.Products.Add(product);
                ctx.Stocks.Add(Stock.Create(product.Id, location.Id, i));
            }

            await ctx.SaveChangesAsync();
            unitId = unit.Id;
            locationId = location.Id;
        }

        int firstCommandCount;
        using (var scope = factory.Services.CreateScope())
        {
            var reader = scope.ServiceProvider.GetRequiredService<IProductOverviewReadQuery>();

            factory.CommandCounter.Reset();
            var page = await reader.GetPageAsync(null, showArchived: false, pageNumber: 1, pageSize: 10);
            firstCommandCount = factory.CommandCounter.Count;

            Assert.Equal(12, page.TotalCount);
            Assert.Equal(10, page.Items.Count);
            // Elk zichtbaar product heeft zijn actieve voorraad reeds gebatcht geladen; een
            // per-product voorraadquery zou het commandoaantal ver boven vijf duwen.
            Assert.All(page.Items, item => Assert.Single(item.ActiveLocations));
            Assert.True(firstCommandCount <= 5, $"Eerste pagina gebruikte {firstCommandCount} databasecommando's (verwacht <= 5).");
        }

        // Voeg veel extra, niet-zichtbare producten (met voorraad) toe.
        using (var scope = factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
            for (var i = 13; i <= 60; i++)
            {
                var product = Product.Create($"Prod {i:000}", $"Omschrijving {i}", unitId);
                ctx.Products.Add(product);
                ctx.Stocks.Add(Stock.Create(product.Id, locationId, i));
            }
            await ctx.SaveChangesAsync();
        }

        using (var scope = factory.Services.CreateScope())
        {
            var reader = scope.ServiceProvider.GetRequiredService<IProductOverviewReadQuery>();

            factory.CommandCounter.Reset();
            var page = await reader.GetPageAsync(null, showArchived: false, pageNumber: 1, pageSize: 10);
            var secondCommandCount = factory.CommandCounter.Count;

            Assert.Equal(60, page.TotalCount);
            Assert.Equal(10, page.Items.Count);
            Assert.True(secondCommandCount <= 5, $"Pagina met meer producten gebruikte {secondCommandCount} databasecommando's (verwacht <= 5).");
            // Het commandoaantal groeit niet met het totale aantal producten.
            Assert.Equal(firstCommandCount, secondCommandCount);
        }
    }

    /// <summary>
    /// Telt de daadwerkelijk uitgevoerde SQL-commando's. De SQLite-huishoudelijke
    /// <c>PRAGMA</c>-commando's bij het openen van een verbinding tellen niet mee, zodat het
    /// aantal exact de leeslogica (tellen, pagina-projectie, gebatchte voorraad) weerspiegelt.
    /// </summary>
    public sealed class CommandCountingInterceptor : DbCommandInterceptor
    {
        private int _count;

        public int Count => Volatile.Read(ref _count);

        public void Reset() => Interlocked.Exchange(ref _count, 0);

        private void CountCommand(DbCommand command)
        {
            var text = command.CommandText?.TrimStart() ?? string.Empty;
            if (!text.StartsWith("PRAGMA", StringComparison.OrdinalIgnoreCase))
            {
                Interlocked.Increment(ref _count);
            }
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            CountCommand(command);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            CountCommand(command);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<object> ScalarExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
        {
            CountCommand(command);
            return base.ScalarExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
            CancellationToken cancellationToken = default)
        {
            CountCommand(command);
            return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            CountCommand(command);
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            CountCommand(command);
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }
    }

    /// <summary>
    /// WebApplicationFactory met tijdelijke SQLite-database en een command-tellende interceptor
    /// op de DbContextFactory voor geïsoleerde integratietests.
    /// </summary>
    public sealed class TestFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"bm_overview_test_{Guid.NewGuid():N}.db");

        public CommandCountingInterceptor CommandCounter { get; } = new();

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
                    o => o.UseSqlite($"Data Source={_dbPath}").AddInterceptors(CommandCounter));
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { }
        }
    }
}
