using Microsoft.EntityFrameworkCore;
using PlaceAggregate = TravelCore.Modules.Place.Domain.Place;

namespace TravelCore.Modules.Place.Infrastructure;

/// <summary>
/// Place-owned DbContext. Owns PostgreSQL schema <c>place</c>.
/// </summary>
public sealed class PlaceDbContext : DbContext
{
    public const string SchemaName = "place";

    public PlaceDbContext(DbContextOptions<PlaceDbContext> options)
        : base(options)
    {
    }

    public DbSet<PlaceAggregate> Places => Set<PlaceAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlaceDbContext).Assembly);
    }
}
