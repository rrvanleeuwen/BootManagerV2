using BootManager.Application.OperationalSettings.DTOs;
using BootManager.Application.OperationalSettings.Services;
using BootManager.Web.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BootManager.UnitTests.OperationalSettings;

public class OperationalSettingsWithReloadServiceTests
{
    private readonly Mock<IOperationalSettingsService> _mockSettingsService;
    private readonly Mock<IIngestControlClient> _mockIngestControlClient;
    private readonly Mock<ILogger<OperationalSettingsWithReloadService>> _mockLogger;
    private readonly OperationalSettingsWithReloadService _service;

    public OperationalSettingsWithReloadServiceTests()
    {
        _mockSettingsService = new Mock<IOperationalSettingsService>();
        _mockIngestControlClient = new Mock<IIngestControlClient>();
        _mockLogger = new Mock<ILogger<OperationalSettingsWithReloadService>>();

        _service = new OperationalSettingsWithReloadService(
            _mockSettingsService.Object,
            _mockIngestControlClient.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SaveAndReloadAsync_WhenSettingsSavedAndReloadSucceeds_ReturnsSuccessResponse()
    {
        // Arrange
        var dto = new OperationalSettingsDto
        {
            ListenAddress = "0.0.0.0",
            ListenPort = 5555,
            DefaultSampleIntervalSeconds = 20
        };

        _mockSettingsService
            .Setup(x => x.SaveAsync(It.IsAny<OperationalSettingsDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var reloadResult = new IngestControlReloadResponse
        {
            Success = true,
            Message = "Reload successful",
            AppliedFields = new List<string> { "DefaultSampleIntervalSeconds" },
            RestartRequiredFields = new List<string>()
        };

        _mockIngestControlClient
            .Setup(x => x.ReloadSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(reloadResult);

        // Act
        var response = await _service.SaveAndReloadAsync(dto);

        // Assert
        Assert.True(response.SettingsSaved);
        Assert.Equal("Instellingen opgeslagen.", response.SaveMessage);
        Assert.Equal("success", response.IngestReloadStatus);
        Assert.Equal(reloadResult.AppliedFields, response.AppliedFields);
        Assert.Equal(reloadResult.RestartRequiredFields, response.RestartRequiredFields);

        _mockSettingsService.Verify(
            x => x.SaveAsync(It.IsAny<OperationalSettingsDto>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockIngestControlClient.Verify(
            x => x.ReloadSettingsAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveAndReloadAsync_WhenIngestNotReachable_ReturnsSaveSuccessWithUnreachableStatus()
    {
        // Arrange
        var dto = new OperationalSettingsDto { ListenPort = 5555 };

        _mockSettingsService
            .Setup(x => x.SaveAsync(It.IsAny<OperationalSettingsDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockIngestControlClient
            .Setup(x => x.ReloadSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IngestControlReloadResponse?)null);

        // Act
        var response = await _service.SaveAndReloadAsync(dto);

        // Assert
        Assert.True(response.SettingsSaved);
        Assert.Equal("unreachable", response.IngestReloadStatus);
        Assert.Contains("niet bereikbaar", response.IngestReloadMessage);
    }

    [Fact]
    public async Task SaveAndReloadAsync_WhenReloadFails_ReturnsSaveSuccessWithFailedStatus()
    {
        // Arrange
        var dto = new OperationalSettingsDto { ListenPort = 5555 };

        _mockSettingsService
            .Setup(x => x.SaveAsync(It.IsAny<OperationalSettingsDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var reloadResult = new IngestControlReloadResponse
        {
            Success = false,
            Message = "Configuration error",
            AppliedFields = new List<string>(),
            RestartRequiredFields = new List<string>()
        };

        _mockIngestControlClient
            .Setup(x => x.ReloadSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(reloadResult);

        // Act
        var response = await _service.SaveAndReloadAsync(dto);

        // Assert
        Assert.True(response.SettingsSaved);
        Assert.Equal("failed", response.IngestReloadStatus);
        Assert.Contains("kon instellingen niet opnieuw laden", response.IngestReloadMessage);
    }

    [Fact]
    public async Task SaveAndReloadAsync_WhenListenAddressChanges_IncludesRestartWarning()
    {
        // Arrange
        var dto = new OperationalSettingsDto
        {
            ListenAddress = "192.168.1.100",
            ListenPort = 5555
        };

        _mockSettingsService
            .Setup(x => x.SaveAsync(It.IsAny<OperationalSettingsDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var reloadResult = new IngestControlReloadResponse
        {
            Success = true,
            Message = "Reload successful",
            AppliedFields = new List<string> { "ListenAddress" },
            RestartRequiredFields = new List<string> { "ListenAddress" }
        };

        _mockIngestControlClient
            .Setup(x => x.ReloadSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(reloadResult);

        // Act
        var response = await _service.SaveAndReloadAsync(dto);

        // Assert
        Assert.Contains("Let op:", response.IngestReloadMessage);
        Assert.Contains("Herstart Ingest/Raspberry Pi", response.IngestReloadMessage);
    }

    [Fact]
    public async Task SaveAndReloadAsync_WhenSaveFails_ThrowsException()
    {
        // Arrange
        var dto = new OperationalSettingsDto { ListenPort = -1 }; // Invalid

        _mockSettingsService
            .Setup(x => x.SaveAsync(It.IsAny<OperationalSettingsDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Port must be positive"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _service.SaveAndReloadAsync(dto));

        Assert.Contains("Port must be positive", ex.Message);

        // Ingest reload should not be attempted
        _mockIngestControlClient.Verify(
            x => x.ReloadSettingsAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
