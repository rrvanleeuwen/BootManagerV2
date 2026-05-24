using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BootManager.Application.Logbook.DTOs;

namespace BootManager.Application.Logbook.Services;

/// <summary>
/// Service-interface voor beheer van logboekbijlagen.
/// </summary>
public interface ILogbookAttachmentService
{
    /// <summary>
    /// Upload een bestand als bijlage bij een logboekregel.
    /// </summary>
    /// <param name="entryId">ID van de logboekregel.</param>
    /// <param name="fileStream">De bestandsinhoud.</param>
    /// <param name="originalFileName">Oorspronkelijke bestandsnaam.</param>
    /// <param name="contentType">MIME-type van het bestand.</param>
    /// <param name="description">Optionele omschrijving of type van de bijlage.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Upload-resultaat met bijlage of fout.</returns>
    Task<AttachmentUploadResultDto> UploadAsync(
        int entryId,
        Stream fileStream,
        string originalFileName,
        string contentType,
        string? description = null,
        CancellationToken ct = default);

    /// <summary>
    /// Haalt een bijlage op voor download.
    /// </summary>
    /// <param name="attachmentId">ID van de bijlage.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Bestandsinhoud en metadata, of null als niet gevonden.</returns>
    Task<(Stream? Stream, string? OriginalFileName, string? ContentType)> DownloadAsync(
        int attachmentId,
        CancellationToken ct = default);

    /// <summary>
    /// Verwijdert een bijlage.
    /// </summary>
    /// <param name="attachmentId">ID van de bijlage.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>true als verwijdering gelukt, false als bijlage niet gevonden.</returns>
    Task<bool> DeleteAsync(int attachmentId, CancellationToken ct = default);

    /// <summary>
    /// Haalt alle bijlagen voor een logboekregel op.
    /// </summary>
    /// <param name="entryId">ID van de logboekregel.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Lijst van bijlagen.</returns>
    Task<IEnumerable<LogbookAttachmentDto>> GetAttachmentsAsync(
        int entryId,
        CancellationToken ct = default);

    /// <summary>
    /// Haalt het aantal bijlagen op voor een logboekregel.
    /// </summary>
    /// <param name="entryId">ID van de logboekregel.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Aantal bijlagen.</returns>
    Task<int> GetAttachmentCountAsync(int entryId, CancellationToken ct = default);
}
