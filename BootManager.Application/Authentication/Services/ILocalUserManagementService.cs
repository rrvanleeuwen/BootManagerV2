namespace BootManager.Application.Authentication.Services;

/// <summary>
/// Service voor Owner-beheer van lokale Crew-accounts.
/// </summary>
public interface ILocalUserManagementService
{
    /// <summary>Haal actieve gebruikers op voor de account-selector (zonder inactieve accounts).</summary>
    Task<List<BootManager.Application.Authentication.DTOs.ActiveUsersListDto>> GetActiveUsersAsync(CancellationToken ct = default);

    /// <summary>Haal alle Crew-accounts op (inclusief inactieve) voor Owner-beheer.</summary>
    Task<List<BootManager.Application.Authentication.DTOs.CrewManagementListDto>> GetAllCrewAsync(CancellationToken ct = default);

    /// <summary>Maak een nieuwe Crew-gebruiker aan met een tijdelijk wachtwoord.</summary>
    Task<BootManager.Application.Authentication.DTOs.CreateCrewResultDto> CreateCrewAsync(string displayName, string temporaryPassword, CancellationToken ct = default);

    /// <summary>Zet het wachtwoord van een Crew-gebruiker naar een nieuw tijdelijk wachtwoord en mark als wijzigingsverplicht.</summary>
    Task<BootManager.Application.Authentication.DTOs.ResetCrewPasswordResultDto> ResetCrewPasswordAsync(Guid crewId, string newTemporaryPassword, CancellationToken ct = default);

    /// <summary>Deactiveer een Crew-account.</summary>
    Task<bool> DisableCrewAsync(Guid crewId, CancellationToken ct = default);

    /// <summary>Activeer een Crew-account opnieuw.</summary>
    Task<bool> ReactivateCrewAsync(Guid crewId, CancellationToken ct = default);

    /// <summary>Update de displaynaam van de Owner.</summary>
    Task<bool> UpdateOwnerDisplayNameAsync(Guid ownerId, string newName, CancellationToken ct = default);
}
