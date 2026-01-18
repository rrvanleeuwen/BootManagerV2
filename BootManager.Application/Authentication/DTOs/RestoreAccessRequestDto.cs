namespace BootManager.Application.Authentication.DTOs;

public enum RecoveryMode
{
    BackupCode,
    MasterKey
}

public sealed class RestoreAccessRequestDto
{
    public RecoveryMode Mode { get; set; }
    public string? BackupCode { get; set; }
    public string? MasterKey { get; set; }
}
