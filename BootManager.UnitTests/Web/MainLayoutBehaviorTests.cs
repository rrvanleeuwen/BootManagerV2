using Bunit;
using Bunit.TestDoubles;
using BootManager.Web.Components.Layout;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace BootManager.UnitTests.Web;

/// <summary>
/// Gedragstests voor MainLayout.razor met een gecontroleerde fake IJSRuntime (bUnit).
/// bUnit is de minimale componenttestdependency: Razor-componenten vereisen een echte
/// render-tree om DOM-output, lifecycle-methoden en event-handler-gedrag te testen —
/// brontekstinspectie is hiervoor niet voldoende.
///
/// Technische noot: bUnit's JSRuntimeMode.Strict gooit een eigen
/// JSRuntimeUnhandledInvocationException (geen JSException), zodat de component
/// deze niet afvangt. Daarom registreert deze testklasse via Services.Replace een
/// echte ControllableJSRuntime die werkelijke JSException-waarden gooit.
/// </summary>
public class MainLayoutBehaviorTests : TestContext
{
    private readonly FakeLogger<MainLayout> _logger;
    private readonly ControllableJSRuntime _fakeJs;

    public MainLayoutBehaviorTests()
    {
        _fakeJs = new ControllableJSRuntime();
        _logger = new FakeLogger<MainLayout>();
        // Replace bUnit's BunitJSRuntime met onze ControllableJSRuntime zodat
        // de component echte JSException-waarden ontvangt.
        Services.Replace(ServiceDescriptor.Singleton<IJSRuntime>(_fakeJs));
        Services.AddSingleton<ILogger<MainLayout>>(_logger);
        this.AddTestAuthorization().SetAuthorized("TestUser");
    }

    // ─── Punt 1 + 2 + 3: import-fout ontsnapt niet, melding zichtbaar, gelogd op Error ───

    /// <summary>
    /// Bewijst dat een JSException bij de module-import:
    ///   1. niet ontsnapt uit de eerste render;
    ///   2. een zichtbare melding oplevert in de gerenderde layout;
    ///   3. gelogd wordt op Error-niveau met een herkenbaar bericht.
    /// </summary>
    [Fact]
    public void ImportFailure_DoesNotEscape_ShowsErrorInLayout_AndLogsAtError()
    {
        _fakeJs.FailImport = true;

        // 1. Exception ontsnapt niet uit de eerste render
        IRenderedComponent<MainLayout> cut = null!;
        var renderEx = Record.Exception(() =>
            cut = RenderComponent<MainLayout>(p => p.Add(m => m.Body, "<p>test</p>")));
        Assert.Null(renderEx);

        // 2. Zichtbare importfout in de gerenderde layout
        cut.WaitForAssertion(
            () => Assert.NotEmpty(cut.FindAll(".alert.alert-danger")),
            TimeSpan.FromSeconds(3));
        Assert.Contains("Navigatiemodule", cut.Find(".alert.alert-danger").TextContent);

        // 3. Importfout gelogd op Error-niveau met herkenbaar bericht
        Assert.Contains(_logger.Entries, e =>
            e.Level == LogLevel.Error && e.Message.Contains("authClient"));
    }

    // ─── Punt 4: setupAutoCollapse-fout ontsnapt niet, gelogd op Warning met ander bericht ───

    /// <summary>
    /// Bewijst dat een JSException bij setupAutoCollapse:
    ///   4a. niet ontsnapt;
    ///   4b. gelogd wordt op Warning-niveau met een bericht dat "setupAutoCollapse" bevat;
    ///   4c. een ander bericht gebruikt dan de importfout.
    /// </summary>
    [Fact]
    public void SetupAutoCollapseFailure_DoesNotEscape_AndLogsAtWarningWithDistinctMessage()
    {
        _fakeJs.FailSetupAutoCollapse = true;

        IRenderedComponent<MainLayout> cut = null!;
        var renderEx = Record.Exception(() =>
            cut = RenderComponent<MainLayout>(p => p.Add(m => m.Body, "<p>test</p>")));

        // 4a. Exception ontsnapt niet
        Assert.Null(renderEx);

        // 4b. Gelogd op Warning-niveau met "setupAutoCollapse" in het bericht.
        // RenderComponent wacht tot OnAfterRenderAsync voltooid is; daarna zijn de
        // logger-entries direct beschikbaar zonder WaitForAssertion.
        cut!.WaitForAssertion(
            () => Assert.Contains(_logger.Entries, e =>
                e.Level == LogLevel.Warning && e.Message.Contains("setupAutoCollapse")),
            TimeSpan.FromSeconds(3));

        // 4c. Het Warning-bericht verschilt van het Error-bericht voor de import
        Assert.DoesNotContain(_logger.Entries, e =>
            e.Level == LogLevel.Warning && e.Message.Contains("/js/authClient.js"));
    }

    // ─── Punt 5 + 6: logout zonder module toont melding, roept postJson NIET aan ───

    /// <summary>
    /// Bewijst dat Logout() wanneer _authModule null is:
    ///   5. een zichtbare melding geeft ("Actie mislukt");
    ///   6. de postJson-aanroep overslaat (null-guard keert vroeg terug).
    /// </summary>
    [Fact]
    public void LogoutWithNullModule_ShowsActieMislukt_AndDoesNotCallPostJson()
    {
        _fakeJs.FailImport = true;

        var cut = RenderComponent<MainLayout>(p => p.Add(m => m.Body, "<p>test</p>"));

        // Wacht tot initialisatie voltooid is (_authModule null, importfout zichtbaar)
        cut.WaitForAssertion(
            () => Assert.NotEmpty(cut.FindAll(".alert.alert-danger")),
            TimeSpan.FromSeconds(3));

        // Open het gebruikersmenu
        cut.Find("#userDropdown").Click();

        // Klik op de Uitloggen-knop (aanwezig zodra _menuOpen = true)
        var logoutBtn = cut.FindAll("button").First(b => b.TextContent.Trim() == "Uitloggen");
        logoutBtn.Click();

        // 5. Zichtbare melding "Actie mislukt" (null-guard pad genomen)
        cut.WaitForAssertion(
            () => Assert.Contains("Actie mislukt", cut.Find(".alert.alert-danger").TextContent),
            TimeSpan.FromSeconds(3));

        // 6. postJson NIET aangeroepen: null-guard keerde vroeg terug voor InvokeAsync op de module
        Assert.DoesNotContain("Uitloggen mislukt", cut.Find(".alert.alert-danger").TextContent);
        Assert.False(_fakeJs.PostJsonWasCalled,
            "postJson mag niet zijn aangeroepen: de null-guard in Logout() keerde vroeg terug");
    }

    // ─── Fake implementaties ──────────────────────────────────────────────────────────────

    /// <summary>
    /// IJSRuntime die werkelijke JSException gooit voor configureerbare aanroepen,
    /// zodat de component de catch-blocks kan uitvoeren zoals in productie.
    /// </summary>
    private sealed class ControllableJSRuntime : IJSRuntime
    {
        public bool FailImport { get; set; }
        public bool FailSetupAutoCollapse { get; set; }
        public bool PostJsonWasCalled => _module.PostJsonCalled;

        private readonly FakeModule _module = new();

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            if (identifier == "import" && FailImport)
                return new ValueTask<TValue>(
                    Task.FromException<TValue>(new JSException("module import failed")));

            if (identifier == "import")
                return new ValueTask<TValue>((TValue)(object)_module);

            if (identifier == "setupAutoCollapse" && FailSetupAutoCollapse)
                return new ValueTask<TValue>(
                    Task.FromException<TValue>(new JSException("setupAutoCollapse failed")));

            return ValueTask.FromResult<TValue>(default!);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken ct, object?[]? args)
            => InvokeAsync<TValue>(identifier, args);

        private sealed class FakeModule : IJSObjectReference
        {
            public bool PostJsonCalled { get; private set; }

            public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            {
                if (identifier == "postJson")
                    PostJsonCalled = true;
                return ValueTask.FromResult<TValue>(default!);
            }

            public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken ct, object?[]? args)
                => InvokeAsync<TValue>(identifier, args);

            public ValueTask DisposeAsync() => ValueTask.CompletedTask;
        }
    }

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public readonly List<(LogLevel Level, string Message, Exception? Exception)> Entries = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, formatter(state, exception), exception));
    }
}
