using System;
using System.Collections.Generic;

namespace BootManager.Core.Entities;

/// <summary>
/// Domein-entiteit voor een reis in het digitale logboek.
/// Bevat de reis-header en samenvatting.
/// </summary>
public class LogbookTrip
{
    /// <summary>
    /// Unieke identificator van de reis.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Naam of omschrijving van de reis (bijv. "Amsterdam – Enkhuizen").
    /// </summary>
    public string Name { get; private set; } = default!;

    /// <summary>
    /// Datum en tijd (UTC) van vertrek.
    /// </summary>
    public DateTime DepartureUtc { get; private set; }

    /// <summary>
    /// Datum en tijd (UTC) van aankomst. Null als de reis nog loopt.
    /// </summary>
    public DateTime? ArrivalUtc { get; private set; }

    /// <summary>
    /// Vertrekhaven of -locatie.
    /// </summary>
    public string? DeparturePort { get; private set; }

    /// <summary>
    /// Bestemmingshaven of -locatie.
    /// </summary>
    public string? DestinationPort { get; private set; }

    /// <summary>
    /// Naam van het vaartuig.
    /// </summary>
    public string? VesselName { get; private set; }

    /// <summary>
    /// Namen van de bemanningsleden, kommagescheiden.
    /// </summary>
    public string? Crew { get; private set; }

    /// <summary>
    /// Vrije notities over de reis.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Tijdstempel (UTC) waarop de reis is aangemaakt.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Tijdstempel (UTC) van de laatste wijziging.
    /// </summary>
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Navigatiekolommen: logboekregels behorend bij deze reis.
    /// </summary>
    public ICollection<LogbookEntry> Entries { get; private set; } = new List<LogbookEntry>();

    /// <summary>
    /// Parameterloze constructor voor EF Core.
    /// </summary>
    private LogbookTrip() { }

    /// <summary>
    /// Maakt een nieuwe <see cref="LogbookTrip"/> aan.
    /// </summary>
    public LogbookTrip(
        string name,
        DateTime departureUtc,
        DateTime? arrivalUtc = null,
        string? departurePort = null,
        string? destinationPort = null,
        string? vesselName = null,
        string? crew = null,
        string? notes = null)
    {
        Name = name;
        DepartureUtc = departureUtc;
        ArrivalUtc = arrivalUtc;
        DeparturePort = departurePort;
        DestinationPort = destinationPort;
        VesselName = vesselName;
        Crew = crew;
        Notes = notes;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Past de reis-header aan op basis van nieuwe waarden.
    /// </summary>
    public void Update(
        string name,
        DateTime departureUtc,
        DateTime? arrivalUtc,
        string? departurePort,
        string? destinationPort,
        string? vesselName,
        string? crew,
        string? notes)
    {
        Name = name;
        DepartureUtc = departureUtc;
        ArrivalUtc = arrivalUtc;
        DeparturePort = departurePort;
        DestinationPort = destinationPort;
        VesselName = vesselName;
        Crew = crew;
        Notes = notes;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
