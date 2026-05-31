using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace BootManager.Web.Services;

/// <summary>
/// Implementatie van veilige, begrensde shutdown-helper executor via Unix domain socket.
/// Verbindt met een host-side systemd service die luistert op een socket.
/// </summary>
public class ShutdownHelperExecutor : IShutdownHelperExecutor
{
    private readonly ILogger<ShutdownHelperExecutor> _logger;

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="ShutdownHelperExecutor"/>.
    /// </summary>
    public ShutdownHelperExecutor(ILogger<ShutdownHelperExecutor> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task ExecuteHelperAsync(string helperSocketPath, CancellationToken ct = default)
    {
        // Valideer dat het socket-pad een Unix-socket pad is (eindigt op .sock of slaat)
        if (!helperSocketPath.EndsWith(".sock", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Shutdown helper socket path must be a .sock file: {helperSocketPath}");
        }

        // Controleer dat het socket-bestand aanwezig is
        if (!File.Exists(helperSocketPath))
        {
            throw new InvalidOperationException(
                $"Shutdown helper socket not found: {helperSocketPath}. " +
                $"Host-side systemd service may not be running.");
        }

        try
        {
            // Maak verbinding met het Unix domain socket
            var socket = new UnixDomainSocketEndPoint(helperSocketPath);
            using var client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

            // Verbind met timeout
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                try
                {
                    await client.ConnectAsync(socket, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new InvalidOperationException(
                        $"Timeout connecting to shutdown helper socket: {helperSocketPath}");
                }
            }

            _logger.LogInformation(
                "Connected to shutdown helper socket: {SocketPath}",
                helperSocketPath);

            // Stuur SHUTDOWN commando (no arguments, no injection)
            const string shutdownCommand = "SHUTDOWN\n";
            var buffer = System.Text.Encoding.UTF8.GetBytes(shutdownCommand);

            using (var networkStream = new NetworkStream(client, ownsSocket: false))
            {
                await networkStream.WriteAsync(buffer, 0, buffer.Length, ct);
                await networkStream.FlushAsync(ct);
            }

            _logger.LogInformation(
                "Shutdown command sent to socket: {SocketPath}",
                helperSocketPath);

            // Fire-and-forget: socket service handles actual shutdown
            await Task.CompletedTask;
        }
        catch (OperationCanceledException)
        {
            throw new InvalidOperationException(
                $"Shutdown helper operation cancelled");
        }
        catch (System.IO.FileNotFoundException ex)
        {
            throw new InvalidOperationException(
                $"Shutdown helper socket not accessible: {helperSocketPath}", ex);
        }
        catch (SocketException ex)
        {
            throw new InvalidOperationException(
                $"Failed to connect to shutdown helper socket: {helperSocketPath}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error executing shutdown helper: {helperSocketPath}", ex);
        }
    }
}
