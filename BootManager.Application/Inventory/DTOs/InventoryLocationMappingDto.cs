namespace BootManager.Application.Inventory.DTOs;

/// <summary>
/// Door de Owner bevestigde mapping van één unieke CSV-locatietekst naar een
/// opslaggebied en een opslaglocatie binnen dat gebied.
/// </summary>
public class InventoryLocationMappingDto
{
    /// <summary>Exacte CSV-locatietekst waarvoor deze mapping geldt.</summary>
    public string SourceLocation { get; set; } = default!;

    /// <summary>Gekozen of nieuw aan te maken gebiedsnaam.</summary>
    public string AreaName { get; set; } = default!;

    /// <summary>Gekozen of nieuw aan te maken locatienaam binnen het gebied.</summary>
    public string LocationName { get; set; } = default!;
}
