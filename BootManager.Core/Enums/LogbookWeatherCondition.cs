namespace BootManager.Core.Enums;

/// <summary>
/// Stabiele domeinwaarde voor de weerconditie die bij een logboekmoment hoort.
/// De numerieke waarden zijn expliciet vastgelegd en mogen niet wijzigen; het is de
/// bron van waarheid voor het weerbeeld. Het pictogram en label zijn presentatie in de UI-laag
/// en mogen nooit als opslagwaarde worden gebruikt.
/// </summary>
public enum LogbookWeatherCondition
{
    /// <summary>Zonnig, geen of nauwelijks bewolking.</summary>
    Zonnig = 1,

    /// <summary>Licht bewolkt.</summary>
    LichtBewolkt = 2,

    /// <summary>Half bewolkt.</summary>
    HalfBewolkt = 3,

    /// <summary>Bewolkt.</summary>
    Bewolkt = 4,

    /// <summary>Buien.</summary>
    Buien = 5,

    /// <summary>Regen.</summary>
    Regen = 6,

    /// <summary>Onweer.</summary>
    Onweer = 7,

    /// <summary>Mist.</summary>
    Mist = 8,

    /// <summary>Veel wind.</summary>
    VeelWind = 9
}
