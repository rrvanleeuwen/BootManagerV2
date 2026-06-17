using System.Security.Claims;

namespace BootManager.Web.Middleware;

/// <summary>
/// Middleware die server-side afdwingt dat Crew-gebruikers met PasswordChangeRequired=true
/// uitsluitend de account- en uitlogpagina's kunnen bereiken totdat zij hun wachtwoord hebben gewijzigd.
/// Owner-gebruikers worden niet geblokkeerd: zij doorlopen hun eigen onboardingflow via /onboarding.
/// API-verzoeken van geblokkeerde Crew krijgen HTTP 403; overige verzoeken worden omgeleid naar /account.
/// </summary>
public sealed class PcrGateMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly HashSet<string> AllowedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/account",
        "/auth/logout",
        "/auth/change-password",
        "/login",
        "/health",
        // Blazor framework assets and interactive server hub are required for
        // circuit setup on whitelisted pages such as /account.
        "/_framework",
        "/_blazor"
    };

    public PcrGateMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var pcrClaim = context.User.FindFirst("bm.password_change_required")?.Value;
            if (string.Equals(pcrClaim, "true", StringComparison.OrdinalIgnoreCase))
            {
                // PCR-gate geldt uitsluitend voor Crew. Owner beheert zijn eigen onboardingflow
                // via /onboarding en mag niet naar /account worden omgeleid.
                if (context.User.IsInRole("Crew"))
                {
                    var path = context.Request.Path.Value ?? "/";
                    if (!IsAllowed(path))
                    {
                        if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsJsonAsync(new { message = "Wachtwoord wijzigen vereist." });
                            return;
                        }

                        context.Response.Redirect("/account");
                        return;
                    }
                }
            }
        }

        await _next(context);
    }

    public static bool IsAllowed(string path)
    {
        foreach (var allowed in AllowedPaths)
        {
            if (path.Equals(allowed, StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith(allowed + "/", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
