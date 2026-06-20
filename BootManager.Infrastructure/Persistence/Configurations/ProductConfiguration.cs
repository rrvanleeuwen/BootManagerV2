using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("Products");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.DefaultUnitId).IsRequired();
        b.Property(x => x.ArchivedAt);

        b.HasOne(x => x.DefaultUnit)
            .WithMany(u => u.Products)
            .HasForeignKey(x => x.DefaultUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.CategoryMappings)
            .WithOne(m => m.Product)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Code)
            .WithOne(c => c.Product)
            .HasForeignKey<ProductCode>(c => c.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
