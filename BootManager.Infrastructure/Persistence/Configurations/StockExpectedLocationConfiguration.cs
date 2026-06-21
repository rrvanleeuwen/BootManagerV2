using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

public class StockExpectedLocationConfiguration : IEntityTypeConfiguration<StockExpectedLocation>
{
    public void Configure(EntityTypeBuilder<StockExpectedLocation> b)
    {
        b.ToTable("StockExpectedLocations");
        b.HasKey(x => x.Id);

        b.Property(x => x.ProductId).IsRequired();
        b.Property(x => x.StorageLocationId).IsRequired();
        b.Property(x => x.UpdatedAt).IsRequired();

        b.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.StorageLocation)
            .WithMany()
            .HasForeignKey(x => x.StorageLocationId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.ProductId).IsUnique();
        b.HasIndex(x => x.UpdatedAt).IsDescending();
    }
}
