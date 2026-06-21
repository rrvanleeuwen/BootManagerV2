namespace BootManager.Core.Entities;

/// <summary>
/// Voorraadregel: beschrijft hoeveelheid van een product op een opslaglocatie.
/// Per locatie maximaal één voorraadregel per product.
/// </summary>
public class Stock
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>Gekoppeld product (verplicht).</summary>
    public Guid ProductId { get; private set; }

    /// <summary>Gekoppelde opslaglocatie (verplicht).</summary>
    public Guid StorageLocationId { get; private set; }

    /// <summary>Verwachte locatie (laatst gebruikte locatie voor het product, behouden ook wanneer Quantity==0).</summary>
    public Guid? ExpectedLocationId { get; private set; }

    /// <summary>Numerieke hoeveelheid in standaardeenheid van het product.</summary>
    public decimal Quantity { get; private set; }

    /// <summary>Timestamp van laatste toevoeging of update.</summary>
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public Product Product { get; private set; } = default!;
    public StorageLocation StorageLocation { get; private set; } = default!;
    public StorageLocation? ExpectedLocation { get; private set; }

    private Stock() { }

    private Stock(Guid productId, Guid locationId, decimal quantity)
    {
        ProductId = productId;
        StorageLocationId = locationId;
        ExpectedLocationId = locationId;
        Quantity = quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Stock Create(Guid productId, Guid locationId, decimal quantity)
        => new(productId, locationId, quantity);

    public void SetQuantity(decimal newQuantity)
    {
        Quantity = newQuantity;
        ExpectedLocationId = StorageLocationId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddQuantity(decimal amount)
    {
        Quantity += amount;
        ExpectedLocationId = StorageLocationId;
        UpdatedAt = DateTime.UtcNow;
    }
}
