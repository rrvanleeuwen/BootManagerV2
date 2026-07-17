using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Inventory.DTOs;
using BootManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BootManager.Infrastructure.Inventory;

/// <summary>
/// EF Core-implementatie van de gepagineerde productoverzicht-read. Filtert, sorteert,
/// telt en pagineert in de database en haalt de actieve voorraad van de zichtbare pagina
/// in één gebatchte query op. Voor een gewone overzichtspagina zijn dit drie
/// databasecommando's: tellen, de pagina-projectie en de gebatchte actieve voorraad.
/// </summary>
public sealed class ProductOverviewReadQuery : IProductOverviewReadQuery
{
    private readonly IDbContextFactory<BootManagerDbContext> _contextFactory;

    public ProductOverviewReadQuery(IDbContextFactory<BootManagerDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public async Task<ProductOverviewPageDto> GetPageAsync(
        string? searchTerm, bool showArchived, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var page = pageNumber < 1 ? 1 : pageNumber;
        var size = pageSize < 1 ? 1 : pageSize;

        await using var db = await _contextFactory.CreateDbContextAsync(ct);

        // Basisquery: filter op de bestaande actieve/gearchiveerde betekenis (ArchivedAt).
        IQueryable<Core.Entities.Product> query = db.Products.AsNoTracking();
        query = showArchived
            ? query.Where(p => p.ArchivedAt != null)
            : query.Where(p => p.ArchivedAt == null);

        // Hoofdletterongevoelige deelmatch in naam of omschrijving, in de database.
        var normalized = searchTerm?.Trim().ToLower();
        if (!string.IsNullOrEmpty(normalized))
        {
            query = query.Where(p =>
                p.Name.ToLower().Contains(normalized) ||
                (p.Description != null && p.Description.ToLower().Contains(normalized)));
        }

        // Commando 1: tel matches in de database.
        var totalCount = await query.CountAsync(ct);

        // Commando 2: projecteer alleen de zichtbare pagina rechtstreeks naar de benodigde
        // velden (product, eenheid, optionele code en actieve categorie) zonder entiteiten
        // of alle relaties in geheugen op te bouwen. Stabiel gesorteerd op naam, dan id.
        var pageRows = await query
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(p => new ProductRow
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                DefaultUnitId = p.DefaultUnitId,
                DefaultUnitName = p.DefaultUnit.Name,
                ArchivedAt = p.ArchivedAt,
                CodeId = p.Code != null ? p.Code.Id : (Guid?)null,
                CodeValue = p.Code != null ? p.Code.Value : null,
                CodeFormat = p.Code != null ? p.Code.Format : null,
                CategoryId = p.CategoryMappings
                    .Where(m => m.IsActive)
                    .Select(m => (Guid?)m.ProductCategoryId)
                    .FirstOrDefault(),
                CategoryName = p.CategoryMappings
                    .Where(m => m.IsActive)
                    .Select(m => m.ProductCategory.Name)
                    .FirstOrDefault(),
                CategoryIconKey = p.CategoryMappings
                    .Where(m => m.IsActive)
                    .Select(m => m.ProductCategory.IconKey)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var productIds = pageRows.Select(r => r.Id).ToList();

        // Commando 3: haal de actieve voorraadregels (Quantity > 0), locatie en gebied voor
        // uitsluitend de zichtbare product-id's in één gebatchte query op. Zonder zichtbare
        // producten is er geen voorraadquery nodig.
        var stockRows = productIds.Count == 0
            ? new List<StockRow>()
            : await db.Stocks.AsNoTracking()
                .Where(s => productIds.Contains(s.ProductId) && s.Quantity > 0)
                .OrderBy(s => s.StorageLocation.StorageArea.Name)
                .ThenBy(s => s.StorageLocation.Name)
                .Select(s => new StockRow
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    StorageLocationId = s.StorageLocationId,
                    Quantity = s.Quantity,
                    StorageLocationName = s.StorageLocation.Name,
                    StorageAreaName = s.StorageLocation.StorageArea.Name
                })
                .ToListAsync(ct);

        var stocksByProduct = stockRows
            .GroupBy(s => s.ProductId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var items = new List<ProductOverviewItemDto>(pageRows.Count);
        foreach (var row in pageRows)
        {
            var activeLocations = (stocksByProduct.TryGetValue(row.Id, out var rows) ? rows : new List<StockRow>())
                .Select(s => new StockDto
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    StorageLocationId = s.StorageLocationId,
                    ProductName = row.Name,
                    StorageAreaName = s.StorageAreaName,
                    StorageLocationName = s.StorageLocationName,
                    Quantity = s.Quantity,
                    DefaultUnitName = row.DefaultUnitName
                })
                .ToList();

            var product = new ProductDto
            {
                Id = row.Id,
                Name = row.Name,
                Description = row.Description,
                DefaultUnitId = row.DefaultUnitId,
                DefaultUnitName = row.DefaultUnitName,
                ActiveCategoryId = row.CategoryId,
                ActiveCategoryName = row.CategoryName,
                ActiveCategoryIconKey = row.CategoryIconKey,
                Code = row.CodeId.HasValue
                    ? new ProductCodeDto { Id = row.CodeId.Value, Value = row.CodeValue!, Format = row.CodeFormat! }
                    : null,
                IsArchived = row.ArchivedAt.HasValue
            };

            items.Add(new ProductOverviewItemDto
            {
                Product = product,
                ActiveLocations = activeLocations,
                TotalQuantity = activeLocations.Sum(l => l.Quantity)
            });
        }

        return new ProductOverviewPageDto
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    /// <summary>Vlakke projectie van één zichtbaar product uit de pagina-query.</summary>
    private sealed class ProductRow
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public string? Description { get; init; }
        public Guid DefaultUnitId { get; init; }
        public string DefaultUnitName { get; init; } = default!;
        public DateTime? ArchivedAt { get; init; }
        public Guid? CodeId { get; init; }
        public string? CodeValue { get; init; }
        public string? CodeFormat { get; init; }
        public Guid? CategoryId { get; init; }
        public string? CategoryName { get; init; }
        public string? CategoryIconKey { get; init; }
    }

    /// <summary>Vlakke projectie van één actieve voorraadregel uit de gebatchte voorraadquery.</summary>
    private sealed class StockRow
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public Guid StorageLocationId { get; init; }
        public decimal Quantity { get; init; }
        public string StorageLocationName { get; init; } = default!;
        public string StorageAreaName { get; init; } = default!;
    }
}
