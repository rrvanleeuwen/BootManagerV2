using BootManager.Application.Storage.Contracts;
using QRCoder;
using System.Text;

namespace BootManager.Infrastructure.Storage.QrCoder;

/// <summary>
/// QRCoder-gebaseerde implementatie van QR-tag rendering voor opslaglocaties.
/// Genereert SVG-achtige HTML/canvas output uit bestaande BootManager QR-tokenwaarden.
/// </summary>
public class QrCoderStorageLocationQrTagRenderer : IStorageLocationQrTagRenderer
{
    /// <summary>
    /// Rendert een QR-code voor een gegeven QR-tokenwaarde als SVG.
    /// </summary>
    public async Task<StorageLocationQrTagRenderResult> RenderQrTagAsync(
        string qrValue,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(qrValue))
        {
            throw new ArgumentException("QR value cannot be null or empty", nameof(qrValue));
        }

        try
        {
            return await Task.Run(() => RenderInternal(qrValue), cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to render QR code for value: {qrValue}", ex);
        }
    }

    private StorageLocationQrTagRenderResult RenderInternal(string qrValue)
    {
        using (var qrGenerator = new QRCodeGenerator())
        {
            // Genereer QR-data met hoge foutcorrectie zodat deze robuust is
            var qrCodeData = qrGenerator.CreateQrCode(qrValue, QRCodeGenerator.ECCLevel.H);

            // Converteer naar SVG-formaat
            var svgContent = RenderAsEmbeddedSvg(qrCodeData);
            var pngBytes = new PngByteQRCode(qrCodeData).GetGraphic(20, drawQuietZones: true);

            return new StorageLocationQrTagRenderResult
            {
                SvgContent = svgContent,
                PngBytes = pngBytes,
                SuggestedFileName = "qr-tag"
            };
        }
    }

    private string RenderAsEmbeddedSvg(QRCodeData qrCodeData)
    {
        var size = qrCodeData.ModuleMatrix.Count;
        var moduleSize = 20; // pixels per module
        var svgSize = size * moduleSize;
        var padding = 10;
        var totalSize = svgSize + (padding * 2);

        var svg = new StringBuilder();
        svg.AppendLine($@"<svg xmlns='http://www.w3.org/2000/svg' width='{totalSize}' height='{totalSize}' viewBox='0 0 {totalSize} {totalSize}'>");
        svg.AppendLine($@"<rect width='{totalSize}' height='{totalSize}' fill='white'/>");

        // Render QR modules
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (qrCodeData.ModuleMatrix[y][x])
                {
                    var xPos = padding + (x * moduleSize);
                    var yPos = padding + (y * moduleSize);
                    svg.AppendLine($@"<rect x='{xPos}' y='{yPos}' width='{moduleSize}' height='{moduleSize}' fill='black'/>");
                }
            }
        }

        svg.AppendLine("</svg>");
        return svg.ToString();
    }
}
