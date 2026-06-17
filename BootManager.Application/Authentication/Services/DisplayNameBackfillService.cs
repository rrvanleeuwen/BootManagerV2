using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BootManager.Application.Authentication.Services;

/// <summary>
/// Post-migratie service die DisplayName vullingt vanuit versleutelde payload voor Owner-accounts.
/// Wordt eenmalig aangeroepen na een migratie van OwnerProfile naar LocalUser.
/// </summary>
public sealed class DisplayNameBackfillService
{
    private readonly IRepository<LocalUser> _repo;
    private readonly IEncryptionService _encryption;
    private readonly ILogger<DisplayNameBackfillService> _logger;

    public DisplayNameBackfillService(
        IRepository<LocalUser> repo,
        IEncryptionService encryption,
        ILogger<DisplayNameBackfillService> logger)
    {
        _repo = repo;
        _encryption = encryption;
        _logger = logger;
    }

    /// <summary>
    /// Backfill DisplayName voor Owner-accounts van de versleutelde payload.
    /// Fallback naar "Owner" wanneer decryptie mislukt of naam ontbreekt.
    /// </summary>
    public async Task BackfillDisplayNamesAsync(CancellationToken ct = default)
    {
        var users = await _repo.ListAsync(u => u.DisplayName == "Owner", ct); // Alleen de gemigeerde met temp displaynaam
        if (!users.Any())
        {
            _logger.LogInformation("No users found for DisplayName backfill.");
            return;
        }

        var updated = 0;
        var fallback = 0;

        foreach (var user in users)
        {
            var displayName = "Owner"; // Safe fallback

            try
            {
                // Decrypt payload
                if (user.EncryptedProfilePayload.Length > 0)
                {
                    var json = _encryption.Decrypt(user.EncryptedProfilePayload);
                    var payload = JsonSerializer.Deserialize<JsonElement>(json);
                    if (payload.TryGetProperty("Name", out var nameElem) && nameElem.ValueKind == JsonValueKind.String)
                    {
                        var name = nameElem.GetString();
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            displayName = name;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to decrypt profile payload for user {UserId}; using fallback.", user.Id);
                fallback++;
            }

            // Update DisplayName
            user.UpdateDisplayName(displayName, DateTime.UtcNow);
            await _repo.UpdateAsync(user, ct);
            updated++;
        }

        _logger.LogInformation("DisplayName backfill completed: {Updated} updated, {Fallback} fallback.", updated, fallback);
    }
}
