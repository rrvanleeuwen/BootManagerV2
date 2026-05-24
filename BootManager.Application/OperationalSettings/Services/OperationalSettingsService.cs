using BootManager.Application.OperationalSettings.DTOs;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Core.Interfaces;

namespace BootManager.Application.OperationalSettings.Services;

/// <summary>
/// Service voor het beheren van operationele instellingen.
/// Maakt standaardinstellingen aan bij eerste gebruik.
/// </summary>
public class OperationalSettingsService : IOperationalSettingsService
{
    private const string DefaultLogbookAttachmentsDirectory = "data/logbook-attachments";

    private readonly IRepository<Core.Entities.OperationalSettings> _repository;
    private readonly ISystemClock _clock;

    /// <summary>
    /// Initialiseert een nieuw exemplaar van <see cref="OperationalSettingsService"/>.
    /// </summary>
    public OperationalSettingsService(
        IRepository<Core.Entities.OperationalSettings> repository,
        ISystemClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    /// <inheritdoc />
    public async Task<OperationalSettingsDto> GetAsync(CancellationToken ct = default)
    {
        var settings = await _repository.SingleOrDefaultAsync(null, ct);

        if (settings is null)
        {
            settings = Core.Entities.OperationalSettings.CreateDefaults(_clock.UtcNow);
            await _repository.AddAsync(settings, ct);
        }

        return MapToDto(settings);
    }

     /// <inheritdoc />
    public async Task SaveAsync(OperationalSettingsDto dto, CancellationToken ct = default)
    {
        Validate(dto);

        var settings = await _repository.SingleOrDefaultAsync(null, ct);

        if (settings is null)
        {
            settings = Core.Entities.OperationalSettings.CreateDefaults(_clock.UtcNow);
            settings.Update(
                dto.ListenAddress,
                dto.ListenPort,
                dto.AlternativeListenPort,
                dto.ApiBaseUrl,
                dto.RawStorageMode,
                dto.DefaultSampleIntervalSeconds,
                dto.CaptureLoggingEnabled,
                dto.LogbookAttachmentsDirectory,
                _clock.UtcNow);
            await _repository.AddAsync(settings, ct);
        }
        else
        {
            settings.Update(
                dto.ListenAddress,
                dto.ListenPort,
                dto.AlternativeListenPort,
                dto.ApiBaseUrl,
                dto.RawStorageMode,
                dto.DefaultSampleIntervalSeconds,
                dto.CaptureLoggingEnabled,
                dto.LogbookAttachmentsDirectory,
                _clock.UtcNow);
            await _repository.UpdateAsync(settings, ct);
        }
    }

    private static OperationalSettingsDto MapToDto(Core.Entities.OperationalSettings s) =>
        new()
        {
            ListenAddress = s.ListenAddress,
            ListenPort = s.ListenPort,
            AlternativeListenPort = s.AlternativeListenPort,
            ApiBaseUrl = s.ApiBaseUrl,
            RawStorageMode = s.RawStorageMode,
            DefaultSampleIntervalSeconds = s.DefaultSampleIntervalSeconds,
            CaptureLoggingEnabled = s.CaptureLoggingEnabled,
            LogbookAttachmentsDirectory = string.IsNullOrWhiteSpace(s.LogbookAttachmentsDirectory)
                ? DefaultLogbookAttachmentsDirectory
                : s.LogbookAttachmentsDirectory
        };

    private static void Validate(OperationalSettingsDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.ListenAddress))
            throw new ArgumentException("Luisteradres is verplicht.", nameof(dto.ListenAddress));

        ValidatePort(dto.ListenPort, nameof(dto.ListenPort));

        if (dto.AlternativeListenPort.HasValue)
            ValidatePort(dto.AlternativeListenPort.Value, nameof(dto.AlternativeListenPort));

        if (!Uri.TryCreate(dto.ApiBaseUrl, UriKind.Absolute, out var apiUri)
            || string.IsNullOrWhiteSpace(apiUri.Scheme)
            || string.IsNullOrWhiteSpace(apiUri.Host))
        {
            throw new ArgumentException("API basis-URL moet een geldige absolute URL zijn.", nameof(dto.ApiBaseUrl));
        }

        if (!Enum.IsDefined(typeof(RawStorageMode), dto.RawStorageMode))
            throw new ArgumentException("Raw opslagmodus is ongeldig.", nameof(dto.RawStorageMode));

        if (dto.DefaultSampleIntervalSeconds < 1 || dto.DefaultSampleIntervalSeconds > 3600)
            throw new ArgumentException("Sample-interval moet tussen 1 en 3600 seconden liggen.", nameof(dto.DefaultSampleIntervalSeconds));

        if (string.IsNullOrWhiteSpace(dto.LogbookAttachmentsDirectory))
            throw new ArgumentException("Logboekbijlagen-directory is verplicht.", nameof(dto.LogbookAttachmentsDirectory));
    }

    private static void ValidatePort(int port, string parameterName)
    {
        if (port < 1 || port > 65535)
            throw new ArgumentException("Poort moet tussen 1 en 65535 liggen.", parameterName);
    }
}
