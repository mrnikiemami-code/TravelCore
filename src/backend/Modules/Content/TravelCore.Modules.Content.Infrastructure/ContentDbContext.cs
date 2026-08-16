using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Content.Infrastructure;

/// <summary>
/// Content-owned DbContext. Owns PostgreSQL schema <c>content</c>.
/// Scaffolding only — no product entities yet (TC-P08-T001).
/// </summary>
public sealed class ContentDbContext : DbContext
{
    public const string SchemaName = "content";

    public ContentDbContext(DbContextOptions<ContentDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly);
    }
}
