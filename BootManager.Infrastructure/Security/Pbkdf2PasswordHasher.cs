using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;
using System.Security.Cryptography;

namespace BootManager.Infrastructure.Security;

public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 120_000;
    private const string AlgorithmName = "PBKDF2-SHA256";

    public HashResult Hash(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var saltBytes = new byte[SaltSize];
        rng.GetBytes(saltBytes);

        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return new HashResult(
            Convert.ToBase64String(hashBytes),
            Convert.ToBase64String(saltBytes),
            $"{AlgorithmName}:{Iterations}");
    }

    public bool Verify(string password, HashResult stored)
    {
        var parts = stored.Algorithm.Split(':');
        var iterations = (parts.Length == 2 && int.TryParse(parts[1], out var it)) ? it : Iterations;

        var saltBytes = Convert.FromBase64String(stored.Salt);
        var expectedHash = Convert.FromBase64String(stored.Hash);

        var actual = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            iterations,
            HashAlgorithmName.SHA256,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expectedHash);
    }
}