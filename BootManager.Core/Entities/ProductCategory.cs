namespace BootManager.Core.Entities;

/// <summary>
/// Productcategorie-entiteit: classificatie voor producten.
/// Bevat verplichte unieke naam, optionele omschrijving en verplichte icoonsleutel.
/// Ondersteunt soft delete (archiveren) en heractiveren.
/// </summary>
public class ProductCategory
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>Leesbare, unieke categorienaam (trimmed, case-insensitive uniek, max 100 chars).</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Genormaliseerde naam voor uniqueness-checking (lowercase).</summary>
    public string NormalizedName { get; private set; } = default!;

    /// <summary>Optionele categorieomschrijving (max 500 chars).</summary>
    public string? Description { get; private set; }

    /// <summary>Icoonsleutel uit vaste set (bijv. 'beverage', 'part', 'tool').</summary>
    public string IconKey { get; private set; } = default!;

    /// <summary>Soft delete: archivering datum; null = actief.</summary>
    public DateTime? ArchivedAt { get; private set; }

    public ICollection<ProductCategoryMapping> ProductMappings { get; private set; } = new List<ProductCategoryMapping>();

    private ProductCategory() { }

    private ProductCategory(string name, string? description, string iconKey)
    {
        Name = name.Trim();
        NormalizedName = name.Trim().ToLowerInvariant();
        Description = string.IsNullOrEmpty(description?.Trim()) ? null : description.Trim();
        IconKey = iconKey;
        ArchivedAt = null;
    }

    public static ProductCategory Create(string name, string? description, string iconKey)
        => new(name, description, iconKey);

    public void UpdateNameAndDescription(string newName, string? newDescription)
    {
        Name = newName.Trim();
        NormalizedName = newName.Trim().ToLowerInvariant();
        Description = string.IsNullOrEmpty(newDescription?.Trim()) ? null : newDescription.Trim();
    }

    public void UpdateIconKey(string newIconKey)
    {
        IconKey = newIconKey;
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
