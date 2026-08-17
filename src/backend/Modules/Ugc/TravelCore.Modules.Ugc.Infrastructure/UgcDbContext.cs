using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Ugc.Infrastructure;

/// <summary>
/// UGC-owned DbContext. Owns PostgreSQL schema <c>ugc</c>.
/// No Review/Rating/Travelogue tables in T001 (P16-R1 scaffolding only).
/// </summary>
public sealed class UgcDbContext : DbContext
{
    public const string SchemaName = "ugc";

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
