using BootManager.Application.Authentication.DTOs;
using BootManager.Application.Authentication.Services;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;

namespace BootManager.UnitTests.Authentication;

public class OwnerSettingsServiceTests
{
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeClock _clock = new();

    [Fact]
    public async Task ChangePassword_Succeeds_WhenCurrentPasswordValid()
    {
        var owner = CreateOwner(password: "oldpass");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var sut = new OwnerSettingsService(repo, _hasher, _clock);

        var req = new ChangePasswordRequestDto { CurrentPassword = "oldpass", NewPassword = "newpass", ConfirmNewPassword = "newpass" };
        await sut.ChangePasswordAsync(req);

        // verify owner updated in repo
        var updated = await repo.SingleOrDefaultAsync();
        Assert.Equal("hash::newpass", updated!.PasswordHash);
    }

    [Fact]
    public async Task ChangePassword_ReplacesOldWithNew()
    {
        var owner = CreateOwner(password: "123456");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var sut = new OwnerSettingsService(repo, _hasher, _clock);

        var req = new ChangePasswordRequestDto { CurrentPassword = "123456", NewPassword = "1234abcd", ConfirmNewPassword = "1234abcd" };
        await sut.ChangePasswordAsync(req);

        var updated = await repo.SingleOrDefaultAsync();
        Assert.Equal("hash::1234abcd", updated!.PasswordHash);

        // old should fail, new should succeed
        var stored = new BootManager.Core.ValueObjects.HashResult(updated.PasswordHash, updated.PasswordSalt, updated.HashAlgorithm);
        Assert.False(_hasher.Verify("123456", stored));
        Assert.True(_hasher.Verify("1234abcd", stored));
    }

    [Fact]
    public async Task ChangePassword_Fails_WhenCurrentInvalid()
    {
        var owner = CreateOwner(password: "oldpass");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var sut = new OwnerSettingsService(repo, _hasher, _clock);

        var req = new ChangePasswordRequestDto { CurrentPassword = "bad", NewPassword = "newpass", ConfirmNewPassword = "newpass" };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await sut.ChangePasswordAsync(req));
    }

    [Fact]
    public async Task SetPin_Succeeds_WhenAuthenticatedByPassword()
    {
        var owner = CreateOwner(password: "mypwd");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var sut = new OwnerSettingsService(repo, _hasher, _clock);

        var req = new ChangePinRequestDto { CurrentPasswordOrPin = "mypwd", NewPin = "1234", ConfirmNewPin = "1234" };
        await sut.SetPinAsync(req);

        var updated = await repo.SingleOrDefaultAsync();
        Assert.Equal("hash::1234", updated!.PinHash);
    }

    [Fact]
    public async Task SetPin_Fails_WhenNotAuthenticated()
    {
        var owner = CreateOwner(password: "mypwd");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var sut = new OwnerSettingsService(repo, _hasher, _clock);

        var req = new ChangePinRequestDto { CurrentPasswordOrPin = "wrong", NewPin = "1234", ConfirmNewPin = "1234" };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await sut.SetPinAsync(req));
    }

    private OwnerProfile CreateOwner(string password, string? pin = null)
    {
        var passwordHash = _hasher.Hash(password);
        var owner = OwnerProfile.Create(
            passwordHash: passwordHash.Hash,
            passwordSalt: passwordHash.Salt,
            hashAlgorithm: passwordHash.Algorithm,
            encryptedProfilePayload: Array.Empty<byte>(),
            encryptionVersion: 1,
            createdUtc: DateTime.UtcNow);

        if (!string.IsNullOrEmpty(pin))
        {
            var pinHash = _hasher.Hash(pin);
            owner.SetPin(pinHash.Hash, pinHash.Salt, DateTime.UtcNow);
        }

        return owner;
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

    private sealed class FakeOwnerRepository : IRepository<OwnerProfile>
    {
        private OwnerProfile? _owner;

        private FakeOwnerRepository(OwnerProfile? owner)
        {
            _owner = owner;
        }

        public static FakeOwnerRepository WithOwner(OwnerProfile owner) => new(owner);
        public static FakeOwnerRepository Empty() => new(null);

        public Task<OwnerProfile?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => Task.FromResult(_owner is not null && _owner.Id == id ? _owner : null);

        public Task<OwnerProfile?> SingleOrDefaultAsync(System.Linq.Expressions.Expression<Func<OwnerProfile, bool>>? predicate = null, CancellationToken ct = default)
        {
            if (_owner is null || predicate is null)
            {
                return Task.FromResult(_owner);
            }

            var compiled = predicate.Compile();
            return Task.FromResult(compiled(_owner) ? _owner : null);
        }

        public Task<IReadOnlyList<OwnerProfile>> ListAsync(System.Linq.Expressions.Expression<Func<OwnerProfile, bool>>? predicate = null, CancellationToken ct = default)
        {
            var list = _owner is null ? Array.Empty<OwnerProfile>() : new[] { _owner };
            if (predicate is null || _owner is null)
            {
                return Task.FromResult((IReadOnlyList<OwnerProfile>)list);
            }

            var compiled = predicate.Compile();
            return Task.FromResult((IReadOnlyList<OwnerProfile>)(compiled(_owner) ? list : Array.Empty<OwnerProfile>()));
        }

        public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<OwnerProfile, bool>>? predicate = null, CancellationToken ct = default)
        {
            if (_owner is null)
            {
                return Task.FromResult(false);
            }

            if (predicate is null)
            {
                return Task.FromResult(true);
            }

            var compiled = predicate.Compile();
            return Task.FromResult(compiled(_owner));
        }

        public Task AddAsync(OwnerProfile entity, CancellationToken ct = default)
        {
            _owner = entity;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(OwnerProfile entity, CancellationToken ct = default)
        {
            _owner = entity;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(OwnerProfile entity, CancellationToken ct = default)
        {
            if (_owner == entity)
            {
                _owner = null;
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FakeClock : ISystemClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    // no-op logger not required because OwnerSettingsService has a compatible ctor
}
