using System;

namespace BootManager.Application.Logbook.DTOs;

/// <summary>
/// DTO voor het aanmaken van een nieuwe reis.
/// </summary>
public class CreateLogbookTripDto
{
    /// <summary>Naam of omschrijving van de reis.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Datum en tijd (UTC) van vertrek.</summary>
    public DateTime DepartureUtc { get; set; }

    /// <summary>Datum en tijd (UTC) van aankomst. Null als de reis nog loopt.</summary>
    public DateTime? ArrivalUtc { get; set; }

    /// <summary>Vertrekhaven of -locatie.</summary>
    public string? DeparturePort { get; set; }

    /// <summary>Bestemmingshaven of -locatie.</summary>
    public string? DestinationPort { get; set; }

    /// <summary>Naam van het vaartuig.</summary>
    public string? VesselName { get; set; }

    /// <summary>Namen van de bemanningsleden, kommagescheiden.</summary>
    public string? Crew { get; set; }

    /// <summary>Vrije notities over de reis.</summary>
    public string? Notes { get; set; }

    /// <summary>Logstand bij aanvang van de reis (nm).</summary>
    public decimal? LogstandStart { get; set; }

    /// <summary>Logstand aan het einde van de reis (nm).</summary>
    public decimal? LogstandEnd { get; set; }

    /// <summary>Motorurenstand bij aanvang van de reis.</summary>
    public decimal? EngineHoursStart { get; set; }

    /// <summary>Motorurenstand aan het einde van de reis.</summary>
    public decimal? EngineHoursEnd { get; set; }

    /// <summary>Brandstof (bijv. "&lt;0.5 tank" of "45 L").</summary>
    public string? Fuel { get; set; }

    /// <summary>Totaal vaaruren van de reis.</summary>
    public decimal? TotalSailingHours { get; set; }

    /// <summary>Loginterval in minuten (standaard 60).</summary>
    public int LogIntervalMinutes { get; set; } = 60;

    /// <summary>Totale reisduur (berekend uit DepartureUtc en ArrivalUtc, niet hetzelfde als TotalSailingHours).</summary>
    public decimal? TotalTripDuration { get; set; }

    /// <summary>
    /// Interne vlag: EngineHoursStart is expliciet van bootprofiel overgenomen.
    /// Voor UI-feedback: als deze vlag false is, was het geen expliciete actie.
    /// </summary>
    public bool EngineHoursCopiedFromProfile { get; set; }

    /// <summary>
    /// Interne vlag: LogstandStart is expliciet van bootprofiel overgenomen.
    /// Voor UI-feedback: als deze vlag false is, was het geen expliciete actie.
    /// </summary>
    public bool LogstandCopiedFromProfile { get; set; }
}
