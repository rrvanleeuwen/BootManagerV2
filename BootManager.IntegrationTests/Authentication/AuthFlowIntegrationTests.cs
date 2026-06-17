using System.Net;
using System.Net.Http.Json;
using BootManager.Application.Authentication.DTOs;
using BootManager.Application.Authentication.Services;
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
/// Regressie-integratietests voor de volledige PILOT-AUTH-01-authflow.
/// Gebruikt tijdelijke SQLite-databases; raakt geen productie- of Raspberry Pi-database.
/// </summary>
public class AuthFlowIntegrationTests
{
    // --- PCR-gate: bootstrap Owner mag /onboarding bereiken ---

    /// <summary>
    /// Bewijs dat de bootstrap Owner (Role=Owner, PasswordChangeRequired=true) NIET naar /account
    /// wordt omgeleid. De Crew-PCR-gate geldt uitsluitend voor Crew.
    /// </summary>
    [Fact]
    public async Task BootstrapOwner_WithPcrTrue_CanAccess_Onboarding_NotRedirectedToAccount()
    {
        await using var factory = new TestFactory();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var ownerId = await GetOwnerIdAsync(factory);
        await LoginAsync(client, ownerId, TestFactory.BootstrapPassword);

        var response = await client.GetAsync("/onboarding");

        // De PcrGateMiddleware mag de Owner NIET doorsturen naar /account.
        Assert.False(
            response.StatusCode == HttpStatusCode.Found &&
            string.Equals(
                response.Headers.Location?.ToString(), "/account",
                StringComparison.OrdinalIgnoreCase),
            $"Bootstrap Owner (PCR=true) werd incorrectly omgeleid naar /account. " +
            $"Status: {response.StatusCode}, Location: {response.Headers.Location}");
    }

    // --- PCR-gate: Crew met PCR=true blijft geblokkeerd ---

    /// <summary>
    /// Bewijs dat de Crew-PCR-gate intact blijft: Crew met PasswordChangeRequired=true
    /// wordt omgeleid van /logbook naar /account.
    /// </summary>
    [Fact]
    public async Task Crew_WithPcrTrue_IsRedirected_FromLogbook_ToAccount()
    {
        await using var factory = new TestFactory();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var crewId = await CreateCrewAsync(factory, "CrewPcr99!", passwordChangeRequired: true);
        await LoginAsync(client, crewId, "CrewPcr99!");

        var response = await client.GetAsync("/logbook");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Equal("/account", response.Headers.Location?.ToString());
    }

    // --- Sessie-invalidatie: wachtwoordreset maakt twee sessies ongeldig ---

    /// <summary>
    /// Bewijs dat twee actieve Crew-sessies beide ongeldig worden na een wachtwoordreset.
    /// CredentialVersion incrementeert → OnValidatePrincipal verwerpt de oude claims.
    /// </summary>
    [Fact]
    public async Task TwoCrewSessions_BecomeInvalid_AfterPasswordReset()
    {
        await using var factory = new TestFactory();

        const string initialPassword = "InitCrew99!";
        var crewId = await CreateCrewAsync(factory, initialPassword, passwordChangeRequired: false);

        var client1 = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        var client2 = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        await LoginAsync(client1, crewId, initialPassword);
        await LoginAsync(client2, crewId, initialPassword);

        // Beide sessies zijn geldig vóór de reset
        var before1 = await client1.GetAsync("/account");
        var before2 = await client2.GetAsync("/account");
        Assert.NotEqual(HttpStatusCode.Unauthorized, before1.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, before2.StatusCode);

        // Wachtwoordreset via service: incrementeert CredentialVersion
        using (var scope = factory.Services.CreateScope())
        {
            var mgmt = scope.ServiceProvider.GetRequiredService<ILocalUserManagementService>();
            await mgmt.ResetCrewPasswordAsync(crewId, "ResetNew99!");
        }

        // Beide sessies zijn nu ongeldig (CredentialVersion in cookie klopt niet meer)
        var after1 = await client1.GetAsync("/logbook");
        var after2 = await client2.GetAsync("/logbook");
        Assert.True(
            after1.StatusCode == HttpStatusCode.Unauthorized ||
            after1.StatusCode == HttpStatusCode.Found,
            $"Sessie 1 verwacht ongeldig na reset, maar kreeg {after1.StatusCode}");
        Assert.True(
            after2.StatusCode == HttpStatusCode.Unauthorized ||
            after2.StatusCode == HttpStatusCode.Found,
            $"Sessie 2 verwacht ongeldig na reset, maar kreeg {after2.StatusCode}");
    }

    // --- Sessie-invalidatie: uitschakelen maakt twee sessies ongeldig ---

    /// <summary>
    /// Bewijs dat twee actieve Crew-sessies beide ongeldig worden na uitschakelen.
    /// IsActive=false → OnValidatePrincipal verwerpt de claims.
    /// </summary>
    [Fact]
    public async Task TwoCrewSessions_BecomeInvalid_AfterDisabling()
    {
        await using var factory = new TestFactory();

        const string crewPassword = "CrewDis99!";
        var crewId = await CreateCrewAsync(factory, crewPassword, passwordChangeRequired: false);

        var client1 = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        var client2 = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        await LoginAsync(client1, crewId, crewPassword);
        await LoginAsync(client2, crewId, crewPassword);

        // Uitschakelen via service: zet IsActive=false
        using (var scope = factory.Services.CreateScope())
        {
            var mgmt = scope.ServiceProvider.GetRequiredService<ILocalUserManagementService>();
            await mgmt.DisableCrewAsync(crewId);
        }

        // Beide sessies zijn nu ongeldig (IsActive=false in DB)
        var after1 = await client1.GetAsync("/logbook");
        var after2 = await client2.GetAsync("/logbook");
        Assert.True(
            after1.StatusCode == HttpStatusCode.Unauthorized ||
            after1.StatusCode == HttpStatusCode.Found,
            $"Sessie 1 verwacht ongeldig na uitschakelen, maar kreeg {after1.StatusCode}");
        Assert.True(
            after2.StatusCode == HttpStatusCode.Unauthorized ||
            after2.StatusCode == HttpStatusCode.Found,
            $"Sessie 2 verwacht ongeldig na uitschakelen, maar kreeg {after2.StatusCode}");
    }

    // --- Uitgeschakelde Crew kan niet inloggen ---

    /// <summary>
    /// Bewijs dat een uitgeschakeld Crew-account geen nieuwe inlogpoging kan doen.
    /// </summary>
    [Fact]
    public async Task DisabledCrew_CannotLogin()
    {
        await using var factory = new TestFactory();

        const string crewPassword = "DisLogin99!";
        var crewId = await CreateCrewAsync(factory, crewPassword, passwordChangeRequired: false);

        using (var scope = factory.Services.CreateScope())
        {
            var mgmt = scope.ServiceProvider.GetRequiredService<ILocalUserManagementService>();
            await mgmt.DisableCrewAsync(crewId);
        }

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });

        var loginResp = await client.PostAsJsonAsync("/auth/login", new LoginRequestDto
        {
            UserId = crewId,
            Password = crewPassword
        });

        Assert.Equal(HttpStatusCode.BadRequest, loginResp.StatusCode);
    }

    // --- Reactivering behoudt wachtwoord en PCR-status ---

    /// <summary>
    /// Bewijs dat na reactivering login met het originele wachtwoord slaagt
    /// en de PCR-status ongewijzigd blijft.
    /// </summary>
    [Fact]
    public async Task ReactivatedCrew_CanLogin_WithSamePassword_AndPreservesPcrStatus()
    {
        await using var factory = new TestFactory();

        const string crewPassword = "ReactivePcr99!";
        var crewId = await CreateCrewAsync(factory, crewPassword, passwordChangeRequired: false);

        using (var scope = factory.Services.CreateScope())
        {
            var mgmt = scope.ServiceProvider.GetRequiredService<ILocalUserManagementService>();
            await mgmt.DisableCrewAsync(crewId);
            await mgmt.ReactivateCrewAsync(crewId);
        }

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

        var loginResp = await client.PostAsJsonAsync("/auth/login", new LoginRequestDto
        {
            UserId = crewId,
            Password = crewPassword
        });

        // Login met hetzelfde wachtwoord moet slagen na reactivering
        Assert.Equal(HttpStatusCode.OK, loginResp.StatusCode);

        // Controleer via DB dat PCR-status bewaard is (false: Crew had al een eigen wachtwoord)
        using (var scope = factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IRepository<LocalUser>>();
            var crew = await repo.GetByIdAsync(crewId);
            Assert.NotNull(crew);
            Assert.True(crew.IsActive, "Crew moet actief zijn na reactivering.");
            Assert.False(crew.PasswordChangeRequired,
                "PasswordChangeRequired mag niet zijn gewijzigd door reactivering.");
        }
    }

    // --- Hulpmethoden ---

    private static async Task<Guid> GetOwnerIdAsync(TestFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<LocalUser>>();
        var owner = await repo.SingleOrDefaultAsync(u => u.Role == LocalUserRole.Owner);
        Assert.NotNull(owner);
        return owner.Id;
    }

    private static async Task<Guid> CreateCrewAsync(TestFactory factory, string password, bool passwordChangeRequired)
    {
        using var scope = factory.Services.CreateScope();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<LocalUser>>();
        var hash = hasher.Hash(password);
        var crew = LocalUser.Create(
            displayName: $"TestCrew_{Guid.NewGuid():N}".Substring(0, 24),
            role: LocalUserRole.Crew,
            passwordHash: hash.Hash,
            passwordSalt: hash.Salt,
            hashAlgorithm: hash.Algorithm,
            encryptedProfilePayload: Array.Empty<byte>(),
            encryptionVersion: 1,
            createdUtc: DateTime.UtcNow,
            passwordChangeRequired: passwordChangeRequired);
        await repo.AddAsync(crew);
        return crew.Id;
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
            throw new Xunit.Sdk.XunitException(
                $"Login mislukt ({resp.StatusCode}): {body} [UserId={userId}]");
        }
    }

    /// <summary>
    /// WebApplicationFactory met tijdelijke SQLite-database voor geïsoleerde integratietests.
    /// </summary>
    public sealed class TestFactory : WebApplicationFactory<Program>
    {
        public const string BootstrapPassword = "IntegrationTest99!";
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"bm_flow_{Guid.NewGuid():N}.db");

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
