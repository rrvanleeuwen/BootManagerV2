namespace BootManager.Application.Inventory.DTOs;

/// <summary>
/// DTO voor product met alle bijbehorende gegevens.
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid DefaultUnitId { get; set; }
    public string? DefaultUnitName { get; set; }
    public Guid? ActiveCategoryId { get; set; }
    public string? ActiveCategoryName { get; set; }
    public string? ActiveCategoryIconKey { get; set; }
    public ProductCodeDto? Code { get; set; }
    public bool IsArchived { get; set; }
}
