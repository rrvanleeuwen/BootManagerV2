namespace BootManager.Core.Entities;

/// <summary>
/// Product-entiteit: item in de productcatalogus.
/// Bevat verplichte naam, exact één standaardeenheid, optionele omschrijving.
/// Ondersteunt soft delete (archiveren) en heractiveren.
/// Gekoppeld aan maximaal één actieve categorie via ProductCategoryMapping.
/// </summary>
public class Product
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>Leesbare productnaam (trimmed, max 100 chars).</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Optionele productomschrijving (max 500 chars).</summary>
    public string? Description { get; private set; }

    /// <summary>Verplichte standaardeenheid.</summary>
    public Guid DefaultUnitId { get; private set; }

    /// <summary>Soft delete: archivering datum; null = actief.</summary>
    public DateTime? ArchivedAt { get; private set; }

    public Unit DefaultUnit { get; private set; } = default!;
    public ICollection<ProductCategoryMapping> CategoryMappings { get; private set; } = new List<ProductCategoryMapping>();
    public ProductCode? Code { get; private set; }

    private Product() { }

    private Product(string name, string? description, Guid defaultUnitId)
    {
        Name = name.Trim();
        Description = string.IsNullOrEmpty(description?.Trim()) ? null : description.Trim();
        DefaultUnitId = defaultUnitId;
        ArchivedAt = null;
    }

    public static Product Create(string name, string? description, Guid defaultUnitId)
        => new(name, description, defaultUnitId);

    public void UpdateNameAndDescription(string newName, string? newDescription)
    {
        Name = newName.Trim();
        Description = string.IsNullOrEmpty(newDescription?.Trim()) ? null : newDescription.Trim();
    }

    public void SetDefaultUnit(Guid unitId)
    {
        DefaultUnitId = unitId;
    }

    public void Archive()
    {
        if (ArchivedAt == null)
            ArchivedAt = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        ArchivedAt = null;
    }

    public bool IsArchived => ArchivedAt.HasValue;
}
