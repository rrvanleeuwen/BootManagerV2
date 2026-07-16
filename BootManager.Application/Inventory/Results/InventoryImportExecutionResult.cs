namespace BootManager.Application.Inventory.Results;

/// <summary>
/// Resultaat van het daadwerkelijk uitvoeren van de CSV-startimport.
/// Bevat succesindicatie, foutmelding en telwaarden over de opgebouwde data.
/// </summary>
public class InventoryImportExecutionResult
{
    /// <summary>True wanneer de volledige import slaagde.</summary>
    public bool Success { get; set; }

    /// <summary>Foutmelding bij mislukte import; null bij succes.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Aantal nieuw aangemaakte eenheden (bestaande eenheden worden hergebruikt).</summary>
    public int UnitsCreated { get; set; }

    /// <summary>Aantal aangemaakte opslaggebieden.</summary>
    public int AreasCreated { get; set; }

    /// <summary>Aantal aangemaakte opslaglocaties.</summary>
    public int LocationsCreated { get; set; }

    /// <summary>Aantal aangemaakte producten.</summary>
    public int ProductsCreated { get; set; }

    /// <summary>Aantal aangemaakte voorraadregels (unieke product-locatiecombinaties).</summary>
    public int StockRowsCreated { get; set; }

    /// <summary>Aantal gegenereerde QR-tokens (één per nieuwe locatie).</summary>
    public int TokensGenerated { get; set; }

    /// <summary>Id's van de geimporteerde opslaglocaties met QR-token.</summary>
    public List<Guid> ImportedLocationIds { get; set; } = new();

    /// <summary>Maakt een succesvol resultaat.</summary>
    public static InventoryImportExecutionResult Ok() => new() { Success = true };

    /// <summary>Maakt een mislukt resultaat met de opgegeven foutmelding.</summary>
    public static InventoryImportExecutionResult Error(string message)
        => new() { Success = false, ErrorMessage = message };
}
