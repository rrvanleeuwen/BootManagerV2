using BootManager.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace BootManager.Infrastructure.Security;

public class AesGcmEncryptionService : IEncryptionService
{
    private readonly byte[] _key; // 32 bytes AES-256

    public AesGcmEncryptionService(string keyMaterial)
    {
        // Verbeterbaar: PBKDF2 + salt loaded uit config / file
        using var sha = SHA256.Create();
        _key = sha.ComputeHash(Encoding.UTF8.GetBytes(keyMaterial));
    }

    public byte[] Encrypt(string plainText)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plaintextBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[nonce.Length + plaintextBytes.Length + 16]; // nonce + cipher + tag

        var cipherSpan = cipherBytes.AsSpan();
        nonce.CopyTo(cipherSpan[..12]);

        var ciphertextSpan = cipherSpan.Slice(12, plaintextBytes.Length);
        var tagSpan = cipherSpan.Slice(12 + plaintextBytes.Length, 16);

        using var aes = new AesGcm(_key);
        aes.Encrypt(nonce, plaintextBytes, ciphertextSpan, tagSpan);
        return cipherBytes;
    }

    public string Decrypt(byte[] cipherBytes)
    {
        var nonce = cipherBytes.AsSpan(0, 12);
        var tag = cipherBytes.AsSpan(cipherBytes.Length - 16, 16);
        var ciphertext = cipherBytes.AsSpan(12, cipherBytes.Length - 12 - 16);

        var plaintext = new byte[ciphertext.Length];
        using var aes = new AesGcm(_key);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return Encoding.UTF8.GetString(plaintext);
    }
}