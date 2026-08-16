using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Seo.Domain;

namespace TravelCore.Modules.Seo.Infrastructure;

/// <summary>
/// SEO-owned DbContext. Owns PostgreSQL schema <c>seo</c>.
/// </summary>
public sealed class SeoDbContext : DbContext
{
    public const string SchemaName = "seo";

    public SeoDbContext(DbContextOptions<SeoDbContext> options)
        : base(options)
    {
    }

    public DbSet<SeoRoute> SeoRoutes => Set<SeoRoute>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SeoDbContext).Assembly);
    }
}
