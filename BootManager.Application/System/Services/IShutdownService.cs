namespace BootManager.Application.Administration.Services;

/// <summary>
/// Service-interface voor het beheren van systeemshutdown-acties.
/// </summary>
public interface IShutdownService
{
    /// <summary>
    /// Initieert een veilige shutdown van het systeem (Raspberry Pi).
    /// </summary>
    /// <remarks>
    /// Deze operatie voert een begrensde shutdown-actie uit:
    /// - In dev-mode: logt alleen een waarschuwing, geen echte shutdown.
    /// - In production: roept een veilig shutdown-script of systeem-commando aan.
    /// </remarks>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>Een task die voltooid is wanneer de shutdown-initialisering afgerond is.</returns>
    /// <exception cref="InvalidOperationException">Wanneer de shutdown-helper niet beschikbaar is.</exception>
    Task InitiateShutdownAsync(CancellationToken ct = default);
}
