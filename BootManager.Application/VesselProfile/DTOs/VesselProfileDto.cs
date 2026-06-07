namespace BootManager.Application.VesselProfile.DTOs;

/// <summary>
/// DTO voor het bootprofiel.
/// </summary>
public sealed record VesselProfileDto
{
    /// <summary>
    /// Unieke identifier van het bootprofiel.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Bootnaam.
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
    /// Actuele motorurenstand (optioneel, niet-negatief).
    /// </summary>
    public decimal? CurrentEngineHours { get; init; }

    /// <summary>
    /// Actuele logstandwaarde in zeemijlen (optioneel, niet-negatief).
    /// </summary>
    public decimal? CurrentLogstand { get; init; }

    /// <summary>
    /// Moment waarop het profiel is aangemaakt (UTC).
    /// </summary>
    public DateTime CreatedUtc { get; init; }

    /// <summary>
    /// Moment waarop het profiel voor het laatst is bijgewerkt (UTC).
    /// </summary>
    public DateTime? UpdatedUtc { get; init; }
}
