using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core-configuratie voor de PositionMeasurement-entiteit.
/// </summary>
public class PositionMeasurementConfiguration : IEntityTypeConfiguration<PositionMeasurement>
{
    /// <summary>
    /// Configureert tabelnaam, keys, verplichting, veldgrootten en precisie.
    /// </summary>
    public void Configure(EntityTypeBuilder<PositionMeasurement> b)
    {
        b.ToTable("PositionMeasurements");
        b.HasKey(x => x.Id);

        // Verplichte velden
        b.Property(x => x.RecordedAtUtc).IsRequired();
        b.Property(x => x.Source).IsRequired().HasMaxLength(256);
        b.Property(x => x.MessageId).IsRequired().HasMaxLength(128);

        // Coördinaten: decimal met precisie voor zes decimalen (ongeveer 0.11 meter nauwkeurigheid)
        b.Property(x => x.Latitude)
            .IsRequired()
            .HasPrecision(10, 6);

        b.Property(x => x.Longitude)
            .IsRequired()
            .HasPrecision(11, 6);

        // Index op RecordedAtUtc voor efficiënte query's op chronologische volgorde
        b.HasIndex(x => x.RecordedAtUtc).IsUnique(false);
    }
}
