namespace BootManager.Infrastructure.Persistence.Configurations;

using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// EF Core-configuratie voor de HeadingMeasurement-entiteit.
/// </summary>
public class HeadingMeasurementConfiguration : IEntityTypeConfiguration<HeadingMeasurement>
{
    /// <summary>
    /// Configureert tabelnaam, keys, verplichting, veldgrootten en precisie.
    /// </summary>
    public void Configure(EntityTypeBuilder<HeadingMeasurement> b)
    {
        b.ToTable("HeadingMeasurements");
        b.HasKey(x => x.Id);

        // Verplichte velden
        b.Property(x => x.RecordedAtUtc).IsRequired();
        b.Property(x => x.Source).IsRequired().HasMaxLength(256);
        b.Property(x => x.MessageId).IsRequired().HasMaxLength(128);

        // Koers: decimal met precisie voor twee decimalen (bijv. 123.45°)
        b.Property(x => x.HeadingDegrees)
            .IsRequired()
            .HasPrecision(10, 2);

        // Index op RecordedAtUtc voor efficiënte query's op chronologische volgorde
        b.HasIndex(x => x.RecordedAtUtc).IsUnique(false);
    }
}
