using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;

namespace BootManager.IntegrationTests.Storage;

internal class NullStockService : IStockService
{
    public Task<InventoryOperationResult<StockDto>> AddOrIncrementStockAsync(Guid productId, Guid locationId, decimal quantity, CancellationToken ct = default)
        => Task.FromResult(InventoryOperationResult<StockDto>.Ok(new StockDto()));

    public Task<InventoryOperationResult<IReadOnlyList<StockDto>>> GetStocksByLocationAsync(Guid locationId, CancellationToken ct = default)
        => Task.FromResult(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

    public Task<InventoryOperationResult<IReadOnlyList<StockDto>>> GetStocksByProductAsync(Guid productId, CancellationToken ct = default)
        => Task.FromResult(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));

    public Task<InventoryOperationResult<IReadOnlyList<ProductDto>>> SearchProductsInLocationAsync(Guid locationId, string searchTerm, CancellationToken ct = default)
        => Task.FromResult(InventoryOperationResult<IReadOnlyList<ProductDto>>.Ok(new List<ProductDto>().AsReadOnly()));

    public Task<InventoryOperationResult> DeleteStockAsync(Guid stockId, CancellationToken ct = default)
        => Task.FromResult(InventoryOperationResult.Ok());

    public Task<InventoryOperationResult<StockDto>> GetMostRecentStockForProductAsync(Guid productId, CancellationToken ct = default)
        => Task.FromResult(InventoryOperationResult<StockDto>.NotFound());

    public Task<InventoryOperationResult<IReadOnlyList<StockDto>>> GetAlternativeLocationsForProductAsync(Guid productId, CancellationToken ct = default)
        => Task.FromResult(InventoryOperationResult<IReadOnlyList<StockDto>>.Ok(new List<StockDto>().AsReadOnly()));
}
