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
    public void Yden03Generator_produces_fluid_level_gateway_sentences()
    {
        var state = CreateState();

        var sentences = Nmea0183Yden03Generator.BuildSentences(state, includeNegative: false).ToList();

        // Should have PCDIN and MXPGN gateway sentences for Fluid Level (PGN 01F211)
        var pcdinSentences = sentences.Where(line => line.StartsWith("$PCDIN,01F211", StringComparison.Ordinal)).ToList();
        var mxpgnSentences = sentences.Where(line => line.StartsWith("$MXPGN,01F211", StringComparison.Ordinal)).ToList();

        Assert.NotEmpty(pcdinSentences);
        Assert.NotEmpty(mxpgnSentences);

        // Verify structure: $PCDIN,01F211,fields...,PAYLOAD*CS where PAYLOAD is 16 hex chars (8 bytes)
        foreach (var sentence in pcdinSentences.Concat(mxpgnSentences))
        {
            Assert.True(HasValidChecksum(sentence), $"Invalid checksum for {sentence}");

            var starIndex = sentence.IndexOf('*', StringComparison.Ordinal);
            var bodyPart = sentence[1..starIndex]; // Remove $ and *CS
            var fields = bodyPart.Split(',');

            // Find payload field (should be 16 hex chars)
            var payloadField = fields.LastOrDefault();
            Assert.NotNull(payloadField);
            Assert.Equal(16, payloadField!.Length); // Must be 16 hex chars (8 bytes)
            Assert.True(IsValidHexString(payloadField), $"Payload '{payloadField}' is not valid hex");
        }
    }

    [Fact]
    public void Yden03Generator_produces_comprehensive_fluid_level_coverage()
    {
        var state = CreateState();

        var sentences = Nmea0183Yden03Generator.BuildSentences(state, includeNegative: false).ToList();

        // Extract all PCDIN/MXPGN fluid level sentences
        var fluidLevelSentences = sentences
            .Where(line => (line.StartsWith("$PCDIN,01F211", StringComparison.Ordinal) ||
                           line.StartsWith("$MXPGN,01F211", StringComparison.Ordinal)))
            .ToList();

        // Extract payloads (last field before checksum)
        var payloads = new List<string>();
        foreach (var sentence in fluidLevelSentences)
        {
            var starIndex = sentence.IndexOf('*', StringComparison.Ordinal);
            var bodyPart = sentence[1..starIndex];
            var fields = bodyPart.Split(',');
            var payloadField = fields.LastOrDefault();
            if (payloadField?.Length == 16)
            {
                payloads.Add(payloadField);
            }
        }

        Assert.NotEmpty(payloads);

        // Extract fluid types and instances from payloads (first byte: high nibble = type, low nibble = instance)
        var fluidInstanceTypes = new List<(byte FluidType, byte Instance)>();
        foreach (var payload in payloads)
        {
            if (byte.TryParse(payload[0..2], System.Globalization.NumberStyles.HexNumber, null, out var byte0))
            {
                byte fluidType = (byte)((byte0 >> 4) & 0x0F);
                byte instance = (byte)(byte0 & 0x0F);
                fluidInstanceTypes.Add((fluidType, instance));
            }
        }

        // Verify fuel instances (type 0): must have at least 2 instances
        var fuelInstances = fluidInstanceTypes.Where(x => x.FluidType == 0).Select(x => x.Instance).Distinct().ToList();
        Assert.True(fuelInstances.Count >= 2, $"Expected at least 2 fuel instances, got {fuelInstances.Count}");

        // Verify fresh water instances (type 1): must have at least 2 instances
        var freshWaterInstances = fluidInstanceTypes.Where(x => x.FluidType == 1).Select(x => x.Instance).Distinct().ToList();
        Assert.True(freshWaterInstances.Count >= 2, $"Expected at least 2 fresh water instances, got {freshWaterInstances.Count}");

        // Verify other types (gray water type 2, oil type 5): must have at least 1 entry
        var otherTypes = fluidInstanceTypes.Where(x => x.FluidType == 2 || x.FluidType == 5).ToList();
        Assert.NotEmpty(otherTypes);

        // Verify invalid level: must have at least one payload with level bytes 0x7FFF (FF7F in little-endian)
        var invalidLevelPayloads = payloads.Where(p => p.Substring(2, 4).Equals("FF7F", StringComparison.OrdinalIgnoreCase)).ToList();
        Assert.NotEmpty(invalidLevelPayloads);
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

    private static bool IsValidHexString(string value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.All(c => "0123456789ABCDEFabcdef".Contains(c));
    }
}
