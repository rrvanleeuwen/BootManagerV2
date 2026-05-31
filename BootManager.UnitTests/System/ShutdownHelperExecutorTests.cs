using BootManager.Web.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BootManager.UnitTests.SystemShutdown;

public class ShutdownHelperExecutorTests
{
    private readonly Mock<ILogger<ShutdownHelperExecutor>> _mockLogger;
    private readonly ShutdownHelperExecutor _executor;

    public ShutdownHelperExecutorTests()
    {
        _mockLogger = new Mock<ILogger<ShutdownHelperExecutor>>();
        _executor = new ShutdownHelperExecutor(_mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteHelperAsync_WithNonExistentScript_ThrowsInvalidOperationException()
    {
        // Arrange
        var nonExistentPath = "/tmp/nonexistent-shutdown-socket-" + Guid.NewGuid() + ".sock";

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _executor.ExecuteHelperAsync(nonExistentPath));

        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteHelperAsync_WithNonReadableScript_ThrowsInvalidOperationException()
    {
        // Arrange
        var tempDir = Path.GetTempPath();
        var scriptPath = Path.Combine(tempDir, $"test-shutdown-{Guid.NewGuid()}.sh");

        try
        {
            // Create a file
            File.WriteAllText(scriptPath, "#!/bin/bash\necho 'Test'\n");

            // On Unix: remove read permissions
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                System.Runtime.InteropServices.OSPlatform.Windows))
            {
                System.Diagnostics.Process.Start("chmod", $"000 {scriptPath}").WaitForExit();

                // Act & Assert
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => _executor.ExecuteHelperAsync(scriptPath));

                Assert.Contains("not readable", ex.Message, StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            // Restore permissions for cleanup
            if (File.Exists(scriptPath))
            {
                if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                    System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    System.Diagnostics.Process.Start("chmod", $"644 {scriptPath}").WaitForExit();
                }
                File.Delete(scriptPath);
            }
        }
    }

    [Fact]
    [Trait("Category", "Linux")]
    public async Task ExecuteHelperAsync_WithValidScriptOnLinux_StartsProcess()
    {
        // This test only runs on Linux/Unix systems
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Windows))
        {
            // Skip on Windows
            return;
        }

        // Arrange
        var tempDir = Path.GetTempPath();
        var scriptPath = Path.Combine(tempDir, $"test-shutdown-{Guid.NewGuid()}.sh");

        try
        {
            // Create a simple shell script that exits quickly
            File.WriteAllText(scriptPath, "#!/bin/bash\nexit 0\n");

            // Make it executable
            System.Diagnostics.Process.Start("chmod", $"+x {scriptPath}").WaitForExit();

            // Act
            await _executor.ExecuteHelperAsync(scriptPath);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Shutdown helper script started")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            if (File.Exists(scriptPath))
            {
                File.Delete(scriptPath);
            }
        }
    }
}
