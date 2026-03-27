using BootManager.Application.PositionMeasurements.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace BootManager.Application.PositionMeasurements.Services;

/// <summary>
/// Interface voor persistentie van geïnterpreteerde positiemetingen.
/// </summary>
public interface IPositionMeasurementService
{
    /// <summary>
    /// Persisteert een nieuwe positiemeting.
    /// </summary>
    /// <param name="request">Het PositionMeasurement-record dat moet worden opgeslagen.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>De ID van het zojuist aangemaakte record.</returns>
    /// <exception cref="ArgumentNullException">Als request null is.</exception>
    /// <exception cref="ArgumentException">Als verplichte velden ontbreken of ongeldig zijn.</exception>
    Task<int> SaveAsync(CreatePositionMeasurementRequestDto request, CancellationToken cancellationToken = default);
}
