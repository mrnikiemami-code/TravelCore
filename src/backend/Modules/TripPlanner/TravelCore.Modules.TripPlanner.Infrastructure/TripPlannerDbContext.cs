using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.TripPlanner.Domain;

namespace TravelCore.Modules.TripPlanner.Infrastructure;

/// <summary>
/// TripPlanner-owned DbContext. Owns PostgreSQL schema <c>trip_planner</c>.
/// </summary>
public sealed class TripPlannerDbContext : DbContext
{
    public const string SchemaName = "trip_planner";

    public TripPlannerDbContext(DbContextOptions<TripPlannerDbContext> options)
        : base(options)
    {
    }

    public DbSet<TripIntent> TripIntents => Set<TripIntent>();

    public DbSet<Lead> Leads => Set<Lead>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TripPlannerDbContext).Assembly);
    }
}
