namespace BootManager.Infrastructure.Persistence.Configurations;

using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// EF Core-configuratie voor de WaterTemperatureMeasurement-entiteit.
/// </summary>
public class WaterTemperatureMeasurementConfiguration : IEntityTypeConfiguration<WaterTemperatureMeasurement>
{
    /// <summary>
    /// Configureert tabelnaam, keys, verplichting, veldgrootten en precisie.
    /// </summary>
    public void Configure(EntityTypeBuilder<WaterTemperatureMeasurement> b)
    {
        b.ToTable("WaterTemperatureMeasurements");
        b.HasKey(x => x.Id);

        // Verplichte velden
        b.Property(x => x.RecordedAtUtc).IsRequired();
        b.Property(x => x.Source).IsRequired().HasMaxLength(256);
        b.Property(x => x.MessageId).IsRequired().HasMaxLength(128);

        // Temperature instance als byte
        b.Property(x => x.TemperatureInstance).IsRequired();

        // Temperatuurwaarden: decimal met precisie voor vier decimalen
        b.Property(x => x.TemperatureKelvin)
            .IsRequired()
            .HasPrecision(10, 4);

        b.Property(x => x.TemperatureCelsius)
            .IsRequired()
            .HasPrecision(10, 4);

        // Index op RecordedAtUtc voor efficiënte query's op chronologische volgorde
        b.HasIndex(x => x.RecordedAtUtc).IsUnique(false);
    }
}
