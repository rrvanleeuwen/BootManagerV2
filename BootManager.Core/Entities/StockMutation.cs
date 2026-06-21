namespace BootManager.Core.Entities;

/// <summary>
/// Voorraadbijzonderheid: registreert een verbruik, telling of correctie op een product-locatiecombinatie.
/// </summary>
public class StockMutation
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>Gekoppeld product (verplicht).</summary>
    public Guid ProductId { get; private set; }

    /// <summary>Gekoppelde opslaglocatie (verplicht).</summary>
    public Guid StorageLocationId { get; private set; }

    /// <summary>Type mutatie: Verbruik, Correctie of Telling.</summary>
    public StockMutationType MutationType { get; private set; }

    /// <summary>Hoeveelheid vóór mutatie.</summary>
    public decimal OldQuantity { get; private set; }

    /// <summary>Hoeveelheid ná mutatie.</summary>
    public decimal NewQuantity { get; private set; }

    /// <summary>Timestamp mutatie (UTC).</summary>
    public DateTime MutatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>Id van de gebruiker die de mutatie uitvoerde.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Optionele vrije notitie.</summary>
    public string? Note { get; private set; }

    public Product Product { get; private set; } = default!;
    public StorageLocation StorageLocation { get; private set; } = default!;
    public LocalUser User { get; private set; } = default!;

    private StockMutation() { }

    private StockMutation(Guid productId, Guid locationId, StockMutationType type, decimal oldQty, decimal newQty, Guid userId, string? note)
    {
        ProductId = productId;
        StorageLocationId = locationId;
        MutationType = type;
        OldQuantity = oldQty;
        NewQuantity = newQty;
        UserId = userId;
        Note = note;
        MutatedAt = DateTime.UtcNow;
    }

    public static StockMutation Create(Guid productId, Guid locationId, StockMutationType type, decimal oldQty, decimal newQty, Guid userId, string? note = null)
        => new(productId, locationId, type, oldQty, newQty, userId, note);
}
