namespace BootManager.Application.Logbook.Services;

/// <summary>
/// Verwijdert een logboekregel database-consistent en retourneert daarna de gekoppelde bijlagebestanden voor fysieke cleanup.
/// </summary>
public interface ILogbookEntryDeletionService
{
    /// <summary>
    /// Verwijdert de logboekregel en gekoppelde bijlage-metadata binnen een database-transactie.
    /// Retourneert pas na succesvolle commit de veilige, volledige bestandspaden van gekoppelde bijlagen.
    /// </summary>
    Task<IReadOnlyList<string>> DeleteEntryAndCollectAttachmentFilePathsAsync(
        int entryId,
        CancellationToken cancellationToken = default);
}
