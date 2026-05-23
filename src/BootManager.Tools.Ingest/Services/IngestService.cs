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
/// Background service voor ingest van netwerkgegevens via één gecombineerde UDP-listener.
/// Detecteert per ontvangen regel het protocol op basis van regelinhoud:
/// regels die beginnen met '$' worden behandeld als NMEA 0183, overige regels als NMEA 2000/raw.
/// Verzendt alle regels via HTTP POST naar de BootManager.Web API.
/// </summary>
public class IngestService : BackgroundService
{
    private readonly IOptions<IngestOptions> _options;
    private readonly ILogger<IngestService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IIngestCaptureLogger _captureLogger;
    private UdpClient? _udpClient;

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="IngestService"/>.
    /// </summary>
    /// <param name="options">De ingest-opties uit configuratie.</param>
    /// <param name="logger">Logger-instantie.</param>
    /// <param name="httpClient">HttpClient voor API-communicatie.</param>
    /// <param name="captureLogger">Optionele capture logger voor raw NDJSON-logging.</param>
    public IngestService(IOptions<IngestOptions> options, ILogger<IngestService> logger, HttpClient httpClient, IIngestCaptureLogger captureLogger)
    {
        _options = options;
        _logger = logger;
        _httpClient = httpClient;
        _captureLogger = captureLogger;
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

        await _captureLogger.InitializeAsync();

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
    /// Luistert naar inkomende UDP-berichten, verwerkt deze in regels,
    /// parseert deze naar modellen en verzendt ze naar de API.
    /// </summary>
    /// <param name="stoppingToken">Token voor het stoppen van de luisterbewerking.</param>
    private async Task ListenForMessagesAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await _udpClient!.ReceiveAsync(stoppingToken);
                var source = result.RemoteEndPoint.ToString();
                var receivedData = Encoding.UTF8.GetString(result.Buffer);

                // Verwerk de ontvangen data in losse regels
                var rawLines = ExtractLinesFromData(receivedData);

                if (rawLines.Count > 0)
                {
                    int successCount = 0;
                    var failedMessageIds = new List<string>();

                    // Parse en verstuur elke regel; detecteer protocol op basis van inhoud
                    foreach (var line in rawLines)
                    {
                        var parsed = ParseNetworkLine(line, source);

                        // Capture raw data before API posting, so field logs survive API hangs/failures.
                        if (_captureLogger.IsEnabled)
                        {
                            var record = new CaptureRecord
                            {
                                ReceivedAtUtc = parsed.ReceivedAtUtc,
                                RemoteEndpoint = source,
                                DetectedProtocol = parsed.Protocol,
                                RawLine = parsed.RawLine,
                                MessageId = string.IsNullOrEmpty(parsed.MessageId) ? null : parsed.MessageId,
                                PayloadHex = string.IsNullOrEmpty(parsed.PayloadHex) ? null : parsed.PayloadHex,
                                ApiPostSucceeded = null,
                                ApiStatusCode = null,
                                ErrorMessage = null
                            };
                            await _captureLogger.WriteAsync(record);
                        }

                        var (success, _, _) = await SendToApiWithDetailsAsync(parsed, stoppingToken);

                        if (success)
                        {
                            successCount++;
                        }
                        else if (!string.IsNullOrEmpty(parsed.MessageId))
                        {
                            failedMessageIds.Add(parsed.MessageId);
                        }
                    }

                    // Compact logging
                    _logger.LogInformation("Packet processed: {SuccessCount}/{TotalCount} sent", 
                        successCount, rawLines.Count);

                    if (failedMessageIds.Count > 0)
                    {
                        _logger.LogWarning("Failed to send messages with IDs: {FailedIds}", 
                            string.Join(", ", failedMessageIds));
                    }
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
    /// Verzendt een geparste netwerkregel naar de BootManager.Web API
    /// en retourneert succes, HTTP-statuscode en eventuele foutmelding.
    /// </summary>
    /// <param name="line">De geparste netwerkregel.</param>
    /// <param name="cancellationToken">Cancellation token voor de HTTP-request.</param>
    /// <returns>Tuple met success-vlag, optionele statuscode en optionele foutmelding.</returns>
    private async Task<(bool success, int? statusCode, string? errorMessage)> SendToApiWithDetailsAsync(ReceivedNetworkLine line, CancellationToken cancellationToken)
    {
        try
        {
            var request = new
            {
                receivedAtUtc = line.ReceivedAtUtc,
                source = line.Source,
                protocol = line.Protocol,
                rawLine = line.RawLine,
                messageId = string.IsNullOrEmpty(line.MessageId) ? null : line.MessageId,
                payloadHex = string.IsNullOrEmpty(line.PayloadHex) ? null : line.PayloadHex
            };

            var url = $"{_options.Value.ApiBaseUrl}{_options.Value.NetworkMessagesEndpoint}";
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            var statusCode = (int)response.StatusCode;

            if (response.IsSuccessStatusCode)
            {
                return (true, statusCode, null);
            }

            var errorMessage = $"API returned status {response.StatusCode}";
            _logger.LogWarning("API returned status {StatusCode} for message {MessageId}", 
                response.StatusCode, line.MessageId ?? "unknown");
            return (false, statusCode, errorMessage);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed for message {MessageId}", 
                line.MessageId ?? "unknown");
            return (false, null, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending to API");
            return (false, null, ex.Message);
        }
    }

    /// <summary>
    /// Parseert een ontvangen regelstring naar een <see cref="ReceivedNetworkLine"/> model.
    /// Regels die beginnen met '$' worden herkend als NMEA 0183 sentences en raw opgeslagen.
    /// Overige regels worden geparseerd als NMEA 2000/raw-like simulatorregel.
    /// Verwacht formaat voor NMEA 2000: HH:mm:ss.fff R 0A1B2C3D AA BB CC ...
    /// Waarbij: 0A1B2C3D de MessageId is en AA BB CC ... de PayloadHex.
    /// </summary>
    /// <param name="line">De ontvangen regelstring.</param>
    /// <param name="source">Het remote endpoint van de afzender.</param>
    /// <returns>Een gevuld of partieel gevuld <see cref="ReceivedNetworkLine"/> model.</returns>
    internal static ReceivedNetworkLine ParseNetworkLine(string line, string source)
    {
        // NMEA 0183 sentence: begint met '$' of '!' (AIS sentences gebruiken vaak '!')
        // Bepaal een stabiele MessageId gebaseerd op de sentence-id (bijv. "$YDGGA,..." -> "YDGGA").
        if (line.StartsWith('$') || line.StartsWith('!'))
        {
            return new ReceivedNetworkLine
            {
                ReceivedAtUtc = DateTime.UtcNow,
                RawLine = line,
                Source = source,
                Protocol = "NMEA0183",
                MessageId = ExtractNmea0183SentenceId(line)
            };
        }

        // NMEA 2000 / raw-like simulatorregel
        var model = new ReceivedNetworkLine
        {
            ReceivedAtUtc = DateTime.UtcNow,
            RawLine = line,
            Source = source,
            Protocol = "NMEA2000"
        };

        // Eenvoudige parsing
        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 3)
        {
            // Verwacht: [0] = HH:mm:ss.fff, [1] = R, [2] = 0A1B2C3D, [3+] = AA BB CC ...

            // Haal MessageId (3e element, typisch device ID in hex)
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

    /// <summary>
    /// Extract the NMEA0183 sentence-id from a raw sentence.
    /// Example: "$YDGGA,1,2,3*00" -> "YDGGA"; "!AIVDM,1,1,,A,..." -> "AIVDM".
    /// Returns empty string if extraction fails.
    /// </summary>
    private static string ExtractNmea0183SentenceId(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return string.Empty;
        }

        var s = line.Trim();

        // Must start with '$' or '!'
        if (!(s.StartsWith('$') || s.StartsWith('!')))
        {
            return string.Empty;
        }

        // Remove leading start character
        var body = s.Length > 1 ? s[1..] : string.Empty;

        if (string.IsNullOrEmpty(body))
        {
            return string.Empty;
        }

        // Sentence-id ends at first comma or asterisk (checksum) or end of string
        var commaIndex = body.IndexOf(',');
        var starIndex = body.IndexOf('*');

        int endIndex = body.Length;
        if (commaIndex >= 0 && starIndex >= 0)
        {
            endIndex = Math.Min(commaIndex, starIndex);
        }
        else if (commaIndex >= 0)
        {
            endIndex = commaIndex;
        }
        else if (starIndex >= 0)
        {
            endIndex = starIndex;
        }

        var sentenceId = body[..endIndex].Trim();

        return string.IsNullOrEmpty(sentenceId) ? string.Empty : sentenceId.ToUpperInvariant();
    }
}
