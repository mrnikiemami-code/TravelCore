using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Seo.Infrastructure;

/// <summary>
/// SEO-owned DbContext. Owns PostgreSQL schema <c>seo</c>.
/// Product entities (SeoRoute, redirects, policies) are deferred to later P05 tasks.
/// </summary>
public sealed class SeoDbContext : DbContext
{
    public const string SchemaName = "seo";

    public SeoDbContext(DbContextOptions<SeoDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SeoDbContext).Assembly);
    }
}
