using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BootManager.Tools.Ingest.Options;

namespace BootManager.Tools.Ingest.Services;

/// <summary>
/// Background service voor ingest van netwerkgegevens.
/// Deze service luistert naar UDP-berichten op het geconfigureerde adres en poort,
/// en logt de ontvangen inhoud.
/// </summary>
public class IngestService : BackgroundService
{
    private readonly IOptions<IngestOptions> _options;
    private readonly ILogger<IngestService> _logger;
    private UdpClient? _udpClient;

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="IngestService"/>.
    /// </summary>
    /// <param name="options">De ingest-opties uit configuratie.</param>
    /// <param name="logger">Logger-instantie.</param>
    public IngestService(IOptions<IngestOptions> options, ILogger<IngestService> logger)
    {
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Voert de ingest-service uit in de achtergrond.
    /// Luistert op het geconfigureerde UDP-adres en poort naar inkomende berichten.
    /// </summary>
    /// <param name="stoppingToken">Token om de service tot stoppen.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IngestService is starting...");
        _logger.LogInformation("Configured to listen on {Address}:{Port}", 
            _options.Value.ListenAddress, _options.Value.ListenPort);

        try
        {
            // Zet het listen-adres om naar IPAddress
            if (!IPAddress.TryParse(_options.Value.ListenAddress, out var ipAddress))
            {
                _logger.LogError("Invalid listen address: {Address}", _options.Value.ListenAddress);
                return;
            }

            var endpoint = new IPEndPoint(ipAddress, _options.Value.ListenPort);
            _udpClient = new UdpClient(endpoint);
            _logger.LogInformation("UDP listener started successfully on {Endpoint}", endpoint);

            // Luister continu naar inkomende UDP-berichten
            await ListenForMessagesAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("IngestService is stopping.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in IngestService");
        }
        finally
        {
            _udpClient?.Dispose();
            _logger.LogInformation("IngestService stopped.");
        }
    }

    /// <summary>
    /// Luistert naar inkomende UDP-berichten en logt deze.
    /// </summary>
    /// <param name="stoppingToken">Token voor het stoppen van de luisterbewerking.</param>
    private async Task ListenForMessagesAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await _udpClient!.ReceiveAsync(stoppingToken);
                var receivedData = Encoding.UTF8.GetString(result.Buffer);
                var remoteEndPoint = result.RemoteEndPoint;

                _logger.LogInformation("Received UDP message from {RemoteEndPoint}: {Message}",
                    remoteEndPoint, receivedData);
            }
            catch (OperationCanceledException)
            {
                // Dit is normaal bij het stoppen van de service
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving UDP message");
            }
        }
    }
}