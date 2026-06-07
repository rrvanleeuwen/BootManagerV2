using System;
using System.Collections.Generic;
using BootManager.Core.Enums;

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
    /// Loginterval in minuten. Bepaalt de verwachte frequentie van logboekregels.
    /// </summary>
    public int LogIntervalMinutes { get; private set; }

    /// <summary>
    /// Vrije notities over de reis.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Logstand bij aanvang van de reis (nm).
    /// </summary>
    public decimal? LogstandStart { get; private set; }

    /// <summary>
    /// Logstand aan het einde van de reis (nm).
    /// </summary>
    public decimal? LogstandEnd { get; private set; }

    /// <summary>
    /// Gelogde mijlen tijdens de reis (nm), berekend uit begin- en eindstand.
    /// </summary>
    public decimal? LoggedMiles { get; private set; }

    /// <summary>
    /// Motorurenstand bij aanvang van de reis.
    /// </summary>
    public decimal? EngineHoursStart { get; private set; }

    /// <summary>
    /// Motorurenstand aan het einde van de reis.
    /// </summary>
    public decimal? EngineHoursEnd { get; private set; }

    /// <summary>
    /// Brandstof (bijv. "&lt;0.5 tank" of "45 L").
    /// </summary>
    public string? Fuel { get; private set; }

    /// <summary>
    /// Totaal vaaruren van de reis.
    /// </summary>
    public decimal? TotalSailingHours { get; private set; }

    /// <summary>
    /// Tijdstempel (UTC) waarop de reis is aangemaakt.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Tijdstempel (UTC) van de laatste wijziging.
    /// </summary>
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Status van de reis: Open (lopend) of Completed (afgesloten).
    /// </summary>
    public LogbookTripStatus Status { get; private set; } = LogbookTripStatus.Open;

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
        string? notes = null,
        decimal? logstandStart = null,
        decimal? logstandEnd = null,
        decimal? engineHoursStart = null,
        decimal? engineHoursEnd = null,
        string? fuel = null,
        decimal? totalSailingHours = null,
        int logIntervalMinutes = 60)
    {
        if (logIntervalMinutes <= 0)
            throw new ArgumentException("Loginterval moet groter dan nul zijn.", nameof(logIntervalMinutes));
        ValidateLogstand(logstandStart, logstandEnd);

        Name = name;
        DepartureUtc = departureUtc;
        ArrivalUtc = arrivalUtc;
        DeparturePort = departurePort;
        DestinationPort = destinationPort;
        VesselName = vesselName;
        Crew = crew;
        Notes = notes;
        LogstandStart = logstandStart;
        LogstandEnd = logstandEnd;
        LoggedMiles = CalculateLoggedMiles(logstandStart, logstandEnd);
        EngineHoursStart = engineHoursStart;
        EngineHoursEnd = engineHoursEnd;
        Fuel = fuel;
        TotalSailingHours = totalSailingHours;
        LogIntervalMinutes = logIntervalMinutes;
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
        string? notes,
        decimal? logstandStart,
        decimal? logstandEnd,
        decimal? engineHoursStart,
        decimal? engineHoursEnd,
        string? fuel,
        decimal? totalSailingHours,
        int logIntervalMinutes = 60)
    {
        if (logIntervalMinutes <= 0)
            throw new ArgumentException("Loginterval moet groter dan nul zijn.", nameof(logIntervalMinutes));
        ValidateLogstand(logstandStart, logstandEnd);

        Name = name;
        DepartureUtc = departureUtc;
        ArrivalUtc = arrivalUtc;
        DeparturePort = departurePort;
        DestinationPort = destinationPort;
        VesselName = vesselName;
        Crew = crew;
        Notes = notes;
        LogstandStart = logstandStart;
        LogstandEnd = logstandEnd;
        LoggedMiles = CalculateLoggedMiles(logstandStart, logstandEnd);
        EngineHoursStart = engineHoursStart;
        EngineHoursEnd = engineHoursEnd;
        Fuel = fuel;
        TotalSailingHours = totalSailingHours;
        LogIntervalMinutes = logIntervalMinutes;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static decimal? CalculateLoggedMiles(decimal? logstandStart, decimal? logstandEnd)
    {
        if (!logstandStart.HasValue || !logstandEnd.HasValue || logstandEnd.Value < logstandStart.Value)
        {
            return null;
        }

        return logstandEnd.Value - logstandStart.Value;
    }

    private static void ValidateLogstand(decimal? logstandStart, decimal? logstandEnd)
    {
        if (logstandStart.HasValue && logstandEnd.HasValue && logstandEnd.Value < logstandStart.Value)
        {
            throw new ArgumentException("Logstand eind mag niet lager zijn dan logstand start.", nameof(logstandEnd));
        }
    }

    /// <summary>
    /// Rondt de reis administratief af en markeert deze als voltooid.
    /// Vereist een geldig aankomstmoment. Stelt ArrivalUtc in als die nog null is.
    /// </summary>
    /// <param name="arrivalTimeUtc">Aankomstmoment (UTC). Verplicht; mag niet null zijn.</param>
    /// <exception cref="InvalidOperationException">Gegooid als de reis al is voltooid of als geen aankomstmoment wordt verstrekt.</exception>
    public void CompleteTrip(DateTime arrivalTimeUtc)
    {
        if (Status == LogbookTripStatus.Completed)
            throw new InvalidOperationException("Deze reis is al voltooid.");

        // Zet aankomstmoment in (altijd, vervang bestaande waarde niet)
        ArrivalUtc = arrivalTimeUtc;
        Status = LogbookTripStatus.Completed;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

