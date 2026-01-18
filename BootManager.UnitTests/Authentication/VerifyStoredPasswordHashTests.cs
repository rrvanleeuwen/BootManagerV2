using System.Security.Cryptography;
using Xunit;

namespace BootManager.UnitTests.Authentication;

public class VerifyStoredPasswordHashTests
{
    private const string StoredHash = "McK3TCWQjm383XfNWCQluZaedOejpHl+oG2ghHpCXSU=";
    private const string StoredSalt = "nDxHhVAO7vxoR1eBfUM3VA==";
    private const string Algorithm = "PBKDF2-SHA256:120000";

    [Fact]
    public void OldPassword_ShouldMatchStoredHash()
    {
        var candidate = "123456";
        var matches = Verify(candidate);
        Assert.True(matches, "Expected stored hash to match candidate '123456'.");
    }

    [Fact]
    public void NewPassword_ShouldNotMatchStoredHash()
    {
        var candidate = "1234abcd";
        var matches = Verify(candidate);
        Assert.False(matches, "Expected stored hash NOT to match candidate '1234abcd'.");
    }

    private static bool Verify(string candidate)
    {
        var parts = Algorithm.Split(':');
        var iterations = (parts.Length == 2 && int.TryParse(parts[1], out var it)) ? it : 120000;
        var salt = Convert.FromBase64String(StoredSalt);
        var expected = Convert.FromBase64String(StoredHash);
        var actual = Rfc2898DeriveBytes.Pbkdf2(candidate, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
