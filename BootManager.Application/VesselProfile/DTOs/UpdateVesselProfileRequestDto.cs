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
}
