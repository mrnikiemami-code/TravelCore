using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Media.Domain;

namespace TravelCore.Modules.Media.Infrastructure;

/// <summary>
/// Media-owned DbContext. Owns PostgreSQL schema <c>media</c>.
/// </summary>
public sealed class MediaDbContext : DbContext
{
    public const string SchemaName = "media";

    public MediaDbContext(DbContextOptions<MediaDbContext> options)
        : base(options)
    {
    }

    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    public DbSet<MediaVariant> MediaVariants => Set<MediaVariant>();

    public DbSet<MediaAssetTranslation> MediaAssetTranslations => Set<MediaAssetTranslation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MediaDbContext).Assembly);
    }
}
