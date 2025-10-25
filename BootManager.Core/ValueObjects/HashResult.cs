namespace BootManager.Core.ValueObjects;

public sealed record HashResult(string Hash, string Salt, string Algorithm);