namespace BootManager.Core.Entities;

/// <summary>
/// Opslaglocatie-entiteit: een fysieke plek binnen een opslaggebied.
/// Bevat verplichte naam en optionele beschrijving.
/// Naam is uniek binnen hetzelfde gebied.
/// </summary>
public class StorageLocation
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid StorageAreaId { get; private set; }

    /// <summary>Leesbare locatienaam (trimmed, case-insensitive uniek per gebied, max 100 chars).</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Genormaliseerde naam voor uniqueness-checking (lowercase).</summary>
    public string NormalizedName { get; private set; } = default!;

    /// <summary>Optionele beschrijving (max 500 chars).</summary>
    public string? Description { get; private set; }

    public StorageArea StorageArea { get; private set; } = default!;

    private StorageLocation() { } // Voor EF

    private StorageLocation(Guid storageAreaId, string name, string? description)
    {
        StorageAreaId = storageAreaId;
        Name = name.Trim();
        NormalizedName = name.Trim().ToLowerInvariant();
        Description = string.IsNullOrEmpty(description?.Trim()) ? null : description.Trim();
    }

    public static StorageLocation Create(Guid storageAreaId, string name, string? description = null)
        => new(storageAreaId, name, description);

    public void UpdateNameAndDescription(string newName, string? newDescription)
    {
        Name = newName.Trim();
        NormalizedName = newName.Trim().ToLowerInvariant();
        Description = string.IsNullOrEmpty(newDescription?.Trim()) ? null : newDescription.Trim();
    }

    public void MoveToArea(Guid newStorageAreaId)
    {
        StorageAreaId = newStorageAreaId;
    }
}
