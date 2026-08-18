using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Flight.Infrastructure;

/// <summary>
/// Flight-owned DbContext. Owns PostgreSQL schema <c>flight</c>.
/// T001 is schema foundation only — no product tables.
/// </summary>
public sealed class FlightDbContext : DbContext
{
    public const string SchemaName = "flight";

    public FlightDbContext(DbContextOptions<FlightDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FlightDbContext).Assembly);
    }
}
