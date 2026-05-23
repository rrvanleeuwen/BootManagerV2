using BootManager.Application.NetworkMessageParsing.Services;
using Xunit;

namespace BootManager.UnitTests.NetworkMessageParsing;

public class Nmea0183ParserServiceTests
{
    [Fact]
    public void Parse_ShouldAccept_DollarStartedSentence()
    {
        var svc = new Nmea0183ParserService(new Microsoft.Extensions.Logging.Abstractions.NullLogger<Nmea0183ParserService>());
        var res = svc.Parse("$YDGGA,1,2,3*00");

        Assert.True(res.IsSuccess);
        Assert.Equal("YD", res.TalkerPrefix);
        Assert.Equal("GGA", res.SentenceType);
    }

    [Fact]
    public void Parse_ShouldAccept_ExclamationStartedAisSentence()
    {
        var svc = new Nmea0183ParserService(new Microsoft.Extensions.Logging.Abstractions.NullLogger<Nmea0183ParserService>());
        var res = svc.Parse("!AIVDO,1,1,,A,13aIC@PP00PJ5;tN?JGf4?vf26nQ,0*57");

        Assert.True(res.IsSuccess);
        Assert.Equal("AI", res.TalkerPrefix);
        Assert.Equal("VDO", res.SentenceType);
        Assert.True(res.ChecksumValid);
    }

    [Fact]
    public void ComputeChecksum_ShouldBeSame_ForExclamationAndDollarBody()
    {
        var svc = new Nmea0183ParserService(new Microsoft.Extensions.Logging.Abstractions.NullLogger<Nmea0183ParserService>());
        var dollar = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D";
        var excl = "!GPGLL,4916.45,N,12311.12,W,225444,A,*1D";

        var res1 = svc.Parse(dollar);
        var res2 = svc.Parse(excl);

        Assert.Equal(res1.ChecksumValid, res2.ChecksumValid);
    }
}
