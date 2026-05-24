using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core-configuratie voor de VesselProfile-entiteit.
/// </summary>
public class VesselProfileConfiguration : IEntityTypeConfiguration<VesselProfile>
{
    /// <summary>
    /// Configureert tabelnaam, keys, verplichting en veldgrootten.
    /// </summary>
    public void Configure(EntityTypeBuilder<VesselProfile> b)
    {
        b.ToTable("VesselProfiles");
        b.HasKey(x => x.Id);

        // Verplichte velden
        b.Property(x => x.VesselName).IsRequired().HasMaxLength(128);
        b.Property(x => x.CreatedUtc).IsRequired();

        // Optionele velden met maximale lengtes
        b.Property(x => x.HomePort).HasMaxLength(128);
        b.Property(x => x.CallSign).HasMaxLength(64);
        b.Property(x => x.Mmsi).HasMaxLength(32);

        // UpdatedUtc is optioneel (nullable)
        b.Property(x => x.UpdatedUtc);

        // Index op CreatedUtc voor efficiënte query's
        b.HasIndex(x => x.CreatedUtc).IsUnique(false);
    }
}
