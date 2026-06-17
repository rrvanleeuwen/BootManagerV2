using System.Net;
using System.Net.Http.Json;
using BootManager.Application.Authentication.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Core.Interfaces;
using BootManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BootManager.IntegrationTests.Authentication;

/// <summary>
/// Integratietests voor het /auth/change-password endpoint, inclusief cookievernieuwing.
/// Elke test gebruikt een eigen factory met een eigen tijdelijke database voor isolatie.
/// </summary>
public class ChangePasswordEndpointTests
{
    [Fact]
    public async Task ChangePassword_ReturnsOk_AndRenewsCookie()
    {
        await using var factory = new TestFactory();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var ownerId = await GetOwnerIdAsync(factory);
        await LoginAsync(client, ownerId, TestFactory.BootstrapPassword);

        var response = await client.PostAsJsonAsync("/auth/change-password", new ChangePasswordDto
        {
            CurrentPassword = TestFactory.BootstrapPassword,
            NewPassword = "UpdatedPass99!",
            ConfirmNewPassword = "UpdatedPass99!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(
            response.Headers.Contains("Set-Cookie"),
            "Verwacht Set-Cookie header: cookie moet worden vernieuwd na wachtwoordwijziging.");
    }

    [Fact]
    public async Task ChangePassword_Rejects_SamePassword_ViaAccountService()
    {
        await using var factory = new TestFactory();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var ownerId = await GetOwnerIdAsync(factory);
        await LoginAsync(client, ownerId, TestFactory.BootstrapPassword);

        var response = await client.PostAsJsonAsync("/auth/change-password", new ChangePasswordDto
        {
            CurrentPassword = TestFactory.BootstrapPassword,
            NewPassword = TestFactory.BootstrapPassword,
            ConfirmNewPassword = TestFactory.BootstrapPassword
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_Requires_Authentication()
    {
        await using var factory = new TestFactory();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });

        var response = await client.PostAsJsonAsync("/auth/change-password", new ChangePasswordDto
        {
            CurrentPassword = "any",
            NewPassword = "NewPass123!",
            ConfirmNewPassword = "NewPass123!"
        });

        // Niet-geauthenticeerd → 401 of redirect naar login
        Assert.True(
            response.StatusCode == HttpStatusCode.Unauthorized ||
            response.StatusCode == HttpStatusCode.Redirect,
            $"Verwacht 401 of redirect, maar kreeg {response.StatusCode}.");
    }

    /// <summary>
    /// Reproduceert de handmatige Crew-flow: Crew met PasswordChangeRequired=true logt in,
    /// wijzigt wachtwoord, krijgt vernieuwde cookie en is daarna niet meer geblokkeerd door de PCR-gate.
    /// </summary>
    [Fact]
    public async Task ChangePassword_Crew_WithPcrRequired_Succeeds_AndUnlocksNavigation()
    {
        await using var factory = new TestFactory();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        const string tempPassword = "TempCrew99!";
        const string newPassword = "NewCrewPass1!";
        Guid crewId;

        using (var scope = factory.Services.CreateScope())
        {
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<LocalUser>>();
            var hash = hasher.Hash(tempPassword);
            var crew = LocalUser.Create(
                displayName: "Carla",
                role: LocalUserRole.Crew,
                passwordHash: hash.Hash,
                passwordSalt: hash.Salt,
                hashAlgorithm: hash.Algorithm,
                encryptedProfilePayload: Array.Empty<byte>(),
                encryptionVersion: 1,
                createdUtc: DateTime.UtcNow,
                passwordChangeRequired: true);
            await repo.AddAsync(crew);
            crewId = crew.Id;
        }

        // Login als Crew met tijdelijk wachtwoord
        await LoginAsync(client, crewId, tempPassword);

        // POST naar /auth/change-password via hetzelfde JSON/cookie-pad als de browser (authClient.js)
        var changeResponse = await client.PostAsJsonAsync("/auth/change-password", new ChangePasswordDto
        {
            CurrentPassword = tempPassword,
            NewPassword = newPassword,
            ConfirmNewPassword = newPassword
        });

        // Bewijs HTTP 200 zonder antiforgeryfout
        Assert.Equal(HttpStatusCode.OK, changeResponse.StatusCode);

        // Bewijs vernieuwde cookie
        Assert.True(
            changeResponse.Headers.Contains("Set-Cookie"),
            "Verwacht Set-Cookie: cookie vernieuwen na wachtwoordwijziging Crew.");

        // Bewijs dat tijdelijk wachtwoord niet meer werkt
        var freshClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        var oldPwdResp = await freshClient.PostAsJsonAsync("/auth/login", new LoginRequestDto
        {
            UserId = crewId,
            Password = tempPassword
        });
        Assert.Equal(HttpStatusCode.BadRequest, oldPwdResp.StatusCode);

        // Bewijs dat nieuw wachtwoord werkt
        var newPwdResp = await freshClient.PostAsJsonAsync("/auth/login", new LoginRequestDto
        {
            UserId = crewId,
            Password = newPassword
        });
        Assert.Equal(HttpStatusCode.OK, newPwdResp.StatusCode);

        // Bewijs dat de nieuwe sessie niet meer door de PCR-gate wordt geblokkeerd
        // (client heeft vernieuwde cookie met bm.password_change_required=false)
        var logbookResp = await client.GetAsync("/logbook");
        Assert.NotEqual(HttpStatusCode.Found, logbookResp.StatusCode);
    }

    /// <summary>
    /// Bewijs dat logout de authenticatiecookie wist en de gebruiker daarna geen toegang meer heeft.
    /// </summary>
    [Fact]
    public async Task Logout_ClearsCookie_AndProtectedRouteBecomesInaccessible()
    {
        await using var factory = new TestFactory();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        // Login
        var ownerId = await GetOwnerIdAsync(factory);
        await LoginAsync(client, ownerId, TestFactory.BootstrapPassword);

        // Logout
        var logoutResp = await client.PostAsJsonAsync("/auth/logout", new { });
        Assert.Equal(HttpStatusCode.OK, logoutResp.StatusCode);

        // Na logout is een beveiligde route niet meer bereikbaar (401 of redirect naar /login)
        var afterLogout = await client.GetAsync("/dashboard");
        Assert.True(
            afterLogout.StatusCode == HttpStatusCode.Unauthorized ||
            afterLogout.StatusCode == HttpStatusCode.Found,
            $"Verwacht 401 of redirect na logout, maar kreeg {afterLogout.StatusCode}.");
    }

    private static async Task<Guid> GetOwnerIdAsync(TestFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<LocalUser>>();
        var owner = await repo.SingleOrDefaultAsync(u => u.Role == LocalUserRole.Owner);
        Assert.NotNull(owner);
        return owner.Id;
    }

    private static async Task LoginAsync(HttpClient client, Guid userId, string password)
    {
        var resp = await client.PostAsJsonAsync("/auth/login", new LoginRequestDto
        {
            UserId = userId,
            Password = password
        });
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Login mislukt ({resp.StatusCode}): {body} [UserId={userId}]");
        }
    }

    /// <summary>
    /// WebApplicationFactory met tijdelijke SQLite-database voor integratietests.
    /// Overschrijft de DbContextFactory via ConfigureTestServices zodat de isolatie gegarandeerd is.
    /// </summary>
    public sealed class TestFactory : WebApplicationFactory<Program>
    {
        public const string BootstrapPassword = "IntegrationTest99!";
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"bm_int_{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Bootstrap:DefaultPassword"] = BootstrapPassword,
                    ["Jwt:Key"] = "integration_test_jwt_key_32chars!!!!",
                    ["Encryption:Key"] = "IntegrationTestEncryptionKey1234"
                });
            });

            // Overschrijf de EF Core DbContextFactory met de testdatabase.
            // ConfigureTestServices loopt ná de app-services, zodat deze registratie wint.
            builder.ConfigureTestServices(services =>
            {
                // Verwijder bestaande DbContext-registraties
                var toRemove = services
                    .Where(d => d.ServiceType == typeof(IDbContextFactory<BootManagerDbContext>) ||
                                d.ServiceType == typeof(BootManagerDbContext))
                    .ToList();
                foreach (var d in toRemove) services.Remove(d);

                // Voeg toe met geïsoleerde testdatabase
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
