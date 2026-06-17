namespace BootManager.Application.Authentication.DTOs;

/// <summary>
/// Resultaat van Crew-account-aanmaak.
/// </summary>
public sealed class CreateCrewResultDto
{
    public bool Success { get; init; }
    public Guid? CrewId { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Resultaat van Crew-wachtwoord-reset.
/// </summary>
public sealed class ResetCrewPasswordResultDto
{
    public bool Success { get; init; }
    public string? Message { get; init; }
}
