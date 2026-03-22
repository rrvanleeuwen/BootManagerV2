using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core-configuratie voor de NetworkMessage-entiteit.
/// </summary>
public class NetworkMessageConfiguration : IEntityTypeConfiguration<NetworkMessage>
{
    /// <summary>
    /// Configureert tabelnaam, keys, verplichting en veldgrootten.
    /// </summary>
    public void Configure(EntityTypeBuilder<NetworkMessage> b)
    {
        b.ToTable("NetworkMessages");
        b.HasKey(x => x.Id);

        // Verplichte timestamp voor ontvangst
        b.Property(x => x.ReceivedAtUtc).IsRequired();

        // Brontekst en protocol: strings met redelijke max-lengte
        b.Property(x => x.Source).IsRequired().HasMaxLength(256);
        b.Property(x => x.Protocol).IsRequired().HasMaxLength(64);

        // Ruwe regel kan lang zijn; bewaren als required text zonder expliciete max
        b.Property(x => x.RawLine).IsRequired();

        // Optionele velden: correlatie-id en hex-payload (beperk lengte voor opslag)
        b.Property(x => x.MessageId).HasMaxLength(128);
        b.Property(x => x.PayloadHex).HasMaxLength(4000);

        // Unieke index op Id (consistent met bestaande configuratiestijl)
        b.HasIndex(nameof(NetworkMessage.Id)).IsUnique();
    }
}