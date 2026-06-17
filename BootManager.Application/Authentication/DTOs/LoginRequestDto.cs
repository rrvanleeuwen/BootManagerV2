namespace BootManager.Application.Authentication.DTOs;

/// <summary>
/// Loginverzoek: de eigenaar kan inloggen met �f wachtwoord �f pincode.
/// Omdat er slechts ��n eigenaar is, is geen gebruikersnaam/e-mail nodig.
/// </summary>
public sealed class LoginRequestDto
{
    /// <summary>Lokale gebruiker-id.</summary>
    public Guid? UserId { get; set; }

    /// <summary>Wachtwoord.</summary>
    public string? Password { get; set; }

    /// <summary>Optionele pincode (legacy).</summary>
    public string? Pin { get; set; }

    /// <summary>Persistente cookie ("Ingelogd blijven").</summary>
    public bool RememberMe { get; set; } = false;
}