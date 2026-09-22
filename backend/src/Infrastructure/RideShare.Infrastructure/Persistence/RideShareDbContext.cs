using Microsoft.EntityFrameworkCore;
using RideShare.Domain.Entities;

namespace RideShare.Infrastructure.Persistence;

/// <summary>
/// Write-side (EF Core) context for the domain aggregates. Identity's own tables live in a
/// separate ApplicationIdentityDbContext (RideShare.Identity project) against the same database —
/// two bounded contexts, one physical SQL Server instance.
/// </summary>
public class RideShareDbContext(DbContextOptions<RideShareDbContext> options) : DbContext(options)
{
    public DbSet<RiderProfile> RiderProfiles => Set<RiderProfile>();
    public DbSet<DriverProfile> DriverProfiles => Set<DriverProfile>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Ride> Rides => Set<Ride>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<CopilotConversation> CopilotConversations => Set<CopilotConversation>();
    public DbSet<CopilotMessage> CopilotMessages => Set<CopilotMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RideShareDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
