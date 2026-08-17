using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.TripPlanner.Infrastructure;

/// <summary>
/// TripPlanner-owned DbContext. Owns PostgreSQL schema <c>trip_planner</c>.
/// No product tables in T001 (P18-R1 scaffolding only).
/// </summary>
public sealed class TripPlannerDbContext : DbContext
{
    public const string SchemaName = "trip_planner";

    public TripPlannerDbContext(DbContextOptions<TripPlannerDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TripPlannerDbContext).Assembly);
    }
}
