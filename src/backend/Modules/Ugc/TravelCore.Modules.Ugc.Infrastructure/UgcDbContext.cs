using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Ugc.Infrastructure;

/// <summary>
/// UGC-owned DbContext. Owns PostgreSQL schema <c>ugc</c>.
/// Review + structured dimension ratings (TC-P16-T002). No independent Rating table.
/// </summary>
public sealed class UgcDbContext : DbContext
{
    public const string SchemaName = "ugc";

    public DbSet<TravelCore.Modules.Ugc.Domain.Review> Reviews => Set<TravelCore.Modules.Ugc.Domain.Review>();

    public UgcDbContext(DbContextOptions<UgcDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UgcDbContext).Assembly);
    }
}
