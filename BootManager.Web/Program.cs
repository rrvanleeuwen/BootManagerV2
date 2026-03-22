using BootManager.Application;
using BootManager.Application.Authentication.DTOs;
using BootManager.Application.Authentication.Services;
using BootManager.Infrastructure;
using BootManager.Infrastructure.Persistence;
using BootManager.Web.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Infra + App
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplicationServices();

// Add controllers (Web API)
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BootManager API", Version = "v1" });
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {token}'"
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new[] { "Bearer" } }
    });
});


// Authentication: Cookie (Blazor server) + JWT Bearer (Web API)
builder.Services
    .AddAuthentication(options =>
    {
        // keep cookies as the default for interactive parts of the app
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.Cookie.Name = "bm.auth";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // use Always if HTTPS-only
        options.SlidingExpiration = true;
    })
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"] ?? "please_change_this_secret_for_production";
        var keyBytes = Encoding.UTF8.GetBytes(key);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
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
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<BootManagerDbContext>();
    await db.Database.MigrateAsync();

    // Ensure a test admin exists for development/testing only. Uses the application
    // registration service so hashing/encryption are applied consistently.
    if (app.Environment.IsDevelopment())
    {
        try
        {
            var reg = services.GetRequiredService<BootManager.Application.OwnerRegistration.Services.IOwnerRegistrationService>();
            var logger = services.GetRequiredService<ILogger<Program>>();

            var firstRun = await reg.CheckFirstRunAsync();
            if (firstRun.IsFirstRun)
            {
                logger.LogInformation("No owner found - creating default test admin (Development only).");
                var config = services.GetRequiredService<IConfiguration>();
                var devEmail = config["DevAdmin:Email"] ?? "admin@localhost";
                var devPassword = config["DevAdmin:Password"] ?? "Admin123!";

                var req = new BootManager.Application.OwnerRegistration.DTOs.RegisterOwnerRequestDto
                {
                    Name = "Administrator",
                    Email = devEmail,
                    Password = devPassword,
                    ConfirmPassword = devPassword,
                    GenerateRecoveryCode = false
                };

                await reg.RegisterFirstOwnerAsync(req);
                logger.LogInformation("Default test admin created (email=admin@localhost). Do NOT use this in production.");
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Failed to ensure default admin.");
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

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

// NOTE: change-password / set-pin / clear-pin minimal APIs removed.
// These operations are invoked directly from Blazor Server components via DI
// (IOwnerSettingsService). Keep login/logout endpoints because they must be
// invoked from the browser so the authentication cookie is set/cleared.

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();
