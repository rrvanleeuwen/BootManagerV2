namespace BootManager.Infrastructure.Persistence.Configurations;

using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// EF Core-configuratie voor de DepthMeasurement-entiteit.
/// </summary>
public class DepthMeasurementConfiguration : IEntityTypeConfiguration<DepthMeasurement>
{
    /// <summary>
    /// Configureert tabelnaam, keys, verplichting, veldgrootten en precisie.
    /// </summary>
    public void Configure(EntityTypeBuilder<DepthMeasurement> b)
    {
        b.ToTable("DepthMeasurements");
        b.HasKey(x => x.Id);

        // Verplichte velden
        b.Property(x => x.RecordedAtUtc).IsRequired();
        b.Property(x => x.Source).IsRequired().HasMaxLength(256);
        b.Property(x => x.MessageId).IsRequired().HasMaxLength(128);

        // Diepte: decimal met precisie voor twee decimalen (bijv. 3.50)
        b.Property(x => x.DepthMeters)
            .IsRequired()
            .HasPrecision(10, 2);

        // Index op RecordedAtUtc voor efficiënte query's op chronologische volgorde
        b.HasIndex(x => x.RecordedAtUtc).IsUnique(false);
    }
}