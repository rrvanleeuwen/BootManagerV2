namespace BootManager.Application.Authentication.DTOs;

/// <summary>
/// Anonieme lijst van actieve gebruikers voor de account-selector.
/// Bevat uitsluitend Id en DisplayName; geen rol, hash, of profiel-informatie.
/// </summary>
public sealed class ActiveUsersListDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = default!;
}
