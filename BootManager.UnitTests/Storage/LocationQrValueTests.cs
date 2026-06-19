using BootManager.Application.Storage.QrFormat;

namespace BootManager.UnitTests.Storage;

/// <summary>
/// Unit tests for LocationQrValue formatter/parser.
/// Tests validation, formatting, token generation, and parsing of BootManager location QR values.
/// </summary>
public class LocationQrValueTests
{
    private const string ValidFormat = "bootmanager:location:";

    [Fact]
    public void GenerateToken_Returns32LowercaseHexCharacters()
    {
        var token = LocationQrValue.GenerateToken();

        Assert.Equal(32, token.Length);
        Assert.True(token.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')),
            "Token contains only lowercase hex characters");
    }

    [Fact]
    public void GenerateToken_ProducesUniqueTokens()
    {
        var token1 = LocationQrValue.GenerateToken();
        var token2 = LocationQrValue.GenerateToken();
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void IsValidToken_AcceptsExactly32LowercaseHexCharacters()
    {
        var validToken = "0123456789abcdef0123456789abcdef";
        Assert.True(LocationQrValue.IsValidToken(validToken));
    }

    [Fact]
    public void IsValidToken_RejectsNull()
    {
        Assert.False(LocationQrValue.IsValidToken(null));
    }

    [Fact]
    public void IsValidToken_RejectsEmpty()
    {
        Assert.False(LocationQrValue.IsValidToken(""));
    }

    [Fact]
    public void IsValidToken_RejectsTooShort()
    {
        Assert.False(LocationQrValue.IsValidToken("0123456789abcdef0123456789abcde"));
    }

    [Fact]
    public void IsValidToken_RejectsTooLong()
    {
        Assert.False(LocationQrValue.IsValidToken("0123456789abcdef0123456789abcdef0"));
    }

    [Fact]
    public void IsValidToken_RejectsUppercaseHex()
    {
        Assert.False(LocationQrValue.IsValidToken("0123456789ABCDEF0123456789ABCDEF"));
    }

    [Fact]
    public void IsValidToken_RejectsNonHexCharacters()
    {
        Assert.False(LocationQrValue.IsValidToken("0123456789abcdef0123456789abcdeg"));
        Assert.False(LocationQrValue.IsValidToken("0123456789abcdef0123456789abcde!"));
    }

    [Fact]
    public void FormatQrValue_ReturnsCorrectFormat()
    {
        var token = "0123456789abcdef0123456789abcdef";
        var qrValue = LocationQrValue.FormatQrValue(token);

        Assert.Equal("bootmanager:location:0123456789abcdef0123456789abcdef", qrValue);
    }

    [Fact]
    public void FormatQrValue_ThrowsOnInvalidToken()
    {
        Assert.Throws<ArgumentException>(() => LocationQrValue.FormatQrValue("invalid"));
        Assert.Throws<ArgumentException>(() => LocationQrValue.FormatQrValue(""));
        Assert.Throws<ArgumentException>(() => LocationQrValue.FormatQrValue(null!));
    }

    [Fact]
    public void TryParseQrValue_ReturnsTokenForValidQrValue()
    {
        var token = "0123456789abcdef0123456789abcdef";
        var qrValue = "bootmanager:location:" + token;

        var parsed = LocationQrValue.TryParseQrValue(qrValue);

        Assert.Equal(token, parsed);
    }

    [Fact]
    public void TryParseQrValue_ReturnsNullForInvalidPrefix()
    {
        var qrValue = "other:format:0123456789abcdef0123456789abcdef";
        var parsed = LocationQrValue.TryParseQrValue(qrValue);

        Assert.Null(parsed);
    }

    [Fact]
    public void TryParseQrValue_ReturnsNullForInvalidToken()
    {
        var qrValue = "bootmanager:location:0123456789abcdef0123456789abcdeg";
        var parsed = LocationQrValue.TryParseQrValue(qrValue);

        Assert.Null(parsed);
    }

    [Fact]
    public void TryParseQrValue_ReturnsNullForNull()
    {
        var parsed = LocationQrValue.TryParseQrValue(null);
        Assert.Null(parsed);
    }

    [Fact]
    public void TryParseQrValue_ReturnsNullForEmpty()
    {
        var parsed = LocationQrValue.TryParseQrValue("");
        Assert.Null(parsed);
    }

    [Fact]
    public void FormatAndParse_RoundTripSuccessfully()
    {
        var originalToken = LocationQrValue.GenerateToken();
        var qrValue = LocationQrValue.FormatQrValue(originalToken);
        var parsedToken = LocationQrValue.TryParseQrValue(qrValue);

        Assert.Equal(originalToken, parsedToken);
    }
}
