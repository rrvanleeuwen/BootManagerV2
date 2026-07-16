namespace BootManager.Application.Inventory.DTOs;

/// <summary>
/// Eén geparste regel uit het vakantievoorraad-CSV (kolommen Aantal, Eenheid, Product, Locatie).
/// Bevat de canonieke hoeveelheid en de ruwe tekstwaarden zoals in het bronbestand.
/// </summary>
public class InventoryImportRowDto
{
    /// <summary>Geparste hoeveelheid (accepteert decimale komma, bijv. 1,5).</summary>
    public decimal Quantity { get; set; }

    /// <summary>Ruwe eenheidstekst uit de kolom Eenheid.</summary>
    public string Unit { get; set; } = default!;

    /// <summary>Ruwe productnaam uit de kolom Product.</summary>
    public string ProductName { get; set; } = default!;

    /// <summary>Ruwe locatietekst uit de kolom Locatie (bron voor de mappingstap).</summary>
    public string SourceLocation { get; set; } = default!;

    /// <summary>1-gebaseerd regelnummer in het bronbestand (voor foutmeldingen).</summary>
    public int LineNumber { get; set; }
}
