namespace BootManager.Application.Inventory.DTOs;

/// <summary>
/// DTO voor voorraadregel op een locatie.
/// </summary>
public class StockDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid StorageLocationId { get; set; }
    public string ProductName { get; set; } = default!;
    public string StorageAreaName { get; set; } = default!;
    public string StorageLocationName { get; set; } = default!;
    public decimal Quantity { get; set; }
    public string DefaultUnitName { get; set; } = default!;
}
