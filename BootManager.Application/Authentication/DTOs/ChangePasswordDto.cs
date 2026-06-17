using BootManager.Core.Enums;

namespace BootManager.Application.Authentication.DTOs;

/// <summary>
/// Verzoek om eigen wachtwoord te wijzigen.
/// </summary>
public sealed class ChangePasswordDto
{
    /// <summary>Huidig wachtwoord voor verificatie.</summary>
    public string CurrentPassword { get; set; } = default!;

    /// <summary>Nieuw wachtwoord (minimaal 8 tekens).</summary>
    public string NewPassword { get; set; } = default!;

    /// <summary>Bevestiging van nieuw wachtwoord.</summary>
    public string ConfirmNewPassword { get; set; } = default!;
}

/// <summary>
/// Resultaat van wachtwoordwijziging.
/// </summary>
public sealed class ChangePasswordResultDto
{
    public bool Success { get; init; }
    public string? Message { get; init; }

    /// <summary>Nieuwe credentialversie wanneer succesvol.</summary>
    public int? NewCredentialVersion { get; init; }

    /// <summary>Weergavenaam van de gebruiker (benodigd voor cookievernieuwing).</summary>
    public string? DisplayName { get; init; }

    /// <summary>Rol van de gebruiker (benodigd voor cookievernieuwing).</summary>
    public LocalUserRole? Role { get; init; }
}
