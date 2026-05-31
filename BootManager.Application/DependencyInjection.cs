using BootManager.Application.Analysis.Services;
using BootManager.Application.Authentication.Services;
using BootManager.Application.Dashboard.Services;
using BootManager.Application.OperationalSettings.Services;
using BootManager.Application.Logbook.Services;
using BootManager.Application.OwnerRegistration.Services;
using BootManager.Application.VesselProfile.Services;
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
using BootManager.Application.HeadingMeasurements.Services;
using BootManager.Application.SpeedThroughWaterMeasurements.Services;
using BootManager.Application.WaterTemperatureMeasurements.Services;
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
        services.AddScoped<IBootstrapOwnerService, BootstrapOwnerService>();
        services.AddScoped<IOwnerSetupStateService, OwnerSetupStateService>();
        services.AddScoped<IOnboardingService, OnboardingService>();
        services.AddScoped<IOwnerLoginService, OwnerLoginService>();
        services.AddScoped<IOwnerRecoveryService, OwnerRecoveryService>();
        services.AddScoped<IOwnerSettingsService, OwnerSettingsService>();

        // Registratie van VesselProfile application-service
        services.AddScoped<IVesselProfileService, VesselProfileService>();

        // Registratie van Analysis application-service
        services.AddScoped<IAnalysisService, AnalysisService>();

        // Registratie van Dashboard measurement application-service
        services.AddScoped<IDashboardMeasurementService, DashboardMeasurementService>();

        // Registratie van NetworkMessage application-service (gebruik generieke repository)
        services.AddScoped<INetworkMessageService, NetworkMessageService>();

        // Registratie van NetworkMessageParser service
        services.AddScoped<INetworkMessageParserService, NetworkMessageParserService>();

        // Registratie van NMEA 0183 parser service
        services.AddScoped<INmea0183ParserService, Nmea0183ParserService>();

        // Registratie van netwerkbericht-interpreters
        // Dit zijn stateless application services die semantische interpretatie uitvoeren
        // bovenop technische parse-resultaten. Transient is geschikt omdat geen state nodig is.
        services.AddTransient<INetworkMessageInterpreter<BatteryMessageInterpretationDto>, BatteryMessageInterpreterService>();
        services.AddTransient<INetworkMessageInterpreter<DepthMessageInterpretationDto>, DepthMessageInterpreterService>();
        services.AddTransient<INetworkMessageInterpreter<MotionMessageInterpretationDto>, MotionMessageInterpreterService>();
        services.AddTransient<INetworkMessageInterpreter<PositionMessageInterpretationDto>, PositionMessageInterpreterService>();
        services.AddTransient<INetworkMessageInterpreter<WindMessageInterpretationDto>, WindMessageInterpreterService>();
        services.AddTransient<INetworkMessageInterpreter<HeadingMessageInterpretationDto>, HeadingMessageInterpreterService>();
        services.AddTransient<INetworkMessageInterpreter<SpeedThroughWaterMessageInterpretationDto>, SpeedThroughWaterMessageInterpreterService>();
        services.AddTransient<INetworkMessageInterpreter<WaterTemperatureMessageInterpretationDto>, WaterTemperatureMessageInterpreterService>();

        // Registratie van NMEA 0183 Fase 3a interpreters
        services.AddTransient<INmea0183MessageInterpreter<SpeedThroughWaterMessageInterpretationDto>, Nmea0183VhwInterpreterService>();
        services.AddTransient<INmea0183MessageInterpreter<WaterTemperatureMessageInterpretationDto>, Nmea0183MtwInterpreterService>();
        services.AddTransient<INmea0183MessageInterpreter<DepthMessageInterpretationDto>, Nmea0183DbtDptInterpreterService>();

        // Registratie van NMEA 0183 Fase 3b interpreters
        services.AddTransient<INmea0183MessageInterpreter<WindMessageInterpretationDto>, Nmea0183MwvInterpreterService>();
        services.AddTransient<INmea0183MessageInterpreter<HeadingMessageInterpretationDto>, Nmea0183HdtHdmInterpreterService>();

        // Registratie van NMEA 0183 Fase 3c interpreters
        services.AddTransient<INmea0183MessageInterpreter<Nmea0183RmcInterpretationDto>, Nmea0183RmcInterpreterService>();
        services.AddTransient<INmea0183MessageInterpreter<PositionMessageInterpretationDto>, Nmea0183GgaInterpreterService>();

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

        // Registratie van HeadingMeasurement application-service (gebruik generieke repository)
        services.AddScoped<IHeadingMeasurementService, HeadingMeasurementService>();

        // Registratie van SpeedThroughWaterMeasurement application-service (gebruik generieke repository)
        services.AddScoped<ISpeedThroughWaterMeasurementService, SpeedThroughWaterMeasurementService>();

        // Registratie van WaterTemperatureMeasurement application-service (gebruik generieke repository)
        services.AddScoped<IWaterTemperatureMeasurementService, WaterTemperatureMeasurementService>();

        // Registratie van Logboek application-service
        services.AddScoped<ILogbookService, LogbookService>();

        // Registratie van Logboek meetdata-suggestie service
        services.AddScoped<ILogbookMeasurementSuggestionService, LogbookMeasurementSuggestionService>();

        // Registratie van Logboek-regel detail service
        services.AddScoped<ILogbookEntryDetailService, LogbookEntryDetailService>();

        // Registratie van Logboek bijlagen service
        services.AddScoped<ILogbookAttachmentService, LogbookAttachmentService>();

        // Registratie van operationele instellingen service
        services.AddScoped<IOperationalSettingsService, OperationalSettingsService>();

        return services;
    }
}