using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

public class StockMutationConfiguration : IEntityTypeConfiguration<StockMutation>
{
    public void Configure(EntityTypeBuilder<StockMutation> b)
    {
        b.ToTable("StockMutations");
        b.HasKey(x => x.Id);

        b.Property(x => x.ProductId).IsRequired();
        b.Property(x => x.StorageLocationId).IsRequired();
        b.Property(x => x.MutationType).IsRequired();
        b.Property(x => x.OldQuantity).IsRequired().HasPrecision(18, 2);
        b.Property(x => x.NewQuantity).IsRequired().HasPrecision(18, 2);
        b.Property(x => x.MutatedAt).IsRequired();
        b.Property(x => x.UserId).IsRequired();
        b.Property(x => x.Note).IsRequired(false).HasMaxLength(500);

        b.HasOne(x => x.Product)
            .WithMany(p => p.StockMutations)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.StorageLocation)
            .WithMany(l => l.StockMutations)
            .HasForeignKey(x => x.StorageLocationId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.MutatedAt).IsDescending();
    }
}
