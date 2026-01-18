using BootManager.Application.Authentication.Services;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;

namespace BootManager.UnitTests.Authentication;

public class OwnerRecoveryServiceTests
{
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeEncryption _encryption = new();
    private readonly FakeClock _clock = new();

    [Fact]
    public async Task RestoreWithBackupCode_Succeeds_WhenCorrect()
    {
        var owner = CreateOwnerWithRecovery("RCODE123");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var sut = new OwnerRecoveryService(repo, _hasher, _encryption, _clock);

        var res = await sut.RestoreWithBackupCodeAsync("RCODE123", "newpw");
        Assert.True(res.Success);

        var updated = await repo.SingleOrDefaultAsync();
        Assert.Null(updated!.RecoveryCodeHash);
    }

    [Fact]
    public async Task RestoreWithBackupCode_Fails_WhenIncorrect()
    {
        var owner = CreateOwnerWithRecovery("RCODE123");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var sut = new OwnerRecoveryService(repo, _hasher, _encryption, _clock);

        var res = await sut.RestoreWithBackupCodeAsync("BAD", "newpw");
        Assert.False(res.Success);
    }

    [Fact]
    public async Task RestoreWithMasterKey_AttemptsDecrypt()
    {
        var owner = CreateOwnerWithRecovery("RCODE123");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var sut = new OwnerRecoveryService(repo, _hasher, _encryption, _clock);

        var res = await sut.RestoreWithMasterKeyAsync("ANY", "newpw");
        Assert.True(res.Success);
    }

    private OwnerProfile CreateOwnerWithRecovery(string code)
    {
        var pwd = _hasher.Hash("pw");
        var owner = OwnerProfile.Create(pwd.Hash, pwd.Salt, pwd.Algorithm, _encryption.Encrypt("{}"), 1, DateTime.UtcNow);
        var rc = _hasher.Hash(code);
        owner.SetRecoveryCode(rc.Hash, rc.Salt, DateTime.UtcNow);
        return owner;
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public HashResult Hash(string password) => new(password + "-h", "salt", "fake");
        public bool Verify(string password, HashResult stored) => stored.Hash == password + "-h";
    }

    private sealed class FakeEncryption : IEncryptionService
    {
        public byte[] Encrypt(string plainText) => System.Text.Encoding.UTF8.GetBytes(plainText);
        public string Decrypt(byte[] cipherBytes) => System.Text.Encoding.UTF8.GetString(cipherBytes);
    }

    private sealed class FakeClock : ISystemClock { public DateTime UtcNow => DateTime.UtcNow; }

    private sealed class FakeOwnerRepository : IRepository<OwnerProfile>
    {
        private OwnerProfile? _owner;
        private FakeOwnerRepository(OwnerProfile? owner) { _owner = owner; }
        public static FakeOwnerRepository WithOwner(OwnerProfile owner) => new(owner);
        public static FakeOwnerRepository Empty() => new(null);
        public Task<OwnerProfile?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(_owner is not null && _owner.Id == id ? _owner : null);
        public Task<OwnerProfile?> SingleOrDefaultAsync(System.Linq.Expressions.Expression<Func<OwnerProfile, bool>>? predicate = null, CancellationToken ct = default)
        {
            if (_owner is null || predicate is null) return Task.FromResult(_owner);
            var compiled = predicate.Compile();
            return Task.FromResult(compiled(_owner) ? _owner : null);
        }
        public Task<IReadOnlyList<OwnerProfile>> ListAsync(System.Linq.Expressions.Expression<Func<OwnerProfile, bool>>? predicate = null, CancellationToken ct = default)
        {
            var list = _owner is null ? Array.Empty<OwnerProfile>() : new[] { _owner };
            if (predicate is null || _owner is null) return Task.FromResult((IReadOnlyList<OwnerProfile>)list);
            var compiled = predicate.Compile();
            return Task.FromResult((IReadOnlyList<OwnerProfile>)(compiled(_owner) ? list : Array.Empty<OwnerProfile>()));
        }
        public Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<OwnerProfile, bool>>? predicate = null, CancellationToken ct = default)
        {
            if (_owner is null) return Task.FromResult(false);
            if (predicate is null) return Task.FromResult(true);
            var compiled = predicate.Compile();
            return Task.FromResult(compiled(_owner));
        }
        public Task AddAsync(OwnerProfile entity, CancellationToken ct = default) { _owner = entity; return Task.CompletedTask; }
        public Task UpdateAsync(OwnerProfile entity, CancellationToken ct = default) { _owner = entity; return Task.CompletedTask; }
        public Task DeleteAsync(OwnerProfile entity, CancellationToken ct = default) { if (_owner == entity) _owner = null; return Task.CompletedTask; }
    }
}
