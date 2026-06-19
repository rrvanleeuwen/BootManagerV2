using BootManager.Core.Enums;

namespace BootManager.Application.Storage.DTOs;

public class StorageLocationOverviewDto
{
    public Guid Id { get; set; }
    public string AreaName { get; set; } = default!;
    public string LocationName { get; set; } = default!;
    public string? QrValue { get; set; }
    public TagStatus TagStatus { get; set; }
}
