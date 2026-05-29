using System.ComponentModel.DataAnnotations;
using BootManager.Core.Enums;

namespace BootManager.Application.OperationalSettings.DTOs;

/// <summary>
/// Data transfer object voor het ophalen en wijzigen van operationele instellingen.
/// </summary>
public class OperationalSettingsDto
{
    /// <summary>IP-adres of hostname waarop de ingest-service luistert.</summary>
    [Required(ErrorMessage = "Luisteradres is verplicht.")]
    [MaxLength(256)]
    public string ListenAddress { get; set; } = "0.0.0.0";

    /// <summary>Primaire poort waarop de ingest-service luistert (1–65535).</summary>
    [Range(1, 65535, ErrorMessage = "Luisterpoort moet tussen 1 en 65535 liggen.")]
    public int ListenPort { get; set; } = 10110;

    /// <summary>Optionele alternatieve luisterpoort (1–65535).</summary>
    [Range(1, 65535, ErrorMessage = "Alternatieve poort moet tussen 1 en 65535 liggen.")]
    public int? AlternativeListenPort { get; set; }

    /// <summary>Basis-URL van de BootManager Web API.</summary>
    [Required(ErrorMessage = "API basis-URL is verplicht.")]
    [MaxLength(512)]
    [Url(ErrorMessage = "API basis-URL moet een geldige absolute URL zijn.")]
    public string ApiBaseUrl { get; set; } = "http://localhost:5046";

    /// <summary>Hoe ruwe NMEA-berichten worden opgeslagen.</summary>
    public RawStorageMode RawStorageMode { get; set; } = RawStorageMode.All;

    /// <summary>Standaard sample-interval in seconden (1–3600).</summary>
    [Range(1, 3600, ErrorMessage = "Sample-interval moet tussen 1 en 3600 seconden liggen.")]
    public int DefaultSampleIntervalSeconds { get; set; } = 10;

    /// <summary>Schakel capture-logging in of uit.</summary>
    public bool CaptureLoggingEnabled { get; set; } = false;

    /// <summary>Schakel ingest-verwerking in of uit. Als false, accepteert Ingest UDP-verkeer maar post niets naar de API.</summary>
    public bool IngestProcessingEnabled { get; set; } = true;

    /// <summary>Directory voor opslag van logboekbijlagen.</summary>
    [Required(ErrorMessage = "Logboekbijlagen-directory is verplicht.")]
    [MaxLength(1024)]
    public string LogbookAttachmentsDirectory { get; set; } = "data/logbook-attachments";
}
