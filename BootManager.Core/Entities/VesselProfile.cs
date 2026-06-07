namespace BootManager.Core.Entities;

/// <summary>
/// Domein-entiteit voor het bootprofiel. Er is maximaal 1 record per installatie (singleton).
/// Bevat bootnaam, thuishaven, roepnaam en MMSI.
/// </summary>
public class VesselProfile
{
    /// <summary>
    /// Unieke identifier voor het bootprofiel.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Bootnaam (verplicht).
    /// </summary>
    public string VesselName { get; private set; } = default!;

    /// <summary>
    /// Thuishaven (optioneel).
    /// </summary>
    public string? HomePort { get; private set; }

    /// <summary>
    /// Roepnaam (optioneel).
    /// </summary>
    public string? CallSign { get; private set; }

    /// <summary>
    /// MMSI-nummer (optioneel).
    /// </summary>
    public string? Mmsi { get; private set; }

    /// <summary>
    /// Actuele motorurenstand (optioneel, niet-negatief). Kan bewust worden gereset.
    /// </summary>
    public decimal? CurrentEngineHours { get; private set; }

    /// <summary>
    /// Actuele logstandwaarde in zeemijlen (optioneel, niet-negatief). Kan bewust worden gereset.
    /// </summary>
    public decimal? CurrentLogstand { get; private set; }

    /// <summary>
    /// Moment waarop het profiel is aangemaakt (UTC).
    /// </summary>
    public DateTime CreatedUtc { get; private set; }

    /// <summary>
    /// Moment waarop het profiel voor het laatst is bijgewerkt (UTC).
    /// </summary>
    public DateTime? UpdatedUtc { get; private set; }

    private VesselProfile() { } // Voor EF

    private VesselProfile(string vesselName, string? homePort, string? callSign, string? mmsi, DateTime createdUtc, decimal? currentEngineHours = null, decimal? currentLogstand = null)
    {
        VesselName = vesselName;
        HomePort = homePort;
        CallSign = callSign;
        Mmsi = mmsi;
        CreatedUtc = createdUtc;
        CurrentEngineHours = currentEngineHours;
        CurrentLogstand = currentLogstand;
    }

    /// <summary>
    /// Maakt een nieuw bootprofiel aan.
    /// </summary>
    /// <param name="vesselName">Bootnaam (verplicht).</param>
    /// <param name="homePort">Thuishaven (optioneel).</param>
    /// <param name="callSign">Roepnaam (optioneel).</param>
    /// <param name="mmsi">MMSI-nummer (optioneel).</param>
    /// <param name="createdUtc">Aanmaaktijd (UTC).</param>
    /// <param name="currentEngineHours">Actuele motorurenstand (optioneel).</param>
    /// <param name="currentLogstand">Actuele logstandwaarde in zeemijlen (optioneel).</param>
    /// <returns>Nieuw VesselProfile.</returns>
    public static VesselProfile Create(string vesselName, string? homePort = null, string? callSign = null, string? mmsi = null, DateTime? createdUtc = null, decimal? currentEngineHours = null, decimal? currentLogstand = null)
    {
        var now = createdUtc ?? DateTime.UtcNow;
        return new VesselProfile(vesselName, homePort, callSign, mmsi, now, currentEngineHours, currentLogstand);
    }

    /// <summary>
    /// Werkt het bootprofiel bij.
    /// </summary>
    /// <param name="vesselName">Nieuwe bootnaam.</param>
    /// <param name="homePort">Nieuwe thuishaven.</param>
    /// <param name="callSign">Nieuwe roepnaam.</param>
    /// <param name="mmsi">Nieuw MMSI-nummer.</param>
    /// <param name="currentEngineHours">Actuele motorurenstand (optioneel).</param>
    /// <param name="currentLogstand">Actuele logstandwaarde in zeemijlen (optioneel).</param>
    /// <param name="updatedUtc">Update-tijd (UTC).</param>
    public void Update(string vesselName, string? homePort = null, string? callSign = null, string? mmsi = null, decimal? currentEngineHours = null, decimal? currentLogstand = null, DateTime? updatedUtc = null)
    {
        VesselName = vesselName;
        HomePort = homePort;
        CallSign = callSign;
        Mmsi = mmsi;
        // Dit is een volledige Settings-update: null wist de optionele tellerstand.
        CurrentEngineHours = currentEngineHours;
        CurrentLogstand = currentLogstand;
        UpdatedUtc = updatedUtc ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Werkt alleen actuele motorurenstand bij.
    /// </summary>
    /// <param name="currentEngineHours">Nieuwe actuele motorurenstand.</param>
    /// <param name="updatedUtc">Update-tijd (UTC).</param>
    public void UpdateCurrentEngineHours(decimal? currentEngineHours, DateTime? updatedUtc = null)
    {
        CurrentEngineHours = currentEngineHours;
        UpdatedUtc = updatedUtc ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Werkt alleen actuele logstandwaarde bij.
    /// </summary>
    /// <param name="currentLogstand">Nieuwe actuele logstandwaarde.</param>
    /// <param name="updatedUtc">Update-tijd (UTC).</param>
    public void UpdateCurrentLogstand(decimal? currentLogstand, DateTime? updatedUtc = null)
    {
        CurrentLogstand = currentLogstand;
        UpdatedUtc = updatedUtc ?? DateTime.UtcNow;
    }
}
