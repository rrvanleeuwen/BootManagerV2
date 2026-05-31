namespace BootManager.Web.Services;

/// <summary>
/// Abstraktie voor veilige, begrensde uitvoering van shutdown-helper script.
/// </summary>
public interface IShutdownHelperExecutor
{
    /// <summary>
    /// Voert het shutdown-helper-script uit met strikte veiligheidsmaatregelen.
    /// </summary>
    /// <remarks>
    /// - Validaat dat het script aanwezig en uitvoerbaar is
    /// - Geen shell-injectie mogelijk (UseShellExecute=false)
    /// - Geen arguments; het script bepaalt zijn eigen logica
    /// - Fire-and-forget: start het process, wacht niet op completion
    /// </remarks>
    /// <param name="helperScriptPath">Absolute pad naar shutdown-helper-script.</param>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>Task die voltooid is wanneer helper gestart is.</returns>
    /// <exception cref="InvalidOperationException">
    /// Wanneer script niet aanwezig, niet leesbaar, niet uitvoerbaar is,
    /// of wanneer process niet kan starten.
    /// </exception>
    Task ExecuteHelperAsync(string helperScriptPath, CancellationToken ct = default);
}
