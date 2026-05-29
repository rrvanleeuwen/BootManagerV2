using Microsoft.Extensions.Logging;
using Moq;
using BootManager.Tools.Ingest.Services;

namespace BootManager.UnitTests.IngestTools;

/// <summary>
/// Tests voor operationele settings reload fallback logica.
/// Verifieert dat reload configured/bootstrap URL eerst probeert, runtime URL als fallback.
/// </summary>
public class IngestSettingsReloadFallbackTests
{
    [Fact]
    public async Task ReloadHelper_WithConfiguredUrlSuccess_UsesPrimaryUrl()
    {
        // Arrange
        const string configuredUrl = "http://localhost:5052";
        const string runtimeUrl = "http://localhost:5046";

        var settingsClientMock = new Mock<IOperationalSettingsClientService>();
        var remoteSettings = new IngestRemoteSettings
        {
            ListenAddress = "127.0.0.1",
            ListenPort = 10110,
            ApiBaseUrl = "http://updated:5052",
            CaptureLoggingEnabled = true,
            IngestProcessingEnabled = true,
            RawStorageMode = "All",
            DefaultSampleIntervalSeconds = 15
        };

        // Configured URL succeeds
        settingsClientMock
            .Setup(x => x.TryGetSettingsAsync(configuredUrl, default))
            .ReturnsAsync(remoteSettings);

        // Runtime URL should not be tried
        settingsClientMock
            .Setup(x => x.TryGetSettingsAsync(runtimeUrl, default))
            .ReturnsAsync((IngestRemoteSettings?)null);

        // Act
        var result = await FetchSettingsWithFallback(
            configuredUrl, runtimeUrl, settingsClientMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("http://updated:5052", result!.ApiBaseUrl);

        // Verify configured URL was tried
        settingsClientMock.Verify(
            x => x.TryGetSettingsAsync(configuredUrl, default),
            Times.Once,
            "Configured URL must be tried first");

        // Verify runtime URL was NOT tried
        settingsClientMock.Verify(
            x => x.TryGetSettingsAsync(runtimeUrl, default),
            Times.Never,
            "Runtime URL should not be tried when configured URL succeeds");
    }

    [Fact]
    public async Task ReloadHelper_WithConfiguredUrlFailsAndRuntimeSucceeds_UsesFallbackUrl()
    {
        // Arrange
        const string configuredUrl = "http://localhost:5052";
        const string runtimeUrl = "http://localhost:5046";

        var settingsClientMock = new Mock<IOperationalSettingsClientService>();
        var remoteSettings = new IngestRemoteSettings
        {
            ListenAddress = "127.0.0.1",
            ListenPort = 10110,
            ApiBaseUrl = runtimeUrl,
            CaptureLoggingEnabled = true,
            IngestProcessingEnabled = true,
            RawStorageMode = "All",
            DefaultSampleIntervalSeconds = 15
        };

        // Configured URL fails
        settingsClientMock
            .Setup(x => x.TryGetSettingsAsync(configuredUrl, default))
            .ReturnsAsync((IngestRemoteSettings?)null);

        // Runtime URL succeeds
        settingsClientMock
            .Setup(x => x.TryGetSettingsAsync(runtimeUrl, default))
            .ReturnsAsync(remoteSettings);

        // Act
        var result = await FetchSettingsWithFallback(
            configuredUrl, runtimeUrl, settingsClientMock.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(runtimeUrl, result!.ApiBaseUrl);

        // Verify configured URL was tried first
        settingsClientMock.Verify(
            x => x.TryGetSettingsAsync(configuredUrl, default),
            Times.Once,
            "Configured URL must be tried first");

        // Verify runtime URL was tried as fallback
        settingsClientMock.Verify(
            x => x.TryGetSettingsAsync(runtimeUrl, default),
            Times.Once,
            "Runtime URL must be tried as fallback when configured fails");
    }

    [Fact]
    public async Task ReloadHelper_WithSameUrlConfiguredAndRuntime_TriesOnceOnly()
    {
        // Arrange
        const string singleUrl = "http://localhost:5052";

        var settingsClientMock = new Mock<IOperationalSettingsClientService>();
        var remoteSettings = new IngestRemoteSettings
        {
            ListenAddress = "127.0.0.1",
            ListenPort = 10110,
            ApiBaseUrl = singleUrl,
            CaptureLoggingEnabled = true,
            IngestProcessingEnabled = true,
            RawStorageMode = "All",
            DefaultSampleIntervalSeconds = 15
        };

        // Success on single URL
        settingsClientMock
            .Setup(x => x.TryGetSettingsAsync(singleUrl, default))
            .ReturnsAsync(remoteSettings);

        // Act
        var result = await FetchSettingsWithFallback(
            singleUrl, singleUrl, settingsClientMock.Object);

        // Assert
        Assert.NotNull(result);

        // Verify URL was tried exactly once (no redundant fallback)
        settingsClientMock.Verify(
            x => x.TryGetSettingsAsync(singleUrl, default),
            Times.Once,
            "When configured and runtime URLs are same, only one attempt should be made");
    }

    [Fact]
    public async Task ReloadHelper_WithBothUrlsFail_ReturnsNull()
    {
        // Arrange
        const string configuredUrl = "http://localhost:5052";
        const string runtimeUrl = "http://localhost:5046";

        var settingsClientMock = new Mock<IOperationalSettingsClientService>();

        // Both fail
        settingsClientMock
            .Setup(x => x.TryGetSettingsAsync(It.IsAny<string>(), default))
            .ReturnsAsync((IngestRemoteSettings?)null);

        // Act
        var result = await FetchSettingsWithFallback(
            configuredUrl, runtimeUrl, settingsClientMock.Object);

        // Assert
        Assert.Null(result);

        // Verify both URLs were tried
        settingsClientMock.Verify(
            x => x.TryGetSettingsAsync(configuredUrl, default),
            Times.Once);

        settingsClientMock.Verify(
            x => x.TryGetSettingsAsync(runtimeUrl, default),
            Times.Once);
    }

    /// <summary>
    /// Simulates the reload-settings fetch logic from IngestControlServer.HandlePostReloadSettings().
    /// Configured/bootstrap URL is tried first, runtime URL is fallback if configured differs and fails.
    /// </summary>
    private static async Task<IngestRemoteSettings?> FetchSettingsWithFallback(
        string configuredApiBaseUrl,
        string runtimeApiBaseUrl,
        IOperationalSettingsClientService settingsClient,
        CancellationToken ct = default)
    {
        // Step 1: Try configured/bootstrap URL first (stable, recommended route)
        var newSettings = await settingsClient.TryGetSettingsAsync(configuredApiBaseUrl, ct);

        // Step 2: If configured fails and differs from runtime, try runtime URL as fallback
        if (newSettings is null && runtimeApiBaseUrl != configuredApiBaseUrl)
        {
            newSettings = await settingsClient.TryGetSettingsAsync(runtimeApiBaseUrl, ct);
        }

        return newSettings;
    }
}
