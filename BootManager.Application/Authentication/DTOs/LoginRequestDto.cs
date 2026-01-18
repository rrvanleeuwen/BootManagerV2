namespace BootManager.Application.Authentication.DTOs;

/// <summary>
/// Loginverzoek: de eigenaar kan inloggen met óf wachtwoord óf pincode.
/// Omdat er slechts één eigenaar is, is geen gebruikersnaam/e-mail nodig.
/// </summary>
public sealed class LoginRequestDto
{
    public string? Password { get; set; }
    public string? Pin { get; set; }
    public bool RememberMe { get; set; } = false;
}