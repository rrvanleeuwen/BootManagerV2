using BootManager.Application.Logbook.Services;
using BootManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BootManager.Infrastructure.Logbook;

/// <summary>
/// Infrastructure service voor database-consistente verwijdering van logboekregels met bijlagen.
/// </summary>
public class LogbookEntryDeletionService : ILogbookEntryDeletionService
{
    private const string DefaultStorageDirectory = "data/logbook-attachments";

    private readonly BootManagerDbContext _db;
    private readonly ILogger<LogbookEntryDeletionService> _logger;

    public LogbookEntryDeletionService(
        BootManagerDbContext db,
        ILogger<LogbookEntryDeletionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> DeleteEntryAndCollectAttachmentFilePathsAsync(
        int entryId,
        CancellationToken cancellationToken = default)
    {
        var entry = await _db.LogbookEntries.SingleOrDefaultAsync(e => e.Id == entryId, cancellationToken)
            ?? throw new InvalidOperationException($"Logboekregel met id {entryId} niet gevonden.");

        var storageDir = await GetStorageDirectoryAsync(cancellationToken);
        var attachments = await _db.LogbookAttachments
            .AsNoTracking()
            .Where(a => a.LogbookEntryId == entryId)
            .ToListAsync(cancellationToken);

        var filePaths = attachments
            .Select(a => Path.Combine(storageDir, a.StoredFileName))
            .Where(path =>
            {
                var safe = IsPathSafe(storageDir, path);
                if (!safe)
                {
                    _logger.LogWarning(
                        "Bijlagepad overgeslagen bij verwijderen van logboekregel {EntryId}: {FilePath}",
                        entryId,
                        path);
                }

                return safe;
            })
            .ToList();

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        _db.LogbookEntries.Remove(entry);
        await _db.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return filePaths;
    }

    private async Task<string> GetStorageDirectoryAsync(CancellationToken cancellationToken)
    {
        var settings = await _db.OperationalSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        var dir = settings == null || string.IsNullOrWhiteSpace(settings.LogbookAttachmentsDirectory)
            ? DefaultStorageDirectory
            : settings.LogbookAttachmentsDirectory;

        return Path.GetFullPath(dir);
    }

    private static bool IsPathSafe(string baseDirectory, string filePath)
    {
        try
        {
            var fullBase = Path.GetFullPath(baseDirectory);
            var fullPath = Path.GetFullPath(filePath);

            return fullPath.StartsWith(fullBase, StringComparison.OrdinalIgnoreCase) &&
                   fullPath.Length > fullBase.Length;
        }
        catch
        {
            return false;
        }
    }
}
