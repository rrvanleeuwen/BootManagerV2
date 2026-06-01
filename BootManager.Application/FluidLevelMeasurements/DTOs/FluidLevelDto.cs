namespace BootManager.Application.FluidLevelMeasurements.DTOs;

using BootManager.Core.Entities;

/// <summary>
/// DTO voor het uitlezen van een FluidLevelMeasurement-record.
/// </summary>
public class FluidLevelDto
{
    /// <summary>
    /// Unieke identificator van de meting.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Tijdstempel (UTC) waarop de meting is geregistreerd.
    /// </summary>
    public DateTime RecordedAtUtc { get; init; }

    /// <summary>
    /// Oorsprong van de meting.
    /// </summary>
    public string Source { get; init; } = default!;

    /// <summary>
    /// Referentie naar het oorspronkelijke netwerkbericht.
    /// </summary>
    public string MessageId { get; init; } = default!;

    /// <summary>
    /// PGN van het bericht.
    /// </summary>
    public uint Pgn { get; init; }

    /// <summary>
    /// Gateway-sentence type.
    /// </summary>
    public string? GatewaySentence { get; init; }

    /// <summary>
    /// Source address uit het NMEA 2000-bericht.
    /// </summary>
    public byte? SourceAddress { get; init; }

    /// <summary>
    /// Fluid instance.
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
    /// Tankniveau als percentage.
    /// </summary>
    public decimal? LevelPercent { get; init; }

    /// <summary>
    /// Tankcapaciteit in liters.
    /// </summary>
    public decimal? CapacityLiters { get; init; }

    /// <summary>
    /// Geeft aan of het niveau ongeldig is.
    /// </summary>
    public bool IsLevelInvalid { get; init; }
}
