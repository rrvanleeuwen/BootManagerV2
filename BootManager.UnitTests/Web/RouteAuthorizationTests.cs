using System.Reflection;
using BootManager.Web.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace BootManager.UnitTests.Web;

/// <summary>
/// Reflection-tests voor Owner/Crew-routeautorisatie op controllers en Razor-componenten.
/// Beschermt tegen regressie: per ongeluk verwijderde of verkeerde [Authorize]-attributen.
/// </summary>
public class RouteAuthorizationTests
{
    // Web-assembly via een unieke Web-type om ambiguïteit met Tools.Ingest te vermijden.
    private static readonly Assembly WebAssembly =
        typeof(BootManager.Web.Middleware.PcrGateMiddleware).Assembly;

    // --- Controller-level tests ---

    [Fact]
    public void LogbookAttachmentsController_RequiresOwnerOrCrewRole()
    {
        var attr = typeof(LogbookAttachmentsController)
            .GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(attr);
        Assert.Contains("Owner", attr.Roles ?? "");
        Assert.Contains("Crew", attr.Roles ?? "");
    }

    // --- Blazor-pagina Owner+Crew tests ---

    [Theory]
    [InlineData("BootManager.Web.Components.Pages.Dashboard")]
    [InlineData("BootManager.Web.Components.Pages.Logbook")]
    [InlineData("BootManager.Web.Components.Pages.LogbookPrint")]
    [InlineData("BootManager.Web.Components.Pages.LogbookEntryDetails")]
    public void Page_RequiresOwnerOrCrewRole(string typeName)
    {
        var pageType = WebAssembly.GetType(typeName);
        Assert.NotNull(pageType);

        var attr = pageType.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Contains("Owner", attr.Roles ?? "");
        Assert.Contains("Crew", attr.Roles ?? "");
    }

    // --- Blazor-pagina Owner-only tests ---

    [Theory]
    [InlineData("BootManager.Web.Components.Pages.Settings")]
    [InlineData("BootManager.Web.Components.Pages.Analysis")]
    [InlineData("BootManager.Web.Components.Pages.Onboarding")]
    public void Page_RequiresOwnerOnlyRole(string typeName)
    {
        var pageType = WebAssembly.GetType(typeName);
        Assert.NotNull(pageType);

        var attr = pageType.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Contains("Owner", attr.Roles ?? "");
        Assert.DoesNotContain("Crew", attr.Roles ?? "");
    }

    // --- OnboardingGate bestaat als type ---

    [Fact]
    public void OnboardingGate_TypeExists_InWebAssembly()
    {
        var gateType = WebAssembly.GetType("BootManager.Web.Components.OnboardingGate");
        Assert.NotNull(gateType);
    }

    // --- NavMenu: account-link verwijderd uit NavMenu (hoort in MainLayout-dropdown) ---

    [Fact]
    public void NavMenu_AccountLink_RemovedFromNavMenu()
    {
        var navMenuType = WebAssembly.GetType("BootManager.Web.Components.Layout.NavMenu");
        Assert.NotNull(navMenuType);

        // GetUserDisplayName bestond uitsluitend voor de account-link die naar de MainLayout-dropdown is verplaatst.
        var method = navMenuType.GetMethod(
            "GetUserDisplayName",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.Null(method);
    }

    [Fact]
    public void StartupGate_DoesNotDependOnLegacyOwnerRegistrationService()
    {
        var gateType = WebAssembly.GetType("BootManager.Web.Components.StartupGate");
        Assert.NotNull(gateType);

        var legacyServiceType = typeof(
            BootManager.Application.OwnerRegistration.Services.IOwnerRegistrationService);

        Assert.DoesNotContain(
            gateType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
            property => property.PropertyType == legacyServiceType);
    }

    // --- Migratiepreservatie: LocalUser heeft alle vereiste velden ---

    [Fact]
    public void LocalUser_HasAllRequiredMigrationFields()
    {
        var type = typeof(BootManager.Core.Entities.LocalUser);

        var expectedProperties = new[]
        {
            "Id", "DisplayName", "NormalizedName", "Role", "IsActive",
            "PasswordHash", "PasswordSalt", "HashAlgorithm",
            "CredentialVersion", "PasswordChangeRequired", "OnboardingCompleted",
            "EncryptedProfilePayload", "EncryptionVersion",
            "PinHash", "PinSalt", "RecoveryCodeHash", "RecoveryCodeSalt",
            "CreatedUtc", "UpdatedUtc"
        };

        foreach (var prop in expectedProperties)
        {
            Assert.NotNull(
                type.GetProperty(prop, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
        }
    }
}
