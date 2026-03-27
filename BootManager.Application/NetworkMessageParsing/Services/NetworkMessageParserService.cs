namespace BootManager.Application.NetworkMessageParsing.Services;

using DTOs;
using Enums;

/// <summary>
/// Concrete implementatie van de netwerkbericht-parser service.
/// 
/// Voert technische parsing uit: ID-herkenning, type-classificatie en hex-to-byte conversie.
/// Dit is GEEN domein-gerelateerde decoding.
/// </summary>
internal class NetworkMessageParserService : INetworkMessageParserService
{
    private static readonly Dictionary<string, NetworkMessageType> MessageIdToType = new(StringComparer.OrdinalIgnoreCase)
    {
        { "0A1B2C3D", NetworkMessageType.Position },
        { "0A1B2C3E", NetworkMessageType.Motion },
        { "0A1B2C3F", NetworkMessageType.Wind },
        { "0A1B2C40", NetworkMessageType.Depth },
        { "0A1B2C41", NetworkMessageType.Battery }
    };

    /// <summary>
    /// Parseert een netwerkbericht.
    /// </summary>
    public NetworkMessageParseResultDto Parse(NetworkMessageParseRequestDto request)
    {
        if (request == null || !IsValidRequest(request))
        {
            return new NetworkMessageParseResultDto
            {
                IsSuccess = false,
                MessageType = NetworkMessageType.Unknown,
                MessageIdHex = request?.MessageIdHex ?? string.Empty,
                PayloadBytes = [],
                ErrorMessage = "Ongeldige of onvolledige parseer-aanvraag."
            };
        }

        var messageType = GetMessageType(request.MessageIdHex);

        var payloadBytes = TryParseHexPayload(request.PayloadHex, out var bytes, out var parseError);
        if (!payloadBytes)
        {
            return new NetworkMessageParseResultDto
            {
                IsSuccess = false,
                MessageType = messageType,
                MessageIdHex = request.MessageIdHex,
                PayloadBytes = [],
                ErrorMessage = parseError
            };
        }

        return new NetworkMessageParseResultDto
        {
            IsSuccess = true,
            MessageType = messageType,
            MessageIdHex = request.MessageIdHex,
            PayloadBytes = bytes!,
            ErrorMessage = null
        };
    }

    private static bool IsValidRequest(NetworkMessageParseRequestDto request)
    {
        return !string.IsNullOrWhiteSpace(request.MessageIdHex) &&
               !string.IsNullOrWhiteSpace(request.PayloadHex);
    }

    private static NetworkMessageType GetMessageType(string messageIdHex)
    {
        return MessageIdToType.TryGetValue(messageIdHex, out var type) ? type : NetworkMessageType.Unknown;
    }

    private static bool TryParseHexPayload(string payloadHex, out byte[]? bytes, out string? error)
    {
        bytes = null;
        error = null;

        if (string.IsNullOrWhiteSpace(payloadHex))
        {
            error = "Payload hex-string is leeg.";
            return false;
        }

        // Verwijder whitespace
        var cleanedHex = payloadHex.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");

        // Controleer of de lengte even is (elke byte is 2 hex-karakters)
        if (cleanedHex.Length % 2 != 0)
        {
            error = "Payload hex-string heeft oneven aantal karakters.";
            return false;
        }

        try
        {
            bytes = Convert.FromHexString(cleanedHex);
            return true;
        }
        catch (FormatException)
        {
            error = "Payload bevat ongeldige hexadecimale karakters.";
            return false;
        }
    }
}