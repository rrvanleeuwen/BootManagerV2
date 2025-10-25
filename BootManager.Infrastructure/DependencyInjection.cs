using BootManager.Core.Interfaces;
using BootManager.Infrastructure.Persistence;
using BootManager.Infrastructure.Repositories;
using BootManager.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BootManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var conn = config.GetConnectionString("Default") ?? "Data Source=bootmanager.db";
        services.AddDbContext<BootManagerDbContext>(o => o.UseSqlite(conn));

        // Generieke repository
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IEncryptionService>(_ =>
            new AesGcmEncryptionService(config["Encryption:Key"] ?? "DEV_CHANGE_ME_KEY"));
        services.AddSingleton<ISystemClock, SystemClock>();

        return services;
    }
}