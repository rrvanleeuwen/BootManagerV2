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

    /// <summary>Numerieke hoeveelheid in standaardeenheid van het product.</summary>
    public decimal Quantity { get; private set; }

    public Product Product { get; private set; } = default!;
    public StorageLocation StorageLocation { get; private set; } = default!;

    private Stock() { }

    private Stock(Guid productId, Guid locationId, decimal quantity)
    {
        ProductId = productId;
        StorageLocationId = locationId;
        Quantity = quantity;
    }

    public static Stock Create(Guid productId, Guid locationId, decimal quantity)
        => new(productId, locationId, quantity);

    public void SetQuantity(decimal newQuantity)
    {
        Quantity = newQuantity;
    }

    public void AddQuantity(decimal amount)
    {
        Quantity += amount;
    }
}
