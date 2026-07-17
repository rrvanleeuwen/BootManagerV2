using BootManager.Application.Inventory.DTOs;

namespace BootManager.Application.Inventory.Contracts;

/// <summary>
/// Gerichte readmodel-boundary voor het productoverzicht: levert één gepagineerde,
/// database-begrensde pagina op zonder per product losse DTO- of voorraadqueries.
/// </summary>
public interface IProductOverviewReadQuery
{
    /// <summary>
    /// Haalt één productoverzichtspagina op, database-gefilterd op archiefstand en op
    /// hoofdletterongevoelige deelmatches in naam of omschrijving, stabiel gesorteerd op
    /// productnaam en daarna product-id. Alleen de gevraagde pagina en de bijbehorende
    /// actieve voorraad (hoeveelheid groter dan nul) worden geladen.
    /// </summary>
    /// <param name="searchTerm">Optionele zoekterm; leeg of null geeft het volledige (gefilterde) overzicht.</param>
    /// <param name="showArchived">True toont gearchiveerde producten, false toont actieve producten.</param>
    /// <param name="pageNumber">1-based paginanummer.</param>
    /// <param name="pageSize">Maximaal aantal producten per pagina.</param>
    Task<ProductOverviewPageDto> GetPageAsync(
        string? searchTerm, bool showArchived, int pageNumber, int pageSize, CancellationToken ct = default);
}
