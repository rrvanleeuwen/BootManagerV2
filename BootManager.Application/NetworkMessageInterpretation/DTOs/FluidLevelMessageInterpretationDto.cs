namespace BootManager.Application.NetworkMessageInterpretation.DTOs;

using BootManager.Core.Entities;

/// <summary>
/// DTO voor het resultaat van semantische interpretatie van een tankniveau-bericht.
/// Bevat zowel succesvol-gedecodeerde waarden als foutinformatie.
/// </summary>
public class FluidLevelMessageInterpretationDto
{
    /// <summary>
    /// Geeft aan of de interpretatie succesvol is.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Foutmelding indien IsSuccess=false.
    /// Null als interpretatie succesvol is.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Fluid instance (0-15) uit byte 0.
    /// </summary>
    public byte FluidInstance { get; init; }

    /// <summary>
    /// Tanktype als enum-waarde.
    /// Wordt bepaald op basis van de ruwe type-waarde.
    /// </summary>
    public FluidType FluidType { get; init; }

    /// <summary>
    /// Ruwe numerieke tanktype-waarde uit byte 0, bits 4-7.
    /// Gebruikt voor veilige opslag van onbekende/toekomstige typen.
    /// </summary>
    public byte RawFluidType { get; init; }

    /// <summary>
    /// Tankniveau als percentage (0-100%).
    /// Null als niveau onbekend/ongeldig is (0x7FFF).
    /// </summary>
    public decimal? LevelPercent { get; init; }

    /// <summary>
    /// Tankcapaciteit in liters (indien aanwezig).
    /// Null als capaciteit onbekend/niet gemeten is.
    /// </summary>
    public decimal? CapacityLiters { get; init; }

    /// <summary>
    /// Geeft aan of het niveau-waarde als ongeldig/onbekend beschouwd wordt.
    /// Dit is het geval wanneer de ruwe waarde 0x7FFF is.
    /// </summary>
    public bool IsLevelInvalid { get; init; }

    /// <summary>
    /// Gateway-sentence type (bijv. "PCDIN", "MXPGN"), indien herleid.
    /// Optioneel.
    /// </summary>
    public string? GatewaySentence { get; init; }

    /// <summary>
    /// Source address uit het NMEA 2000-bericht (indien herleid).
    /// Optioneel.
    /// </summary>
    public byte? SourceAddress { get; init; }
}
