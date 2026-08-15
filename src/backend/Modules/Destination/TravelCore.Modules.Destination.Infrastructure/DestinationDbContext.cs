using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Destination.Infrastructure;

/// <summary>
/// Empty Destination-owned DbContext shell. Owns PostgreSQL schema <c>destination</c>.
/// No entities, migrations, or business features in TC-P04-T001.
/// </summary>
public sealed class DestinationDbContext : DbContext
{
    public const string SchemaName = "destination";

    public DestinationDbContext(DbContextOptions<DestinationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
    }
}
