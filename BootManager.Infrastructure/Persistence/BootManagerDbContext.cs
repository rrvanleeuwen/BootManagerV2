using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BootManager.Infrastructure.Persistence;

public class BootManagerDbContext : DbContext
{
    public BootManagerDbContext(DbContextOptions<BootManagerDbContext> options) : base(options) { }

    public DbSet<OwnerProfile> OwnerProfiles => Set<OwnerProfile>();

    /// <summary>
    /// DbSet voor opgeslagen ruwe netwerkregels.
    /// </summary>
    public DbSet<NetworkMessage> NetworkMessages => Set<NetworkMessage>(); // Opgenomen voor persistente opslag van inkomende netwerkregels

    /// <summary>
    /// DbSet voor opgeslagen geïnterpreteerde batterijmetingen.
    /// </summary>
    public DbSet<BatteryMeasurement> BatteryMeasurements => Set<BatteryMeasurement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.OwnerProfileConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.NetworkMessageConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.BatteryMeasurementConfiguration());
    }
}