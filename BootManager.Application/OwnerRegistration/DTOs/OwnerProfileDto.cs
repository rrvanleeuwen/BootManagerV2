namespace BootManager.Application.OwnerRegistration.DTOs;

public sealed class OwnerProfileDto
{
    public Guid OwnerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime CreatedUtc { get; init; }
}