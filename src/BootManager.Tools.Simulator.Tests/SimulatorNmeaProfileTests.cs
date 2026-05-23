using System;
using System.Collections.Generic;
using System.Linq;
using BootManager.Tools.Simulator.Models;
using BootManager.Tools.Simulator.NMEA0183;
using BootManager.Tools.Simulator.NMEA0183.Yden03;
using BootManager.Tools.Simulator.Options;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace BootManager.Tools.Simulator.Tests;

public class SimulatorNmeaProfileTests
{
    [Fact]
    public void Yden03Generator_produces_yden_and_ais_sentences()
    {
        var state = CreateState();

        var sentences = Nmea0183Yden03Generator.BuildSentences(state, includeNegative: false).ToList();

        Assert.Contains(sentences, line => line.StartsWith("$YD", StringComparison.Ordinal));
        Assert.Contains(sentences, line => line.StartsWith("!AIVDM", StringComparison.Ordinal) || line.StartsWith("!AIVDO", StringComparison.Ordinal));

        foreach (var line in sentences)
        {
            Assert.True(HasValidChecksum(line), $"Invalid checksum for {line}");
        }
    }

    [Fact]
    public void DefaultBuilder_uses_invariant_decimal_separator_for_position_sentences()
    {
        var state = CreateState();

        var rmc = Nmea0183SentenceBuilder.BuildRmc(state);
        var gga = Nmea0183SentenceBuilder.BuildGga(state);

        Assert.Contains("5236.", rmc);
        Assert.Contains("00518.", rmc);
        Assert.Contains("5236.", gga);
        Assert.Contains("00518.", gga);
        Assert.DoesNotContain("5236,", rmc);
        Assert.DoesNotContain("00518,", rmc);
        Assert.DoesNotContain("5236,", gga);
        Assert.DoesNotContain("00518,", gga);
        Assert.True(HasValidChecksum(rmc), $"Invalid checksum for {rmc}");
        Assert.True(HasValidChecksum(gga), $"Invalid checksum for {gga}");
    }

    [Fact]
    public void Binding_from_configuration_sets_Nmea0183Profile_to_YDEN03()
    {
        var inMemory = new Dictionary<string, string?>
        {
            ["Simulator:Nmea0183Profile"] = "YDEN03"
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemory)
            .Build();

        var options = new SimulatorOptions();
        config.GetSection("Simulator").Bind(options);

        Assert.Equal(Nmea0183Profile.YDEN03, options.Nmea0183Profile);
    }

    private static BoatState CreateState()
        => new()
        {
            TimestampUtc = DateTime.UtcNow,
            Latitude = 52.6,
            Longitude = 5.3,
            SogKnots = 4.0,
            CogDegrees = 90.0,
            HeadingDegrees = 85.0,
            WindSpeedMps = 5.0,
            WindAngleDeg = 45.0,
            DepthMeters = 3.5,
            WaterTemperatureCelsius = 12.3
        };

    private static bool HasValidChecksum(string line)
    {
        var starIndex = line.IndexOf('*', StringComparison.Ordinal);
        if (line.Length < 4 || starIndex < 1 || starIndex + 3 != line.Length)
            return false;

        var body = line[1..starIndex];
        var expected = line[(starIndex + 1)..];
        return string.Equals(Nmea0183SentenceBuilder.CalculateChecksum(body), expected, StringComparison.OrdinalIgnoreCase);
    }
}
