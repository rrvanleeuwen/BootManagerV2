using System.Reflection;

namespace BootManager.UnitTests.Web;

/// <summary>
/// Markup-regressietests voor MainLayout.razor.
/// Bewijst dat de gebruikersdropdown volledig Blazor-gestuurd is en de vereiste items correct aanwezig zijn.
/// Leest de .razor-bronbestanden zodat ook de gegenereerde render-tree-logica wordt gedekt.
/// </summary>
public class MainLayoutMarkupTests
{
    private static readonly string? SolutionRoot = FindSolutionRoot();

    private static string? FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "BootManager.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }

    private static string ReadSource(string relPath)
    {
        if (SolutionRoot is null) return string.Empty;
        var full = Path.Combine(SolutionRoot, relPath);
        return File.Exists(full) ? File.ReadAllText(full) : string.Empty;
    }

    // --- MainLayout dropdown: geen Bootstrap JS, wel Blazor ---

    [Fact]
    public void MainLayout_UserDropdown_DoesNotUse_DataToggleDropdown()
    {
        var content = ReadSource(@"BootManager.Web\Components\Layout\MainLayout.razor");
        Assert.False(string.IsNullOrEmpty(content), "MainLayout.razor niet gevonden.");
        Assert.DoesNotContain("data-toggle=\"dropdown\"", content);
    }

    [Fact]
    public void MainLayout_ProfileTrigger_HasBlazorClickHandler_And_AriaExpanded()
    {
        var content = ReadSource(@"BootManager.Web\Components\Layout\MainLayout.razor");
        Assert.False(string.IsNullOrEmpty(content), "MainLayout.razor niet gevonden.");
        Assert.Contains("@onclick=\"ToggleMenu\"", content);
        Assert.Contains("aria-expanded", content);
    }

    // --- Dropdown-inhoud: Mijn account, Instellingen, Uitloggen ---

    [Fact]
    public void MainLayout_Dropdown_HasMijnAccount()
    {
        var content = ReadSource(@"BootManager.Web\Components\Layout\MainLayout.razor");
        Assert.False(string.IsNullOrEmpty(content), "MainLayout.razor niet gevonden.");
        Assert.Contains("Mijn account", content);
    }

    [Fact]
    public void MainLayout_Dropdown_HasExactlyOneLogoutAction()
    {
        var content = ReadSource(@"BootManager.Web\Components\Layout\MainLayout.razor");
        Assert.False(string.IsNullOrEmpty(content), "MainLayout.razor niet gevonden.");
        var count = content.Split("@onclick=\"Logout\"").Length - 1;
        Assert.Equal(1, count);
    }

    [Fact]
    public void MainLayout_Instellingen_IsUnderOwnerAuthorizeView()
    {
        var content = ReadSource(@"BootManager.Web\Components\Layout\MainLayout.razor");
        Assert.False(string.IsNullOrEmpty(content), "MainLayout.razor niet gevonden.");
        // Roles="Owner" moet voor "Instellingen" staan in het bestand
        var ownerIdx = content.IndexOf("Roles=\"Owner\"", StringComparison.Ordinal);
        var settingsIdx = content.IndexOf("Instellingen", StringComparison.Ordinal);
        Assert.True(ownerIdx >= 0, "AuthorizeView Roles=\"Owner\" niet gevonden in MainLayout.");
        Assert.True(settingsIdx > ownerIdx, "Instellingen moet ná de Owner-autorisatiecheck staan.");
    }

    // --- NavMenu: oud account-item en losse logout blijven afwezig ---

    [Fact]
    public void NavMenu_NoStandaloneAccountLink_And_NoGetUserDisplayName()
    {
        var content = ReadSource(@"BootManager.Web\Components\Layout\NavMenu.razor");
        Assert.False(string.IsNullOrEmpty(content), "NavMenu.razor niet gevonden.");
        Assert.DoesNotContain("GetUserDisplayName", content);
        // Geen losse Logout-knop in NavMenu
        Assert.DoesNotContain("@onclick=\"Logout\"", content);
    }

    // --- Module-importfout: zichtbare melding en onmiddellijke re-render ---

    [Fact]
    public void MainLayout_ImportFailure_SetsError_AndCallsStateHasChanged()
    {
        var content = ReadSource(@"BootManager.Web\Components\Layout\MainLayout.razor");
        Assert.False(string.IsNullOrEmpty(content), "MainLayout.razor niet gevonden.");

        // Bij importfout moet _logoutError worden ingesteld én onmiddellijk worden gerenderd.
        Assert.Contains("_logoutError = \"Navigatiemodule", content);
        Assert.Contains("StateHasChanged()", content);

        // StateHasChanged() moet ná de foutmelding staan (bewijst volgorde in catch-block)
        var errorIdx = content.IndexOf("_logoutError = \"Navigatiemodule", StringComparison.Ordinal);
        var stateIdx = content.IndexOf("StateHasChanged()", StringComparison.Ordinal);
        Assert.True(stateIdx > errorIdx,
            "StateHasChanged() moet ná de _logoutError-toewijzing staan in het catch-block");
    }

    [Fact]
    public void MainLayout_SetupAutoCollapse_HasSeparate_ExceptionGuard()
    {
        var content = ReadSource(@"BootManager.Web\Components\Layout\MainLayout.razor");
        Assert.False(string.IsNullOrEmpty(content), "MainLayout.razor niet gevonden.");

        // setupAutoCollapse moet in een eigen try-block staan zodat een JSException
        // niet uit OnAfterRenderAsync kan ontsnappen en het circuit kan verbreken.
        // Minimaal 2 catch-blocks voor JSException: één voor de import, één voor setupAutoCollapse.
        var catchCount = System.Text.RegularExpressions.Regex.Matches(
            content, @"catch\s*\(JSException").Count;
        Assert.True(catchCount >= 2,
            $"Verwacht minimaal 2 JSException-catch-blocks in MainLayout " +
            $"(import + setupAutoCollapse), gevonden: {catchCount}");
    }

    [Fact]
    public void MainLayout_Logout_ReturnsEarly_WhenModuleIsNull()
    {
        var content = ReadSource(@"BootManager.Web\Components\Layout\MainLayout.razor");
        Assert.False(string.IsNullOrEmpty(content), "MainLayout.razor niet gevonden.");

        // Logout moet een vroeg-return hebben wanneer _authModule null is,
        // zodat er geen stille knop optreedt en geen endpointaanroep zonder module.
        Assert.Contains("_authModule is null", content);
        Assert.Contains("Actie mislukt", content);
    }

    // --- Compilatiecheck: ToggleMenu en CloseMenu bestaan als methoden in de gecompileerde klasse ---

    private static readonly Assembly WebAssembly =
        typeof(BootManager.Web.Middleware.PcrGateMiddleware).Assembly;

    [Fact]
    public void MainLayout_CompiledType_HasToggleMenu_And_CloseMenu()
    {
        var type = WebAssembly.GetType("BootManager.Web.Components.Layout.MainLayout");
        Assert.NotNull(type);

        var toggle = type.GetMethod("ToggleMenu", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(toggle);

        var close = type.GetMethod("CloseMenu", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Assert.NotNull(close);
    }
}
