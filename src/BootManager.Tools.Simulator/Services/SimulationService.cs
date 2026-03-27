using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BootManager.Tools.Simulator.Models;
using BootManager.Tools.Simulator.Options;
using BootManager.Tools.Simulator.Scenarios;
using BootManager.Tools.Simulator.NMEA2000;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BootManager.Tools.Simulator.Services;

/// <summary>
/// Simuleert een boot en verzendt periodiek RAW-achtige UDP-berichten met NMEA 2000-achtige payloads.
/// 
/// Dit is een simulatie en benadering van NMEA 2000-semantiek, geen volledige gecertificeerde implementatie.
/// 
/// Format per regel:
///   HH:mm:ss.fff R [PGN_HEX_6CHARS] [PAYLOAD_HEX...]
/// 
/// Voorbeeld:
///   14:23:45.123 R 01F801 7E 18 4A 62 5C D0 4F 4C
/// 
/// waarbij 01F801 hexadecimaal PGN 129025 (Position) voorstelt.
/// </summary>
public class SimulationService : BackgroundService
{
    private readonly SimulatorOptions _options;
    private readonly ScenarioLoader _loader;
    private readonly UdpClient _udpClient;

    private BoatState _state;
    private readonly Random _rand = new();

    /// <summary>
    /// Maakt een nieuwe instantie van de simulator service en initialiseert de starttoestand vanuit een scenario.
    /// </summary>
    /// <param name="options">Gebruikersconfiguratie voor de simulator.</param>
    public SimulationService(IOptions<SimulatorOptions> options)
    {
        _options = options.Value;
        _loader = new ScenarioLoader();
        _udpClient = new UdpClient();

        var scenario = _loader.LoadByName(_options.Scenario);
        if (scenario == null)
        {
            scenario = _loader.LoadAll(_options.ScenarioPath ?? "Scenarios").FirstOrDefault();
        }

        if (scenario == null)
        {
            var path = _options.ScenarioPath ?? "Scenarios";
            throw new InvalidOperationException($"No scenario found. Searched by name '{_options.Scenario}' and in path '{path}'.");
        }

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
            BatterySoc = scenario.StartBatterySoc
        };
    }

    /// <summary>
    /// Achtergrondtaak die periodiek de toestand bijwerkt en UDP-berichten verstuurt.
    /// </summary>
    /// <param name="stoppingToken">Token dat aangeeft wanneer de service moet stoppen.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine($"Simulation starting: Scenario={_options.Scenario} Target={_options.TargetIp}:{_options.TargetPort} IntervalMs={_options.IntervalMs}");

        var endPoint = new IPEndPoint(IPAddress.Parse(_options.TargetIp), _options.TargetPort);

        while (!stoppingToken.IsCancellationRequested)
        {
            var before = DateTime.UtcNow;
            UpdateState(_options.IntervalMs);

            var lines = BuildRawNMEA2000Lines(_state);

            foreach (var line in lines)
            {
                var bytes = System.Text.Encoding.ASCII.GetBytes(line + "\r\n");
                try
                {
                    await _udpClient.SendAsync(bytes, bytes.Length, endPoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UDP send error: {ex.Message}");
                }
                Console.WriteLine(line);
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
    /// Voert één simulatiestap uit: kleine variaties toepassen en positie verplaatsen.
    /// </summary>
    /// <param name="intervalMs">Interval van de tick in milliseconden.</param>
    private void UpdateState(int intervalMs)
    {
        var dt = intervalMs / 1000.0; // seconds

        // kleine natuurlijke variaties
        _state.TimestampUtc = DateTime.UtcNow;
        _state.SogKnots += (_rand.NextDouble() - 0.5) * 0.2;
        _state.SogKnots = Math.Clamp(_state.SogKnots, 2.0, 8.0);

        _state.CogDegrees += (_rand.NextDouble() - 0.5) * 1.5;
        _state.HeadingDegrees += (_rand.NextDouble() - 0.5) * 2.0;

        // normaliseer hoeken na wijziging zodat ze binnen verwachte grenzen blijven
        _state.CogDegrees = NormalizeAngle360(_state.CogDegrees);
        _state.HeadingDegrees = NormalizeAngle360(_state.HeadingDegrees);

        _state.WindSpeedMps += (_rand.NextDouble() - 0.5) * 0.3;
        _state.WindSpeedMps = Math.Max(0.1, _state.WindSpeedMps);
        _state.WindAngleDeg += (_rand.NextDouble() - 0.5) * 5.0;
        _state.WindAngleDeg = NormalizeAngle180(_state.WindAngleDeg);

        _state.DepthMeters += (_rand.NextDouble() - 0.5) * 0.05;
        _state.DepthMeters = Math.Clamp(_state.DepthMeters, 2.0, 8.0);

        _state.BatteryVoltage += (_rand.NextDouble() - 0.5) * 0.005;
        _state.BatteryVoltage = Math.Clamp(_state.BatteryVoltage, 12.0, 14.8);

        _state.BatterySoc += (_rand.NextDouble() - 0.5) * 0.02;
        _state.BatterySoc = Math.Clamp(_state.BatterySoc, 0.0, 100.0);

        // Verplaatsing op basis van SOG en COG (benadering)
        // Gebruik nautische koers: 0° = noord, 90° = oost.
        var sogMps = _state.SogKnots * 0.514444; // knots to m/s
        var distanceMeters = sogMps * dt;
        var bearingRad = _state.CogDegrees * Math.PI / 180.0; // radialen vanaf noord, met de klok mee

        // equirectangular benadering geschikt voor kleine verplaatsingen
        var earthRadius = 6371000.0;
        var deltaLat = (distanceMeters * Math.Cos(bearingRad)) / earthRadius; // noord-zuid component
        var deltaLon = (distanceMeters * Math.Sin(bearingRad)) / (earthRadius * Math.Cos(_state.Latitude * Math.PI / 180.0)); // oost-west component gecorrigeerd voor breedte

        _state.Latitude += deltaLat * 180.0 / Math.PI;
        _state.Longitude += deltaLon * 180.0 / Math.PI;
    }

    /// <summary>
    /// Normaliseert een hoek naar het interval [0, 360).
    /// </summary>
    private static double NormalizeAngle360(double angle)
    {
        var a = angle % 360.0;
        if (a < 0) a += 360.0;
        return a;
    }

    /// <summary>
    /// Normaliseert een hoek naar het interval (-180, 180].
    /// </summary>
    private static double NormalizeAngle180(double angle)
    {
        var a = angle % 360.0;
        if (a <= -180.0) a += 360.0;
        if (a > 180.0) a -= 360.0;
        return a;
    }

    /// <summary>
    /// Bouwt RAW-achtige tekstregels met NMEA 2000-achtige PGN's (in hexadecimale notatie) en payloads.
    /// 
    /// Format: HH:mm:ss.fff R [PGN_HEX] [PAYLOAD_HEX...]
    /// 
    /// Retourneert 6 regels (Position, COG/SOG, Heading, Wind, Depth, Battery).
    /// Dit is een simulatie van NMEA 2000-semantiek, geen volledige gecertificeerde implementatie.
    /// </summary>
    private IEnumerable<string> BuildRawNMEA2000Lines(BoatState s)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss.fff");

        var positionPayload = NMEA2000PayloadBuilder.BuildPositionPayload(s);
        var cogSogPayload = NMEA2000PayloadBuilder.BuildCogSogPayload(s);
        var headingPayload = NMEA2000PayloadBuilder.BuildHeadingPayload(s);
        var windPayload = NMEA2000PayloadBuilder.BuildWindPayload(s);
        var depthPayload = NMEA2000PayloadBuilder.BuildDepthPayload(s);
        var batteryPayload = NMEA2000PayloadBuilder.BuildBatteryPayload(s);

        var pgns = new[]
        {
            (NMEA2000PgnSpecification.PGN_POSITION, positionPayload),
            (NMEA2000PgnSpecification.PGN_COG_SOG, cogSogPayload),
            (NMEA2000PgnSpecification.PGN_HEADING, headingPayload),
            (NMEA2000PgnSpecification.PGN_WIND, windPayload),
            (NMEA2000PgnSpecification.PGN_DEPTH, depthPayload),
            (NMEA2000PgnSpecification.PGN_BATTERY, batteryPayload)
        };

        var lines = new List<string>();
        foreach (var (pgn, payload) in pgns)
        {
            var pgnHex = pgn.ToString("X").PadLeft(6, '0');
            var payloadHex = NMEA2000PayloadBuilder.BytesToHexString(payload);
            var line = $"{ts} R {pgnHex} {payloadHex}";
            lines.Add(line);
        }

        return lines;
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
