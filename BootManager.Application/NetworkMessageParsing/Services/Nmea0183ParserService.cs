namespace BootManager.Application.NetworkMessageParsing.Services;

using DTOs;
using Microsoft.Extensions.Logging;

/// <summary>
/// Concrete implementatie van de NMEA 0183 parser service.
///
/// Voert technische parsing uit:
/// - sentence-structuur validatie
/// - talker-prefix en sentence-type herkenning
/// - veld-extractie
/// - optionele checksum-validatie
///
/// Voert GEEN semantische interpretatie uit. Dit is een tussenstap voor Fase 3 interpreters.
/// </summary>
public class Nmea0183ParserService : INmea0183ParserService
{
    private readonly ILogger<Nmea0183ParserService> _logger;

    /// <summary>
    /// Initialiseert een nieuwe instantie van <see cref="Nmea0183ParserService"/>.
    /// </summary>
    public Nmea0183ParserService(ILogger<Nmea0183ParserService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parseert een NMEA 0183 sentence technisch.
    ///
    /// Verwacht format: $&lt;talker&gt;&lt;type&gt;,&lt;veld1&gt;,...[*&lt;XX&gt;]
    /// Talker is minimaal 1 teken, type minimaal 3 tekens.
    /// Talker-prefixen van 2 tekens zijn standaard (bijv. "II", "GP", "HC", "WI").
    /// </summary>
    public Nmea0183ParseResultDto Parse(string rawSentence)
    {
        var result = new Nmea0183ParseResultDto
        {
            RawSentence = rawSentence ?? string.Empty
        };

        if (string.IsNullOrWhiteSpace(rawSentence))
        {
            result.IsSuccess = false;
            result.ErrorMessage = "Lege NMEA 0183 sentence ontvangen.";
            _logger.LogWarning("Lege NMEA 0183 sentence ontvangen.");
            return result;
        }

        var sentence = rawSentence.Trim();

        // NMEA 0183 sentences beginnen met '$' of '!' (AIS uses '!' for AIVDM/AIVDO)
        if (!(sentence.StartsWith('$') || sentence.StartsWith('!')))
        {
            result.IsSuccess = false;
            result.ErrorMessage = $"Sentence begint niet met '$' of '!': {sentence}";
            _logger.LogWarning("Ongeldige NMEA 0183 sentence (geen '$' of '!'): {Sentence}", sentence);
            return result;
        }

        // Verwijder leading start-character ('$' of '!')
        var body = sentence[1..];

        // Splits checksum af indien aanwezig (*XX aan het einde)
        string? checksumHex = null;
        var starIndex = body.IndexOf('*');
        if (starIndex >= 0)
        {
            checksumHex = body[(starIndex + 1)..].Trim();
            body = body[..starIndex];
        }

        // body = "<talker+type>,veld1,veld2,..."
        var commaIndex = body.IndexOf(',');
        string sentenceId;
        string[] fields;

        if (commaIndex >= 0)
        {
            sentenceId = body[..commaIndex];
            fields = body[(commaIndex + 1)..].Split(',');
        }
        else
        {
            // Geen komma: alleen sentence-id, geen velden
            sentenceId = body;
            fields = [];
        }

        if (sentenceId.Length < 3)
        {
            result.IsSuccess = false;
            result.ErrorMessage = $"Sentence-ID te kort: '{sentenceId}'";
            _logger.LogWarning("NMEA 0183 sentence-ID te kort: '{SentenceId}'", sentenceId);
            return result;
        }

        // Talker is alles behalve de laatste 3 tekens; sentence-type is de laatste 3 tekens
        // Standaard: 2-teken talker + 3-teken type (bijv. "IIVHW" → talker "II", type "VHW")
        // Enkelteken talker ook toegestaan (bijv. "AVHW" → talker "A", type "VHW")
        var typeLength = 3;
        var talkerLength = sentenceId.Length - typeLength;

        if (talkerLength < 0)
        {
            result.IsSuccess = false;
            result.ErrorMessage = $"Sentence-ID te kort voor type-extractie: '{sentenceId}'";
            _logger.LogWarning("NMEA 0183 sentence-ID te kort voor type-extractie: '{SentenceId}'", sentenceId);
            return result;
        }

        result.TalkerPrefix = sentenceId[..talkerLength];
        result.SentenceType = sentenceId[talkerLength..].ToUpperInvariant();
        result.Fields = fields;

        // Checksum validatie
        if (checksumHex != null)
        {
            // Checksum is computed over the characters between the start character ('$' or '!') and '*'
            result.ChecksumValid = ValidateChecksum(sentence, checksumHex);
            if (result.ChecksumValid == false)
            {
                _logger.LogWarning(
                    "NMEA 0183 checksum ongeldig voor sentence '{SentenceType}': verwacht {Expected}, berekend {Computed}",
                    result.SentenceType,
                    checksumHex,
                    ComputeChecksum(sentence));
            }
        }

        result.IsSuccess = true;
        _logger.LogDebug(
            "NMEA 0183 sentence geparset: Talker={Talker}, Type={Type}, Velden={FieldCount}, Checksum={Checksum}",
            result.TalkerPrefix,
            result.SentenceType,
            result.Fields.Count,
            result.ChecksumValid.HasValue ? (result.ChecksumValid.Value ? "OK" : "FOUT") : "n.v.t.");

        return result;
    }

    /// <summary>
    /// Valideert de XOR-checksum van een NMEA 0183 sentence.
    /// De checksum wordt berekend over de tekens tussen '$' en '*'.
    /// </summary>
    private static bool ValidateChecksum(string sentence, string checksumHex)
    {
        if (checksumHex.Length != 2)
        {
            return false;
        }

        if (!byte.TryParse(checksumHex, System.Globalization.NumberStyles.HexNumber, null, out var expected))
        {
            return false;
        }

        var computed = ComputeRawChecksum(sentence);
        return computed == expected;
    }

    /// <summary>
    /// Berekent de XOR-checksum als hexadecimale string (voor logging).
    /// </summary>
    private static string ComputeChecksum(string sentence)
        => ComputeRawChecksum(sentence).ToString("X2");

    /// <summary>
    /// Berekent de XOR-checksum over tekens tussen '$' en '*'.
    /// </summary>
    private static byte ComputeRawChecksum(string sentence)
    {
        byte xor = 0;
        // Compute XOR over characters between start char ('$' or '!') and '*'
        var inBody = false;

        foreach (var c in sentence)
        {
            if (c == '$' || c == '!')
            {
                inBody = true;
                continue;
            }

            if (c == '*')
            {
                break;
            }

            if (inBody)
            {
                xor ^= (byte)c;
            }
        }

        return xor;
    }
}
