using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

public class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> b)
    {
        b.ToTable("Stocks");
        b.HasKey(x => x.Id);

        b.Property(x => x.ProductId).IsRequired();
        b.Property(x => x.StorageLocationId).IsRequired();
        b.Property(x => x.Quantity).IsRequired().HasPrecision(18, 2);

        b.HasOne(x => x.Product)
            .WithMany(p => p.Stocks)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.StorageLocation)
            .WithMany(l => l.Stocks)
            .HasForeignKey(x => x.StorageLocationId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.ProductId, x.StorageLocationId }).IsUnique();
    }
}
