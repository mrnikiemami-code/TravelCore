using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Media.Infrastructure;

/// <summary>
/// Media-owned DbContext. Owns PostgreSQL schema <c>media</c>.
/// Product entities (MediaAsset, variants, translations) are deferred to later P06 tasks.
/// </summary>
public sealed class MediaDbContext : DbContext
{
    public const string SchemaName = "media";

    public MediaDbContext(DbContextOptions<MediaDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MediaDbContext).Assembly);
    }
}
