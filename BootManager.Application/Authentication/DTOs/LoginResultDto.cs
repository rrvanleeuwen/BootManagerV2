using BootManager.Core.Enums;

namespace BootManager.Application.Authentication.DTOs;

/// <summary>
/// Resultaat van credentialvalidatie met gebruiksgegevens.
/// </summary>
public sealed class LoginResultDto
{
    public bool Success { get; init; }
    public Guid? UserId { get; init; }
    public string? DisplayName { get; init; }
    public LocalUserRole? Role { get; init; }
    public int CredentialVersion { get; init; }
    public bool PasswordChangeRequired { get; init; }
    public string? Message { get; init; }

    /// <summary>Achterwaartse compatibiliteit: OwnerId is alias voor UserId.</summary>
    [Obsolete("Gebruik UserId in plaats daarvan.")]
    public Guid? OwnerId => UserId;
}