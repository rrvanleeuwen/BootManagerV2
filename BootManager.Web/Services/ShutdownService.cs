using BootManager.Application.Administration.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BootManager.Web.Options;

namespace BootManager.Web.Services;

/// <summary>
/// Implementatie van IShutdownService voor veilige Pi-shutdown via begrensde helper-script.
/// </summary>
public class ShutdownService : IShutdownService
{
    private readonly ILogger<ShutdownService> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IOptions<ShutdownOptions> _options;
    private readonly IShutdownHelperExecutor _executor;

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="ShutdownService"/>.
    /// </summary>
    /// <param name="logger">Logger voor diagnostische informatie.</param>
    /// <param name="environment">Host-environment (Development, Production, etc.).</param>
    /// <param name="options">Shutdown-configuratieopties.</param>
    /// <param name="executor">Helper-executor voor veilige scriptuitvoering.</param>
    public ShutdownService(
        ILogger<ShutdownService> logger,
        IHostEnvironment environment,
        IOptions<ShutdownOptions> options,
        IShutdownHelperExecutor executor)
    {
        _logger = logger;
        _environment = environment;
        _options = options;
        _executor = executor;
    }

    /// <inheritdoc />
    public async Task InitiateShutdownAsync(CancellationToken ct = default)
    {
        if (_environment.IsDevelopment())
        {
            // In development mode, only log a warning without actual shutdown
            _logger.LogWarning(
                "DEVELOPMENT MODE: Shutdown requested but not executed. " +
                "To test execution, set 'Shutdown:AllowTestExecutionInDevelopment=true' in appsettings and ensure helper script exists.");
            await Task.CompletedTask;
            return;
        }

        // In production mode (or if test execution enabled), execute the shutdown helper
        try
        {
            var helperSocketPath = _options.Value.HelperSocketPath;

            _logger.LogInformation(
                "Initiating system shutdown via helper socket: {HelperSocketPath}",
                helperSocketPath);

            // Execute the helper; throws InvalidOperationException if not available
            await _executor.ExecuteHelperAsync(helperSocketPath, ct);

            _logger.LogInformation(
                "System shutdown initiated. System should shut down within 20 seconds.");

            await Task.CompletedTask;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex,
                "Shutdown helper not available or not configured: {Message}",
                ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating shutdown: {Message}", ex.Message);
            throw new InvalidOperationException("Error initiating shutdown.", ex);
        }
    }
}
