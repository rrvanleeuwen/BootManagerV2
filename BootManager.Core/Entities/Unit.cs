namespace BootManager.Core.Entities;

/// <summary>
/// Eenheid-entiteit: maat voor hoeveelheden van producten.
/// Bevat verplichte unieke naam.
/// Ondersteunt soft delete (archiveren) en heractiveren.
/// </summary>
public class Unit
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>Leesbare, unieke eenheidsnaam (trimmed, case-insensitive uniek, max 100 chars).</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Genormaliseerde naam voor uniqueness-checking (lowercase).</summary>
    public string NormalizedName { get; private set; } = default!;

    /// <summary>Soft delete: archivering datum; null = actief.</summary>
    public DateTime? ArchivedAt { get; private set; }

    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Unit() { }

    private Unit(string name)
    {
        Name = name.Trim();
        NormalizedName = name.Trim().ToLowerInvariant();
        ArchivedAt = null;
    }

    public static Unit Create(string name)
        => new(name);

    public void UpdateName(string newName)
    {
        Name = newName.Trim();
        NormalizedName = newName.Trim().ToLowerInvariant();
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
