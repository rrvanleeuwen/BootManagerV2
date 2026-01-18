using BootManager.Application.Authentication.Services;
using BootManager.Application.OwnerRegistration.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BootManager.Application;

/// <summary>
/// Registreert Application-services voor DI.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOwnerRegistrationService, OwnerRegistrationService>();
        services.AddScoped<IOwnerLoginService, OwnerLoginService>();
        return services;
    }
}