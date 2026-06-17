using BootManager.Application.OwnerRegistration.DTOs;
using BootManager.Application.OwnerRegistration.Services;
using BootManager.Application.VesselProfile.DTOs;
using BootManager.Application.VesselProfile.Services;
using BootManager.Core.Entities;
using BootManager.Core.Enums;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Text.Json;

namespace BootManager.UnitTests.OwnerRegistration;

public class OnboardingServiceTests
{
    private readonly FakePasswordHasher _hasher = new();
    private readonly FakeEncryptionService _encryption = new();
    private readonly FakeClock _clock = new();
    private readonly FakeVesselProfileService _vesselService = new();
    private readonly FakeLogger<OnboardingService> _logger = new();

    [Fact]
    public async Task CompleteInitialOnboarding_Succeeds_WhenValidRequest()
    {
        var owner = CreateOwner(password: "bootstrap123");
        var repo = FakeLocalUserRepository.WithUser(owner);
        var sut = new OnboardingService(repo, _hasher, _encryption, _clock, _vesselService, _logger);

        var req = new CompleteOnboardingRequestDto
        {
            CurrentPassword = "bootstrap123",
            NewPassword = "newowner123",
            ConfirmNewPassword = "newowner123",
            OwnerName = "Roelof",
            OwnerEmail = "roelof@example.com",
            VesselName = "Linde",
            HomePort = "Amsterdam",
            CallSign = "PX1",
            Mmsi = "245123456"
        };

        var result = await sut.CompleteInitialOnboardingAsync(req);

        Assert.True(result.Success);
        var updated = await repo.SingleOrDefaultAsync(u => u.Role == LocalUserRole.Owner);
        Assert.Equal("hash::newowner123", updated!.PasswordHash);
        Assert.True(updated.OnboardingCompleted);
        Assert.False(updated.PasswordChangeRequired);
    }

    [Fact]
    public async Task CompleteInitialOnboarding_Fails_WhenPasswordTooShort()
    {
        var owner = CreateOwner();
        var repo = FakeLocalUserRepository.WithUser(owner);
        var sut = new OnboardingService(repo, _hasher, _encryption, _clock, _vesselService, _logger);

        var req = new CompleteOnboardingRequestDto
        {
            CurrentPassword = "bootstrap",
            NewPassword = "short",
            ConfirmNewPassword = "short",
            OwnerName = "Test",
            VesselName = "Test"
        };

        var result = await sut.CompleteInitialOnboardingAsync(req);
        Assert.False(result.Success);
    }

    private LocalUser CreateOwner(string password = "bootstrap")
    {
        var hasher = new FakePasswordHasher();
        var encryption = new FakeEncryptionService();
        var hash = hasher.Hash(password);

        var payloadObj = new { Name = "Owner", Email = "owner@example.com" };
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
            passwordChangeRequired: true,
            onboardingCompleted: false);
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

    private sealed class FakeEncryptionService : IEncryptionService
    {
        public byte[] Encrypt(string plaintext) => System.Text.Encoding.UTF8.GetBytes(plaintext);
        public string Decrypt(byte[] ciphertext) => System.Text.Encoding.UTF8.GetString(ciphertext);
    }

    private sealed class FakeClock : ISystemClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public HashResult Hash(string password) => new($"hash::{password}", "salt", "fake");
        public bool Verify(string password, HashResult stored) => stored.Hash == $"hash::{password}";
    }

    private sealed class FakeVesselProfileService : IVesselProfileService
    {
        public Task<VesselProfileDto> GetOrCreateVesselProfileAsync(CancellationToken ct = default)
            => Task.FromResult(new VesselProfileDto { Id = Guid.NewGuid(), VesselName = "Test" });

        public Task<VesselProfileDto> UpdateVesselProfileAsync(UpdateVesselProfileRequestDto request, CancellationToken ct = default)
            => Task.FromResult(new VesselProfileDto { Id = Guid.NewGuid(), VesselName = request.VesselName });

        public Task<VesselProfileDto> GetVesselProfileAsync(CancellationToken ct = default)
            => Task.FromResult(new VesselProfileDto { Id = Guid.NewGuid(), VesselName = "Test" });

        public Task<VesselProfileDto> AdvanceCurrentMetersAsync(decimal?[] engineHoursCandidates, decimal?[] logstandCandidates, CancellationToken ct = default)
            => Task.FromResult(new VesselProfileDto { Id = Guid.NewGuid(), VesselName = "Test" });
    }

    private sealed class FakeLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}
