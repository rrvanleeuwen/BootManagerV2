using System;
using BootManager.Core.Enums;

namespace BootManager.Application.Logbook.DTOs;

/// <summary>
/// DTO voor weergave van een logboekregel.
/// </summary>
public class LogbookEntryDto
{
    /// <summary>Unieke identificator van de logboekregel.</summary>
    public int Id { get; set; }

    /// <summary>Verwijzing naar de bijbehorende reis.</summary>
    public int LogbookTripId { get; set; }

    /// <summary>Tijdstempel (UTC) van de logboekregel.</summary>
    public DateTime EntryTimeUtc { get; set; }

    /// <summary>Barometer-stand in hPa. Null indien niet ingevuld.</summary>
    public decimal? BaroPressure { get; set; }

    /// <summary>Logwaarde in nautische mijlen. Null indien niet ingevuld.</summary>
    public decimal? LogValue { get; set; }

    /// <summary>Koers in graden (0-359). Null indien niet ingevuld.</summary>
    public int? Course { get; set; }

    /// <summary>Positie, zeilvoering of opmerkingen.</summary>
    public string? Remarks { get; set; }

    /// <summary>Windrichting en -kracht. Null indien niet ingevuld.</summary>
    public string? WindDescription { get; set; }

    /// <summary>GPS-status of fix-indicator. Null indien niet beschikbaar.</summary>
    public string? GpsStatus { get; set; }

    /// <summary>Breedtegraad (WGS84, decimaal). Null indien niet beschikbaar.</summary>
    public double? Latitude { get; set; }

    /// <summary>Lengtegraad (WGS84, decimaal). Null indien niet beschikbaar.</summary>
    public double? Longitude { get; set; }

    /// <summary>Gemiddelde SOG in knopen over de logperiode. Null indien niet beschikbaar.</summary>
    public decimal? AverageSogKnots { get; set; }

    /// <summary>Status van de logboekregel in de akkoordflow.</summary>
    public LogbookEntryStatus Status { get; set; }

    /// <summary>Tijdstempel (UTC) van de laatste wijziging.</summary>
    public DateTime UpdatedAtUtc { get; set; }
}
