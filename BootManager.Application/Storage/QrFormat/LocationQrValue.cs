namespace BootManager.Application.Storage.QrFormat;

/// <summary>
/// Parsing en formatting van BootManager-specifieke locatie-QR-waarden.
/// Format: bootmanager:location:&lt;32-lowercase-hex-token&gt;
/// </summary>
public static class LocationQrValue
{
    private const string Prefix = "bootmanager:location:";
    private const int TokenLength = 32; // 16 bytes as 32 hex chars

    /// <summary>Format een geldige token als QR-waarde.</summary>
    public static string FormatQrValue(string token)
    {
        if (!IsValidToken(token))
            throw new ArgumentException("Ongeldige token-indeling.", nameof(token));
        return Prefix + token;
    }

    /// <summary>Parse een QR-waarde en retourneer de token, of null als het geen BootManager-locatie-QR is.</summary>
    public static string? TryParseQrValue(string? qrValue)
    {
        if (string.IsNullOrEmpty(qrValue))
            return null;

        if (!qrValue.StartsWith(Prefix, StringComparison.Ordinal))
            return null;

        var token = qrValue.Substring(Prefix.Length);
        return IsValidToken(token) ? token : null;
    }

    /// <summary>Controleer of een string een geldige token is (32 lowercase hexadecimale tekens).</summary>
    public static bool IsValidToken(string? token)
    {
        if (string.IsNullOrEmpty(token) || token.Length != TokenLength)
            return false;

        return token.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'));
    }

    /// <summary>Genereer een cryptografisch willekeurige token (16 bytes als 32 lowercase hex chars).</summary>
    public static string GenerateToken()
    {
        var bytes = new byte[16];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
