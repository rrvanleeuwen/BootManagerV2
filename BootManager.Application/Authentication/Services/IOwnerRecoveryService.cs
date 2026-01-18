using BootManager.Application.Authentication.DTOs;

namespace BootManager.Application.Authentication.Services;

public interface IOwnerRecoveryService
{
    // Restore access using a one-time backup code and set a new password in the same operation.
    Task<RestoreAccessResponseDto> RestoreWithBackupCodeAsync(string backupCode, string newPassword, CancellationToken ct = default);

    // Restore access using a master key import and set a new password in the same operation.
    Task<RestoreAccessResponseDto> RestoreWithMasterKeyAsync(string masterKey, string newPassword, CancellationToken ct = default);
}
