namespace BootManager.Application.Authentication.DTOs;

/// <summary>
/// Response DTO voor het ophalen van eigenaargegevens (naam en e-mail).
/// </summary>
public sealed class GetOwnerProfileResponseDto
{
    /// <summary>
    /// De naam van de eigenaar.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Het e-mailadres van de eigenaar (optioneel).
    /// </summary>
    public string Email { get; init; } = string.Empty;
}
