using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

public class OwnerProfileConfiguration : IEntityTypeConfiguration<OwnerProfile>
{
    public void Configure(EntityTypeBuilder<OwnerProfile> b)
    {
        b.ToTable("OwnerProfiles");
        b.HasKey(x => x.Id);
        b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
        b.Property(x => x.PasswordSalt).IsRequired().HasMaxLength(256);
        b.Property(x => x.HashAlgorithm).IsRequired().HasMaxLength(64);
        b.Property(x => x.RecoveryCodeHash).HasMaxLength(512);
        b.Property(x => x.RecoveryCodeSalt).HasMaxLength(256);
        b.Property(x => x.EncryptedProfilePayload).IsRequired();
        b.Property(x => x.EncryptionVersion).IsRequired();
        b.Property(x => x.CreatedUtc).IsRequired();
        b.HasIndex(nameof(OwnerProfile.Id)).IsUnique();
        // Constraint: maximaal 1 row (afdwingen via applicatielogica; optioneel trigger/ check)
    }
}