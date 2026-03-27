namespace BootManager.Core.Interfaces;

/// <summary>
/// Definiëert het contract voor versleuteling en ontsleuteling van gevoelige gegevens.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Versleutelt een tekenreeks.
    /// </summary>
    /// <param name="plainText">De onversleutelde tekst.</param>
    /// <returns>Versleutelde bytes.</returns>
    byte[] Encrypt(string plainText);

    /// <summary>
    /// Ontsleutelt versleutelde bytes terug naar leesbare tekst.
    /// </summary>
    /// <param name="cipherBytes">De versleutelde bytes.</param>
    /// <returns>De ontsleutelde tekenreeks.</returns>
    string Decrypt(byte[] cipherBytes);
}