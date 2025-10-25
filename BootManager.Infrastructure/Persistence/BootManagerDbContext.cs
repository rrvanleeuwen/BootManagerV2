using BootManager.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BootManager.Infrastructure.Persistence;

public class BootManagerDbContext : DbContext
{
    public BootManagerDbContext(DbContextOptions<BootManagerDbContext> options) : base(options) { }

    public DbSet<OwnerProfile> OwnerProfiles => Set<OwnerProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.OwnerProfileConfiguration());
    }
}