using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BootManager.Tools.Ingest.Models;
using BootManager.Tools.Ingest.Options;

namespace BootManager.Tools.Ingest.Services;

/// <summary>
/// Background service voor ingest van NMEA 0183 sentences via UDP.
/// Luistert op een configureerbaar UDP-endpoint, slaat sentences raw op via de Web API.
/// Semantische parsing van NMEA 0183 sentences vindt niet plaats in deze service.
/// </summary>
public class Nmea0183IngestService : BackgroundService
{
    private readonly IOptions<IngestOptions> _options;
    private readonly ILogger<Nmea0183IngestService> _logger;
    private readonly HttpClient _httpClient;
    private UdpClient? _udpClient;

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="Nmea0183IngestService"/>.
    /// </summary>
    /// <param name="options">De ingest-opties uit configuratie.</param>
    /// <param name="logger">Logger-instantie.</param>
    /// <param name="httpClient">HttpClient voor API-communicatie.</param>
    public Nmea0183IngestService(IOptions<IngestOptions> options, ILogger<Nmea0183IngestService> logger, HttpClient httpClient)
    {
        _options = options;
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Voert de NMEA 0183 ingest-service uit in de achtergrond.
    /// De service wordt alleen gestart als de NMEA 0183 listener is ingeschakeld.
    /// </summary>
    /// <param name="stoppingToken">Token om de service tot stoppen.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var nmea0183Options = _options.Value.Nmea0183;

        if (!nmea0183Options.Enabled)
        {
            _logger.LogInformation("Nmea0183IngestService is disabled via configuratie.");
            return;
        }

        _logger.LogInformation("Nmea0183IngestService is starting...");
        _logger.LogInformation("Configured to listen on {Address}:{Port}",
            nmea0183Options.ListenAddress, nmea0183Options.ListenPort);

        try
        {
            if (!IPAddress.TryParse(nmea0183Options.ListenAddress, out var ipAddress))
            {
                _logger.LogError("Ongeldig listen-adres voor NMEA 0183: {Address}", nmea0183Options.ListenAddress);
                return;
            }

            var endpoint = new IPEndPoint(ipAddress, nmea0183Options.ListenPort);
            _udpClient = new UdpClient(endpoint);
            _logger.LogInformation("NMEA 0183 UDP listener gestart op {Endpoint}", endpoint);

            await ListenForSentencesAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Nmea0183IngestService wordt gestopt.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout in Nmea0183IngestService");
        }
        finally
        {
            _udpClient?.Dispose();
            _logger.LogInformation("Nmea0183IngestService gestopt.");
        }
    }

    /// <summary>
    /// Luistert continu naar inkomende UDP-datagrammen en verwerkt NMEA 0183 sentences.
    /// </summary>
    /// <param name="stoppingToken">Token voor het stoppen van de luisterbewerking.</param>
    private async Task ListenForSentencesAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await _udpClient!.ReceiveAsync(stoppingToken);
                var source = result.RemoteEndPoint.ToString();
                var receivedData = Encoding.UTF8.GetString(result.Buffer);

                var sentences = ExtractSentences(receivedData);

                if (sentences.Count > 0)
                {
                    int successCount = 0;

                    foreach (var sentence in sentences)
                    {
                        var model = new ReceivedNetworkLine
                        {
                            ReceivedAtUtc = DateTime.UtcNow,
                            RawLine = sentence,
                            Source = source,
                            Protocol = "NMEA0183"
                            // MessageId en PayloadHex worden niet gevuld voor NMEA 0183 raw sentences
                        };

                        var success = await SendToApiAsync(model, stoppingToken);
                        if (success) successCount++;
                    }

                    _logger.LogInformation("NMEA 0183 pakket verwerkt: {SuccessCount}/{TotalCount} sentences verstuurd",
                        successCount, sentences.Count);
                }
                else
                {
                    _logger.LogDebug("Leeg NMEA 0183 pakket ontvangen");
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ontvangen van NMEA 0183 UDP-bericht");
            }
        }
    }

    /// <summary>
    /// Verzendt een ontvangen NMEA 0183 netwerkregel naar de BootManager.Web API.
    /// </summary>
    /// <param name="line">De ontvangen netwerkregel.</param>
    /// <param name="cancellationToken">Cancellation token voor de HTTP-request.</param>
    /// <returns>True als de request succesvol was, false otherwise.</returns>
    private async Task<bool> SendToApiAsync(ReceivedNetworkLine line, CancellationToken cancellationToken)
    {
        try
        {
            var request = new
            {
                receivedAtUtc = line.ReceivedAtUtc,
                source = line.Source,
                protocol = line.Protocol,
                rawLine = line.RawLine,
                messageId = (string?)null,
                payloadHex = (string?)null
            };

            var url = $"{_options.Value.ApiBaseUrl}{_options.Value.NetworkMessagesEndpoint}";
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            _logger.LogWarning("API retourneerde status {StatusCode} voor NMEA 0183 sentence", response.StatusCode);
            return false;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request mislukt voor NMEA 0183 sentence");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Onverwachte fout bij versturen naar API");
            return false;
        }
    }

    /// <summary>
    /// Haalt afzonderlijke NMEA 0183 sentences uit ontvangen UDP-data.
    /// Splitst op regeleindes en slaat lege regels over.
    /// </summary>
    /// <param name="data">De ontvangen UTF-8 gedecodeerde gegevens.</param>
    /// <returns>Lijst met niet-lege sentences.</returns>
    private static List<string> ExtractSentences(string data)
    {
        var sentences = new List<string>();
        var rawLines = data.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        foreach (var rawLine in rawLines)
        {
            var trimmed = rawLine.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                sentences.Add(trimmed);
            }
        }

        return sentences;
    }
}
