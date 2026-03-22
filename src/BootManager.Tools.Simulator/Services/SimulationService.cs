using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using BootManager.Tools.Simulator.Models;
using BootManager.Tools.Simulator.Options;
using BootManager.Tools.Simulator.Scenarios;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BootManager.Tools.Simulator.Services;

/// <summary>
/// Simuleert een boot en verzendt periodiek RAW-achtige UDP-berichten met statusinformatie.
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

            var lines = BuildRawLines(_state);

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
        _state.SogKnots += ( _rand.NextDouble() - 0.5) * 0.2; // +/-0.1
        _state.SogKnots = Math.Clamp(_state.SogKnots, 2.0, 8.0);

        _state.CogDegrees += (_rand.NextDouble() - 0.5) * 1.5; // kleine drift
        _state.HeadingDegrees += (_rand.NextDouble() - 0.5) * 2.0;

        // normaliseer hoeken na wijziging zodat ze binnen verwachte grenzen blijven
        _state.CogDegrees = NormalizeAngle360(_state.CogDegrees);
        _state.HeadingDegrees = NormalizeAngle360(_state.HeadingDegrees);

        _state.WindSpeedMps += (_rand.NextDouble() - 0.5) * 0.3;
        _state.WindSpeedMps = Math.Max(0.1, _state.WindSpeedMps);
        _state.WindAngleDeg += (_rand.NextDouble() - 0.5) * 5.0;
        _state.WindAngleDeg = NormalizeAngle180(_state.WindAngleDeg);

        _state.DepthMeters += (_rand.NextDouble() - 0.5) * 0.05; // lichte variatie
        _state.DepthMeters = Math.Clamp(_state.DepthMeters, 2.0, 8.0); // realistische bodemdiepte

        _state.BatteryVoltage += (_rand.NextDouble() - 0.5) * 0.005;
        _state.BatteryVoltage = Math.Clamp(_state.BatteryVoltage, 12.0, 14.8); // realistische spanningsrange

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
    /// <param name="angle">Hoek in graden (kan negatief of groter dan 360 zijn).</param>
    /// <returns>Genormaliseerde hoek in graden tussen 0 (inclusief) en 360 (exclusief).</returns>
    private static double NormalizeAngle360(double angle)
    {
        var a = angle % 360.0;
        if (a < 0) a += 360.0;
        return a;
    }

    /// <summary>
    /// Normaliseert een hoek naar het interval (-180, 180].
    /// </summary>
    /// <param name="angle">Hoek in graden.</param>
    /// <returns>Genormaliseerde hoek in graden tussen -180 (exclusief) en 180 (inclusief).</returns>
    private static double NormalizeAngle180(double angle)
    {
        var a = angle % 360.0;
        if (a <= -180.0) a += 360.0;
        if (a > 180.0) a -= 360.0;
        return a;
    }

    /// <summary>
    /// Bouwt RAW-achtige tekstregels met hex-gecodeerde payloads voor verschillende sensoren/statussen.
    /// </summary>
    /// <param name="s">Huidige boottoestand.</param>
    /// <returns>Collectie tekstregels die verzonden worden.</returns>
    private IEnumerable<string> BuildRawLines(BoatState s)
    {
        // Compose multiple RAW-like lines with different fake ids
        // Format: HH:mm:ss.fff R <hex bytes...>

        var ts = DateTime.Now.ToString("HH:mm:ss.fff");

        // Helper to create payload bytes (very simple scaling)
        byte[] PosPayload()
        {
            var lat = (int)Math.Round(s.Latitude * 1e7); // 1e-7 deg -> int
            var lon = (int)Math.Round(s.Longitude * 1e7);
            var sog = (int)Math.Round(s.SogKnots * 100); // centi-knots
            var cog = (int)Math.Round(s.CogDegrees * 100);
            var buf = new List<byte>();
            buf.AddRange(BitConverter.GetBytes(lat));
            buf.AddRange(BitConverter.GetBytes(lon));
            buf.AddRange(BitConverter.GetBytes(sog));
            buf.AddRange(BitConverter.GetBytes(cog));
            return buf.ToArray();
        }

        byte[] MotionPayload()
        {
            var hdg = (int)Math.Round(s.HeadingDegrees * 100);
            var sog = (int)Math.Round(s.SogKnots * 100);
            var buf = new List<byte>();
            buf.AddRange(BitConverter.GetBytes(hdg));
            buf.AddRange(BitConverter.GetBytes(sog));
            return buf.ToArray();
        }

        byte[] WindPayload()
        {
            var wsp = (int)Math.Round(s.WindSpeedMps * 100);
            var wang = (int)Math.Round(s.WindAngleDeg * 100);
            var buf = new List<byte>();
            buf.AddRange(BitConverter.GetBytes(wsp));
            buf.AddRange(BitConverter.GetBytes(wang));
            return buf.ToArray();
        }

        byte[] DepthPayload()
        {
            var depth = (int)Math.Round(s.DepthMeters * 100);
            return BitConverter.GetBytes(depth);
        }

        byte[] BatteryPayload()
        {
            var volt = (int)Math.Round(s.BatteryVoltage * 1000); // mV
            var soc = (int)Math.Round(s.BatterySoc * 100); // centi%
            var buf = new List<byte>();
            buf.AddRange(BitConverter.GetBytes(volt));
            buf.AddRange(BitConverter.GetBytes(soc));
            return buf.ToArray();
        }

        string ToHex(byte[] data)
        {
            return string.Join(' ', data.Select(b => b.ToString("X2")));
        }

        // IDs chosen arbitrarily
        var posLine = $"{ts} R 0A1B2C3D " + ToHex(PosPayload());
        var motionLine = $"{ts} R 0A1B2C3E " + ToHex(MotionPayload());
        var windLine = $"{ts} R 0A1B2C3F " + ToHex(WindPayload());
        var depthLine = $"{ts} R 0A1B2C40 " + ToHex(DepthPayload());
        var battLine = $"{ts} R 0A1B2C41 " + ToHex(BatteryPayload());

        return new[] { posLine, motionLine, windLine, depthLine, battLine };
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
