namespace BootManager.Application.Authentication.DTOs;

public sealed class ChangePinRequestDto
{
    public string CurrentPasswordOrPin { get; set; } = string.Empty; // authenticate with either
    public string NewPin { get; set; } = string.Empty;
    public string ConfirmNewPin { get; set; } = string.Empty;
}
