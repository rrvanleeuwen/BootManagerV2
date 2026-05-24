using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core-configuratie voor de <see cref="LogbookAttachment"/>-entiteit.
/// </summary>
public class LogbookAttachmentConfiguration : IEntityTypeConfiguration<LogbookAttachment>
{
    /// <summary>
    /// Configureert tabelnaam, keys, verplichting, precisie en veldgrootten.
    /// </summary>
    public void Configure(EntityTypeBuilder<LogbookAttachment> b)
    {
        b.ToTable("LogbookAttachments");
        b.HasKey(x => x.Id);

        b.Property(x => x.LogbookEntryId).IsRequired();
        b.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(512);
        b.Property(x => x.StoredFileName).IsRequired().HasMaxLength(512);
        b.Property(x => x.ContentType).IsRequired().HasMaxLength(128);
        b.Property(x => x.SizeBytes).IsRequired();
        b.Property(x => x.UploadedAtUtc).IsRequired();

        // Relatie naar LogbookEntry met cascade delete
        b.HasOne(x => x.Entry)
            .WithMany(e => e.Attachments)
            .HasForeignKey(x => x.LogbookEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index op LogbookEntryId voor snelle queries
        b.HasIndex(x => x.LogbookEntryId).IsUnique(false);
    }
}
