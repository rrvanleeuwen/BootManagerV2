using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BootManager.Tools.Simulator.Models;
using BootManager.Tools.Simulator.NMEA0183;
using BootManager.Tools.Simulator.Options;
using BootManager.Tools.Simulator.Scenarios;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BootManager.Tools.Simulator.Services;

/// <summary>
/// Simuleert een boot en verzendt periodiek NMEA 0183 sentences via UDP.
/// 
/// De service verstuurt per tick de volgende sentence-types (fase 3a-3c):
/// - VHW  → SpeedThroughWaterMeasurement
/// - MTW  → WaterTemperatureMeasurement
/// - DBT  → DepthMeasurement
/// - MWV  → WindMeasurement
/// - HDT  → HeadingMeasurement
/// - RMC  → PositionMeasurement + MotionMeasurement
/// - GGA  → PositionMeasurement
/// 
/// Wanneer <see cref="SimulatorOptions.IncludeNegativeTestSentences"/> is ingeschakeld,
/// worden ook negatieve testvarianten gestuurd die raw opslag triggeren maar geen measurement opleveren.
/// </summary>
public class Nmea0183SimulationService : BackgroundService
{
    private readonly SimulatorOptions _options;
    private readonly UdpClient _udpClient;

    private BoatState _state;
    private readonly Random _rand = new();

    /// <summary>
    /// Maakt een nieuwe instantie van <see cref="Nmea0183SimulationService"/>.
    /// </summary>
    /// <param name="options">Configuratieopties voor de simulator.</param>
    public Nmea0183SimulationService(IOptions<SimulatorOptions> options)
    {
        _options = options.Value;
        _udpClient = new UdpClient();

        var loader = new ScenarioLoader();
        var scenario = loader.LoadByName(_options.Scenario)
            ?? loader.LoadAll(_options.ScenarioPath ?? "Scenarios").FirstOrDefault()
            ?? throw new InvalidOperationException($"Scenario '{_options.Scenario}' niet gevonden.");

        _state = new BoatState
        {
            TimestampUtc = DateTime.UtcNow,
            Latitude = scenario.StartLatitude,
            Longitude = scenario.StartLongitude,
            SogKnots = scenario.StartSogKnots,
            CogDegrees = scenario.StartCogDegrees,
            HeadingDegrees = scenario.StartHeadingDegrees,
            WindSpeedMps = scenario.StartWindSpeedMps,
            WindAngleDeg = scenario.StartWindAngleDeg,
            DepthMeters = scenario.StartDepthMeters,
            BatteryVoltage = scenario.StartBatteryVoltage,
            BatterySoc = scenario.StartBatterySoc,
            SpeedThroughWaterKnots = scenario.StartSpeedThroughWaterKnots,
            WaterTemperatureCelsius = scenario.StartWaterTemperatureCelsius
        };
    }

    /// <summary>
    /// Achtergrondtaak die periodiek de toestand bijwerkt en NMEA 0183 sentences verstuurt.
    /// </summary>
    /// <param name="stoppingToken">Token dat aangeeft wanneer de service moet stoppen.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var endPoint = new IPEndPoint(IPAddress.Parse(_options.Nmea0183TargetIp), _options.Nmea0183TargetPort);

        Console.WriteLine($"[NMEA0183] Simulator gestart: Scenario={_options.Scenario} " +
                          $"Target={_options.Nmea0183TargetIp}:{_options.Nmea0183TargetPort} " +
                          $"IntervalMs={_options.IntervalMs} " +
                          $"IncludeNegative={_options.IncludeNegativeTestSentences}");
        Console.WriteLine("[NMEA0183] Sentence-types: VHW, MTW, DBT, MWV, HDT, RMC, GGA" +
                          (_options.IncludeNegativeTestSentences ? " + negatieve varianten (MWV-V, RMC-V, GGA-fix0, VHW-badcs)" : string.Empty));
        if (_options.OutputMode == SimulatorOutputMode.Both)
            Console.WriteLine("[NMEA0183] Let op: OutputMode=Both – NMEA2000 en NMEA0183 lopen elk met eigen runtime state en tick. Waarden zijn scenario-consistent, maar niet exact tick-gesynchroniseerd.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var before = DateTime.UtcNow;
            UpdateState(_options.IntervalMs);

            var sentences = BuildNmea0183Sentences(_state);

            foreach (var sentence in sentences)
            {
                var bytes = Encoding.ASCII.GetBytes(sentence + "\r\n");
                try
                {
                    await _udpClient.SendAsync(bytes, bytes.Length, endPoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[NMEA0183] UDP send fout: {ex.Message}");
                }
                Console.WriteLine($"[NMEA0183] {sentence}");
            }

            var elapsed = DateTime.UtcNow - before;
            var delay = _options.IntervalMs - (int)elapsed.TotalMilliseconds;
            if (delay > 0)
            {
                try { await Task.Delay(delay, stoppingToken); } catch (TaskCanceledException) { }
            }
        }
    }

    /// <summary>
    /// Bouwt de lijst van te verzenden NMEA 0183 sentences voor de huidige toestand.
    /// Positieve sentences altijd; negatieve alleen als <see cref="SimulatorOptions.IncludeNegativeTestSentences"/> is ingeschakeld.
    /// </summary>
    private IEnumerable<string> BuildNmea0183Sentences(BoatState s)
    {
        var sentences = new List<string>
        {
            // Fase 3a
            Nmea0183SentenceBuilder.BuildVhw(s),
            Nmea0183SentenceBuilder.BuildMtw(s),
            Nmea0183SentenceBuilder.BuildDbt(s),
            // Fase 3b
            Nmea0183SentenceBuilder.BuildMwv(s),
            Nmea0183SentenceBuilder.BuildHdt(s),
            // Fase 3c
            Nmea0183SentenceBuilder.BuildRmc(s),
            Nmea0183SentenceBuilder.BuildGga(s),
        };

        if (_options.IncludeNegativeTestSentences)
        {
            sentences.Add(Nmea0183SentenceBuilder.BuildMwvStatusV(s));
            sentences.Add(Nmea0183SentenceBuilder.BuildRmcStatusV(s));
            sentences.Add(Nmea0183SentenceBuilder.BuildGgaNoFix(s));
            sentences.Add(Nmea0183SentenceBuilder.BuildVhwBadChecksum(s));
        }

        return sentences;
    }

    /// <summary>
    /// Voert één simulatiestap uit: kleine variaties toepassen en positie verplaatsen.
    /// </summary>
    private void UpdateState(int intervalMs)
    {
        var dt = intervalMs / 1000.0;

        _state.TimestampUtc = DateTime.UtcNow;
        _state.SogKnots += (_rand.NextDouble() - 0.5) * 0.2;
        _state.SogKnots = Math.Clamp(_state.SogKnots, 2.0, 8.0);

        _state.CogDegrees += (_rand.NextDouble() - 0.5) * 1.5;
        _state.HeadingDegrees += (_rand.NextDouble() - 0.5) * 2.0;
        _state.CogDegrees = NormalizeAngle360(_state.CogDegrees);
        _state.HeadingDegrees = NormalizeAngle360(_state.HeadingDegrees);

        _state.WindSpeedMps += (_rand.NextDouble() - 0.5) * 0.3;
        _state.WindSpeedMps = Math.Max(0.1, _state.WindSpeedMps);
        _state.WindAngleDeg += (_rand.NextDouble() - 0.5) * 5.0;
        _state.WindAngleDeg = NormalizeAngle180(_state.WindAngleDeg);

        _state.DepthMeters += (_rand.NextDouble() - 0.5) * 0.05;
        _state.DepthMeters = Math.Clamp(_state.DepthMeters, 2.0, 8.0);

        _state.SpeedThroughWaterKnots = _state.SogKnots + (_rand.NextDouble() - 0.5) * 0.3;
        _state.SpeedThroughWaterKnots = Math.Clamp(_state.SpeedThroughWaterKnots, 1.5, 9.0);

        _state.WaterTemperatureCelsius += (_rand.NextDouble() - 0.5) * 0.02;
        _state.WaterTemperatureCelsius = Math.Clamp(_state.WaterTemperatureCelsius, 5.0, 30.0);

        var sogMps = _state.SogKnots * 0.514444;
        var distanceMeters = sogMps * dt;
        var bearingRad = _state.CogDegrees * Math.PI / 180.0;
        var earthRadius = 6371000.0;
        var deltaLat = (distanceMeters * Math.Cos(bearingRad)) / earthRadius;
        var deltaLon = (distanceMeters * Math.Sin(bearingRad)) / (earthRadius * Math.Cos(_state.Latitude * Math.PI / 180.0));
        _state.Latitude += deltaLat * 180.0 / Math.PI;
        _state.Longitude += deltaLon * 180.0 / Math.PI;
    }

    private static double NormalizeAngle360(double angle)
    {
        var a = angle % 360.0;
        if (a < 0) a += 360.0;
        return a;
    }

    private static double NormalizeAngle180(double angle)
    {
        var a = angle % 360.0;
        if (a <= -180.0) a += 360.0;
        if (a > 180.0) a -= 360.0;
        return a;
    }

    /// <summary>
    /// Ruimt bronnen op wanneer de service wordt verwijderd.
    /// </summary>
    public override void Dispose()
    {
        _udpClient.Dispose();
        base.Dispose();
    }
}
