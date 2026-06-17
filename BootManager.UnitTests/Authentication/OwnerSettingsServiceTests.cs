using BootManager.Application.Authentication.DTOs;
using BootManager.Application.Authentication.Services;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;
using System.Linq.Expressions;
using System.Text.Json;

namespace BootManager.UnitTests.Authentication;

public class OwnerSettingsServiceTests
{
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeClock _clock = new();
    private readonly FakeEncryptionService _encryption = new();

    [Fact]
    public async Task ChangePassword_Succeeds_WhenCurrentPasswordValid()
    {
        var owner = CreateOwner(password: "oldpass");
        var repo = FakeLocalUserRepository.WithUser(owner);
        var sut = new OwnerSettingsService(repo, _hasher, _clock, _encryption);

        var req = new ChangePasswordRequestDto { CurrentPassword = "oldpass", NewPassword = "newpass1", ConfirmNewPassword = "newpass1" };
        await sut.ChangePasswordAsync(req);

        var updated = await repo.SingleOrDefaultAsync(u => u.Role == LocalUserRole.Owner);
        Assert.Equal("hash::newpass1", updated!.PasswordHash);
    }

    [Fact]
    public async Task ChangePassword_Fails_WhenCurrentInvalid()
    {
        var owner = CreateOwner(password: "oldpass");
        var repo = FakeLocalUserRepository.WithUser(owner);
        var sut = new OwnerSettingsService(repo, _hasher, _clock, _encryption);

        var req = new ChangePasswordRequestDto { CurrentPassword = "bad", NewPassword = "newpass1", ConfirmNewPassword = "newpass1" };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await sut.ChangePasswordAsync(req));
    }

    [Fact]
    public async Task GetOwnerProfile_Succeeds_ReturnsNameAndEmail()
    {
        var owner = CreateOwner(password: "pwd", name: "John Doe", email: "john@example.com");
        var repo = FakeLocalUserRepository.WithUser(owner);
        var sut = new OwnerSettingsService(repo, _hasher, _clock, _encryption);

        var result = await sut.GetOwnerProfileAsync();

        Assert.NotNull(result);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal("john@example.com", result.Email);
    }

    [Fact]
    public async Task GetOwnerProfile_Fails_WhenNoOwnerExists()
    {
        var repo = FakeLocalUserRepository.Empty();
        var sut = new OwnerSettingsService(repo, _hasher, _clock, _encryption);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.GetOwnerProfileAsync());
    }

    [Fact]
    public async Task UpdateOwnerProfile_Succeeds_UpdatesNameAndEmail()
    {
        var owner = CreateOwner(password: "pwd", name: "Old Name", email: "old@example.com");
        var repo = FakeLocalUserRepository.WithUser(owner);
        var sut = new OwnerSettingsService(repo, _hasher, _clock, _encryption);

        var req = new UpdateOwnerProfileRequestDto { Name = "New Name", Email = "new@example.com" };
        await sut.UpdateOwnerProfileAsync(req);

        var updated = await repo.SingleOrDefaultAsync(u => u.Role == LocalUserRole.Owner);
        Assert.NotNull(updated);
        Assert.Equal("New Name", updated.DisplayName); // Now synced
    }

    [Fact]
    public async Task UpdateOwnerProfile_Fails_WhenNameAlreadyExists()
    {
        var owner1 = CreateOwner(password: "pwd", name: "Owner1", displayName: "Owner1");
        var owner2 = CreateOwner(password: "pwd", name: "Owner2", displayName: "Owner2");
        var repo = FakeLocalUserRepository.WithUsers(owner1, owner2);
        var sut = new OwnerSettingsService(repo, _hasher, _clock, _encryption);

        var req = new UpdateOwnerProfileRequestDto { Name = "Owner2", Email = "" };
        await Assert.ThrowsAsync<ArgumentException>(async () => await sut.UpdateOwnerProfileAsync(req));
    }

    private static LocalUser CreateOwner(
        string password,
        string name = "Owner",
        string? email = null,
        string? displayName = null)
    {
        var hasher = new FakePasswordHasher();
        var encryption = new FakeEncryptionService();
        var hash = hasher.Hash(password);

        var payloadObj = new { Name = name, Email = email ?? string.Empty };
        var json = JsonSerializer.Serialize(payloadObj);
        var encrypted = encryption.Encrypt(json);

        return LocalUser.Create(
            displayName: displayName ?? name,
            role: LocalUserRole.Owner,
            passwordHash: hash.Hash,
            passwordSalt: hash.Salt,
            hashAlgorithm: hash.Algorithm,
            encryptedProfilePayload: encrypted,
            encryptionVersion: 1,
            createdUtc: DateTime.UtcNow);
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

    private sealed class FakeEncryptionService : IEncryptionService
    {
        public byte[] Encrypt(string plaintext)
            => System.Text.Encoding.UTF8.GetBytes(plaintext);

        public string Decrypt(byte[] ciphertext)
            => System.Text.Encoding.UTF8.GetString(ciphertext);
    }

    private sealed class FakeClock : ISystemClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public HashResult Hash(string password)
            => new($"hash::{password}", "salt", "fake");

        public bool Verify(string password, HashResult stored)
            => stored.Hash == $"hash::{password}";
    }
}
