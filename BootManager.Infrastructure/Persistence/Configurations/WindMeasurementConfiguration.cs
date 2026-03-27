namespace BootManager.Infrastructure.Persistence.Configurations;

using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// EF Core-configuratie voor de WindMeasurement-entiteit.
/// </summary>
public class WindMeasurementConfiguration : IEntityTypeConfiguration<WindMeasurement>
{
    /// <summary>
    /// Configureert tabelnaam, keys, verplichting, veldgrootten en precisie.
    /// </summary>
    public void Configure(EntityTypeBuilder<WindMeasurement> b)
    {
        b.ToTable("WindMeasurements");
        b.HasKey(x => x.Id);

        // Verplichte velden
        b.Property(x => x.RecordedAtUtc).IsRequired();
        b.Property(x => x.Source).IsRequired().HasMaxLength(256);
        b.Property(x => x.MessageId).IsRequired().HasMaxLength(128);

        // Windhoek: decimal met precisie voor twee decimalen (bijv. 45.50)
        b.Property(x => x.WindAngleDegrees)
            .IsRequired()
            .HasPrecision(10, 2);

        // Windsnelheid: decimal met precisie voor twee decimalen (bijv. 12.50 m/s)
        b.Property(x => x.WindSpeed)
            .IsRequired()
            .HasPrecision(10, 2);

        // Eenheid van windsnelheid
        b.Property(x => x.SpeedUnit).IsRequired().HasMaxLength(10);

        // Index op RecordedAtUtc voor efficiënte query's op chronologische volgorde
        b.HasIndex(x => x.RecordedAtUtc).IsUnique(false);
    }
}