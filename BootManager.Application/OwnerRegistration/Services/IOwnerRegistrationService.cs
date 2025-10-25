using BootManager.Application.OwnerRegistration.DTOs;

namespace BootManager.Application.OwnerRegistration.Services;

public interface IOwnerRegistrationService
{
    Task<IsFirstRunResultDto> CheckFirstRunAsync(CancellationToken ct = default);
    Task<RegisterOwnerResponseDto> RegisterFirstOwnerAsync(RegisterOwnerRequestDto request, CancellationToken ct = default);
}