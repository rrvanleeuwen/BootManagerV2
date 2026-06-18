using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

public class StorageAreaConfiguration : IEntityTypeConfiguration<StorageArea>
{
    public void Configure(EntityTypeBuilder<StorageArea> b)
    {
        b.ToTable("StorageAreas");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.Property(x => x.NormalizedName).IsRequired().HasMaxLength(100);

        b.HasIndex(x => x.NormalizedName).IsUnique();

        b.HasMany(x => x.Locations)
            .WithOne(l => l.StorageArea)
            .HasForeignKey(l => l.StorageAreaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
