namespace BootManager.Core.Entities;

/// <summary>
/// Opslaggebied-entiteit: een container voor opslaglocaties aan boord.
/// Bevat verplichte unieke naam (hooflettergevoelig).
/// </summary>
public class StorageArea
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>Leesbare, unieke gebiedsnaam (trimmed, case-insensitive uniek, max 100 chars).</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Genormaliseerde naam voor uniqueness-checking (lowercase).</summary>
    public string NormalizedName { get; private set; } = default!;

    public ICollection<StorageLocation> Locations { get; private set; } = new List<StorageLocation>();

    private StorageArea() { } // Voor EF

    private StorageArea(string name)
    {
        Name = name.Trim();
        NormalizedName = name.Trim().ToLowerInvariant();
    }

    public static StorageArea Create(string name)
        => new(name);

    public void UpdateName(string newName)
    {
        Name = newName.Trim();
        NormalizedName = newName.Trim().ToLowerInvariant();
    }
}
