using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core-configuratie voor de BatteryMeasurement-entiteit.
/// </summary>
public class BatteryMeasurementConfiguration : IEntityTypeConfiguration<BatteryMeasurement>
{
    /// <summary>
    /// Configureert tabelnaam, keys, verplichting, veldgrootten en precisie.
    /// </summary>
    public void Configure(EntityTypeBuilder<BatteryMeasurement> b)
    {
        b.ToTable("BatteryMeasurements");
        b.HasKey(x => x.Id);

        // Verplichte velden
        b.Property(x => x.RecordedAtUtc).IsRequired();
        b.Property(x => x.Source).IsRequired().HasMaxLength(256);
        b.Property(x => x.MessageId).IsRequired().HasMaxLength(128);

        // Spanning: decimal met precisie voor twee decimalen (bijv. 12.60)
        b.Property(x => x.Voltage)
            .IsRequired()
            .HasPrecision(10, 2);

        // Optioneel: laadtoestand (0-100)
        b.Property(x => x.StateOfCharge);

        // Index op RecordedAtUtc voor efficiënte query's op chronologische volgorde
        b.HasIndex(x => x.RecordedAtUtc).IsUnique(false);
    }
}