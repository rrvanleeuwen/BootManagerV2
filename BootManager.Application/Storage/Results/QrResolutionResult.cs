namespace BootManager.Application.Storage.Results;

/// <summary>
/// Resultaat van QR-herkenning: invalide, onbekend of gekoppeld aan een bestaande locatie.
/// </summary>
public class QrResolutionResult
{
    public QrStatus Status { get; set; }
    public Guid? LinkedLocationId { get; set; }
    public string? Token { get; set; }

    public static QrResolutionResult Invalid() => new() { Status = QrStatus.Invalid };
    public static QrResolutionResult Unknown(string token) => new() { Status = QrStatus.Unknown, Token = token };
    public static QrResolutionResult Linked(Guid locationId) => new() { Status = QrStatus.Linked, LinkedLocationId = locationId };
}

public enum QrStatus
{
    Invalid,
    Unknown,
    Linked
}
