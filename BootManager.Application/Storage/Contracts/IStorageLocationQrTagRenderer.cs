namespace BootManager.Application.Storage.Contracts;

/// <summary>
/// Abstraction voor het renderen van QR-codes voor opslaglocatietags.
/// Deze interface houdt QR-generatie los van presentatie en library-specifieke details.
/// </summary>
public interface IStorageLocationQrTagRenderer
{
    /// <summary>
    /// Rendert een QR-code voor een gegeven opslaglocatie QR-waarde.
    /// </summary>
    /// <param name="qrValue">De bestaande BootManager QR-tokenwaarde (bootmanager:location:...)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Renderresultaat met SVG en metadata</returns>
    Task<StorageLocationQrTagRenderResult> RenderQrTagAsync(string qrValue, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultaat van QR-rendering: bevat de SVG-uitvoer en metadata.
/// </summary>
public class StorageLocationQrTagRenderResult
{
    /// <summary>
    /// SVG-string van de gerenderde QR-code.
    /// </summary>
    public required string SvgContent { get; set; }

    /// <summary>
    /// PNG-byte-array van dezelfde QR-code voor robuuste clientdownload.
    /// </summary>
    public required byte[] PngBytes { get; set; }

    /// <summary>
    /// MIME-type van de inhoud (altijd image/svg+xml).
    /// </summary>
    public string ContentType => "image/svg+xml";

    /// <summary>
    /// Gesuggereerde bestandsnaam voor PNG-download (zonder extensie).
    /// </summary>
    public string? SuggestedFileName { get; set; }
}
