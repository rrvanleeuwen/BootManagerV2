using BootManager.Application.Authentication.DTOs;
using BootManager.Application.Authentication.Services;
using BootManager.Core.Entities;
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
        var owner = CreateOwner(password: "123456");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var sut = new OwnerLoginService(repo, _hasher);

        var result = await sut.ValidateAsync(new LoginRequestDto { Password = "123456" });

        Assert.True(result.Success);
        Assert.Equal(owner.Id, result.OwnerId);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFailure_ForWrongPassword()
    {
        var owner = CreateOwner(password: "123456");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var sut = new OwnerLoginService(repo, _hasher);

        var result = await sut.ValidateAsync(new LoginRequestDto { Password = "bad" });

        Assert.False(result.Success);
        Assert.Equal("Ongeldig wachtwoord.", result.Message);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFailure_WhenNoOwnerExists()
    {
        var repo = FakeOwnerRepository.Empty();
        var sut = new OwnerLoginService(repo, _hasher);

        var result = await sut.ValidateAsync(new LoginRequestDto { Password = "irrelevant" });

        Assert.False(result.Success);
        Assert.Equal("Geen eigenaarprofiel gevonden.", result.Message);
    }

    [Fact]
    public async Task ValidateAsync_UsesPin_WhenPasswordMissing()
    {
        var owner = CreateOwner(password: "abcdef", pin: "7777");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var sut = new OwnerLoginService(repo, _hasher);

        var result = await sut.ValidateAsync(new LoginRequestDto { Pin = "7777" });

        Assert.True(result.Success);
        Assert.Equal(owner.Id, result.OwnerId);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFailure_WhenPinNotConfigured()
    {
        var owner = CreateOwner(password: "abcdef");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var sut = new OwnerLoginService(repo, _hasher);

        var result = await sut.ValidateAsync(new LoginRequestDto { Pin = "1234" });

        Assert.False(result.Success);
        Assert.Equal("Er is geen pincode ingesteld.", result.Message);
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

        public Task<OwnerProfile?> SingleOrDefaultAsync(Expression<Func<OwnerProfile, bool>>? predicate = null, CancellationToken ct = default)
        {
            if (_owner is null || predicate is null)
            {
                return Task.FromResult(_owner);
            }

            var compiled = predicate.Compile();
            return Task.FromResult(compiled(_owner) ? _owner : null);
        }

        public Task<IReadOnlyList<OwnerProfile>> ListAsync(Expression<Func<OwnerProfile, bool>>? predicate = null, CancellationToken ct = default)
        {
            var list = _owner is null ? Array.Empty<OwnerProfile>() : new[] { _owner };
            if (predicate is null || _owner is null)
            {
                return Task.FromResult((IReadOnlyList<OwnerProfile>)list);
            }

            var compiled = predicate.Compile();
            return Task.FromResult((IReadOnlyList<OwnerProfile>)(compiled(_owner) ? list : Array.Empty<OwnerProfile>()));
        }

        public Task<bool> AnyAsync(Expression<Func<OwnerProfile, bool>>? predicate = null, CancellationToken ct = default)
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
}
