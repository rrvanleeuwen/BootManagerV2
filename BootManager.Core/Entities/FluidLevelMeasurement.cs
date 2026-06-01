namespace BootManager.Core.Entities;

/// <summary>
/// Domein-entiteit voor het opslaan van geïnterpreteerde tankniveau-metingen.
/// Gebaseerd op PGN 127505 (Fluid Level).
/// Ondersteunt meerdere tanktypen en instances via FluidType + FluidInstance.
/// </summary>
public class FluidLevelMeasurement
{
    /// <summary>
    /// Unieke identificator van de meting.
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
    /// PGN (Parameter Group Number) van het bericht.
    /// Voor tankniveau: 127505.
    /// </summary>
    public uint Pgn { get; private set; }

    /// <summary>
    /// Gateway-sentence type (bijv. "PCDIN" of "MXPGN").
    /// Optioneel; kan null zijn voor niet-gateway berichten.
    /// </summary>
    public string? GatewaySentence { get; private set; }

    /// <summary>
    /// Source address uit het NMEA 2000-bericht (indien beschikbaar).
    /// Optioneel; kan null zijn.
    /// </summary>
    public byte? SourceAddress { get; private set; }

    /// <summary>
    /// Fluid instance (0-15). Onderscheidt verschillende tanks van hetzelfde type.
    /// </summary>
    public byte FluidInstance { get; private set; }

    /// <summary>
    /// Tanktype als enum-waarde (Fuel, Water, GrayWater, BlackWater, etc.).
    /// </summary>
    public FluidType FluidType { get; private set; }

    /// <summary>
    /// Raw numerieke tanktype-waarde uit het bericht.
    /// Gebruikt voor opslag van onbekende/toekomstige typen.
    /// </summary>
    public byte RawFluidType { get; private set; }

    /// <summary>
    /// Tankniveau als percentage (0-100%).
    /// Nullable; null geeft aan dat het niveau onbekend/ongeldig is.
    /// </summary>
    public decimal? LevelPercent { get; private set; }

    /// <summary>
    /// Tankcapaciteit in liters (indien aanwezig).
    /// Nullable; null geeft aan dat capaciteit niet gemeten/onbekend is.
    /// </summary>
    public decimal? CapacityLiters { get; private set; }

    /// <summary>
    /// Geeft aan of het niveau-waarde als ongeldig/onbekend beschouwd wordt.
    /// Dit is het geval wanneer de ruwe waarde 0x7FFF is.
    /// </summary>
    public bool IsLevelInvalid { get; private set; }

    private FluidLevelMeasurement() { } // Voor EF

    /// <summary>
    /// Initialiseert een nieuwe tankniveau-meting met de verplichte velden.
    /// </summary>
    public FluidLevelMeasurement(
        DateTime recordedAtUtc,
        string source,
        string messageId,
        uint pgn,
        byte fluidInstance,
        FluidType fluidType,
        byte rawFluidType,
        decimal? levelPercent,
        decimal? capacityLiters,
        bool isLevelInvalid,
        string? gatewaySentence = null,
        byte? sourceAddress = null)
    {
        RecordedAtUtc = recordedAtUtc;
        Source = source;
        MessageId = messageId;
        Pgn = pgn;
        FluidInstance = fluidInstance;
        FluidType = fluidType;
        RawFluidType = rawFluidType;
        LevelPercent = levelPercent;
        CapacityLiters = capacityLiters;
        IsLevelInvalid = isLevelInvalid;
        GatewaySentence = gatewaySentence;
        SourceAddress = sourceAddress;
    }
}

/// <summary>
/// Enum voor ondersteuning van bekende tanktypen in PGN 127505 Fluid Level.
/// Waarden corresponderen met NMEA 2000-definitie.
/// </summary>
public enum FluidType
{
    /// <summary>
    /// Brandstoftank.
    /// </summary>
    Fuel = 0,

    /// <summary>
    /// Zoetwater tank.
    /// </summary>
    FreshWater = 1,

    /// <summary>
    /// Grijs water (huishoudelijk afvalwater).
    /// </summary>
    GrayWater = 2,

    /// <summary>
    /// Zwart water / sanitair afvalwater.
    /// </summary>
    BlackWater = 3,

    /// <summary>
    /// Live well / viswater tank.
    /// </summary>
    LiveWell = 4,

    /// <summary>
    /// Motorolie / smeerolie.
    /// </summary>
    Oil = 5,

    /// <summary>
    /// Onbekend of toekomstig tanktype (fallback).
    /// </summary>
    Unknown = 7
}
