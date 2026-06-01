namespace BootManager.Infrastructure.Persistence.Configurations;

using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// EF Core configuratie voor FluidLevelMeasurement-entiteit.
/// </summary>
public class FluidLevelMeasurementConfiguration : IEntityTypeConfiguration<FluidLevelMeasurement>
{
    public void Configure(EntityTypeBuilder<FluidLevelMeasurement> builder)
    {
        // Tabel
        builder.ToTable("FluidLevelMeasurements");

        // Primary key
        builder.HasKey(f => f.Id);

        // Properties
        builder.Property(f => f.Id)
            .ValueGeneratedOnAdd();

        builder.Property(f => f.RecordedAtUtc)
            .IsRequired();

        builder.Property(f => f.Source)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(f => f.MessageId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(f => f.Pgn)
            .IsRequired();

        builder.Property(f => f.GatewaySentence)
            .HasMaxLength(20);

        builder.Property(f => f.SourceAddress);

        builder.Property(f => f.FluidInstance)
            .IsRequired();

        builder.Property(f => f.FluidType)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(f => f.RawFluidType)
            .IsRequired();

        builder.Property(f => f.LevelPercent)
            .HasColumnType("decimal(5, 2)");

        builder.Property(f => f.CapacityLiters)
            .HasColumnType("decimal(10, 2)");

        builder.Property(f => f.IsLevelInvalid)
            .IsRequired();

        // Indexen voor snelle zoekopdrachten
        builder.HasIndex(f => f.RecordedAtUtc);
        builder.HasIndex(f => f.FluidType);
        builder.HasIndex(f => new { f.FluidType, f.FluidInstance });
    }
}
