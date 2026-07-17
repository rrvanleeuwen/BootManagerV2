namespace BootManager.Application.Inventory.DTOs;

/// <summary>
/// Eén gepagineerd productoverzicht-resultaat: het totale aantal matches in de database
/// en uitsluitend de producten van de gevraagde pagina.
/// </summary>
public class ProductOverviewPageDto
{
    /// <summary>Totaal aantal producten dat aan het filter voldoet (over alle pagina's).</summary>
    public int TotalCount { get; set; }

    /// <summary>De zichtbare producten van de gevraagde pagina, in stabiele sorteervolgorde.</summary>
    public IReadOnlyList<ProductOverviewItemDto> Items { get; set; } = new List<ProductOverviewItemDto>();
}
