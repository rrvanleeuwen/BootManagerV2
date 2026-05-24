using System;

namespace BootManager.Core.Entities;

/// <summary>
/// Domein-entiteit voor een bijlage bij een logboekregel.
/// Bevat metadata over het bestand en een veilige gegenereerde opslagnaam.
/// </summary>
public class LogbookAttachment
{
    /// <summary>
    /// Unieke identificator van de bijlage.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Verwijzing naar de bijbehorende logboekregel.
    /// </summary>
    public int LogbookEntryId { get; private set; }

    /// <summary>
    /// Oorspronkelijke bestandsnaam zoals geüpload (niet gebruikt als fysiek pad).
    /// </summary>
    public string OriginalFileName { get; private set; } = default!;

    /// <summary>
    /// Gegenereerde veilige bestandsnaam voor opslag (bijv. GUID + extensie).
    /// </summary>
    public string StoredFileName { get; private set; } = default!;

    /// <summary>
    /// MIME-type van het bestand (bijv. "application/pdf").
    /// </summary>
    public string ContentType { get; private set; } = default!;

    /// <summary>
    /// Bestandsgrootte in bytes.
    /// </summary>
    public long SizeBytes { get; private set; }

    /// <summary>
    /// Datum en tijd (UTC) van upload.
    /// </summary>
    public DateTime UploadedAtUtc { get; private set; }

    /// <summary>
    /// Optionele omschrijving of type van de bijlage (bijv. "Foto van de zee", "Brandstofbon").
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Navigatieproperty naar de logboekregel.
    /// </summary>
    public LogbookEntry? Entry { get; private set; }

    /// <summary>
    /// Parameterloze constructor voor EF Core.
    /// </summary>
    private LogbookAttachment() { }

    /// <summary>
    /// Maakt een nieuwe <see cref="LogbookAttachment"/> aan.
    /// </summary>
    public LogbookAttachment(
        int logbookEntryId,
        string originalFileName,
        string storedFileName,
        string contentType,
        long sizeBytes,
        string? description = null)
    {
        LogbookEntryId = logbookEntryId;
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        ContentType = contentType;
        SizeBytes = sizeBytes;
        UploadedAtUtc = DateTime.UtcNow;
        Description = description;
    }
}
