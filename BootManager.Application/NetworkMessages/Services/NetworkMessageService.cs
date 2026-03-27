using BootManager.Application.NetworkMessages.DTOs;
using BootManager.Application.NetworkMessageParsing.DTOs;
using BootManager.Application.NetworkMessageParsing.Services;
using BootManager.Application.NetworkMessageInterpretation.Contracts;
using BootManager.Application.NetworkMessageInterpretation.DTOs;
using BootManager.Application.BatteryMeasurements.DTOs;
using BootManager.Application.BatteryMeasurements.Services;
using BootManager.Application.DepthMeasurements.DTOs;
using BootManager.Application.DepthMeasurements.Services;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BootManager.Application.NetworkMessages.Services;

/// <summary>
/// Implementation van <see cref="INetworkMessageService"/> met behulp van de generieke <see cref="IRepository{T}"/>.
/// Voert parsing uit als tussenstap richting latere interpretatie, zonder extra persistentie.
/// Voert ook semantische interpretatie uit voor ondersteunde berichttypen (bijv. Battery, Depth) en persisteert succesvolle afleidingen.
/// </summary>
public class NetworkMessageService : INetworkMessageService
{
    private readonly IRepository<NetworkMessage> _repo;
    private readonly INetworkMessageParserService _parserService;
    private readonly INetworkMessageInterpreter<BatteryMessageInterpretationDto> _batteryInterpreter;
    private readonly IBatteryMeasurementService _batteryMeasurementService;
    private readonly INetworkMessageInterpreter<DepthMessageInterpretationDto> _depthInterpreter;
    private readonly IDepthMeasurementService _depthMeasurementService;
    private readonly ILogger<NetworkMessageService> _logger;

    /// <summary>
    /// Creëert een nieuwe <see cref="NetworkMessageService"/>.
    /// </summary>
    public NetworkMessageService(
        IRepository<NetworkMessage> repo,
        INetworkMessageParserService parserService,
        INetworkMessageInterpreter<BatteryMessageInterpretationDto> batteryInterpreter,
        IBatteryMeasurementService batteryMeasurementService,
        INetworkMessageInterpreter<DepthMessageInterpretationDto> depthInterpreter,
        IDepthMeasurementService depthMeasurementService,
        ILogger<NetworkMessageService> logger)
    {
        _repo = repo;
        _parserService = parserService;
        _batteryInterpreter = batteryInterpreter;
        _batteryMeasurementService = batteryMeasurementService;
        _depthInterpreter = depthInterpreter;
        _depthMeasurementService = depthMeasurementService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(CreateNetworkMessageRequestDto request, CancellationToken ct = default)
    {
        // Map DTO -> entity en persist via generieke repository.
        var entity = NetworkMessage.Create(
            receivedAtUtc: request.ReceivedAtUtc,
            source: request.Source,
            protocol: request.Protocol,
            rawLine: request.RawLine,
            messageId: request.MessageId,
            payloadHex: request.PayloadHex
        );

        // Parsing als tussenstap: voer parse uit voordat we opslaan.
        // Dit resultaat is voorlopig alleen intern en leidt niet tot extra persistentie.
        if (!string.IsNullOrWhiteSpace(request.MessageId) && !string.IsNullOrWhiteSpace(request.PayloadHex))
        {
            try
            {
                var parseRequest = new NetworkMessageParseRequestDto
                {
                    Source = request.Source,
                    ReceivedAtUtc = request.ReceivedAtUtc,
                    RawLine = request.RawLine,
                    MessageIdHex = request.MessageId,
                    PayloadHex = request.PayloadHex
                };

                var parseResult = _parserService.Parse(parseRequest);

                if (parseResult.IsSuccess)
                {
                    _logger.LogInformation(
                        "Netwerkbericht geparset: MessageType={MessageType}, MessageId={MessageId}",
                        parseResult.MessageType,
                        parseResult.MessageIdHex);

                    // Semantische interpretatie en afgeleide opslag voor ondersteunde berichttypen
                    await TryInterpretAndSaveBatteryMessageAsync(parseResult, request, ct);
                    await TryInterpretAndSaveDepthMessageAsync(parseResult, request, ct);
                }
                else
                {
                    _logger.LogWarning(
                        "Netwerkbericht parse-fout: MessageId={MessageId}, Error={Error}",
                        parseResult.MessageIdHex,
                        parseResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                // Parse-fouten blokkeren geen raw opslag. Log alleen ter info.
                _logger.LogWarning(
                    ex,
                    "Onverwachte fout bij parsing van netwerkbericht MessageId={MessageId}",
                    request.MessageId);
            }
        }

        await _repo.AddAsync(entity, ct);
        return entity.Id;
    }

    /// <summary>
    /// Probeert semantische Battery-interpretatie uit te voeren op een technisch parse-resultaat
    /// en persisteert het resultaat als een BatteryMeasurement.
    /// Fouten blokkeren niet de bestaande raw opslag.
    /// </summary>
    /// <param name="parseResult">Het technische parse-resultaat.</param>
    /// <param name="request">De originele netwerkbericht-request voor metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task TryInterpretAndSaveBatteryMessageAsync(
        NetworkMessageParseResultDto parseResult,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_batteryInterpreter.CanInterpret(parseResult))
            {
                return;
            }

            var interpretation = _batteryInterpreter.Interpret(parseResult);

            if (interpretation.IsSuccess && interpretation.Voltage.HasValue)
            {
                _logger.LogInformation(
                    "Battery-interpretatie geslaagd: Voltage={Voltage}{Unit}",
                    interpretation.Voltage,
                    interpretation.Unit);

                // Persisteer afgeleide battery-meting
                try
                {
                    var batteryDto = new CreateBatteryMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        Voltage = interpretation.Voltage.Value,
                        StateOfCharge = interpretation.StateOfCharge
                    };

                    await _batteryMeasurementService.SaveAsync(batteryDto, ct);
                }
                catch (Exception ex)
                {
                    // Battery-opslag-fouten blokkeren geen raw opslag. Log compact.
                    _logger.LogWarning(
                        ex,
                        "Batterijmeting-opslag mislukt voor MessageId={MessageId}",
                        request.MessageId);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Battery-interpretatie mislukt: {Error}",
                    interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            // Interpretatie-fouten blokkeren geen raw opslag.
            _logger.LogWarning(
                ex,
                "Onverwachte fout bij Battery-interpretatie");
        }
    }

    /// <summary>
    /// Probeert semantische Depth-interpretatie uit te voeren op een technisch parse-resultaat
    /// en persisteert het resultaat als een DepthMeasurement.
    /// Fouten blokkeren niet de bestaande raw opslag.
    /// </summary>
    /// <param name="parseResult">Het technische parse-resultaat.</param>
    /// <param name="request">De originele netwerkbericht-request voor metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task TryInterpretAndSaveDepthMessageAsync(
        NetworkMessageParseResultDto parseResult,
        CreateNetworkMessageRequestDto request,
        CancellationToken ct)
    {
        try
        {
            if (!_depthInterpreter.CanInterpret(parseResult))
            {
                return;
            }

            var interpretation = _depthInterpreter.Interpret(parseResult);

            if (interpretation.IsSuccess && interpretation.DepthMeters.HasValue)
            {
                _logger.LogInformation(
                    "Depth-interpretatie geslaagd: Depth={Depth}{Unit}",
                    interpretation.DepthMeters,
                    interpretation.Unit);

                // Persisteer afgeleide depth-meting
                try
                {
                    var depthDto = new CreateDepthMeasurementRequestDto
                    {
                        RecordedAtUtc = request.ReceivedAtUtc,
                        Source = request.Source,
                        MessageId = request.MessageId ?? string.Empty,
                        DepthMeters = interpretation.DepthMeters.Value
                    };

                    await _depthMeasurementService.SaveAsync(depthDto, ct);
                }
                catch (Exception ex)
                {
                    // Depth-opslag-fouten blokkeren geen raw opslag. Log compact.
                    _logger.LogWarning(
                        ex,
                        "Dieptemeting-opslag mislukt voor MessageId={MessageId}",
                        request.MessageId);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Depth-interpretatie mislukt: {Error}",
                    interpretation.ErrorMessage ?? "Onbekende fout");
            }
        }
        catch (Exception ex)
        {
            // Interpretatie-fouten blokkeren geen raw opslag.
            _logger.LogWarning(
                ex,
                "Onverwachte fout bij Depth-interpretatie");
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NetworkMessageDto>> GetLatestAsync(int limit = 50, CancellationToken ct = default)
    {
        var items = await _repo.ListAsync(ct: ct);

        return items
            .OrderByDescending(x => x.ReceivedAtUtc)
            .Take(limit)
            .Select(x => new NetworkMessageDto
            {
                Id = x.Id,
                ReceivedAtUtc = x.ReceivedAtUtc,
                Source = x.Source,
                Protocol = x.Protocol,
                RawLine = x.RawLine,
                MessageId = x.MessageId,
                PayloadHex = x.PayloadHex
            })
            .ToList()
            .AsReadOnly();
    }
}