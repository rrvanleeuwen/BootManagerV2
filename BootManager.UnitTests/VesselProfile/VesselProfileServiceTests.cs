using BootManager.Application.VesselProfile.DTOs;
using BootManager.Application.VesselProfile.Services;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace BootManager.UnitTests.VesselProfile;

/// <summary>
/// Unit tests voor VesselProfileService.
/// </summary>
public class VesselProfileServiceTests
{
    private readonly FakeSystemClock _clock = new();
    private readonly FakeLogger<VesselProfileService> _logger = new();

    [Fact]
    public async Task GetOrCreateVesselProfileAsync_CreatesEmptyProfile_WhenNoneExists()
    {
        var repo = FakeVesselProfileRepository.Empty();
        var sut = new VesselProfileService(repo, _clock, _logger);

        var result = await sut.GetOrCreateVesselProfileAsync();

        Assert.NotNull(result);
        Assert.Equal("Unnamed Vessel", result.VesselName);
        Assert.Null(result.HomePort);
        Assert.Null(result.CallSign);
        Assert.Null(result.Mmsi);
        Assert.Single(repo.Profiles);
    }

    [Fact]
    public async Task GetOrCreateVesselProfileAsync_ReturnsExistingProfile_WhenOneExists()
    {
        var existingProfile = CreateVesselProfile("Test Vessel", "Harbor", "CALL", "123456789");
        var repo = FakeVesselProfileRepository.WithProfile(existingProfile);
        var sut = new VesselProfileService(repo, _clock, _logger);

        var result = await sut.GetOrCreateVesselProfileAsync();

        Assert.NotNull(result);
        Assert.Equal("Test Vessel", result.VesselName);
        Assert.Equal("Harbor", result.HomePort);
        Assert.Equal("CALL", result.CallSign);
        Assert.Equal("123456789", result.Mmsi);
        Assert.Single(repo.Profiles);
    }

    [Fact]
    public async Task UpdateVesselProfileAsync_UpdatesProfile_WithValidData()
    {
        var existingProfile = CreateVesselProfile("Old Name", null, null, null);
        var repo = FakeVesselProfileRepository.WithProfile(existingProfile);
        var sut = new VesselProfileService(repo, _clock, _logger);

        var request = new UpdateVesselProfileRequestDto
        {
            VesselName = "New Name",
            HomePort = "New Harbor",
            CallSign = "NEWCALL",
            Mmsi = "987654321"
        };

        var result = await sut.UpdateVesselProfileAsync(request);

        Assert.NotNull(result);
        Assert.Equal("New Name", result.VesselName);
        Assert.Equal("New Harbor", result.HomePort);
        Assert.Equal("NEWCALL", result.CallSign);
        Assert.Equal("987654321", result.Mmsi);
        Assert.NotNull(result.UpdatedUtc);
    }

    [Fact]
    public async Task UpdateVesselProfileAsync_ThrowsArgumentException_WhenVesselNameEmpty()
    {
        var existingProfile = CreateVesselProfile("Test", null, null, null);
        var repo = FakeVesselProfileRepository.WithProfile(existingProfile);
        var sut = new VesselProfileService(repo, _clock, _logger);

        var request = new UpdateVesselProfileRequestDto
        {
            VesselName = string.Empty,
            HomePort = null,
            CallSign = null,
            Mmsi = null
        };

        await Assert.ThrowsAsync<ArgumentException>(() => sut.UpdateVesselProfileAsync(request));
    }

    [Fact]
    public async Task UpdateVesselProfileAsync_ThrowsArgumentException_WhenVesselNameNull()
    {
        var existingProfile = CreateVesselProfile("Test", null, null, null);
        var repo = FakeVesselProfileRepository.WithProfile(existingProfile);
        var sut = new VesselProfileService(repo, _clock, _logger);

        var request = new UpdateVesselProfileRequestDto
        {
            VesselName = null!,
            HomePort = null,
            CallSign = null,
            Mmsi = null
        };

        await Assert.ThrowsAsync<ArgumentException>(() => sut.UpdateVesselProfileAsync(request));
    }

    [Fact]
    public async Task UpdateVesselProfileAsync_ThrowsArgumentException_WhenVesselNameTooLong()
    {
        var existingProfile = CreateVesselProfile("Test", null, null, null);
        var repo = FakeVesselProfileRepository.WithProfile(existingProfile);
        var sut = new VesselProfileService(repo, _clock, _logger);

        var longName = new string('A', 129);
        var request = new UpdateVesselProfileRequestDto
        {
            VesselName = longName,
            HomePort = null,
            CallSign = null,
            Mmsi = null
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.UpdateVesselProfileAsync(request));
        Assert.Contains("128", ex.Message);
    }

    [Fact]
    public async Task UpdateVesselProfileAsync_ThrowsArgumentException_WhenHomePortTooLong()
    {
        var existingProfile = CreateVesselProfile("Test", null, null, null);
        var repo = FakeVesselProfileRepository.WithProfile(existingProfile);
        var sut = new VesselProfileService(repo, _clock, _logger);

        var longPort = new string('A', 129);
        var request = new UpdateVesselProfileRequestDto
        {
            VesselName = "Test Vessel",
            HomePort = longPort,
            CallSign = null,
            Mmsi = null
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.UpdateVesselProfileAsync(request));
        Assert.Contains("128", ex.Message);
    }

    [Fact]
    public async Task UpdateVesselProfileAsync_ThrowsArgumentException_WhenCallSignTooLong()
    {
        var existingProfile = CreateVesselProfile("Test", null, null, null);
        var repo = FakeVesselProfileRepository.WithProfile(existingProfile);
        var sut = new VesselProfileService(repo, _clock, _logger);

        var longCallSign = new string('A', 65);
        var request = new UpdateVesselProfileRequestDto
        {
            VesselName = "Test Vessel",
            HomePort = null,
            CallSign = longCallSign,
            Mmsi = null
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.UpdateVesselProfileAsync(request));
        Assert.Contains("64", ex.Message);
    }

    [Fact]
    public async Task UpdateVesselProfileAsync_ThrowsArgumentException_WhenMmsiTooLong()
    {
        var existingProfile = CreateVesselProfile("Test", null, null, null);
        var repo = FakeVesselProfileRepository.WithProfile(existingProfile);
        var sut = new VesselProfileService(repo, _clock, _logger);

        var longMmsi = new string('0', 33);
        var request = new UpdateVesselProfileRequestDto
        {
            VesselName = "Test Vessel",
            HomePort = null,
            CallSign = null,
            Mmsi = longMmsi
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => sut.UpdateVesselProfileAsync(request));
        Assert.Contains("32", ex.Message);
    }

    [Fact]
    public async Task UpdateVesselProfileAsync_AllowsNullOptionalFields()
    {
        var existingProfile = CreateVesselProfile("Old", "OldHarbor", "OLDCALL", "111111111");
        var repo = FakeVesselProfileRepository.WithProfile(existingProfile);
        var sut = new VesselProfileService(repo, _clock, _logger);

        var request = new UpdateVesselProfileRequestDto
        {
            VesselName = "New",
            HomePort = null,
            CallSign = null,
            Mmsi = null
        };

        var result = await sut.UpdateVesselProfileAsync(request);

        Assert.NotNull(result);
        Assert.Equal("New", result.VesselName);
        Assert.Null(result.HomePort);
        Assert.Null(result.CallSign);
        Assert.Null(result.Mmsi);
    }

    [Fact]
    public async Task UpdateVesselProfileAsync_ThrowsInvalidOperationException_WhenNoProfileExists()
    {
        var repo = FakeVesselProfileRepository.Empty();
        var sut = new VesselProfileService(repo, _clock, _logger);

        var request = new UpdateVesselProfileRequestDto
        {
            VesselName = "Test",
            HomePort = null,
            CallSign = null,
            Mmsi = null
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.UpdateVesselProfileAsync(request));
    }

    private static Core.Entities.VesselProfile CreateVesselProfile(string name, string? homePort, string? callSign, string? mmsi)
    {
        return Core.Entities.VesselProfile.Create(name, homePort, callSign, mmsi, DateTime.UtcNow);
    }

    private class FakeVesselProfileRepository : IRepository<Core.Entities.VesselProfile>
    {
        private List<Core.Entities.VesselProfile> _profiles = new();

        public IReadOnlyList<Core.Entities.VesselProfile> Profiles => _profiles.AsReadOnly();

        public static FakeVesselProfileRepository Empty() => new();

        public static FakeVesselProfileRepository WithProfile(Core.Entities.VesselProfile profile)
        {
            var repo = new FakeVesselProfileRepository();
            repo._profiles.Add(profile);
            return repo;
        }

        public Task<Core.Entities.VesselProfile?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var profile = _profiles.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(profile);
        }

        public Task<Core.Entities.VesselProfile?> SingleOrDefaultAsync(Expression<Func<Core.Entities.VesselProfile, bool>>? predicate = null, CancellationToken ct = default)
        {
            var profile = predicate == null
                ? _profiles.FirstOrDefault()
                : _profiles.AsQueryable().FirstOrDefault(predicate);
            return Task.FromResult(profile);
        }

        public Task<IReadOnlyList<Core.Entities.VesselProfile>> ListAsync(Expression<Func<Core.Entities.VesselProfile, bool>>? predicate = null, CancellationToken ct = default)
        {
            var list = (IReadOnlyList<Core.Entities.VesselProfile>)(predicate == null
                ? _profiles
                : _profiles.AsQueryable().Where(predicate).ToList());
            return Task.FromResult(list);
        }

        public Task<bool> AnyAsync(Expression<Func<Core.Entities.VesselProfile, bool>>? predicate = null, CancellationToken ct = default)
        {
            var exists = predicate == null
                ? _profiles.Count > 0
                : _profiles.AsQueryable().Any(predicate);
            return Task.FromResult(exists);
        }

        public Task AddAsync(Core.Entities.VesselProfile entity, CancellationToken ct = default)
        {
            _profiles.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Core.Entities.VesselProfile entity, CancellationToken ct = default)
        {
            var existing = _profiles.FirstOrDefault(p => p.Id == entity.Id);
            if (existing is not null)
            {
                _profiles.Remove(existing);
                _profiles.Add(entity);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Core.Entities.VesselProfile entity, CancellationToken ct = default)
        {
            _profiles.Remove(entity);
            return Task.CompletedTask;
        }
    }

    private class FakeSystemClock : ISystemClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    private class FakeLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}
