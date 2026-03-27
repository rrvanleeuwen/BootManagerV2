using System;

namespace BootManager.Core.Entities;

/// <summary>
/// Domein-entiteit voor het opslaan van geïnterpreteerde bewegingsmetingen.
/// Bevat de eerste betekenisvolle meetwaarden die uit netwerkberichten zijn geëxtraheerd.
/// 
/// Gebaseerd op NMEA 2000-achtige implementatie (PGN 129026 COG/SOG).
/// Zodra volledige NMEA 2000-ondersteuning wordt toegevoegd, zal dit vervangen of uitgebreid moeten worden.
/// </summary>
public class MotionMeasurement
{
    /// <summary>
    /// Unieke identificator van de bewegingsmeting.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Tijdstempel (UTC) waarop de meting is geregistreerd.
    /// </summary>
    public DateTime RecordedAtUtc { get; private set; }

    /// <summary>
    /// Oorsprong van de meting (bijv. IP-adres of apparaatnaam van de bron).
    /// </summary>
    public string Source { get; private set; } = default!;

    /// <summary>
    /// Referentie naar het oorspronkelijke netwerkbericht waaruit deze meting is afgeleid.
    /// </summary>
    public string MessageId { get; private set; } = default!;

    /// <summary>
    /// Koers over grond in graden (0-359,99).
    /// </summary>
    public decimal CourseOverGroundDegrees { get; private set; }

    /// <summary>
    /// Snelheid over grond in de gespecificeerde eenheid.
    /// </summary>
    public decimal SpeedOverGround { get; private set; }

    /// <summary>
    /// Eenheid van snelheid (bijv. "kn" voor knopen, "m/s" voor meter per seconde).
    /// </summary>
    public string SpeedUnit { get; private set; } = default!;

    private MotionMeasurement() { } // Voor EF

    /// <summary>
    /// Initialiseert een nieuwe bewegingsmeting met de verplichte velden.
    /// </summary>
    public MotionMeasurement(
        DateTime recordedAtUtc,
        string source,
        string messageId,
        decimal courseOverGroundDegrees,
        decimal speedOverGround,
        string speedUnit)
    {
        RecordedAtUtc = recordedAtUtc;
        Source = source;
        MessageId = messageId;
        CourseOverGroundDegrees = courseOverGroundDegrees;
        SpeedOverGround = speedOverGround;
        SpeedUnit = speedUnit;
    }
}
