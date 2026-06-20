using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> b)
    {
        b.ToTable("ProductCategories");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.Property(x => x.NormalizedName).IsRequired().HasMaxLength(100);
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.IconKey).IsRequired().HasMaxLength(50);
        b.Property(x => x.ArchivedAt);

        b.HasIndex(x => x.NormalizedName).IsUnique();

        b.HasMany(x => x.ProductMappings)
            .WithOne(m => m.ProductCategory)
            .HasForeignKey(m => m.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
