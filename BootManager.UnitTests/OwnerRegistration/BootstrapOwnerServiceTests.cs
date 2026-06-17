using BootManager.Application.OwnerRegistration.Services;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Text.Json;

namespace BootManager.UnitTests.OwnerRegistration;

public class BootstrapOwnerServiceTests
{
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeEncryptionService _encryption = new();
    private readonly FakeSystemClock _clock = new();
    private readonly FakeLogger<BootstrapOwnerService> _logger = new();

    [Fact]
    public async Task EnsureBootstrapOwnerAsync_CreatesOwner_WhenDatabaseEmpty()
    {
        var repo = FakeLocalUserRepository.Empty();
        var sut = new BootstrapOwnerService(repo, _hasher, _encryption, _clock, _logger);

        var result = await sut.EnsureBootstrapOwnerAsync("TestPassword123!", isProduction: false);

        Assert.True(result);
        Assert.Single(repo.Users);

        var owner = repo.Users.First();
        Assert.Equal(LocalUserRole.Owner, owner.Role);
        Assert.True(owner.PasswordChangeRequired);
        Assert.False(owner.OnboardingCompleted);
    }

    [Fact]
    public async Task EnsureBootstrapOwnerAsync_SkipsCreation_WhenUserExists()
    {
        var owner = CreateLocalUser();
        var repo = FakeLocalUserRepository.WithUser(owner);
        var sut = new BootstrapOwnerService(repo, _hasher, _encryption, _clock, _logger);

        var result = await sut.EnsureBootstrapOwnerAsync("TestPassword123!", isProduction: false);

        Assert.False(result);
        Assert.Single(repo.Users);
    }

    [Fact]
    public async Task EnsureBootstrapOwnerAsync_ThrowsInProduction_WhenPasswordEmpty()
    {
        var repo = FakeLocalUserRepository.Empty();
        var sut = new BootstrapOwnerService(repo, _hasher, _encryption, _clock, _logger);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.EnsureBootstrapOwnerAsync(null, isProduction: true)
        );

        Assert.Contains("Bootstrap:DefaultPassword is required", ex.Message);
    }

    [Fact]
    public async Task EnsureBootstrapOwnerAsync_UsesFallback_InDevelopmentWhenPasswordEmpty()
    {
        var repo = FakeLocalUserRepository.Empty();
        var sut = new BootstrapOwnerService(repo, _hasher, _encryption, _clock, _logger);

        var result = await sut.EnsureBootstrapOwnerAsync(null, isProduction: false);

        Assert.True(result);
        Assert.Single(repo.Users);
    }

    [Fact]
    public async Task EnsureBootstrapOwnerAsync_SetsPasswordChangeRequired_ToTrue()
    {
        var repo = FakeLocalUserRepository.Empty();
        var sut = new BootstrapOwnerService(repo, _hasher, _encryption, _clock, _logger);

        await sut.EnsureBootstrapOwnerAsync("TestPassword123!", isProduction: false);

        var owner = repo.Users.First();
        Assert.True(owner.PasswordChangeRequired);
    }

    [Fact]
    public async Task EnsureBootstrapOwnerAsync_SetsOnboardingCompleted_ToFalse()
    {
        var repo = FakeLocalUserRepository.Empty();
        var sut = new BootstrapOwnerService(repo, _hasher, _encryption, _clock, _logger);

        await sut.EnsureBootstrapOwnerAsync("TestPassword123!", isProduction: false);

        var owner = repo.Users.First();
        Assert.False(owner.OnboardingCompleted);
    }

    private static LocalUser CreateLocalUser(string password = "TestPassword123!")
    {
        var hasher = new FakePasswordHasher();
        var encryption = new FakeEncryptionService();
        var hash = hasher.Hash(password);

        var payloadObj = new { Name = "Owner", Email = "test@example.com" };
        var json = JsonSerializer.Serialize(payloadObj);
        var encrypted = encryption.Encrypt(json);

        return LocalUser.Create(
            displayName: "Owner",
            role: LocalUserRole.Owner,
            passwordHash: hash.Hash,
            passwordSalt: hash.Salt,
            hashAlgorithm: hash.Algorithm,
            encryptedProfilePayload: encrypted,
            encryptionVersion: 1,
            createdUtc: DateTime.UtcNow,
            passwordChangeRequired: false,
            onboardingCompleted: false);
    }

    private sealed class FakeLocalUserRepository : IRepository<LocalUser>
    {
        private readonly List<LocalUser> _users = [];

        public IReadOnlyList<LocalUser> Users => _users.AsReadOnly();

        public static FakeLocalUserRepository Empty() => new();

        public static FakeLocalUserRepository WithUser(LocalUser user)
        {
            var repo = new FakeLocalUserRepository();
            repo._users.Add(user);
            return repo;
        }

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

    private sealed class FakeEncryptionService : IEncryptionService
    {
        public byte[] Encrypt(string plaintext)
            => System.Text.Encoding.UTF8.GetBytes(plaintext);

        public string Decrypt(byte[] ciphertext)
            => System.Text.Encoding.UTF8.GetString(ciphertext);
    }

    private sealed class FakeSystemClock : ISystemClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public HashResult Hash(string password)
            => new($"hash::{password}", "salt", "fake");

        public bool Verify(string password, HashResult stored)
            => stored.Hash == $"hash::{password}";
    }
}
