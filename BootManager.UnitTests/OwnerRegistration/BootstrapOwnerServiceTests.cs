using BootManager.Application.OwnerRegistration.Services;
using BootManager.Core.Entities;
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
        var repo = FakeOwnerRepository.Empty();
        var sut = new BootstrapOwnerService(repo, _hasher, _encryption, _clock, _logger);

        var result = await sut.EnsureBootstrapOwnerAsync("TestPassword123!", isProduction: false);

        Assert.True(result);
        Assert.Single(repo.Owners);

        var owner = repo.Owners.First();
        Assert.True(owner.PasswordChangeRequired);
        Assert.False(owner.OnboardingCompleted);
        Assert.NotNull(owner.PasswordHash);
        Assert.NotNull(owner.PasswordSalt);
    }

    [Fact]
    public async Task EnsureBootstrapOwnerAsync_SkipsCreation_WhenOwnerExists()
    {
        var existingOwner = CreateOwner();
        var repo = FakeOwnerRepository.WithOwner(existingOwner);
        var sut = new BootstrapOwnerService(repo, _hasher, _encryption, _clock, _logger);

        var result = await sut.EnsureBootstrapOwnerAsync("TestPassword123!", isProduction: false);

        Assert.False(result);
        Assert.Single(repo.Owners);
    }

    [Fact]
    public async Task EnsureBootstrapOwnerAsync_ThrowsInProduction_WhenPasswordEmpty()
    {
        var repo = FakeOwnerRepository.Empty();
        var sut = new BootstrapOwnerService(repo, _hasher, _encryption, _clock, _logger);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.EnsureBootstrapOwnerAsync(null, isProduction: true)
        );

        Assert.Contains("Bootstrap:DefaultPassword is required", ex.Message);
    }

    [Fact]
    public async Task EnsureBootstrapOwnerAsync_UsesFallback_InDevelopmentWhenPasswordEmpty()
    {
        var repo = FakeOwnerRepository.Empty();
        var sut = new BootstrapOwnerService(repo, _hasher, _encryption, _clock, _logger);

        var result = await sut.EnsureBootstrapOwnerAsync(null, isProduction: false);

        Assert.True(result);
        Assert.Single(repo.Owners);
    }

    [Fact]
    public async Task EnsureBootstrapOwnerAsync_SetsPasswordChangeRequired_ToTrue()
    {
        var repo = FakeOwnerRepository.Empty();
        var sut = new BootstrapOwnerService(repo, _hasher, _encryption, _clock, _logger);

        await sut.EnsureBootstrapOwnerAsync("TestPassword123!", isProduction: false);

        var owner = repo.Owners.First();
        Assert.True(owner.PasswordChangeRequired);
    }

    [Fact]
    public async Task EnsureBootstrapOwnerAsync_SetsOnboardingCompleted_ToFalse()
    {
        var repo = FakeOwnerRepository.Empty();
        var sut = new BootstrapOwnerService(repo, _hasher, _encryption, _clock, _logger);

        await sut.EnsureBootstrapOwnerAsync("TestPassword123!", isProduction: false);

        var owner = repo.Owners.First();
        Assert.False(owner.OnboardingCompleted);
    }

    private static OwnerProfile CreateOwner(string password = "TestPassword123!")
    {
        var hasher = new FakePasswordHasher();
        var encryption = new FakeEncryptionService();
        var hash = hasher.Hash(password);

        var payloadObj = new { Name = "Test Owner", Email = "test@example.com" };
        var json = JsonSerializer.Serialize(payloadObj);
        var encrypted = encryption.Encrypt(json);

        return OwnerProfile.Create(
            passwordHash: hash.Hash,
            passwordSalt: hash.Salt,
            hashAlgorithm: hash.Algorithm,
            encryptedProfilePayload: encrypted,
            encryptionVersion: 1,
            createdUtc: DateTime.UtcNow,
            passwordChangeRequired: false,
            onboardingCompleted: false
        );
    }

    private class FakeOwnerRepository : IRepository<OwnerProfile>
    {
        private List<OwnerProfile> _owners = [];

        public IReadOnlyList<OwnerProfile> Owners => _owners.AsReadOnly();

        public static FakeOwnerRepository Empty() => new();

        public static FakeOwnerRepository WithOwner(OwnerProfile owner)
        {
            var repo = new FakeOwnerRepository();
            repo._owners.Add(owner);
            return repo;
        }

        public Task<OwnerProfile?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_owners.FirstOrDefault(o => o.Id == id));

        public Task<OwnerProfile?> SingleOrDefaultAsync(Expression<Func<OwnerProfile, bool>>? predicate = null, CancellationToken ct = default)
        {
            if (predicate is null)
                return Task.FromResult(_owners.FirstOrDefault());

            var compiled = predicate.Compile();
            return Task.FromResult(_owners.FirstOrDefault(compiled));
        }

        public Task<IReadOnlyList<OwnerProfile>> ListAsync(Expression<Func<OwnerProfile, bool>>? predicate = null, CancellationToken ct = default)
        {
            if (predicate is null)
                return Task.FromResult((IReadOnlyList<OwnerProfile>)_owners.AsReadOnly());

            var compiled = predicate.Compile();
            return Task.FromResult((IReadOnlyList<OwnerProfile>)_owners.Where(compiled).ToList().AsReadOnly());
        }

        public Task<bool> AnyAsync(Expression<Func<OwnerProfile, bool>>? predicate = null, CancellationToken ct = default)
        {
            if (predicate is null)
                return Task.FromResult(_owners.Any());

            var compiled = predicate.Compile();
            return Task.FromResult(_owners.Any(compiled));
        }

        public Task<int> CountAsync(Expression<Func<OwnerProfile, bool>>? predicate = null, CancellationToken ct = default)
        {
            if (predicate is null)
                return Task.FromResult(_owners.Count);

            var compiled = predicate.Compile();
            return Task.FromResult(_owners.Count(compiled));
        }

        public Task AddAsync(OwnerProfile entity, CancellationToken ct = default)
        {
            _owners.Add(entity);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(OwnerProfile entity, CancellationToken ct = default)
        {
            var existing = _owners.FirstOrDefault(o => o.Id == entity.Id);
            if (existing is not null)
            {
                _owners.Remove(existing);
                _owners.Add(entity);
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(OwnerProfile entity, CancellationToken ct = default)
        {
            _owners.Remove(entity);
            return Task.CompletedTask;
        }
    }

    private class FakeEncryptionService : IEncryptionService
    {
        public byte[] Encrypt(string plaintext)
            => System.Text.Encoding.UTF8.GetBytes(plaintext);

        public string Decrypt(byte[] ciphertext)
            => System.Text.Encoding.UTF8.GetString(ciphertext);
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

    private class FakePasswordHasher : IPasswordHasher
    {
        public HashResult Hash(string password)
            => new($"hash::{password}", "salt", "fake");

        public bool Verify(string password, HashResult stored)
            => stored.Hash == $"hash::{password}";
    }
}
