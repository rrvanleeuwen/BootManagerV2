using BootManager.Application.Authentication.Services;
using BootManager.Application.OwnerRegistration.Services;
using BootManager.Application.NetworkMessages.Services;
using BootManager.Application.NetworkMessageParsing.Services;
using BootManager.Application.NetworkMessageInterpretation.Contracts;
using BootManager.Application.NetworkMessageInterpretation.DTOs;
using BootManager.Application.NetworkMessageInterpretation.Services;
using BootManager.Application.BatteryMeasurements.Services;
using BootManager.Application.DepthMeasurements.Services;
using BootManager.Application.MotionMeasurements.Services;
using BootManager.Application.PositionMeasurements.Services;
using BootManager.Application.WindMeasurements.Services;
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
        services.AddScoped<IOwnerRecoveryService, OwnerRecoveryService>();
        services.AddScoped<IOwnerSettingsService, OwnerSettingsService>();

        // Registratie van NetworkMessage application-service (gebruik generieke repository)
        services.AddScoped<INetworkMessageService, NetworkMessageService>();

        // Registratie van NetworkMessageParser service
        services.AddScoped<INetworkMessageParserService, NetworkMessageParserService>();

        // Registratie van netwerkbericht-interpreters
        // Dit zijn stateless application services die semantische interpretatie uitvoeren
        // bovenop technische parse-resultaten. Transient is geschikt omdat geen state nodig is.
        services.AddTransient<INetworkMessageInterpreter<BatteryMessageInterpretationDto>, BatteryMessageInterpreterService>();
        services.AddTransient<INetworkMessageInterpreter<DepthMessageInterpretationDto>, DepthMessageInterpreterService>();
        services.AddTransient<INetworkMessageInterpreter<MotionMessageInterpretationDto>, MotionMessageInterpreterService>();
        services.AddTransient<INetworkMessageInterpreter<PositionMessageInterpretationDto>, PositionMessageInterpreterService>();
        services.AddTransient<INetworkMessageInterpreter<WindMessageInterpretationDto>, WindMessageInterpreterService>();

        // Registratie van BatteryMeasurement application-service (gebruik generieke repository)
        services.AddScoped<IBatteryMeasurementService, BatteryMeasurementService>();

        // Registratie van DepthMeasurement application-service (gebruik generieke repository)
        services.AddScoped<IDepthMeasurementService, DepthMeasurementService>();

        // Registratie van MotionMeasurement application-service (gebruik generieke repository)
        services.AddScoped<IMotionMeasurementService, MotionMeasurementService>();

        // Registratie van PositionMeasurement application-service (gebruik generieke repository)
        services.AddScoped<IPositionMeasurementService, PositionMeasurementService>();

        // Registratie van WindMeasurement application-service (gebruik generieke repository)
        services.AddScoped<IWindMeasurementService, WindMeasurementService>();

        return services;
    }
}