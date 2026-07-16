using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;

namespace BootManager.Application.Inventory.Contracts;

/// <summary>
/// Contract voor de Owner-only CSV-startimport van de vakantievoorraad.
/// Splitst parsen (zonder neveneffecten) van de destructieve, mapping-gestuurde import.
/// </summary>
public interface IInventoryImportService
{
    /// <summary>
    /// Parseert de CSV-inhoud met kolommen Aantal, Eenheid, Product, Locatie.
    /// Voert geen enkele databasewijziging uit.
    /// </summary>
    /// <param name="csvContent">Volledige tekstinhoud van het CSV-bestand.</param>
    InventoryImportParseResult ParseCsv(string csvContent);

    /// <summary>
    /// Voert de startimport uit: verwijdert bestaande voorraadbeheerdata (behalve eenheden en
    /// categorieën) en bouwt daarna gebieden, locaties, QR-tokens, producten en voorraadregels op
    /// volgens de bevestigde locatie-mappings.
    /// </summary>
    /// <param name="rows">Eerder geparste CSV-regels.</param>
    /// <param name="mappings">Bevestigde mapping per unieke bronlocatie.</param>
    /// <param name="ct">Annuleringstoken.</param>
    Task<InventoryImportExecutionResult> ExecuteImportAsync(
        IReadOnlyList<InventoryImportRowDto> rows,
        IReadOnlyList<InventoryLocationMappingDto> mappings,
        CancellationToken ct = default);
}
