namespace BootManager.Core.Enums;

/// <summary>
/// Status van een logboekreis.
/// </summary>
public enum LogbookTripStatus
{
    /// <summary>
    /// Lopende reis: nog niet administratief afgerond.
    /// </summary>
    Open = 0,

    /// <summary>
    /// Afgeronde reis: administratief voltooid.
    /// </summary>
    Completed = 1
}
