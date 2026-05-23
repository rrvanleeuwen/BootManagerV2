namespace BootManager.Tools.Ingest.Services;

/// <summary>
/// Interface voor de client-service die operationele instellingen ophaalt bij BootManager.Web.
/// </summary>
public interface IOperationalSettingsClientService
{
    /// <summary>
    /// Probeert operationele instellingen op te halen bij BootManager.Web.
    /// </summary>
    /// <param name="baseUrl">De basis-URL van BootManager.Web (uit appsettings).</param>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>
    /// Het settings-model als ophalen lukt, anders <c>null</c>.
    /// </returns>
    Task<IngestRemoteSettings?> TryGetSettingsAsync(string baseUrl, CancellationToken ct = default);
}
