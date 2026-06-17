using BootManager.Application.Authentication.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;

namespace BootManager.Application.Authentication.Services;

/// <summary>
/// Implementeert lokale gebruiker-beheer: Crew aanmaken, resetten, uitschakelen.
/// </summary>
public sealed class LocalUserManagementService : ILocalUserManagementService
{
    private readonly IRepository<LocalUser> _repo;
    private readonly IPasswordHasher _hasher;

    public LocalUserManagementService(IRepository<LocalUser> repo, IPasswordHasher hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public async Task<List<ActiveUsersListDto>> GetActiveUsersAsync(CancellationToken ct = default)
    {
        var users = await _repo.ListAsync(u => u.IsActive, ct);
        return users
            .OrderBy(u => u.DisplayName)
            .Select(u => new ActiveUsersListDto { Id = u.Id, DisplayName = u.DisplayName })
            .ToList();
    }

    public async Task<List<CrewManagementListDto>> GetAllCrewAsync(CancellationToken ct = default)
    {
        var crew = await _repo.ListAsync(u => u.Role == LocalUserRole.Crew, ct);
        return crew
            .OrderBy(u => u.DisplayName)
            .Select(u => new CrewManagementListDto
            {
                Id = u.Id,
                DisplayName = u.DisplayName,
                IsActive = u.IsActive,
                PasswordChangeRequired = u.PasswordChangeRequired
            })
            .ToList();
    }

    public async Task<CreateCrewResultDto> CreateCrewAsync(string displayName, string temporaryPassword, CancellationToken ct = default)
    {
        // Validatie: displayName trimmed, max 100, uniek (case-insensitive)
        var trimmed = displayName?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > 100)
        {
            return new CreateCrewResultDto { Success = false, Message = "Accountnaam moet tussen 1 en 100 tekens zijn." };
        }

        // Validatie: temporaryPassword moet minstens 8 tekens zijn
        if (string.IsNullOrEmpty(temporaryPassword) || temporaryPassword.Length < 8)
        {
            return new CreateCrewResultDto { Success = false, Message = "Wachtwoord moet minstens 8 tekens lang zijn." };
        }

        // Uniekheids-check
        var normalized = trimmed.ToLowerInvariant();
        var existing = await _repo.ListAsync(u => u.NormalizedName == normalized, ct);
        if (existing.Any())
        {
            return new CreateCrewResultDto { Success = false, Message = "Deze accountnaam bestaat al." };
        }

        // Wachtwoord hashen
        var (hash, salt, algo) = _hasher.Hash(temporaryPassword);
        var now = DateTime.UtcNow;

        var crew = LocalUser.Create(
            displayName: trimmed,
            role: LocalUserRole.Crew,
            passwordHash: hash,
            passwordSalt: salt,
            hashAlgorithm: algo,
            encryptedProfilePayload: Array.Empty<byte>(),
            encryptionVersion: 1,
            createdUtc: now,
            passwordChangeRequired: true,
            onboardingCompleted: true // Crew doet geen onboarding
        );

        await _repo.AddAsync(crew, ct);

        return new CreateCrewResultDto { Success = true, CrewId = crew.Id };
    }

    public async Task<ResetCrewPasswordResultDto> ResetCrewPasswordAsync(Guid crewId, string newTemporaryPassword, CancellationToken ct = default)
    {
        var user = await _repo.SingleOrDefaultAsync(u => u.Id == crewId, ct);
        if (user is null)
        {
            return new ResetCrewPasswordResultDto { Success = false, Message = "Gebruiker niet gevonden." };
        }

        if (user.Role != LocalUserRole.Crew)
        {
            return new ResetCrewPasswordResultDto { Success = false, Message = "Alleen Crew-accounts kunnen worden gereset." };
        }

        if (string.IsNullOrEmpty(newTemporaryPassword) || newTemporaryPassword.Length < 8)
        {
            return new ResetCrewPasswordResultDto { Success = false, Message = "Wachtwoord moet minstens 8 tekens lang zijn." };
        }

        var (hash, salt, algo) = _hasher.Hash(newTemporaryPassword);
        var now = DateTime.UtcNow;

        user.UpdatePassword(hash, salt, algo, now);
        user.SetPasswordChangeRequired(true, now);

        await _repo.UpdateAsync(user, ct);
        return new ResetCrewPasswordResultDto { Success = true };
    }

    public async Task<bool> DisableCrewAsync(Guid crewId, CancellationToken ct = default)
    {
        var user = await _repo.SingleOrDefaultAsync(u => u.Id == crewId, ct);
        if (user is null || user.Role != LocalUserRole.Crew)
        {
            return false;
        }

        user.SetActive(false, DateTime.UtcNow);
        await _repo.UpdateAsync(user, ct);
        return true;
    }

    public async Task<bool> ReactivateCrewAsync(Guid crewId, CancellationToken ct = default)
    {
        var user = await _repo.SingleOrDefaultAsync(u => u.Id == crewId, ct);
        if (user is null || user.Role != LocalUserRole.Crew)
        {
            return false;
        }

        user.SetActive(true, DateTime.UtcNow);
        await _repo.UpdateAsync(user, ct);
        return true;
    }

    public async Task<bool> UpdateOwnerDisplayNameAsync(Guid ownerId, string newName, CancellationToken ct = default)
    {
        var user = await _repo.SingleOrDefaultAsync(u => u.Id == ownerId && u.Role == LocalUserRole.Owner, ct);
        if (user is null)
        {
            return false;
        }

        var trimmed = newName?.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.Length > 100)
        {
            return false;
        }

        // Uniekheids-check (behalve voor de user zelf)
        var normalized = trimmed.ToLowerInvariant();
        var existing = await _repo.ListAsync(u => u.NormalizedName == normalized && u.Id != ownerId, ct);
        if (existing.Any())
        {
            return false;
        }

        user.UpdateDisplayName(trimmed, DateTime.UtcNow);
        await _repo.UpdateAsync(user, ct);
        return true;
    }
}
