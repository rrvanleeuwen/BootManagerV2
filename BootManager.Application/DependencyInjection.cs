using BootManager.Application.OwnerRegistration.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BootManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOwnerRegistrationService, OwnerRegistrationService>();
        return services;
    }
}