namespace BootManager.Core.Enums;

/// <summary>
/// Handmatige status van een opslaglocatie-tag (QR-label).
/// </summary>
public enum TagStatus
{
    /// <summary>
    /// QR-tag is nog niet geprint.
    /// </summary>
    NotPrinted = 0,

    /// <summary>
    /// QR-tag is geprint.
    /// </summary>
    Printed = 1,

    /// <summary>
    /// QR-tag is aan boord aangebracht en actief in gebruik.
    /// </summary>
    Applied = 2,

    /// <summary>
    /// QR-tag is vervangen door een nieuw token.
    /// </summary>
    Replaced = 3
}
