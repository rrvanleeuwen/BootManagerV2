using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;

namespace BootManager.Application.Inventory.Contracts;

/// <summary>
/// Service voor voorraadbeheer per product en locatie.
/// </summary>
public interface IStockService
{
    /// <summary>
    /// Voegt voorraad toe aan een locatie voor een product.
    /// Als het product al op die locatie ligt, wordt de hoeveelheid aangevuld.
    /// </summary>
    Task<InventoryOperationResult<StockDto>> AddOrIncrementStockAsync(
        Guid productId, Guid locationId, decimal quantity, CancellationToken ct = default);

    /// <summary>
    /// Haalt alle voorraadregels voor een gegeven locatie.
    /// </summary>
    Task<InventoryOperationResult<IReadOnlyList<StockDto>>> GetStocksByLocationAsync(
        Guid locationId, CancellationToken ct = default);

    /// <summary>
    /// Haalt alle gekoppelde locaties met voorraad voor een gegeven product.
    /// </summary>
    Task<InventoryOperationResult<IReadOnlyList<StockDto>>> GetStocksByProductAsync(
        Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Zoekt producten in een gegeven locatie op productnaam of gekoppelde code.
    /// </summary>
    Task<InventoryOperationResult<IReadOnlyList<ProductDto>>> SearchProductsInLocationAsync(
        Guid locationId, string searchTerm, CancellationToken ct = default);

    /// <summary>
    /// Verwijdert een voorraadregel.
    /// </summary>
    Task<InventoryOperationResult> DeleteStockAsync(Guid stockId, CancellationToken ct = default);

    /// <summary>
    /// Haalt de meest recente voorraadregel voor een product op (voor locatievoorstel).
    /// </summary>
    Task<InventoryOperationResult<StockDto>> GetMostRecentStockForProductAsync(
        Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Haalt alternatieve locaties voor een product op (zonder duplicaten, zonder meest recente).
    /// </summary>
    Task<InventoryOperationResult<IReadOnlyList<StockDto>>> GetAlternativeLocationsForProductAsync(
        Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Haalt alle actieve voorraadregels voor een product op (waarbij Quantity > 0).
    /// </summary>
    Task<InventoryOperationResult<IReadOnlyList<StockDto>>> GetActiveStocksByProductAsync(
        Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Haalt de verwachte (laatst gebruikte) locatie voor een product op, zelfs als daar geen actieve voorraad meer is.
    /// Raadpleegt het aparte verwachte-locatie-register, niet afhankelijk van actieve Stock-regels.
    /// </summary>
    Task<InventoryOperationResult<StockDto>> GetExpectedLocationForProductAsync(
        Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Verwerkt een voorraadbijzonderheid (Verbruik, Correctie of Telling) en slaat deze op.
    /// Voor Verbruik: neemt de opgegeven hoeveelheid af van de huidige voorraad.
    /// Voor Correctie/Telling: stelt de voorraad in op de opgegeven hoeveelheid.
    /// Blokkeert Verbruik wanneer de afname groter is dan huidige voorraad.
    /// Verwijdert de voorraadregel wanneer de resultaat 0 is, maar behoudt de verwachte locatie.
    /// </summary>
    Task<InventoryOperationResult> MutateStockAsync(
        Guid productId, Guid locationId, Core.Entities.StockMutationType mutationType,
        decimal quantityOrAmount, Guid userId, string? note = null, CancellationToken ct = default);

    /// <summary>
    /// Haalt alle voorraadbijzonderheden op, standaard nieuwste eerst.
    /// </summary>
    Task<InventoryOperationResult<IReadOnlyList<StockMutationDto>>> GetStockMutationsAsync(
        CancellationToken ct = default);
}
