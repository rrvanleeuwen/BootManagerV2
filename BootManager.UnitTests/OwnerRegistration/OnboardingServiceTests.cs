using BootManager.Application.OwnerRegistration.DTOs;
using BootManager.Application.OwnerRegistration.Services;
using BootManager.Application.VesselProfile.DTOs;
using BootManager.Application.VesselProfile.Services;
using BootManager.Core.Entities;
using BootManager.Core.Interfaces;
using BootManager.Core.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using System.Text.Json;
using Xunit;

namespace BootManager.UnitTests.OwnerRegistration;

public class OnboardingServiceTests
{
    private readonly FakePasswordHasher _hasher;
    private readonly FakeEncryptionService _encryption;
    private readonly Mock<ISystemClock> _mockClock;
    private readonly Mock<IVesselProfileService> _mockVesselService;
    private readonly Mock<ILogger<OnboardingService>> _mockLogger;

    public OnboardingServiceTests()
    {
        _hasher = new FakePasswordHasher();
        _encryption = new FakeEncryptionService();
        _mockClock = new Mock<ISystemClock>();
        _mockVesselService = new Mock<IVesselProfileService>();
        _mockLogger = new Mock<ILogger<OnboardingService>>();

        _mockClock.Setup(c => c.UtcNow).Returns(DateTime.UtcNow);
    }

    [Fact]
    public async Task CompleteInitialOnboardingAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var request = new CompleteOnboardingRequestDto
        {
            OwnerName = "John Doe",
            OwnerEmail = "john@example.com",
            VesselName = "My Boat",
            HomePort = "Amsterdam",
            CallSign = "PH-ABC",
            Mmsi = "123456789",
            CurrentPassword = "bootstrap123",
            NewPassword = "NewSecurePassword123!",
            ConfirmNewPassword = "NewSecurePassword123!"
        };

        var owner = CreateOwner("bootstrap123", passwordChangeRequired: true, onboardingCompleted: false);
        var repo = FakeOwnerRepository.WithOwner(owner);

        var vesselDto = new VesselProfileDto
        {
            Id = Guid.NewGuid(),
            VesselName = "My Boat",
            HomePort = "Amsterdam",
            CallSign = "PH-ABC",
            Mmsi = "123456789"
        };

        _mockVesselService
            .Setup(v => v.GetOrCreateVesselProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VesselProfileDto
            {
                Id = Guid.NewGuid(),
                VesselName = "Unnamed Vessel"
            });

        _mockVesselService
            .Setup(v => v.UpdateVesselProfileAsync(It.IsAny<UpdateVesselProfileRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vesselDto);

        var service = new OnboardingService(repo, _hasher, _encryption, _mockClock.Object, _mockVesselService.Object, _mockLogger.Object);

        // Act
        var response = await service.CompleteInitialOnboardingAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.Null(response.ErrorMessage);
        Assert.Equal("John Doe", response.UpdatedOwnerName);
        Assert.Equal("john@example.com", response.UpdatedOwnerEmail);
        Assert.NotNull(response.UpdatedVesselProfile);
        _mockVesselService.Verify(
            v => v.GetOrCreateVesselProfileAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CompleteInitialOnboardingAsync_WithMissingOwnerName_ReturnsFailure()
    {
        // Arrange
        var request = new CompleteOnboardingRequestDto
        {
            OwnerName = "", // Leeg
            VesselName = "My Boat",
            CurrentPassword = "bootstrap123",
            NewPassword = "NewSecurePassword123!",
            ConfirmNewPassword = "NewSecurePassword123!"
        };

        var owner = CreateOwner("bootstrap123");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var service = new OnboardingService(repo, _hasher, _encryption, _mockClock.Object, _mockVesselService.Object, _mockLogger.Object);

        // Act
        var response = await service.CompleteInitialOnboardingAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
        Assert.Contains("Eigenaarsnaam", response.ErrorMessage);
    }

    [Fact]
    public async Task CompleteInitialOnboardingAsync_WithMissingVesselName_ReturnsFailure()
    {
        // Arrange
        var request = new CompleteOnboardingRequestDto
        {
            OwnerName = "John Doe",
            VesselName = "", // Leeg
            CurrentPassword = "bootstrap123",
            NewPassword = "NewSecurePassword123!",
            ConfirmNewPassword = "NewSecurePassword123!"
        };

        var owner = CreateOwner("bootstrap123");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var service = new OnboardingService(repo, _hasher, _encryption, _mockClock.Object, _mockVesselService.Object, _mockLogger.Object);

        // Act
        var response = await service.CompleteInitialOnboardingAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
        Assert.Contains("Bootnaam", response.ErrorMessage);
    }

    [Fact]
    public async Task CompleteInitialOnboardingAsync_WithPasswordTooShort_ReturnsFailure()
    {
        // Arrange
        var request = new CompleteOnboardingRequestDto
        {
            OwnerName = "John Doe",
            VesselName = "My Boat",
            CurrentPassword = "bootstrap123",
            NewPassword = "short", // < 8 karakters
            ConfirmNewPassword = "short"
        };

        var owner = CreateOwner("bootstrap123");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var service = new OnboardingService(repo, _hasher, _encryption, _mockClock.Object, _mockVesselService.Object, _mockLogger.Object);

        // Act
        var response = await service.CompleteInitialOnboardingAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
        Assert.Contains("minimaal 8", response.ErrorMessage);
    }

    [Fact]
    public async Task CompleteInitialOnboardingAsync_WithPasswordMismatch_ReturnsFailure()
    {
        // Arrange
        var request = new CompleteOnboardingRequestDto
        {
            OwnerName = "John Doe",
            VesselName = "My Boat",
            CurrentPassword = "bootstrap123",
            NewPassword = "NewSecurePassword123!",
            ConfirmNewPassword = "DifferentPassword123!" // Niet gelijk
        };

        var owner = CreateOwner("bootstrap123");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var service = new OnboardingService(repo, _hasher, _encryption, _mockClock.Object, _mockVesselService.Object, _mockLogger.Object);

        // Act
        var response = await service.CompleteInitialOnboardingAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
        Assert.Contains("niet overeen", response.ErrorMessage);
    }

    [Fact]
    public async Task CompleteInitialOnboardingAsync_WithIncorrectCurrentPassword_ReturnsFailure()
    {
        // Arrange
        var request = new CompleteOnboardingRequestDto
        {
            OwnerName = "John Doe",
            VesselName = "My Boat",
            CurrentPassword = "wrongpassword",
            NewPassword = "NewSecurePassword123!",
            ConfirmNewPassword = "NewSecurePassword123!"
        };

        var owner = CreateOwner("bootstrap123");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var service = new OnboardingService(repo, _hasher, _encryption, _mockClock.Object, _mockVesselService.Object, _mockLogger.Object);

        // Act
        var response = await service.CompleteInitialOnboardingAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
        Assert.Contains("onjuist", response.ErrorMessage);
    }

    [Fact]
    public async Task CompleteInitialOnboardingAsync_WithNewPasswordSameAsOld_ReturnsFailure()
    {
        // Arrange
        var request = new CompleteOnboardingRequestDto
        {
            OwnerName = "John Doe",
            VesselName = "My Boat",
            CurrentPassword = "bootstrap123",
            NewPassword = "bootstrap123", // Hetzelfde
            ConfirmNewPassword = "bootstrap123"
        };

        var owner = CreateOwner("bootstrap123");
        var repo = FakeOwnerRepository.WithOwner(owner);
        var service = new OnboardingService(repo, _hasher, _encryption, _mockClock.Object, _mockVesselService.Object, _mockLogger.Object);

        // Act
        var response = await service.CompleteInitialOnboardingAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
        Assert.Contains("hetzelfde", response.ErrorMessage);
    }

    [Fact]
    public async Task CompleteInitialOnboardingAsync_WithNoOwnerFound_ReturnsFailure()
    {
        // Arrange
        var request = new CompleteOnboardingRequestDto
        {
            OwnerName = "John Doe",
            VesselName = "My Boat",
            CurrentPassword = "bootstrap123",
            NewPassword = "NewSecurePassword123!",
            ConfirmNewPassword = "NewSecurePassword123!"
        };

        var repo = FakeOwnerRepository.Empty();
        var service = new OnboardingService(repo, _hasher, _encryption, _mockClock.Object, _mockVesselService.Object, _mockLogger.Object);

        // Act
        var response = await service.CompleteInitialOnboardingAsync(request);

        // Assert
        Assert.False(response.Success);
        Assert.NotNull(response.ErrorMessage);
        Assert.Contains("No owner profile", response.ErrorMessage);
    }

    [Fact]
    public async Task CompleteInitialOnboardingAsync_WithOptionalFieldsEmpty_ReturnsSuccess()
    {
        // Arrange - alle optionele velden leeg
        var request = new CompleteOnboardingRequestDto
        {
            OwnerName = "John Doe",
            OwnerEmail = null, // Optioneel
            VesselName = "My Boat",
            HomePort = null, // Optioneel
            CallSign = null, // Optioneel
            Mmsi = null, // Optioneel
            CurrentPassword = "bootstrap123",
            NewPassword = "NewSecurePassword123!",
            ConfirmNewPassword = "NewSecurePassword123!"
        };

        var owner = CreateOwner("bootstrap123", passwordChangeRequired: true, onboardingCompleted: false);
        var repo = FakeOwnerRepository.WithOwner(owner);

        var vesselDto = new VesselProfileDto
        {
            Id = Guid.NewGuid(),
            VesselName = "My Boat",
            HomePort = null,
            CallSign = null,
            Mmsi = null
        };

        _mockVesselService
            .Setup(v => v.GetOrCreateVesselProfileAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VesselProfileDto
            {
                Id = Guid.NewGuid(),
                VesselName = "Unnamed Vessel"
            });

        _mockVesselService
            .Setup(v => v.UpdateVesselProfileAsync(It.IsAny<UpdateVesselProfileRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vesselDto);

        var service = new OnboardingService(repo, _hasher, _encryption, _mockClock.Object, _mockVesselService.Object, _mockLogger.Object);

        // Act
        var response = await service.CompleteInitialOnboardingAsync(request);

        // Assert
        Assert.True(response.Success);
        Assert.Null(response.ErrorMessage);
    }

    // Helpers
    private static OwnerProfile CreateOwner(
        string password,
        bool passwordChangeRequired = true,
        bool onboardingCompleted = false)
    {
        var hasher = new FakePasswordHasher();
        var encryption = new FakeEncryptionService();

        var payloadJson = JsonSerializer.Serialize(new { Name = "Bootstrap Owner", Email = "owner@bootmanager.local" });
        var encrypted = encryption.Encrypt(payloadJson);

        return OwnerProfile.Create(
            passwordHash: hasher.Hash(password).Hash,
            passwordSalt: hasher.Hash(password).Salt,
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

    private class FakePasswordHasher : IPasswordHasher
    {
        public HashResult Hash(string password)
            => new($"hash::{password}", "salt", "fake");

        public bool Verify(string password, HashResult stored)
            => stored.Hash == $"hash::{password}";
    }
}
