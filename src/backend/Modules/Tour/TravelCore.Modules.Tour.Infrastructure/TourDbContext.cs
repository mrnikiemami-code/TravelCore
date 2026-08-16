using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Tour.Infrastructure;

/// <summary>
/// Tour-owned DbContext. Owns PostgreSQL schema <c>tour</c>.
/// Scaffolding only — no product entities yet (TC-P09-T001).
/// </summary>
public sealed class TourDbContext : DbContext
{
    public const string SchemaName = "tour";

    public TourDbContext(DbContextOptions<TourDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TourDbContext).Assembly);
    }
}
