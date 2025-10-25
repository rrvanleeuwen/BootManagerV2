using System.ComponentModel.DataAnnotations;

namespace BootManager.Application.OwnerRegistration.DTOs;

public sealed class RegisterOwnerRequestDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool GenerateRecoveryCode { get; set; } = true;
}