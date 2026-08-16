using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Place.Infrastructure;

/// <summary>
/// Place-owned DbContext. Owns PostgreSQL schema <c>place</c>.
/// Scaffolding only — no product entities yet (TC-P07-T001).
/// </summary>
public sealed class PlaceDbContext : DbContext
{
    public const string SchemaName = "place";

    public PlaceDbContext(DbContextOptions<PlaceDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlaceDbContext).Assembly);
    }
}
