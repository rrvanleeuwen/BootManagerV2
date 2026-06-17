namespace BootManager.Infrastructure.Persistence;

using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;

public class BootManagerDbContext : DbContext
{
    public BootManagerDbContext(DbContextOptions<BootManagerDbContext> options) : base(options) { }

    public DbSet<LocalUser> LocalUsers => Set<LocalUser>();

    /// <summary>
    /// DbSet voor het bootprofiel (singleton per installatie).
    /// </summary>
    public DbSet<VesselProfile> VesselProfiles => Set<VesselProfile>();

    /// <summary>
    /// DbSet voor opgeslagen ruwe netwerkregels.
    /// </summary>
    public DbSet<NetworkMessage> NetworkMessages => Set<NetworkMessage>();

    /// <summary>
    /// DbSet voor opgeslagen geïnterpreteerde batterijmetingen.
    /// </summary>
    public DbSet<BatteryMeasurement> BatteryMeasurements => Set<BatteryMeasurement>();

    /// <summary>
    /// DbSet voor opgeslagen geïnterpreteerde dieptemetingen.
    /// </summary>
    public DbSet<DepthMeasurement> DepthMeasurements => Set<DepthMeasurement>();

    /// <summary>
    /// DbSet voor opgeslagen geïnterpreteerde windmetingen.
    /// </summary>
    public DbSet<WindMeasurement> WindMeasurements => Set<WindMeasurement>();

    /// <summary>
    /// DbSet voor opgeslagen geïnterpreteerde bewegingsmetingen.
    /// </summary>
    public DbSet<MotionMeasurement> MotionMeasurements => Set<MotionMeasurement>();

    /// <summary>
    /// DbSet voor opgeslagen geïnterpreteerde positiemetingen.
    /// </summary>
    public DbSet<PositionMeasurement> PositionMeasurements => Set<PositionMeasurement>();

    /// <summary>
    /// DbSet voor opgeslagen geïnterpreteerde koersmetingen.
    /// </summary>
    public DbSet<HeadingMeasurement> HeadingMeasurements => Set<HeadingMeasurement>();

    /// <summary>
    /// DbSet voor opgeslagen geïnterpreteerde snelheid-door-water-metingen.
    /// </summary>
    public DbSet<SpeedThroughWaterMeasurement> SpeedThroughWaterMeasurements => Set<SpeedThroughWaterMeasurement>();

    /// <summary>
    /// DbSet voor opgeslagen geïnterpreteerde watertemperatuur-metingen.
    /// </summary>
    public DbSet<WaterTemperatureMeasurement> WaterTemperatureMeasurements => Set<WaterTemperatureMeasurement>();

    /// <summary>
    /// DbSet voor opgeslagen geïnterpreteerde tankniveau-metingen.
    /// </summary>
    public DbSet<FluidLevelMeasurement> FluidLevelMeasurements => Set<FluidLevelMeasurement>();

    /// <summary>
    /// DbSet voor logboek-reizen.
    /// </summary>
    public DbSet<LogbookTrip> LogbookTrips => Set<LogbookTrip>();

    /// <summary>
    /// DbSet voor logboekregels.
    /// </summary>
    public DbSet<LogbookEntry> LogbookEntries => Set<LogbookEntry>();

    /// <summary>
    /// DbSet voor operationele instellingen.
    /// </summary>
    public DbSet<OperationalSettings> OperationalSettings => Set<OperationalSettings>();

    /// <summary>
    /// DbSet voor logboekbijlagen.
    /// </summary>
    public DbSet<LogbookAttachment> LogbookAttachments => Set<LogbookAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.LocalUserConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.VesselProfileConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.NetworkMessageConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.BatteryMeasurementConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.DepthMeasurementConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.WindMeasurementConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.MotionMeasurementConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.PositionMeasurementConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.HeadingMeasurementConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.SpeedThroughWaterMeasurementConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.WaterTemperatureMeasurementConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.FluidLevelMeasurementConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.LogbookTripConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.LogbookEntryConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.LogbookAttachmentConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.OperationalSettingsConfiguration());
    }
}