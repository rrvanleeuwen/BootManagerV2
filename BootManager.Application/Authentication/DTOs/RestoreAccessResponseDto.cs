namespace BootManager.Application.Authentication.DTOs;

public sealed class RestoreAccessResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    // Plaintext new recovery code shown once to the user after successful restore
    public string? NewRecoveryCodePlain { get; set; }
}
