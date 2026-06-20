namespace BootManager.Core.Entities;

/// <summary>
/// Koppeling tussen product en categorie: voorbereid voor toekomstige meerdere categorieën.
/// Deze story laat slechts één actieve categorie per product toe in de UI.
/// Ondersteunt archiveren/heractiveren van koppelingen.
/// </summary>
public class ProductCategoryMapping
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid ProductId { get; private set; }
    public Guid ProductCategoryId { get; private set; }

    /// <summary>
    /// Status van de koppeling: true = actief, false = gearchiveerd.
    /// In deze story mag per product slechts één actieve koppeling bestaan.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public Product Product { get; private set; } = default!;
    public ProductCategory ProductCategory { get; private set; } = default!;

    private ProductCategoryMapping() { }

    private ProductCategoryMapping(Guid productId, Guid categoryId)
    {
        ProductId = productId;
        ProductCategoryId = categoryId;
        IsActive = true;
    }

    public static ProductCategoryMapping Create(Guid productId, Guid categoryId)
        => new(productId, categoryId);

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Reactivate()
    {
        IsActive = true;
    }
}
