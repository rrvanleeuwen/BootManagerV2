namespace BootManager.Infrastructure.Persistence.Configurations;

using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// EF Core-configuratie voor de SpeedThroughWaterMeasurement-entiteit.
/// </summary>
public class SpeedThroughWaterMeasurementConfiguration : IEntityTypeConfiguration<SpeedThroughWaterMeasurement>
{
    /// <summary>
    /// Configureert tabelnaam, keys, verplichting, veldgrootten en precisie.
    /// </summary>
    public void Configure(EntityTypeBuilder<SpeedThroughWaterMeasurement> b)
    {
        b.ToTable("SpeedThroughWaterMeasurements");
        b.HasKey(x => x.Id);

        // Verplichte velden
        b.Property(x => x.RecordedAtUtc).IsRequired();
        b.Property(x => x.Source).IsRequired().HasMaxLength(256);
        b.Property(x => x.MessageId).IsRequired().HasMaxLength(128);

        // Snelheidswaarden: decimal met precisie voor twee decimalen
        b.Property(x => x.SpeedMetersPerSecond)
            .IsRequired()
            .HasPrecision(10, 4);

        b.Property(x => x.SpeedKnots)
            .IsRequired()
            .HasPrecision(10, 4);

        // Reference type als byte
        b.Property(x => x.SpeedWaterReferenceType).IsRequired();

        // Index op RecordedAtUtc voor efficiënte query's op chronologische volgorde
        b.HasIndex(x => x.RecordedAtUtc).IsUnique(false);
    }
}
