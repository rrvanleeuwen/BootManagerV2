using BootManager.Core.Enums;
using BootManager.Tools.Ingest.Policies;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BootManager.UnitTests.IngestSampling;

/// <summary>
/// Tests voor <see cref="IngestSamplingPolicy"/>.
/// Valideert All/Sampled/OffAfterSuccessfulParse modes en stream key isolation.
/// </summary>
public class IngestSamplingPolicyTests
{
    private static ILogger<IngestSamplingPolicy> CreateMockLogger()
    {
        return new Mock<ILogger<IngestSamplingPolicy>>().Object;
    }

    [Fact]
    public void AllMode_ShouldAlwaysAllowMessages()
    {
        var logger = CreateMockLogger();
        var policy = new IngestSamplingPolicy(RawStorageMode.All, 10, logger);

        // Eerste bericht
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));
        // Tweede bericht direct na elkaar
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));
        // Derde bericht
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));
    }

    [Fact]
    public void SampledMode_ShouldAllowFirstMessage()
    {
        var logger = CreateMockLogger();
        var policy = new IngestSamplingPolicy(RawStorageMode.Sampled, 10, logger);

        Assert.True(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));
    }

    [Fact]
    public void SampledMode_ShouldBlockSecondMessageWithinInterval()
    {
        var logger = CreateMockLogger();
        var policy = new IngestSamplingPolicy(RawStorageMode.Sampled, 10, logger);

        // Eerste bericht doorlaten
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));

        // Tweede bericht onmiddellijk daarna moet worden geblokkeerd
        Assert.False(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));
    }

    [Fact]
    public void SampledMode_ShouldAllowMessageAfterIntervalExpires()
    {
        var logger = CreateMockLogger();
        var policy = new IngestSamplingPolicy(RawStorageMode.Sampled, 1, logger); // 1 seconde interval

        // Eerste bericht
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));

        // Tweede bericht onmiddellijk geblokkeerd
        Assert.False(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));

        // Wacht langer dan interval
        System.Threading.Thread.Sleep(1100);

        // Derde bericht na interval moet weer worden toegelaten
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));
    }

    [Fact]
    public void SampledMode_ShouldUseSeparateStreamKeysPerMessageId()
    {
        var logger = CreateMockLogger();
        var policy = new IngestSamplingPolicy(RawStorageMode.Sampled, 10, logger);

        // Eerste YDGGA (positie)
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));

        // Tweede YDGGA onmiddellijk geblokkeerd
        Assert.False(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));

        // Maar YDHDM (heading) moet doorgelaten worden (ander stream key)
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "YDHDM"));

        // En weer YDHDM moet geblokkeerd worden
        Assert.False(policy.ShouldProcessMessage("NMEA0183", "YDHDM"));
    }

    [Fact]
    public void SampledMode_ShouldUseSeparateStreamKeysPerProtocol()
    {
        var logger = CreateMockLogger();
        var policy = new IngestSamplingPolicy(RawStorageMode.Sampled, 10, logger);

        // NMEA0183 bericht
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));
        Assert.False(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));

        // NMEA2000 bericht met dezelfde MessageId moet doorgelaten worden (ander protocol)
        Assert.True(policy.ShouldProcessMessage("NMEA2000", "YDGGA"));
        Assert.False(policy.ShouldProcessMessage("NMEA2000", "YDGGA"));
    }

    [Fact]
    public void SampledMode_ShouldHandleNullOrEmptyMessageId()
    {
        var logger = CreateMockLogger();
        var policy = new IngestSamplingPolicy(RawStorageMode.Sampled, 10, logger);

        // Eerste bericht zonder MessageId
        Assert.True(policy.ShouldProcessMessage("NMEA2000", null));

        // Tweede bericht zonder MessageId moet worden geblokkeerd (zelfde stream key: "NMEA2000:Unknown")
        Assert.False(policy.ShouldProcessMessage("NMEA2000", null));

        // Maar met lege string moet ook hetzelfde zijn
        Assert.False(policy.ShouldProcessMessage("NMEA2000", ""));
    }

    [Fact]
    public void OffAfterSuccessfulParseMode_ShouldBehaveLikeSampled()
    {
        var logger = CreateMockLogger();
        var policy = new IngestSamplingPolicy(RawStorageMode.OffAfterSuccessfulParse, 10, logger);

        // OffAfterSuccessfulParse gedraagt zich zoals Sampled
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));
        Assert.False(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));
    }

    [Fact]
    public void InvalidInterval_ShouldUseDefaultFallback()
    {
        var logger = CreateMockLogger();

        // Interval <= 0 moet fallback naar 10 seconden
        var policy = new IngestSamplingPolicy(RawStorageMode.Sampled, -5, logger);

        Assert.True(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));
        Assert.False(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));

        // Met 1 seconde interval zou dit werk, dus de -5 is inderdaad vervangen door fallback
        System.Threading.Thread.Sleep(1100);
        // Nog steeds geblokkeerd (zou pas na 10 seconden doorgelaten worden)
        Assert.False(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));
    }

    [Fact]
    public void Reset_ShouldClearAllStreamKeyTiming()
    {
        var logger = CreateMockLogger();
        var policy = new IngestSamplingPolicy(RawStorageMode.Sampled, 10, logger);

        // Eerste bericht
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));

        // Tweede bericht geblokkeerd
        Assert.False(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));

        // Reset
        policy.Reset();

        // Nu moet de eerste bericht opnieuw doorgelaten worden
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));
    }

    [Fact]
    public void SampledMode_MessageIdNormalization_ShouldBeConsistent()
    {
        var logger = CreateMockLogger();
        var policy = new IngestSamplingPolicy(RawStorageMode.Sampled, 10, logger);

        // Eerste bericht met lowercase
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "ydgga"));

        // Tweede bericht met uppercase moet dezelfde stream key gebruiken en dus worden geblokkeerd
        Assert.False(policy.ShouldProcessMessage("NMEA0183", "YDGGA"));

        // En mixed case ook
        Assert.False(policy.ShouldProcessMessage("NMEA0183", "YdGgA"));
    }
}
