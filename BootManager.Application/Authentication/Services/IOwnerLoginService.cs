using BootManager.Application.Authentication.DTOs;

namespace BootManager.Application.Authentication.Services;

/// <summary>
/// Valideert eigenaar-credentials (wachtwoord of pincode). Plaatsen van cookies gebeurt in de Web-laag.
/// </summary>
public interface IOwnerLoginService
{
    Task<LoginResultDto> ValidateAsync(LoginRequestDto request, CancellationToken ct = default);
}