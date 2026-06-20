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
}
