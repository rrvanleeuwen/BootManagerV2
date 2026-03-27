using BootManager.Application.NetworkMessages.DTOs;
using BootManager.Application.NetworkMessageParsing.DTOs;
using BootManager.Application.NetworkMessageParsing.Services;
using BootManager.Application.NetworkMessageInterpretation.Contracts;
using BootManager.Application.NetworkMessageInterpretation.DTOs;
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
/// Implementation of <see cref="INetworkMessageService"/> using the generic <see cref="IRepository{T}"/>.
/// Voert parsing uit als tussenstap richting latere interpretatie, zonder extra persistentie.
/// Voert ook semantische interpretatie uit voor ondersteunde berichttypen (bijv. Battery).
/// </summary>
public class NetworkMessageService : INetworkMessageService
{
    private readonly IRepository<NetworkMessage> _repo;
    private readonly INetworkMessageParserService _parserService;
    private readonly INetworkMessageInterpreter<BatteryMessageInterpretationDto> _batteryInterpreter;
    private readonly ILogger<NetworkMessageService> _logger;

    /// <summary>
    /// Creëert een nieuwe <see cref="NetworkMessageService"/>.
    /// </summary>
    public NetworkMessageService(
        IRepository<NetworkMessage> repo,
        INetworkMessageParserService parserService,
        INetworkMessageInterpreter<BatteryMessageInterpretationDto> batteryInterpreter,
        ILogger<NetworkMessageService> logger)
    {
        _repo = repo;
        _parserService = parserService;
        _batteryInterpreter = batteryInterpreter;
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

                    // Semantische interpretatie voor ondersteunde berichttypen
                    TryInterpretBatteryMessage(parseResult);
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
    /// Probeert semantische Battery-interpretatie uit te voeren op een technisch parse-resultaat.
    /// Als interpretatie slaagt, logt dit intern. Fouten blokkeren niet de bestaande raw opslag.
    /// </summary>
    /// <param name="parseResult">Het technische parse-resultaat.</param>
    private void TryInterpretBatteryMessage(NetworkMessageParseResultDto parseResult)
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

    /// <inheritdoc />
    public async Task<IReadOnlyList<NetworkMessageDto>> GetLatestAsync(int limit = 50, CancellationToken ct = default)
    {
        // Haal records via generieke repository en sorteer in-memory op ReceivedAtUtc desc.
        // Opmerking: deze service gebruikt alleen IRepository<T> zoals gevraagd; optimalisaties op DB-niveau
        // kunnen later toegevoegd worden indien gewenst.
        var all = await _repo.ListAsync(ct: ct);
        var latest = all
            .OrderByDescending(n => n.ReceivedAtUtc)
            .Take(limit)
            .Select(n => new NetworkMessageDto
            {
                Id = n.Id,
                ReceivedAtUtc = n.ReceivedAtUtc,
                Source = n.Source,
                Protocol = n.Protocol,
                RawLine = n.RawLine,
                MessageId = n.MessageId,
                PayloadHex = n.PayloadHex
            })
            .ToList()
            .AsReadOnly();

        return latest;
    }
}