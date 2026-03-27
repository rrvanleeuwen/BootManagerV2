using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core-configuratie voor de MotionMeasurement-entiteit.
/// </summary>
public class MotionMeasurementConfiguration : IEntityTypeConfiguration<MotionMeasurement>
{
    /// <summary>
    /// Configureert tabelnaam, keys, verplichting, veldgrootten en precisie.
    /// </summary>
    public void Configure(EntityTypeBuilder<MotionMeasurement> b)
    {
        b.ToTable("MotionMeasurements");
        b.HasKey(x => x.Id);

        // Verplichte velden
        b.Property(x => x.RecordedAtUtc).IsRequired();
        b.Property(x => x.Source).IsRequired().HasMaxLength(256);
        b.Property(x => x.MessageId).IsRequired().HasMaxLength(128);

        // Koers: decimal met precisie voor twee decimalen (0-359,99)
        b.Property(x => x.CourseOverGroundDegrees)
            .IsRequired()
            .HasPrecision(6, 2);

        // Snelheid: decimal met precisie voor twee decimalen
        b.Property(x => x.SpeedOverGround)
            .IsRequired()
            .HasPrecision(8, 2);

        // Eenheid: tekststring (bijv. "kn", "m/s")
        b.Property(x => x.SpeedUnit)
            .IsRequired()
            .HasMaxLength(16);

        // Index op RecordedAtUtc voor efficiënte query's op chronologische volgorde
        b.HasIndex(x => x.RecordedAtUtc).IsUnique(false);
    }
}
