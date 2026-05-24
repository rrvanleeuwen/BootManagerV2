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

    /// <summary>
    /// Accordeert een logboekregel: status wordt Confirmed. Heeft geen effect als de regel al Confirmed is.
    /// </summary>
    Task ConfirmEntryAsync(int entryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Berekent het volgende verwachte logmoment voor een reis.
    /// Retourneert DateTime.UtcNow plus het loginterval, gebaseerd op de laatst ingevulde logboekregel (of DepartureUtc als geen regels).
    /// </summary>
    Task<DateTime> GetNextExpectedLogMomentAsync(int tripId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Maakt een Draft-logboekregel aan voor het opgegeven moment met automatische meetdatasuggesties.
    /// </summary>
    Task<LogbookEntryDto> CreateDraftEntryAsync(int tripId, DateTime entryTimeUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Berekent alle gemiste logmomenten voor een reis.
    /// Retourneert een overzicht met totaal aantal en geordende lijst van gemiste momenten (UTC).
    /// </summary>
    Task<MissedLogMomentsDto> GetMissedLogMomentsAsync(int tripId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Maakt meerdere Draft-logboekregels aan voor gemiste logmomenten, maximaal 24 per keer.
    /// Defensief begrensd: als meer dan 24 gemist zijn, worden alleen de eerste 24 aangemaakt.
    /// </summary>
    /// <param name="tripId">Reis-ID.</param>
    /// <param name="maxCount">Maximaal aantal regels aan te maken (standaard 24).</param>
    /// <returns>Aantal daadwerkelijk aangemaakte Draft-regels.</returns>
    Task<int> CreateMultipleDraftEntriesAsync(int tripId, int maxCount = 24, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verwijdert een logboekregel (hard delete). Werkt voor zowel Draft als Confirmed regels.
    /// </summary>
    /// <param name="entryId">ID van de te verwijderen regel.</param>
    Task DeleteEntryAsync(int entryId, CancellationToken cancellationToken = default);
}
