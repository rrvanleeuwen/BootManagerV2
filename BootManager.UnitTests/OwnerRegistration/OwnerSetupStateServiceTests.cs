using BootManager.Application.OwnerRegistration.Services;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Text.Json;

namespace BootManager.UnitTests.OwnerRegistration;

public class OwnerSetupStateServiceTests
{
    [Fact]
    public async Task GetSetupStateAsync_ReturnsHasOwnerFalse_WhenNoDatabaseRecord()
    {
        var repo = FakeOwnerRepository.Empty();
        var sut = new OwnerSetupStateService(repo);

        var result = await sut.GetSetupStateAsync();

        Assert.False(result.HasOwner);
        Assert.False(result.PasswordChangeRequired);
        Assert.False(result.OnboardingCompleted);
        Assert.True(result.SetupRequired);
    }

    [Fact]
    public async Task GetSetupStateAsync_ReturnsSetupRequired_WhenPasswordChangeRequired()
    {
        var owner = CreateOwner(passwordChangeRequired: true, onboardingCompleted: true);
        var repo = FakeOwnerRepository.WithOwner(owner);
        var sut = new OwnerSetupStateService(repo);

        var result = await sut.GetSetupStateAsync();

        Assert.True(result.HasOwner);
        Assert.True(result.PasswordChangeRequired);
        Assert.True(result.OnboardingCompleted);
        Assert.True(result.SetupRequired); // true because PasswordChangeRequired=true
    }

    [Fact]
    public async Task GetSetupStateAsync_ReturnsSetupRequired_WhenOnboardingNotCompleted()
    {
        var owner = CreateOwner(passwordChangeRequired: false, onboardingCompleted: false);
        var repo = FakeOwnerRepository.WithOwner(owner);
        var sut = new OwnerSetupStateService(repo);

        var result = await sut.GetSetupStateAsync();

        Assert.True(result.HasOwner);
        Assert.False(result.PasswordChangeRequired);
        Assert.False(result.OnboardingCompleted);
        Assert.True(result.SetupRequired); // true because OnboardingCompleted=false
    }

    [Fact]
    public async Task GetSetupStateAsync_ReturnsSetupNotRequired_WhenBothFlagsTrue()
    {
        var owner = CreateOwner(passwordChangeRequired: false, onboardingCompleted: true);
        var repo = FakeOwnerRepository.WithOwner(owner);
        var sut = new OwnerSetupStateService(repo);

        var result = await sut.GetSetupStateAsync();

        Assert.True(result.HasOwner);
        Assert.False(result.PasswordChangeRequired);
        Assert.True(result.OnboardingCompleted);
        Assert.False(result.SetupRequired); // false because both flags are in "done" state
    }

    [Fact]
    public async Task GetSetupStateAsync_ReturnsSetupRequired_WhenBothFlagsFalse()
    {
        var owner = CreateOwner(passwordChangeRequired: false, onboardingCompleted: false);
        var repo = FakeOwnerRepository.WithOwner(owner);
        var sut = new OwnerSetupStateService(repo);

        var result = await sut.GetSetupStateAsync();

        Assert.True(result.HasOwner);
        Assert.False(result.PasswordChangeRequired);
        Assert.False(result.OnboardingCompleted);
        Assert.True(result.SetupRequired); // true because OnboardingCompleted=false
    }

    // Helpers
    private static OwnerProfile CreateOwner(bool passwordChangeRequired, bool onboardingCompleted)
    {
        var hasher = new FakePasswordHasher();
        var encryption = new FakeEncryptionService();

        var payload = JsonSerializer.SerializeToUtf8Bytes(new { Name = "Test Owner", Email = "test@example.com" });
        var encrypted = encryption.Encrypt("Test Owner");

        return OwnerProfile.Create(
            passwordHash: hasher.Hash("Password123!").Hash,
            passwordSalt: "salt",
            hashAlgorithm: "fake",
            encryptedProfilePayload: encrypted,
            encryptionVersion: 1,
            createdUtc: DateTime.UtcNow,
            passwordChangeRequired: passwordChangeRequired,
            onboardingCompleted: onboardingCompleted
        );
    }

    // Fake Implementations
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

    private class FakePasswordHasher : IPasswordHasher
    {
        public HashResult Hash(string password)
            => new($"hash::{password}", "salt", "fake");

        public bool Verify(string password, HashResult stored)
            => stored.Hash == $"hash::{password}";
    }
}
