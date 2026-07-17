namespace BootManager.Core.Enums;

/// <summary>
/// Stabiele domeinwaarde voor de gebeurtenis die bij een logboekmoment hoort.
/// De numerieke waarden zijn expliciet vastgelegd en mogen niet wijzigen, zodat
/// opgeslagen regels betekenisvast blijven. Presentatie (label/icoon) hoort in de UI-laag.
/// </summary>
public enum LogbookEventType
{
    /// <summary>Overstag gegaan (door de wind).</summary>
    Overstag = 1,

    /// <summary>Gegijpt (voor de wind).</summary>
    Gijp = 2,

    /// <summary>Zeilvoering gewijzigd (reven, wisselen, etc.).</summary>
    ZeilGewijzigd = 3,

    /// <summary>Motor gestart.</summary>
    MotorGestart = 4,

    /// <summary>Motor gestopt.</summary>
    MotorGestopt = 5,

    /// <summary>Vertrek uit haven of ankerplaats.</summary>
    Vertrek = 6,

    /// <summary>Aankomst in haven of ankerplaats.</summary>
    Aankomst = 7,

    /// <summary>Voor anker gegaan.</summary>
    VoorAnker = 8,

    /// <summary>Bijzonder moment dat vastlegging verdient.</summary>
    BijzonderMoment = 9,

    /// <summary>Alleen een momentopname zonder specifieke gebeurtenis.</summary>
    Momentopname = 10
}
