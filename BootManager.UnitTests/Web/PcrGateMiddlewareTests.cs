using System.Security.Claims;
using BootManager.Web.Middleware;
using Microsoft.AspNetCore.Http;

namespace BootManager.UnitTests.Web;

/// <summary>
/// Unit tests voor PcrGateMiddleware: server-side Crew PasswordChangeRequired-gate.
/// </summary>
public class PcrGateMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_PassesThrough_WhenUserNotAuthenticated()
    {
        var ctx = CreateContext("/api/logbook", authenticated: false, pcrTrue: false);
        var nextCalled = false;
        var sut = new PcrGateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await sut.InvokeAsync(ctx);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_PassesThrough_WhenPcrFalse()
    {
        var ctx = CreateContext("/api/logbookattachments/1", authenticated: true, pcrTrue: false);
        var nextCalled = false;
        var sut = new PcrGateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await sut.InvokeAsync(ctx);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_Returns403_ForApiPath_WhenCrewPcrTrue()
    {
        var ctx = CreateContext("/api/logbookattachments/upload", authenticated: true, pcrTrue: true, role: "Crew");
        var nextCalled = false;
        var sut = new PcrGateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await sut.InvokeAsync(ctx);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_Redirects_ForBlazorPath_WhenCrewPcrTrue()
    {
        var ctx = CreateContext("/logbook", authenticated: true, pcrTrue: true, role: "Crew");
        var nextCalled = false;
        var sut = new PcrGateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await sut.InvokeAsync(ctx);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status302Found, ctx.Response.StatusCode);
        Assert.Equal("/account", ctx.Response.Headers.Location.ToString());
    }

    [Fact]
    public async Task InvokeAsync_AllowsAccountPath_WhenCrewPcrTrue()
    {
        var ctx = CreateContext("/account", authenticated: true, pcrTrue: true, role: "Crew");
        var nextCalled = false;
        var sut = new PcrGateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await sut.InvokeAsync(ctx);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_AllowsAuthLogout_WhenCrewPcrTrue()
    {
        var ctx = CreateContext("/auth/logout", authenticated: true, pcrTrue: true, role: "Crew");
        var nextCalled = false;
        var sut = new PcrGateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await sut.InvokeAsync(ctx);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_AllowsAuthChangePassword_WhenCrewPcrTrue()
    {
        var ctx = CreateContext("/auth/change-password", authenticated: true, pcrTrue: true, role: "Crew");
        var nextCalled = false;
        var sut = new PcrGateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await sut.InvokeAsync(ctx);

        Assert.True(nextCalled);
    }

    [Theory]
    [InlineData("/_framework/blazor.web.js")]
    [InlineData("/_blazor")]
    public async Task InvokeAsync_AllowsBlazorInfrastructure_WhenCrewPcrTrue(string path)
    {
        var ctx = CreateContext(path, authenticated: true, pcrTrue: true, role: "Crew");
        var nextCalled = false;
        var sut = new PcrGateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await sut.InvokeAsync(ctx);

        Assert.True(nextCalled);
    }

    // --- Owner met PCR=true: gate mag Owner nooit blokkeren ---

    [Fact]
    public async Task InvokeAsync_PassesThrough_ForOwner_WithPcrTrue_OnDashboard()
    {
        var ctx = CreateContext("/dashboard", authenticated: true, pcrTrue: true, role: "Owner");
        var nextCalled = false;
        var sut = new PcrGateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await sut.InvokeAsync(ctx);

        // Owner mag NIET worden omgeleid naar /account; Owner gebruikt eigen onboardingflow
        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_PassesThrough_ForOwner_WithPcrTrue_OnOnboarding()
    {
        var ctx = CreateContext("/onboarding", authenticated: true, pcrTrue: true, role: "Owner");
        var nextCalled = false;
        var sut = new PcrGateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await sut.InvokeAsync(ctx);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_PassesThrough_ForOwner_WithPcrTrue_OnSettings()
    {
        var ctx = CreateContext("/settings", authenticated: true, pcrTrue: true, role: "Owner");
        var nextCalled = false;
        var sut = new PcrGateMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        await sut.InvokeAsync(ctx);

        Assert.True(nextCalled);
    }

    [Theory]
    [InlineData("/account")]
    [InlineData("/auth/logout")]
    [InlineData("/auth/change-password")]
    [InlineData("/login")]
    [InlineData("/health")]
    [InlineData("/_framework/blazor.web.js")]
    [InlineData("/_blazor")]
    public void IsAllowed_ReturnsTrue_ForWhitelistedPaths(string path)
    {
        Assert.True(PcrGateMiddleware.IsAllowed(path));
    }

    [Theory]
    [InlineData("/api/logbookattachments")]
    [InlineData("/logbook")]
    [InlineData("/dashboard")]
    [InlineData("/settings")]
    public void IsAllowed_ReturnsFalse_ForNonWhitelistedPaths(string path)
    {
        Assert.False(PcrGateMiddleware.IsAllowed(path));
    }

    private static DefaultHttpContext CreateContext(string path, bool authenticated, bool pcrTrue, string? role = null)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = path;
        ctx.Response.Body = new MemoryStream();

        if (authenticated)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new("bm.password_change_required", pcrTrue ? "true" : "false")
            };
            if (role is not null)
                claims.Add(new Claim(ClaimTypes.Role, role));
            ctx.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        return ctx;
    }
}
