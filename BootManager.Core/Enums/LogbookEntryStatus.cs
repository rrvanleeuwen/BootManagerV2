namespace BootManager.Core.Enums;

/// <summary>
/// Status van een logboekregel in de akkoordflow.
/// </summary>
public enum LogbookEntryStatus
{
    /// <summary>
    /// Conceptregel: nog niet geaccordeerd door de gebruiker.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Definitieve regel: geaccordeerd en klaar voor het officiële logboek.
    /// </summary>
    Confirmed = 1
}
