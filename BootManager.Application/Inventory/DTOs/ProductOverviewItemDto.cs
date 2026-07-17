namespace BootManager.Application.Inventory.DTOs;

/// <summary>
/// Eén zichtbaar product op een productoverzichtspagina, inclusief de reeds gebatcht
/// geladen actieve voorraadregels en het totaal. Bedoeld als kant-en-klare
/// presentatiedata zodat het overzicht geen losse voorraadquery per product hoeft te doen.
/// </summary>
public class ProductOverviewItemDto
{
    /// <summary>Product-, eenheid-, code- en actieve-categoriegegevens van het overzichtsitem.</summary>
    public ProductDto Product { get; set; } = default!;

    /// <summary>Actieve voorraadlocaties (hoeveelheid groter dan nul) van dit product.</summary>
    public IReadOnlyList<StockDto> ActiveLocations { get; set; } = new List<StockDto>();

    /// <summary>Totale actieve hoeveelheid over alle voorraadlocaties; nul zonder actieve voorraad.</summary>
    public decimal TotalQuantity { get; set; }
}
