namespace BootManager.Core.Interfaces;

/// <summary>
/// Abstraktie voor systeemtijd (handig voor testen).
/// </summary>
public interface ISystemClock
{
    /// <summary>
    /// Huidige UTC-tijd.
    /// </summary>
    DateTime UtcNow { get; }
}

public sealed class SystemClock : ISystemClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}