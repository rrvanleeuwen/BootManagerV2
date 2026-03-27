using BootManager.Application.Authentication.DTOs;

namespace BootManager.Application.Authentication.Services;

/// <summary>
/// Beheerst het herstel van toegang wanneer de eigenaar zijn wachtwoord is vergeten.
/// </summary>
public interface IOwnerRecoveryService
{
    /// <summary>
    /// Herstel toegang met behulp van een eenmalig backup-code en stel een nieuw wachtwoord in.
    /// </summary>
    /// <param name="backupCode">De eerder gegenereerde backup-code.</param>
    /// <param name="newPassword">Het nieuwe wachtwoord.</param>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>RestoreAccessResponseDto met successtatus en eventuele foutdetails.</returns>
    Task<RestoreAccessResponseDto> RestoreWithBackupCodeAsync(string backupCode, string newPassword, CancellationToken ct = default);

    /// <summary>
    /// Herstel toegang met behulp van een master-key import en stel een nieuw wachtwoord in.
    /// </summary>
    /// <param name="masterKey">De master-key voor herstel.</param>
    /// <param name="newPassword">Het nieuwe wachtwoord.</param>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>RestoreAccessResponseDto met successtatus en eventuele foutdetails.</returns>
    Task<RestoreAccessResponseDto> RestoreWithMasterKeyAsync(string masterKey, string newPassword, CancellationToken ct = default);
}
