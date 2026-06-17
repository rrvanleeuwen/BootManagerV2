using System.Security.Claims;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace BootManager.Web.Services;

/// <summary>
/// Revalidates the authenticated user inside existing Blazor Server circuits.
/// Cookie validation protects new HTTP requests; this closes the same gap for
/// already-open interactive sessions after password reset or account disable.
/// </summary>
public sealed class LocalUserRevalidatingAuthenticationStateProvider(
    ILoggerFactory loggerFactory,
    IRepository<LocalUser> users)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(5);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        var principal = authenticationState.User;
        if (principal.Identity?.IsAuthenticated != true)
            return true;

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var credentialVersionClaim = principal.FindFirst("bm.credential_version")?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId) ||
            !int.TryParse(credentialVersionClaim, out var credentialVersion))
        {
            return false;
        }

        var user = await users.GetByIdAsync(userId, cancellationToken);
        return user is not null &&
               user.IsActive &&
               user.CredentialVersion == credentialVersion;
    }
}
