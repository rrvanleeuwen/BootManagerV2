using BootManager.Application.Logbook.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BootManager.Application.Logbook.Services;

/// <summary>
/// Contract voor logboek-gerelateerde bewerkingen: reizen en logboekregels.
/// </summary>
public interface ILogbookService
{
    /// <summary>
    /// Maakt een nieuwe reis aan en retourneert het opgeslagen DTO.
    /// </summary>
    Task<LogbookTripDto> CreateTripAsync(CreateLogbookTripDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retourneert de reis met het opgegeven id, of null als niet gevonden.
    /// </summary>
    Task<LogbookTripDto?> GetTripAsync(int tripId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retourneert alle reizen gesorteerd op vertrekdatum aflopend.
    /// </summary>
    Task<IReadOnlyList<LogbookTripDto>> GetAllTripsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Werkt een bestaande reis bij op basis van het opgegeven id.
    /// </summary>
    Task UpdateTripAsync(int tripId, CreateLogbookTripDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retourneert alle logboekregels voor een reis gesorteerd op tijdstempel oplopend.
    /// </summary>
    Task<IReadOnlyList<LogbookEntryDto>> GetEntriesAsync(int tripId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Maakt een nieuwe logboekregel aan voor de opgegeven reis.
    /// </summary>
    Task<LogbookEntryDto> CreateEntryAsync(int tripId, SaveLogbookEntryDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Werkt een bestaande logboekregel bij.
    /// </summary>
    Task UpdateEntryAsync(int entryId, SaveLogbookEntryDto dto, CancellationToken cancellationToken = default);
}
