namespace BootManager.Web.Options;

/// <summary>
/// Configuratieopties voor veilige systeemshutdown via Unix domain socket.
/// </summary>
public class ShutdownOptions
{
    /// <summary>
    /// Sectienaam in appsettings.json voor configuratie.
    /// </summary>
    public const string SectionName = "Shutdown";

    /// <summary>
    /// Pad naar het shutdown-helper Unix domain socket op de Pi.
    /// Default: /run/bootmanager/shutdown.sock
    ///
    /// Dit socket moet beschikbaar zijn op de host en gemount zijn in de Docker container.
    /// De host draait een systemd service die luistert op dit socket en accepts het SHUTDOWN commando.
    ///
    /// Production deployment vereist:
    /// 1. Host-side systemd service: bootmanager-shutdown.service
    /// 2. Socket aangemaakt in /run/bootmanager/shutdown.sock
    /// 3. Docker Compose mount: - /run/bootmanager/shutdown.sock:/run/bootmanager/shutdown.sock:ro
    /// </summary>
    public string HelperSocketPath { get; set; } = "/run/bootmanager/shutdown.sock";

    /// <summary>
    /// Geeft aan of shutdown in development-mode getest/geïmiteerd moet worden.
    /// True: alleen loggen. False: werking testen (als socket aanwezig is).
    /// </summary>
    public bool AllowTestExecutionInDevelopment { get; set; } = false;
}
