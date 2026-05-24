using System;

namespace BootManager.Application.Logbook.DTOs;

/// <summary>
/// Enum voor het resultaat van een upload-bewerking.
/// </summary>
public enum AttachmentUploadStatus
{
    /// <summary>Upload is gelukt.</summary>
    Success = 0,

    /// <summary>Bestand is te groot.</summary>
    FileTooLarge = 1,

    /// <summary>Ongeldig bestandstype.</summary>
    InvalidFileType = 2,

    /// <summary>Fout bij het opslaan van het bestand.</summary>
    StorageError = 3,

    /// <summary>Fout bij het opslaan van metadata in de database.</summary>
    DatabaseError = 4,

    /// <summary>Onbekende fout.</summary>
    UnknownError = 5
}

/// <summary>
/// DTO voor het resultaat van een bijlage-upload.
/// </summary>
public class AttachmentUploadResultDto
{
    /// <summary>Status van de upload-bewerking.</summary>
    public AttachmentUploadStatus Status { get; set; }

    /// <summary>Bericht over het resultaat (leeg als gelukt).</summary>
    public string? Message { get; set; }

    /// <summary>De nieuw geuploade bijlage (als upload gelukt is).</summary>
    public LogbookAttachmentDto? Attachment { get; set; }

    /// <summary>Geeft aan of de upload gelukt is.</summary>
    public bool Success => Status == AttachmentUploadStatus.Success;

    /// <summary>
    /// Maakt een succesvol resultaat met bijlage.
    /// </summary>
    public static AttachmentUploadResultDto SuccessResult(LogbookAttachmentDto attachment) =>
        new()
        {
            Status = AttachmentUploadStatus.Success,
            Attachment = attachment
        };

    /// <summary>
    /// Maakt een fout-resultaat.
    /// </summary>
    public static AttachmentUploadResultDto Error(AttachmentUploadStatus status, string message) =>
        new()
        {
            Status = status,
            Message = message
        };
}
