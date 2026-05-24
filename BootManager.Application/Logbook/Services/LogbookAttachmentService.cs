using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BootManager.Application.Logbook.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BootManager.Application.Logbook.Services;

/// <summary>
/// Service voor beheer van logboekbijlagen.
/// Handelt upload, download en verwijdering af met veiligheidsmaatregelen tegen path-traversal.
/// </summary>
public class LogbookAttachmentService : ILogbookAttachmentService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB
    private const string DefaultStorageDirectory = "data/logbook-attachments";
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "text/plain",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };

    private readonly IRepository<LogbookEntry> _entryRepository;
    private readonly IRepository<LogbookAttachment> _attachmentRepository;
    private readonly IRepository<Core.Entities.OperationalSettings> _settingsRepository;
    private readonly ILogger<LogbookAttachmentService> _logger;

    /// <summary>
    /// Initialiseert een nieuw exemplaar van <see cref="LogbookAttachmentService"/>.
    /// </summary>
    public LogbookAttachmentService(
        IRepository<LogbookEntry> entryRepository,
        IRepository<LogbookAttachment> attachmentRepository,
        IRepository<Core.Entities.OperationalSettings> settingsRepository,
        ILogger<LogbookAttachmentService> logger)
    {
        _entryRepository = entryRepository;
        _attachmentRepository = attachmentRepository;
        _settingsRepository = settingsRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<AttachmentUploadResultDto> UploadAsync(
        int entryId,
        Stream fileStream,
        string originalFileName,
        string contentType,
        string? description = null,
        CancellationToken ct = default)
    {
        try
        {
            // Controleer of logboekregel bestaat
            var entries = await _entryRepository.ListAsync(e => e.Id == entryId, ct);
            var entry = entries.FirstOrDefault();
            if (entry == null)
                return AttachmentUploadResultDto.Error(
                    AttachmentUploadStatus.DatabaseError,
                    "Logboekregel niet gevonden.");

            // Valideer bestandsgrootte
            if (fileStream.Length > MaxFileSizeBytes)
            {
                _logger.LogWarning("Upload afgewezen: bestand te groot ({Size} bytes) voor entry {EntryId}",
                    fileStream.Length, entryId);
                return AttachmentUploadResultDto.Error(
                    AttachmentUploadStatus.FileTooLarge,
                    $"Bestand is te groot. Maximum {MaxFileSizeBytes / 1024 / 1024} MB toegestaan.");
            }

            // Valideer content-type
            if (!IsAllowedContentType(contentType))
            {
                _logger.LogWarning("Upload afgewezen: ongeldig content-type '{ContentType}' voor entry {EntryId}",
                    contentType, entryId);
                return AttachmentUploadResultDto.Error(
                    AttachmentUploadStatus.InvalidFileType,
                    "Dit bestandstype is niet toegestaan.");
            }

            // Krijg opslagdirectory
            var storageDir = await GetStorageDirectoryAsync(ct);
            if (string.IsNullOrEmpty(storageDir))
                return AttachmentUploadResultDto.Error(
                    AttachmentUploadStatus.StorageError,
                    "Opslagdirectory is niet geconfigureerd.");

            // Zorg dat directory bestaat
            try
            {
                if (!Directory.Exists(storageDir))
                    Directory.CreateDirectory(storageDir);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij het aanmaken van directory {Directory}", storageDir);
                return AttachmentUploadResultDto.Error(
                    AttachmentUploadStatus.StorageError,
                    "Kan opslagdirectory niet aanmaken.");
            }

            // Genereer veilige bestandsnaam
            var storedFileName = GenerateStoredFileName(originalFileName);
            var filePath = Path.Combine(storageDir, storedFileName);

            // Valideer pad tegen path-traversal
            if (!IsPathSafe(storageDir, filePath))
            {
                _logger.LogError("Path-traversal attempt detected for entry {EntryId}: {FileName}", 
                    entryId, storedFileName);
                return AttachmentUploadResultDto.Error(
                    AttachmentUploadStatus.StorageError,
                    "Ongeldig bestandspad.");
            }

            // Sla bestand op
            try
            {
                using (var fileWriter = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await fileStream.CopyToAsync(fileWriter, ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij het opslaan van bestand {FilePath} voor entry {EntryId}",
                    filePath, entryId);
                return AttachmentUploadResultDto.Error(
                    AttachmentUploadStatus.StorageError,
                    "Kan bestand niet opslaan.");
            }

            // Maak attachment-record
            var attachment = new LogbookAttachment(
                entryId,
                originalFileName,
                storedFileName,
                contentType,
                fileStream.Length,
                description);

            try
            {
                await _attachmentRepository.AddAsync(attachment, ct);
                _logger.LogInformation("Bijlage {StoredFileName} succesvol geüpload voor entry {EntryId}",
                    storedFileName, entryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij het opslaan van attachment-metadata voor entry {EntryId}",
                    entryId);
                // Probeer bestand op te ruimen
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx, "Fout bij cleanup van bestand {FilePath}", filePath);
                }
                return AttachmentUploadResultDto.Error(
                    AttachmentUploadStatus.DatabaseError,
                    "Kan bijlage niet opslaan in database.");
            }

            var dto = new LogbookAttachmentDto
            {
                Id = attachment.Id,
                OriginalFileName = attachment.OriginalFileName,
                ContentType = attachment.ContentType,
                SizeBytes = attachment.SizeBytes,
                UploadedAtUtc = attachment.UploadedAtUtc,
                Description = attachment.Description
            };

            return AttachmentUploadResultDto.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Onbekende fout bij upload voor entry {EntryId}", entryId);
            return AttachmentUploadResultDto.Error(
                AttachmentUploadStatus.UnknownError,
                "Er is een onbekende fout opgetreden.");
        }
    }

    /// <inheritdoc />
    public async Task<(Stream? Stream, string? OriginalFileName, string? ContentType)> DownloadAsync(
        int attachmentId,
        CancellationToken ct = default)
    {
        try
        {
            var attachments = await _attachmentRepository.ListAsync(a => a.Id == attachmentId, ct);
            var attachment = attachments.FirstOrDefault();
            if (attachment == null)
            {
                _logger.LogWarning("Download aangevraagd voor niet-bestaande attachment {AttachmentId}", attachmentId);
                return (null, null, null);
            }

            var storageDir = await GetStorageDirectoryAsync(ct);
            if (string.IsNullOrEmpty(storageDir))
            {
                _logger.LogError("Opslagdirectory niet geconfigureerd bij download van attachment {AttachmentId}",
                    attachmentId);
                return (null, null, null);
            }

            var filePath = Path.Combine(storageDir, attachment.StoredFileName);

            // Valideer pad tegen path-traversal
            if (!IsPathSafe(storageDir, filePath))
            {
                _logger.LogError("Path-traversal attempt detected for download of attachment {AttachmentId}",
                    attachmentId);
                return (null, null, null);
            }

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Bestand niet gevonden voor attachment {AttachmentId}: {FilePath}",
                    attachmentId, filePath);
                return (null, null, null);
            }

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return (stream, attachment.OriginalFileName, attachment.ContentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij download van attachment {AttachmentId}", attachmentId);
            return (null, null, null);
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int attachmentId, CancellationToken ct = default)
    {
        try
        {
            var attachments = await _attachmentRepository.ListAsync(a => a.Id == attachmentId, ct);
            var attachment = attachments.FirstOrDefault();
            if (attachment == null)
            {
                _logger.LogWarning("Delete aangevraagd voor niet-bestaande attachment {AttachmentId}", attachmentId);
                return false;
            }

            var storageDir = await GetStorageDirectoryAsync(ct);
            if (!string.IsNullOrEmpty(storageDir))
            {
                var filePath = Path.Combine(storageDir, attachment.StoredFileName);

                // Valideer pad tegen path-traversal
                if (IsPathSafe(storageDir, filePath))
                {
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                            _logger.LogInformation("Bestand verwijderd: {FilePath}", filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Fout bij verwijdering van bestand {FilePath}", filePath);
                        // Niet fataal; metadata kan toch verwijderd worden
                    }
                }
            }

            // Verwijder metadata
            await _attachmentRepository.DeleteAsync(attachment, ct);
            _logger.LogInformation("Attachment {AttachmentId} verwijderd", attachmentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij verwijdering van attachment {AttachmentId}", attachmentId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<LogbookAttachmentDto>> GetAttachmentsAsync(
        int entryId,
        CancellationToken ct = default)
    {
        try
        {
            var attachments = await _attachmentRepository.ListAsync(
                a => a.LogbookEntryId == entryId,
                ct);

            return attachments
                .OrderByDescending(a => a.UploadedAtUtc)
                .Select(a => new LogbookAttachmentDto
                {
                    Id = a.Id,
                    OriginalFileName = a.OriginalFileName,
                    ContentType = a.ContentType,
                    SizeBytes = a.SizeBytes,
                    UploadedAtUtc = a.UploadedAtUtc,
                    Description = a.Description
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij ophalen van bijlagen voor entry {EntryId}", entryId);
            return Enumerable.Empty<LogbookAttachmentDto>();
        }
    }

    /// <inheritdoc />
    public async Task<int> GetAttachmentCountAsync(int entryId, CancellationToken ct = default)
    {
        try
        {
            var attachments = await _attachmentRepository.ListAsync(
                a => a.LogbookEntryId == entryId,
                ct);
            return attachments.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij ophalen van aantal bijlagen voor entry {EntryId}", entryId);
            return 0;
        }
    }

    // --- Helper methods ---

    private bool IsAllowedContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        return AllowedContentTypes.Contains(contentType.Split(';')[0].Trim());
    }

    private string GenerateStoredFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        return fileName;
    }

    private bool IsPathSafe(string baseDirectory, string filePath)
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

    private async Task<string?> GetStorageDirectoryAsync(CancellationToken ct)
    {
        try
        {
            var settings = await _settingsRepository.SingleOrDefaultAsync(null, ct);
            if (settings != null)
            {
                var dir = string.IsNullOrWhiteSpace(settings.LogbookAttachmentsDirectory)
                    ? DefaultStorageDirectory
                    : settings.LogbookAttachmentsDirectory;

                // Normaliseer het pad
                dir = Path.GetFullPath(dir);
                return dir;
            }

            return Path.GetFullPath(DefaultStorageDirectory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij ophalen van opslagdirectory uit instellingen");
            return null;
        }
    }
}
