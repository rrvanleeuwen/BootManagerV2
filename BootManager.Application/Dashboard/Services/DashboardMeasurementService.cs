using BootManager.Application.Dashboard.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;

namespace BootManager.Application.Dashboard.Services;

/// <summary>
/// Implementatie van de dashboard-measurement service.
/// Haalt de recentste beschikbare meetwaarden op voor weergave op het operationele dashboard.
/// </summary>
public class DashboardMeasurementService : IDashboardMeasurementService
{
    private readonly IRepository<WindMeasurement> _windRepository;
    private readonly IRepository<HeadingMeasurement> _headingRepository;
    private readonly IRepository<PositionMeasurement> _positionRepository;
    private readonly IRepository<SpeedThroughWaterMeasurement> _speedThroughWaterRepository;
    private readonly IRepository<MotionMeasurement> _motionRepository;
    private readonly IRepository<DepthMeasurement> _depthRepository;
    private readonly IRepository<WaterTemperatureMeasurement> _waterTemperatureRepository;
    private readonly IRepository<BatteryMeasurement> _batteryRepository;
    private readonly IRepository<FluidLevelMeasurement> _fluidLevelRepository;

    public DashboardMeasurementService(
        IRepository<WindMeasurement> windRepository,
        IRepository<HeadingMeasurement> headingRepository,
        IRepository<PositionMeasurement> positionRepository,
        IRepository<SpeedThroughWaterMeasurement> speedThroughWaterRepository,
        IRepository<MotionMeasurement> motionRepository,
        IRepository<DepthMeasurement> depthRepository,
        IRepository<WaterTemperatureMeasurement> waterTemperatureRepository,
        IRepository<BatteryMeasurement> batteryRepository,
        IRepository<FluidLevelMeasurement> fluidLevelRepository)
    {
        _windRepository = windRepository;
        _headingRepository = headingRepository;
        _positionRepository = positionRepository;
        _speedThroughWaterRepository = speedThroughWaterRepository;
        _motionRepository = motionRepository;
        _depthRepository = depthRepository;
        _waterTemperatureRepository = waterTemperatureRepository;
        _batteryRepository = batteryRepository;
        _fluidLevelRepository = fluidLevelRepository;
    }

    public async Task<CurrentMeasurementsDto> GetCurrentMeasurementsAsync(CancellationToken cancellationToken = default)
    {
        // Get latest records for each measurement type
        var latestWind = await GetLatestAsync(_windRepository, cancellationToken);
        var latestHeading = await GetLatestAsync(_headingRepository, cancellationToken);
        var latestPosition = await GetLatestAsync(_positionRepository, cancellationToken);
        var latestSpeedThroughWater = await GetLatestAsync(_speedThroughWaterRepository, cancellationToken);
        var latestMotion = await GetLatestAsync(_motionRepository, cancellationToken);
        var latestDepth = await GetLatestAsync(_depthRepository, cancellationToken);
        var latestWaterTemperature = await GetLatestAsync(_waterTemperatureRepository, cancellationToken);
        var latestBattery = await GetLatestAsync(_batteryRepository, cancellationToken);

        // Get fluid levels: all records, grouped by (FluidType, FluidInstance), take latest from each group
        var fluidLevelDtos = await GetLatestFluidLevelsAsync(cancellationToken);

        // Build the DTO using object initializer
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

            SpeedThroughWater = latestSpeedThroughWater != null ? new SpeedThroughWaterMeasurementDto
            {
                SpeedKnots = latestSpeedThroughWater.SpeedKnots,
                RecordedAtUtc = latestSpeedThroughWater.RecordedAtUtc
            } : new(),

            Motion = latestMotion != null ? new MotionMeasurementDto
            {
                CourseOverGroundDegrees = latestMotion.CourseOverGroundDegrees,
                SpeedOverGroundKnots = ConvertSpeedToKnots(latestMotion.SpeedOverGround, latestMotion.SpeedUnit),
                RecordedAtUtc = latestMotion.RecordedAtUtc
            } : new(),

            Depth = latestDepth != null ? new DepthMeasurementDto
            {
                DepthMeters = latestDepth.DepthMeters,
                RecordedAtUtc = latestDepth.RecordedAtUtc
            } : new(),

            WaterTemperature = latestWaterTemperature != null ? new WaterTemperatureMeasurementDto
            {
                TemperatureCelsius = latestWaterTemperature.TemperatureCelsius,
                RecordedAtUtc = latestWaterTemperature.RecordedAtUtc
            } : new(),

            Battery = latestBattery != null ? new BatteryMeasurementDto
            {
                Voltage = latestBattery.Voltage,
                StateOfCharge = latestBattery.StateOfCharge,
                RecordedAtUtc = latestBattery.RecordedAtUtc
            } : new(),

            FluidLevels = fluidLevelDtos
        };
    }

    /// <summary>
    /// Haalt het meest recente record uit een repository op, gesorteerd op RecordedAtUtc (aflopend).
    /// </summary>
    private async Task<T?> GetLatestAsync<T>(IRepository<T> repository, CancellationToken ct) where T : class
    {
        var allRecords = await repository.ListAsync(null, ct);
        return allRecords.OrderByDescending(r =>
        {
            var prop = typeof(T).GetProperty("RecordedAtUtc");
            return (DateTime)(prop?.GetValue(r) ?? DateTime.MinValue);
        }).FirstOrDefault();
    }

    /// <summary>
    /// Haalt de meest recente fluid level voor elke (FluidType, FluidInstance) combinatie op.
    /// Retourneert een list van FluidLevelMeasurementDto's, gesorteerd op RecordedAtUtc (aflopend).
    /// </summary>
    private async Task<List<FluidLevelMeasurementDto>> GetLatestFluidLevelsAsync(CancellationToken ct)
    {
        var allFluidLevels = await _fluidLevelRepository.ListAsync(null, ct);

        // Group by (FluidType, FluidInstance) and take the latest from each group
        var latestPerTank = allFluidLevels
            .GroupBy(f => (f.FluidType, f.FluidInstance))
            .Select(g => g.OrderByDescending(f => f.RecordedAtUtc).First())
            .OrderByDescending(f => f.RecordedAtUtc)
            .ToList();

        // Convert to DTO list
        var dtos = latestPerTank.Select(f => new FluidLevelMeasurementDto
        {
            FluidType = f.FluidType,
            FluidInstance = f.FluidInstance,
            LevelPercent = f.LevelPercent,
            CapacityLiters = f.CapacityLiters,
            RecordedAtUtc = f.RecordedAtUtc,
            IsLevelInvalid = f.IsLevelInvalid
        }).ToList();

        return dtos;
    }

    /// <summary>
    /// Converteert snelheid naar knopen op basis van de gegeven eenheid.
    /// </summary>
    private decimal ConvertSpeedToKnots(decimal speed, string unit)
    {
        return unit?.ToLowerInvariant() switch
        {
            "kn" or "knots" or "kt" => speed,
            "m/s" or "mps" => speed * 1.94384m, // 1 m/s = 1.94384 knots
            "km/h" or "kmh" => speed * 0.539957m, // 1 km/h = 0.539957 knots
            _ => speed // Assume knots if unknown
        };
    }
}
