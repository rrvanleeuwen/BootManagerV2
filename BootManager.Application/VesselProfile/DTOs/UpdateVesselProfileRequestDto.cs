namespace BootManager.Application.VesselProfile.DTOs;

/// <summary>
/// DTO voor het bijwerken van het bootprofiel.
/// </summary>
public sealed record UpdateVesselProfileRequestDto
{
    /// <summary>
    /// Bootnaam (verplicht).
    /// </summary>
    public string VesselName { get; init; } = string.Empty;

    /// <summary>
    /// Thuishaven (optioneel).
    /// </summary>
    public string? HomePort { get; init; }

    /// <summary>
    /// Roepnaam (optioneel).
    /// </summary>
    public string? CallSign { get; init; }

    /// <summary>
    /// MMSI-nummer (optioneel).
    /// </summary>
    public string? Mmsi { get; init; }

    /// <summary>
    /// Actuele motorurenstand (optioneel, niet-negatief). Kan null/leeg zijn, of een lagere waarde voor reset.
    /// </summary>
    public decimal? CurrentEngineHours { get; init; }

    /// <summary>
    /// Actuele logstandwaarde in zeemijlen (optioneel, niet-negatief). Kan null/leeg zijn, of een lagere waarde voor reset.
    /// </summary>
    public decimal? CurrentLogstand { get; init; }
}
