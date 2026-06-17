using BootManager.Application.Authentication.DTOs;
using BootManager.Application.Authentication.Services;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;
using System.Linq.Expressions;

namespace BootManager.UnitTests.Authentication;

public class LocalUserManagementServiceTests
{
    private readonly FakePasswordHasher _hasher = new();

    [Fact]
    public async Task CreateCrew_Succeeds_WithValidInputs()
    {
        var repo = FakeLocalUserRepository.Empty();
        var sut = new LocalUserManagementService(repo, _hasher);

        var result = await sut.CreateCrewAsync("CrewMember", "password123");

        Assert.True(result.Success);
        Assert.NotEqual(Guid.Empty, result.CrewId);
    }

    [Fact]
    public async Task CreateCrew_Fails_WithShortPassword()
    {
        var repo = FakeLocalUserRepository.Empty();
        var sut = new LocalUserManagementService(repo, _hasher);

        var result = await sut.CreateCrewAsync("CrewMember", "short");

        Assert.False(result.Success);
        Assert.Contains("8", result.Message ?? "");
    }

    [Fact]
    public async Task CreateCrew_Fails_WithDuplicateName()
    {
        var crew1 = LocalUser.Create("Crew1", LocalUserRole.Crew, "hash", "salt", "algo", Array.Empty<byte>(), 1, DateTime.UtcNow);
        var repo = FakeLocalUserRepository.WithUser(crew1);
        var sut = new LocalUserManagementService(repo, _hasher);

        var result = await sut.CreateCrewAsync("Crew1", "password123");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ResetCrewPassword_Succeeds_AndIncrementsCredentialVersion()
    {
        var crew = LocalUser.Create("Crew1", LocalUserRole.Crew, "hash", "salt", "algo", Array.Empty<byte>(), 1, DateTime.UtcNow);
        var oldVersion = crew.CredentialVersion;
        var repo = FakeLocalUserRepository.WithUser(crew);
        var sut = new LocalUserManagementService(repo, _hasher);

        var result = await sut.ResetCrewPasswordAsync(crew.Id, "newpass123");

        Assert.True(result.Success);
        var updated = await repo.GetByIdAsync(crew.Id);
        Assert.True(updated!.CredentialVersion > oldVersion);
    }

    [Fact]
    public async Task DisableCrew_Succeeds_AndIncrementsCredentialVersion()
    {
        var crew = LocalUser.Create("Crew1", LocalUserRole.Crew, "hash", "salt", "algo", Array.Empty<byte>(), 1, DateTime.UtcNow);
        var oldVersion = crew.CredentialVersion;
        var repo = FakeLocalUserRepository.WithUser(crew);
        var sut = new LocalUserManagementService(repo, _hasher);

        await sut.DisableCrewAsync(crew.Id);

        var updated = await repo.GetByIdAsync(crew.Id);
        Assert.False(updated!.IsActive);
        Assert.True(updated.CredentialVersion > oldVersion);
    }

    [Fact]
    public async Task ReactivateCrew_Succeeds_AndRestoresActiveStatus()
    {
        var crew = LocalUser.Create("Crew1", LocalUserRole.Crew, "hash", "salt", "algo", Array.Empty<byte>(), 1, DateTime.UtcNow);
        crew.SetActive(false, DateTime.UtcNow);
        var repo = FakeLocalUserRepository.WithUser(crew);
        var sut = new LocalUserManagementService(repo, _hasher);

        var result = await sut.ReactivateCrewAsync(crew.Id);

        Assert.True(result);
        var updated = await repo.GetByIdAsync(crew.Id);
        Assert.True(updated!.IsActive);
        // Reactiveren wijzigt PasswordChangeRequired niet
        Assert.False(updated.PasswordChangeRequired);
    }

    [Fact]
    public async Task GetAllCrew_ReturnsAllCrewIncludingInactive()
    {
        var crew1 = LocalUser.Create("Crew1", LocalUserRole.Crew, "hash", "salt", "algo", Array.Empty<byte>(), 1, DateTime.UtcNow);
        var crew2 = LocalUser.Create("Crew2", LocalUserRole.Crew, "hash", "salt", "algo", Array.Empty<byte>(), 1, DateTime.UtcNow);
        crew2.SetActive(false, DateTime.UtcNow);
        var owner = LocalUser.Create("Owner1", LocalUserRole.Owner, "hash", "salt", "algo", Array.Empty<byte>(), 1, DateTime.UtcNow);

        var repo = FakeLocalUserRepository.WithUsers(crew1, crew2, owner);
        var sut = new LocalUserManagementService(repo, _hasher);

        var result = await sut.GetAllCrewAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Id == crew1.Id && c.IsActive);
        Assert.Contains(result, c => c.Id == crew2.Id && !c.IsActive);
    }

    private sealed class FakeLocalUserRepository : IRepository<LocalUser>
    {
        private List<LocalUser> _users = [];

        public static FakeLocalUserRepository Empty() => new();
        public static FakeLocalUserRepository WithUser(LocalUser user) => new() { _users = [user] };
        public static FakeLocalUserRepository WithUsers(params LocalUser[] users) => new() { _users = users.ToList() };

        public Task<LocalUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

        public Task<LocalUser?> SingleOrDefaultAsync(Expression<Func<LocalUser, bool>>? predicate = null, CancellationToken ct = default)
        {
            if (predicate is null)
                return Task.FromResult(_users.FirstOrDefault());
            var compiled = predicate.Compile();
            return Task.FromResult(_users.FirstOrDefault(compiled));
        }

        public Task<IReadOnlyList<LocalUser>> ListAsync(Expression<Func<LocalUser, bool>>? predicate = null, CancellationToken ct = default)
        {
            if (predicate is null)
                return Task.FromResult((IReadOnlyList<LocalUser>)_users.AsReadOnly());
            var compiled = predicate.Compile();
            return Task.FromResult((IReadOnlyList<LocalUser>)_users.Where(compiled).ToList().AsReadOnly());
        }

        public Task<bool> AnyAsync(Expression<Func<LocalUser, bool>>? predicate = null, CancellationToken ct = default)
        {
            if (predicate is null)
                return Task.FromResult(_users.Any());
            var compiled = predicate.Compile();
            return Task.FromResult(_users.Any(compiled));
        }

        public Task<int> CountAsync(Expression<Func<LocalUser, bool>>? predicate = null, CancellationToken ct = default)
        {
            if (predicate is null)
                return Task.FromResult(_users.Count);
            var compiled = predicate.Compile();
            return Task.FromResult(_users.Count(compiled));
        }

        public Task AddAsync(LocalUser entity, CancellationToken ct = default)
        {
            _users.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(LocalUser entity, CancellationToken ct = default)
        {
            var index = _users.FindIndex(u => u.Id == entity.Id);
            if (index >= 0)
                _users[index] = entity;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(LocalUser entity, CancellationToken ct = default)
        {
            _users.RemoveAll(u => u.Id == entity.Id);
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public HashResult Hash(string password) => new($"hash::{password}", "salt", "fake");
        public bool Verify(string password, HashResult stored) => stored.Hash == $"hash::{password}";
    }
}
