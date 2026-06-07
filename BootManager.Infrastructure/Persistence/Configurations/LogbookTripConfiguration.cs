using BootManager.Core.Entities;
using BootManager.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core-configuratie voor de <see cref="LogbookTrip"/>-entiteit.
/// </summary>
public class LogbookTripConfiguration : IEntityTypeConfiguration<LogbookTrip>
{
    /// <summary>
    /// Configureert tabelnaam, keys, verplichting en veldgrootten.
    /// </summary>
    public void Configure(EntityTypeBuilder<LogbookTrip> b)
    {
        b.ToTable("LogbookTrips");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).IsRequired().HasMaxLength(256);
        b.Property(x => x.DepartureUtc).IsRequired();
        b.Property(x => x.ArrivalUtc);
        b.Property(x => x.DeparturePort).HasMaxLength(128);
        b.Property(x => x.DestinationPort).HasMaxLength(128);
        b.Property(x => x.VesselName).HasMaxLength(128);
        b.Property(x => x.Crew).HasMaxLength(512);
        b.Property(x => x.LogIntervalMinutes).IsRequired().HasDefaultValue(60);
        b.Property(x => x.Notes).HasMaxLength(2048);
        b.Property(x => x.LogstandStart).HasColumnType("TEXT");
        b.Property(x => x.LogstandEnd).HasColumnType("TEXT");
        b.Property(x => x.LoggedMiles).HasColumnType("TEXT");
        b.Property(x => x.EngineHoursStart).HasColumnType("TEXT");
        b.Property(x => x.EngineHoursEnd).HasColumnType("TEXT");
        b.Property(x => x.Fuel).HasMaxLength(64);
        b.Property(x => x.TotalSailingHours).HasColumnType("TEXT");
        b.Property(x => x.Status).IsRequired().HasDefaultValue(LogbookTripStatus.Open);
        b.Property(x => x.CreatedAtUtc).IsRequired();
        b.Property(x => x.UpdatedAtUtc).IsRequired();

        // Één-op-veel relatie: reis heeft meerdere logboekregels
        b.HasMany(x => x.Entries)
            .WithOne(e => e.Trip)
            .HasForeignKey(e => e.LogbookTripId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.DepartureUtc).IsUnique(false);
    }
}
