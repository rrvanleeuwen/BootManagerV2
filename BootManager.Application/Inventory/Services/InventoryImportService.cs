using System.Globalization;
using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Application.Inventory.Results;
using BootManager.Application.Storage.Services;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;

namespace BootManager.Application.Inventory.Services;

/// <summary>
/// Service voor de Owner-only CSV-startimport van de vakantievoorraad.
/// Parseert het vaste kolommodel (Aantal, Eenheid, Product, Locatie), wist bestaande
/// voorraadbeheerdata behalve eenheden en categorieën, en bouwt vervolgens gebieden,
/// locaties, QR-tokens, producten en voorraadregels op uit de bevestigde locatie-mappings.
/// Bestaande opslag-, product-, eenheid- en tokenservices worden hergebruikt.
/// </summary>
public class InventoryImportService : IInventoryImportService
{
    private static readonly string[] ExpectedHeader = { "aantal", "eenheid", "product", "locatie" };

    private readonly IStorageService _storage;
    private readonly IProductService _products;
    private readonly IStockService _stock;
    private readonly IUnitService _units;

    private readonly IRepository<StockMutation> _mutationRepo;
    private readonly IRepository<Stock> _stockRepo;
    private readonly IRepository<StockExpectedLocation> _expectedLocationRepo;
    private readonly IRepository<ProductCode> _codeRepo;
    private readonly IRepository<ProductCategoryMapping> _categoryMappingRepo;
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<StorageLocation> _locationRepo;
    private readonly IRepository<StorageArea> _areaRepo;

    public InventoryImportService(
        IStorageService storage,
        IProductService products,
        IStockService stock,
        IUnitService units,
        IRepository<StockMutation> mutationRepo,
        IRepository<Stock> stockRepo,
        IRepository<StockExpectedLocation> expectedLocationRepo,
        IRepository<ProductCode> codeRepo,
        IRepository<ProductCategoryMapping> categoryMappingRepo,
        IRepository<Product> productRepo,
        IRepository<StorageLocation> locationRepo,
        IRepository<StorageArea> areaRepo)
    {
        _storage = storage;
        _products = products;
        _stock = stock;
        _units = units;
        _mutationRepo = mutationRepo;
        _stockRepo = stockRepo;
        _expectedLocationRepo = expectedLocationRepo;
        _codeRepo = codeRepo;
        _categoryMappingRepo = categoryMappingRepo;
        _productRepo = productRepo;
        _locationRepo = locationRepo;
        _areaRepo = areaRepo;
    }

    /// <inheritdoc />
    public InventoryImportParseResult ParseCsv(string csvContent)
    {
        if (string.IsNullOrWhiteSpace(csvContent))
            return InventoryImportParseResult.Failed("Het CSV-bestand is leeg.");

        var lines = csvContent
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Split('\n');

        var result = new InventoryImportParseResult();
        var distinct = new List<string>();
        var seenLocations = new HashSet<string>(StringComparer.Ordinal);

        var headerParsed = false;

        for (var i = 0; i < lines.Length; i++)
        {
            var rawLine = lines[i];
            if (string.IsNullOrWhiteSpace(rawLine))
                continue;

            var lineNumber = i + 1;
            var parts = rawLine.Split(';');

            if (!headerParsed)
            {
                if (!IsValidHeader(parts))
                {
                    return InventoryImportParseResult.Failed(
                        "Onverwachte kolomkoppen. Verwacht: Aantal;Eenheid;Product;Locatie.");
                }
                headerParsed = true;
                continue;
            }

            if (parts.Length != 4)
            {
                result.Errors.Add($"Regel {lineNumber}: verwacht 4 kolommen (Aantal;Eenheid;Product;Locatie).");
                continue;
            }

            var rawQuantity = parts[0].Trim();
            var unit = parts[1].Trim();
            var product = parts[2].Trim();
            var location = parts[3].Trim();

            if (!TryParseQuantity(rawQuantity, out var quantity) || quantity <= 0)
            {
                result.Errors.Add($"Regel {lineNumber}: ongeldige hoeveelheid '{parts[0].Trim()}'.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(unit))
            {
                result.Errors.Add($"Regel {lineNumber}: eenheid mag niet leeg zijn.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(product))
            {
                result.Errors.Add($"Regel {lineNumber}: product mag niet leeg zijn.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(location))
            {
                result.Errors.Add($"Regel {lineNumber}: locatie mag niet leeg zijn.");
                continue;
            }

            result.Rows.Add(new InventoryImportRowDto
            {
                Quantity = quantity,
                Unit = unit,
                ProductName = product,
                SourceLocation = location,
                LineNumber = lineNumber
            });

            if (seenLocations.Add(location))
                distinct.Add(location);
        }

        if (!headerParsed)
            return InventoryImportParseResult.Failed("Geen kolomkoppen gevonden in het CSV-bestand.");

        result.DistinctSourceLocations = distinct;
        result.Success = result.Errors.Count == 0 && result.Rows.Count > 0;

        if (result.Rows.Count == 0 && result.Errors.Count == 0)
            result.Errors.Add("Geen dataregels gevonden in het CSV-bestand.");

        return result;
    }

    /// <inheritdoc />
    public async Task<InventoryImportExecutionResult> ExecuteImportAsync(
        IReadOnlyList<InventoryImportRowDto> rows,
        IReadOnlyList<InventoryLocationMappingDto> mappings,
        CancellationToken ct = default)
    {
        if (rows == null || rows.Count == 0)
            return InventoryImportExecutionResult.Error("Er zijn geen regels om te importeren.");

        // Valideer volledige mapping VÓÓR enige destructieve wijziging (validate-first-then-execute).
        var mappingBySource = new Dictionary<string, InventoryLocationMappingDto>(StringComparer.Ordinal);
        foreach (var mapping in mappings ?? Array.Empty<InventoryLocationMappingDto>())
        {
            if (mapping == null || string.IsNullOrWhiteSpace(mapping.SourceLocation))
                continue;
            mappingBySource[mapping.SourceLocation] = mapping;
        }

        foreach (var source in rows.Select(r => r.SourceLocation).Distinct(StringComparer.Ordinal))
        {
            if (!mappingBySource.TryGetValue(source, out var mapping)
                || string.IsNullOrWhiteSpace(mapping.AreaName)
                || string.IsNullOrWhiteSpace(mapping.LocationName))
            {
                return InventoryImportExecutionResult.Error(
                    $"Locatie '{source}' is nog niet volledig gemapt naar gebied en locatie.");
            }
        }

        var result = new InventoryImportExecutionResult { Success = true };

        // Destructieve reset: verwijder in afhankelijkheidsvolgorde. Eenheden en categorieën blijven.
        await DeleteAllAsync(_mutationRepo, ct);
        await DeleteAllAsync(_stockRepo, ct);
        await DeleteAllAsync(_expectedLocationRepo, ct);
        await DeleteAllAsync(_codeRepo, ct);
        await DeleteAllAsync(_categoryMappingRepo, ct);
        await DeleteAllAsync(_productRepo, ct);
        await DeleteAllAsync(_locationRepo, ct);
        await DeleteAllAsync(_areaRepo, ct);

        // Bouw gebieden, locaties en QR-tokens op uit de bevestigde mappings.
        var areaIdByName = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var locationIdByKey = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
        var locationIdBySource = new Dictionary<string, Guid>(StringComparer.Ordinal);

        foreach (var source in rows.Select(r => r.SourceLocation).Distinct(StringComparer.Ordinal))
        {
            var mapping = mappingBySource[source];
            var areaName = mapping.AreaName.Trim();
            var locationName = mapping.LocationName.Trim();

            if (!areaIdByName.TryGetValue(areaName, out var areaId))
            {
                var areaResult = await _storage.CreateAreaAsync(areaName, ct);
                if (!areaResult.Success || areaResult.Data == null)
                    return InventoryImportExecutionResult.Error(
                        $"Aanmaken van gebied '{areaName}' mislukte: {areaResult.ErrorMessage}");

                areaId = areaResult.Data.Id;
                areaIdByName[areaName] = areaId;
                result.AreasCreated++;
            }

            var locationKey = $"{areaId}|{locationName.ToLowerInvariant()}";
            if (!locationIdByKey.TryGetValue(locationKey, out var locationId))
            {
                var locationResult = await _storage.CreateLocationAsync(areaId, locationName, null, ct);
                if (!locationResult.Success || locationResult.Data == null)
                    return InventoryImportExecutionResult.Error(
                        $"Aanmaken van locatie '{locationName}' mislukte: {locationResult.ErrorMessage}");

                locationId = locationResult.Data.Id;
                locationIdByKey[locationKey] = locationId;
                result.LocationsCreated++;

                var tokenResult = await _storage.GenerateOrGetQrTokenAsync(locationId, ct);
                if (!tokenResult.Success)
                    return InventoryImportExecutionResult.Error(
                        $"QR-token genereren voor locatie '{locationName}' mislukte: {tokenResult.ErrorMessage}");

                result.TokensGenerated++;
                result.ImportedLocationIds.Add(locationId);
            }

            locationIdBySource[source] = locationId;
        }

        // Laad bestaande eenheden zodat deze hergebruikt en niet gedupliceerd worden.
        var existingUnits = await _units.GetAllAsync(ct);
        var unitIdByName = existingUnits
            .GroupBy(u => u.Name.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First().Id);

        var productIdByKey = new Dictionary<string, Guid>(StringComparer.Ordinal);
        var stockKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var row in rows)
        {
            var normalizedUnit = row.Unit.Trim().ToLowerInvariant();
            if (!unitIdByName.TryGetValue(normalizedUnit, out var unitId))
            {
                var unitResult = await _units.CreateAsync(row.Unit.Trim(), ct);
                if (!unitResult.Success || unitResult.Data == null)
                    return InventoryImportExecutionResult.Error(
                        $"Aanmaken van eenheid '{row.Unit.Trim()}' mislukte: {unitResult.ErrorMessage}");

                unitId = unitResult.Data.Id;
                unitIdByName[normalizedUnit] = unitId;
                result.UnitsCreated++;
            }

            var productKey = $"{row.ProductName.Trim().ToLowerInvariant()}|{unitId}";
            if (!productIdByKey.TryGetValue(productKey, out var productId))
            {
                var productResult = await _products.CreateAsync(row.ProductName.Trim(), null, unitId, null, ct);
                if (!productResult.Success || productResult.Data == null)
                    return InventoryImportExecutionResult.Error(
                        $"Aanmaken van product '{row.ProductName.Trim()}' mislukte: {productResult.ErrorMessage}");

                productId = productResult.Data.Id;
                productIdByKey[productKey] = productId;
                result.ProductsCreated++;
            }

            var locationId = locationIdBySource[row.SourceLocation];
            var stockResult = await _stock.AddOrIncrementStockAsync(productId, locationId, row.Quantity, ct);
            if (!stockResult.Success)
                return InventoryImportExecutionResult.Error(
                    $"Voorraad toevoegen voor product '{row.ProductName.Trim()}' mislukte: {stockResult.ErrorMessage}");

            if (stockKeys.Add($"{productId}|{locationId}"))
                result.StockRowsCreated++;
        }

        return result;
    }

    private static async Task DeleteAllAsync<T>(IRepository<T> repo, CancellationToken ct) where T : class
    {
        var all = await repo.ListAsync(ct: ct);
        foreach (var entity in all)
            await repo.DeleteAsync(entity, ct);
    }

    private static bool IsValidHeader(string[] parts)
    {
        if (parts.Length != ExpectedHeader.Length)
            return false;

        for (var i = 0; i < ExpectedHeader.Length; i++)
        {
            if (!string.Equals(parts[i].Trim(), ExpectedHeader[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Parseert een hoeveelheid met zowel decimale komma als punt als scheidingsteken.
    /// </summary>
    private static bool TryParseQuantity(string raw, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        var normalized = raw.Trim().Replace(" ", string.Empty).Replace(',', '.');
        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }
}
