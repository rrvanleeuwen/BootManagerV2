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
}
