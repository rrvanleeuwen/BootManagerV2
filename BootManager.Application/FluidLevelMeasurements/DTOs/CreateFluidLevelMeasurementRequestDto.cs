namespace BootManager.Application.FluidLevelMeasurements.DTOs;

using BootManager.Core.Entities;

/// <summary>
/// DTO voor het aanmaken van een nieuw FluidLevelMeasurement-record.
/// </summary>
public class CreateFluidLevelMeasurementRequestDto
{
    /// <summary>
    /// Tijdstempel (UTC) waarop de meting is geregistreerd.
    /// </summary>
    public DateTime RecordedAtUtc { get; init; }

    /// <summary>
    /// Oorsprong van de meting (bijv. IP-adres of apparaatnaam van de bron).
    /// </summary>
    public string Source { get; init; } = default!;

    /// <summary>
    /// Referentie naar het oorspronkelijke netwerkbericht waaruit deze meting is afgeleid.
    /// </summary>
    public string MessageId { get; init; } = default!;

    /// <summary>
    /// PGN van het bericht.
    /// </summary>
    public uint Pgn { get; init; }

    /// <summary>
    /// Gateway-sentence type (bijv. "PCDIN" of "MXPGN").
    /// Optioneel.
    /// </summary>
    public string? GatewaySentence { get; init; }

    /// <summary>
    /// Source address uit het NMEA 2000-bericht (indien beschikbaar).
    /// Optioneel.
    /// </summary>
    public byte? SourceAddress { get; init; }

    /// <summary>
    /// Fluid instance (0-15).
    /// </summary>
    public byte FluidInstance { get; init; }

    /// <summary>
    /// Tanktype als enum.
    /// </summary>
    public FluidType FluidType { get; init; }

    /// <summary>
    /// Raw numerieke tanktype-waarde.
    /// </summary>
    public byte RawFluidType { get; init; }

    /// <summary>
    /// Tankniveau als percentage (0-100%), of null als onbekend/ongeldig.
    /// </summary>
    public decimal? LevelPercent { get; init; }

    /// <summary>
    /// Tankcapaciteit in liters, of null als onbekend.
    /// </summary>
    public decimal? CapacityLiters { get; init; }

    /// <summary>
    /// Geeft aan of het niveau-waarde als ongeldig/onbekend beschouwd wordt.
    /// </summary>
    public bool IsLevelInvalid { get; init; }
}
