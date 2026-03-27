namespace BootManager.Application.NetworkMessageInterpretation.DTOs;

/// <summary>
/// Semantisch interpretatieresultaat voor batterij-statusberichten.
/// 
/// Dit DTO vertegenwoordigt de domein-waarden die zijn afgeleid van een technisch parse-resultaat.
/// Dit is GEEN rauwe parse-output, maar een geïnterpreteerde, betekenisvolle interpretatie.
/// 
/// BELANGRIJK: Deze interpretatie is specifiek voor het huidige simulatorformaat en niet
/// een algemene NMEA2000-decoder. Zodra volledige NMEA2000-ondersteuning wordt toegevoegd,
/// zal dit vervangen of uitgebreid moeten worden.
/// </summary>
public class BatteryMessageInterpretationDto
{
    /// <summary>
    /// Geeft aan of de interpretatie succesvol is voltooid.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// De geïnterpreteerde spanning in volts (V).
    /// Null als IsSuccess == false of als de spanning niet kon worden afgeleid.
    /// </summary>
    public decimal? Voltage { get; set; }

    /// <summary>
    /// De eenheid van de spanning. Altijd "V" voor volts.
    /// </summary>
    public string Unit { get; set; } = "V";

    /// <summary>
    /// Foutbericht als IsSuccess == false.
    /// Beschrijft waarom de interpretatie niet kon worden voltooid.
    /// </summary>
    public string? ErrorMessage { get; set; }
}