using System.Threading;
using System.Threading.Tasks;
using BootManager.Application.Logbook.DTOs;

namespace BootManager.Application.Logbook.Services;

/// <summary>
/// Service-contract voor het ophalen van detaildata voor één logboekregel.
/// </summary>
public interface ILogbookEntryDetailService
{
    /// <summary>
    /// Haalt de volledige detailweergave op voor de opgegeven logboekregel.
    /// Retourneert null als de logboekregel niet bestaat.
    /// </summary>
    /// <param name="entryId">Unieke identificator van de logboekregel.</param>
    /// <param name="cancellationToken">Annulerings-token.</param>
    Task<LogbookEntryDetailDto?> GetEntryDetailAsync(int entryId, CancellationToken cancellationToken = default);
}
