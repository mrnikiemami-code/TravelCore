using Microsoft.EntityFrameworkCore;
using DestinationAggregate = TravelCore.Modules.Destination.Domain.Destination;

namespace TravelCore.Modules.Destination.Infrastructure;

/// <summary>
/// Destination-owned DbContext. Owns PostgreSQL schema <c>destination</c>.
/// </summary>
public sealed class DestinationDbContext : DbContext
{
    public const string SchemaName = "destination";

    public DestinationDbContext(DbContextOptions<DestinationDbContext> options)
        : base(options)
    {
    }

    public DbSet<DestinationAggregate> Destinations => Set<DestinationAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DestinationDbContext).Assembly);
    }
}
