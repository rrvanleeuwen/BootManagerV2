namespace BootManager.Application.Storage.DTOs;

public class StorageLocationDto
{
    public Guid Id { get; set; }
    public Guid StorageAreaId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}
