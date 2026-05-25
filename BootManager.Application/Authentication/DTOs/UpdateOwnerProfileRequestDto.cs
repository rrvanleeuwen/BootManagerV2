using System.ComponentModel.DataAnnotations;

namespace BootManager.Application.Authentication.DTOs;

/// <summary>
/// Request DTO voor het bijwerken van eigenaargegevens (naam en e-mail).
/// </summary>
public sealed class UpdateOwnerProfileRequestDto
{
    /// <summary>
    /// De nieuwe naam van de eigenaar (verplicht).
    /// </summary>
    [Required(ErrorMessage = "Naam is verplicht.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Naam moet tussen 1 en 100 tekens zijn.")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Het nieuwe e-mailadres van de eigenaar (optioneel).
    /// </summary>
    [EmailAddress(ErrorMessage = "Ongeldig e-mailadres.")]
    [StringLength(200)]
    public string? Email { get; init; }
}
