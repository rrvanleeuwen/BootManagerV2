namespace BootManager.Application.Inventory.DTOs;

/// <summary>
/// DTO voor gekoppelde productcode.
/// </summary>
public class ProductCodeDto
{
    public Guid Id { get; set; }
    public string Value { get; set; } = default!;
    public string Format { get; set; } = default!;
}
