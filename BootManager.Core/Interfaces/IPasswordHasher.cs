using BootManager.Core.ValueObjects;

namespace BootManager.Core.Interfaces;

/// <summary>
/// Definiëert het contract voor wachtwoordhashing en verificatie.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Haalt een wachtwoord en retourneert een gehashte versie met salt.
    /// </summary>
    /// <param name="password">Het wachtwoord om te hashen.</param>
    /// <returns>HashResult met hash en salt.</returns>
    HashResult Hash(string password);

    /// <summary>
    /// Verifieert of een ingevoerd wachtwoord overeenkomt met de opgeslagen hash.
    /// </summary>
    /// <param name="password">Het in te voeren wachtwoord.</param>
    /// <param name="stored">De eerder opgeslagen hash en salt.</param>
    /// <returns>True als het wachtwoord geldig is, anders false.</returns>
    bool Verify(string password, HashResult stored);
}