using BootManager.Core.Interfaces;
using BootManager.Application.Dashboard.Services;
using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Logbook.Services;
using BootManager.Application.Storage.Contracts;
using BootManager.Infrastructure.Dashboard;
using BootManager.Infrastructure.Inventory;
using BootManager.Infrastructure.Logbook;
using BootManager.Infrastructure.Persistence;
using BootManager.Infrastructure.Repositories;
using BootManager.Infrastructure.Security;
using BootManager.Infrastructure.Storage.QrCoder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BootManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var conn = config.GetConnectionString("Default") ?? "Data Source=bootmanager.db";
        services.AddDbContextFactory<BootManagerDbContext>(o => o.UseSqlite(conn));

        // Generieke repository
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<ILogbookEntryDeletionService, LogbookEntryDeletionService>();
        services.AddScoped<IDashboardMeasurementService, DashboardMeasurementService>();

        // QR rendering voor opslaglocatietags
        services.AddScoped<IStorageLocationQrTagRenderer, QrCoderStorageLocationQrTagRenderer>();

        // Gerichte EF Core-readmodel voor het productoverzicht (databasegestuurde paginering)
        services.AddScoped<IProductOverviewReadQuery, ProductOverviewReadQuery>();

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IEncryptionService>(_ =>
            new AesGcmEncryptionService(config["Encryption:Key"] ?? "DEV_CHANGE_ME_KEY"));
        services.AddSingleton<ISystemClock, SystemClock>();

        return services;
    }
}
