using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

public class LocalUserConfiguration : IEntityTypeConfiguration<LocalUser>
{
    public void Configure(EntityTypeBuilder<LocalUser> b)
    {
        b.ToTable("LocalUsers");
        b.HasKey(x => x.Id);

        b.Property(x => x.DisplayName).IsRequired().HasMaxLength(100);
        b.Property(x => x.NormalizedName).IsRequired().HasMaxLength(100);
        b.Property(x => x.Role).IsRequired();
        b.Property(x => x.IsActive).IsRequired();

        b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
        b.Property(x => x.PasswordSalt).IsRequired().HasMaxLength(256);
        b.Property(x => x.HashAlgorithm).IsRequired().HasMaxLength(64);
        b.Property(x => x.CredentialVersion).IsRequired();

        b.Property(x => x.PasswordChangeRequired).IsRequired();
        b.Property(x => x.OnboardingCompleted).IsRequired();

        b.Property(x => x.EncryptedProfilePayload).IsRequired();
        b.Property(x => x.EncryptionVersion).IsRequired();

        // Legacy fields for migration compatibility
        b.Property(x => x.PinHash).HasMaxLength(512);
        b.Property(x => x.PinSalt).HasMaxLength(256);
        b.Property(x => x.RecoveryCodeHash).HasMaxLength(512);
        b.Property(x => x.RecoveryCodeSalt).HasMaxLength(256);

        b.Property(x => x.CreatedUtc).IsRequired();

        // Unique index on normalized name
        b.HasIndex(x => x.NormalizedName).IsUnique();
    }
}
