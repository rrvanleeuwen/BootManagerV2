using BootManager.Application.Inventory.DTOs;

namespace BootManager.Application.Inventory.Results;

/// <summary>
/// Resultaat van het parsen van een vakantievoorraad-CSV.
/// Bevat de geparste regels, de unieke bronlocaties die gemapt moeten worden en eventuele parsefouten.
/// </summary>
public class InventoryImportParseResult
{
    /// <summary>True wanneer er ten minste één regel is en geen enkele parsefout optrad.</summary>
    public bool Success { get; set; }

    /// <summary>Alle succesvol geparste regels.</summary>
    public List<InventoryImportRowDto> Rows { get; set; } = new();

    /// <summary>Unieke CSV-locatieteksten in volgorde van eerste voorkomen; elk vereist één mapping.</summary>
    public List<string> DistinctSourceLocations { get; set; } = new();

    /// <summary>Parsefouten (lege lijst betekent geen fouten).</summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>Maakt een mislukt parseresultaat met de opgegeven foutmeldingen.</summary>
    public static InventoryImportParseResult Failed(params string[] errors)
        => new() { Success = false, Errors = errors.ToList() };
}
