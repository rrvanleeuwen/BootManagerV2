using BootManager.Application.Authentication.DTOs;
using BootManager.Application.Authentication.Services;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;
using System.Linq.Expressions;

namespace BootManager.UnitTests.Authentication;

public class AccountServiceTests
{
    private readonly FakePasswordHasher _hasher = new();

    [Fact]
    public async Task ChangePasswordAsync_Succeeds_AndIncrementsCredentialVersion()
    {
        var user = CreateUser(password: "oldpass123");
        var oldVersion = user.CredentialVersion;
        var repo = FakeLocalUserRepository.WithUser(user);
        var sut = new AccountService(repo, _hasher);

        var result = await sut.ChangePasswordAsync(user.Id, new ChangePasswordDto
        {
            CurrentPassword = "oldpass123",
            NewPassword = "newpass456",
            ConfirmNewPassword = "newpass456"
        });

        Assert.True(result.Success);
        Assert.True(result.NewCredentialVersion > oldVersion);
        var updated = await repo.GetByIdAsync(user.Id);
        Assert.Equal("hash::newpass456", updated!.PasswordHash);
    }

    [Fact]
    public async Task ChangePasswordAsync_ClearsPasswordChangeRequired()
    {
        var user = CreateUser(password: "temp12345", passwordChangeRequired: true);
        var repo = FakeLocalUserRepository.WithUser(user);
        var sut = new AccountService(repo, _hasher);

        var result = await sut.ChangePasswordAsync(user.Id, new ChangePasswordDto
        {
            CurrentPassword = "temp12345",
            NewPassword = "newpass456",
            ConfirmNewPassword = "newpass456"
        });

        Assert.True(result.Success);
        var updated = await repo.GetByIdAsync(user.Id);
        Assert.False(updated!.PasswordChangeRequired);
    }

    [Fact]
    public async Task ChangePasswordAsync_Fails_WhenCurrentPasswordWrong()
    {
        var user = CreateUser(password: "correct123");
        var repo = FakeLocalUserRepository.WithUser(user);
        var sut = new AccountService(repo, _hasher);

        var result = await sut.ChangePasswordAsync(user.Id, new ChangePasswordDto
        {
            CurrentPassword = "wrongpass",
            NewPassword = "newpass456",
            ConfirmNewPassword = "newpass456"
        });

        Assert.False(result.Success);
        Assert.Contains("onjuist", result.Message ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ChangePasswordAsync_Fails_WhenPasswordTooShort()
    {
        var user = CreateUser(password: "correct123");
        var repo = FakeLocalUserRepository.WithUser(user);
        var sut = new AccountService(repo, _hasher);

        var result = await sut.ChangePasswordAsync(user.Id, new ChangePasswordDto
        {
            CurrentPassword = "correct123",
            NewPassword = "short",
            ConfirmNewPassword = "short"
        });

        Assert.False(result.Success);
        Assert.Contains("8", result.Message ?? "");
    }

    [Fact]
    public async Task ChangePasswordAsync_Fails_WhenConfirmPasswordMismatch()
    {
        var user = CreateUser(password: "correct123");
        var repo = FakeLocalUserRepository.WithUser(user);
        var sut = new AccountService(repo, _hasher);

        var result = await sut.ChangePasswordAsync(user.Id, new ChangePasswordDto
        {
            CurrentPassword = "correct123",
            NewPassword = "newpass456",
            ConfirmNewPassword = "different4"
        });

        Assert.False(result.Success);
        Assert.Contains("overeen", result.Message ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ChangePasswordAsync_Fails_WhenNewPasswordSameAsCurrent()
    {
        var user = CreateUser(password: "same12345");
        var repo = FakeLocalUserRepository.WithUser(user);
        var sut = new AccountService(repo, _hasher);

        var result = await sut.ChangePasswordAsync(user.Id, new ChangePasswordDto
        {
            CurrentPassword = "same12345",
            NewPassword = "same12345",
            ConfirmNewPassword = "same12345"
        });

        Assert.False(result.Success);
        Assert.Contains("verschillen", result.Message ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ChangePasswordAsync_ReturnsDisplayNameAndRole_ForCookieRenewal()
    {
        var user = CreateUser(password: "oldpass123", role: LocalUserRole.Owner);
        var repo = FakeLocalUserRepository.WithUser(user);
        var sut = new AccountService(repo, _hasher);

        var result = await sut.ChangePasswordAsync(user.Id, new ChangePasswordDto
        {
            CurrentPassword = "oldpass123",
            NewPassword = "newpass456",
            ConfirmNewPassword = "newpass456"
        });

        Assert.True(result.Success);
        Assert.Equal("TestUser", result.DisplayName);
        Assert.Equal(LocalUserRole.Owner, result.Role);
    }

    [Fact]
    public async Task ChangePasswordAsync_WorksForCrew()
    {
        var crew = CreateUser(password: "crewpass1", role: LocalUserRole.Crew, passwordChangeRequired: true);
        var repo = FakeLocalUserRepository.WithUser(crew);
        var sut = new AccountService(repo, _hasher);

        var result = await sut.ChangePasswordAsync(crew.Id, new ChangePasswordDto
        {
            CurrentPassword = "crewpass1",
            NewPassword = "newcrew456",
            ConfirmNewPassword = "newcrew456"
        });

        Assert.True(result.Success);
        var updated = await repo.GetByIdAsync(crew.Id);
        Assert.False(updated!.PasswordChangeRequired);
    }

    private LocalUser CreateUser(
        string password,
        LocalUserRole role = LocalUserRole.Owner,
        bool passwordChangeRequired = false)
    {
        var hash = _hasher.Hash(password);
        return LocalUser.Create(
            displayName: "TestUser",
            role: role,
            passwordHash: hash.Hash,
            passwordSalt: hash.Salt,
            hashAlgorithm: hash.Algorithm,
            encryptedProfilePayload: Array.Empty<byte>(),
            encryptionVersion: 1,
            createdUtc: DateTime.UtcNow,
            passwordChangeRequired: passwordChangeRequired);
    }

    private sealed class FakeLocalUserRepository : IRepository<LocalUser>
    {
        private List<LocalUser> _users = [];

        public static FakeLocalUserRepository WithUser(LocalUser user) => new() { _users = [user] };

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
