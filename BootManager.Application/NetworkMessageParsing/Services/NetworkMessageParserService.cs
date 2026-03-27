namespace BootManager.Application.NetworkMessageParsing.Services;

using DTOs;
using Enums;

/// <summary>
/// Concrete implementatie van de netwerkbericht-parser service.
/// 
/// Voert technische parsing uit: PGN-herkenning, type-classificatie en hex-to-byte conversie.
/// Dit is GEEN domein-gerelateerde decoding.
/// 
/// De parser normaliseert MessageId's robuust om zowel hex-PGN's (uit huidige simulator)
/// als decimale PGN-strings (toekomstige raw input) betrouwbaar te classificeren.
/// </summary>
internal class NetworkMessageParserService : INetworkMessageParserService
{
    /// <summary>
    /// Mapping van genormaliseerde PGN-waarden (decimaal) naar berichttypen.
    /// 
    /// Deze mapping is gebaseerd op werkelijke NMEA 2000-semantiek:
    /// - 129025: Position (Rapid Update)
    /// - 129026: COG/SOG (Course Over Ground, Speed Over Ground)
    /// - 127250: Heading (Vessel Heading)
    /// - 130306: Wind Data
    /// - 128267: Water Depth (Rapid Update)
    /// - 127508: Battery Status
    /// </summary>
    private static readonly Dictionary<uint, NetworkMessageType> PgnToType = new()
    {
        { 129025, NetworkMessageType.Position },
        { 129026, NetworkMessageType.Motion },
        { 127250, NetworkMessageType.Heading },
        { 130306, NetworkMessageType.Wind },
        { 128267, NetworkMessageType.Depth },
        { 127508, NetworkMessageType.Battery }
    };

    /// <summary>
    /// Legacy-mapping voor oudere/arbitraire message IDs (backward compatibility).
    /// </summary>
    private static readonly Dictionary<string, NetworkMessageType> LegacyIdToType = new(StringComparer.OrdinalIgnoreCase)
    {
        { "0A1B2C3D", NetworkMessageType.Position },
        { "0A1B2C3E", NetworkMessageType.Motion },
        { "0A1B2C3F", NetworkMessageType.Wind },
        { "0A1B2C40", NetworkMessageType.Depth },
        { "0A1B2C41", NetworkMessageType.Battery }
    };

    /// <summary>
    /// Parseert een netwerkbericht.
    /// 
    /// Herkent:
    /// - Hex-PGN's (bijv. "01F801" voor PGN 129025)
    /// - Decimale PGN-strings (bijv. "129025")
    /// - Legacy arbitraire IDs (bijv. "0A1B2C3D")
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

    /// <summary>
    /// Bepaalt het berichttype op basis van MessageId.
    /// 
    /// Normaliseert eerst de MessageId naar een standaardformaat (genormaliseerde PGN-waarde),
    /// en classificeert vervolgens.
    /// </summary>
    private static NetworkMessageType GetMessageType(string messageIdHex)
    {
        var normalizedPgn = NormalizeMessageId(messageIdHex);

        // Probeer PGN-mapping
        if (normalizedPgn.HasValue && PgnToType.TryGetValue(normalizedPgn.Value, out var type))
        {
            return type;
        }

        // Fall-back naar legacy IDs
        if (LegacyIdToType.TryGetValue(messageIdHex, out var legacyType))
        {
            return legacyType;
        }

        return NetworkMessageType.Unknown;
    }

    /// <summary>
    /// Normaliseert een MessageId naar een genormaliseerde PGN-waarde (uint).
    /// 
    /// Ondersteunt:
    /// - Hex-strings (bijv. "01F801" → 129025)
    /// - Decimale strings (bijv. "129025" → 129025)
    /// - Case-insensitief verwerking
    /// 
    /// Retourneert null als normalisatie mislukt.
    /// </summary>
    private static uint? NormalizeMessageId(string messageId)
    {
        if (string.IsNullOrWhiteSpace(messageId))
        {
            return null;
        }

        var trimmedId = messageId.Trim();

        // Poging 1: Probeer als decimale string
        if (uint.TryParse(trimmedId, System.Globalization.NumberStyles.None, null, out var decimalValue))
        {
            return decimalValue;
        }

        // Poging 2: Probeer als hex-string (zonder 0x-prefix)
        // Hex-strings uit de simulator zijn 6 karakters (bijv. "01F801" voor 129025)
        if (uint.TryParse(trimmedId, System.Globalization.NumberStyles.HexNumber, null, out var hexValue))
        {
            return hexValue;
        }

        return null;
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

        var cleanedHex = payloadHex.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");

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