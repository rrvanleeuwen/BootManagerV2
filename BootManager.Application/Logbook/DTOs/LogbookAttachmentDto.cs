using System;

namespace BootManager.Application.Logbook.DTOs;

/// <summary>
/// DTO voor weergave van een bijlage bij een logboekregel.
/// </summary>
public class LogbookAttachmentDto
{
    /// <summary>Unieke identificator van de bijlage.</summary>
    public int Id { get; set; }

    /// <summary>Oorspronkelijke bestandsnaam zoals geüpload.</summary>
    public string OriginalFileName { get; set; } = default!;

    /// <summary>MIME-type van het bestand.</summary>
    public string ContentType { get; set; } = default!;

    /// <summary>Bestandsgrootte in bytes.</summary>
    public long SizeBytes { get; set; }

    /// <summary>Datum en tijd (UTC) van upload.</summary>
    public DateTime UploadedAtUtc { get; set; }

    /// <summary>Optionele omschrijving of type van de bijlage.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Geformateerde bestandsgrootte voor weergave (bijv. "2.5 MB").
    /// </summary>
    public string FormattedSize => FormatBytes(SizeBytes);

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:F1} {sizes[order]}";
    }
}
