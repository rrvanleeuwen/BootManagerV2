namespace BootManager.Application.Inventory.DTOs;

/// <summary>
/// DTO voor productcategorie.
/// </summary>
public class ProductCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string IconKey { get; set; } = default!;
    public bool IsArchived { get; set; }
}
