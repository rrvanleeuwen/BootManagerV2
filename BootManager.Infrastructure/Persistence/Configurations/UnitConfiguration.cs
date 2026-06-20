using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> b)
    {
        b.ToTable("Units");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.Property(x => x.NormalizedName).IsRequired().HasMaxLength(100);
        b.Property(x => x.ArchivedAt);

        b.HasIndex(x => x.NormalizedName).IsUnique();

        b.HasMany(x => x.Products)
            .WithOne(p => p.DefaultUnit)
            .HasForeignKey(p => p.DefaultUnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
