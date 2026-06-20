using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

public class ProductCategoryMappingConfiguration : IEntityTypeConfiguration<ProductCategoryMapping>
{
    public void Configure(EntityTypeBuilder<ProductCategoryMapping> b)
    {
        b.ToTable("ProductCategoryMappings");
        b.HasKey(x => x.Id);

        b.Property(x => x.ProductId).IsRequired();
        b.Property(x => x.ProductCategoryId).IsRequired();
        b.Property(x => x.IsActive).IsRequired();
        b.Property(x => x.CreatedAt).IsRequired();

        b.HasOne(x => x.Product)
            .WithMany(p => p.CategoryMappings)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.ProductCategory)
            .WithMany(c => c.ProductMappings)
            .HasForeignKey(x => x.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.ProductId, x.IsActive })
            .IsUnique()
            .HasFilter("\"IsActive\" = true");
    }
}
