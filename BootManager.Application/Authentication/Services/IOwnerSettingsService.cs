using BootManager.Application.Authentication.DTOs;

namespace BootManager.Application.Authentication.Services;

public interface IOwnerSettingsService
{
    Task ChangePasswordAsync(ChangePasswordRequestDto request, CancellationToken ct = default);
    Task SetPinAsync(ChangePinRequestDto request, CancellationToken ct = default);
    Task ClearPinAsync(CancellationToken ct = default);
}
