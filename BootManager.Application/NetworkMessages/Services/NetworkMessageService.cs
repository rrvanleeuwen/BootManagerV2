using BootManager.Application.NetworkMessages.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BootManager.Application.NetworkMessages.Services;

/// <summary>
/// Implementation of <see cref="INetworkMessageService"/> using the generic <see cref="IRepository{T}"/>.
/// </summary>
public class NetworkMessageService : INetworkMessageService
{
    private readonly IRepository<NetworkMessage> _repo;

    /// <summary>
    /// Creëert een nieuwe <see cref="NetworkMessageService"/>.
    /// </summary>
    public NetworkMessageService(IRepository<NetworkMessage> repo)
    {
        _repo = repo;
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

        await _repo.AddAsync(entity, ct);
        return entity.Id;
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