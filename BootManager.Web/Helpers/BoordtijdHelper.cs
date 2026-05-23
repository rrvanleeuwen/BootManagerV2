using System;

namespace BootManager.Web.Helpers;

/// <summary>
/// Hulpklasse voor conversie tussen UTC en lokale boordtijd (Europe/Amsterdam).
/// Gebruikt cross-platform fallbacks voor Windows en Linux/macOS.
/// </summary>
public static class BoordtijdHelper
{
    private static readonly TimeZoneInfo _boordtijdZone = LaadZone();

    private static TimeZoneInfo LaadZone()
    {
        // IANA (Linux/macOS)
        try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam"); }
        catch { }
        // Windows
        try { return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"); }
        catch { }
        return TimeZoneInfo.Local;
    }

    /// <summary>
    /// Converteert een UTC-tijdstempel naar lokale boordtijd.
    /// </summary>
    public static DateTime NaarLokaal(DateTime utc)
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), _boordtijdZone);

    /// <summary>
    /// Interpreteert een lokale boordtijd als UTC.
    /// </summary>
    public static DateTime NaarUtc(DateTime lokaal)
        => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(lokaal, DateTimeKind.Unspecified), _boordtijdZone);
}
