using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BootManager.Application.Logbook.DTOs;
using BootManager.Application.Logbook.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BootManager.Web.Controllers;

/// <summary>
/// API controller voor beheer van logboekbijlagen.
/// Handelt upload, download en verwijdering van bijlagen af.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Owner")]
public class LogbookAttachmentsController : ControllerBase
{
    private readonly ILogbookAttachmentService _attachmentService;

    /// <summary>
    /// Initialiseert een nieuw exemplaar van <see cref="LogbookAttachmentsController"/>.
    /// </summary>
    public LogbookAttachmentsController(ILogbookAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    /// <summary>
    /// Upload een bestand als bijlage bij een logboekregel.
    /// </summary>
    /// <param name="entryId">ID van de logboekregel.</param>
    /// <param name="file">Het bestand om te uploaden.</param>
    /// <param name="description">Optionele omschrijving of type van de bijlage.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Upload-resultaat met bijlage of fout.</returns>
    [HttpPost("upload/{entryId:int}")]
    public async Task<ActionResult<AttachmentUploadResultDto>> Upload(
        int entryId,
        [FromForm] IFormFile file,
        [FromForm] string? description = null,
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Geen bestand geselecteerd.");

        using (var stream = file.OpenReadStream())
        {
            var result = await _attachmentService.UploadAsync(
                entryId,
                stream,
                file.FileName,
                file.ContentType,
                description,
                ct);

            if (result.Status != AttachmentUploadStatus.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
    /// <summary>
    /// Download een bijlage.
    /// </summary>
    /// <param name="attachmentId">ID van de bijlage.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Bestandsinhoud.</returns>
    [HttpGet("download/{attachmentId:int}")]
    public async Task<IActionResult> Download(int attachmentId, CancellationToken ct = default)
    {
        var (stream, fileName, contentType) = await _attachmentService.DownloadAsync(attachmentId, ct);

        if (stream == null)
            return NotFound();

        return File(stream, contentType ?? "application/octet-stream", fileName);
    }

    /// <summary>
    /// Verwijdert een bijlage.
    /// </summary>
    /// <param name="attachmentId">ID van de bijlage.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Succes- of foutresponse.</returns>
    [HttpDelete("{attachmentId:int}")]
    public async Task<IActionResult> Delete(int attachmentId, CancellationToken ct = default)
    {
        var success = await _attachmentService.DeleteAsync(attachmentId, ct);

        if (!success)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Haalt alle bijlagen voor een logboekregel op.
    /// </summary>
    /// <param name="entryId">ID van de logboekregel.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Lijst van bijlagen.</returns>
    [HttpGet("entry/{entryId:int}")]
    public async Task<ActionResult<IEnumerable<LogbookAttachmentDto>>> GetAttachments(
        int entryId,
        CancellationToken ct = default)
    {
        var attachments = await _attachmentService.GetAttachmentsAsync(entryId, ct);
        return Ok(attachments);
    }

    /// <summary>
    /// Haalt het aantal bijlagen op voor een logboekregel.
    /// </summary>
    /// <param name="entryId">ID van de logboekregel.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Aantal bijlagen.</returns>
    [HttpGet("count/{entryId:int}")]
    public async Task<ActionResult<int>> GetAttachmentCount(int entryId, CancellationToken ct = default)
    {
        var count = await _attachmentService.GetAttachmentCountAsync(entryId, ct);
        return Ok(count);
    }
}
