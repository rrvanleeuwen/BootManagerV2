namespace BootManager.Application.Storage.DTOs;

public class StorageLocationDetailDto
{
    public Guid Id { get; set; }
    public string AreaName { get; set; } = default!;
    public string LocationName { get; set; } = default!;
    public string? Description { get; set; }
    public string? QrValue { get; set; }
}
