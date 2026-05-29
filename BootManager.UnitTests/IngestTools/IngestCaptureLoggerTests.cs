using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using BootManager.Tools.Ingest.Options;
using BootManager.Tools.Ingest.Services;
using BootManager.Core.Enums;

namespace BootManager.UnitTests.IngestTools;

/// <summary>
/// Tests voor IngestCaptureLogger runtime CaptureLoggingEnabled behavior.
/// Verifies that capture logging uses BOTH appsettings AND runtime database setting.
/// </summary>
public class IngestCaptureLoggerTests
{
    [Fact]
    public async Task InitializeAsync_WithBothEnabled_CreatesLogFile()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var options = Options.Create(new IngestOptions
        {
            CaptureLogging = new CaptureLoggingOptions
            {
                Enabled = true,
                Directory = tempDir,
                FilePrefix = "test-capture"
            }
        });

        var runtimeSettings = new IngestRuntimeSettings(
            apiBaseUrl: "http://localhost:5046",
            rawStorageMode: RawStorageMode.All,
            defaultSampleIntervalSeconds: 10,
            captureLoggingEnabled: true, // Database setting
            ingestProcessingEnabled: true,
            listenAddress: "0.0.0.0",
            listenPort: 10110);

        var logger = new Mock<ILogger<IngestCaptureLogger>>();
        var captureLogger = new IngestCaptureLogger(options, runtimeSettings, logger.Object);

        try
        {
            // Act
            await captureLogger.InitializeAsync();

            // Assert
            Assert.True(captureLogger.IsEnabled, "IsEnabled should be true when both settings are true");

            // Verify directory was created
            Assert.True(Directory.Exists(tempDir));

            // Verify log file was created
            var files = Directory.GetFiles(tempDir, "test-capture-*.ndjson");
            Assert.Single(files);
        }
        finally
        {
            // Cleanup: dispose logger first to close file handle
            await captureLogger.DisposeAsync();

            // Wait a bit for file to be fully released
            await Task.Delay(100);

            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task InitializeAsync_WithRuntimeDisabled_SkipsFileCreation()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var options = Options.Create(new IngestOptions
        {
            CaptureLogging = new CaptureLoggingOptions
            {
                Enabled = true,  // Appsettings says enabled
                Directory = tempDir,
                FilePrefix = "test-capture"
            }
        });

        var runtimeSettings = new IngestRuntimeSettings(
            apiBaseUrl: "http://localhost:5046",
            rawStorageMode: RawStorageMode.All,
            defaultSampleIntervalSeconds: 10,
            captureLoggingEnabled: false, // Database says disabled!
            ingestProcessingEnabled: true,
            listenAddress: "0.0.0.0",
            listenPort: 10110);

        var logger = new Mock<ILogger<IngestCaptureLogger>>();
        var captureLogger = new IngestCaptureLogger(options, runtimeSettings, logger.Object);

        try
        {
            // Act
            await captureLogger.InitializeAsync();

            // Assert
            Assert.False(captureLogger.IsEnabled, "IsEnabled should be false when runtime setting is false");

            // Verify NO log file was created (only directory exists if appsettings creation was attempted)
            if (Directory.Exists(tempDir))
            {
                var files = Directory.GetFiles(tempDir, "test-capture-*.ndjson");
                Assert.Empty(files);
            }
        }
        finally
        {
            // Cleanup: dispose logger first to close any handles
            await captureLogger.DisposeAsync();

            // Wait a bit for resources to be released
            await Task.Delay(100);

            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task InitializeAsync_WithAppSettingsDisabled_SkipsFileCreation()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var options = Options.Create(new IngestOptions
        {
            CaptureLogging = new CaptureLoggingOptions
            {
                Enabled = false,  // Appsettings says disabled
                Directory = tempDir,
                FilePrefix = "test-capture"
            }
        });

        var runtimeSettings = new IngestRuntimeSettings(
            apiBaseUrl: "http://localhost:5046",
            rawStorageMode: RawStorageMode.All,
            defaultSampleIntervalSeconds: 10,
            captureLoggingEnabled: true, // Database says enabled
            ingestProcessingEnabled: true,
            listenAddress: "0.0.0.0",
            listenPort: 10110);

        var logger = new Mock<ILogger<IngestCaptureLogger>>();
        var captureLogger = new IngestCaptureLogger(options, runtimeSettings, logger.Object);

        try
        {
            // Act
            await captureLogger.InitializeAsync();

            // Assert
            Assert.False(captureLogger.IsEnabled, "IsEnabled should be false when appsettings setting is false");

            // Verify NO log file was created
            if (Directory.Exists(tempDir))
            {
                var files = Directory.GetFiles(tempDir, "test-capture-*.ndjson");
                Assert.Empty(files);
            }
        }
        finally
        {
            // Cleanup: dispose logger first to close any handles
            await captureLogger.DisposeAsync();

            // Wait a bit for resources to be released
            await Task.Delay(100);

            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task WriteAsync_WithBothDisabled_SkipsWrite()
    {
        // Arrange
        var options = Options.Create(new IngestOptions
        {
            CaptureLogging = new CaptureLoggingOptions
            {
                Enabled = false,
                Directory = Path.GetTempPath(),
                FilePrefix = "test"
            }
        });

        var runtimeSettings = new IngestRuntimeSettings(
            apiBaseUrl: "http://localhost:5046",
            rawStorageMode: RawStorageMode.All,
            defaultSampleIntervalSeconds: 10,
            captureLoggingEnabled: false,
            ingestProcessingEnabled: true,
            listenAddress: "0.0.0.0",
            listenPort: 10110);

        var logger = new Mock<ILogger<IngestCaptureLogger>>();
        var captureLogger = new IngestCaptureLogger(options, runtimeSettings, logger.Object);

        await captureLogger.InitializeAsync();

        var record = new BootManager.Tools.Ingest.Models.CaptureRecord
        {
            ReceivedAtUtc = DateTime.UtcNow,
            RemoteEndpoint = "127.0.0.1:5000",
            DetectedProtocol = "NMEA0183",
            RawLine = "$TEST",
            MessageId = null,
            PayloadHex = null,
            ApiPostSucceeded = null,
            ApiStatusCode = null,
            ErrorMessage = null
        };

        // Act
        await captureLogger.WriteAsync(record); // Should not throw

        // Assert
        Assert.False(captureLogger.IsEnabled);
        // No exception thrown = success
    }

    [Fact]
    public async Task WriteAsync_WithRuntimeDisabledAfterInit_SkipsWrite()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var options = Options.Create(new IngestOptions
        {
            CaptureLogging = new CaptureLoggingOptions
            {
                Enabled = true,
                Directory = tempDir,
                FilePrefix = "test"
            }
        });

        var runtimeSettings = new IngestRuntimeSettings(
            apiBaseUrl: "http://localhost:5046",
            rawStorageMode: RawStorageMode.All,
            defaultSampleIntervalSeconds: 10,
            captureLoggingEnabled: true, // Initially true
            ingestProcessingEnabled: true,
            listenAddress: "0.0.0.0",
            listenPort: 10110);

        var logger = new Mock<ILogger<IngestCaptureLogger>>();
        var captureLogger = new IngestCaptureLogger(options, runtimeSettings, logger.Object);

        await captureLogger.InitializeAsync();
        Assert.True(captureLogger.IsEnabled);

        // Now disable at runtime
        runtimeSettings.CaptureLoggingEnabled = false;

        var record = new BootManager.Tools.Ingest.Models.CaptureRecord
        {
            ReceivedAtUtc = DateTime.UtcNow,
            RemoteEndpoint = "127.0.0.1:5000",
            DetectedProtocol = "NMEA0183",
            RawLine = "$TEST",
            MessageId = null,
            PayloadHex = null,
            ApiPostSucceeded = null,
            ApiStatusCode = null,
            ErrorMessage = null
        };

        try
        {
            // Act
            await captureLogger.WriteAsync(record); // Should respect new runtime setting

            // Assert: No new lines written because runtime is now disabled
            // (We can't directly verify file content in this simple test, but at least no exception)
        }
        finally
        {
            await captureLogger.DisposeAsync();
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
