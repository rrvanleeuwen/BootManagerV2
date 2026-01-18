using BootManager.Application;
using BootManager.Application.Authentication.DTOs;
using BootManager.Application.Authentication.Services;
using BootManager.Infrastructure;
using BootManager.Infrastructure.Persistence;
using BootManager.Web.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Infra + App
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplicationServices();

// Auth cookie
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.Cookie.Name = "bm.auth";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // use Always if HTTPS-only
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// Provide AuthenticationState to Razor Components
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

// Register a scoped HttpClient with the current app base URI (works in Blazor Server/Interactive Server)
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});

var app = builder.Build();

// DB init/migratie (pas aan naar MigrateAsync zodra je migrations gebruikt)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BootManagerDbContext>();
    await db.Database.MigrateAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

// Minimal API: login
app.MapPost("/auth/login", async (LoginRequestDto req, IOwnerLoginService login, HttpContext http) =>
{
    var result = await login.ValidateAsync(req, http.RequestAborted);
    if (!result.Success || result.OwnerId is null)
        return Results.BadRequest(new { message = result.Message ?? "Inloggen mislukt." });

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, result.OwnerId.Value.ToString()),
        new(ClaimTypes.Name, "Owner"),
        new(ClaimTypes.Role, "Owner")
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    var props = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
    {
        IsPersistent = req.RememberMe
    };

    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
    return Results.Ok();
})
.DisableAntiforgery();

// Minimal API: logout
app.MapPost("/auth/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
})
.DisableAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();
