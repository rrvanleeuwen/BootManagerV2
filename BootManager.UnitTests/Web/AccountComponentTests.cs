namespace BootManager.UnitTests.Web;

/// <summary>
/// Regressietests voor de Account-component en de betrokken disposal-paden.
/// Bewijst dat de formulierflow nooit als native POST /account eindigt en dat
/// JSDisconnectedException tijdens disposal veilig wordt genegeerd.
/// </summary>
public class AccountComponentTests
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

    // --- Account.razor: geen native form-submit naar /account ---

    [Fact]
    public void Account_SubmitButton_IsTypeButton_NotTypeSubmit()
    {
        var content = ReadSource(@"BootManager.Web\Components\Pages\Account.razor");
        Assert.False(string.IsNullOrEmpty(content), "Account.razor niet gevonden.");
        // type="submit" zou een native POST /account veroorzaken
        Assert.DoesNotContain("type=\"submit\"", content);
        // type="button" met @onclick voorkomt native submission
        Assert.Contains("type=\"button\"", content);
        Assert.Contains("@onclick=\"HandleChangePassword\"", content);
    }

    [Fact]
    public void Account_EditForm_DoesNotUseOnValidSubmit()
    {
        var content = ReadSource(@"BootManager.Web\Components\Pages\Account.razor");
        Assert.False(string.IsNullOrEmpty(content), "Account.razor niet gevonden.");
        // OnValidSubmit="..." as attribute would trigger native submit-path in SSR/disconnected mode
        Assert.DoesNotContain("OnValidSubmit=", content);
    }

    [Fact]
    public void Account_Handler_CallsOnlyChangePasswordEndpoint()
    {
        var content = ReadSource(@"BootManager.Web\Components\Pages\Account.razor");
        Assert.False(string.IsNullOrEmpty(content), "Account.razor niet gevonden.");
        Assert.Contains("\"/auth/change-password\"", content);
        // Geen rechtstreekse server-side AccountService-aanroep vanuit de pagina
        Assert.DoesNotContain("AccountService", content);
    }

    [Fact]
    public void Account_Handler_ValidatesViaEditContextBeforeRequest()
    {
        var content = ReadSource(@"BootManager.Web\Components\Pages\Account.razor");
        Assert.False(string.IsNullOrEmpty(content), "Account.razor niet gevonden.");
        Assert.Contains("_editContext.Validate()", content);
    }

    [Fact]
    public void Account_NavigatesWithForceLoad_AfterSuccess()
    {
        var content = ReadSource(@"BootManager.Web\Components\Pages\Account.razor");
        Assert.False(string.IsNullOrEmpty(content), "Account.razor niet gevonden.");
        // forceLoad: true is vereist zodat de browser het vernieuwde cookie laadt
        // en het nieuwe circuit PCR=false heeft (anders redirect-loop via OnboardingGate)
        Assert.Contains("forceLoad: true", content);
    }

    // --- Disposal: JSDisconnectedException veilig genegeerd ---

    [Fact]
    public void Account_DisposeAsync_CatchesJSDisconnectedException()
    {
        var content = ReadSource(@"BootManager.Web\Components\Pages\Account.razor");
        Assert.False(string.IsNullOrEmpty(content), "Account.razor niet gevonden.");
        Assert.Contains("JSDisconnectedException", content);
    }

    [Fact]
    public void Login_DisposeAsync_CatchesJSDisconnectedException()
    {
        var content = ReadSource(@"BootManager.Web\Components\Pages\Login.razor");
        Assert.False(string.IsNullOrEmpty(content), "Login.razor niet gevonden.");
        Assert.Contains("JSDisconnectedException", content);
    }

    [Fact]
    public void MainLayout_DisposeAsync_CatchesJSDisconnectedException()
    {
        var content = ReadSource(@"BootManager.Web\Components\Layout\MainLayout.razor");
        Assert.False(string.IsNullOrEmpty(content), "MainLayout.razor niet gevonden.");
        Assert.Contains("JSDisconnectedException", content);
    }

    // --- Module-URL: canoniek absoluut pad, geen route-relatieve import ---

    [Fact]
    public void AllAuthComponents_DoNotUse_RelativeModuleUrl()
    {
        var login = ReadSource(@"BootManager.Web\Components\Pages\Login.razor");
        var account = ReadSource(@"BootManager.Web\Components\Pages\Account.razor");
        var layout = ReadSource(@"BootManager.Web\Components\Layout\MainLayout.razor");

        Assert.False(string.IsNullOrEmpty(login), "Login.razor niet gevonden.");
        Assert.False(string.IsNullOrEmpty(account), "Account.razor niet gevonden.");
        Assert.False(string.IsNullOrEmpty(layout), "MainLayout.razor niet gevonden.");

        // Route-relatieve import lost op t.o.v. /_framework/blazor.web.js → /_framework/js/authClient.js → 404 HTML → SyntaxError
        Assert.DoesNotContain("\"./js/authClient.js\"", login);
        Assert.DoesNotContain("\"./js/authClient.js\"", account);
        Assert.DoesNotContain("\"./js/authClient.js\"", layout);
    }

    [Fact]
    public void AllAuthComponents_UseCanonical_AbsoluteModuleUrl()
    {
        var login = ReadSource(@"BootManager.Web\Components\Pages\Login.razor");
        var account = ReadSource(@"BootManager.Web\Components\Pages\Account.razor");
        var layout = ReadSource(@"BootManager.Web\Components\Layout\MainLayout.razor");

        Assert.False(string.IsNullOrEmpty(login), "Login.razor niet gevonden.");
        Assert.False(string.IsNullOrEmpty(account), "Account.razor niet gevonden.");
        Assert.False(string.IsNullOrEmpty(layout), "MainLayout.razor niet gevonden.");

        // /js/authClient.js is de canonieke assetroute (base href="/")
        Assert.Contains("\"/js/authClient.js\"", login);
        Assert.Contains("\"/js/authClient.js\"", account);
        Assert.Contains("\"/js/authClient.js\"", layout);
    }

    [Fact]
    public void AllAuthComponents_ModuleImportError_IsCaught_InOnAfterRenderAsync()
    {
        var login = ReadSource(@"BootManager.Web\Components\Pages\Login.razor");
        var account = ReadSource(@"BootManager.Web\Components\Pages\Account.razor");
        var layout = ReadSource(@"BootManager.Web\Components\Layout\MainLayout.razor");

        Assert.False(string.IsNullOrEmpty(login), "Login.razor niet gevonden.");
        Assert.False(string.IsNullOrEmpty(account), "Account.razor niet gevonden.");
        Assert.False(string.IsNullOrEmpty(layout), "MainLayout.razor niet gevonden.");

        // JSException moet worden gevangen in OnAfterRenderAsync zodat het circuit niet crasht
        Assert.Contains("JSException", login);
        Assert.Contains("JSException", account);
        Assert.Contains("JSException", layout);
    }
}
