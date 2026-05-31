using BootManager.Application.Administration.Services;
using BootManager.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace BootManager.UnitTests.SystemShutdown;

public class SystemControllerTests
{
    private readonly Mock<IShutdownService> _mockShutdownService;
    private readonly Mock<ILogger<SystemController>> _mockLogger;
    private readonly SystemController _controller;

    public SystemControllerTests()
    {
        _mockShutdownService = new Mock<IShutdownService>();
        _mockLogger = new Mock<ILogger<SystemController>>();

        _controller = new SystemController(
            _mockShutdownService.Object,
            _mockLogger.Object);
    }

    private void SetupUser(string userId = "test-user", string name = "TestOwner")
    {
        var claims = new List<Claim>
        {
            new Claim("sub", userId),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, "Owner")
        };

        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task Shutdown_WhenServiceSucceeds_ReturnsOkWithInitiatedMessage()
    {
        // Arrange
        SetupUser();
        _mockShutdownService
            .Setup(x => x.InitiateShutdownAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Shutdown(CancellationToken.None);

        // Assert
        var actionResult = Assert.IsType<ActionResult<object>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.NotNull(okResult.Value);

        // Get properties from the anonymous object
        var responseObj = okResult.Value;
        var statusProp = responseObj.GetType().GetProperty("status");
        var messageProp = responseObj.GetType().GetProperty("message");

        Assert.NotNull(statusProp);
        Assert.NotNull(messageProp);

        Assert.Equal("initiated", statusProp.GetValue(responseObj));
        var messageValue = messageProp.GetValue(responseObj);
        Assert.NotNull(messageValue);
        var message = messageValue.ToString();
        Assert.NotNull(message);
        Assert.Contains("BootManager Pi wordt afgesloten", message);
        Assert.Contains("Wacht 20 seconden", message);
    }

    [Fact]
    public async Task Shutdown_WhenShutdownServiceThrowsInvalidOperation_Returns503ServiceUnavailable()
    {
        // Arrange
        SetupUser();
        _mockShutdownService
            .Setup(x => x.InitiateShutdownAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Shutdown helper not available"));

        // Act
        var result = await _controller.Shutdown(CancellationToken.None);

        // Assert
        var actionResult = Assert.IsType<ActionResult<object>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(503, statusResult.StatusCode);
    }

    [Fact]
    public async Task Shutdown_WhenUnexpectedErrorOccurs_Returns500InternalServerError()
    {
        // Arrange
        SetupUser();
        _mockShutdownService
            .Setup(x => x.InitiateShutdownAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.Shutdown(CancellationToken.None);

        // Assert
        var actionResult = Assert.IsType<ActionResult<object>>(result);
        var statusResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task Shutdown_LogsUserInformation()
    {
        // Arrange
        SetupUser("user123", "TestAdmin");
        _mockShutdownService
            .Setup(x => x.InitiateShutdownAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.Shutdown(CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Shutdown initiated")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Shutdown_CallsShutdownServiceWithCancellationToken()
    {
        // Arrange
        SetupUser();
        var cts = new CancellationTokenSource();
        _mockShutdownService
            .Setup(x => x.InitiateShutdownAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.Shutdown(cts.Token);

        // Assert
        _mockShutdownService.Verify(
            x => x.InitiateShutdownAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
