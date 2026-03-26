using BootManager.Application.NetworkMessages.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BootManager.Application.NetworkMessages.Services
{
    /// <summary>
    /// Interface voor application-service die NetworkMessage use-cases aanbiedt.
    /// </summary>
    public interface INetworkMessageService
    {
        /// <summary>
        /// Maakt een nieuw NetworkMessage-record aan en retourneert het nieuw aangemaakte Id.
        /// </summary>
        Task<System.Guid> CreateAsync(CreateNetworkMessageRequestDto request, CancellationToken ct = default);

        /// <summary>
        /// Haalt de meest recente NetworkMessages op, begrensd door <paramref name="limit"/>.
        /// </summary>
        Task<IReadOnlyList<NetworkMessageDto>> GetLatestAsync(int limit = 50, CancellationToken ct = default);
    }
}