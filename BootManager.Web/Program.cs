using BootManager.Application;
using BootManager.Application.Authentication.DTOs;
using BootManager.Application.Authentication.Services;
using BootManager.Application.OperationalSettings.Services;
using BootManager.Infrastructure;
using BootManager.Infrastructure.Persistence;
using BootManager.Web.Components;
using BootManager.Web.Services;
using BootManager.Web.Options;
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
        options.Events.OnValidatePrincipal = context =>
        {
            var persistentClaim = context.Principal?.FindFirst("bm.persistent")?.Value;
            if (string.Equals(persistentClaim, "true", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            var sessionId = context.Principal?.FindFirst("bm.session_id")?.Value;
            var sessions = context.HttpContext.RequestServices.GetRequiredService<IAuthSessionStore>();
            if (!sessions.IsValid(sessionId))
            {
                context.RejectPrincipal();
                return context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return Task.CompletedTask;
        };
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
builder.Services.AddSingleton<IAuthSessionStore, AuthSessionStore>();

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

// Register IngestControlClient as a singleton for communicating with Ingest control API
builder.Services.AddSingleton<IIngestControlClient>(sp =>
{
    // Haal control API URL uit configuratie, fallback naar localhost
    var config = sp.GetRequiredService<IConfiguration>();
    var controlApiUrl = config["Ingest:ControlApi:BaseUrl"] ?? "http://127.0.0.1:5010";

    // Initialiseer de client met een aparte HttpClient
    var httpClient = new HttpClient();
    var logger = sp.GetRequiredService<ILogger<BootManager.Web.Services.IngestControlClient>>();
    return new BootManager.Web.Services.IngestControlClient(httpClient, logger, controlApiUrl);
});

// Register OperationalSettingsWithReloadService (voor UI en controller)
builder.Services.AddScoped<IOperationalSettingsWithReloadService, OperationalSettingsWithReloadService>();

// Configure shutdown options from appsettings
builder.Services.Configure<ShutdownOptions>(
    builder.Configuration.GetSection(ShutdownOptions.SectionName));

// Register ShutdownHelperExecutor for safe, bounded helper script execution
builder.Services.AddScoped<IShutdownHelperExecutor, ShutdownHelperExecutor>();

// Register ShutdownService (voor veilige Pi-shutdown)
builder.Services.AddScoped<BootManager.Application.Administration.Services.IShutdownService, BootManager.Web.Services.ShutdownService>();

var app = builder.Build();

// DB init/migratie
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<BootManagerDbContext>();
    await db.Database.MigrateAsync();

    // Ensure bootstrap owner exists
    try
    {
        var bootstrap = services.GetRequiredService<BootManager.Application.OwnerRegistration.Services.IBootstrapOwnerService>();
        var config = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        var bootstrapPassword = config["Bootstrap:DefaultPassword"];
        var isProduction = !app.Environment.IsDevelopment();

        var created = await bootstrap.EnsureBootstrapOwnerAsync(bootstrapPassword, isProduction);
        if (created)
        {
            logger.LogInformation("Bootstrap owner created successfully.");
        }
    }
    catch (InvalidOperationException ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(ex, "Bootstrap owner creation failed. Application cannot start.");
        throw;
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unexpected error during bootstrap owner setup.");
        throw;
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

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .AllowAnonymous();

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
        new(ClaimTypes.Role, "Owner"),
        new("bm.persistent", req.RememberMe ? "true" : "false")
    };

    if (!req.RememberMe)
    {
        var sessions = http.RequestServices.GetRequiredService<IAuthSessionStore>();
        claims.Add(new Claim("bm.session_id", sessions.CreateSession()));
    }

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
    var sessionId = http.User.FindFirst("bm.session_id")?.Value;
    http.RequestServices.GetRequiredService<IAuthSessionStore>().Remove(sessionId);
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
