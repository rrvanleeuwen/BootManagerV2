using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BootManager.Tools.Ingest.Models;
using BootManager.Tools.Ingest.Options;

namespace BootManager.Tools.Ingest.Services;

/// <summary>
/// Background service voor ingest van netwerkgegevens.
/// Deze service luistert naar UDP-berichten op het geconfigureerde adres en poort,
/// splitst deze in losse regels en parseert ze in interne modellen.
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
    /// Luistert naar inkomende UDP-berichten, verwerkt deze in regels en parseert deze naar interne modellen.
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

                // Verwerk de ontvangen data in losse regels
                var rawLines = ExtractLinesFromData(receivedData);

                if (rawLines.Count > 0)
                {
                    // Parse elke regel naar het interne model
                    var parsedLines = new List<ReceivedNetworkLine>();
                    foreach (var line in rawLines)
                    {
                        var parsed = ParseNetworkLine(line);
                        parsedLines.Add(parsed);
                    }

                    _logger.LogInformation("Packet processed: {LineCount} lines, {MessageIds}", 
                        parsedLines.Count, 
                        string.Join(", ", parsedLines.Where(p => !string.IsNullOrEmpty(p.MessageId)).Select(p => p.MessageId)));
                }
                else
                {
                    _logger.LogDebug("Empty packet received");
                }
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

    /// <summary>
    /// Parseert een ontvangen regelstring naar een <see cref="ReceivedNetworkLine"/> model.
    /// Verwacht formaat: HH:mm:ss.fff R 0A1B2C3D AA BB CC ...
    /// Waarbij: 0A1B2C3D de MessageId is en AA BB CC ... de PayloadHex.
    /// </summary>
    /// <param name="line">De ontvangen regelstring.</param>
    /// <returns>Een gevuld of partieel gevuld <see cref="ReceivedNetworkLine"/> model.</returns>
    private static ReceivedNetworkLine ParseNetworkLine(string line)
    {
        var model = new ReceivedNetworkLine
        {
            ReceivedAtUtc = DateTime.UtcNow,
            RawLine = line,
            Source = "Simulator",
            Protocol = "YdenRawLike"
        };

        // Eenvoudige parsing: splits op spaties
        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 3)
        {
            // Verwacht: [0] = HH:mm:ss.fff, [1] = R, [2] = 0A1B2C3D, [3+] = AA BB CC ...
            
            // Haal MessageId (4e element, typisch device ID in hex)
            if (parts.Length > 2)
            {
                model.MessageId = parts[2];
            }

            // Haal PayloadHex (alles van het 4e element af)
            if (parts.Length > 3)
            {
                model.PayloadHex = string.Join(" ", parts.Skip(3));
            }
        }

        return model;
    }

    /// <summary>
    /// Haalt afzonderlijke regels uit ontvangen UDP-data.
    /// Splitst op CRLF en LF, en slaat lege en whitespace-regels over.
    /// </summary>
    /// <param name="data">De ontvangen UTF-8 gedecodeerde gegevens.</param>
    /// <returns>Lijst met niet-lege regels.</returns>
    private static List<string> ExtractLinesFromData(string data)
    {
        var lines = new List<string>();

        // Splitsen op zowel CRLF als LF
        var rawLines = data.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        foreach (var rawLine in rawLines)
        {
            var trimmed = rawLine.Trim();

            // Sla lege en whitespace-regels over
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            lines.Add(trimmed);
        }

        return lines;
    }
}