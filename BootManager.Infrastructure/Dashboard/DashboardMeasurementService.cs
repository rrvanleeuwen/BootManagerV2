using BootManager.Application.Dashboard.DTOs;
using BootManager.Application.Dashboard.Services;
using BootManager.Core.Entities;
using BootManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BootManager.Infrastructure.Dashboard;

/// <summary>
/// Reads dashboard measurements with a short-lived context per operation.
/// This avoids sharing a circuit-scoped DbContext with interactive Blazor work.
/// </summary>
public sealed class DashboardMeasurementService : IDashboardMeasurementService
{
    private readonly IDbContextFactory<BootManagerDbContext> _dbContextFactory;

    public DashboardMeasurementService(
        IDbContextFactory<BootManagerDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CurrentMeasurementsDto> GetCurrentMeasurementsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var latestWind = await GetLatestAsync(
            db.WindMeasurements,
            measurement => measurement.RecordedAtUtc,
            cancellationToken);
        var latestHeading = await GetLatestAsync(
            db.HeadingMeasurements,
            measurement => measurement.RecordedAtUtc,
            cancellationToken);
        var latestPosition = await GetLatestAsync(
            db.PositionMeasurements,
            measurement => measurement.RecordedAtUtc,
            cancellationToken);
        var latestSpeedThroughWater = await GetLatestAsync(
            db.SpeedThroughWaterMeasurements,
            measurement => measurement.RecordedAtUtc,
            cancellationToken);
        var latestMotion = await GetLatestAsync(
            db.MotionMeasurements,
            measurement => measurement.RecordedAtUtc,
            cancellationToken);
        var latestDepth = await GetLatestAsync(
            db.DepthMeasurements,
            measurement => measurement.RecordedAtUtc,
            cancellationToken);
        var latestWaterTemperature = await GetLatestAsync(
            db.WaterTemperatureMeasurements,
            measurement => measurement.RecordedAtUtc,
            cancellationToken);
        var latestBattery = await GetLatestAsync(
            db.BatteryMeasurements,
            measurement => measurement.RecordedAtUtc,
            cancellationToken);

        var allFluidLevels = await db.FluidLevelMeasurements
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var fluidLevelDtos = allFluidLevels
            .GroupBy(f => (f.FluidType, f.FluidInstance))
            .Select(g => g.OrderByDescending(f => f.RecordedAtUtc).First())
            .OrderByDescending(f => f.RecordedAtUtc)
            .Select(f => new FluidLevelMeasurementDto
            {
                FluidType = f.FluidType,
                FluidInstance = f.FluidInstance,
                LevelPercent = f.LevelPercent,
                CapacityLiters = f.CapacityLiters,
                RecordedAtUtc = f.RecordedAtUtc,
                IsLevelInvalid = f.IsLevelInvalid
            })
            .ToList();

        return new CurrentMeasurementsDto
        {
            Wind = latestWind != null ? new WindMeasurementDto
            {
                AngleDegrees = latestWind.WindAngleDegrees,
                SpeedMetersPerSecond = latestWind.WindSpeed,
                RecordedAtUtc = latestWind.RecordedAtUtc
            } : new(),
            Heading = latestHeading != null ? new HeadingMeasurementDto
            {
                HeadingDegrees = latestHeading.HeadingDegrees,
                RecordedAtUtc = latestHeading.RecordedAtUtc
            } : new(),
            Position = latestPosition != null ? new PositionMeasurementDto
            {
                Latitude = latestPosition.Latitude,
                Longitude = latestPosition.Longitude,
                RecordedAtUtc = latestPosition.RecordedAtUtc
            } : new(),
            SpeedThroughWater = latestSpeedThroughWater != null
                ? new SpeedThroughWaterMeasurementDto
                {
                    SpeedKnots = latestSpeedThroughWater.SpeedKnots,
                    RecordedAtUtc = latestSpeedThroughWater.RecordedAtUtc
                }
                : new(),
            Motion = latestMotion != null ? new MotionMeasurementDto
            {
                CourseOverGroundDegrees = latestMotion.CourseOverGroundDegrees,
                SpeedOverGroundKnots = ConvertSpeedToKnots(
                    latestMotion.SpeedOverGround,
                    latestMotion.SpeedUnit),
                RecordedAtUtc = latestMotion.RecordedAtUtc
            } : new(),
            Depth = latestDepth != null ? new DepthMeasurementDto
            {
                DepthMeters = latestDepth.DepthMeters,
                RecordedAtUtc = latestDepth.RecordedAtUtc
            } : new(),
            WaterTemperature = latestWaterTemperature != null
                ? new WaterTemperatureMeasurementDto
                {
                    TemperatureCelsius = latestWaterTemperature.TemperatureCelsius,
                    RecordedAtUtc = latestWaterTemperature.RecordedAtUtc
                }
                : new(),
            Battery = latestBattery != null ? new BatteryMeasurementDto
            {
                Voltage = latestBattery.Voltage,
                StateOfCharge = latestBattery.StateOfCharge,
                RecordedAtUtc = latestBattery.RecordedAtUtc
            } : new(),
            FluidLevels = fluidLevelDtos
        };
    }

    private static Task<T?> GetLatestAsync<T>(
        DbSet<T> set,
        Expression<Func<T, DateTime>> recordedAtSelector,
        CancellationToken cancellationToken)
        where T : class
    {
        return set
            .AsNoTracking()
            .OrderByDescending(recordedAtSelector)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static decimal ConvertSpeedToKnots(decimal speed, string? unit)
    {
        return unit?.ToLowerInvariant() switch
        {
            "kn" or "knots" or "kt" => speed,
            "m/s" or "mps" => speed * 1.94384m,
            "km/h" or "kmh" => speed * 0.539957m,
            _ => speed
        };
    }
}
