using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Ugc.Infrastructure;

/// <summary>
/// UGC-owned DbContext. Owns PostgreSQL schema <c>ugc</c>.
/// Review + Travelogue + UserPhoto relationship (TC-P16-T005). No independent Rating table.
/// </summary>
public sealed class UgcDbContext : DbContext
{
    public const string SchemaName = "ugc";

    public DbSet<TravelCore.Modules.Ugc.Domain.Review> Reviews => Set<TravelCore.Modules.Ugc.Domain.Review>();

    public DbSet<TravelCore.Modules.Ugc.Domain.Travelogue> Travelogues => Set<TravelCore.Modules.Ugc.Domain.Travelogue>();

    public DbSet<TravelCore.Modules.Ugc.Domain.UserPhoto> UserPhotos => Set<TravelCore.Modules.Ugc.Domain.UserPhoto>();

    public DbSet<TravelCore.Modules.Ugc.Domain.Comment> Comments => Set<TravelCore.Modules.Ugc.Domain.Comment>();

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
