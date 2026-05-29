using Xunit;
using BootManager.Tools.Ingest.Services;
using BootManager.Core.Enums;
using BootManager.Tools.Ingest.Policies;
using Microsoft.Extensions.Logging;
using Moq;

namespace BootManager.UnitTests.IngestTools;

/// <summary>
/// Tests voor runtime settings en live updates van ingest-instellingen.
/// </summary>
public class IngestRuntimeSettingsTests
{
    [Fact]
    public void Constructor_InitializesPropertiesCorrectly()
    {
        // Arrange & Act
        var settings = new IngestRuntimeSettings(
            "http://api.example.com",
            RawStorageMode.Sampled,
            15,
            true,
            true,
            "192.168.1.1",
            2000);

        // Assert
        Assert.Equal("http://api.example.com", settings.ApiBaseUrl);
        Assert.Equal(RawStorageMode.Sampled, settings.RawStorageMode);
        Assert.Equal(15, settings.DefaultSampleIntervalSeconds);
        Assert.True(settings.CaptureLoggingEnabled);
        Assert.Equal("192.168.1.1", settings.ListenAddress);
        Assert.Equal(2000, settings.ListenPort);
    }

    [Fact]
    public void ApiBaseUrl_CanBeUpdatedLive()
    {
        // Arrange
        var settings = new IngestRuntimeSettings(
            "http://api.example.com",
            RawStorageMode.All,
            10,
            false,
            true,
            "0.0.0.0",
            10110);

        // Act
        settings.ApiBaseUrl = "http://api.newhost.com";

        // Assert
        Assert.Equal("http://api.newhost.com", settings.ApiBaseUrl);
    }

    [Fact]
    public void RawStorageMode_CanBeUpdatedLive()
    {
        // Arrange
        var settings = new IngestRuntimeSettings(
            "http://localhost:5046",
            RawStorageMode.All,
            10,
            false,
            true,
            "0.0.0.0",
            10110);

        // Act
        settings.RawStorageMode = RawStorageMode.Sampled;

        // Assert
        Assert.Equal(RawStorageMode.Sampled, settings.RawStorageMode);
    }

    [Fact]
    public void DefaultSampleIntervalSeconds_CanBeUpdatedLive()
    {
        // Arrange
        var settings = new IngestRuntimeSettings(
            "http://localhost:5046",
            RawStorageMode.Sampled,
            10,
            false,
            true,
            "0.0.0.0",
            10110);

        // Act
        settings.DefaultSampleIntervalSeconds = 30;

        // Assert
        Assert.Equal(30, settings.DefaultSampleIntervalSeconds);
    }

    [Fact]
    public void CaptureLoggingEnabled_CanBeUpdatedLive()
    {
        // Arrange
        var settings = new IngestRuntimeSettings(
            "http://localhost:5046",
            RawStorageMode.All,
            10,
            false,
            true,
            "0.0.0.0",
            10110);

        // Act
        settings.CaptureLoggingEnabled = true;

        // Assert
        Assert.True(settings.CaptureLoggingEnabled);
    }

    [Fact]
    public void ListenAddress_IsReadOnly()
    {
        // Arrange
        var settings = new IngestRuntimeSettings(
            "http://localhost:5046",
            RawStorageMode.All,
            10,
            false,
            true,
            "0.0.0.0",
            10110);

        // Act & Assert - ListenAddress is read-only property
        Assert.Equal("0.0.0.0", settings.ListenAddress);
        // Cannot set - no setter
    }

    [Fact]
    public void ListenPort_IsReadOnly()
    {
        // Arrange
        var settings = new IngestRuntimeSettings(
            "http://localhost:5046",
            RawStorageMode.All,
            10,
            false,
            true,
            "0.0.0.0",
            10110);

        // Act & Assert - ListenPort is read-only property
        Assert.Equal(10110, settings.ListenPort);
        // Cannot set - no setter
    }

    [Fact]
    public async Task PropertiesAreThreadSafe()
    {
        // Arrange
        var settings = new IngestRuntimeSettings(
            "http://localhost:5046",
            RawStorageMode.All,
            10,
            false,
            true,
            "0.0.0.0",
            10110);

        var errors = new List<Exception>();
        var tasks = new List<Task>();

        // Act - Multiple threads updating simultaneously
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 100; j++)
                    {
                        settings.ApiBaseUrl = $"http://api{j}.example.com";
                        settings.DefaultSampleIntervalSeconds = 10 + j;
                        settings.RawStorageMode = j % 2 == 0 ? RawStorageMode.All : RawStorageMode.Sampled;
                        settings.CaptureLoggingEnabled = j % 2 == 0;

                        // Read back to ensure consistency
                        var _ = settings.ApiBaseUrl;
                        var __ = settings.DefaultSampleIntervalSeconds;
                        var ___ = settings.RawStorageMode;
                        var ____ = settings.CaptureLoggingEnabled;
                    }
                }
                catch (Exception ex)
                {
                    lock (errors)
                    {
                        errors.Add(ex);
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - No exceptions should occur
        Assert.Empty(errors);
    }
}

/// <summary>
/// Tests voor live updates van de IngestSamplingPolicy.
/// </summary>
public class IngestSamplingPolicyUpdateTests
{
    private readonly ILogger<IngestSamplingPolicy> _mockLogger;

    public IngestSamplingPolicyUpdateTests()
    {
        var mockLogger = new Mock<ILogger<IngestSamplingPolicy>>();
        _mockLogger = mockLogger.Object;
    }

    [Fact]
    public void Update_ChangesRawStorageMode()
    {
        // Arrange
        var policy = new IngestSamplingPolicy(RawStorageMode.All, 10, _mockLogger);

        // Act
        policy.Update(RawStorageMode.Sampled, 10);

        // Assert - Policy should now block based on sampling
        // First message with key "NMEA0183:GGA" should pass
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "GGA"));
        // Second message immediately should be blocked
        Assert.False(policy.ShouldProcessMessage("NMEA0183", "GGA"));
    }

    [Fact]
    public void Update_ChangesInterval()
    {
        // Arrange
        var policy = new IngestSamplingPolicy(RawStorageMode.Sampled, 10, _mockLogger);

        // Act
        policy.Update(RawStorageMode.Sampled, 1); // 1-second interval

        // Assert - Should allow messages more frequently with updated interval
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "GGA")); // First message passes
        System.Threading.Thread.Sleep(1100); // Wait just over 1 second
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "GGA")); // Should pass after 1 second
    }

    [Fact]
    public void Update_ResetsStateOnModeChange()
    {
        // Arrange
        var policy = new IngestSamplingPolicy(RawStorageMode.Sampled, 100, _mockLogger);

        // Act - Set up sampled state
        policy.ShouldProcessMessage("NMEA2000", "PGN123");

        // Change mode - state should be reset
        policy.Update(RawStorageMode.All, 100);

        // Assert - Now all messages should pass (mode is All)
        Assert.True(policy.ShouldProcessMessage("NMEA2000", "PGN123"));
        Assert.True(policy.ShouldProcessMessage("NMEA2000", "PGN123")); // Second one also passes because mode is All
    }

    [Fact]
    public void Update_ValidatesInterval_FallbackTo10OnNegative()
    {
        // Arrange
        var policy = new IngestSamplingPolicy(RawStorageMode.Sampled, 10, _mockLogger);

        // Act - Update with invalid interval
        policy.Update(RawStorageMode.Sampled, -1);

        // Assert - Should use fallback interval (10 seconds)
        // First message passes
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "RMC"));
        // Second message blocked (10-second interval)
        Assert.False(policy.ShouldProcessMessage("NMEA0183", "RMC"));
    }

    [Fact]
    public void ShouldProcessMessage_RespectsModeChanges()
    {
        // Arrange
        var policy = new IngestSamplingPolicy(RawStorageMode.All, 10, _mockLogger);

        // Act & Assert - All mode: all pass
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "GGA"));
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "GGA"));

        // Switch to Sampled
        policy.Update(RawStorageMode.Sampled, 100);

        // Now second message blocks
        Assert.True(policy.ShouldProcessMessage("NMEA0183", "RMC"));
        Assert.False(policy.ShouldProcessMessage("NMEA0183", "RMC"));
    }
}
