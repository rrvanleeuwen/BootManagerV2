namespace BootManager.Core.Entities;

/// <summary>
/// Tracks the expected (last used) location for a product, persisting even after the active Stock record is deleted.
/// Ensures that when inventory is consumed to 0, we can still suggest where that product was last used.
/// </summary>
public class StockExpectedLocation
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>Product for which we track expected location.</summary>
    public Guid ProductId { get; private set; }

    /// <summary>Expected location (last used location for this product).</summary>
    public Guid StorageLocationId { get; private set; }

    /// <summary>Timestamp when this expected location was last updated.</summary>
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public Product Product { get; private set; } = default!;
    public StorageLocation StorageLocation { get; private set; } = default!;

    private StockExpectedLocation() { }

    private StockExpectedLocation(Guid productId, Guid locationId)
    {
        ProductId = productId;
        StorageLocationId = locationId;
        UpdatedAt = DateTime.UtcNow;
    }

    public static StockExpectedLocation Create(Guid productId, Guid locationId)
        => new(productId, locationId);

    public void UpdateLocation(Guid newLocationId)
    {
        StorageLocationId = newLocationId;
        UpdatedAt = DateTime.UtcNow;
    }
}
