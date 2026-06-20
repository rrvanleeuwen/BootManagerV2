using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

public class ProductCodeConfiguration : IEntityTypeConfiguration<ProductCode>
{
    public void Configure(EntityTypeBuilder<ProductCode> b)
    {
        b.ToTable("ProductCodes");
        b.HasKey(x => x.Id);

        b.Property(x => x.ProductId).IsRequired();
        b.Property(x => x.Value).IsRequired().HasMaxLength(255);
        b.Property(x => x.NormalizedValue).IsRequired().HasMaxLength(255);
        b.Property(x => x.Format).IsRequired().HasMaxLength(50);
        b.Property(x => x.CreatedAt).IsRequired();

        b.HasOne(x => x.Product)
            .WithOne(p => p.Code)
            .HasForeignKey<ProductCode>(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.NormalizedValue).IsUnique();
    }
}
