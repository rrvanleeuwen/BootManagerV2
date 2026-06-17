using BootManager.Application.OwnerRegistration.Services;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;
using System.Linq.Expressions;

namespace BootManager.UnitTests.OwnerRegistration;

public class OwnerSetupStateServiceTests
{
    [Fact]
    public async Task GetSetupStateAsync_ReturnsHasOwnerFalse_WhenNullUserId()
    {
        var repo = FakeLocalUserRepository.Empty();
        var sut = new OwnerSetupStateService(repo);

        var result = await sut.GetSetupStateAsync(null);

        Assert.False(result.HasOwner);
        Assert.False(result.PasswordChangeRequired);
        Assert.False(result.OnboardingCompleted);
    }

    [Fact]
    public async Task GetSetupStateAsync_ReturnsHasOwnerFalse_WhenUserNotFound()
    {
        var repo = FakeLocalUserRepository.Empty();
        var sut = new OwnerSetupStateService(repo);

        var result = await sut.GetSetupStateAsync(Guid.NewGuid());

        Assert.False(result.HasOwner);
    }

    [Fact]
    public async Task GetSetupStateAsync_ReturnsSetupRequired_WhenOwnerPasswordChangeRequired()
    {
        var owner = CreateLocalUser(displayName: "Owner", role: LocalUserRole.Owner, passwordChangeRequired: true, onboardingCompleted: true);
        var repo = FakeLocalUserRepository.WithUser(owner);
        var sut = new OwnerSetupStateService(repo);

        var result = await sut.GetSetupStateAsync(owner.Id);

        Assert.True(result.HasOwner);
        Assert.True(result.PasswordChangeRequired);
        Assert.True(result.OnboardingCompleted);
        Assert.True(result.SetupRequired);
    }

    [Fact]
    public async Task GetSetupStateAsync_ReturnsSetupRequired_WhenOwnerOnboardingNotCompleted()
    {
        var owner = CreateLocalUser(displayName: "Owner", role: LocalUserRole.Owner, passwordChangeRequired: false, onboardingCompleted: false);
        var repo = FakeLocalUserRepository.WithUser(owner);
        var sut = new OwnerSetupStateService(repo);

        var result = await sut.GetSetupStateAsync(owner.Id);

        Assert.True(result.HasOwner);
        Assert.False(result.PasswordChangeRequired);
        Assert.False(result.OnboardingCompleted);
        Assert.True(result.SetupRequired);
    }

    [Fact]
    public async Task GetSetupStateAsync_ReturnsSetupNotRequired_WhenOwnerComplete()
    {
        var owner = CreateLocalUser(displayName: "Owner", role: LocalUserRole.Owner, passwordChangeRequired: false, onboardingCompleted: true);
        var repo = FakeLocalUserRepository.WithUser(owner);
        var sut = new OwnerSetupStateService(repo);

        var result = await sut.GetSetupStateAsync(owner.Id);

        Assert.True(result.HasOwner);
        Assert.False(result.PasswordChangeRequired);
        Assert.True(result.OnboardingCompleted);
        Assert.False(result.SetupRequired);
    }

    [Fact]
    public async Task GetSetupStateAsync_CrewWithPasswordChangeRequired()
    {
        var crew = CreateLocalUser(displayName: "Crew", role: LocalUserRole.Crew, passwordChangeRequired: true, onboardingCompleted: true);
        var repo = FakeLocalUserRepository.WithUser(crew);
        var sut = new OwnerSetupStateService(repo);

        var result = await sut.GetSetupStateAsync(crew.Id);

        Assert.True(result.HasOwner);
        Assert.True(result.PasswordChangeRequired);
        Assert.True(result.SetupRequired);
    }

    [Fact]
    public async Task GetSetupStateAsync_CrewWithoutPasswordChange()
    {
        var crew = CreateLocalUser(displayName: "Crew", role: LocalUserRole.Crew, passwordChangeRequired: false, onboardingCompleted: true);
        var repo = FakeLocalUserRepository.WithUser(crew);
        var sut = new OwnerSetupStateService(repo);

        var result = await sut.GetSetupStateAsync(crew.Id);

        Assert.True(result.HasOwner);
        Assert.False(result.PasswordChangeRequired);
        Assert.False(result.SetupRequired);
    }

    private static LocalUser CreateLocalUser(
        string displayName,
        LocalUserRole role,
        bool passwordChangeRequired,
        bool onboardingCompleted)
    {
        var hasher = new FakePasswordHasher();
        var hash = hasher.Hash("DefaultPassword123!");

        var user = LocalUser.Create(
            displayName: displayName,
            role: role,
            passwordHash: hash.Hash,
            passwordSalt: hash.Salt,
            hashAlgorithm: hash.Algorithm,
            encryptedProfilePayload: Array.Empty<byte>(),
            encryptionVersion: 1,
            createdUtc: DateTime.UtcNow,
            passwordChangeRequired: passwordChangeRequired,
            onboardingCompleted: onboardingCompleted);

        return user;
    }

    private sealed class FakeLocalUserRepository : IRepository<LocalUser>
    {
        private readonly List<LocalUser> _users = [];

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

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public HashResult Hash(string password)
            => new($"hash::{password}", "salt", "fake");

        public bool Verify(string password, HashResult stored)
            => stored.Hash == $"hash::{password}";
    }
}
