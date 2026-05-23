namespace BootManager.Web.Controllers;

/// <summary>
/// Response DTO voor het opslaan van operationele instellingen met Ingest reload status.
/// </summary>
public class SaveOperationalSettingsResponse
{
    /// <summary>
    /// Of de instellingen succesvol in de database zijn opgeslagen.
    /// </summary>
    public bool SettingsSaved { get; set; }

    /// <summary>
    /// Bericht over het opslaan van instellingen.
    /// </summary>
    public string SaveMessage { get; set; } = "";

    /// <summary>
    /// Status van de Ingest reload: "success", "failed", "unreachable", of null.
    /// </summary>
    public string? IngestReloadStatus { get; set; }

    /// <summary>
    /// Bericht over de Ingest reload.
    /// </summary>
    public string IngestReloadMessage { get; set; } = "";

    /// <summary>
    /// Lijst met velden die succesvol in Ingest zijn toegepast.
    /// </summary>
    public List<string> AppliedFields { get; set; } = new();

    /// <summary>
    /// Lijst met velden die herstart van Ingest vereisen.
    /// </summary>
    public List<string> RestartRequiredFields { get; set; } = new();
}
