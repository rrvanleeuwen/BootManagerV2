using BootManager.Application.Analysis.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using System.Text.Json;

namespace BootManager.Application.Analysis.Services;

/// <summary>
/// Implementatie van de analyse-service.
/// Verzamelt diagnostische gegevens over NetworkMessages en Measurements voor een gegeven tijdsvenster.
/// </summary>
public class AnalysisService : IAnalysisService
{
    private readonly IRepository<NetworkMessage> _networkMessageRepository;
    private readonly IRepository<BatteryMeasurement> _batteryRepository;
    private readonly IRepository<DepthMeasurement> _depthRepository;
    private readonly IRepository<WindMeasurement> _windRepository;
    private readonly IRepository<MotionMeasurement> _motionRepository;
    private readonly IRepository<PositionMeasurement> _positionRepository;
    private readonly IRepository<HeadingMeasurement> _headingRepository;
    private readonly IRepository<SpeedThroughWaterMeasurement> _speedThroughWaterRepository;
    private readonly IRepository<WaterTemperatureMeasurement> _waterTemperatureRepository;

    public AnalysisService(
        IRepository<NetworkMessage> networkMessageRepository,
        IRepository<BatteryMeasurement> batteryRepository,
        IRepository<DepthMeasurement> depthRepository,
        IRepository<WindMeasurement> windRepository,
        IRepository<MotionMeasurement> motionRepository,
        IRepository<PositionMeasurement> positionRepository,
        IRepository<HeadingMeasurement> headingRepository,
        IRepository<SpeedThroughWaterMeasurement> speedThroughWaterRepository,
        IRepository<WaterTemperatureMeasurement> waterTemperatureRepository)
    {
        _networkMessageRepository = networkMessageRepository;
        _batteryRepository = batteryRepository;
        _depthRepository = depthRepository;
        _windRepository = windRepository;
        _motionRepository = motionRepository;
        _positionRepository = positionRepository;
        _headingRepository = headingRepository;
        _speedThroughWaterRepository = speedThroughWaterRepository;
        _waterTemperatureRepository = waterTemperatureRepository;
    }

    public async Task<AnalysisSummaryDto> GetAnalysisSummaryAsync(AnalysisTimeWindowDto timeWindow, CancellationToken ct = default)
    {
        var networkMessageCount = await _networkMessageRepository.CountAsync(
            m => m.ReceivedAtUtc >= timeWindow.StartUtc && m.ReceivedAtUtc <= timeWindow.EndUtc,
            ct);

        var measurementCounts = new List<MeasurementCountDto>();

        var batteryCount = await _batteryRepository.CountAsync(
            m => m.RecordedAtUtc >= timeWindow.StartUtc && m.RecordedAtUtc <= timeWindow.EndUtc,
            ct);
        if (batteryCount > 0)
            measurementCounts.Add(new MeasurementCountDto { MeasurementType = "Battery", Count = batteryCount });

        var depthCount = await _depthRepository.CountAsync(
            m => m.RecordedAtUtc >= timeWindow.StartUtc && m.RecordedAtUtc <= timeWindow.EndUtc,
            ct);
        if (depthCount > 0)
            measurementCounts.Add(new MeasurementCountDto { MeasurementType = "Depth", Count = depthCount });

        var windCount = await _windRepository.CountAsync(
            m => m.RecordedAtUtc >= timeWindow.StartUtc && m.RecordedAtUtc <= timeWindow.EndUtc,
            ct);
        if (windCount > 0)
            measurementCounts.Add(new MeasurementCountDto { MeasurementType = "Wind", Count = windCount });

        var motionCount = await _motionRepository.CountAsync(
            m => m.RecordedAtUtc >= timeWindow.StartUtc && m.RecordedAtUtc <= timeWindow.EndUtc,
            ct);
        if (motionCount > 0)
            measurementCounts.Add(new MeasurementCountDto { MeasurementType = "Motion", Count = motionCount });

        var positionCount = await _positionRepository.CountAsync(
            m => m.RecordedAtUtc >= timeWindow.StartUtc && m.RecordedAtUtc <= timeWindow.EndUtc,
            ct);
        if (positionCount > 0)
            measurementCounts.Add(new MeasurementCountDto { MeasurementType = "Position", Count = positionCount });

        var headingCount = await _headingRepository.CountAsync(
            m => m.RecordedAtUtc >= timeWindow.StartUtc && m.RecordedAtUtc <= timeWindow.EndUtc,
            ct);
        if (headingCount > 0)
            measurementCounts.Add(new MeasurementCountDto { MeasurementType = "Heading", Count = headingCount });

        var speedThroughWaterCount = await _speedThroughWaterRepository.CountAsync(
            m => m.RecordedAtUtc >= timeWindow.StartUtc && m.RecordedAtUtc <= timeWindow.EndUtc,
            ct);
        if (speedThroughWaterCount > 0)
            measurementCounts.Add(new MeasurementCountDto { MeasurementType = "Speed Through Water", Count = speedThroughWaterCount });

        var waterTemperatureCount = await _waterTemperatureRepository.CountAsync(
            m => m.RecordedAtUtc >= timeWindow.StartUtc && m.RecordedAtUtc <= timeWindow.EndUtc,
            ct);
        if (waterTemperatureCount > 0)
            measurementCounts.Add(new MeasurementCountDto { MeasurementType = "Water Temperature", Count = waterTemperatureCount });

        return new AnalysisSummaryDto
        {
            StartUtc = timeWindow.StartUtc,
            EndUtc = timeWindow.EndUtc,
            TotalNetworkMessages = networkMessageCount,
            MeasurementCounts = measurementCounts
        };
    }

    public string ExportAsCSV(AnalysisSummaryDto summary)
    {
        var lines = new List<string>();
        lines.Add("Analysis Summary");
        lines.Add("");
        lines.Add($"Time Window,{summary.StartUtc:O},{summary.EndUtc:O}");
        lines.Add($"Total Network Messages,{summary.TotalNetworkMessages}");
        lines.Add("");
        lines.Add("Measurement Counts");
        lines.Add("Type,Count");

        foreach (var mc in summary.MeasurementCounts)
        {
            lines.Add($"{mc.MeasurementType},{mc.Count}");
        }

        lines.Add("");
        lines.Add("Notes");
        lines.Add($"\"{summary.WarningErrorsStatus}\"");

        return string.Join(Environment.NewLine, lines);
    }

    public string ExportAsJSON(AnalysisSummaryDto summary)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(summary, options);
    }
}
