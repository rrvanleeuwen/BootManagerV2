using System;
using BootManager.Tools.Ingest.Services;
using BootManager.Tools.Ingest.Models;
using Xunit;

namespace BootManager.UnitTests.NetworkMessageParsing;

public class IngestServiceProtocolDetectionTests
{
    [Fact]
    public void ParseNetworkLine_ShouldDetect_DollarAsNmea0183()
    {
        var line = "$YDGGA,1,2,3*00";
        var res = IngestService.ParseNetworkLine(line, "127.0.0.1:10110");

        Assert.Equal("NMEA0183", res.Protocol);
        Assert.Equal(line, res.RawLine);
    }

    [Fact]
    public void ParseNetworkLine_ShouldDetect_ExclamationAsNmea0183()
    {
        var line = "!AIVDM,1,1,,A,15N:;P0000oG?P@E`8bdv?vN0<1,0*5C";
        var res = IngestService.ParseNetworkLine(line, "127.0.0.1:10110");

        Assert.Equal("NMEA0183", res.Protocol);
        Assert.Equal(line, res.RawLine);
    }

    [Fact]
    public void ParseNetworkLine_ShouldDetect_RawLikeAsNmea2000()
    {
        var line = "12:34:56.789 R 0A1B2C3D AA BB CC";
        var res = IngestService.ParseNetworkLine(line, "127.0.0.1:10110");

        Assert.Equal("NMEA2000", res.Protocol);
        Assert.Equal(line, res.RawLine);
        Assert.Equal("0A1B2C3D", res.MessageId);
    }
}
