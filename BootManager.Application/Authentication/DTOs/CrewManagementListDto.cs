namespace BootManager.Application.Authentication.DTOs;

/// <summary>
/// DTO voor Crew-beheer met volledige informatie inclusief werkelijke actieve status.
/// </summary>
public sealed class CrewManagementListDto
{
    /// <summary>Unieke gebruikers-ID.</summary>
    public Guid Id { get; init; }

    /// <summary>Leesbare accountnaam.</summary>
    public string DisplayName { get; init; } = default!;

    /// <summary>Werkelijke actieve status.</summary>
    public bool IsActive { get; init; }

    /// <summary>Vlag: wachtwoord moet worden gewijzigd.</summary>
    public bool PasswordChangeRequired { get; init; }
}
