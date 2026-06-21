using BootManager.Core.Entities;

namespace BootManager.Application.Inventory.DTOs;

/// <summary>
/// DTO voor voorraadbijzonderheid (mutatie, telling, correctie).
/// </summary>
public class StockMutationDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid StorageLocationId { get; set; }
    public StockMutationType MutationType { get; set; }
    public decimal OldQuantity { get; set; }
    public decimal NewQuantity { get; set; }
    public DateTime MutatedAt { get; set; }
    public Guid UserId { get; set; }
    public string? Note { get; set; }
    public string ProductName { get; set; } = default!;
    public string StorageAreaName { get; set; } = default!;
    public string StorageLocationName { get; set; } = default!;
    public string DefaultUnitName { get; set; } = default!;
    public string UserDisplayName { get; set; } = default!;
}
