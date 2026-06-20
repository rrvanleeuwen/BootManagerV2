using BootManager.Application.Inventory.Contracts;
using BootManager.Application.Storage.Services;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Moq;
using System.Linq.Expressions;

namespace BootManager.UnitTests.Storage;

public class StorageServiceTests
{
    private static IRepository<StorageArea> CreateAreaRepository(List<StorageArea> areas = null!)
    {
        return new FakeStorageAreaRepository(areas ?? new List<StorageArea>());
    }

    private static IRepository<StorageLocation> CreateLocationRepository(List<StorageLocation> locations = null!)
    {
        return new FakeStorageLocationRepository(locations ?? new List<StorageLocation>());
    }

    private static IStockService CreateMockStockService()
    {
        return new Mock<IStockService>().Object;
    }

    // --- StorageArea Tests ---

    [Fact]
    public async Task CreateAreaAsync_Succeeds_WithValidName()
    {
        var areaRepo = CreateAreaRepository();
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.CreateAreaAsync("Kombuis");

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Kombuis", result.Data.Name);
    }

    [Fact]
    public async Task CreateAreaAsync_TrimsName()
    {
        var areaRepo = CreateAreaRepository();
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.CreateAreaAsync("  Kombuis  ");

        Assert.True(result.Success);
        Assert.Equal("Kombuis", result.Data!.Name);
    }

    [Fact]
    public async Task CreateAreaAsync_Fails_WithEmptyName()
    {
        var areaRepo = CreateAreaRepository();
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.CreateAreaAsync("");

        Assert.False(result.Success);
        Assert.Contains("niet leeg", result.ErrorMessage!);
    }

    [Fact]
    public async Task CreateAreaAsync_Fails_WithNameTooLong()
    {
        var areaRepo = CreateAreaRepository();
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var longName = new string('A', 101);
        var result = await service.CreateAreaAsync(longName);

        Assert.False(result.Success);
        Assert.Contains("100", result.ErrorMessage!);
    }

    [Fact]
    public async Task CreateAreaAsync_Fails_WithDuplicateName_CaseInsensitive()
    {
        var area = StorageArea.Create("Kombuis");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area });
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.CreateAreaAsync("kombuis");

        Assert.False(result.Success);
        Assert.Contains("bestaat al", result.ErrorMessage!);
    }

    [Fact]
    public async Task RenameAreaAsync_Succeeds_WithValidNewName()
    {
        var area = StorageArea.Create("Kombuis");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area });
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.RenameAreaAsync(area.Id, "Salon");

        Assert.True(result.Success);
    }

    [Fact]
    public async Task RenameAreaAsync_Fails_WithNonExistentArea()
    {
        var areaRepo = CreateAreaRepository();
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.RenameAreaAsync(Guid.NewGuid(), "Salon");

        Assert.False(result.Success);
        Assert.Contains("niet gevonden", result.ErrorMessage!);
    }

    [Fact]
    public async Task RenameAreaAsync_Fails_WithDuplicateName_CaseInsensitive()
    {
        var area1 = StorageArea.Create("Kombuis");
        var area2 = StorageArea.Create("Salon");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area1, area2 });
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.RenameAreaAsync(area2.Id, "kombuis");

        Assert.False(result.Success);
        Assert.Contains("bestaat al", result.ErrorMessage!);
    }

    [Fact]
    public async Task DeleteAreaAsync_Succeeds_WithEmptyArea()
    {
        var area = StorageArea.Create("Kombuis");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area });
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.DeleteAreaAsync(area.Id);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task DeleteAreaAsync_Fails_WithLocations()
    {
        var area = StorageArea.Create("Kombuis");
        var location = StorageLocation.Create(area.Id, "Kast 1");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area });
        var locRepo = CreateLocationRepository(new List<StorageLocation> { location });
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.DeleteAreaAsync(area.Id);

        Assert.False(result.Success);
        Assert.Contains("bevat locaties", result.ErrorMessage!);
    }

    // --- StorageLocation Tests ---

    [Fact]
    public async Task CreateLocationAsync_Succeeds_WithValidData()
    {
        var area = StorageArea.Create("Kombuis");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area });
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.CreateLocationAsync(area.Id, "Kast 1", "Keukenkast");

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Kast 1", result.Data.Name);
        Assert.Equal("Keukenkast", result.Data.Description);
    }

    [Fact]
    public async Task CreateLocationAsync_AllowsNullDescription()
    {
        var area = StorageArea.Create("Kombuis");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area });
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.CreateLocationAsync(area.Id, "Kast 1", null);

        Assert.True(result.Success);
        Assert.Null(result.Data!.Description);
    }

    [Fact]
    public async Task CreateLocationAsync_TrimsNameAndDescription()
    {
        var area = StorageArea.Create("Kombuis");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area });
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.CreateLocationAsync(area.Id, "  Kast 1  ", "  Beschrijving  ");

        Assert.True(result.Success);
        Assert.Equal("Kast 1", result.Data!.Name);
        Assert.Equal("Beschrijving", result.Data.Description);
    }

    [Fact]
    public async Task CreateLocationAsync_Fails_WithNonExistentArea()
    {
        var areaRepo = CreateAreaRepository();
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.CreateLocationAsync(Guid.NewGuid(), "Kast 1", null);

        Assert.False(result.Success);
        Assert.Contains("niet gevonden", result.ErrorMessage!);
    }

    [Fact]
    public async Task CreateLocationAsync_Fails_WithEmptyName()
    {
        var area = StorageArea.Create("Kombuis");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area });
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.CreateLocationAsync(area.Id, "", null);

        Assert.False(result.Success);
        Assert.Contains("niet leeg", result.ErrorMessage!);
    }

    [Fact]
    public async Task CreateLocationAsync_Fails_WithNameTooLong()
    {
        var area = StorageArea.Create("Kombuis");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area });
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var longName = new string('A', 101);
        var result = await service.CreateLocationAsync(area.Id, longName, null);

        Assert.False(result.Success);
        Assert.Contains("100", result.ErrorMessage!);
    }

    [Fact]
    public async Task CreateLocationAsync_Fails_WithDescriptionTooLong()
    {
        var area = StorageArea.Create("Kombuis");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area });
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var longDesc = new string('A', 501);
        var result = await service.CreateLocationAsync(area.Id, "Kast 1", longDesc);

        Assert.False(result.Success);
        Assert.Contains("500", result.ErrorMessage!);
    }

    [Fact]
    public async Task CreateLocationAsync_Fails_WithDuplicateNameInSameArea_CaseInsensitive()
    {
        var area = StorageArea.Create("Kombuis");
        var location = StorageLocation.Create(area.Id, "Kast 1");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area });
        var locRepo = CreateLocationRepository(new List<StorageLocation> { location });
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.CreateLocationAsync(area.Id, "kast 1", null);

        Assert.False(result.Success);
        Assert.Contains("bestaat al", result.ErrorMessage!);
    }

    [Fact]
    public async Task CreateLocationAsync_AllowsSameNameInDifferentArea()
    {
        var area1 = StorageArea.Create("Kombuis");
        var area2 = StorageArea.Create("Salon");
        var location1 = StorageLocation.Create(area1.Id, "Kast 1");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area1, area2 });
        var locRepo = CreateLocationRepository(new List<StorageLocation> { location1 });
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.CreateLocationAsync(area2.Id, "Kast 1", null);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task UpdateLocationAsync_Succeeds_WithValidData()
    {
        var area = StorageArea.Create("Kombuis");
        var location = StorageLocation.Create(area.Id, "Kast 1", "Oude beschrijving");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area });
        var locRepo = CreateLocationRepository(new List<StorageLocation> { location });
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.UpdateLocationAsync(location.Id, "Kast 2", "Nieuwe beschrijving");

        Assert.True(result.Success);
        Assert.Equal("Kast 2", result.Data!.Name);
        Assert.Equal("Nieuwe beschrijving", result.Data.Description);
    }

    [Fact]
    public async Task UpdateLocationAsync_KeepsIdStable()
    {
        var area = StorageArea.Create("Kombuis");
        var location = StorageLocation.Create(area.Id, "Kast 1", null);
        var originalId = location.Id;
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area });
        var locRepo = CreateLocationRepository(new List<StorageLocation> { location });
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.UpdateLocationAsync(originalId, "Kast 2", "Beschrijving");

        Assert.Equal(originalId, result.Data!.Id);
    }

    [Fact]
    public async Task MoveLocationAsync_Succeeds_ToAnotherArea()
    {
        var area1 = StorageArea.Create("Kombuis");
        var area2 = StorageArea.Create("Salon");
        var location = StorageLocation.Create(area1.Id, "Kast 1", null);
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area1, area2 });
        var locRepo = CreateLocationRepository(new List<StorageLocation> { location });
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.MoveLocationAsync(location.Id, area2.Id);

        Assert.True(result.Success);
        Assert.Equal(area2.Id, result.Data!.StorageAreaId);
    }

    [Fact]
    public async Task MoveLocationAsync_KeepsIdStable()
    {
        var area1 = StorageArea.Create("Kombuis");
        var area2 = StorageArea.Create("Salon");
        var location = StorageLocation.Create(area1.Id, "Kast 1", null);
        var originalId = location.Id;
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area1, area2 });
        var locRepo = CreateLocationRepository(new List<StorageLocation> { location });
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.MoveLocationAsync(originalId, area2.Id);

        Assert.Equal(originalId, result.Data!.Id);
    }

    [Fact]
    public async Task MoveLocationAsync_Fails_WithDuplicateNameInTargetArea()
    {
        var area1 = StorageArea.Create("Kombuis");
        var area2 = StorageArea.Create("Salon");
        var location1 = StorageLocation.Create(area1.Id, "Kast 1");
        var location2 = StorageLocation.Create(area2.Id, "Kast 1");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area1, area2 });
        var locRepo = CreateLocationRepository(new List<StorageLocation> { location1, location2 });
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.MoveLocationAsync(location1.Id, area2.Id);

        Assert.False(result.Success);
        Assert.Contains("bestaat al", result.ErrorMessage!);
    }

    [Fact]
    public async Task DeleteLocationAsync_Succeeds()
    {
        var area = StorageArea.Create("Kombuis");
        var location = StorageLocation.Create(area.Id, "Kast 1");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area });
        var locRepo = CreateLocationRepository(new List<StorageLocation> { location });
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.DeleteLocationAsync(location.Id);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task GetLocationDetailAsync_ReturnsDetail_WithValidId()
    {
        var area = StorageArea.Create("Kombuis");
        var location = StorageLocation.Create(area.Id, "Kast 1", "Beschrijving");
        var areaRepo = CreateAreaRepository(new List<StorageArea> { area });
        var locRepo = CreateLocationRepository(new List<StorageLocation> { location });
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.GetLocationDetailAsync(location.Id);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Kombuis", result.Data.AreaName);
        Assert.Equal("Kast 1", result.Data.LocationName);
        Assert.Equal("Beschrijving", result.Data.Description);
    }

    [Fact]
    public async Task GetLocationDetailAsync_ReturnsNotFound_WithInvalidId()
    {
        var areaRepo = CreateAreaRepository();
        var locRepo = CreateLocationRepository();
        var service = new StorageService(areaRepo, locRepo, CreateMockStockService());

        var result = await service.GetLocationDetailAsync(Guid.NewGuid());

        Assert.False(result.Success);
    }

    // Fake repositories for testing
    private class FakeStorageAreaRepository : IRepository<StorageArea>
    {
        private readonly List<StorageArea> _areas;

        public FakeStorageAreaRepository(List<StorageArea> areas)
        {
            _areas = new List<StorageArea>(areas);
        }

        public Task<StorageArea?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_areas.FirstOrDefault(a => a.Id == id));

        public Task<StorageArea?> SingleOrDefaultAsync(Expression<Func<StorageArea, bool>>? predicate = null, CancellationToken ct = default)
            => Task.FromResult(predicate == null ? _areas.FirstOrDefault() : _areas.AsQueryable().FirstOrDefault(predicate));

        public Task<IReadOnlyList<StorageArea>> ListAsync(Expression<Func<StorageArea, bool>>? predicate = null, CancellationToken ct = default)
        {
            IReadOnlyList<StorageArea> list = predicate == null ? _areas : _areas.AsQueryable().Where(predicate).ToList();
            return Task.FromResult(list);
        }

        public Task<bool> AnyAsync(Expression<Func<StorageArea, bool>>? predicate = null, CancellationToken ct = default)
            => Task.FromResult(predicate == null ? _areas.Count > 0 : _areas.AsQueryable().Any(predicate));

        public Task<int> CountAsync(Expression<Func<StorageArea, bool>>? predicate = null, CancellationToken ct = default)
            => Task.FromResult(predicate == null ? _areas.Count : _areas.AsQueryable().Count(predicate));

        public Task AddAsync(StorageArea entity, CancellationToken ct = default)
        {
            _areas.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(StorageArea entity, CancellationToken ct = default)
        {
            var existing = _areas.FirstOrDefault(a => a.Id == entity.Id);
            if (existing is not null)
            {
                _areas.Remove(existing);
                _areas.Add(entity);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(StorageArea entity, CancellationToken ct = default)
        {
            _areas.Remove(entity);
            return Task.CompletedTask;
        }
    }

    private class FakeStorageLocationRepository : IRepository<StorageLocation>
    {
        private readonly List<StorageLocation> _locations;

        public FakeStorageLocationRepository(List<StorageLocation> locations)
        {
            _locations = new List<StorageLocation>(locations);
        }

        public Task<StorageLocation?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_locations.FirstOrDefault(l => l.Id == id));

        public Task<StorageLocation?> SingleOrDefaultAsync(Expression<Func<StorageLocation, bool>>? predicate = null, CancellationToken ct = default)
            => Task.FromResult(predicate == null ? _locations.FirstOrDefault() : _locations.AsQueryable().FirstOrDefault(predicate));

        public Task<IReadOnlyList<StorageLocation>> ListAsync(Expression<Func<StorageLocation, bool>>? predicate = null, CancellationToken ct = default)
        {
            IReadOnlyList<StorageLocation> list = predicate == null ? _locations : _locations.AsQueryable().Where(predicate).ToList();
            return Task.FromResult(list);
        }

        public Task<bool> AnyAsync(Expression<Func<StorageLocation, bool>>? predicate = null, CancellationToken ct = default)
            => Task.FromResult(predicate == null ? _locations.Count > 0 : _locations.AsQueryable().Any(predicate));

        public Task<int> CountAsync(Expression<Func<StorageLocation, bool>>? predicate = null, CancellationToken ct = default)
            => Task.FromResult(predicate == null ? _locations.Count : _locations.AsQueryable().Count(predicate));

        public Task AddAsync(StorageLocation entity, CancellationToken ct = default)
        {
            _locations.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(StorageLocation entity, CancellationToken ct = default)
        {
            var existing = _locations.FirstOrDefault(l => l.Id == entity.Id);
            if (existing is not null)
            {
                _locations.Remove(existing);
                _locations.Add(entity);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(StorageLocation entity, CancellationToken ct = default)
        {
            _locations.Remove(entity);
            return Task.CompletedTask;
        }
    }
}
