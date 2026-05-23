using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core-configuratie voor de <see cref="LogbookEntry"/>-entiteit.
/// </summary>
public class LogbookEntryConfiguration : IEntityTypeConfiguration<LogbookEntry>
{
    /// <summary>
    /// Configureert tabelnaam, keys, verplichting, precisie en veldgrootten.
    /// </summary>
    public void Configure(EntityTypeBuilder<LogbookEntry> b)
    {
        b.ToTable("LogbookEntries");
        b.HasKey(x => x.Id);

        b.Property(x => x.LogbookTripId).IsRequired();
        b.Property(x => x.EntryTimeUtc).IsRequired();
        b.Property(x => x.BaroPressure).HasPrecision(7, 2);
        b.Property(x => x.LogValue).HasPrecision(10, 3);
        b.Property(x => x.Course);
        b.Property(x => x.Remarks).HasMaxLength(1024);
        b.Property(x => x.WindDescription).HasMaxLength(64);
        b.Property(x => x.GpsStatus).HasMaxLength(32);
        b.Property(x => x.Latitude);
        b.Property(x => x.Longitude);
        b.Property(x => x.AverageSogKnots).HasPrecision(7, 3);
        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.UpdatedAtUtc).IsRequired();

        b.HasIndex(x => new { x.LogbookTripId, x.EntryTimeUtc }).IsUnique(false);
    }
}
