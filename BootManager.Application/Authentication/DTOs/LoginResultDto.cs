namespace BootManager.Application.Authentication.DTOs;

/// <summary>
/// Resultaat van credentialvalidatie. Geen cookie/loginsessie hier; uitsluitend validatie.
/// </summary>
public sealed class LoginResultDto
{
    public bool Success { get; init; }
    public Guid? OwnerId { get; init; }
    public string? Message { get; init; }
}