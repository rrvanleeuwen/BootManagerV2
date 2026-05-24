namespace BootManager.Application.OwnerRegistration.Services;

/// <summary>
/// Beheert de bootstrap-flow voor de eerste eigenaar bij een lege database.
/// </summary>
public interface IBootstrapOwnerService
{
    /// <summary>
    /// Zet de bootstrap eigenaar op als de database leeg is.
    /// </summary>
    /// <param name="bootstrapPassword">Het wachtwoord voor de bootstrap eigenaar uit configuratie.</param>
    /// <param name="isProduction">Of de applicatie in Production-mode draait.</param>
    /// <param name="ct">Annuleringstoken.</param>
    /// <returns>true als bootstrap eigenaar aangemaakt, false als er al een eigenaar bestond.</returns>
    /// <exception cref="InvalidOperationException">In Production als bootstrapPassword leeg/null is.</exception>
    Task<bool> EnsureBootstrapOwnerAsync(string? bootstrapPassword, bool isProduction, CancellationToken ct = default);
}
