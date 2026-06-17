using BootManager.Application.Authentication.DTOs;

namespace BootManager.Application.Authentication.Services;

/// <summary>
/// Service voor accountbewerking (wachtwoordwijziging) voor ingelogde Owner/Crew.
/// </summary>
public interface IAccountService
{
    /// <summary>Wijzig het wachtwoord van de huidig ingelogde gebruiker.</summary>
    Task<ChangePasswordResultDto> ChangePasswordAsync(Guid userId, ChangePasswordDto request, CancellationToken ct = default);
}
