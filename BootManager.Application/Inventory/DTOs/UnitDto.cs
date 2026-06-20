namespace BootManager.Application.Inventory.DTOs;

/// <summary>
/// DTO voor eenheid.
/// </summary>
public class UnitDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsArchived { get; set; }
}
