using BootManager.Tools.Ingest.Services;

namespace BootManager.UnitTests.IngestTools;

/// <summary>
/// Tests voor de pure HttpListener-prefixopbouw van de Ingest control API.
/// </summary>
public class IngestControlServerPrefixTests
{
    [Theory]
    [InlineData("0.0.0.0", 5010, "http://*:5010/")]
    [InlineData(" 0.0.0.0 ", 5010, "http://*:5010/")]
    [InlineData("127.0.0.1", 5010, "http://127.0.0.1:5010/")]
    [InlineData("localhost", 5010, "http://localhost:5010/")]
    [InlineData(null, 5010, "http://127.0.0.1:5010/")]
    [InlineData("", 5010, "http://127.0.0.1:5010/")]
    [InlineData("   ", 5010, "http://127.0.0.1:5010/")]
    public void BuildHttpListenerPrefix_ReturnsExpectedPrefix(
        string? listenAddress,
        int listenPort,
        string expectedPrefix)
    {
        var prefix = IngestControlServer.BuildHttpListenerPrefix(listenAddress, listenPort);

        Assert.Equal(expectedPrefix, prefix);
    }

    [Fact]
    public void BuildHttpListenerPrefix_WithIpv6Localhost_ReturnsValidHttpListenerPrefix()
    {
        var prefix = IngestControlServer.BuildHttpListenerPrefix("::1", 5010);

        Assert.Equal("http://[::1]:5010/", prefix);
    }
}
