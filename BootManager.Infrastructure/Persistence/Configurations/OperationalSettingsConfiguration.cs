using BootManager.Core.Entities;
using BootManager.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BootManager.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core-configuratie voor de <see cref="OperationalSettings"/>-entiteit.
/// </summary>
public class OperationalSettingsConfiguration : IEntityTypeConfiguration<OperationalSettings>
{
    public void Configure(EntityTypeBuilder<OperationalSettings> b)
    {
        b.ToTable("OperationalSettings");
        b.HasKey(x => x.Id);

        b.Property(x => x.ListenAddress).IsRequired().HasMaxLength(256);
        b.Property(x => x.ListenPort).IsRequired();
        b.Property(x => x.AlternativeListenPort);
        b.Property(x => x.ApiBaseUrl).IsRequired().HasMaxLength(512);
        b.Property(x => x.RawStorageMode).IsRequired()
            .HasConversion<string>().HasMaxLength(64);
        b.Property(x => x.DefaultSampleIntervalSeconds).IsRequired();
        b.Property(x => x.CaptureLoggingEnabled).IsRequired();
        b.Property(x => x.CreatedUtc).IsRequired();
        b.Property(x => x.UpdatedUtc);
    }
}
