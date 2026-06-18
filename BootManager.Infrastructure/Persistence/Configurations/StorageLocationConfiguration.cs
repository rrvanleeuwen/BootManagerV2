using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

public class StorageLocationConfiguration : IEntityTypeConfiguration<StorageLocation>
{
    public void Configure(EntityTypeBuilder<StorageLocation> b)
    {
        b.ToTable("StorageLocations");
        b.HasKey(x => x.Id);

        b.Property(x => x.StorageAreaId).IsRequired();
        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.Property(x => x.NormalizedName).IsRequired().HasMaxLength(100);
        b.Property(x => x.Description).HasMaxLength(500);

        b.HasIndex(x => new { x.StorageAreaId, x.NormalizedName }).IsUnique();

        b.HasOne(x => x.StorageArea)
            .WithMany(a => a.Locations)
            .HasForeignKey(x => x.StorageAreaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
