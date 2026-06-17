using System.Net;
using BootManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BootManager.IntegrationTests.StaticFiles;

/// <summary>
/// Integratietest die bewijst dat GET /js/authClient.js JavaScript retourneert, nooit HTML.
/// Reproduceert de browserdiagnose voor de "Unexpected token '&lt;'" syntaxfout.
/// </summary>
public class AuthClientJsTests
{
    /// <summary>
    /// Verifieert HTTP 200, JavaScript content-type en JavaScript-body (niet HTML).
    /// Bewijs dat de canonieke assetroute /js/authClient.js bereikbaar is en JavaScript levert.
    /// </summary>
    [Fact]
    public async Task AuthClientJs_Returns_JavaScript_NotHtml()
    {
        await using var factory = new JsTestFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/js/authClient.js");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var contentType = response.Content.Headers.ContentType?.MediaType;
        Assert.True(
            contentType == "text/javascript" || contentType == "application/javascript",
            $"Verwacht JavaScript content-type, maar kreeg: {contentType}");

        var body = await response.Content.ReadAsStringAsync();
        Assert.False(
            body.TrimStart().StartsWith("<", StringComparison.Ordinal),
            "Response mag niet beginnen met '<' (HTML); verwacht JavaScript-inhoud.");
        Assert.Contains("export", body, StringComparison.Ordinal);
    }

    private sealed class JsTestFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"bm_js_{Guid.NewGuid():N}.db");

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
