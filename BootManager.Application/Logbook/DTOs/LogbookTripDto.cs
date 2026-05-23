using System;
using System.Collections.Generic;

namespace BootManager.Application.Logbook.DTOs;

/// <summary>
/// DTO voor weergave van een reis inclusief samenvatting.
/// </summary>
public class LogbookTripDto
{
    /// <summary>Unieke identificator van de reis.</summary>
    public int Id { get; set; }

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

    /// <summary>Gelogde mijlen tijdens de reis (nm).</summary>
    public decimal? LoggedMiles { get; set; }

    /// <summary>Motorurenstand bij aanvang van de reis.</summary>
    public decimal? EngineHoursStart { get; set; }

    /// <summary>Motorurenstand aan het einde van de reis.</summary>
    public decimal? EngineHoursEnd { get; set; }

    /// <summary>Brandstof (bijv. "&lt;0.5 tank" of "45 L").</summary>
    public string? Fuel { get; set; }

    /// <summary>Totaal vaaruren van de reis.</summary>
    public decimal? TotalSailingHours { get; set; }

    /// <summary>Tijdstempel (UTC) waarop de reis is aangemaakt.</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Tijdstempel (UTC) van de laatste wijziging.</summary>
    public DateTime UpdatedAtUtc { get; set; }
}
