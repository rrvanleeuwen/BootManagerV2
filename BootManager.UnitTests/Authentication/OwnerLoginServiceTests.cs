using BootManager.Application.Authentication.DTOs;
using BootManager.Application.Authentication.Services;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;
using System.Linq.Expressions;

namespace BootManager.UnitTests.Authentication;

public class OwnerLoginServiceTests
{
    private readonly FakePasswordHasher _hasher = new();

    [Fact]
    public async Task ValidateAsync_ReturnsSuccess_ForValidPassword()
    {
        var user = CreateLocalUser(displayName: "TestOwner", password: "12345678", role: LocalUserRole.Owner);
        var repo = FakeLocalUserRepository.WithUser(user);
        var sut = new OwnerLoginService(repo, _hasher);

        var result = await sut.ValidateAsync(new LoginRequestDto { UserId = user.Id, Password = "12345678" });

        Assert.True(result.Success);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("TestOwner", result.DisplayName);
        Assert.Equal(LocalUserRole.Owner, result.Role);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFailure_ForWrongPassword()
    {
        var user = CreateLocalUser(displayName: "TestOwner", password: "12345678");
        var repo = FakeLocalUserRepository.WithUser(user);
        var sut = new OwnerLoginService(repo, _hasher);

        var result = await sut.ValidateAsync(new LoginRequestDto { UserId = user.Id, Password = "wrongpass" });

        Assert.False(result.Success);
        Assert.Equal("Ongeldig wachtwoord.", result.Message);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFailure_WhenUserNotFound()
    {
        var repo = FakeLocalUserRepository.Empty();
        var sut = new OwnerLoginService(repo, _hasher);

        var result = await sut.ValidateAsync(new LoginRequestDto { UserId = Guid.NewGuid(), Password = "irrelevant" });

        Assert.False(result.Success);
        Assert.Equal("Gebruiker niet gevonden.", result.Message);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFailure_WhenUserInactive()
    {
        var user = CreateLocalUser(displayName: "Inactive", password: "12345678");
        user.SetActive(false, DateTime.UtcNow);
        var repo = FakeLocalUserRepository.WithUser(user);
        var sut = new OwnerLoginService(repo, _hasher);

        var result = await sut.ValidateAsync(new LoginRequestDto { UserId = user.Id, Password = "12345678" });

        Assert.False(result.Success);
        Assert.Equal("Dit account is uitgeschakeld.", result.Message);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsSuccess_ForValidCrew()
    {
        var user = CreateLocalUser(displayName: "TestCrew", password: "crewpass", role: LocalUserRole.Crew);
        var repo = FakeLocalUserRepository.WithUser(user);
        var sut = new OwnerLoginService(repo, _hasher);

        var result = await sut.ValidateAsync(new LoginRequestDto { UserId = user.Id, Password = "crewpass" });

        Assert.True(result.Success);
        Assert.Equal(LocalUserRole.Crew, result.Role);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFailure_WhenNoUserIdProvided()
    {
        var repo = FakeLocalUserRepository.Empty();
        var sut = new OwnerLoginService(repo, _hasher);

        var result = await sut.ValidateAsync(new LoginRequestDto { Password = "irrelevant" });

        Assert.False(result.Success);
        Assert.Equal("Geen gebruiker geselecteerd.", result.Message);
    }

    private LocalUser CreateLocalUser(string displayName, string password, LocalUserRole role = LocalUserRole.Owner)
    {
        var passwordHash = _hasher.Hash(password);
        return LocalUser.Create(
            displayName: displayName,
            role: role,
            passwordHash: passwordHash.Hash,
            passwordSalt: passwordHash.Salt,
            hashAlgorithm: passwordHash.Algorithm,
            encryptedProfilePayload: Array.Empty<byte>(),
            encryptionVersion: 1,
            createdUtc: DateTime.UtcNow);
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public HashResult Hash(string password)
        {
            return new HashResult($"hash::{password}", "salt", "fake");
        }

        public bool Verify(string password, HashResult stored)
        {
            return stored.Hash == $"hash::{password}";
        }
    }

    private sealed class FakeLocalUserRepository : IRepository<LocalUser>
    {
        private readonly List<LocalUser> _users = new();

        private FakeLocalUserRepository(LocalUser? user)
        {
            if (user != null)
                _users.Add(user);
        }

        public static FakeLocalUserRepository WithUser(LocalUser user) => new(user);
        public static FakeLocalUserRepository Empty() => new(null);

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
                return Task.FromResult((IReadOnlyList<LocalUser>)_users);

            var compiled = predicate.Compile();
            return Task.FromResult((IReadOnlyList<LocalUser>)_users.Where(compiled).ToList());
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
                return Task.FromResult(_users.Count());

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
}
