using BootManager.Application.Administration.Services;
using BootManager.Web.Services;
using BootManager.Web.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BootManager.UnitTests.SystemShutdown;

public class ShutdownServiceTests
{
    private readonly Mock<ILogger<ShutdownService>> _mockLogger;
    private readonly Mock<IShutdownHelperExecutor> _mockExecutor;
    private readonly ShutdownService _developmentService;
    private readonly ShutdownService _productionService;

    public ShutdownServiceTests()
    {
        _mockLogger = new Mock<ILogger<ShutdownService>>();
        _mockExecutor = new Mock<IShutdownHelperExecutor>();

        // Create development environment mock
        var devEnvironment = new Mock<IHostEnvironment>();
        devEnvironment.SetupGet(e => e.EnvironmentName).Returns("Development");

        // Create production environment mock
        var prodEnvironment = new Mock<IHostEnvironment>();
        prodEnvironment.SetupGet(e => e.EnvironmentName).Returns("Production");

        var options = Options.Create(new ShutdownOptions
        {
            HelperSocketPath = "/run/bootmanager/shutdown.sock"
        });

        _developmentService = new ShutdownService(_mockLogger.Object, devEnvironment.Object, options, _mockExecutor.Object);
        _productionService = new ShutdownService(_mockLogger.Object, prodEnvironment.Object, options, _mockExecutor.Object);
    }

    [Fact]
    public async Task InitiateShutdownAsync_InDevelopmentMode_LogsWarningAndDoesNotExecute()
    {
        // Act
        await _developmentService.InitiateShutdownAsync(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("DEVELOPMENT MODE")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Executor must NOT be called
        _mockExecutor.Verify(
            x => x.ExecuteHelperAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task InitiateShutdownAsync_InProductionMode_CallsExecutorWithConfiguredPath()
    {
        // Arrange
        _mockExecutor
            .Setup(x => x.ExecuteHelperAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _productionService.InitiateShutdownAsync(CancellationToken.None);

        // Assert
        _mockExecutor.Verify(
            x => x.ExecuteHelperAsync("/run/bootmanager/shutdown.sock", It.IsAny<CancellationToken>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Initiating system shutdown")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task InitiateShutdownAsync_InProductionMode_WhenHelperNotAvailable_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockExecutor
            .Setup(x => x.ExecuteHelperAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Helper script not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _productionService.InitiateShutdownAsync(CancellationToken.None));

        _mockExecutor.Verify(
            x => x.ExecuteHelperAsync("/run/bootmanager/shutdown.sock", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InitiateShutdownAsync_InProductionMode_LogsSuccessfulInitiation()
    {
        // Arrange
        _mockExecutor
            .Setup(x => x.ExecuteHelperAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _productionService.InitiateShutdownAsync(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("System should shut down within 20 seconds")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InitiateShutdownAsync_WhenCancellationRequested_RespectsCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert - Should not throw
        await _productionService.InitiateShutdownAsync(cts.Token);
    }
}
