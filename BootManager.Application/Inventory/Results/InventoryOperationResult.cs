namespace BootManager.Application.Inventory.Results;

/// <summary>
/// Generiek resultaat voor inventory-operaties zonder retourwaarde.
/// </summary>
public class InventoryOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public static InventoryOperationResult Ok() => new() { Success = true };
    public static InventoryOperationResult Error(string message) => new() { Success = false, ErrorMessage = message };
}

/// <summary>
/// Resultaat voor operaties die een entiteit retourneren.
/// </summary>
public class InventoryOperationResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }

    public static InventoryOperationResult<T> Ok(T data) => new() { Success = true, Data = data };
    public static InventoryOperationResult<T> Error(string message) => new() { Success = false, ErrorMessage = message };
    public static InventoryOperationResult<T> NotFound() => new() { Success = false, ErrorMessage = "Niet gevonden." };
}
