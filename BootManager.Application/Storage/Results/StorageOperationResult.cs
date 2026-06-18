namespace BootManager.Application.Storage.Results;

/// <summary>
/// Generiek resultaat voor opslagbeheer-operaties.
/// Success=true betekent operatie slaagde; anders bevat ErrorMessage de reden.
/// </summary>
public class StorageOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public static StorageOperationResult Ok() => new() { Success = true };
    public static StorageOperationResult Error(string message) => new() { Success = false, ErrorMessage = message };
}

/// <summary>
/// Resultaat voor operaties die een entiteit retourneren.
/// </summary>
public class StorageOperationResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }

    public static StorageOperationResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static StorageOperationResult<T> Error(string message) => new() { Success = false, ErrorMessage = message };
    public static StorageOperationResult<T> NotFound() => new() { Success = false, ErrorMessage = "Niet gevonden." };
}
