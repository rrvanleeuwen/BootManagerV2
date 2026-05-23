using System;
using BootManager.Core.Enums;

namespace BootManager.Core.Entities;

/// <summary>
/// Domein-entiteit voor een logboekregel binnen een reis.
/// Handmatige invoer is leidend; automatische meetwaarden zijn optioneel.
/// </summary>
public class LogbookEntry
{
    /// <summary>
    /// Unieke identificator van de logboekregel.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Verwijzing naar de bijbehorende reis.
    /// </summary>
    public int LogbookTripId { get; private set; }

    /// <summary>
    /// Tijdstempel (UTC) van de logboekregel (handmatig ingevuld).
    /// </summary>
    public DateTime EntryTimeUtc { get; private set; }

    /// <summary>
    /// Barometer-stand in hPa (handmatig). Null indien niet ingevuld.
    /// </summary>
    public decimal? BaroPressure { get; private set; }

    /// <summary>
    /// Logwaarde (afstand door water) in nautische mijlen (handmatig). Null indien niet ingevuld.
    /// </summary>
    public decimal? LogValue { get; private set; }

    /// <summary>
    /// Koers in graden (0-359), handmatig. Null indien niet ingevuld.
    /// </summary>
    public int? Course { get; private set; }

    /// <summary>
    /// Vrije tekst: positie, zeilvoering of opmerkingen.
    /// </summary>
    public string? Remarks { get; private set; }

    /// <summary>
    /// Windrichting en -kracht (handmatig, bijv. "NW 4"). Null indien niet ingevuld.
    /// </summary>
    public string? WindDescription { get; private set; }

    /// <summary>
    /// GPS-kwaliteitsindicator of fix-status (placeholder voor automatische data). Null indien niet beschikbaar.
    /// </summary>
    public string? GpsStatus { get; private set; }

    /// <summary>
    /// Breedtegraad (WGS84, decimaal). Automatisch of handmatig. Null indien niet beschikbaar.
    /// </summary>
    public double? Latitude { get; private set; }

    /// <summary>
    /// Lengtegraad (WGS84, decimaal). Automatisch of handmatig. Null indien niet beschikbaar.
    /// </summary>
    public double? Longitude { get; private set; }

    /// <summary>
    /// Gemiddelde snelheid over grond (SOG) in knopen over de logperiode. Null indien niet beschikbaar.
    /// </summary>
    public decimal? AverageSogKnots { get; private set; }

    /// <summary>
    /// Status van de logboekregel in de akkoordflow.
    /// </summary>
    public LogbookEntryStatus Status { get; private set; }

    /// <summary>
    /// Tijdstempel (UTC) waarop de regel is aangemaakt.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Tijdstempel (UTC) van de laatste wijziging.
    /// </summary>
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Navigatieproperty naar de reis.
    /// </summary>
    public LogbookTrip? Trip { get; private set; }

    /// <summary>
    /// Parameterloze constructor voor EF Core.
    /// </summary>
    private LogbookEntry() { }

    /// <summary>
    /// Maakt een nieuwe <see cref="LogbookEntry"/> aan.
    /// </summary>
    public LogbookEntry(
        int logbookTripId,
        DateTime entryTimeUtc,
        decimal? baroPressure = null,
        decimal? logValue = null,
        int? course = null,
        string? remarks = null,
        string? windDescription = null,
        string? gpsStatus = null,
        double? latitude = null,
        double? longitude = null,
        decimal? averageSogKnots = null)
    {
        LogbookTripId = logbookTripId;
        EntryTimeUtc = entryTimeUtc;
        BaroPressure = baroPressure;
        LogValue = logValue;
        Course = course;
        Remarks = remarks;
        WindDescription = windDescription;
        GpsStatus = gpsStatus;
        Latitude = latitude;
        Longitude = longitude;
        AverageSogKnots = averageSogKnots;
        Status = LogbookEntryStatus.Confirmed;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Past de logboekregel aan op basis van nieuwe waarden.
    /// </summary>
    public void Update(
        DateTime entryTimeUtc,
        decimal? baroPressure,
        decimal? logValue,
        int? course,
        string? remarks,
        string? windDescription,
        string? gpsStatus,
        double? latitude,
        double? longitude,
        decimal? averageSogKnots)
    {
        EntryTimeUtc = entryTimeUtc;
        BaroPressure = baroPressure;
        LogValue = logValue;
        Course = course;
        Remarks = remarks;
        WindDescription = windDescription;
        GpsStatus = gpsStatus;
        Latitude = latitude;
        Longitude = longitude;
        AverageSogKnots = averageSogKnots;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Markeert de logboekregel als definitief (Confirmed). Heeft geen effect als de regel al Confirmed is.
    /// </summary>
    public void Confirm()
    {
        if (Status == LogbookEntryStatus.Confirmed) return;
        Status = LogbookEntryStatus.Confirmed;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
