namespace BootManager.Core.Entities;

/// <summary>
/// Gekoppelde code voor een product: barcode, QR-code of vrije tekstcode.
/// Aparte entiteit om flexibiliteit in toekomstige scope te bieden.
/// Waarde is genormaliseerd en case-onafhankelijk uniek catalog-breed.
/// Blijft bestaan en uniek ook wanneer het gekoppelde product gearchiveerd is.
/// </summary>
public class ProductCode
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid ProductId { get; private set; }

    /// <summary>Genormaliseerde code-waarde (lowercase, trimmed).</summary>
    public string NormalizedValue { get; private set; } = default!;

    /// <summary>Originele (getrimde) code-waarde zoals ingevoerd.</summary>
    public string Value { get; private set; } = default!;

    /// <summary>Code-formaat/type: bijv. 'barcode', 'qr', 'text'.</summary>
    public string Format { get; private set; } = default!;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public Product Product { get; private set; } = default!;

    private ProductCode() { }

    private ProductCode(Guid productId, string value, string format)
    {
        ProductId = productId;
        Value = value.Trim();
        NormalizedValue = value.Trim().ToLowerInvariant();
        Format = format;
    }

    public static ProductCode Create(Guid productId, string value, string format)
        => new(productId, value, format);

    public void UpdateValue(string newValue, string newFormat)
    {
        Value = newValue.Trim();
        NormalizedValue = newValue.Trim().ToLowerInvariant();
        Format = newFormat;
    }
}
