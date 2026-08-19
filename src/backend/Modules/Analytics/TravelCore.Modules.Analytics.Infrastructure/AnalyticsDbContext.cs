using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Analytics.Infrastructure;

/// <summary>
/// Analytics-owned DbContext. Owns PostgreSQL schema <c>analytics</c>.
/// T004 is schema foundation only — no product tables.
/// </summary>
public sealed class AnalyticsDbContext : DbContext
{
    public const string SchemaName = "analytics";

    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
    }
}
